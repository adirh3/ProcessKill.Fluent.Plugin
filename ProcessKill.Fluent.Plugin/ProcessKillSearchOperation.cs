using Blast.Core.Results;

namespace ProcessKill.Fluent.Plugin
{
    public class ProcessKillSearchOperation : SearchOperationBase
    {
        internal const string KillIconGlyph = "\uEA39";

        public ProcessKillSearchOperation() : base("Kill", "Kill the process", KillIconGlyph)
        {
        }
    }
}