using LPR381.Input_File_Handler;
using LPR381.Output_File_Handler;
using LPR381.Sensitivity_Analysis;
using LPR381.Solving;
using LPR381.Stored_Info;
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
        // Set by btnSenSolve_Click, consumed by btnSenRange_Click / btnSenShadowPrice_Click
        private LpModel currentModel;
        private SolverResult currentResult;
        private AnalyzeSensitivity analyzer;

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
            if (Display.lines == null)
            {
                MessageBox.Show("Load a file first.");
                return;
            }

            try
            {
                currentModel = HandleInput.ParseModel(Display.lines);

                Solver solver = new Solver();
                currentResult = solver.SolvePrimalSimplex(currentModel);

                if (currentResult.SwitchedToDualSimplex)
                {
                    MessageBox.Show(
                        "This problem couldn't start with standard Primal Simplex due to negative RHS values, so Dual Simplex was used automatically to find a feasible starting point.",
                        "Switched to Dual Simplex",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }

                if (currentResult.IsInfeasible)
                {
                    MessageBox.Show("This problem is infeasible.", "Infeasible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (currentResult.IsUnbounded)
                {
                    MessageBox.Show("This problem is unbounded.", "Unbounded", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Sensitivity analysis operations below are performed against this model/result pair
                analyzer = new AnalyzeSensitivity
                {
                    OriginalModel = currentModel,
                    SolvedResult = currentResult
                };

                Display.PopulateFullHistory(currentResult, currentModel, dgwSenDisplay);
                WriteOutputFile.WriteResultToFile(currentResult, Display.GetOutputFilePath());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while solving the problem: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSenChooseFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog1.FileName;
                Display.lines = HandleInput.ReadModelFile(filePath);
                Display.showUserInput(Display.lines, dgwSenDisplay);

                // A newly loaded file invalidates any previously solved model
                currentModel = null;
                currentResult = null;
                analyzer = null;
            }

        }

        private void btnSenShadowPrice_Click(object sender, EventArgs e)
        {
            if (!EnsureSolved())
            {
                return;
            }

            try
            {
                double[] shadowPrices = analyzer.GetShadowPrices();

                var sb = new StringBuilder();
                sb.AppendLine("Shadow Prices:");
                for (int i = 0; i < shadowPrices.Length; i++)
                {
                    sb.AppendLine($"c{i + 1}: {FormatBound(shadowPrices[i])}");
                }

                MessageBox.Show(sb.ToString(), "Shadow Prices", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while calculating shadow prices: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSenRange_Click(object sender, EventArgs e)
        {
            if (!EnsureSolved())
            {
                return;
            }

            string input = ShowInputDialog(
                "Enter what to find the range for:\n- A decision variable, e.g. x1\n- A constraint RHS, e.g. c1",
                "Find Range");

            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            input = input.Trim().ToLower();

            try
            {
                Range range;
                string label;

                if (input.StartsWith("x"))
                {
                    if (!int.TryParse(input.Substring(1), out int varNumber) ||
                        varNumber < 1 || varNumber > currentModel.DecisionVariableCount)
                    {
                        MessageBox.Show($"Enter a variable between x1 and x{currentModel.DecisionVariableCount}.",
                            "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    int variableIndex = varNumber - 1;
                    label = "x" + varNumber;

                    bool isBasic = currentResult.FinalTableau.BasicVariables.Contains(label);
                    range = isBasic
                        ? analyzer.GetBasicVariableRange(variableIndex)
                        : analyzer.GetNonBasicVariableRange(variableIndex);
                }
                else if (input.StartsWith("c"))
                {
                    if (!int.TryParse(input.Substring(1), out int constraintNumber) ||
                        constraintNumber < 1 || constraintNumber > currentModel.Constraints.Count)
                    {
                        MessageBox.Show($"Enter a constraint between c1 and c{currentModel.Constraints.Count}.",
                            "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    label = "c" + constraintNumber + " RHS";
                    range = analyzer.GetRHSRange(constraintNumber - 1);
                }
                else
                {
                    MessageBox.Show("Enter a variable (e.g. x1) or a constraint (e.g. c1).",
                        "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show(
                    $"Range for {label}:\nLower Bound: {FormatBound(range.LowerBound)}\nUpper Bound: {FormatBound(range.UpperBound)}",
                    "Range Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while calculating the range: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool EnsureSolved()
        {
            if (currentModel == null || currentResult == null || analyzer == null)
            {
                MessageBox.Show("Solve the problem first (Sensitivity Analysis Solve).", "Not Solved",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!currentResult.IsOptimal)
            {
                MessageBox.Show("Sensitivity analysis requires an optimal solution.", "Not Optimal",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private static string FormatBound(double value)
        {
            if (double.IsNegativeInfinity(value)) return "-Infinity";
            if (double.IsPositiveInfinity(value)) return "Infinity";
            return Math.Round(value, 3).ToString();
        }

        // Small reusable text-input prompt — the form has no textboxes for picking
        // a variable/constraint, so this stands in for one.
        private static string ShowInputDialog(string text, string caption)
        {
            using (Form prompt = new Form())
            {
                prompt.Width = 420;
                prompt.Height = 160;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.Text = caption;
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.MaximizeBox = false;
                prompt.MinimizeBox = false;

                Label textLabel = new Label() { Left = 10, Top = 10, Width = 385, Height = 60, Text = text };
                TextBox textBox = new TextBox() { Left = 10, Top = 75, Width = 385 };
                Button confirmation = new Button() { Text = "OK", Left = 230, Width = 80, Top = 105, DialogResult = DialogResult.OK };
                Button cancel = new Button() { Text = "Cancel", Left = 315, Width = 80, Top = 105, DialogResult = DialogResult.Cancel };

                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(cancel);
                prompt.AcceptButton = confirmation;
                prompt.CancelButton = cancel;

                return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : null;
            }
        }
    }
}
