using DuplicateDetectorExtension.Models;

namespace DuplicateDetector.Tests
{
    public class DuplicateDetectorGuiTests : TestBase
    {
        private readonly DuplicateDetectorGui _toolHook;
        
        private readonly UIToolView _uiToolHook;
        private readonly IUIMultiLineTextInput _uiTextInput;
        private readonly IUIMultiLineTextInput _uiTextOutput;
        private readonly IUISelectDropDownList _uiSettingMode;
        private readonly IUINumberInput _uiSettingOffset;
        private readonly IUINumberInput _uiSettingLength;
        private readonly IUIButton _uiButtonUnduplicate;

        public DuplicateDetectorGuiTests()
        {
            _toolHook = new DuplicateDetectorGui(new MockISettingProvider());
            
            _uiToolHook = _toolHook.View;
            _uiTextInput = (IUIMultiLineTextInput)_uiToolHook.GetChildElementById("duplicate-detector-input-text");
            _uiTextOutput = (IUIMultiLineTextInput)_uiToolHook.GetChildElementById("duplicate-detector-ouput-text");
            _uiSettingMode = (IUISelectDropDownList)((IUISetting)_uiToolHook.GetChildElementById("duplicate-detector-mode-setting")).InteractiveElement;
            _uiSettingOffset = (IUINumberInput)((IUISetting)_uiToolHook.GetChildElementById("duplicate-detector-offset-setting")).InteractiveElement;
            _uiSettingLength = (IUINumberInput)((IUISetting)_uiToolHook.GetChildElementById("duplicate-detector-length-setting")).InteractiveElement;
            _uiButtonUnduplicate = (IUIButton)_uiToolHook.GetChildElementById("duplicate-detector-remove-button");
        }

        [Theory]
        [MemberData(nameof(GetTestData_WholeLineTest))]
        public async Task WholeLineTest(string input, string expected, int expectedReds, bool unduplicateButtonEnabled)
        {
            //Setting mode to "Line"
            _uiSettingMode.Select(0);

            _uiTextInput.Text(input);
            await _toolHook.WorkTask;
            _uiTextOutput.Text.Should().Be(expected);
            _uiTextInput.HighlightedSpans.Count(h => h.Color == UIHighlightedTextSpanColor.Green).Should().Be(0);
            _uiTextInput.HighlightedSpans.Count(h => h.Color == UIHighlightedTextSpanColor.Red).Should().Be(expectedReds);

            _uiButtonUnduplicate.IsEnabled.Should().Be(unduplicateButtonEnabled);
        }

        public static IEnumerable<object[]> GetTestData_WholeLineTest()
        {
            yield return new object[] { Properties.Resources.LineTest_NoDuplicate, string.Empty, 0, false };
            yield return new object[] { Properties.Resources.LineTest_ActualDuplicate, Properties.Resources.LineTest_ActualDuplicate_Expected, 7, true };
        }

        [Theory]
        [MemberData(nameof(GetTestData_OffsetLengthLineTest))]
        public async Task OffsetLengthLineTest(string input, string offset, string length, string expected, int expectedGreens, int expectedReds, bool unduplicateButtonEnabled)
        {
            //Setting mode to "OffsetLength"
            _uiSettingMode.Select(1);
            _uiSettingOffset.Text(offset);
            _uiSettingLength.Text(length);

            _uiTextInput.Text(input);
            await _toolHook.WorkTask;
            _uiTextOutput.Text.Should().Be(expected);
            _uiTextInput.HighlightedSpans.Count(h => h.Color == UIHighlightedTextSpanColor.Green).Should().Be(expectedGreens);
            _uiTextInput.HighlightedSpans.Count(h => h.Color == UIHighlightedTextSpanColor.Red).Should().Be(expectedReds);
            
            _uiButtonUnduplicate.IsEnabled.Should().Be(unduplicateButtonEnabled);
        }

        public static IEnumerable<object[]> GetTestData_OffsetLengthLineTest()
        {
            yield return new object[] { Properties.Resources.OffsetLengthTest, "27", "1", string.Empty, 17, 0, false };
            yield return new object[] { Properties.Resources.OffsetLengthTest, "1", "5", Properties.Resources.OffsetLengthTest_ActualDuplicate_Expected, 12, 5, true };
        }


        [Theory]
        [MemberData(nameof(GetTestData_DeduplicateLineTest))]
        public void DeduplicateLineTest(string text, ERemoveDuplicateMode unduplicateMode, string expected)
        {
            var cts = new CancellationTokenSource();

            var result = DuplicateDetectorHelper.RemoveDuplicates(
                text,
                EDuplicateMode.Line,
                null,
                null,
                unduplicateMode,
                cts.Token)
                .Where(x => !string.IsNullOrWhiteSpace(x));

            string.Join("\n", result).Should().Be(expected);
        }

        public static IEnumerable<object[]> GetTestData_DeduplicateLineTest()
        {
            yield return new object[] { Properties.Resources.Deduplicate_LineTest, ERemoveDuplicateMode.KeepFirstOccurence, "un\ndeux\ntrois"};
            yield return new object[] { Properties.Resources.Deduplicate_LineTest, ERemoveDuplicateMode.KeepLastOccurence, "deux\ntrois\nun"};
            yield return new object[] { Properties.Resources.Deduplicate_LineTest, ERemoveDuplicateMode.RemoveAll, "deux\ntrois"};
        }
    }
}