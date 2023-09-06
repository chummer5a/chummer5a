using System;
using System.Windows.Forms;
using NLog;

namespace Chummer
{
    /// <summary>
    /// Class to handle the forceful shutdown of Chummer if the eventloop stucks for some reason.
    /// </summary>
    public class ApplicationMonitor
    {
        private readonly System.Timers.Timer timer;
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private int elapsedCount;

        public ApplicationMonitor(int terminateAfterMilliseconds)
        {
            timer = new System.Timers.Timer();
            timer.Interval = terminateAfterMilliseconds;
            timer.Elapsed += TimerElapsed;
        }

        /// <summary>
        /// Starts the timer that will kill the event loop and the application if not stopped.
        /// </summary>
        public void StartMonitoring()
        {
            Log.Debug("Started the Forcefull Shutdown Timer");
            timer.Start();
        }

        private void TimerElapsed(object sender, EventArgs e)
        {
            elapsedCount++;

            // The first time we ask "nicely" to stop.
            if (elapsedCount == 1)
            {
                Log.Warn("Chummer took more than {Interval} to fully close calling Application.Exit", timer.Interval);
                Application.Exit();
                return;
            }

            // Now we just kill the process completely
            Log.Error("Chummer took more than {Interval} seconds to fully close and will be killed now", 2*timer.Interval);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        /// <summary>
        /// This stops the forceful shutdown of chummer
        /// </summary>
        public void SignalShutdown()
        {
            Log.Info("Stopped the forced shutdown", timer.Interval);
            timer.Stop();
        }
    }
}
