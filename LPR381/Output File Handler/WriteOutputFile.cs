using LPR381.Solving;
using LPR381.Stored_Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPR381.Output_File_Handler
{
    internal class WriteOutputFile
    {
        #region WriteResultToFile
        public static void WriteResultToFile(SolverResult result, string outputFilePath)
        {
            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                if (result.AllNodes != null && result.AllNodes.Count > 0)
                {
                    WriteBranchAndBoundResult(writer, result);
                }
                else
                {
                    WriteSimplexResult(writer, result);
                }
            }
        }
        #endregion

        #region AppendResultToFile
        // Used by sensitivity analysis "apply change" operations so every change made
        // after the initial solve is appended to the same output file, not overwritten.
        public static void AppendResultToFile(SolverResult result, string outputFilePath, string header)
        {
            using (StreamWriter writer = new StreamWriter(outputFilePath, append: true))
            {
                writer.WriteLine();
                writer.WriteLine("=== " + header + " ===");
                writer.WriteLine();

                if (result.AllNodes != null && result.AllNodes.Count > 0)
                {
                    WriteBranchAndBoundResult(writer, result);
                }
                else
                {
                    WriteSimplexResult(writer, result);
                }
            }
        }
        #endregion

        #region AppendTextToFile
        // Used for sensitivity operations that don't produce a full SolverResult
        // (ranges, shadow prices, duality strength, the dual model description).
        public static void AppendTextToFile(string outputFilePath, string text)
        {
            using (StreamWriter writer = new StreamWriter(outputFilePath, append: true))
            {
                writer.WriteLine();
                writer.WriteLine(text);
            }
        }
        #endregion

        #region WriteSimplexResult
        private static void WriteSimplexResult(StreamWriter writer, SolverResult result)
        {
            writer.WriteLine("=== Iteration History ===");
            writer.WriteLine();
            if (result.IterationHistory != null)
            {
                foreach (var table in result.IterationHistory)
                {
                    WriteTableau(writer, table);
                    writer.WriteLine();
                }
            }
            WriteFinalResult(writer, result);
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

        #region WriteBranchAndBoundResult
        private static void WriteBranchAndBoundResult(StreamWriter writer, SolverResult result)
        {
            writer.WriteLine("=== Branch and Bound Tree ===");
            writer.WriteLine();

            var (labels, childrenByParent) = BuildTree(result.AllNodes);
            var root = result.AllNodes.FirstOrDefault(n => n.Parent == null);

            if (root != null)
            {
                WriteNodeRecursive(writer, root, labels, childrenByParent);
            }

            writer.WriteLine();
            WriteFinalResult(writer, result);
        }
        #endregion

        #region BuildTree / AssignChildLabels
        // Groups nodes by parent and assigns hierarchical labels like "1", "1.1", "2.2.1"
        public static (Dictionary<BranchNode, string> labels, Dictionary<BranchNode, List<BranchNode>> childrenByParent)
            BuildTree(List<BranchNode> allNodes)
        {
            var childrenByParent = allNodes
                .Where(n => n.Parent != null)
                .GroupBy(n => n.Parent)
                .ToDictionary(g => g.Key, g => g.ToList());

            var labels = new Dictionary<BranchNode, string>();
            var root = allNodes.FirstOrDefault(n => n.Parent == null);

            if (root != null)
            {
                labels[root] = "Root";
                AssignChildLabels(root, "", labels, childrenByParent);
            }

            return (labels, childrenByParent);
        }

        private static void AssignChildLabels(BranchNode node, string parentLabel,
            Dictionary<BranchNode, string> labels, Dictionary<BranchNode, List<BranchNode>> childrenByParent)
        {
            if (!childrenByParent.TryGetValue(node, out var children)) return;

            for (int i = 0; i < children.Count; i++)
            {
                string childLabel = string.IsNullOrEmpty(parentLabel)
                    ? (i + 1).ToString()
                    : parentLabel + "." + (i + 1);

                labels[children[i]] = childLabel;
                AssignChildLabels(children[i], childLabel, labels, childrenByParent);
            }
        }
        #endregion

        #region WriteNodeRecursive
        private static void WriteNodeRecursive(StreamWriter writer, BranchNode node,
            Dictionary<BranchNode, string> labels, Dictionary<BranchNode, List<BranchNode>> childrenByParent)
        {
            writer.WriteLine($"--- Node {labels[node]}: {node.BranchDescription} ---");
            writer.WriteLine("Fathomed: " + node.IsFathomed +
                (node.FathomReason != null ? " (" + node.FathomReason + ")" : ""));

            if (node.KnapsackTable != null)
            {
                WriteKnapsackTable(writer, node.KnapsackTable);
            }
            else if (node.SubProblemResult != null)
            {
                if (node.SubProblemResult.IterationHistory != null)
                {
                    foreach (var table in node.SubProblemResult.IterationHistory)
                    {
                        WriteTableau(writer, table);
                        writer.WriteLine();
                    }
                }

                if (node.SubProblemResult.IsOptimal && node.SubProblemResult.VariableValues != null)
                {
                    writer.WriteLine("Node Objective: " + Math.Round(node.SubProblemResult.ObjectiveValue, 3));
                    writer.WriteLine("Node Variables: " +
                        string.Join(", ", node.SubProblemResult.VariableValues.Select(v => Math.Round(v, 3))));
                }
            }
            else
            {
                writer.WriteLine("(no subproblem data — fathomed before solving)");
            }

            writer.WriteLine();

            if (childrenByParent.TryGetValue(node, out var children))
            {
                foreach (var child in children)
                    WriteNodeRecursive(writer, child, labels, childrenByParent);
            }
        }
        #endregion

        #region WriteKnapsackTable
        private static void WriteKnapsackTable(StreamWriter writer, KnapsackSubproblemTable table)
        {
            writer.WriteLine($"Knapsack Table - Capacity: {Math.Round(table.Capacity, 3)}");
            writer.Write("Item".PadRight(10));
            writer.Write("Value".PadRight(10));
            writer.Write("Weight".PadRight(10));
            writer.Write("Ratio".PadRight(10));
            writer.Write("Decision".PadRight(10));
            writer.Write("RemCap".PadRight(10));
            writer.WriteLine("Status");

            foreach (var row in table.Rows)
            {
                writer.Write(row.ItemName.PadRight(10));
                writer.Write(Math.Round(row.Value, 3).ToString("F3").PadRight(10));
                writer.Write(Math.Round(row.Weight, 3).ToString("F3").PadRight(10));
                writer.Write((double.IsPositiveInfinity(row.Ratio) ? "Inf" : Math.Round(row.Ratio, 3).ToString("F3")).PadRight(10));
                writer.Write(Math.Round(row.Decision, 3).ToString("F3").PadRight(10));
                writer.Write(Math.Round(row.RemainingCapacityAfter, 3).ToString("F3").PadRight(10));
                writer.WriteLine(row.Status);
            }

            writer.WriteLine("Objective Value: " + Math.Round(table.ObjectiveValue, 3));
            if (table.BranchVariableIndex != -1)
                writer.WriteLine("Branch Variable: x" + (table.BranchVariableIndex + 1));
        }
        #endregion

        #region WriteFinalResult
        private static void WriteFinalResult(StreamWriter writer, SolverResult result)
        {
            writer.WriteLine("=== Final Result ===");
            writer.WriteLine("Optimal: " + result.IsOptimal);
            writer.WriteLine("Infeasible: " + result.IsInfeasible);
            writer.WriteLine("Unbounded: " + result.IsUnbounded);
            if (result.IsOptimal && result.VariableValues != null)
            {
                writer.WriteLine("Objective Value: " + Math.Round(result.ObjectiveValue, 3));
                writer.WriteLine("Variable Values: " +
                    string.Join(", ", result.VariableValues.Select(v => Math.Round(v, 3))));
            }
        }
        #endregion
    }
}
