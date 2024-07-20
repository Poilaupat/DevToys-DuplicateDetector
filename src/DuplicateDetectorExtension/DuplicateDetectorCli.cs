using CommunityToolkit.Diagnostics;
using DevToys.Api;
using DuplicateDetectorExtension.Models;
using Microsoft.Extensions.Logging;
using OneOf;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DuplicateDetectorExtension
{
    [Export(typeof(ICommandLineTool))]
    [Name("DuplicateDetector")]
    [CommandName(
        Name = "duplicateDetector",
        Alias = "dd",
        ResourceManagerBaseName = "DuplicateDetectorExtension.DuplicateDetectorExtension",
        DescriptionResourceName = nameof(DuplicateDetectorExtension.Description))]
    internal sealed class DuplicateDetectorCli : ICommandLineTool
    {
        [Import]
        internal IFileStorage _fileStorage = null!;

        [CommandLineOption(
            Name = "input",
            Alias = "i",
            IsRequired = true,
            DescriptionResourceName = nameof(DuplicateDetectorExtension.Input))]
        internal FileInfo? Input { get; set; }

        [CommandLineOption(
            Name = "outputFile",
            Alias = "o",
            DescriptionResourceName = nameof(DuplicateDetectorExtension.OutputFileDescription))]
        internal FileInfo? OutputFile { get; set; }

        [CommandLineOption(
            Name = "mode",
            Alias = "m",
            IsRequired = true,
            DescriptionResourceName = nameof(DuplicateDetectorExtension.ModeDescription))]
        internal EDuplicateMode Mode { get; set; } = EDuplicateMode.Line;

        [CommandLineOption(
            Name = "offset",
            Alias = "off",
            DescriptionResourceName = nameof(DuplicateDetectorExtension.OffsetDescription))]
        internal int? Offset { get; set; }

        [CommandLineOption(
            Name = "length",
            Alias = "len",
            DescriptionResourceName = nameof(DuplicateDetectorExtension.LengthDescription))]
        internal int? Length { get; set; }

        /// <summary>
        /// Search the duplicates
        /// </summary>
        /// <param name="logger">A logger</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns></returns>
        public async ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
        {
            if (Input is null)
            {
                Console.Error.WriteLine(DuplicateDetectorExtension.MissingInput);
                return -1;
            }

            ResultInfo<string> input = await ReadFile(Input, _fileStorage, cancellationToken);
            if (!input.HasSucceeded)
            {
                Console.Error.WriteLine(string.Format(DuplicateDetectorExtension.InputFileNotFound, Input.Name));
                return -1;
            }

            if (Mode == EDuplicateMode.OffsetLength && (Offset is null || Length is null))
            {
                Console.Error.WriteLine(DuplicateDetectorExtension.MissingOffsetOrLength);
                return -1;
            }

            Guard.IsNotNull(input.Data);

            ResultInfo<(LineCollection, List<Duplicate>)> result = await DuplicateDetectorHelper.SearchDuplicatesAsync(
                input.Data,
                Mode,
                Offset,
                Length,
                logger,
                cancellationToken
            );

            cancellationToken.ThrowIfCancellationRequested();

            if (!result.HasSucceeded)
            {
                Console.Error.WriteLine(DuplicateDetectorExtension.Error);
                return -1;
            }

            var output = result.Data.Item2.Any() ?
                string.Join("\n", result.Data.Item2.Select(d => $"{d.Value}[{string.Join(",", d.LineNbrs)}]")) :
                DuplicateDetectorExtension.NoDuplicateFound;

            if (OutputFile is null)
            {
                Console.WriteLine(output);
            }
            else
            {
                await File.WriteAllTextAsync(OutputFile.FullName, output, cancellationToken);
            }
            
            return 0;
        }

        private static async Task<ResultInfo<string>> ReadFile(FileInfo inputFile, IFileStorage fileStorage, CancellationToken ct)
        {
            Guard.IsNotNull(fileStorage, "fileStorage");
            if (!fileStorage.FileExists(inputFile.FullName))
            {
                return new ResultInfo<string>("", hasSucceeded: false);
            }

            using Stream fileStream = fileStorage.OpenReadFile(inputFile.FullName);
            using StreamReader reader = new StreamReader(fileStream);
                return new ResultInfo<string>(await reader.ReadToEndAsync(ct));
        }
    }
}
