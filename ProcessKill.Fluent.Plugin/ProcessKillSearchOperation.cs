using Avalonia.Input;
using Blast.Core.Results;

namespace ProcessKill.Fluent.Plugin
{
    public class ProcessKillSearchOperation : SearchOperationBase
    {
        internal bool RunAsAdmin { get; }

        internal const string KillIconGlyph = "\uEA39";

        public ProcessKillSearchOperation(bool runAsAdmin) : base("Kill" + (runAsAdmin ? " as admin" : string.Empty),
            "Kill the process" + (runAsAdmin ? " as admin" : string.Empty), KillIconGlyph)
        {
            HideMainWindow = false;
            RunAsAdmin = runAsAdmin;
            KeyGesture = runAsAdmin switch
            {
                true => new KeyGesture(Key.Delete, KeyModifiers.Shift),
                false => new KeyGesture(Key.Delete)
            };
        }
    }
}