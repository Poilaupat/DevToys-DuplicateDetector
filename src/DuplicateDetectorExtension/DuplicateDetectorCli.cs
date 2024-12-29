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
using System.Security.Cryptography.X509Certificates;
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
            DescriptionResourceName = nameof(DuplicateDetectorExtension.ModeSettingDescription))]
        internal EDuplicateMode Mode { get; set; } = EDuplicateMode.Line;

        [CommandLineOption(
            Name = "offset",
            Alias = "off",
            DescriptionResourceName = nameof(DuplicateDetectorExtension.OffsetSettingDescription))]
        internal int? Offset { get; set; }

        [CommandLineOption(
            Name = "length",
            Alias = "len",
            DescriptionResourceName = nameof(DuplicateDetectorExtension.LengthSettingDescription))]
        internal int? Length { get; set; }

        [CommandLineOption(
            Name = "remove-duplicates",
            Alias = "rd",
            DescriptionResourceName = nameof(DuplicateDetectorExtension.RemoveDuplicatesSettingDescription))]
        internal bool? Removeduplicates { get; set; }

        [CommandLineOption(
           Name = "remove-duplicates-mode",
           Alias = "rdm",
           DescriptionResourceName = nameof(DuplicateDetectorExtension.RemoveDuplicatesModeSettingDescription))]
        internal ERemoveDuplicateMode RemoveDuplicatesMode { get; set; } = ERemoveDuplicateMode.KeepFirstOccurence;

        /// <summary>
        /// Executes the task
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

            if (Removeduplicates ?? false)
            {
                return await RemoveDuplicate(input.Data, logger, cancellationToken);
            }
            else
            {
                return await SeachDuplicates(input.Data, logger, cancellationToken);
            }
        }

        /// <summary>
        /// Search duplicates
        /// </summary>
        /// <param name="input">The text to search on</param>
        /// <param name="logger">A logger</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns></returns>
        private async ValueTask<int> SeachDuplicates(string input, ILogger logger, CancellationToken cancellationToken)
        {
            try
            {
                var dsr = DuplicateDetectorHelper.SearchDuplicates(
                    input,
                    Mode,
                    Offset,
                    Length,
                    cancellationToken
                );

                cancellationToken.ThrowIfCancellationRequested();

                var output = dsr.Duplicates.Any() ?
                    string.Join("\n", dsr.Duplicates.Select(d => $"{d.Value}[{string.Join(",", d.LineNbrs)}]")) :
                    DuplicateDetectorExtension.NoDuplicateFound;

                if (OutputFile is null)
                {
                    Console.WriteLine(output);
                }
                else
                {
                    await File.WriteAllTextAsync(OutputFile.FullName, output, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, DuplicateDetectorExtension.Error);
                Console.Error.WriteLine(DuplicateDetectorExtension.Error);
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Removes duplicates
        /// </summary>
        /// <param name="input">The text to work on</param>
        /// <param name="logger">A logger</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns></returns>
        private async ValueTask<int> RemoveDuplicate(string input, ILogger logger, CancellationToken cancellationToken)
        {
            try
            {
                var res = DuplicateDetectorHelper.RemoveDuplicates(
                    input,
                    Mode,
                    Offset,
                    Length,
                    RemoveDuplicatesMode,
                    cancellationToken
                );

                cancellationToken.ThrowIfCancellationRequested();

                var output = string.Join('\n', res);

                if (OutputFile is null)
                {
                    Console.WriteLine(output);
                }
                else
                {
                    await File.WriteAllTextAsync(OutputFile.FullName, output, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, DuplicateDetectorExtension.Error);
                Console.Error.WriteLine(DuplicateDetectorExtension.Error);
                return -1;
            }

            return 0;
        }

        //Reads an input file from the file system
        private static async Task<ResultInfo<string>> ReadFile(FileInfo inputFile, IFileStorage fileStorage, CancellationToken ct)
        {
            Guard.IsNotNull(fileStorage, "fileStorage");
            if (!fileStorage.FileExists(inputFile.FullName))
            {
                return new ResultInfo<string>(string.Empty, hasSucceeded: false);
            }

            using Stream fileStream = fileStorage.OpenReadFile(inputFile.FullName);
            using StreamReader reader = new StreamReader(fileStream);
            return new ResultInfo<string>(await reader.ReadToEndAsync(ct));
        }
    }
}
