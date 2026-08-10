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
        public SolverResult SolvePrimalSimplex(LpModel model) { /* TODO */ return null; }
        public SolverResult SolveRevisedSimplex(LpModel model) { /* TODO */ return null; }
        public SolverResult SolveBranchAndBound(LpModel model) { /* TODO — builds/walks a BranchNode tree, calls SolvePrimalSimplex per node */ return null; }
        public SolverResult SolveCuttingPlane(LpModel model) { /* TODO — calls SolvePrimalSimplex, then adds cut rows */ return null; }
        public SolverResult SolveKnapsackBranchAndBound(LpModel model) { /* TODO — separate bounding logic, not simplex-based */ return null; }


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
        private List<int> GetIntegerVariableIndices(LpModel model) { return null; }
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
        private void Pivot(Tableau table, int pivotRow, int pivotCol) { /* Logic to manipulate numbers once pivotRow and pivotCol has been selected */ }
        private int FindPivotColumn(Tableau table) { /* TODO */ return -1; }
        private int FindPivotRow(Tableau table, int pivotCol) { /* TODO */ return -1; }

        private void DualSimplex(Tableau table, List<Tableau> history) { }
        private int FindDualPivotRow(Tableau table) { return -1; }
        private int FindDualPivotCol(Tableau table, int pivotRow) { return -1; }
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
    }
}
