using DuplicateDetector.Tests.Mocks;
using DuplicateDetectorExtension.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateDetector.Tests
{
    public class DuplicateDetectorCliTests : TestBase
    {
        private readonly MockIFileStorage _fileStorage = new();
        private readonly StringWriter _consoleWriter = new();
        private readonly StringWriter _consoleErrorWriter = new();
        private readonly DuplicateDetectorCli _tool;
        private readonly Mock<ILogger> _loggerMock;
        
        public DuplicateDetectorCliTests()
        {
            _tool = new DuplicateDetectorCli();
            _tool._fileStorage = _fileStorage; 

            _loggerMock = new Mock<ILogger>();
            
            _consoleWriter = new StringWriter();
            _consoleErrorWriter = new StringWriter();
            Console.SetOut(_consoleWriter);
            Console.SetError(_consoleErrorWriter);
        }

        [Fact]
        public async void MissingInput()
        {
            _tool.Input = null;

            int result = await _tool.InvokeAsync(_loggerMock.Object, default);
            result.Should().Be(-1);

            string consoleOutput = _consoleErrorWriter.ToString().Trim();
            consoleOutput.Should().Be("Missing Input parameter");
        }

        [Fact]
        public async void MissingOffset()
        {
            string filepath = Path.Combine("TestData", "OffsetLength.txt");
            _tool.Input = new FileInfo(filepath);

            _tool.Mode = EDuplicateMode.OffsetLength;
            _tool.Length = 1;

            int result = await _tool.InvokeAsync(_loggerMock.Object, default);
            result.Should().Be(-1);

            string consoleOutput = _consoleErrorWriter.ToString().Trim();
            consoleOutput.Should().Be("If mode is OffsetLength, Offset and Length parameters become mandatory");
        }

        [Fact]
        public async void MissingLength()
        {
            string filepath = Path.Combine("TestData", "OffsetLength.txt");
            _tool.Input = new FileInfo(filepath);

            _tool.Mode = EDuplicateMode.OffsetLength;
            _tool.Offset = 1;

            int result = await _tool.InvokeAsync(_loggerMock.Object, default);
            result.Should().Be(-1);

            string consoleOutput = _consoleErrorWriter.ToString().Trim();
            consoleOutput.Should().Be("If mode is OffsetLength, Offset and Length parameters become mandatory");
        }

        [Fact]
        public async void LineModeNoDuplicate()
        {
            string filepath = Path.Combine("TestData", "LineWithoutDuplicate.txt");
            _tool.Input = new FileInfo(filepath);

            _tool.Mode = EDuplicateMode.Line;

            int result = await _tool.InvokeAsync(_loggerMock.Object, default);
            result.Should().Be(0);

            string consoleOutput = _consoleWriter.ToString().Trim();
            consoleOutput.Should().Be("No duplicate found");
        }

        [Fact]
        public async void OffsetLengthModeNoDuplicate()
        {
            string filepath = Path.Combine("TestData", "OffsetLength.txt");
            _tool.Input = new FileInfo(filepath);

            _tool.Mode = EDuplicateMode.OffsetLength;
            _tool.Offset = 6;
            _tool.Length = 2;

            int result = await _tool.InvokeAsync(_loggerMock.Object, default);
            result.Should().Be(0);

            string consoleOutput = _consoleWriter.ToString().Trim();
            consoleOutput.Should().Be("No duplicate found");
        }

        [Fact]
        public async void LineModeDuplicates()
        {
            string filepath = Path.Combine("TestData", "LineWithDuplicates.txt");
            _tool.Input = new FileInfo(filepath);

            _tool.Mode = EDuplicateMode.Line;

            int result = await _tool.InvokeAsync(_loggerMock.Object, default);
            result.Should().Be(0);

            string consoleOutput = _consoleWriter.ToString().Trim();
            consoleOutput.Should().Be("Rex[1,9]\nMilou[3,8,11]");
        }

        [Fact]
        public async void OffsetLengthModeDuplicates()
        {
            string filepath = Path.Combine("TestData", "OffsetLength.txt");
            _tool.Input = new FileInfo(filepath);

            _tool.Mode = EDuplicateMode.OffsetLength;
            _tool.Offset = 2;
            _tool.Length = 4;  

            int result = await _tool.InvokeAsync(_loggerMock.Object, default);
            result.Should().Be(0);

            string consoleOutput = _consoleWriter.ToString().Trim();
            consoleOutput.Should().Be("aaaa[1,6]\ndddd[4,9]");
        }

        [Fact]
        public async void LineModeRemove_KeepFirst()
        {
            string filepath = Path.Combine("TestData", "LineWithDuplicates.txt");
            _tool.Input = new FileInfo(filepath);

            _tool.Mode = EDuplicateMode.Line;
            _tool.Removeduplicates = true;
            _tool.RemoveDuplicatesMode = ERemoveDuplicateMode.KeepFirstOccurence;

            int result = await _tool.InvokeAsync(_loggerMock.Object, default);
            result.Should().Be(0);

            string consoleOutput = _consoleWriter.ToString().Trim();
            consoleOutput.Should().Be("Rex\nRintintin\nMilou\nIdefix\nLassie\nKlebar\nKador\nSnoopy");
        }

        [Fact]
        public async void LineModeRemove_KeepLast()
        {
            string filepath = Path.Combine("TestData", "LineWithDuplicates.txt");
            _tool.Input = new FileInfo(filepath);

            _tool.Mode = EDuplicateMode.Line;
            _tool.Removeduplicates = true;
            _tool.RemoveDuplicatesMode = ERemoveDuplicateMode.KeepLastOccurence;

            int result = await _tool.InvokeAsync(_loggerMock.Object, default);
            result.Should().Be(0);

            string consoleOutput = _consoleWriter.ToString().Trim();
            consoleOutput.Should().Be("Rintintin\nIdefix\nLassie\nKlebar\nKador\nRex\nSnoopy\nMilou");
        }

        [Fact]
        public async void LineModeRemove_RemoveAll()
        {
            string filepath = Path.Combine("TestData", "LineWithDuplicates.txt");
            _tool.Input = new FileInfo(filepath);

            _tool.Mode = EDuplicateMode.Line;
            _tool.Removeduplicates = true;
            _tool.RemoveDuplicatesMode = ERemoveDuplicateMode.RemoveAll;

            int result = await _tool.InvokeAsync(_loggerMock.Object, default);
            result.Should().Be(0);

            string consoleOutput = _consoleWriter.ToString().Trim();
            consoleOutput.Should().Be("Rintintin\nIdefix\nLassie\nKlebar\nKador\nSnoopy");
        }

        [Fact]
        public async void OffsetLengthModeRemove_KeepFirst()
        {
            string filepath = Path.Combine("TestData", "OffsetLength.txt");
            _tool.Input = new FileInfo(filepath);

            _tool.Mode = EDuplicateMode.OffsetLength;
            _tool.Offset = 2;
            _tool.Length = 4;
            _tool.Removeduplicates = true;
            _tool.RemoveDuplicatesMode = ERemoveDuplicateMode.KeepFirstOccurence;

            int result = await _tool.InvokeAsync(_loggerMock.Object, default);
            result.Should().Be(0);

            string consoleOutput = _consoleWriter.ToString().Trim();
            consoleOutput.Should().Be("00aaaa0000\n11bbbb1111\n22cccc2222\n33dddd3333\n44eeee4444\n66ffff6666\n77gggg7777\n99hhhh9999");
        }

        [Fact]
        public async void OffsetLengthModeRemove_KeepLast()
        {
            string filepath = Path.Combine("TestData", "OffsetLength.txt");
            _tool.Input = new FileInfo(filepath);

            _tool.Mode = EDuplicateMode.OffsetLength;
            _tool.Offset = 2;
            _tool.Length = 4;
            _tool.Removeduplicates = true;
            _tool.RemoveDuplicatesMode = ERemoveDuplicateMode.KeepLastOccurence;

            int result = await _tool.InvokeAsync(_loggerMock.Object, default);
            result.Should().Be(0);

            string consoleOutput = _consoleWriter.ToString().Trim();
            consoleOutput.Should().Be("11bbbb1111\n22cccc2222\n44eeee4444\n55aaaa5555\n66ffff6666\n77gggg7777\n88dddd8888\n99hhhh9999");
        }

        [Fact]
        public async void OffsetLengthModeRemove_RemoveAll()
        {
            string filepath = Path.Combine("TestData", "OffsetLength.txt");
            _tool.Input = new FileInfo(filepath);

            _tool.Mode = EDuplicateMode.OffsetLength;
            _tool.Offset = 2;
            _tool.Length = 4;
            _tool.Removeduplicates = true;
            _tool.RemoveDuplicatesMode = ERemoveDuplicateMode.RemoveAll;

            int result = await _tool.InvokeAsync(_loggerMock.Object, default);
            result.Should().Be(0);

            string consoleOutput = _consoleWriter.ToString().Trim();
            consoleOutput.Should().Be("11bbbb1111\n22cccc2222\n44eeee4444\n66ffff6666\n77gggg7777\n99hhhh9999");
        }
    }
}
