using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Laragon
{
    /// <summary>
    /// Whether this code is running inside the Visual Studio designer rather than in the
    /// program.
    ///
    /// The designer really does build a window to show it, so its constructor runs: without this
    /// test, opening the main window in the designer would read the person's ini, load icons and
    /// — because the ini says AutoStart — start Apache and MySQL on the developer's machine. A
    /// constructor that has nothing better to do at design time asks this first and stops.
    /// </summary>
    internal static class DesignTime
    {
        private static bool? _designing;

        internal static bool IsDesigning
        {
            get
            {
                if (!_designing.HasValue)
                {
                    _designing = WorkedOut();
                }

                return _designing.Value;
            }
        }

        private static bool WorkedOut()
        {
            // the normal answer: a component built by the designer reports so through its licence
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return true;
            }

            // the second answer: the designer runs in a program of its own, which is not ours
            try
            {
                string name = Process.GetCurrentProcess().ProcessName ?? string.Empty;

                return name.StartsWith("devenv", StringComparison.OrdinalIgnoreCase)
                    || name.StartsWith("DesignToolsServer", StringComparison.OrdinalIgnoreCase)
                    || name.StartsWith("XDesProc", StringComparison.OrdinalIgnoreCase)
                    || name.IndexOf("designer", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
