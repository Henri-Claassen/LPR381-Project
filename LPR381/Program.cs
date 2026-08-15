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
            
            // Run Unit Test
            try 
            {
                Solving.CuttingPlaneTest.RunTest();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Test Failed");
            }

            // Application.Run(new FormMain_Menu()); // Ensure the form opens if it exists, otherwise comment it out if it fails to compile

            Application.Run(new FormMain_Menu());
        }
    }
}
