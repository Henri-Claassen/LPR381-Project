using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPR381.Solving;

namespace LPR381.Output_File_Handler
{
    internal class WriteOutputFile
    {
        #region WriteResultToFile
        public static void WriteResultToFile(SolverResult result, string outputFilePath) 
        {
            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                writer.WriteLine("=== Iteration History ===");
                writer.WriteLine();

                foreach (var table in result.IterationHistory)
                {
                    WriteTableau(writer, table);
                    writer.WriteLine();
                }

                writer.WriteLine("=== Final Result ===");
                writer.WriteLine("Optimal: " + result.IsOptimal);
                writer.WriteLine("Infeasible: " + result.IsInfeasible);
                writer.WriteLine("Unbounded: " + result.IsUnbounded);

                if (result.IsOptimal)
                {
                    writer.WriteLine("Objective Value: " + Math.Round(result.ObjectiveValue, 3));
                    writer.WriteLine("Variable Values: " + string.Join(", ", result.VariableValues.Select(v => Math.Round(v, 3))));
                }
            }
        }
        #endregion

        #region WriteTableau
        private static void WriteTableau(StreamWriter writer, Tableau tableau) 
        {
            writer.Write(tableau.TableNumber.PadRight(10));
            foreach (var col in tableau.ColumnNames)
                writer.Write(col.PadRight(10));
            writer.WriteLine();

            for (int i = 0; i < tableau.Rows.Count; i++)
            {
                writer.Write(tableau.RowNames[i].PadRight(10));
                foreach (var val in tableau.Rows[i])
                {
                    double rounded = Math.Round(val, 3);
                    writer.Write(rounded.ToString("F3").PadRight(10));
                }
                writer.WriteLine();
            }

            writer.WriteLine("Basic Variables: " + string.Join(", ", tableau.BasicVariables));
     
        }
        #endregion

    }
}
