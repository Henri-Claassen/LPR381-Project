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
            dgwMainDisplay.CellBeginEdit += dgwMainDisplay_CellBeginEdit;
            dgwMainDisplay.CellEndEdit += dgwMainDisplay_CellEndEdit;
        }

        private void dgwMainDisplay_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            var row = dgwMainDisplay.Rows[e.RowIndex];

            bool isEditableDataRow = row.Tag as string == "data";
            bool isLabelColumn = e.ColumnIndex == 0;

            if(!isEditableDataRow || isLabelColumn)
            {
                e.Cancel = true;
            }
        }

        private void dgwMainDisplay_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var cell = dgwMainDisplay.Rows[e.RowIndex].Cells[e.ColumnIndex];
            string newValue = cell.Value?.ToString();

            if (!double.TryParse(newValue, out double parsedValue))
            {
                MessageBox.Show("Please enter a valid number.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cell.Value = "0";
                return;
            }
        }

        private void btnChooseFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string filePath = openFileDialog1.FileName;
                    Display.lines = HandleInput.ReadModelFile(filePath);
                    Display.showUserInput(Display.lines, dgwMainDisplay);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not read the file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSimplex_Click(object sender, EventArgs e)
        {
            if (Display.lines == null)
            {
                MessageBox.Show("Load a file first.");
                return;
            }

            try
            {
                LpModel model = HandleInput.ParseModel(Display.lines);
                Solver solver = new Solver();
                SolverResult result = solver.SolvePrimalSimplex(model);

                if (result.SwitchedToDualSimplex)
                {
                    MessageBox.Show(
                        "This problem couldn't start with standard Primal Simplex due to negative RHS values, so Dual Simplex was used automatically to find a feasible starting point.",
                        "Switched to Dual Simplex",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }

                Display.PopulateFullHistory(result, model, dgwMainDisplay);
                WriteOutputFile.WriteResultToFile(result, Display.GetOutputFilePath());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while solving the problem: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCuttingPlane_Click(object sender, EventArgs e)
        {
            if (Display.lines == null)
            {
                MessageBox.Show("Load a file first.");
                return;
            }

            try
            {
                LpModel model = HandleInput.ParseModel(Display.lines);
                Solver solver = new Solver();
                SolverResult result = solver.SolveCuttingPlane(model);

                Display.PopulateFullHistory(result, model, dgwMainDisplay);
                WriteOutputFile.WriteResultToFile(result, Display.GetOutputFilePath());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while solving the problem: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExit1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void btnMainF1_Click(object sender, EventArgs e)
        {
            FormMain_Menu main_Menu = new FormMain_Menu();
            main_Menu.Show();
            this.Close();
        }

        private void btnF1CanonicalForm_Click(object sender, EventArgs e)
        {
            if (Display.lines == null)
            {
                MessageBox.Show("Load a file first.");
                return;
            }

            try
            {
                LpModel model = HandleInput.ParseModel(Display.lines);
                Solver solver = new Solver();
                Tableau table = solver.BuildCanonicalForm(model);

                canonicalTableau = table;
                canonicalModel = model;

                Display.populateMainDisplay(table, dgwMainDisplay);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while building the canonical form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBranchAndBound_Click(object sender, EventArgs e)
        {
            if (Display.lines == null)
            {
                MessageBox.Show("Load a file first.");
                return;
            }
            try
            {
                LpModel model = HandleInput.ParseModel(Display.lines);
                Solver solver = new Solver();
                SolverResult result = solver.SolveBranchAndBound(model);

                if (result.IsInfeasible)
                {
                    MessageBox.Show("This problem's LP relaxation is infeasible.", "Infeasible",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (result.IsUnbounded)
                {
                    MessageBox.Show("This problem's LP relaxation is unbounded.", "Unbounded",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (!result.IsOptimal)
                {
                    MessageBox.Show("No integer-feasible solution was found.", "No Solution",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                Display.PopulateFullHistory(result, model, dgwMainDisplay);
                WriteOutputFile.WriteResultToFile(result, Display.GetOutputFilePath());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while solving the problem: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
        }

        private void btnKnapsack_Click(object sender, EventArgs e)
        {
            if (Display.lines == null)
            {
                MessageBox.Show("Load a file first.");
                return;
            }
            try
            {
                LpModel model = HandleInput.ParseModel(Display.lines);
                Solver solver = new Solver();

                if (!solver.IsKnapsackModel(model))
                {
                    MessageBox.Show(
                        "This model isn't a valid 0/1 knapsack problem. It must be a maximization with exactly one <= constraint and all binary variables.",
                        "Invalid Model", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SolverResult result = solver.SolveKnapsackBranchAndBound(model);

                Display.PopulateFullHistory(result, model, dgwMainDisplay);
                WriteOutputFile.WriteResultToFile(result, Display.GetOutputFilePath());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while solving the knapsack problem: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
        }

        private Tableau canonicalTableau;
        private LpModel canonicalModel;

        private void btnNewPivot_Click(object sender, EventArgs e)
        {
            if (canonicalTableau == null)
            {
                MessageBox.Show("Generate the Canonical Form first.");
                return;
            }

            try
            {
                Tableau editedTable = ReadTableauFromGrid(canonicalTableau, dgwMainDisplay);

                Solver solver = new Solver();
                SolverResult result = solver.SolveFromEditedTableau(editedTable, canonicalModel.DecisionVariableCount);

                Display.PopulateFullHistory(result, canonicalModel, dgwMainDisplay);
                WriteOutputFile.WriteResultToFile(result, Display.GetOutputFilePath());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Tableau ReadTableauFromGrid(Tableau template, DataGridView grid)
        {
            var table = new Tableau
            {
                TableNumber = "t-i",
                IsMaximization = template.IsMaximization,
                ColumnNames = new List<string>(template.ColumnNames),
                RowNames = new List<string>(template.RowNames),
                BasicVariables = new List<string>(template.BasicVariables)
            };

            for (int i = 0; i < template.Rows.Count; i++)
            {
                var row = new List<double>();
                for (int j = 0; j < template.Rows[i].Count; j++)
                {
                    string cellValue = grid.Rows[i].Cells[j + 1].Value?.ToString();
                    if (!double.TryParse(cellValue, out double parsed))
                        throw new InvalidOperationException($"Invalid number in row {i + 1}, column {j + 1}.");
                    row.Add(parsed);
                }
                table.Rows.Add(row);
            }

            return table;
        }
    }
}
