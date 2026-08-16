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
            dgvNLDisplay.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvNLDisplay.CellBeginEdit += dgvNLDisplay_CellBeginEdit;
            dgvNLDisplay.CellEndEdit += dgvNLDisplay_CellEndEdit;

        }

        private void dgvNLDisplay_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            var row = dgvNLDisplay.Rows[e.RowIndex];

            bool isEditableDataRow = row.Tag as string == "data";
            bool isLabelColumn = e.ColumnIndex == 0;

            if (!isEditableDataRow || isLabelColumn)
            {
                e.Cancel = true;
            }
        }

        private void dgvNLDisplay_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var cell = dgvNLDisplay.Rows[e.RowIndex].Cells[e.ColumnIndex];
            string newValue = cell.Value?.ToString();

            if (!double.TryParse(newValue, out double parsedValue))
            {
                MessageBox.Show("Please enter a valid number.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cell.Value = "0";
                return;
            }
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
            Environment.Exit(0);
        }

        private void btnNLSolve_Click(object sender, EventArgs e)
        {
            //Code for solving the nonlinear programming problem will go here
        }

        private void btnNLShadowPrice_Click(object sender, EventArgs e)
        {
            //Shadow Price calculation for nonlinear programming will be put here
        }
    }
}
