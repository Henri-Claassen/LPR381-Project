using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LPR381.Stored_Info;


namespace LPR381.Input_File_Handler
{
    internal class HandleInput
    {
        #region ReadModelFile
        //This method is used after the filePath is read to get the text from within the file it puts each line into 1 line of an array
        public static string[] ReadModelFile(string filePath)
        {
            return File.ReadAllLines(filePath);
        }
        #endregion

        #region ParseNlpModel
        public static NlpModel ParseNlpModel(string[] lines)
        {
            if (lines == null || lines.Length < 2)
            {
                throw new FormatException("The non-linear model file needs at least 2 lines: MAX/MIN, then the objective function.");
            }

            var model = new NlpModel();

            string opt = lines[0].Trim().ToUpper();
            model.IsMaximization = opt == "MAX";

            model.ObjectiveFunction = lines[1].Trim();
            
            if (lines.Length > 2)
            {
                string[] parts = lines[2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                model.InitialPoint = new double[parts.Length];
                for (int i = 0; i < parts.Length; i++)
                {
                    model.InitialPoint[i] = Convert.ToDouble(parts[i]);
                }
            }
            else
            {
                model.InitialPoint = new double[] { 1, 1 }; // default fallback
            }
            
            return model;
        }
        #endregion

        #region ParseModel
        public static LpModel ParseModel(string[] lines)
        {
            if (lines == null || lines.Length < 2)
            {
                throw new FormatException("The model file needs at least 2 lines: the objective function, then sign restrictions.");
            }

            var model = new LpModel();

            // Line 1: objective function
            var (isMax, coefficients) = ParseObjectiveLine(lines[0]);
            model.IsMaximization = isMax;
            model.ObjectiveCoefficients = coefficients;

            // Middle lines: constraints (skip line 0 and the last line)
            int numVars = coefficients.Length;
            for (int i = 1; i < lines.Length - 1; i++)
            {
                Constraints constraint = ParseConstraintLine(lines[i], numVars);
                model.Constraints.Add(constraint);
            }

            // Last line: sign restrictions
            model.SignRestrictions = ParseSignRestrictions(lines[lines.Length - 1]);

            ValidateModel(model);

            return model;
        }
        #endregion

        #region ValidateModel
        // Catches a malformed sign-restrictions line early, with a message that points at
        // what's wrong, instead of an IndexOutOfRangeException surfacing later deep inside
        // the solver. (Constraint coefficient counts don't need checking here — ParseConstraintLine
        // always builds a fixed numVars-length array, so they can never mismatch.)
        private static void ValidateModel(LpModel model)
        {
            int numVars = model.ObjectiveCoefficients.Length;

            if (model.SignRestrictions.Length != numVars)
            {
                throw new FormatException(
                    $"The sign restrictions line has {model.SignRestrictions.Length} entries but the objective function has {numVars} coefficients — they must match.");
            }

            var allowedSigns = new HashSet<string> { "+", "-", "urs", "int", "bin" };
            foreach (var sign in model.SignRestrictions)
            {
                if (!allowedSigns.Contains(sign))
                {
                    throw new FormatException($"'{sign}' is not a valid sign restriction. Use +, -, urs, int, or bin.");
                }
            }
        }
        #endregion

        #region ParseObjectiveLine
        private static (bool isMax, double[] coefficients) ParseObjectiveLine(string line)
        {
            string[] splitLine = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (splitLine.Length < 2)
            {
                throw new FormatException("The objective line needs 'max' or 'min' followed by at least one coefficient.");
            }

            string keyword = splitLine[0].ToLower();
            if (keyword != "max" && keyword != "min")
            {
                throw new FormatException($"The objective line must start with 'max' or 'min', not '{splitLine[0]}'.");
            }
            bool isMax = keyword == "max";

            double[] coefficients = new double[splitLine.Length - 1];
            for (int i = 1; i < splitLine.Length; i++)
            {
                if (!double.TryParse(splitLine[i], out coefficients[i - 1]))
                {
                    throw new FormatException($"'{splitLine[i]}' in the objective line is not a valid number.");
                }
            }

            return (isMax, coefficients);
        }
        #endregion

        #region ParseConstraintLine
        private static Constraints ParseConstraintLine(string line, int numVars)
        {
            string[] splitLine = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Needs at least one coefficient per variable plus a relation token — check
            // before indexing, since a short/malformed line would otherwise throw a
            // generic IndexOutOfRangeException that doesn't say what's actually wrong.
            if (splitLine.Length <= numVars)
            {
                throw new FormatException($"Constraint line \"{line}\" has too few values — expected {numVars} coefficients plus a relation.");
            }

            var constraint = new Constraints();
            constraint.Coefficients = new double[numVars];

            for (int i = 0; i < numVars; i++)
            {
                if (!double.TryParse(splitLine[i], out constraint.Coefficients[i]))
                {
                    throw new FormatException($"'{splitLine[i]}' in constraint line \"{line}\" is not a valid number.");
                }
            }

            string relationSign = splitLine[numVars];
            int relationLength;

            if (relationSign.StartsWith("<="))
            {
                constraint.Relation = "<=";
                relationLength = 2;
            }
            else if (relationSign.StartsWith(">="))
            {
                constraint.Relation = ">=";
                relationLength = 2;
            }
            else if (relationSign.StartsWith("="))
            {
                constraint.Relation = "=";
                relationLength = 1;
            }
            else
            {
                throw new FormatException($"Unrecognized relation in constraint line: \"{line}\"");
            }

            string rhsText = relationSign.Substring(relationLength); //If there is an error here the <=40 doesnt have a space in the text file it looks like this <= 40 need to add fail safe against that//
            if (string.IsNullOrWhiteSpace(rhsText))
            {
                // No RHS glued onto the relation (e.g. "<= 40" instead of "<=40") — it's the next token instead.
                if (splitLine.Length <= numVars + 1)
                {
                    throw new FormatException($"Constraint line \"{line}\" is missing its right-hand-side value.");
                }
                rhsText = splitLine[numVars + 1];
            }

            if (!double.TryParse(rhsText, out double rhs))
            {
                throw new FormatException($"'{rhsText}' in constraint line \"{line}\" is not a valid right-hand-side number.");
            }
            constraint.RHS = rhs;

            return constraint;
        }
        #endregion

        #region ParseSignRestrictions
        private static string[] ParseSignRestrictions(string line)
        {
            return line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }
        #endregion
    }
}
