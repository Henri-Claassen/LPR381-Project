using LPR381.Solving;
using LPR381.Stored_Info;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LPR381.UserDisplay
{
    internal class Display
    {
        public static string[] lines;

        #region ShowUserInput
        //Method to show the raw values from the text file
        public static void showUserInput(string[] lines, DataGridView  dataGrid)
        {
            string[][] data = new string[lines.Length][];
            for (int i = 0; i < lines.Length; i++)
            {
                data[i] = lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
            dataGrid.Rows.Clear();
            dataGrid.Columns.Clear();

            int maxColumns = 0;
            foreach (string[] row in data)
            {
                if (row.Length > maxColumns)
                    maxColumns = row.Length;
            }
            for (int i = 0; i < maxColumns; i++)
            {
                dataGrid.Columns.Add("col" + i, "col" + i);
            }
            foreach (string[] row in data)
            {
                dataGrid.Rows.Add(row);
            }
        }
        #endregion

        #region populateMainDisplay
        //Method to show the canonical form to the user
        public static void populateMainDisplay(Tableau table, DataGridView dataGrid)
        {
            dataGrid.Rows.Clear();
            dataGrid.Columns.Clear();
            dataGrid.Columns.Add("TableNumber", table.TableNumber);
            foreach (var col in table.ColumnNames)
            {
                dataGrid.Columns.Add(col, col);
            }
            for (int i = 0; i < table.Rows.Count; i++)
            {
                int rowIndex = dataGrid.Rows.Add();
                dataGrid.Rows[rowIndex].Cells["TableNumber"].Value = table.RowNames[i];

                for (int j = 0; j < table.Rows[i].Count; j++)
                {
                    dataGrid.Rows[rowIndex].Cells[j + 1].Value = table.Rows[i][j];   // +1 to skip past the RowLabel column
                }
            }
        }
        #endregion

        #region PopulateFullHistory
        public static void PopulateFullHistory(SolverResult result, LpModel model, DataGridView dataGrid)
        {
            var history = result.IterationHistory;
            dataGrid.Rows.Clear();
            dataGrid.Columns.Clear();

            int maxCols = history.Max(t => t.ColumnNames.Count) + 1;
            for (int c = 0; c < maxCols; c++)
                dataGrid.Columns.Add("col" + c, "");

            foreach (var table in history)
            {
                int rhsCol = table.ColumnNames.Count - 1;

                // Header row: TableNumber + column names
                int headerRowIndex = dataGrid.Rows.Add();
                dataGrid.Rows[headerRowIndex].Cells[0].Value = table.TableNumber;
                for (int j = 0; j < table.ColumnNames.Count; j++)
                    dataGrid.Rows[headerRowIndex].Cells[j + 1].Value = table.ColumnNames[j];

                dataGrid.Rows[headerRowIndex].DefaultCellStyle.BackColor = Color.LightGray;
                dataGrid.Rows[headerRowIndex].DefaultCellStyle.Font = new Font(dataGrid.Font, FontStyle.Bold);
                dataGrid.Rows[headerRowIndex].Tag = "header";

                // Data rows
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    int rowIndex = dataGrid.Rows.Add();
                    dataGrid.Rows[rowIndex].Cells[0].Value = table.RowNames[i];
                    for (int j = 0; j < table.Rows[i].Count; j++)
                        dataGrid.Rows[rowIndex].Cells[j + 1].Value = Math.Round(table.Rows[i][j], 3).ToString("G7");
                    dataGrid.Rows[rowIndex].Tag = "data";
                }

                // BV row (names)
                int bvRowIndex = dataGrid.Rows.Add();
                dataGrid.Rows[bvRowIndex].Cells[0].Value = "BV";
                for (int i = 0; i < table.BasicVariables.Count; i++)
                    dataGrid.Rows[bvRowIndex].Cells[i + 1].Value = table.BasicVariables[i];

                dataGrid.Rows[bvRowIndex].DefaultCellStyle.BackColor = Color.LightBlue;
                dataGrid.Rows[bvRowIndex].Tag = "bv";

                // BV Values row — includes Z's value, aligned under the correct BV
                int bvValRowIndex = dataGrid.Rows.Add();
                dataGrid.Rows[bvValRowIndex].Cells[0].Value = "BV Values";
                for (int i = 0; i < table.Rows.Count; i++)
                    dataGrid.Rows[bvValRowIndex].Cells[i + 1].Value = Math.Round(table.Rows[i][rhsCol], 3).ToString("G7");

                dataGrid.Rows[bvValRowIndex].DefaultCellStyle.BackColor = Color.LightCyan;
                dataGrid.Rows[bvValRowIndex].Tag = "bvvalues";

                // Spacer row between tables
                dataGrid.Rows.Add();
            }

            // Show history and final z-value
            if (result.IsOptimal)
            {
                int summaryHeaderRow = dataGrid.Rows.Add();
                dataGrid.Rows[summaryHeaderRow].Cells[0].Value = "Final Values";
                dataGrid.Rows[summaryHeaderRow].Cells[1].Value = "Z";

                int summaryRow = dataGrid.Rows.Add();
                dataGrid.Rows[summaryRow].Cells[1].Value = Math.Round(result.ObjectiveValue, 3).ToString("G7");

                for (int j = 0; j < model.DecisionVariableCount; j++)
                {
                    dataGrid.Rows[summaryHeaderRow].Cells[j + 2].Value = "x" + (j + 1);
                    dataGrid.Rows[summaryRow].Cells[j + 2].Value = Math.Round(result.VariableValues[j], 3).ToString("G7");
                }

                dataGrid.Rows[summaryHeaderRow].DefaultCellStyle.BackColor = Color.LightGreen;
                dataGrid.Rows[summaryRow].DefaultCellStyle.BackColor = Color.LightGreen;
                dataGrid.Rows[summaryHeaderRow].DefaultCellStyle.Font = new Font(dataGrid.Font, FontStyle.Bold);
            }
            else
            {
                // Show red for infeasible or unbounded
                int statusRow = dataGrid.Rows.Add();
                string statusText = result.IsInfeasible ? "INFEASIBLE — no solution exists" : "UNBOUNDED — no optimal solution exists";
                dataGrid.Rows[statusRow].Cells[0].Value = statusText;

                dataGrid.Rows[statusRow].DefaultCellStyle.ForeColor = Color.Red;
                dataGrid.Rows[statusRow].DefaultCellStyle.Font = new Font(dataGrid.Font, FontStyle.Bold);
            }
        }
        #endregion

        #region GetOutputFilePath
        public static string GetOutputFilePath()
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string downloadsFolder = Path.Combine(userProfile, "Downloads");

            // Ensure it exists (should always exist on Windows, but cheap safety check)
            if (!Directory.Exists(downloadsFolder))
                Directory.CreateDirectory(downloadsFolder);

            string fileName = "LP_Output_" + DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss") + ".txt";
            return Path.Combine(downloadsFolder, fileName);
        }
        #endregion


    }
}
