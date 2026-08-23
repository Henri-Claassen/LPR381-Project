using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LPR381
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Last line of defense: catch anything a button handler's own try/catch
            // missed so the app shows a message instead of crashing outright.
            Application.ThreadException += (sender, e) => ShowUnhandledException(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => ShowUnhandledException(e.ExceptionObject as Exception);

            // The Cutting Plane unit test passed successfully. Removed the startup popup.
            // try { Solving.CuttingPlaneTest.RunTest(); } catch (Exception ex) { MessageBox.Show(ex.Message, "Test Failed"); }

            // Application.Run(new FormMain_Menu()); // Ensure the form opens if it exists, otherwise comment it out if it fails to compile

            Application.Run(new FormMain_Menu());
        }

        private static void ShowUnhandledException(Exception ex)
        {
            MessageBox.Show(
                ex != null ? ex.Message : "An unknown error occurred.",
                "Unexpected Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
