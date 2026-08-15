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

        #region ParseModel
        public static LpModel ParseModel(string[] lines)
        {
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

            return model;
        }
        #endregion

        #region ParseObjectiveLine
        private static (bool isMax, double[] coefficients) ParseObjectiveLine(string line)
        {
            string[] splitLine = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            bool isMax = splitLine[0].ToLower() == "max";

            double[] coefficients = new double[splitLine.Length - 1];
            for (int i = 1; i < splitLine.Length; i++)
            {
                coefficients[i - 1] = Convert.ToDouble(splitLine[i]);
            }

            return (isMax, coefficients);
        }
        #endregion

        #region ParseConstraintLine
        private static Constraints ParseConstraintLine(string line, int numVars)
        {
            string[] splitLine = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var constraint = new Constraints();
            constraint.Coefficients = new double[numVars];

            for (int i = 0; i < numVars; i++)
            {
                constraint.Coefficients[i] = Convert.ToDouble(splitLine[i]);
            }

            string relationSign = splitLine[numVars];

            if (relationSign.StartsWith("<="))
            {
                constraint.Relation = "<=";
                string rhsText = relationSign.Substring(2); //If there is an error here the <=40 doesnt have a space in the text file it looks like this <= 40 need to add fail safe against that//
                if (string.IsNullOrWhiteSpace(rhsText))
                {
                    rhsText = splitLine[numVars + 1]; // Get the next part if it's empty
                    constraint.RHS = Convert.ToDouble(rhsText);
                }
            }
            else if (relationSign.StartsWith(">="))
            {
                constraint.Relation = ">=";
                string rhsText = relationSign.Substring(2);
                if(string.IsNullOrWhiteSpace(rhsText)) 
                {
                    rhsText = splitLine[numVars + 1];
                    constraint.RHS = Convert.ToDouble(rhsText);
                }
            }
            else if (relationSign.StartsWith("="))
            {
                constraint.Relation = "=";
                string rhsText = relationSign.Substring(1);
                if (string.IsNullOrWhiteSpace(rhsText))
                {
                    rhsText = splitLine[numVars + 1];
                    constraint.RHS = Convert.ToDouble(rhsText);
                }
            }
            else
            {
                throw new FormatException($"Unrecognized relation in constraint line: \"{line}\"");
            }
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
