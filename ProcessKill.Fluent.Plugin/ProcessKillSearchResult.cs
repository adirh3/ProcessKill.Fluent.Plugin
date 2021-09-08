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
    public enum KillOperationType
    {
        Single,
        All
    }

    public class ProcessKillSearchResult : SearchResultBase
    {
        internal string ProcessName { get; }
        internal KillOperationType KillOperationType { get; }

        internal const string KillTag = "Kill";
        internal const string KillAllTag = "KillAll";

        private static readonly SearchTag[] SearchTags = {new() {Name = KillTag}};

        internal static readonly ObservableCollection<ISearchOperation> KillSupportedOperations =
            new() {new ProcessKillSearchOperation(false), new ProcessKillSearchOperation(true)};


        public ProcessKillSearchResult(string processName, string searchedText, double score, Process process,
            KillOperationType killOperationType) : base(
            processName,
            searchedText, "Kill", score,
            KillSupportedOperations,
            new ObservableCollection<SearchTag>(SearchTags.ToList().Append(new SearchTag {Name = processName})))
        {
            ProcessName = processName;
            KillOperationType = killOperationType;

            try
            {
                ProcessId = AdditionalInformation = process.Id.ToString();
                string windowTitle = process.MainWindowTitle;
                if (!string.IsNullOrWhiteSpace(windowTitle))
                {
                    AdditionalInformation = windowTitle;
                    Score += 4;
                }

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

        public string ProcessId { get; set; }

        protected override void OnSelectedSearchResultChanged()
        {
        }
    }
}