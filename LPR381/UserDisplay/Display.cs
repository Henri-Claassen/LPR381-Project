using LPR381.Output_File_Handler;
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
            for (int i = 0; i < table.Rows.Count; i++)//tags the cells in the program as data so it can be edited
            {
                int rowIndex = dataGrid.Rows.Add();
                dataGrid.Rows[rowIndex].Cells["TableNumber"].Value = table.RowNames[i];
                dataGrid.Rows[rowIndex].Tag = "data";

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
            if (result.AllNodes != null && result.AllNodes.Count > 0)
            {
                PopulateBranchAndBoundHistory(result, model, dataGrid);
                return;
            }

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

        #region PopulateBranchAndBoundHistory
        private static void PopulateBranchAndBoundHistory(SolverResult result, LpModel model, DataGridView dataGrid)
        {
            dataGrid.Rows.Clear();
            dataGrid.Columns.Clear();

            int maxCols = 1;
            foreach (var node in result.AllNodes)
            {
                if (node.KnapsackTable != null)
                    maxCols = Math.Max(maxCols, 8);
                if (node.SubProblemResult?.IterationHistory != null && node.SubProblemResult.IterationHistory.Count > 0)
                    maxCols = Math.Max(maxCols, node.SubProblemResult.IterationHistory.Max(t => t.ColumnNames.Count) + 1);
            }
            for (int c = 0; c < maxCols; c++)
                dataGrid.Columns.Add("col" + c, "");

            var (labels, childrenByParent) = WriteOutputFile.BuildTree(result.AllNodes);
            var root = result.AllNodes.FirstOrDefault(n => n.Parent == null);
            if (root != null)
            {
                WriteNodeBlockRecursive(dataGrid, root, labels, childrenByParent);
            }

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
                int statusRow = dataGrid.Rows.Add();
                string statusText = result.IsInfeasible ? "INFEASIBLE — no solution exists" : "UNBOUNDED — no optimal solution exists";
                dataGrid.Rows[statusRow].Cells[0].Value = statusText;
                dataGrid.Rows[statusRow].DefaultCellStyle.ForeColor = Color.Red;
                dataGrid.Rows[statusRow].DefaultCellStyle.Font = new Font(dataGrid.Font, FontStyle.Bold);
            }
        }

        private static void WriteNodeBlockRecursive(DataGridView dataGrid, BranchNode node,
            Dictionary<BranchNode, string> labels, Dictionary<BranchNode, List<BranchNode>> childrenByParent)
        {
            int nodeHeaderRow = dataGrid.Rows.Add();
            dataGrid.Rows[nodeHeaderRow].Cells[0].Value =
                $"Node {labels[node]}: {node.BranchDescription}" + (node.IsFathomed ? $"  [Fathomed: {node.FathomReason}]" : "");
            dataGrid.Rows[nodeHeaderRow].DefaultCellStyle.BackColor = Color.Khaki;
            dataGrid.Rows[nodeHeaderRow].DefaultCellStyle.Font = new Font(dataGrid.Font, FontStyle.Bold);
            dataGrid.Rows[nodeHeaderRow].Tag = "nodeheader";

            if (node.KnapsackTable != null)
            {
                if (node.FixedIn != null && node.FixedIn.Count > 0)
                {
                    int r = dataGrid.Rows.Add();
                    dataGrid.Rows[r].Cells[0].Value = "Fixed IN (=1): " + string.Join(", ", node.FixedIn.OrderBy(i => i).Select(i => "x" + (i + 1)));
                }
                if (node.FixedOut != null && node.FixedOut.Count > 0)
                {
                    int r = dataGrid.Rows.Add();
                    dataGrid.Rows[r].Cells[0].Value = "Fixed OUT (=0): " + string.Join(", ", node.FixedOut.OrderBy(i => i).Select(i => "x" + (i + 1)));
                }

                int headerRowIndex = dataGrid.Rows.Add();
                string[] headers = { "Item", "Value", "Weight", "Ratio", "Decision", "RemCap", "Status" };
                dataGrid.Rows[headerRowIndex].Cells[0].Value = "Capacity: " + Math.Round(node.KnapsackTable.Capacity, 3);
                for (int j = 0; j < headers.Length; j++)
                    dataGrid.Rows[headerRowIndex].Cells[j + 1].Value = headers[j];
                dataGrid.Rows[headerRowIndex].DefaultCellStyle.BackColor = Color.LightGray;
                dataGrid.Rows[headerRowIndex].DefaultCellStyle.Font = new Font(dataGrid.Font, FontStyle.Bold);
                dataGrid.Rows[headerRowIndex].Tag = "header";

                foreach (var row in node.KnapsackTable.Rows)
                {
                    int rowIndex = dataGrid.Rows.Add();
                    dataGrid.Rows[rowIndex].Cells[1].Value = row.ItemName;
                    dataGrid.Rows[rowIndex].Cells[2].Value = Math.Round(row.Value, 3).ToString("G7");
                    dataGrid.Rows[rowIndex].Cells[3].Value = Math.Round(row.Weight, 3).ToString("G7");
                    dataGrid.Rows[rowIndex].Cells[4].Value = double.IsPositiveInfinity(row.Ratio) ? "Inf" : Math.Round(row.Ratio, 3).ToString("G7");
                    dataGrid.Rows[rowIndex].Cells[5].Value = Math.Round(row.Decision, 3).ToString("G7");
                    dataGrid.Rows[rowIndex].Cells[6].Value = Math.Round(row.RemainingCapacityAfter, 3).ToString("G7");
                    dataGrid.Rows[rowIndex].Cells[7].Value = row.Status;
                    dataGrid.Rows[rowIndex].Tag = "data";
                }

                int objRow = dataGrid.Rows.Add();
                dataGrid.Rows[objRow].Cells[0].Value = "Objective: " + Math.Round(node.KnapsackTable.ObjectiveValue, 3);
                if (node.KnapsackTable.BranchVariableIndex != -1)
                    dataGrid.Rows[objRow].Cells[1].Value = "Branch var: x" + (node.KnapsackTable.BranchVariableIndex + 1);
            }
            else if (node.SubProblemResult?.IterationHistory != null)
            {
                foreach (var table in node.SubProblemResult.IterationHistory)
                {
                    int rhsCol = table.ColumnNames.Count - 1;

                    int headerRowIndex = dataGrid.Rows.Add();
                    dataGrid.Rows[headerRowIndex].Cells[0].Value = table.TableNumber;
                    for (int j = 0; j < table.ColumnNames.Count; j++)
                        dataGrid.Rows[headerRowIndex].Cells[j + 1].Value = table.ColumnNames[j];
                    dataGrid.Rows[headerRowIndex].DefaultCellStyle.BackColor = Color.LightGray;
                    dataGrid.Rows[headerRowIndex].DefaultCellStyle.Font = new Font(dataGrid.Font, FontStyle.Bold);
                    dataGrid.Rows[headerRowIndex].Tag = "header";

                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        int rowIndex = dataGrid.Rows.Add();
                        dataGrid.Rows[rowIndex].Cells[0].Value = table.RowNames[i];
                        for (int j = 0; j < table.Rows[i].Count; j++)
                            dataGrid.Rows[rowIndex].Cells[j + 1].Value = Math.Round(table.Rows[i][j], 3).ToString("G7");
                        dataGrid.Rows[rowIndex].Tag = "data";
                    }

                    int bvRowIndex = dataGrid.Rows.Add();
                    dataGrid.Rows[bvRowIndex].Cells[0].Value = "BV";
                    for (int i = 0; i < table.BasicVariables.Count; i++)
                        dataGrid.Rows[bvRowIndex].Cells[i + 1].Value = table.BasicVariables[i];
                    dataGrid.Rows[bvRowIndex].DefaultCellStyle.BackColor = Color.LightBlue;
                    dataGrid.Rows[bvRowIndex].Tag = "bv";

                    int bvValRowIndex = dataGrid.Rows.Add();
                    dataGrid.Rows[bvValRowIndex].Cells[0].Value = "BV Values";
                    for (int i = 0; i < table.Rows.Count; i++)
                        dataGrid.Rows[bvValRowIndex].Cells[i + 1].Value = Math.Round(table.Rows[i][rhsCol], 3).ToString("G7");
                    dataGrid.Rows[bvValRowIndex].DefaultCellStyle.BackColor = Color.LightCyan;
                    dataGrid.Rows[bvValRowIndex].Tag = "bvvalues";

                    dataGrid.Rows.Add();
                }
            }
            else
            {
                int r = dataGrid.Rows.Add();
                dataGrid.Rows[r].Cells[0].Value = "(no subproblem data — fathomed before solving)";
                dataGrid.Rows[r].DefaultCellStyle.ForeColor = Color.Gray;
            }

            dataGrid.Rows.Add();

            if (childrenByParent.TryGetValue(node, out var children))
                foreach (var child in children)
                    WriteNodeBlockRecursive(dataGrid, child, labels, childrenByParent);
        }
        #endregion
    }
}
