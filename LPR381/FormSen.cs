using LPR381.Input_File_Handler;
using LPR381.UserDisplay;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LPR381
{
    public partial class FormSen : Form
    {
        public FormSen()
        {
            InitializeComponent();
            dgwSenDisplay.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgwSenDisplay.CellBeginEdit += dgwSenDisplay_CellBeginEdit;
            dgwSenDisplay.CellEndEdit += dgwSenDisplay_CellEndEdit;
        }

        private void dgwSenDisplay_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            var row = dgwSenDisplay.Rows[e.RowIndex];

            bool isEditableDataRow = row.Tag as string == "data";
            bool isLabelColumn = e.ColumnIndex == 0;

            if (!isEditableDataRow || isLabelColumn)
            {
                e.Cancel = true;
            }
        }

        private void dgwSenDisplay_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var cell = dgwSenDisplay.Rows[e.RowIndex].Cells[e.ColumnIndex];
            string newValue = cell.Value?.ToString();

            if (!double.TryParse(newValue, out double parsedValue))
            {
                MessageBox.Show("Please enter a valid number.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cell.Value = "0";
                return;
            }
        }

        private void btnSenMM_Click(object sender, EventArgs e)
        {
            FormMain_Menu main = new FormMain_Menu();
            main.Show();
            this.Close();
        }

        private void btnSenExit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void btnSenSolve_Click(object sender, EventArgs e)
        {
            //Code for solving the sensitivity analysis problem will go here
        }

        private void btnSenChooseFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog1.FileName;
                Display.lines = HandleInput.ReadModelFile(filePath);
                Display.showUserInput(Display.lines, dgwSenDisplay);
            }

        }

        private void btnSenShadowPrice_Click(object sender, EventArgs e)
        {
            //Code for Shadow Price calculation will go here
        }

        private void btnSenRange_Click(object sender, EventArgs e)
        {

        }
    }
}
