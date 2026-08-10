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

namespace LPR381
{
    public partial class Form1 : Form
    {
        //Variables to use throughout for algorithms etc
        string[] lines;

        public Form1()
        {
            InitializeComponent();
            dgwMainDisplay.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        #region ShowUserInput
        //Method to show the raw values from the text file
        private void showUserInput(string[] lines)
        {
            string[][] data = new string[lines.Length][];
            for (int i = 0; i < lines.Length; i++)
            {
                data[i] = lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
            dgwMainDisplay.Rows.Clear();
            dgwMainDisplay.Columns.Clear();

            int maxColumns = 0;
            foreach (string[] row in data)
            {
                if (row.Length > maxColumns)
                    maxColumns = row.Length;
            }
            for (int i = 0; i < maxColumns; i++)
            {
                dgwMainDisplay.Columns.Add("col" + i, "col" + i);
            }
            foreach (string[] row in data)
            {
                dgwMainDisplay.Rows.Add(row);
            }
        }
        #endregion

        #region populateMainDisplay
        //Method to show the canonical form to the user
        private void populateMainDisplay(Tableau table)
        {
            dgwMainDisplay.Rows.Clear();
            dgwMainDisplay.Columns.Clear();
            dgwMainDisplay.Columns.Add("TableNumber",table.TableNumber);
            foreach (var col in table.ColumnNames)
            {
                dgwMainDisplay.Columns.Add(col,col);
            }
            for (int i = 0; i < table.Rows.Count; i++)
            {
                int rowIndex = dgwMainDisplay.Rows.Add();
                dgwMainDisplay.Rows[rowIndex].Cells["TableNumber"].Value = table.RowNames[i];

                for (int j = 0; j < table.Rows[i].Count; j++)
                {
                    dgwMainDisplay.Rows[rowIndex].Cells[j + 1].Value = table.Rows[i][j];   // +1 to skip past the RowLabel column
                }
            }
        }
        #endregion

        #region PopulateFullHistory
        private void PopulateFullHistory(SolverResult result, LpModel model)
        {
            var history = result.IterationHistory;
            dgwMainDisplay.Rows.Clear();
            dgwMainDisplay.Columns.Clear();

            int maxCols = history.Max(t => t.ColumnNames.Count) + 1;
            for (int c = 0; c < maxCols; c++)
                dgwMainDisplay.Columns.Add("col" + c, "");

            foreach (var table in history)
            {
                int rhsCol = table.ColumnNames.Count - 1;

                // Header row: TableNumber + column names
                int headerRowIndex = dgwMainDisplay.Rows.Add();
                dgwMainDisplay.Rows[headerRowIndex].Cells[0].Value = table.TableNumber;
                for (int j = 0; j < table.ColumnNames.Count; j++)
                    dgwMainDisplay.Rows[headerRowIndex].Cells[j + 1].Value = table.ColumnNames[j];

                dgwMainDisplay.Rows[headerRowIndex].DefaultCellStyle.BackColor = Color.LightGray;
                dgwMainDisplay.Rows[headerRowIndex].DefaultCellStyle.Font = new Font(dgwMainDisplay.Font, FontStyle.Bold);

                // Data rows
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    int rowIndex = dgwMainDisplay.Rows.Add();
                    dgwMainDisplay.Rows[rowIndex].Cells[0].Value = table.RowNames[i];
                    for (int j = 0; j < table.Rows[i].Count; j++)
                        dgwMainDisplay.Rows[rowIndex].Cells[j + 1].Value = Math.Round(table.Rows[i][j], 3).ToString("G7");
                }

                // BV row (names)
                int bvRowIndex = dgwMainDisplay.Rows.Add();
                dgwMainDisplay.Rows[bvRowIndex].Cells[0].Value = "BV";
                for (int i = 0; i < table.BasicVariables.Count; i++)
                    dgwMainDisplay.Rows[bvRowIndex].Cells[i + 1].Value = table.BasicVariables[i];

                dgwMainDisplay.Rows[bvRowIndex].DefaultCellStyle.BackColor = Color.LightBlue;

                // BV Values row — includes Z's value, aligned under the correct BV
                int bvValRowIndex = dgwMainDisplay.Rows.Add();
                dgwMainDisplay.Rows[bvValRowIndex].Cells[0].Value = "BV Values";
                for (int i = 0; i < table.Rows.Count; i++)
                    dgwMainDisplay.Rows[bvValRowIndex].Cells[i + 1].Value = Math.Round(table.Rows[i][rhsCol], 3).ToString("G7");

                dgwMainDisplay.Rows[bvValRowIndex].DefaultCellStyle.BackColor = Color.LightCyan;

                // Spacer row between tables
                dgwMainDisplay.Rows.Add();
            }

            // Final summary: Z value + all original decision variables
            int summaryHeaderRow = dgwMainDisplay.Rows.Add();
            dgwMainDisplay.Rows[summaryHeaderRow].Cells[0].Value = "Final Values";
            dgwMainDisplay.Rows[summaryHeaderRow].Cells[1].Value = "Z";   // NEW

            int summaryRow = dgwMainDisplay.Rows.Add();
            dgwMainDisplay.Rows[summaryRow].Cells[1].Value = Math.Round(result.ObjectiveValue, 3).ToString("G7");   // NEW

            for (int j = 0; j < model.DecisionVariableCount; j++)
            {
                dgwMainDisplay.Rows[summaryHeaderRow].Cells[j + 2].Value = "x" + (j + 1);   // shifted +2 to leave room for Z at index 1
                dgwMainDisplay.Rows[summaryRow].Cells[j + 2].Value = Math.Round(result.VariableValues[j], 3).ToString("G7");
            }

            dgwMainDisplay.Rows[summaryHeaderRow].DefaultCellStyle.BackColor = Color.LightGreen;
            dgwMainDisplay.Rows[summaryRow].DefaultCellStyle.BackColor = Color.LightGreen;
            dgwMainDisplay.Rows[summaryHeaderRow].DefaultCellStyle.Font = new Font(dgwMainDisplay.Font, FontStyle.Bold);
        }
        #endregion

        #region GetOutputFilePath
        private string GetOutputFilePath()
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string downloadsFolder = Path.Combine(userProfile, "Downloads");

            // Ensure it exists (should always exist on Windows, but cheap safety check)
            if (!Directory.Exists(downloadsFolder))
                Directory.CreateDirectory(downloadsFolder);

            string fileName = "LP_Output_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
            return Path.Combine(downloadsFolder, fileName);
        }
        #endregion

        private void btnChooseFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog1.FileName;
                lines = HandleInput.ReadModelFile(filePath);
                showUserInput(lines);
            }
        }

        private void btnSimplex_Click(object sender, EventArgs e)
        {
            if (lines == null)
            {
                MessageBox.Show("Load a file first.");
                return;
            }
            LpModel model = HandleInput.ParseModel(lines);
            Solver solver = new Solver();
            SolverResult result = solver.SolvePrimalSimplex(model);

            PopulateFullHistory(result, model);
            WriteOutputFile.WriteResultToFile(result, GetOutputFilePath());
        }
    }
}
