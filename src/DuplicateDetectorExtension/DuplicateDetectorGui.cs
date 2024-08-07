﻿using DevToys.Api;
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

        #endregion

        #region UIElements

        private readonly IUISetting _UISettingMode = Setting("duplicate-detector-mode-setting");
        private readonly IUISetting _UISettingOffset = Setting("duplicate-detector-offset-setting");
        private readonly IUISetting _UISettingLength = Setting("duplicate-detector-length-setting");

        private IUIMultiLineTextInput _UITextInput = MultiLineTextInput("duplicate-detector-input-text");
        private IUIMultiLineTextInput _UITextOutput = MultiLineTextInput("duplicate-detector-ouput-text");

        #endregion

        private readonly DisposableSemaphore _semaphore = new();
        private readonly ILogger _logger;
        private CancellationTokenSource? _cts;

        internal Task? WorkTask { get; private set; }

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
                                                .Title(DuplicateDetectorExtension.Mode)
                                                .Description(DuplicateDetectorExtension.ModeDescription)
                                                .Handle(
                                                    _settingsProvider,
                                                    _SettingMode,
                                                    onOptionSelected: OnDuplicateSearchModeChanged,
                                                    Item(DuplicateDetectorExtension.Line, EDuplicateMode.Line),
                                                    Item(DuplicateDetectorExtension.OffsetLength, EDuplicateMode.OffsetLength)),

                                            _UISettingOffset
                                                .Title(DuplicateDetectorExtension.Offset)
                                                .Description(DuplicateDetectorExtension.OffsetDescription)
                                                .InteractiveElement(
                                                    NumberInput()
                                                        .HideCommandBar()
                                                        .Minimum(0)
                                                        .OnValueChanged(OnOffsetSettingChanged)
                                                        .Value(_settingsProvider.GetSetting(_SettingOffset))),

                                            _UISettingLength
                                                .Title(DuplicateDetectorExtension.Length)
                                                .Description(DuplicateDetectorExtension.LengthDescription)
                                                .InteractiveElement(
                                                    NumberInput()
                                                        .HideCommandBar()
                                                        .Minimum(1)
                                                        .OnValueChanged(OnLengthSettingChanged)
                                                        .Value(_settingsProvider.GetSetting(_SettingLength)))
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
            StartProcess();
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Handles the event triggered when user changes the Length param 
        /// </summary>
        private ValueTask OnLengthSettingChanged(double value)
        {
            _settingsProvider.SetSetting(_SettingLength, (int)value);
            StartProcess();
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

            StartProcess();
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Handles the vent triggered when InputText is changed
        /// </summary>
        private ValueTask OnInputTextChanged(string _)
        {
            StartProcess();
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Handles the tool chaining event from DevToys
        /// </summary>
        /// <param name="dataTypeName"></param>
        /// <param name="parsedData"></param>
        public void OnDataReceived(string dataTypeName, object? parsedData)
        {
            if (dataTypeName == PredefinedCommonDataTypeNames.Text && parsedData is string text)
            {
                _UITextInput.Text(text);
            }
        }

        private void SetHighlights(EDuplicateMode mode, LineCollection lines, List<Duplicate> duplicates)
        {
            var highlights = new List<UIHighlightedTextSpan>();

            // Highlights the duplicates
            if (duplicates.Any())
            {
                foreach (var lineNumber in duplicates.SelectMany(d => d.LineNbrs))
                {
                    if (lines.Collection.Any())
                    {
                        var line = lines
                            .Collection
                            .FirstOrDefault(l => l.LineNumber == lineNumber);

                        if (line is not null)
                            highlights.Add(new UIHighlightedTextSpan(line.Index + line.SearchedOffset, line.SearchedLength, UIHighlightedTextSpanColor.Red));
                    }
                }
            }

            // Highligths the searched part of the lines (only in Offset/Length mode and if not already highlighted)
            if (mode == EDuplicateMode.OffsetLength)
            {
                foreach (var line in lines
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
                }
            }

            _UITextInput.Highlight(highlights.ToArray());
        }

        /// <summary>
        /// Starts the search oof the duplicates
        /// </summary>
        private void StartProcess()
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
        /// <param name="cts">A cancellation token</param>
        /// <returns></returns>
        private async Task SearchDuplicates(string text, EDuplicateMode mode, int offset, int length, CancellationToken cts)
        {
            using (await _semaphore.WaitAsync(cts))
            {
                await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cts);

                try
                {
                    // Doing search
                    ResultInfo<(LineCollection, List<Duplicate>)> result = await DuplicateDetectorHelper.SearchDuplicatesAsync(
                         text,
                         mode,
                         offset,
                         length,
                         _logger,
                         cts);

                    if (!result.HasSucceeded ||
                        result.Data.Item1 is null ||
                        result.Data.Item2 is null)
                    {
                        cts.ThrowIfCancellationRequested();
                        throw new Exception("The duplicate search has failed");
                    }

                    var lines = result.Data.Item1;
                    var duplicates = result.Data.Item2;

                    // Diplaying result (duplicates + line numbers)
                    _UITextOutput.Text(
                        string.Join("\r\n",
                        duplicates.Select(d => $"{d.Value} [{string.Join(",", d.LineNbrs)}]")));

                    // Highlighting duplicates in the input MultiLine
                    SetHighlights(mode, lines, duplicates);
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
    }
}
