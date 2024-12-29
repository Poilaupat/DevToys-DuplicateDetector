using DevToys.Api;
using static DevToys.Api.GUI;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Text.RegularExpressions;
using DuplicateDetectorExtension.Models;
using static System.Net.Mime.MediaTypeNames;

namespace DuplicateDetectorExtension
{
    [Export(typeof(IGuiTool))]
    [Name("DuplicateDetector")]
    [ToolDisplayInformation(
        IconFontName = "FluentSystemIcon",
        IconGlyph = '\u2315',
        GroupName = PredefinedCommonToolGroupNames.Text,
        ResourceManagerAssemblyIdentifier = nameof(ResourceAssemblyIdentifier),
        ResourceManagerBaseName = "DuplicateDetectorExtension.DuplicateDetectorExtension",
        ShortDisplayTitleResourceName = nameof(DuplicateDetectorExtension.ShortDisplayTitle),
        LongDisplayTitleResourceName = nameof(DuplicateDetectorExtension.LongDisplayTitle),
        DescriptionResourceName = nameof(DuplicateDetectorExtension.Description),
        AccessibleNameResourceName = nameof(DuplicateDetectorExtension.AccessibleName)
    )]
    [AcceptedDataTypeName(PredefinedCommonDataTypeNames.Text)]
    internal sealed class DuplicateDetectorGui : IGuiTool, IDisposable
    {
        #region Enums

        private enum GridRows
        {
            ConfigRow,
            AppRow,
        }

        private enum GridColumns
        {
            Stretch
        }

        #endregion

        #region Settings

        private readonly ISettingsProvider _settingsProvider;

        private static readonly SettingDefinition<EDuplicateMode> _SettingMode
            = new(
                name: $"{nameof(DuplicateDetectorGui)}.{nameof(_SettingMode)}",
                defaultValue: EDuplicateMode.Line);

        private static readonly SettingDefinition<int> _SettingOffset
            = new(
                name: $"{nameof(DuplicateDetectorGui)}.{nameof(_SettingOffset)}",
                defaultValue: 0);

        private static readonly SettingDefinition<int> _SettingLength
            = new(
                name: $"{nameof(DuplicateDetectorGui)}.{nameof(_SettingLength)}",
                defaultValue: 0);

        private static readonly SettingDefinition<ERemoveDuplicateMode> _SettingRemoveMode
            = new(
                name: $"{nameof(DuplicateDetectorGui)}.{nameof(_SettingRemoveMode)}",
                defaultValue: ERemoveDuplicateMode.KeepFirstOccurence);

        #endregion

        #region UIElements

        private readonly IUISetting _UISettingMode = Setting("duplicate-detector-mode-setting");
        private readonly IUISetting _UISettingOffset = Setting("duplicate-detector-offset-setting");
        private readonly IUISetting _UISettingLength = Setting("duplicate-detector-length-setting");
        private readonly IUISetting _UISettingRemoveMode = Setting("duplicate-detector-remove-mode-setting");

        private IUIMultiLineTextInput _UITextInput = MultiLineTextInput("duplicate-detector-input-text");
        private IUIMultiLineTextInput _UITextOutput = MultiLineTextInput("duplicate-detector-ouput-text");

        private IUIButton _UIRemoveDuplicatesButton = Button("duplicate-detector-remove-button");

        #endregion

        private readonly DisposableSemaphore _semaphore = new();
        private readonly ILogger _logger;
        private CancellationTokenSource? _cts;

        internal Task? WorkTask { get; private set; }

        //internal List<Duplicate> Duplicates { get; private set; } = new List<Duplicate>();

        public UIToolView View
            => new(
                isScrollable: true,
                Grid()
                    .Rows(
                        (GridRows.ConfigRow, Auto),
                        (GridRows.AppRow, new UIGridLength(1, UIGridUnitType.Fraction)))
                    .Columns(
                        (GridColumns.Stretch, new UIGridLength(1, UIGridUnitType.Fraction)))
                    .Cells(
                        Cell(
                            GridRows.ConfigRow,
                            GridColumns.Stretch,
                            Stack()
                                .Vertical()
                                .WithChildren(
                                    Label()
                                        .Text(DuplicateDetectorExtension.Configuration),
                                    SettingGroup("")
                                        .Icon("FluentSystemIcons", '\uF6A9')
                                        .Title(DuplicateDetectorExtension.Options)
                                        .WithSettings(

                                            _UISettingMode
                                                .Title(DuplicateDetectorExtension.ModeSetting)
                                                .Description(DuplicateDetectorExtension.ModeSettingDescription)
                                                .Handle(
                                                    _settingsProvider,
                                                    _SettingMode,
                                                    onOptionSelected: OnDuplicateSearchModeChanged,
                                                    Item(DuplicateDetectorExtension.ModeSettingLine, EDuplicateMode.Line),
                                                    Item(DuplicateDetectorExtension.ModeSettingOffsetLength, EDuplicateMode.OffsetLength)),

                                            _UISettingOffset
                                                .Title(DuplicateDetectorExtension.OffsetSetting)
                                                .Description(DuplicateDetectorExtension.OffsetSettingDescription)
                                                .InteractiveElement(
                                                    NumberInput()
                                                        .HideCommandBar()
                                                        .Minimum(0)
                                                        .OnValueChanged(OnOffsetSettingChanged)
                                                        .Value(_settingsProvider.GetSetting(_SettingOffset))),

                                            _UISettingLength
                                                .Title(DuplicateDetectorExtension.LengthSetting)
                                                .Description(DuplicateDetectorExtension.LengthSettingDescription)
                                                .InteractiveElement(
                                                    NumberInput()
                                                        .HideCommandBar()
                                                        .Minimum(1)
                                                        .OnValueChanged(OnLengthSettingChanged)
                                                        .Value(_settingsProvider.GetSetting(_SettingLength))),

                                            _UISettingRemoveMode
                                                .Title(DuplicateDetectorExtension.UnduplicateSettingTitle)
                                                .Description(DuplicateDetectorExtension.UndiplicateSettingDescription)
                                                .Handle(
                                                    _settingsProvider,
                                                    _SettingRemoveMode,
                                                    null,
                                                    Item(DuplicateDetectorExtension.UnduplicateModeSettingKeepFirst, ERemoveDuplicateMode.KeepFirstOccurence),
                                                    Item(DuplicateDetectorExtension.UnduplicateModeSettingKeepLast, ERemoveDuplicateMode.KeepLastOccurence),
                                                    Item(DuplicateDetectorExtension.UnduplicateModeSettingRemoveAll, ERemoveDuplicateMode.RemoveAll))
                                        )
                                )
                            ),

                        Cell(GridRows.AppRow, GridColumns.Stretch,
                            SplitGrid()
                            .Vertical()
                            .LeftPaneLength(new UIGridLength(2d, UIGridUnitType.Fraction))
                            .RightPaneLength(new UIGridLength(1d, UIGridUnitType.Fraction))
                            .WithLeftPaneChild(
                                _UITextInput
                                .Title(DuplicateDetectorExtension.Input)
                                .CommandBarExtraContent(
                                    _UIRemoveDuplicatesButton
                                    .Text(DuplicateDetectorExtension.RemoveButtonTitle)
                                    .OnClick(OnRemoveButtonClicked)
                                    .Disable())
                                .Extendable()
                                .OnTextChanged(OnInputTextChanged))
                            .WithRightPaneChild(
                                _UITextOutput
                                .Title(DuplicateDetectorExtension.Duplicates)
                                .ReadOnly()
                                .HideCommandBar()
                                .NeverShowLineNumber()))
                    ));

        [ImportingConstructor]
        public DuplicateDetectorGui(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
            _logger = this.Log();

            OnDuplicateSearchModeChanged(_settingsProvider.GetSetting(_SettingMode));
        }

        public void Dispose()
        {
            if (_cts is not null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
            _semaphore.Dispose();
        }

        /// <summary>
        /// Handles the event triggered when user changes the Offset param
        /// </summary>
        private ValueTask OnOffsetSettingChanged(double value)
        {
            _settingsProvider.SetSetting(_SettingOffset, (int)value);
            StartSearchProcess();
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Handles the event triggered when user changes the Length param 
        /// </summary>
        private ValueTask OnLengthSettingChanged(double value)
        {
            _settingsProvider.SetSetting(_SettingLength, (int)value);
            StartSearchProcess();
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Handles the event triggered when user changes the search mode option 
        /// </summary>
        private ValueTask OnDuplicateSearchModeChanged(EDuplicateMode mode)
        {
            switch (mode)
            {
                case EDuplicateMode.Line:
                    _UISettingOffset.Disable();
                    _UISettingLength.Disable();
                    break;

                case EDuplicateMode.OffsetLength:
                    _UISettingOffset.Enable();
                    _UISettingLength.Enable();
                    break;
            }

            StartSearchProcess();
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Handles the event triggered when InputText is changed
        /// </summary>
        private ValueTask OnInputTextChanged(string _)
        {
            StartSearchProcess();
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Handles the event triggered when the remove button is clicked
        /// </summary>
        private ValueTask OnRemoveButtonClicked()
        {
            StartUnduplicateProcess();
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Handles the tool chaining event from DevToys
        /// </summary>
        public void OnDataReceived(string dataTypeName, object? parsedData)
        {
            if (dataTypeName == PredefinedCommonDataTypeNames.Text && parsedData is string text)
            {
                _UITextInput.Text(text);
            }
        }

        /// <summary>
        /// Highlights the found duplicates. Also highligths searched parts of the lines in offset mode
        /// </summary>
        private void SetHighlights(EDuplicateMode mode, DuplicateSearchResult dsr, CancellationToken ct)
        {
            var highlights = new List<UIHighlightedTextSpan>();


            // Highlights the duplicates
            if (dsr.Duplicates.Any())
            {
                foreach (var lineNumber in dsr.Duplicates.SelectMany(d => d.LineNbrs))
                {
                    if (dsr.Lines.Collection.Any())
                    {
                        var line = dsr.Lines
                            .Collection
                            .FirstOrDefault(l => l.LineNumber == lineNumber);

                        if (line is not null)
                            highlights.Add(new UIHighlightedTextSpan(line.Index + line.SearchedOffset, line.SearchedLength, UIHighlightedTextSpanColor.Red));
                    }

                    ct.ThrowIfCancellationRequested();
                }
            }

            // Highligths the searched part of the lines (only in Offset/Length mode and if not already highlighted)
            if (mode == EDuplicateMode.OffsetLength)
            {
                foreach (var line in dsr.Lines
                    .Collection
                    .Where(l => !string.IsNullOrWhiteSpace(l.Value)))
                {
                    if (!highlights.Any(h =>
                        h.StartPosition == line.Index + line.SearchedOffset &&
                        h.Length == line.SearchedLength &&
                        h.Color == UIHighlightedTextSpanColor.Red))
                    {
                        highlights.Add(new UIHighlightedTextSpan(line.Index + line.SearchedOffset, line.SearchedLength, UIHighlightedTextSpanColor.Green));
                    }

                    ct.ThrowIfCancellationRequested();
                }
            }

            _UITextInput.Highlight(highlights.ToArray());
        }

        /// <summary>
        /// Activates or deactivates the remove duplicates button
        /// </summary>
        /// <param name="enable"></param>
        private void SetRemoveButtonActivation(bool enable)
        {
            if (enable)
                _UIRemoveDuplicatesButton.Enable();
            else
                _UIRemoveDuplicatesButton.Disable();
        }

        /// <summary>
        /// Starts the search of the duplicates
        /// </summary>
        private void StartSearchProcess()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            WorkTask = SearchDuplicates(
                    _UITextInput.Text,
                    _settingsProvider.GetSetting(_SettingMode),
                    _settingsProvider.GetSetting(_SettingOffset),
                    _settingsProvider.GetSetting(_SettingLength),
                    _cts.Token);
        }

        /// <summary>
        /// Performs the search of the duplicates
        /// </summary>
        /// <param name="text">The text to be searched</param>
        /// <param name="mode">The search mode (Line or Offset/Length)</param>
        /// <param name="offset">The offset to use when mode is Offset/Length </param>
        /// <param name="length">The length to use when mode is Offset/Length</param>
        /// <param name="ct">A cancellation token</param>
        /// <returns></returns>
        private async Task SearchDuplicates(
            string text,
            EDuplicateMode mode,
            int offset,
            int length,
            CancellationToken ct)
        {
            using (await _semaphore.WaitAsync(ct))
            {
                await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(ct);

                try
                {
                    // Doing search
                    var dsr = DuplicateDetectorHelper.SearchDuplicates(
                         text,
                         mode,
                         offset,
                         length,
                         ct);


                    // Diplaying result (duplicates + line numbers)
                    _UITextOutput.Text(
                        string.Join("\r\n",
                        dsr.Duplicates.Select(d => $"{d.Value} [{string.Join(",", d.LineNbrs)}]")));

                    SetHighlights(mode, dsr, ct);

                    SetRemoveButtonActivation(dsr.Duplicates.Any());
                }
                catch (OperationCanceledException)
                {
                    return; //Do not log an error if the exception comes from a user operation cancellation
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error while searching duplicates (Mode = {mode})");
                }
            }
        }

        /// <summary>
        /// Starts removing the duplicates
        /// </summary>
        private void StartUnduplicateProcess()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            WorkTask = RemoveDuplicates(
                _UITextInput.Text,
                _settingsProvider.GetSetting(_SettingMode),
                _settingsProvider.GetSetting(_SettingOffset),
                _settingsProvider.GetSetting(_SettingLength),
                _settingsProvider.GetSetting(_SettingRemoveMode),
                _cts.Token);
        }

        /// <summary>
        /// Performs the unduplicate operation
        /// </summary>
        /// <returns></returns>
        private async Task RemoveDuplicates(
            string text,
            EDuplicateMode mode,
            int offset,
            int length,
            ERemoveDuplicateMode unduplicateMode,
            CancellationToken ct)
        {
            using (await _semaphore.WaitAsync(ct))
            {
                await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(ct);

                try
                {
                    var cleanedLines = DuplicateDetectorHelper.RemoveDuplicates(
                        text,
                        mode,
                        offset,
                        length,
                        unduplicateMode,
                        ct);

                    _UITextInput.Text(string.Join("\n", cleanedLines));
                }
                catch (OperationCanceledException)
                {
                    return; //Do not log an error if the exception comes from a user operation cancellation
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error while removing duplicates");
                }
            }
        }
    }
}
