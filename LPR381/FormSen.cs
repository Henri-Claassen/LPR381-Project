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
        // Set by btnSenSolve_Click, consumed by every sensitivity operation below.
        // AnalyzeSensitivity always measures changes against this original solve —
        // "Apply" operations show a what-if result, they don't replace this baseline.
        private LpModel currentModel;
        private SolverResult currentResult;
        private AnalyzeSensitivity analyzer;

        // Selected via clicking a cell in dgwSenDisplay — see dgwSenDisplay_CellClick.
        private int? selectedVariableIndex;
        private int? selectedConstraintIndex;

        // Every applied change / range query after Solve gets appended here.
        private string sensitivityFilePath;

        public FormSen()
        {
            InitializeComponent();
            dgwSenDisplay.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgwSenDisplay.CellBeginEdit += dgwSenDisplay_CellBeginEdit;
            dgwSenDisplay.CellEndEdit += dgwSenDisplay_CellEndEdit;
            dgwSenDisplay.CellClick += dgwSenDisplay_CellClick;
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

        #region Cell-click selection
        // Clicking a cell in the tableau selects which variable/constraint the
        // Range/Apply buttons act on, instead of typing "x1" or "c1" into a prompt.
        // Only "header" and "data" rows are considered — "bv"/"bvvalues" rows position
        // their cells by ROW index (see Display.PopulateFullHistory), not by column,
        // so mapping a click there back to a column would pick the wrong variable.
        private void dgwSenDisplay_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || currentModel == null)
            {
                return;
            }

            var row = dgwSenDisplay.Rows[e.RowIndex];
            string rowTag = row.Tag as string;

            if (rowTag != "header" && rowTag != "data")
            {
                return;
            }

            string columnName = FindColumnNameAbove(e.RowIndex, e.ColumnIndex);
            if (TryParseVariableIndex(columnName, out int varIndex) && varIndex < currentModel.DecisionVariableCount)
            {
                selectedVariableIndex = varIndex;
            }

            if (rowTag == "data")
            {
                string rowLabel = row.Cells[0].Value?.ToString();
                if (TryParseConstraintIndex(rowLabel, out int constraintIndex) && constraintIndex < currentModel.Constraints.Count)
                {
                    selectedConstraintIndex = constraintIndex;
                }
            }

            UpdateSelectionLabel();
        }

        private string FindColumnNameAbove(int rowIndex, int colIndex)
        {
            for (int r = rowIndex; r >= 0; r--)
            {
                if (dgwSenDisplay.Rows[r].Tag as string == "header" && colIndex < dgwSenDisplay.Rows[r].Cells.Count)
                {
                    return dgwSenDisplay.Rows[r].Cells[colIndex].Value?.ToString();
                }
            }
            return null;
        }

        private static bool TryParseVariableIndex(string columnName, out int index)
        {
            index = -1;
            if (string.IsNullOrEmpty(columnName))
            {
                return false;
            }

            string trimmed = columnName.TrimEnd('\'', '"');
            if (trimmed.Length < 2 || trimmed[0] != 'x')
            {
                return false;
            }

            if (!int.TryParse(trimmed.Substring(1), out int n) || n < 1)
            {
                return false;
            }

            index = n - 1;
            return true;
        }

        private static bool TryParseConstraintIndex(string rowLabel, out int index)
        {
            index = -1;
            if (string.IsNullOrEmpty(rowLabel))
            {
                return false;
            }

            string trimmed = rowLabel.TrimEnd('\'', '"');
            if (trimmed.Length < 2 || trimmed[0] != 'c')
            {
                return false;
            }

            if (!int.TryParse(trimmed.Substring(1), out int n) || n < 1)
            {
                return false;
            }

            index = n - 1;
            return true;
        }

        private void UpdateSelectionLabel()
        {
            string varPart = selectedVariableIndex.HasValue ? "x" + (selectedVariableIndex.Value + 1) : "none";
            string cPart = selectedConstraintIndex.HasValue ? "c" + (selectedConstraintIndex.Value + 1) : "none";
            lblSenSelection.Text = $"Selected variable: {varPart}    |    Selected constraint: {cPart}    (click a cell in the tableau to change selection)";
        }
        #endregion

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

        #region Solve
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

                selectedVariableIndex = null;
                selectedConstraintIndex = null;
                UpdateSelectionLabel();

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

                sensitivityFilePath = Display.GetOutputFilePath();
                WriteOutputFile.WriteResultToFile(currentResult, sensitivityFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while solving the problem: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        private void btnSenChooseFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog1.FileName;
                Display.lines = HandleInput.ReadModelFile(filePath);
                Display.showUserInput(Display.lines, dgwSenDisplay);

                // A newly loaded file invalidates any previously solved model/log
                currentModel = null;
                currentResult = null;
                analyzer = null;
                selectedVariableIndex = null;
                selectedConstraintIndex = null;
                sensitivityFilePath = null;
                UpdateSelectionLabel();
            }
        }

        #region Shadow Prices
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
                AppendToSensitivityLog(sb.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while calculating shadow prices: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Variable range / apply change (basic or non-basic, auto-detected)
        private void btnSenRangeVar_Click(object sender, EventArgs e)
        {
            if (!EnsureSolved() || !EnsureVariableSelected())
            {
                return;
            }

            try
            {
                int j = selectedVariableIndex.Value;
                string label = "x" + (j + 1);
                bool isBasic = currentResult.FinalTableau.BasicVariables.Contains(label);
                Range range = isBasic ? analyzer.GetBasicVariableRange(j) : analyzer.GetNonBasicVariableRange(j);

                string message = $"Range for {label} ({(isBasic ? "basic" : "non-basic")}):\nLower Bound: {FormatBound(range.LowerBound)}\nUpper Bound: {FormatBound(range.UpperBound)}";
                MessageBox.Show(message, "Variable Range", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AppendToSensitivityLog(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSenApplyVar_Click(object sender, EventArgs e)
        {
            if (!EnsureSolved() || !EnsureVariableSelected())
            {
                return;
            }

            int j = selectedVariableIndex.Value;
            string label = "x" + (j + 1);

            string input = ShowInputDialog($"Enter the new objective coefficient for {label}:", "Apply Variable Change");
            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            if (!double.TryParse(input, out double newValue))
            {
                MessageBox.Show("Enter a valid number.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                bool isBasic = currentResult.FinalTableau.BasicVariables.Contains(label);
                SolverResult changedResult = isBasic
                    ? analyzer.ApplyBasicVariableChange(j, newValue)
                    : analyzer.ApplyNonBasicVariableChange(j, newValue);

                DisplayAndLogChange($"Applied change: {label} objective coefficient -> {newValue}", changedResult, currentModel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Constraint RHS range / apply change
        private void btnSenRangeRHS_Click(object sender, EventArgs e)
        {
            if (!EnsureSolved() || !EnsureConstraintSelected())
            {
                return;
            }

            try
            {
                int i = selectedConstraintIndex.Value;
                Range range = analyzer.GetRHSRange(i);
                string message = $"Range for c{i + 1} RHS:\nLower Bound: {FormatBound(range.LowerBound)}\nUpper Bound: {FormatBound(range.UpperBound)}";
                MessageBox.Show(message, "RHS Range", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AppendToSensitivityLog(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSenApplyRHS_Click(object sender, EventArgs e)
        {
            if (!EnsureSolved() || !EnsureConstraintSelected())
            {
                return;
            }

            int i = selectedConstraintIndex.Value;

            string input = ShowInputDialog($"Enter the new RHS for c{i + 1}:", "Apply RHS Change");
            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            if (!double.TryParse(input, out double newRhs))
            {
                MessageBox.Show("Enter a valid number.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                SolverResult changedResult = analyzer.ApplyRHSChange(i, newRhs);
                DisplayAndLogChange($"Applied change: c{i + 1} RHS -> {newRhs}", changedResult, currentModel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Non-basic variable column range / apply change
        private void btnSenRangeCol_Click(object sender, EventArgs e)
        {
            if (!EnsureSolved() || !EnsureVariableSelected())
            {
                return;
            }

            try
            {
                int j = selectedVariableIndex.Value;
                string label = "x" + (j + 1);
                Range range = analyzer.GetVariableColumnRange(j);
                string message = $"Scale-factor range for {label}'s column (multiplies its original coefficients):\nLower Bound: {FormatBound(range.LowerBound)}\nUpper Bound: {FormatBound(range.UpperBound)}";
                MessageBox.Show(message, "Column Range", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AppendToSensitivityLog(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSenApplyCol_Click(object sender, EventArgs e)
        {
            if (!EnsureSolved() || !EnsureVariableSelected())
            {
                return;
            }

            int j = selectedVariableIndex.Value;
            string label = "x" + (j + 1);

            string input = ShowInputDialog(
                $"Enter new coefficients for {label}'s column, one per constraint (c1..c{currentModel.Constraints.Count}), comma-separated:",
                "Apply Column Change");
            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            if (!TryParseDoubleArray(input, currentModel.Constraints.Count, out double[] newColumn))
            {
                MessageBox.Show($"Enter exactly {currentModel.Constraints.Count} comma-separated numbers.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                SolverResult changedResult = analyzer.ApplyVariableColumnChange(j, newColumn);
                DisplayAndLogChange($"Applied column change: {label} -> [{string.Join(", ", newColumn)}]", changedResult, currentModel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Add new activity / constraint
        private void btnSenAddActivity_Click(object sender, EventArgs e)
        {
            if (!EnsureSolved())
            {
                return;
            }

            string columnInput = ShowInputDialog(
                $"Enter the new activity's technological coefficients, one per constraint (c1..c{currentModel.Constraints.Count}), comma-separated:",
                "Add New Activity");
            if (string.IsNullOrWhiteSpace(columnInput))
            {
                return;
            }

            string objInput = ShowInputDialog("Enter the new activity's objective coefficient:", "Add New Activity");
            if (string.IsNullOrWhiteSpace(objInput))
            {
                return;
            }

            if (!TryParseDoubleArray(columnInput, currentModel.Constraints.Count, out double[] newColumn))
            {
                MessageBox.Show($"Enter exactly {currentModel.Constraints.Count} comma-separated numbers.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!double.TryParse(objInput, out double objCoeff))
            {
                MessageBox.Show("Enter a valid number.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                SolverResult changedResult = analyzer.AddNewActivity(newColumn, objCoeff);

                // The result now has one more variable than currentModel does — build a
                // display-only model wrapper so Display.PopulateFullHistory's summary row
                // (which loops model.DecisionVariableCount) shows the new activity too.
                var displayModel = new LpModel
                {
                    IsMaximization = currentModel.IsMaximization,
                    ObjectiveCoefficients = new double[currentModel.DecisionVariableCount + 1],
                    SignRestrictions = currentModel.SignRestrictions,
                    Constraints = currentModel.Constraints
                };

                DisplayAndLogChange(
                    $"Added new activity x{currentModel.DecisionVariableCount + 1} (obj coeff {objCoeff}, column [{string.Join(", ", newColumn)}])",
                    changedResult, displayModel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSenAddConstraint_Click(object sender, EventArgs e)
        {
            if (!EnsureSolved())
            {
                return;
            }

            string input = ShowInputDialog(
                $"Enter the new constraint as: coefficients (comma-separated, one per x1..x{currentModel.DecisionVariableCount}), then relation (<=, >=, or =), then RHS.\nExample: 1,2,1 <= 10",
                "Add New Constraint");
            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            try
            {
                Constraints newConstraint = ParseConstraintInput(input, currentModel.DecisionVariableCount);
                SolverResult changedResult = analyzer.AddNewConstraint(newConstraint);

                DisplayAndLogChange(
                    $"Added new constraint: [{string.Join(", ", newConstraint.Coefficients)}] {newConstraint.Relation} {newConstraint.RHS}",
                    changedResult, currentModel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static Constraints ParseConstraintInput(string input, int expectedVarCount)
        {
            string relation = null;
            int relationIndex = -1;
            string[] relationTokens = { "<=", ">=", "=" };

            foreach (var token in relationTokens)
            {
                int idx = input.IndexOf(token);
                if (idx >= 0 && (relationIndex == -1 || idx < relationIndex))
                {
                    relationIndex = idx;
                    relation = token;
                }
            }

            if (relation == null)
            {
                throw new FormatException("Include a relation: <=, >=, or =.");
            }

            string coefficientsPart = input.Substring(0, relationIndex);
            string rhsPart = input.Substring(relationIndex + relation.Length);

            if (!TryParseDoubleArray(coefficientsPart, expectedVarCount, out double[] coefficients))
            {
                throw new FormatException($"Enter exactly {expectedVarCount} comma-separated coefficients.");
            }

            if (!double.TryParse(rhsPart.Trim(), out double rhs))
            {
                throw new FormatException("Enter a valid RHS number.");
            }

            return new Constraints { Coefficients = coefficients, Relation = relation, RHS = rhs };
        }
        #endregion

        #region Duality
        private void btnSenApplyDuality_Click(object sender, EventArgs e)
        {
            if (!EnsureSolved())
            {
                return;
            }

            try
            {
                LpModel dual = analyzer.ApplyDuality();
                string description = DescribeModel(dual, "Dual Model");
                MessageBox.Show(description, "Dual Model", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AppendToSensitivityLog(description);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSenSolveDual_Click(object sender, EventArgs e)
        {
            if (!EnsureSolved())
            {
                return;
            }

            try
            {
                LpModel dual = analyzer.ApplyDuality();
                SolverResult dualResult = analyzer.SolveDualModel();

                Display.PopulateFullHistory(dualResult, dual, dgwSenDisplay);

                EnsureSensitivityFile();
                WriteOutputFile.AppendResultToFile(dualResult, sensitivityFilePath, "Dual Model Solution");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSenDualityStrength_Click(object sender, EventArgs e)
        {
            if (!EnsureSolved())
            {
                return;
            }

            try
            {
                string strength = analyzer.CheckDualityStrength();
                string message = $"This model exhibits {strength} duality.";
                MessageBox.Show(message, "Duality Strength", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AppendToSensitivityLog(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string DescribeModel(LpModel model, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(title + ":");
            sb.Append(model.IsMaximization ? "max" : "min");
            for (int j = 0; j < model.ObjectiveCoefficients.Length; j++)
            {
                sb.Append($" {(model.ObjectiveCoefficients[j] >= 0 ? "+" : "")}{model.ObjectiveCoefficients[j]}");
            }
            sb.AppendLine();

            foreach (var c in model.Constraints)
            {
                foreach (var coef in c.Coefficients)
                {
                    sb.Append($" {(coef >= 0 ? "+" : "")}{coef}");
                }
                sb.Append($" {c.Relation} {c.RHS}");
                sb.AppendLine();
            }

            sb.AppendLine(string.Join(" ", model.SignRestrictions));
            return sb.ToString();
        }
        #endregion

        #region Shared helpers
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

        private bool EnsureVariableSelected()
        {
            if (!selectedVariableIndex.HasValue)
            {
                MessageBox.Show("Click a variable column in the tableau first.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private bool EnsureConstraintSelected()
        {
            if (!selectedConstraintIndex.HasValue)
            {
                MessageBox.Show("Click a constraint row in the tableau first.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // Shows the new solution after an "Apply" operation and appends it to the output file.
        // displayModel supplies DecisionVariableCount for the grid's summary row — pass a
        // wrapper with a bumped count for AddNewActivity, currentModel everywhere else.
        private void DisplayAndLogChange(string description, SolverResult changedResult, LpModel displayModel)
        {
            if (changedResult.IsInfeasible)
            {
                MessageBox.Show(description + "\n\nThe modified model is infeasible.", "Infeasible",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (changedResult.IsUnbounded)
            {
                MessageBox.Show(description + "\n\nThe modified model is unbounded.", "Unbounded",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (changedResult.IsOptimal)
            {
                var sb = new StringBuilder();
                sb.AppendLine(description);
                sb.AppendLine();
                sb.AppendLine("New Objective: " + FormatBound(changedResult.ObjectiveValue));
                for (int j = 0; j < changedResult.VariableValues.Length; j++)
                {
                    sb.AppendLine($"x{j + 1} = {FormatBound(changedResult.VariableValues[j])}");
                }
                MessageBox.Show(sb.ToString(), "Change Applied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            Display.PopulateFullHistory(changedResult, displayModel, dgwSenDisplay);

            EnsureSensitivityFile();
            WriteOutputFile.AppendResultToFile(changedResult, sensitivityFilePath, description);
        }

        private void EnsureSensitivityFile()
        {
            if (sensitivityFilePath == null)
            {
                sensitivityFilePath = Display.GetOutputFilePath();
            }
        }

        private void AppendToSensitivityLog(string text)
        {
            EnsureSensitivityFile();
            WriteOutputFile.AppendTextToFile(sensitivityFilePath, text);
        }

        private static bool TryParseDoubleArray(string input, int expectedCount, out double[] values)
        {
            values = null;
            var parts = input.Split(',');
            if (parts.Length != expectedCount)
            {
                return false;
            }

            var result = new double[expectedCount];
            for (int i = 0; i < expectedCount; i++)
            {
                if (!double.TryParse(parts[i].Trim(), out result[i]))
                {
                    return false;
                }
            }

            values = result;
            return true;
        }

        private static string FormatBound(double value)
        {
            if (double.IsNegativeInfinity(value)) return "-Infinity";
            if (double.IsPositiveInfinity(value)) return "Infinity";
            return Math.Round(value, 3).ToString();
        }

        // Small reusable text-input prompt — several operations need a value typed in
        // (a new coefficient, a new RHS) that a cell click alone can't provide.
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
        #endregion
    }
}
