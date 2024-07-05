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

namespace DuplicateFinderExtension
{
    [Export(typeof(IGuiTool))]
    [Name("DuplicateFinder")]
    [ToolDisplayInformation(
        IconFontName = "FluentSystemIcon",
        IconGlyph = '\u26AD',
        GroupName = PredefinedCommonToolGroupNames.Text,
        ResourceManagerAssemblyIdentifier = nameof(ResourceAssemblyIdentifier),
        ResourceManagerBaseName = "DuplicateFinderExtension.DuplicateFinderExtension",
        ShortDisplayTitleResourceName = nameof(DuplicateFinderExtension.ShortDisplayTitle),
        LongDisplayTitleResourceName = nameof(DuplicateFinderExtension.LongDisplayTitle),
        DescriptionResourceName = nameof(DuplicateFinderExtension.Description),
        AccessibleNameResourceName = nameof(DuplicateFinderExtension.AccessibleName)
    )]
    [AcceptedDataTypeName(PredefinedCommonDataTypeNames.Text)]
    internal sealed class DuplicateFinderGui : IGuiTool
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
            name: $"{nameof(DuplicateFinderGui)}.{nameof(_SettingMode)}",
            defaultValue: EDuplicateMode.Line);

        private static readonly SettingDefinition<int> _SettingOffset
            = new(
                name: $"{nameof(DuplicateFinderGui)}.{nameof(_SettingOffset)}",
                defaultValue: 0);

        private static readonly SettingDefinition<int> _SettingLength
            = new(
                name: $"{nameof(DuplicateFinderGui)}.{nameof(_SettingLength)}",
                defaultValue: 0);

        #endregion

        #region UIElements

        private readonly IUISetting _UISettingMode = Setting("duplicate-finder-mode-setting");
        private readonly IUISetting _UISettingOffset = Setting("duplicate-finder-offset-setting");
        private readonly IUISetting _UISettingLength = Setting("duplicate-finder-length-setting");

        private IUIMultiLineTextInput _UITextInput = MultiLineTextInput("duplicate-finder-input-text");
        private IUIMultiLineTextInput _UITextOutput = MultiLineTextInput("duplicate-finder-ouput-text");

        #endregion

        private readonly DisposableSemaphore _semaphore = new();
        private readonly ILogger _logger;
        private CancellationTokenSource? _cts;
        internal IList<Duplicate> _duplicates = new List<Duplicate>();

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
                                        .Text(DuplicateFinderExtension.Configuration),
                                    SettingGroup("")
                                        .Icon("FluentSystemIcons", '\uF6A9')
                                        .Title(DuplicateFinderExtension.Options)
                                        .WithSettings(

                                            _UISettingMode
                                                .Title(DuplicateFinderExtension.Mode)
                                                .Description(DuplicateFinderExtension.ModeDescription)
                                                .Handle(
                                                    _settingsProvider,
                                                    _SettingMode,
                                                    onOptionSelected: OnDuplicateSearchModeChanged,
                                                    Item(DuplicateFinderExtension.Line, EDuplicateMode.Line),
                                                    Item(DuplicateFinderExtension.OffsetLength, EDuplicateMode.OffsetLength)),

                                            _UISettingOffset
                                                .Title(DuplicateFinderExtension.Offset)
                                                .Description(DuplicateFinderExtension.OffsetDescription)
                                                .InteractiveElement(
                                                    NumberInput()
                                                        .HideCommandBar()
                                                        .Minimum(0)
                                                        .OnValueChanged(OnOffsetSettingChanged)
                                                        .Value(_settingsProvider.GetSetting(_SettingOffset))),

                                            _UISettingLength
                                                .Title(DuplicateFinderExtension.Length)
                                                .Description(DuplicateFinderExtension.LengthDescription)
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
                                .Title(DuplicateFinderExtension.Input)
                                .Extendable()
                                .OnTextChanged(OnInputTextChanged))
                            .WithRightPaneChild(
                                _UITextOutput
                                .Title(DuplicateFinderExtension.Duplicates)
                                .ReadOnly()
                                .HideCommandBar()
                                .NeverShowLineNumber()))
                    ));

        [ImportingConstructor]
        public DuplicateFinderGui(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
            _logger = this.Log();

            OnDuplicateSearchModeChanged(_settingsProvider.GetSetting(_SettingMode));
        }

        private ValueTask OnOffsetSettingChanged(double value)
        {
            _settingsProvider.SetSetting(_SettingOffset, (int)value);
            StartProcess();
            return ValueTask.CompletedTask;
        }

        private ValueTask OnLengthSettingChanged(double value)
        {
            _settingsProvider.SetSetting(_SettingLength, (int)value);
            StartProcess();
            return ValueTask.CompletedTask;
        }

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

        private ValueTask OnInputTextChanged(string _)
        {
            StartProcess();
            return ValueTask.CompletedTask;
        }

        public void OnDataReceived(string dataTypeName, object? parsedData)
        {
            if (dataTypeName == PredefinedCommonDataTypeNames.Text && parsedData is string text)
            {
                _UITextInput.Text(text);
            }
        }

        private void SetHighlights()
        {
            var highlights = new List<UIHighlightedTextSpan>();
            var lines = Regex.Matches(_UITextInput.Text, $"(\r\n)");
            foreach (var lineNbr in _duplicates
                .SelectMany(d => d.LineNbrs)
                .OrderBy(n => n))
            {
                int index, length;

                if (lineNbr > 1)
                {
                    index = lines[lineNbr - 2].Index + 2;
                    length = lines[lineNbr - 1].Index - lines[lineNbr - 2].Index - 2;
                }
                else
                {
                    index = 0;
                    length = lines[lineNbr - 1].Index;
                }

                highlights.Add(new UIHighlightedTextSpan(index, length, UIHighlightedTextSpanColor.Red));
            }

            _UITextInput.Highlight(highlights.ToArray());
        }

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

        private async Task SearchDuplicates(string text, EDuplicateMode mode, int offset, int length, CancellationToken cts)
        {
            using (await _semaphore.WaitAsync(cts))
            {
                await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cts);

                try
                {
                    LineCollection c = new LineCollection(mode, offset, length);
                    c.LoadText(text);

                    var lines = text
                        .Split("\r\n")
                        .AsQueryable();

                    if (mode == EDuplicateMode.OffsetLength)
                        lines = lines
                            .Select(l => l.Length >= (offset + length) ? l.Substring(offset, length) : string.Empty);

                    _duplicates = lines
                        .Where(l => !string.IsNullOrEmpty(l))
                        .GroupBy(l => l)
                        .Where(g => g.Count() > 1)
                        .Select(g => new Duplicate(g.Key))
                        .ToList();

                    foreach(var item in lines.Select((line, index) => new { index, line }))
                    {
                        var duplicate = _duplicates
                            .FirstOrDefault(d => d.Value.Equals(item.line));

                        if(duplicate is not null)
                        {
                            duplicate.LineNbrs.Add(item.index + 1);
                        }
                    }

                    _UITextOutput.Text(
                        string.Join("\r\n", 
                        _duplicates.Select(d => $"{d.Value}[{string.Join(",", d.LineNbrs)}]")));

                    SetHighlights();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error while searching duplicates (Mode = {mode})");
                }
            }
        }
    }
}
