using LPR381.Input_File_Handler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LPR381.UserDisplay;

namespace LPR381
{
    public partial class FormNL : Form
    {
        public FormNL()
        {
            InitializeComponent();
        }

        private void btnNLMM_Click(object sender, EventArgs e)
        {
            FormMain_Menu main_Menu = new FormMain_Menu();
            main_Menu.Show();
            this.Close();
        }

        private void btnChooseFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {   
                string filePath = openFileDialog1.FileName;
                Display.lines = HandleInput.ReadModelFile(filePath);
                Display.showUserInput(Display.lines, dgvNLDisplay);
            }
        }

        private void btnNLExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnNLSolve_Click(object sender, EventArgs e)
        {
            //Code for solving the nonlinear programming problem will go here
        }
    }
}
