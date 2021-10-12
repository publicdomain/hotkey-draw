
using System;
using System.Threading;
using System.Windows.Forms;

namespace HotkeyDraw
{
    /// <summary>
    /// Class with program entry point.
    /// </summary>
    internal sealed class Program
    {
        static Mutex mutex = new Mutex(false, "HotkeyDraw");

        /// <summary>
        /// Program entry point.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            if (!mutex.WaitOne(TimeSpan.Zero, false))
            {
                MessageBox.Show("HotkeyDraw already started!", "Single instance", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

    }
}
