using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Blast.API.FileSystem;
using Blast.Core.Interfaces;
using Blast.Core.Results;

namespace ProcessKill.Fluent.Plugin
{
    public class ProcessKillSearchResult : SearchResultBase
    {
        internal string ProcessName { get; }
        internal const string KillTag = "Kill";

        private static readonly SearchTag[] SearchTags = {new() {Name = KillTag}};

        internal static readonly ObservableCollection<ISearchOperation> KillSupportedOperations =
            new() {new ProcessKillSearchOperation()};


        public ProcessKillSearchResult(string processName, string searchedText, double score, Process process) : base(
            processName,
            searchedText, "Kill", score,
            KillSupportedOperations,
            new ObservableCollection<SearchTag>(SearchTags.ToList().Append(new SearchTag {Name = processName})))
        {
            ProcessName = processName;
            try
            {
                string processFile = process.MainModule?.FileName;
                if (string.IsNullOrWhiteSpace(processFile) || !File.Exists(processFile))
                    return;
                PreviewImage = FileUtils.FileUtilsInstance.GetFileIconHighRes(processFile);
            }
            catch (Exception)
            {
                // ignored, can't extract icon
            }
        }

        protected override void OnSelectedSearchResultChanged()
        {
        }
    }
}