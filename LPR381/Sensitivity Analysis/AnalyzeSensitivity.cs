using System;
using System.Collections.Generic;
using System.Linq;
using LPR381.Solving;
using LPR381.Stored_Info;

namespace LPR381.Sensitivity_Analysis
{
    internal class AnalyzeSensitivity
    {
        private const double TOLERANCE = 1e-7;
        private const double MAX_SEARCH_VALUE = 1000000000.0;

        public LpModel OriginalModel { get; set; }
        public SolverResult SolvedResult { get; set; }

        // ============================================================
        // 1. NON-BASIC VARIABLE OBJECTIVE COEFFICIENT
        // ============================================================

        public Range GetNonBasicVariableRange(int variableIndex)
        {
            ValidateVariableIndex(variableIndex);
            ValidateSolvedResult();

            if (IsVariableBasic(variableIndex))
            {
                throw new InvalidOperationException(
                    "The selected variable is basic, not non-basic.");
            }

            double currentValue = OriginalModel.ObjectiveCoefficients[variableIndex];

            return FindBasisPreservingRange(
                currentValue,
                (model, value) =>
                {
                    model.ObjectiveCoefficients[variableIndex] = value;
                });
        }

        public SolverResult ApplyNonBasicVariableChange(
            int variableIndex,
            double newValue)
        {
            ValidateVariableIndex(variableIndex);
            ValidateSolvedResult();

            if (IsVariableBasic(variableIndex))
            {
                throw new InvalidOperationException(
                    "The selected variable is basic, not non-basic.");
            }

            LpModel changedModel = CloneModel(OriginalModel);

            changedModel.ObjectiveCoefficients[variableIndex] = newValue;

            return SolveModel(changedModel);
        }


        public Range GetBasicVariableRange(int variableIndex)
        {
            ValidateVariableIndex(variableIndex);
            ValidateSolvedResult();

            if (!IsVariableBasic(variableIndex))
            {
                throw new InvalidOperationException(
                    "The selected variable is non-basic, not basic.");
            }

            double currentValue =
                OriginalModel.ObjectiveCoefficients[variableIndex];

            return FindBasisPreservingRange(
                currentValue,
                (model, value) =>
                {
                    model.ObjectiveCoefficients[variableIndex] = value;
                });
        }

        public SolverResult ApplyBasicVariableChange(
            int variableIndex,
            double newValue)
        {
            ValidateVariableIndex(variableIndex);
            ValidateSolvedResult();

            if (!IsVariableBasic(variableIndex))
            {
                throw new InvalidOperationException(
                    "The selected variable is non-basic, not basic.");
            }

            LpModel changedModel = CloneModel(OriginalModel);

            changedModel.ObjectiveCoefficients[variableIndex] = newValue;

            return SolveModel(changedModel);
        }


        public Range GetRHSRange(int constraintIndex)
        {
            ValidateConstraintIndex(constraintIndex);
            ValidateSolvedResult();

            double currentValue =
                OriginalModel.Constraints[constraintIndex].RHS;

            return FindBasisPreservingRange(
                currentValue,
                (model, value) =>
                {
                    model.Constraints[constraintIndex].RHS = value;
                });
        }

        public SolverResult ApplyRHSChange(
            int constraintIndex,
            double newRHS)
        {
            ValidateConstraintIndex(constraintIndex);
            ValidateSolvedResult();

            LpModel changedModel = CloneModel(OriginalModel);

            changedModel.Constraints[constraintIndex].RHS = newRHS;

            return SolveModel(changedModel);
        }


        public Range GetVariableColumnRange(int variableIndex)
        {
            ValidateVariableIndex(variableIndex);
            ValidateSolvedResult();

            if (IsVariableBasic(variableIndex))
            {
                throw new InvalidOperationException(
                    "The selected variable must be non-basic for column sensitivity analysis.");
            }

            double[] originalColumn =
                OriginalModel.Constraints
                    .Select(c => c.Coefficients[variableIndex])
                    .ToArray();

            double currentScale = 1.0;

            return FindBasisPreservingRange(
                currentScale,
                (model, scale) =>
                {
                    for (int i = 0;
                         i < model.Constraints.Count;
                         i++)
                    {
                        model.Constraints[i]
                            .Coefficients[variableIndex]
                            = originalColumn[i] * scale;
                    }
                });
        }


        public SolverResult ApplyVariableColumnChange(
            int variableIndex,
            double[] newColumn)
        {
            ValidateVariableIndex(variableIndex);
            ValidateSolvedResult();

            if (newColumn == null)
            {
                throw new ArgumentNullException(nameof(newColumn));
            }

            if (newColumn.Length != OriginalModel.Constraints.Count)
            {
                throw new ArgumentException(
                    "The new column must contain one coefficient for every constraint.");
            }

            if (IsVariableBasic(variableIndex))
            {
                throw new InvalidOperationException(
                    "The selected variable must be non-basic for column sensitivity analysis.");
            }

            LpModel changedModel = CloneModel(OriginalModel);

            for (int i = 0;
                 i < changedModel.Constraints.Count;
                 i++)
            {
                changedModel.Constraints[i]
                    .Coefficients[variableIndex] = newColumn[i];
            }

            return SolveModel(changedModel);
        }


        public SolverResult AddNewActivity(
            double[] newColumnCoefficients,
            double objectiveCoefficient)
        {
            ValidateSolvedResult();

            if (newColumnCoefficients == null)
            {
                throw new ArgumentNullException(
                    nameof(newColumnCoefficients));
            }

            if (newColumnCoefficients.Length !=
                OriginalModel.Constraints.Count)
            {
                throw new ArgumentException(
                    "The new activity must contain one coefficient for every constraint.");
            }

            LpModel changedModel = CloneModel(OriginalModel);

            // Add the new objective coefficient.
            List<double> newObjective =
                changedModel.ObjectiveCoefficients.ToList();

            newObjective.Add(objectiveCoefficient);

            changedModel.ObjectiveCoefficients =
                newObjective.ToArray();

            // Add the new column to every constraint.
            for (int i = 0;
                 i < changedModel.Constraints.Count;
                 i++)
            {
                List<double> coefficients =
                    changedModel.Constraints[i]
                        .Coefficients
                        .ToList();

                coefficients.Add(newColumnCoefficients[i]);

                changedModel.Constraints[i]
                    .Coefficients = coefficients.ToArray();
            }

            // Default new activity to a non-negative continuous variable.
            List<string> signs =
                changedModel.SignRestrictions.ToList();

            signs.Add("+");

            changedModel.SignRestrictions =
                signs.ToArray();

            return SolveModel(changedModel);
        }


        public SolverResult AddNewConstraint(
            Constraints newConstraint)
        {
            ValidateSolvedResult();

            if (newConstraint == null)
            {
                throw new ArgumentNullException(
                    nameof(newConstraint));
            }

            if (newConstraint.Coefficients == null)
            {
                throw new ArgumentException(
                    "The new constraint must contain coefficients.");
            }

            if (newConstraint.Coefficients.Length !=
                OriginalModel.DecisionVariableCount)
            {
                throw new ArgumentException(
                    "The new constraint must contain one coefficient for every decision variable.");
            }

            if (newConstraint.Relation != "<=" &&
                newConstraint.Relation != ">=" &&
                newConstraint.Relation != "=")
            {
                throw new ArgumentException(
                    "Constraint relation must be <=, >= or =.");
            }

            LpModel changedModel = CloneModel(OriginalModel);

            changedModel.Constraints.Add(
                new Constraints
                {
                    Coefficients =
                        (double[])newConstraint.Coefficients.Clone(),

                    Relation = newConstraint.Relation,

                    RHS = newConstraint.RHS
                });

            return SolveModel(changedModel);
        }


        public double[] GetShadowPrices()
        {
            ValidateSolvedResult();

            double[] shadowPrices =
                new double[OriginalModel.Constraints.Count];

            SolverResult baseResult =
                SolveModel(CloneModel(OriginalModel));

            if (!baseResult.IsOptimal)
            {
                throw new InvalidOperationException(
                    "Shadow prices can only be calculated for an optimal solution.");
            }


            for (int i = 0;
                 i < OriginalModel.Constraints.Count;
                 i++)
            {
                double rhs =
                    OriginalModel.Constraints[i].RHS;

                double epsilon =
                    0.00001 * Math.Max(1.0, Math.Abs(rhs));

                LpModel plusModel =
                    CloneModel(OriginalModel);

                LpModel minusModel =
                    CloneModel(OriginalModel);

                plusModel.Constraints[i].RHS =
                    rhs + epsilon;

                minusModel.Constraints[i].RHS =
                    rhs - epsilon;

                SolverResult plusResult =
                    SolveModel(plusModel);

                SolverResult minusResult =
                    SolveModel(minusModel);

                if (plusResult.IsOptimal &&
                    minusResult.IsOptimal)
                {
                    shadowPrices[i] =
                        (plusResult.ObjectiveValue -
                         minusResult.ObjectiveValue)
                        / (2.0 * epsilon);
                }
                else if (plusResult.IsOptimal)
                {
                    shadowPrices[i] =
                        (plusResult.ObjectiveValue -
                         baseResult.ObjectiveValue)
                        / epsilon;
                }
                else if (minusResult.IsOptimal)
                {
                    shadowPrices[i] =
                        (baseResult.ObjectiveValue -
                         minusResult.ObjectiveValue)
                        / epsilon;
                }
                else
                {
                    shadowPrices[i] = double.NaN;
                }
            }

            return shadowPrices;
        }


        public LpModel ApplyDuality()
        {
            if (OriginalModel == null)
            {
                throw new InvalidOperationException(
                    "OriginalModel has not been assigned.");
            }

            LpModel dual = new LpModel();

            // Max primal -> Min dual
            // Min primal -> Max dual
            dual.IsMaximization =
                !OriginalModel.IsMaximization;

            // --------------------------------------------------------
            // Dual objective coefficients
            //
            // The primal RHS values become the dual objective
            // coefficients.
            // --------------------------------------------------------

            dual.ObjectiveCoefficients =
                OriginalModel.Constraints
                    .Select(c => c.RHS)
                    .ToArray();


            List<string> dualSigns =
                new List<string>();

            foreach (Constraints constraint
                     in OriginalModel.Constraints)
            {
                if (constraint.Relation == "=")
                {
                    // Equality constraint -> unrestricted dual variable
                    dualSigns.Add("urs");
                }
                else if (OriginalModel.IsMaximization)
                {
                    // Max primal:
                    // <= -> y >= 0
                    // >= -> y <= 0
                    if (constraint.Relation == "<=")
                        dualSigns.Add("+");
                    else
                        dualSigns.Add("-");
                }
                else
                {
                    // Min primal:
                    // <= -> y <= 0
                    // >= -> y >= 0
                    if (constraint.Relation == "<=")
                        dualSigns.Add("-");
                    else
                        dualSigns.Add("+");
                }
            }

            dual.SignRestrictions =
                dualSigns.ToArray();


            for (int j = 0;
                 j < OriginalModel.DecisionVariableCount;
                 j++)
            {
                double[] coefficients =
                    OriginalModel.Constraints
                        .Select(c => c.Coefficients[j])
                        .ToArray();

                string relation;

                string primalSign =
                    OriginalModel.SignRestrictions[j];

                if (primalSign == "urs")
                {
                    relation = "=";
                }
                else if (OriginalModel.IsMaximization)
                {
                    // Max primal -> Min dual
                    //
                    // x >= 0 -> >=
                    // x <= 0 -> <=
                    if (primalSign == "-")
                        relation = "<=";
                    else
                        relation = ">=";
                }
                else
                {
                    // Min primal -> Max dual
                    //
                    // x >= 0 -> <=
                    // x <= 0 -> >=
                    if (primalSign == "-")
                        relation = ">=";
                    else
                        relation = "<=";
                }

                dual.Constraints.Add(
                    new Constraints
                    {
                        Coefficients = coefficients,
                        Relation = relation,
                        RHS = OriginalModel.ObjectiveCoefficients[j]
                    });
            }

            return dual;
        }


        public SolverResult SolveDualModel()
        {
            LpModel dual = ApplyDuality();

            return SolveModel(dual);
        }


        public string CheckDualityStrength()
        {
            ValidateSolvedResult();

            SolverResult primalResult =
                SolveModel(CloneModel(OriginalModel));

            if (!primalResult.IsOptimal)
            {
                return "weak";
            }

            LpModel dual =
                ApplyDuality();

            SolverResult dualResult =
                SolveModel(dual);

            if (!dualResult.IsOptimal)
            {
                return "weak";
            }

            double difference =
                Math.Abs(
                    primalResult.ObjectiveValue -
                    dualResult.ObjectiveValue);

            if (difference <= 0.0001)
            {
                return "strong";
            }

            return "weak";
        }



        private SolverResult SolveModel(LpModel model)
        {
            Solver solver = new Solver();

            

            if (model.IsMaximization)
            {
                return solver.SolvePrimalSimplex(model);
            }

            LpModel transformed =
                CloneModel(model);

            transformed.IsMaximization = true;

            for (int i = 0;
                 i < transformed.ObjectiveCoefficients.Length;
                 i++)
            {
                transformed.ObjectiveCoefficients[i] *= -1.0;
            }

            SolverResult result =
                solver.SolvePrimalSimplex(transformed);

            if (result.IsOptimal)
            {
                result.ObjectiveValue *= -1.0;
            }

            if (result.FinalTableau != null)
            {
                result.FinalTableau.IsMaximization = false;
            }

            return result;
        }



        private Range FindBasisPreservingRange(
            double currentValue,
            Action<LpModel, double> modifier)
        {
            ValidateSolvedResult();

            double? lower =
                FindBoundary(
                    currentValue,
                    -1,
                    modifier);

            double? upper =
                FindBoundary(
                    currentValue,
                    +1,
                    modifier);

            return new Range
            {
                LowerBound =
                    lower.HasValue
                        ? lower.Value
                        : double.NegativeInfinity,

                UpperBound =
                    upper.HasValue
                        ? upper.Value
                        : double.PositiveInfinity
            };
        }


        /*
         * Searches in one direction until the current optimal basis
         * stops being optimal.
         */
        private double? FindBoundary(
            double currentValue,
            int direction,
            Action<LpModel, double> modifier)
        {
            double stableValue = currentValue;

            double step =
                Math.Max(
                    1.0,
                    Math.Abs(currentValue) * 0.1);

            for (int iteration = 0;
                 iteration < 60;
                 iteration++)
            {
                double candidate =
                    currentValue +
                    (direction * step);

                if (Math.Abs(candidate) >
                    MAX_SEARCH_VALUE)
                {
                    return null;
                }

                SolverResult result =
                    SolveWithParameter(
                        candidate,
                        modifier);

                if (HasSameBasis(result))
                {
                    stableValue = candidate;
                    step *= 2.0;
                }
                else
                {
                    return BinarySearchBoundary(
                        stableValue,
                        candidate,
                        direction,
                        modifier);
                }
            }

            return null;
        }


        private double BinarySearchBoundary(
            double stableValue,
            double unstableValue,
            int direction,
            Action<LpModel, double> modifier)
        {
            double stable = stableValue;
            double unstable = unstableValue;

            for (int iteration = 0;
                 iteration < 70;
                 iteration++)
            {
                double middle =
                    (stable + unstable) / 2.0;

                if (Math.Abs(stable - unstable) <
                    TOLERANCE *
                    Math.Max(1.0, Math.Abs(middle)))
                {
                    break;
                }

                SolverResult result =
                    SolveWithParameter(
                        middle,
                        modifier);

                if (HasSameBasis(result))
                {
                    stable = middle;
                }
                else
                {
                    unstable = middle;
                }
            }

            return stable;
        }


        private SolverResult SolveWithParameter(
            double value,
            Action<LpModel, double> modifier)
        {
            try
            {
                LpModel model =
                    CloneModel(OriginalModel);

                modifier(model, value);

                return SolveModel(model);
            }
            catch
            {
                return null;
            }
        }


        private bool HasSameBasis(
            SolverResult result)
        {
            if (result == null ||
                !result.IsOptimal ||
                result.FinalTableau == null ||
                SolvedResult == null ||
                SolvedResult.FinalTableau == null)
            {
                return false;
            }

            List<string> originalBasis =
                SolvedResult.FinalTableau.BasicVariables;

            List<string> newBasis =
                result.FinalTableau.BasicVariables;

            if (originalBasis.Count != newBasis.Count)
            {
                return false;
            }

            for (int i = 0;
                 i < originalBasis.Count;
                 i++)
            {
                if (originalBasis[i] != newBasis[i])
                {
                    return false;
                }
            }

            return true;
        }


        private bool IsVariableBasic(int variableIndex)
        {
            string variableName =
                "x" + (variableIndex + 1);

            if (SolvedResult == null ||
                SolvedResult.FinalTableau == null)
            {
                return false;
            }

           
            string sign =
                OriginalModel.SignRestrictions[variableIndex];

            if (sign == "urs")
            {
                throw new InvalidOperationException(
                    "URS variables do not have a single basic/non-basic column.");
            }

            return SolvedResult.FinalTableau
                .BasicVariables
                .Contains(variableName);
        }


        private void ValidateSolvedResult()
        {
            if (OriginalModel == null)
            {
                throw new InvalidOperationException(
                    "OriginalModel has not been assigned.");
            }

            if (SolvedResult == null)
            {
                throw new InvalidOperationException(
                    "SolvedResult has not been assigned.");
            }

            if (!SolvedResult.IsOptimal)
            {
                throw new InvalidOperationException(
                    "Sensitivity analysis requires an optimal solution.");
            }

            if (SolvedResult.FinalTableau == null)
            {
                throw new InvalidOperationException(
                    "The solved result does not contain a final tableau.");
            }
        }


        private void ValidateVariableIndex(
            int variableIndex)
        {
            if (OriginalModel == null)
            {
                throw new InvalidOperationException(
                    "OriginalModel has not been assigned.");
            }

            if (variableIndex < 0 ||
                variableIndex >=
                OriginalModel.DecisionVariableCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(variableIndex));
            }
        }


        private void ValidateConstraintIndex(
            int constraintIndex)
        {
            if (OriginalModel == null)
            {
                throw new InvalidOperationException(
                    "OriginalModel has not been assigned.");
            }

            if (constraintIndex < 0 ||
                constraintIndex >=
                OriginalModel.Constraints.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(constraintIndex));
            }
        }



        private LpModel CloneModel(LpModel original)
        {
            LpModel copy = new LpModel();

            copy.IsMaximization =
                original.IsMaximization;

            copy.ObjectiveCoefficients =
                original.ObjectiveCoefficients != null
                    ? (double[])original.ObjectiveCoefficients.Clone()
                    : new double[0];

            copy.SignRestrictions =
                original.SignRestrictions != null
                    ? (string[])original.SignRestrictions.Clone()
                    : new string[0];

            copy.TempColNames =
                original.TempColNames != null
                    ? (string[])original.TempColNames.Clone()
                    : null;

            foreach (Constraints constraint
                     in original.Constraints)
            {
                copy.Constraints.Add(
                    new Constraints
                    {
                        Coefficients =
                            constraint.Coefficients != null
                                ? (double[])constraint.Coefficients.Clone()
                                : new double[0],

                        Relation = constraint.Relation,

                        RHS = constraint.RHS
                    });
            }

            return copy;
        }
    }


    public class Range
    {
        public double LowerBound { get; set; }

        public double UpperBound { get; set; }
    }
}