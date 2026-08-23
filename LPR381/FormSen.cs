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

        // Which of the two the LAST click was actually about — lets one "Find Range"
        // button show the right thing instead of needing a separate button per kind.
        private enum SelectionKind { None, Variable, Constraint }
        private SelectionKind lastSelectionKind = SelectionKind.None;

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

            if (rowTag == "header")
            {
                // Clicking a column header is unambiguously "I want this variable".
                if (TryParseVariableIndex(columnName, out int varIndex) && varIndex < currentModel.DecisionVariableCount)
                {
                    selectedVariableIndex = varIndex;
                    lastSelectionKind = SelectionKind.Variable;
                }
            }
            else // rowTag == "data"
            {
                bool isRowLabelCell = e.ColumnIndex == 0;
                bool isRhsCell = string.Equals(columnName, "RHS", StringComparison.OrdinalIgnoreCase);

                if (isRowLabelCell || isRhsCell)
                {
                    // Clicking the row's own label, or its RHS value, means "I want this constraint".
                    string rowLabel = row.Cells[0].Value?.ToString();
                    if (TryParseConstraintIndex(rowLabel, out int constraintIndex) && constraintIndex < currentModel.Constraints.Count)
                    {
                        selectedConstraintIndex = constraintIndex;
                        lastSelectionKind = SelectionKind.Constraint;
                    }
                }
                else if (TryParseVariableIndex(columnName, out int varIndex) && varIndex < currentModel.DecisionVariableCount)
                {
                    // Clicking a coefficient inside a variable's column means "I want this variable".
                    selectedVariableIndex = varIndex;
                    lastSelectionKind = SelectionKind.Variable;
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
            string findRangeTarget = lastSelectionKind == SelectionKind.Variable ? varPart + " (variable)"
                : lastSelectionKind == SelectionKind.Constraint ? cPart + " (RHS)"
                : "none";
            lblSenSelection.Text = $"Selected variable: {varPart}    |    Selected constraint: {cPart}    |    Find Range will show: {findRangeTarget}";
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
                lastSelectionKind = SelectionKind.None;
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
                try
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
                    lastSelectionKind = SelectionKind.None;
                    sensitivityFilePath = null;
                    UpdateSelectionLabel();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not read the file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

        #region Find Range (one button — shows NBV, BV, or RHS range depending on what was clicked)
        private void btnSenFindRange_Click(object sender, EventArgs e)
        {
            if (!EnsureSolved())
            {
                return;
            }

            if (lastSelectionKind == SelectionKind.Variable && selectedVariableIndex.HasValue)
            {
                ShowVariableRange(selectedVariableIndex.Value);
            }
            else if (lastSelectionKind == SelectionKind.Constraint && selectedConstraintIndex.HasValue)
            {
                ShowRHSRange(selectedConstraintIndex.Value);
            }
            else
            {
                MessageBox.Show(
                    "Click a variable's column (its header, or any coefficient under it) for a variable range, " +
                    "or a constraint's row label / RHS cell for an RHS range — then press Find Range again.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ShowVariableRange(int j)
        {
            try
            {
                string label = "x" + (j + 1);
                bool isBasic = currentResult.FinalTableau.BasicVariables.Contains(label);
                var sb = new StringBuilder();

                if (isBasic)
                {
                    Range range = analyzer.GetBasicVariableRange(j);
                    sb.AppendLine($"{label} is a BASIC VARIABLE (BV).");
                    sb.AppendLine();
                    sb.AppendLine("Objective coefficient range — how far this coefficient can move while the current basis stays optimal:");
                    sb.AppendLine($"[{FormatBound(range.LowerBound)}, {FormatBound(range.UpperBound)}]");
                }
                else
                {
                    Range objRange = analyzer.GetNonBasicVariableRange(j);
                    sb.AppendLine($"{label} is a NON-BASIC VARIABLE (NBV).");
                    sb.AppendLine();
                    sb.AppendLine("Objective coefficient range — how far this coefficient can move before it becomes worth bringing into the basis:");
                    sb.AppendLine($"[{FormatBound(objRange.LowerBound)}, {FormatBound(objRange.UpperBound)}]");

                    try
                    {
                        Range colRange = analyzer.GetVariableColumnRange(j);
                        sb.AppendLine();
                        sb.AppendLine("Column scale-factor range — how much its constraint coefficients can be scaled (1.0 = unchanged) while it stays non-basic:");
                        sb.AppendLine($"[{FormatBound(colRange.LowerBound)}, {FormatBound(colRange.UpperBound)}]");
                    }
                    catch
                    {
                        // Column ranging can fail in edge cases (e.g. an all-zero column) —
                        // the objective-coefficient range above is still valid and shown.
                    }
                }

                string message = sb.ToString();
                MessageBox.Show(message, "Variable Range", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AppendToSensitivityLog(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowRHSRange(int i)
        {
            try
            {
                Range range = analyzer.GetRHSRange(i);
                string message = $"c{i + 1} is a CONSTRAINT RHS.\n\n" +
                    "RHS range — how far the right-hand side can move while the current basis stays optimal:\n" +
                    $"[{FormatBound(range.LowerBound)}, {FormatBound(range.UpperBound)}]";
                MessageBox.Show(message, "RHS Range", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AppendToSensitivityLog(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Apply variable change (basic or non-basic, auto-detected)
        private void btnSenApplyVar_Click(object sender, EventArgs e)
        {
            if (!EnsureSolved() || !EnsureVariableSelected())
            {
                return;
            }

            int j = selectedVariableIndex.Value;
            string label = "x" + (j + 1);
            bool isBasic = currentResult.FinalTableau.BasicVariables.Contains(label);

            string input = ShowInputDialog(
                $"Apply a new objective coefficient to {label} ({(isBasic ? "currently basic" : "currently non-basic")}) and re-solve the model with it.\n" +
                $"Current coefficient: {currentModel.ObjectiveCoefficients[j]}\n\n" +
                "Enter the new coefficient.\nExample: 4",
                "Apply Variable Change");
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
                SolverResult changedResult = isBasic
                    ? analyzer.ApplyBasicVariableChange(j, newValue)
                    : analyzer.ApplyNonBasicVariableChange(j, newValue);

                LpModel newModel = CloneModel(currentModel);
                newModel.ObjectiveCoefficients[j] = newValue;

                ApplyModelChange($"Applied change: {label} objective coefficient -> {newValue}", newModel, changedResult);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Apply RHS change
        private void btnSenApplyRHS_Click(object sender, EventArgs e)
        {
            if (!EnsureSolved() || !EnsureConstraintSelected())
            {
                return;
            }

            int i = selectedConstraintIndex.Value;

            string input = ShowInputDialog(
                $"Apply a new right-hand-side to constraint c{i + 1} and re-solve the model with it.\n" +
                $"Current RHS: {currentModel.Constraints[i].RHS}\n\n" +
                "Enter the new RHS.\nExample: 10",
                "Apply RHS Change");
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

                LpModel newModel = CloneModel(currentModel);
                newModel.Constraints[i].RHS = newRhs;

                ApplyModelChange($"Applied change: c{i + 1} RHS -> {newRhs}", newModel, changedResult);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Apply non-basic variable column change
        private void btnSenApplyCol_Click(object sender, EventArgs e)
        {
            if (!EnsureSolved() || !EnsureVariableSelected())
            {
                return;
            }

            int j = selectedVariableIndex.Value;
            string label = "x" + (j + 1);
            double[] originalColumn = currentModel.Constraints.Select(c => c.Coefficients[j]).ToArray();

            string input = ShowInputDialog(
                $"Apply new technological coefficients to {label}'s column (how much of each constraint's resource one unit of {label} uses) and re-solve.\n" +
                $"Current column: [{string.Join(", ", originalColumn)}]\n\n" +
                $"Enter one new coefficient per constraint (c1..c{currentModel.Constraints.Count}), comma-separated.\n" +
                "Example: 2,1,3",
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

                LpModel newModel = CloneModel(currentModel);
                for (int c = 0; c < newModel.Constraints.Count; c++)
                {
                    newModel.Constraints[c].Coefficients[j] = newColumn[c];
                }

                ApplyModelChange($"Applied column change: {label} -> [{string.Join(", ", newColumn)}]", newModel, changedResult);
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
                "Add a new decision variable (activity) to the model and see how it changes the optimal solution — " +
                "for example, a new product that consumes some of each constraint's resource.\n\n" +
                $"Enter its technological coefficients, one per constraint (c1..c{currentModel.Constraints.Count}), comma-separated.\n" +
                "Example: 2,1  (uses 2 units of c1's resource and 1 unit of c2's resource per unit made)",
                "Add New Activity");
            if (string.IsNullOrWhiteSpace(columnInput))
            {
                return;
            }

            string objInput = ShowInputDialog(
                "Now enter the new activity's objective coefficient (its profit or cost per unit).\nExample: 5",
                "Add New Activity");
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

                int newActivityLabel = currentModel.DecisionVariableCount + 1;

                LpModel newModel = CloneModel(currentModel);

                var newObjective = newModel.ObjectiveCoefficients.ToList();
                newObjective.Add(objCoeff);
                newModel.ObjectiveCoefficients = newObjective.ToArray();

                for (int c = 0; c < newModel.Constraints.Count; c++)
                {
                    var coeffs = newModel.Constraints[c].Coefficients.ToList();
                    coeffs.Add(newColumn[c]);
                    newModel.Constraints[c].Coefficients = coeffs.ToArray();
                }

                var newSigns = newModel.SignRestrictions.ToList();
                newSigns.Add("+"); // matches AnalyzeSensitivity.AddNewActivity's own default
                newModel.SignRestrictions = newSigns.ToArray();

                ApplyModelChange(
                    $"Added new activity x{newActivityLabel} (obj coeff {objCoeff}, column [{string.Join(", ", newColumn)}])",
                    newModel, changedResult);
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
                "Add a new constraint to the model and see how it changes the optimal solution — " +
                "for example, a resource limit that wasn't in the original problem.\n\n" +
                $"Enter the coefficients (comma-separated, one per x1..x{currentModel.DecisionVariableCount}), then a relation (<=, >=, or =), then the RHS.\n" +
                "Example: 1,2,1 <= 10  (means x1 + 2x2 + x3 <= 10)",
                "Add New Constraint");
            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            try
            {
                Constraints newConstraint = ParseConstraintInput(input, currentModel.DecisionVariableCount);
                SolverResult changedResult = analyzer.AddNewConstraint(newConstraint);

                LpModel newModel = CloneModel(currentModel);
                newModel.Constraints.Add(new Constraints
                {
                    Coefficients = (double[])newConstraint.Coefficients.Clone(),
                    Relation = newConstraint.Relation,
                    RHS = newConstraint.RHS
                });

                ApplyModelChange(
                    $"Added new constraint: [{string.Join(", ", newConstraint.Coefficients)}] {newConstraint.Relation} {newConstraint.RHS}",
                    newModel, changedResult);
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

        // Applies a change as the new working baseline: currentModel/currentResult/analyzer
        // all advance to it, so later operations (further applies, ranges, shadow prices,
        // add activity/constraint...) build on top of every earlier change, not just the
        // very first solve. Rejected (rolled back) if the change makes the model infeasible
        // or unbounded, so the working model never gets left in a broken state.
        private void ApplyModelChange(string description, LpModel newModel, SolverResult changedResult)
        {
            if (changedResult.IsInfeasible)
            {
                MessageBox.Show(description + "\n\nThe modified model is infeasible — the change was NOT applied. The working model is unchanged; try a different value.",
                    "Infeasible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                EnsureSensitivityFile();
                WriteOutputFile.AppendResultToFile(changedResult, sensitivityFilePath, description + " (rejected — infeasible)");
                return;
            }

            if (changedResult.IsUnbounded)
            {
                MessageBox.Show(description + "\n\nThe modified model is unbounded — the change was NOT applied. The working model is unchanged; try a different value.",
                    "Unbounded", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                EnsureSensitivityFile();
                WriteOutputFile.AppendResultToFile(changedResult, sensitivityFilePath, description + " (rejected — unbounded)");
                return;
            }

            currentModel = newModel;
            currentResult = changedResult;
            analyzer.OriginalModel = currentModel;
            analyzer.SolvedResult = currentResult;

            // Indices from before this change may no longer point at the same thing
            // (e.g. a new activity shifts nothing, but it's safest to make the user re-click).
            selectedVariableIndex = null;
            selectedConstraintIndex = null;
            lastSelectionKind = SelectionKind.None;
            UpdateSelectionLabel();

            var sb = new StringBuilder();
            sb.AppendLine(description);
            sb.AppendLine();
            sb.AppendLine("New Objective: " + FormatBound(changedResult.ObjectiveValue));
            for (int j = 0; j < changedResult.VariableValues.Length; j++)
            {
                sb.AppendLine($"x{j + 1} = {FormatBound(changedResult.VariableValues[j])}");
            }
            MessageBox.Show(sb.ToString(), "Change Applied", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Display.PopulateFullHistory(changedResult, currentModel, dgwSenDisplay);

            EnsureSensitivityFile();
            WriteOutputFile.AppendResultToFile(changedResult, sensitivityFilePath, description);
        }

        // Mirrors AnalyzeSensitivity's own private CloneModel — used here to build the
        // next working model in parallel with the SolverResult the analyzer computes,
        // since the analyzer only returns the result, not the model it solved.
        private static LpModel CloneModel(LpModel original)
        {
            var copy = new LpModel
            {
                IsMaximization = original.IsMaximization,
                ObjectiveCoefficients = (double[])original.ObjectiveCoefficients.Clone(),
                SignRestrictions = (string[])original.SignRestrictions.Clone(),
                TempColNames = original.TempColNames != null ? (string[])original.TempColNames.Clone() : null
            };

            foreach (var constraint in original.Constraints)
            {
                copy.Constraints.Add(new Constraints
                {
                    Coefficients = (double[])constraint.Coefficients.Clone(),
                    Relation = constraint.Relation,
                    RHS = constraint.RHS
                });
            }

            return copy;
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
                prompt.Width = 480;
                prompt.Height = 350;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.Text = caption;
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.MaximizeBox = false;
                prompt.MinimizeBox = false;

                Label textLabel = new Label() { Left = 10, Top = 10, Width = 445, Height = 180, Text = text };
                TextBox textBox = new TextBox() { Left = 10, Top = 200, Width = 445 };
                Button confirmation = new Button() { Text = "OK", Left = 280, Width = 85, Height = 32, Top = 240, DialogResult = DialogResult.OK };
                Button cancel = new Button() { Text = "Cancel", Left = 375, Width = 85, Height = 32, Top = 240, DialogResult = DialogResult.Cancel };

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
