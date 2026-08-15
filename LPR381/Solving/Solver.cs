using LPR381.Stored_Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPR381.Solving
{
    internal class Solver
    {
        #region SolvePrimalSimplex
        public SolverResult SolvePrimalSimplex(LpModel model)
        {
            Tableau table = BuildCanonicalForm(model);
            var history = new List<Tableau> { CloneTableau(table) }; // snapshot t-i before any pivots

            // Check for negative RHS (from >= or = constraints) — needs Dual Simplex cleanup first
            int rhsCol = table.Rows[0].Count - 1;
            bool hasNegativeRHS = table.Rows.Skip(1).Any(row => row[rhsCol] < 0);
            var result = new SolverResult();

            if (hasNegativeRHS)
            {
                result.SwitchedToDualSimplex = true;
                bool infeasible = DualSimplex(table, history);
                if (infeasible)
                {
                    result.IsInfeasible = true;
                    result.IsOptimal = false;
                    result.FinalTableau = table;
                    result.IterationHistory = history;
                    return result; // stop here — no point running the primal loop on an infeasible tableau
                }
            }

            // Normal primal simplex loop

            var (isOptimal, isUnbounded) = RunPrimalLoop(table, history);

            // Build the result
            result.FinalTableau = table;
            result.IterationHistory = history;
            result.IsUnbounded = isUnbounded;
            result.IsOptimal = isOptimal;
            if (!isUnbounded)
            {
                // Objective value sits in the bottom-right corner of the objective row
                result.ObjectiveValue = table.Rows[0][rhsCol];

                // Extract variable values: for each ORIGINAL decision variable column,
                // if it's currently basic, its value is that row's RHS; otherwise it's 0 (non-basic)
                result.VariableValues = new double[model.DecisionVariableCount];
                for (int j = 0; j < model.DecisionVariableCount; j++)
                {
                    int basicRowIndex = table.BasicVariables.IndexOf(table.ColumnNames[j]);
                    result.VariableValues[j] = basicRowIndex == -1 ? 0 : table.Rows[basicRowIndex][rhsCol];
                }
            }

            return result;
        }
        #endregion

        public SolverResult SolveRevisedSimplex(LpModel model) { /* TODO */ return null; }
        public SolverResult SolveBranchAndBound(LpModel model) { /* TODO — builds/walks a BranchNode tree, calls SolvePrimalSimplex per node */ return null; }
        public SolverResult SolveCuttingPlane(LpModel model) { /* TODO — calls SolvePrimalSimplex, then adds cut rows */ return null; }
        public SolverResult SolveKnapsackBranchAndBound(LpModel model) { /* TODO — separate bounding logic, not simplex-based */ return null; }

        #region Prepocessing SignRestrictions
        private LpModel PreprocessingSignRestrictions(LpModel model)
        {
            var newModel = new LpModel();
            newModel.IsMaximization = model.IsMaximization;

            var newObjCoefficients = new List<double>();
            var newSignRestrictions = new List<string>();
            var newVariableNames = new List<string>();

            var variableExpansions = new List<(int count, int[] signs)>();

            for (int i = 0; i < model.DecisionVariableCount; i++)
            {
                string sign = model.SignRestrictions[i];
                string originalName = "x" + (i + 1);

                if (sign == "-")
                {
                    variableExpansions.Add((1, new int[] { -1 }));
                    newObjCoefficients.Add(model.ObjectiveCoefficients[i] * -1);
                    newSignRestrictions.Add("+");
                    newVariableNames.Add(originalName);
                }
                else if (sign == "urs")
                {
                    variableExpansions.Add((2, new int[] { 1, -1 }));
                    newObjCoefficients.Add(model.ObjectiveCoefficients[i]);
                    newObjCoefficients.Add(model.ObjectiveCoefficients[i] * -1);
                    newSignRestrictions.Add("+");
                    newSignRestrictions.Add("+");
                    newVariableNames.Add(originalName + "'");
                    newVariableNames.Add(originalName + '"');
                }
                else // "+", "int", "bin"
                {
                    variableExpansions.Add((1, new int[] { 1 }));
                    newObjCoefficients.Add(model.ObjectiveCoefficients[i]);
                    newSignRestrictions.Add(sign);
                    newVariableNames.Add(originalName);
                }
            }

            newModel.ObjectiveCoefficients = newObjCoefficients.ToArray();
            newModel.SignRestrictions = newSignRestrictions.ToArray();
            newModel.TempColNames = newVariableNames.ToArray();

            foreach (var constraint in model.Constraints)
            {
                var newCoeffs = new List<double>();
                for (int i = 0; i < model.DecisionVariableCount; i++)
                {
                    var (count, signs) = variableExpansions[i];
                    for (int k = 0; k < count; k++)
                        newCoeffs.Add(constraint.Coefficients[i] * signs[k]);
                }

                newModel.Constraints.Add(new Constraints
                {
                    Coefficients = newCoeffs.ToArray(),
                    Relation = constraint.Relation,
                    RHS = constraint.RHS
                });
            }

            return newModel;
        }
        #endregion

        private List<int> GetIntegerVariableIndices(LpModel model) { return null; }

        #region BuildCanonicalForm
        public Tableau BuildCanonicalForm(LpModel rawModel)
        {
            LpModel model = PreprocessingSignRestrictions(rawModel);
            var tableau = new Tableau();
            tableau.TableNumber = "t-i"; //The initial number of the table
            tableau.IsMaximization = model.IsMaximization;
            tableau.RowNames.Add("z"); //Add z to obj fun row
            tableau.BasicVariables.Add("Z"); //Z is added to basic variables so the indexes line up for tablaeu.Rows and tableau.BasicVariables
            int numVars = model.DecisionVariableCount;

            // Step 1: expand constraints — "=" becomes two rows, "<=" and ">=" stay as one each
            //We create a new list expanded rows it holds a double array for the different row values, a double for the rhs value
            //The flip just checks if it is <= (false) we dont need to *-1 all the values in the constraints
            //If it is >= flip is true and then we *-1 all the values in the constraints
            //Lastly we keep track of how many constraints there are if there are 3 constraints we need 3 extra variable columns
            //But lets say its an = constraint then it gets 2 extra columns so we need a way to keep track of that
            var expandedRows = new List<(double[] coefficients, double rhs, bool flip, int constraintNumber)>();

            for (int i = 0; i < model.Constraints.Count; i++)
            {
                var constraint = model.Constraints[i];
                int constraintNumber = i + 1; // We keep track of the constraint number that has been added so if we get s1 we know its s1 if we get e2 we know its e2 if we get an = we know its s3 and e3

                if (constraint.Relation == "<=")
                {
                    expandedRows.Add((constraint.Coefficients, constraint.RHS, false, constraintNumber));
                    tableau.RowNames.Add("c" + (i + 1)); //Add c1 to rowName for example
                }
                else if (constraint.Relation == ">=")
                {
                    expandedRows.Add((constraint.Coefficients, constraint.RHS, true, constraintNumber));
                    tableau.RowNames.Add("c" + (i + 1)); //Add c1 to rowname for example
                }
                else // "="
                {
                    expandedRows.Add((constraint.Coefficients, constraint.RHS, false, constraintNumber)); // "<=" half -> s{n}
                    tableau.RowNames.Add("c" + (i + 1) + "'"); //Add c1' to rowname for example

                    expandedRows.Add((constraint.Coefficients, constraint.RHS, true, constraintNumber));   // ">=" half -> e{n}, SAME number
                    tableau.RowNames.Add("c" + (i + 1) + '"');//Add c1" to rowname for example
                }
            }

            var binVariableIndices = new List<int>();
            for (int j = 0; j < numVars; j++)
            {
                if (model.SignRestrictions[j] == "bin")
                    binVariableIndices.Add(j);
            }

            int numConstraintRows = expandedRows.Count; //We find the total nr of constraints
            int numBinRows = binVariableIndices.Count;
            int totalCols = numVars + numConstraintRows + numBinRows + 1; //We find the total nr of columns +1 for the rhs column and numBinRows for each bin row that needs to be added

            // --- Column names ---
            for (int j = 0; j < numVars; j++)
                tableau.ColumnNames.Add(model.TempColNames[j]); //Now we add the columns names from the tempColNames from lpModel that were produced in sign restrictions

            foreach (var constraints in expandedRows)
            {
                //If flip is true it is >= so we add e if it is false it is <= so we add s
                string label = (constraints.flip ? "e" : "s") + constraints.constraintNumber;
                tableau.ColumnNames.Add(label); //For each constraints we add the constraint labels ex: s1,e2,s3,e3
            }

            int lastConstraintNumber = model.Constraints.Count;
            // NEW — one slack column per bin row, numbering continues after the constraint slack/excess columns
            for (int b = 0; b < numBinRows; b++)
            {
                tableau.ColumnNames.Add("s" + (lastConstraintNumber + b + 1));
            }

            tableau.ColumnNames.Add("RHS");

            // --- Objective row ---
            // We multiply the objective row with -1 for each value
            // to find pivot columns based on max / min we do it in FindPivotColumn method

            //Creates a list with doubles in the list so if totalCols is 14 it adds 14 doubles of 0.0 to the list
            List<double> objRow = new List<double>(new double[totalCols]);

            //Lets say numVars is 6 that means we have x1 - x6 now we replace x1-x6 spots in the'z' row with the objectiveCoefficients
            for (int j = 0; j < numVars; j++)
            {
                objRow[j] = -model.ObjectiveCoefficients[j]; //We replace the 0.0 with the actual values provided by the text file
            }
            tableau.Rows.Add(objRow);

            // --- Constraint rows ---
            for (int i = 0; i < numConstraintRows; i++)
            {
                var (coefficients, rhs, flip, constraintNumber) = expandedRows[i]; //We deconstruct expandedRows[i] into the variables coefficients, rhs, flip and constraintNumbers
                List<double> row = new List<double>(new double[totalCols]); //We again create a list with for example 14 0.0 doubles initialized and then just replace the 0's with actual variables where needed

                //If flip is true we have >= which means we need to multiple all the values with -1 in the constraints
                //If flip is false we have <= so we do not need to change the constraint values so we multiple with 1 to keep them the same
                int sign = flip ? -1 : 1; //This is the weird if statement before : is true and the next part is else

                for (int j = 0; j < numVars; j++)
                    row[j] = coefficients[j] * sign;//Here we add the coefficients from c1 if it is >= we multiply with -1 if it is <= we multiply with 1

                row[numVars + i] = 1; // We change the double from 0.0 to 1 in c1 where it is s1/e1 for example, in c2 we change where it is s2/e2 to 1
                row[totalCols - 1] = rhs * sign; //We multiple the rhs value with 1 if it is <= and -1 if it is >=

                tableau.Rows.Add(row); //Now we add c1 to the table
                tableau.BasicVariables.Add(tableau.ColumnNames[numVars + i]); //Here we add which columns are basic variables in the initial table it will be all the constraint values like s1,s2,e3,e4 etc
            }

            // NEW — bin upper-bound rows: x_j + s = 1
            for (int b = 0; b < numBinRows; b++)
            {
                int varIndex = binVariableIndices[b];
                List<double> row = new List<double>(new double[totalCols]);

                row[varIndex] = 1;
                int slackCol = numVars + numConstraintRows + b;
                row[slackCol] = 1;
                row[totalCols - 1] = 1;

                tableau.Rows.Add(row);
                tableau.BasicVariables.Add(tableau.ColumnNames[slackCol]);
                tableau.RowNames.Add("c" + (lastConstraintNumber + b + 1));
            }

            return tableau;
        }
        #endregion

        #region RunPrimalLoop
        private (bool isOptimal, bool isUnbounded) RunPrimalLoop(Tableau table, List<Tableau> history)
        {
            while (true)
            {
                int col = FindPivotColumn(table);
                if (col == -1) return (true, false); // optimal

                int row = FindPivotRow(table, col);
                if (row == -1) return (false, true); // unbounded

                Pivot(table, row, col);
                table.TableNumber = "t-" + (history.Count + 1);
                history.Add(CloneTableau(table));
            }
        }
        #endregion

        #region Pivot
        private void Pivot(Tableau table, int pivotRow, int pivotCol) 
        {
            double pivotValue = table.Rows[pivotRow][pivotCol];

            for (int i = 0; i < table.Rows[pivotRow].Count; i++)
            {
                table.Rows[pivotRow][i] = table.Rows[pivotRow][i] / pivotValue;
            }

            for (int i = 0; i < table.Rows.Count; i++) //Rows
            {
                if (i == pivotRow)
                {
                    continue;
                }

                double factor = table.Rows[i][pivotCol];

                for (int j = 0; j < table.Rows[0].Count; j++) //Columns
                {
                    table.Rows[i][j] = table.Rows[i][j] - factor * table.Rows[pivotRow][j];
                }
            }
            table.BasicVariables[pivotRow] = table.ColumnNames[pivotCol];
        }
        #endregion

        #region FindPivotColumn
        private int FindPivotColumn(Tableau table) 
        {
            int colNumber = -1;
            var objRow = table.Rows[0];
            int lastCol = objRow.Count - 1; // exclude RHS column

            if (table.IsMaximization)
            {
                for (int i = 0; i < lastCol; i++)
                {
                    if (objRow[i] >= 0)
                        continue;

                    if (colNumber == -1 || objRow[i] < objRow[colNumber])
                    {
                        colNumber = i;
                    }
                }
            }
            else
            {
                for (int i = 0; i < lastCol; i++)
                {
                    if (objRow[i] <= 0)
                        continue;

                    if (colNumber == -1 || objRow[i] > objRow[colNumber])
                    {
                        colNumber = i;
                    }
                }
            }

            return colNumber;
        }
        #endregion

        #region FindPivotRow
        private int FindPivotRow(Tableau table, int pivotCol) 
        {
            int rowNr = -1;
            int rhsCol = table.Rows[0].Count - 1;

            for (int i = 1; i < table.Rows.Count; i++)
            {
                if (table.Rows[i][pivotCol] <= 0)
                {
                    continue; // excludes negative AND zero pivot-column values — this is what removes the ambiguity you're describing
                }

                double ratio = table.Rows[i][rhsCol] / table.Rows[i][pivotCol];

                if (rowNr == -1 || ratio < (table.Rows[rowNr][rhsCol] / table.Rows[rowNr][pivotCol]))
                {
                    rowNr = i;
                }
            }

            return rowNr;
        }
        #endregion

        #region DualSimplex
        private bool DualSimplex(Tableau table, List<Tableau> history)
        {
            while (true)
            {
                int rowNr = FindDualPivotRow(table);
                if (rowNr == -1)
                {
                    return false; // no negative RHS left -> feasible, done
                }

                int colNr = FindDualPivotCol(table, rowNr);
                if (colNr == -1)
                {
                    return true; // no valid pivot column -> infeasible
                }

                Pivot(table, rowNr, colNr);
                table.TableNumber = "t-" + (history.Count + 1);
                history.Add(CloneTableau(table));
            }
        }
        #endregion

        #region FindDualPivotRow
        private int FindDualPivotRow(Tableau table)
        {
            int pivotRow = -1;

            for (int i = 1; i < table.Rows.Count; i++)
            {
                if ((table.Rows[i][(table.Rows[i].Count - 1)] >= 0))
                {
                    continue;
                }
                if (pivotRow == -1 || (table.Rows[pivotRow][table.Rows[pivotRow].Count - 1]) > (table.Rows[i][table.Rows[i].Count - 1]))
                {
                    pivotRow = i;
                }
            }
            return pivotRow;
        }
        #endregion

        #region FindDualPivotCol
        private int FindDualPivotCol(Tableau table, int pivotRow)
        {
            int pivotCol = -1;

            for (int i = 0; i < table.Rows[0].Count - 1; i++)
            {
                if (table.Rows[pivotRow][i] >= 0)
                {
                    continue;
                }
                if (pivotCol == -1)
                {
                    pivotCol = i;
                }
                if (Math.Abs((table.Rows[0][pivotCol]) / (table.Rows[pivotRow][pivotCol])) > Math.Abs((table.Rows[0][i]) / (table.Rows[pivotRow][i])))
                {
                    pivotCol = i;
                }
            }
            return pivotCol;
        }
        #endregion

        #region CloneTableau
        private Tableau CloneTableau(Tableau table)
        {
            var copy = new Tableau();

            // Copy each row individually — new List<double>(existingRow) copies the VALUES,
            // not just a reference to the same list
            foreach (var row in table.Rows)
            {
                copy.Rows.Add(new List<double>(row));
            }

            // Same idea for the two label lists
            copy.ColumnNames = new List<string>(table.ColumnNames);
            copy.BasicVariables = new List<string>(table.BasicVariables);
            copy.RowNames = new List<string>(table.RowNames);
            copy.TableNumber = table.TableNumber;
            copy.IsMaximization = table.IsMaximization;
            return copy;
        }
        #endregion
    }
}
