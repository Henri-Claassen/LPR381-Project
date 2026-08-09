using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPR381.Solving;
using LPR381.Stored_Info;

namespace LPR381.Sensitivity_Analysis
{
    internal class AnalyzeSensitivity
    {
        public LpModel OriginalModel { get; set; }
        public SolverResult SolvedResult { get; set; }

        public Range GetNonBasicVariableRange(int variableIndex) { /* TODO */ return null; }
        public SolverResult ApplyNonBasicVariableChange(int variableIndex, double newValue) { /* TODO */ return null; }
        public Range GetBasicVariableRange(int variableIndex) { /* TODO */ return null; }
        public SolverResult ApplyBasicVariableChange(int variableIndex, double newValue) { /* TODO */ return null; }
        public Range GetRHSRange(int constraintIndex) { /* TODO */ return null; }
        public SolverResult ApplyRHSChange(int constraintIndex, double newRHS) { /* TODO */ return null; }
        public Range GetVariableColumnRange(int variableIndex) { /* TODO */ return null; }
        public SolverResult ApplyVariableColumnChange(int variableIndex, double[] newColumn) { /* TODO */ return null; }
        public SolverResult AddNewActivity(double[] newColumnCoefficients, double objectiveCoefficient) { /* TODO */ return null; }
        public SolverResult AddNewConstraint(Constraints newConstraint) { /* TODO */ return null; }
        public double[] GetShadowPrices() { /* TODO */ return null; }
        public LpModel ApplyDuality() { /* TODO */ return null; }
        public SolverResult SolveDualModel() { /* TODO */ return null; }
        public string CheckDualityStrength() { /* TODO — "strong" or "weak" */ return null; }
    }
    

    public class Range
    {
        public double LowerBound { get; set; }
        public double UpperBound { get; set; }
    }
}

