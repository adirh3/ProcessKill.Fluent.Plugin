﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Blast.API.Core.Processes;
using Blast.API.Processes;
using Blast.API.Search;
using Blast.Core;
using Blast.Core.Interfaces;
using Blast.Core.Objects;
using Blast.Core.Results;

namespace ProcessKill.Fluent.Plugin
{
    public class ProcessKillSearchApp : ISearchApplication
    {
        private readonly SearchApplicationInfo _applicationInfo;
        private readonly IProcessManager _processManager;

        public ProcessKillSearchApp()
        {
            _applicationInfo = new SearchApplicationInfo("Kill process",
                "Kill processes (yes without mercy)", ProcessKillSearchResult.KillSupportedOperations)
            {
                SearchTagName = ProcessKillSearchResult.KillTag,
                MinimumSearchLength = 1,
                SearchTagOnly = true,
                IsProcessSearchEnabled = false,
                IsProcessSearchOffline = false,
                ApplicationIconGlyph = ProcessKillSearchOperation.KillIconGlyph,
                SearchAllTime = ApplicationSearchTime.Moderate,
                DefaultSearchTags = new List<SearchTag>
                {
                    new()
                    {
                        Name = ProcessKillSearchResult.KillTag,
                        IconGlyph = ProcessKillSearchOperation.KillIconGlyph
                    },
                    new()
                    {
                        Name = ProcessKillSearchResult.KillAllTag,
                        IconGlyph = ProcessKillSearchOperation.KillIconGlyph
                    }
                }
            };
            _processManager = ProcessUtils.GetManagerInstance();
        }

        public SearchApplicationInfo GetApplicationInfo()
        {
            return _applicationInfo;
        }

        public IAsyncEnumerable<ISearchResult> SearchAsync(SearchRequest searchRequest,
            CancellationToken cancellationToken)
        {
            string searchedText = searchRequest.SearchedText;
            string searchedTag = searchRequest.SearchedTag;

            if (string.IsNullOrWhiteSpace(searchedText) || string.IsNullOrWhiteSpace(searchedTag) ||
                !searchedTag.Equals(ProcessKillSearchResult.KillTag, StringComparison.OrdinalIgnoreCase) &&
                !searchedTag.Equals(ProcessKillSearchResult.KillAllTag, StringComparison.OrdinalIgnoreCase))
                return SynchronousAsyncEnumerable.Empty;

            return new SynchronousAsyncEnumerable(SearchProcesses(searchedText, searchedTag));
        }

        private static IEnumerable<ISearchResult> SearchProcesses(string searchedText, string searchedTag)
        {
            KillOperationType killOperationType = KillOperationType.Single;
            if (searchedTag.Equals(ProcessKillSearchResult.KillAllTag, StringComparison.OrdinalIgnoreCase))
                killOperationType = KillOperationType.All;

            var processNames = new HashSet<string>();
            foreach (Process process in Process.GetProcesses())
            {
                string processName = process.ProcessName;

                if (killOperationType == KillOperationType.All && processNames.Contains(processName))
                    continue;

                processNames.Add(processName);
                double score = processName.SearchTokens(searchedText);
                if (score == 0)
                    continue;

                yield return new ProcessKillSearchResult(processName, searchedText, score, process, killOperationType);
                process.Dispose();
            }
        }


        public ValueTask<IHandleResult> HandleSearchResult(ISearchResult searchResult)
        {
            if (searchResult is not ProcessKillSearchResult processKillSearchResult)
                throw new InvalidCastException(nameof(processKillSearchResult));
            if (processKillSearchResult.SelectedOperation is not ProcessKillSearchOperation processKillSearchOperation)
                throw new InvalidCastException(nameof(processKillSearchOperation));

            string processName = processKillSearchResult.ProcessName + ".exe";
            string args = processKillSearchResult.KillOperationType switch
            {
                KillOperationType.All => $"/im \"{processName}\"",
                KillOperationType.Single => $"/pid {processKillSearchResult.ProcessId}",
                _ => throw new ArgumentOutOfRangeException()
            };
            Process.Start(new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = "/f " + args,
                Verb = processKillSearchOperation.RunAsAdmin ? "runas" : string.Empty,
                CreateNoWindow = true
            })?.Dispose();

            processKillSearchResult.RemoveResult = true;

            return new ValueTask<IHandleResult>(new HandleResult(true, false));
        }
    }
}