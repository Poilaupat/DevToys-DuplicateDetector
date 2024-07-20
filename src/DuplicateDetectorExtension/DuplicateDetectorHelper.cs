using DevToys.Api;
using DuplicateDetectorExtension.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DuplicateDetectorExtension
{
    internal static class DuplicateDetectorHelper
    {
        /// <summary>
        /// Finds duplicates in provided text
        /// </summary>
        /// <param name="text">The text to search duplicates on</param>
        /// <param name="mode">The search mode (line or offset/length)</param>
        /// <param name="offset">The offset in each line</param>
        /// <param name="length">The length in each line</param>
        /// <param name="logger">A logger</param>
        /// <param name="ct">The cancellation token</param>
        /// <returns>A <see cref="ResultInfo{T}"/> containing the found <see cref="LineCollection"/> and the list of <see cref="Duplicate"/> </returns>
        public static async ValueTask<ResultInfo<(LineCollection lines, List<Duplicate> duplicates)>> SearchDuplicatesAsync(
            string text,
            EDuplicateMode mode, 
            int? offset, 
            int? length,
            ILogger logger,
            CancellationToken ct)
        {
            await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(ct);

            // Loading text as a collection of lines (compatible UNIX-Windows)
            var lines = new LineCollection(mode, offset, length);
            lines.LoadText(text);

            ct.ThrowIfCancellationRequested();

            // Finding duplicates
             var duplicates = lines
                .Collection
                .Select(c => c.SearchedValue)
                .Where(l => !string.IsNullOrEmpty(l))
                .GroupBy(l => l)
                .Where(g => g.Count() > 1)
                .Select(g => new Duplicate(g.Key))
                .ToList();

            ct.ThrowIfCancellationRequested();

            // Identifying the number of each line that is a duplicate 
            foreach (var line in lines.Collection)
            {
                var duplicate = duplicates
                    .FirstOrDefault(d => d.Value.Equals(line.SearchedValue));

                if (duplicate is not null)
                {
                    duplicate.LineNbrs.Add(line.LineNumber);
                }

                ct.ThrowIfCancellationRequested();
            }

            return new((lines, duplicates));
        }
    }
}
