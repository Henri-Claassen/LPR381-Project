using LPR381.Input_File_Handler;
using LPR381.Output_File_Handler;
using LPR381.Solving;
using LPR381.Stored_Info;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LPR381.UserDisplay;


namespace LPR381
{
    public partial class Form1 : Form
    {
        //Variables to use throughout for algorithms etc

        public Form1()
        {
            InitializeComponent();
            dgwMainDisplay.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void btnChooseFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog1.FileName;
                Display.lines = HandleInput.ReadModelFile(filePath);
                Display.showUserInput(Display.lines, dgwMainDisplay);
            }
        }

        private void btnSimplex_Click(object sender, EventArgs e)
        {
            if (Display.lines == null)
            {
                MessageBox.Show("Load a file first.");
                return;
            }
            LpModel model = HandleInput.ParseModel(Display.lines);
            Solver solver = new Solver();
            SolverResult result = solver.SolvePrimalSimplex(model);

            Display.PopulateFullHistory(result, model, dgwMainDisplay);
            WriteOutputFile.WriteResultToFile(result, Display.GetOutputFilePath());
        }

        private void btnExit1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnMainF1_Click(object sender, EventArgs e)
        {
            FormMain_Menu main_Menu = new FormMain_Menu();
            main_Menu.Show();
            this.Close();
        }
    }
}
