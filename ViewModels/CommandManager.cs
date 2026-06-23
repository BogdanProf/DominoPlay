using System;

namespace DominoPlay.ViewModels
{
    public static class CommandManager
    {
        public static event EventHandler? RequerySuggested;

        public static void InvalidateRequerySuggested()
        {
            RequerySuggested?.Invoke(null, EventArgs.Empty);
        }
    }
}