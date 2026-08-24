using LPR381.Stored_Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        //Repivot code, that uses the existing solver code in order for it to pivot off of that starting at the given tableau instead of building the model
        #region ContinueFromEditedRegion
        public SolverResult SolveFromEditedTableau(Tableau editedTable, int decisionVariableCount)
        {
            Tableau table = CloneTableau(editedTable);
            table.TableNumber = "t-i";
            var history = new List<Tableau> { CloneTableau(table) };

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
                    return result;
                }
            }

            var (isOptimal, isUnbounded) = RunPrimalLoop(table, history);

            result.FinalTableau = table;
            result.IterationHistory = history;
            result.IsUnbounded = isUnbounded;
            result.IsOptimal = isOptimal;
            if (!isUnbounded)
            {
                result.ObjectiveValue = table.Rows[0][rhsCol];
                result.VariableValues = new double[decisionVariableCount];
                for (int j = 0; j < decisionVariableCount; j++)
                {
                    int basicRowIndex = table.BasicVariables.IndexOf(table.ColumnNames[j]);
                    result.VariableValues[j] = basicRowIndex == -1 ? 0 : table.Rows[basicRowIndex][rhsCol];
                }
            }

            return result;
        }
        #endregion

        public SolverResult SolveRevisedSimplex(LpModel model) { /* TODO */ return null; }
        #region SolveBranchAndBound
        public SolverResult SolveBranchAndBound(LpModel model) {
            double incumbent = model.IsMaximization ? double.NegativeInfinity : double.PositiveInfinity;
             var integerVarIndices = GetIntegerVariableIndices(model); // only these should trigger branching
            BranchNode rootNode = new BranchNode {
                SubProblemModel = model,
                Parent = null,
                BranchDescription = "Root",
                IsFathomed = false,
                FathomReason = null,
                SubProblemResult = SolvePrimalSimplex(model)
            };

            var result = new SolverResult { AllNodes = new List<BranchNode> { rootNode } };
            result.AllNodes = new List<BranchNode> { rootNode };
            if (!rootNode.SubProblemResult.IsOptimal){
                result.IsInfeasible = rootNode.SubProblemResult.IsInfeasible;
                result.IsUnbounded = rootNode.SubProblemResult.IsUnbounded;
                return result;
            }

            var (isIntegerFeasible, fathomReason, objectiveValue) =
                TestBranchSolution(rootNode.SubProblemResult, incumbent, model.IsMaximization, integerVarIndices);

            if (isIntegerFeasible){
                rootNode.IsFathomed = true;
                rootNode.FathomReason = fathomReason;
                incumbent = objectiveValue;
                result.ObjectiveValue = objectiveValue;
                result.VariableValues = rootNode.SubProblemResult.VariableValues;
                return result; // nothing left to branch on
            }
                else
                {
                     var nodesToSolve = new List<BranchNode> { rootNode };
                    List<BranchNode> currentNodes = new List<BranchNode> { rootNode };
                    while (nodesToSolve.Count>0)
                    {
                        var node = nodesToSolve[0];
                    nodesToSolve.RemoveAt(0); // BFS; use a Stack<BranchNode> instead for DFS if you prefer

                    int branchVarIndex = -1;
                    double branchValue = 0, bestFractionalDistance = double.MaxValue;

                    foreach (int j in integerVarIndices)
                    {
                        if (j == -1) continue; // placeholder for non-integer vars, skip
                        double value = node.SubProblemResult.VariableValues[j];
                        if (Math.Abs(value - Math.Round(value)) < 1e-9) continue; // treat as integer within tolerance

                        double dist = Math.Abs(value - Math.Floor(value) - 0.5);
                        if (dist < bestFractionalDistance)
                        {
                            bestFractionalDistance = dist;
                            branchVarIndex = j;
                            branchValue = value;
                        }
                    }

                    if (branchVarIndex == -1)
                    {
                        node.IsFathomed = true;
                        node.FathomReason = "No fractional integer variable found (unexpected)";
                        continue;
                    }

                    var leftNode = new BranchNode
                    {
                        Parent = node,
                        BranchDescription = $"x{branchVarIndex + 1} <= {Math.Floor(branchValue)}",
                        IsFathomed = false
                    };
                    var rightNode = new BranchNode
                    {
                        Parent = node,
                        BranchDescription = $"x{branchVarIndex + 1} >= {Math.Ceiling(branchValue)}",
                        IsFathomed = false
                    };

                    SolveChildNode(node, leftNode, branchVarIndex, branchValue, true, model.DecisionVariableCount);
                    

                    result.AllNodes.Add(leftNode);

                    SolveChildNode(node, rightNode, branchVarIndex, branchValue, false, model.DecisionVariableCount);

                    result.AllNodes.Add(rightNode);



                    foreach (var child in new[] { leftNode, rightNode })
                    {
                        if (child.IsFathomed) continue; // infeasible 

                        var (childIsInt, childReason, childObj) =
                            TestBranchSolution(child.SubProblemResult, incumbent, model.IsMaximization, integerVarIndices);

                        bool better = model.IsMaximization ? childObj > incumbent : childObj < incumbent;

                        if (!better)
                        {
                            child.IsFathomed = true;
                            child.FathomReason = "worse than incumbent";
                            continue;
                        }

                        if (childIsInt )
                        {
                            child.IsFathomed = true;
                            child.FathomReason = "integer solution";
                            if (better)
                            {
                                incumbent = childObj;
                                result.ObjectiveValue = childObj;
                                result.VariableValues = child.SubProblemResult.VariableValues;
                            }
                        }
                        else if (!childIsInt)
                        {
                            nodesToSolve.Add(child); // keep branching
                        }
                    }
                }
                result.IsOptimal = incumbent != (model.IsMaximization ? double.NegativeInfinity : double.PositiveInfinity);
                return result;
            }

            
             
        }
        private void SolveChildNode(BranchNode parent, BranchNode child, int branchVarIndex,
                             double branchValue, bool isLeftBranch, int decisionVarCount)
        {
            Tableau nodeTableau = CloneTableau(parent.SubProblemResult.FinalTableau); // don't mutate parent!
            AddBranchConstraint(nodeTableau, branchVarIndex, branchValue, isLeftBranch);

            var history = new List<Tableau> { CloneTableau(nodeTableau) }; // t-i for THIS node only

            bool infeasible = DualSimplex(nodeTableau, history); // appends only this node's pivots

            var (optimal, unbounded) = RunPrimalLoop(nodeTableau, history); // run primal simplex after dual

            if (unbounded) {
                child.IsFathomed = true;
                child.FathomReason = "unbounded";
                child.SubProblemResult = new SolverResult
                {
                    IsOptimal = false,
                    IsUnbounded = true,
                    FinalTableau = nodeTableau,
                    IterationHistory = history
                };
                return;
            }


            // Dual simplex only touches RHS feasibility, never the objective row,
            // so once it terminates feasible, the node is also optimal — no primal cleanup needed.
            int rhsCol = nodeTableau.Rows[0].Count - 1;
            //MessageBox.Show($"Node {child.BranchDescription} solved. Objective value: {nodeTableau.Rows[0][rhsCol]}");
            child.SubProblemResult = new SolverResult
            {
                IsOptimal = true,
                FinalTableau = history[history.Count-1],
                IterationHistory = history,
                ObjectiveValue = history[history.Count-1].Rows[0][rhsCol],
                VariableValues = ExtractSolution(history[history.Count-1], decisionVarCount)
            };
        }
        private double[] ExtractSolution(Tableau table, int decisionVarCount)
        {
            int rhsCol = table.Rows[0].Count - 1;
            var values = new double[decisionVarCount];
            for (int j = 0; j < decisionVarCount; j++)
            {
                int r = table.BasicVariables.IndexOf(table.ColumnNames[j]);
                values[j] = r == -1 ? 0 : table.Rows[r][rhsCol];
            }
            return values;
        }
        private void AddBranchConstraint(Tableau table, int variableIndex, double value, bool isLeftBranch)
        {
            string colName = "x" + (variableIndex + 1);
            int varCol = table.ColumnNames.IndexOf(colName);
            if (varCol == -1){ 
                throw new InvalidOperationException("Branching variable column not found in tableau.");
            }

            int rowIndex = table.BasicVariables.IndexOf(colName);
            if (rowIndex == -1){
                throw new InvalidOperationException("Branching variable is not basic — can't branch on it.");
            }

            double bound = isLeftBranch ? Math.Floor(value) : Math.Ceiling(value);
            double newRhs = isLeftBranch ? bound - value : value - bound; // always <= 0

            // 1. Insert a new slack/excess column, before RHS, into EVERY existing row
            int insertPos = table.ColumnNames.Count - 1;
            string varName = (isLeftBranch ? "s" : "e") + table.ColumnNames.Count;
            table.ColumnNames.Insert(insertPos, varName);
            foreach (var row in table.Rows){
                row.Insert(insertPos, 0.0);
            }

            // 2. Build the new row from the branching variable's basic row
            var basicRow = table.Rows[rowIndex]; // already has the new 0 column from step 1
            var newRow = new List<double>();
            for (int k = 0; k < basicRow.Count - 1; k++) // exclude RHS
            {
                if (k == varCol){
                    newRow.Add(0.0);
                }
                else if (k == insertPos){     
                    newRow.Add(1.0);  // this row's own new slack
                }
                else{
                    newRow.Add(isLeftBranch ? -basicRow[k] : basicRow[k]);
                }
            }
            newRow.Add(newRhs);

            table.Rows.Add(newRow);
            table.BasicVariables.Add(varName); // new slack is basic in the new row
            table.RowNames.Add("c" + table.RowNames.Count);
        }

        private (bool, string, double) TestBranchSolution(SolverResult r, double incumbent,
            bool isMax, List<int> integerVarIndices)
        {
            bool isIntegerFeasible = true;
            foreach (int j in integerVarIndices)
            {
                if (j == -1) continue;
                double v = r.VariableValues[j];
                if (Math.Abs(v - Math.Round(v)) > 1e-9) { isIntegerFeasible = false; break; }
            }
            return (isIntegerFeasible, isIntegerFeasible ? "integer solution" : "fractional", r.ObjectiveValue);
        }
        #endregion
        #region SolveKnapsackBranchAndBound
        public SolverResult SolveKnapsackBranchAndBound(LpModel model) { 
            var history = new List<KnapsackSubproblemTable>();

            if (!IsKnapsackModel(model)) throw new InvalidOperationException("Model is not a valid knapsack problem.");
            
            var items = BuildKnapsackItems(model);
            double capacity = model.Constraints[0].RHS;

            double incumbent = double.NegativeInfinity;
            double[] incumbentValues = null;

            var rootNode = new BranchNode
            {
                SubProblemModel = model,
                Parent = null,
                BranchDescription = "Knapsack Root",
                IsFathomed = false
            };

            var result = new SolverResult
            {
                AllNodes = new List<BranchNode> { rootNode },
                KnapsackHistory = history,
                IsUnbounded = false,
                IsInfeasible = false
            };

            var nodesToSolve = new List<BranchNode> { rootNode };

            while (nodesToSolve.Count > 0)
            {
                var node = nodesToSolve[0];
                nodesToSolve.RemoveAt(0);

                // Set variBle values for fixed-in and fixed-out items, and compute remaining capacity
                double fixedValue = 0;
                double remainingCapacity = capacity;
                var candidateItems = new List<KnapsackItem>();

                foreach (var item in items)
                {
                    //use the original index to check if the item is fixed in or out
                    if (node.FixedIn.Contains(item.originalIndex))
                    {
                        fixedValue += item.value; //build rhs value for the knapsack problem
                        remainingCapacity -= item.weight; //build the remaining capacity for the knapsack problem
                    }
                    else if (!node.FixedOut.Contains(item.originalIndex))
                    {
                        candidateItems.Add(item); //only add items that are not fixed out to the candidate list
                    }
                }

                // Infeasible if fixed-in items alone exceed capacity
                if (remainingCapacity < 0)
                {
                    node.IsFathomed = true;
                    node.FathomReason = "infeasible";
                    continue;
                }

                var (freeValues, freeValue, isFractional, branchVarIndex, table) =
                    RunGreedyFill(candidateItems, remainingCapacity, node.BranchDescription, model.DecisionVariableCount);

                double nodeObjective = fixedValue + freeValue;

                // Merge fixed-in decisions (=1) into the full solution vector
                var fullValues = (double[])freeValues.Clone();
                foreach (int idx in node.FixedIn) fullValues[idx] = 1;
                foreach (int idx in node.FixedOut) fullValues[idx] = 0;

                table.ObjectiveValue = nodeObjective; // include fixed contribution in the displayed total
                history.Add(table);
                node.KnapsackTable = table;
                node.BranchVariableIndex = branchVarIndex;
                node.SubProblemResult = new SolverResult { IsOptimal = true, ObjectiveValue = nodeObjective, VariableValues = fullValues };

                // Bound: prune if this node can't beat the incumbent
                if (nodeObjective <= incumbent)
                {
                    node.IsFathomed = true;
                    node.FathomReason = "worse than incumbent";
                    continue;
                }

                if (!isFractional)
                {
                    node.IsFathomed = true;
                    node.FathomReason = "integer solution";
                    incumbent = nodeObjective;
                    incumbentValues = fullValues;
                    continue;
                }
                
                // Branch on branchVarIndex
                var excludeNode = new BranchNode
                {
                    Parent = node,
                    BranchDescription = $"x{branchVarIndex + 1} = 0",
                    FixedIn = new HashSet<int>(node.FixedIn),
                    FixedOut = new HashSet<int>(node.FixedOut) { branchVarIndex }
                };
                var includeNode = new BranchNode
                {
                    Parent = node,
                    BranchDescription = $"x{branchVarIndex + 1} = 1",
                    FixedIn = new HashSet<int>(node.FixedIn) { branchVarIndex },
                    FixedOut = new HashSet<int>(node.FixedOut)
                };

                result.AllNodes.Add(excludeNode);
                result.AllNodes.Add(includeNode);
                nodesToSolve.Add(excludeNode);
                nodesToSolve.Add(includeNode);
            }

            result.IsOptimal = true;
            result.ObjectiveValue = incumbent;
            result.VariableValues = incumbentValues;
            return result;
        }

        private List<KnapsackItem> BuildKnapsackItems(LpModel model)
        {
            var items = new List<KnapsackItem>();
            for (int i = 0; i < model.DecisionVariableCount; i++)
            {
                items.Add(new KnapsackItem
                {
                    originalIndex = i,
                    value = model.ObjectiveCoefficients[i],
                    weight = model.Constraints[0].Coefficients[i]
                });
            }
            return items;
        }

        private (double[] variableValues, double totalValue, bool isFractional, int branchVarIndex, KnapsackSubproblemTable table)
        RunGreedyFill(List<KnapsackItem> candidateItems, double capacity, string nodeDescription, int totalItemCount){
            var sortedItems = candidateItems.OrderByDescending(item => item.weight == 0 ? double.MaxValue : item.value / item.weight).ToList();

            var variableValues = new double[totalItemCount];
            double totalValue = 0;
            double remainingCapacity = capacity;
            bool fraction = false;
            int branchVarIndex = -1;

            var table = new KnapsackSubproblemTable
            {
                NodeDescription = nodeDescription,
                Capacity = capacity
            };

            foreach (var item in sortedItems)//iterate through the sorted items and fill the knapsack greedily
            {
                double decision;
                string status;

                if (remainingCapacity <= 0)
                {
                    decision = 0;
                    status = "excluded";
                }
                else if (item.weight <= remainingCapacity)
                {
                    decision = 1;
                    totalValue += item.value;
                    remainingCapacity -= item.weight;
                    status = "taken";
                }
                else
                {
                    decision = remainingCapacity / item.weight;
                    totalValue += decision * item.value;
                    fraction = true;
                    branchVarIndex = item.originalIndex;
                    remainingCapacity = 0;
                    status = "fractional";
                }

                variableValues[item.originalIndex] = decision; // store the decision for this item in the full solution vector

                table.Rows.Add(new KnapsackTableRow //build the knapsack table row for this item
                {
                    ItemName = "x" + (item.originalIndex + 1),
                    Value = item.value,
                    Weight = item.weight,
                    Ratio = item.weight == 0 ? double.PositiveInfinity : item.value / item.weight,
                    Decision = decision,
                    RemainingCapacityAfter = remainingCapacity,
                    Status = status
                });
            }

            table.ObjectiveValue = totalValue;
            table.BranchVariableIndex = branchVarIndex;

            return (variableValues, totalValue, fraction, branchVarIndex, table);
        }
        public bool IsKnapsackModel(LpModel model) {
            if (model.Constraints.Count != 1 || !model.IsMaximization) return false;
            var constraint = model.Constraints[0];
            if (constraint.Relation != "<=") return false;
            if (model.SignRestrictions.Any(s => s != "bin")) return false;
            return true;
        }
            
        #endregion

        public SolverResult SolveCuttingPlane(LpModel model)
        {
            SolverResult currentResult = SolvePrimalSimplex(model);

            while (currentResult.IsOptimal && !currentResult.IsInfeasible && !currentResult.IsUnbounded)
            {
                bool allIntegers = true;
                int targetVarIndex = -1;
                double minDistanceToHalf = double.MaxValue;

                for (int j = 0; j < model.DecisionVariableCount; j++)
                {
                    double val = currentResult.VariableValues[j];
                    double fractionalPart = GetFractionalPart(val);

                    if (fractionalPart > 1e-5) // Not an integer
                    {
                        allIntegers = false;
                        
                        double dist = Math.Abs(fractionalPart - 0.5);
                        
                        // Tie breaker: lower subscript (lower j). Since we iterate j=0,1,2..., 
                        // strictly less (<) naturally keeps the lower subscript in case of a tie.
                        if (dist < minDistanceToHalf - 1e-6) 
                        {
                            minDistanceToHalf = dist;
                            targetVarIndex = j;
                        }
                    }
                }

                if (allIntegers)
                {
                    return currentResult; // Optimal integer solution found!
                }

                Tableau t = currentResult.FinalTableau;
                
                // If sign restrictions added temp col names like x1', x1" this needs care, but VariableValues[j] corresponds to TempColNames mapping.
                // Assuming targetVarIndex corresponds to ColumnNames[targetVarIndex] since they are added first.
                string varName = t.ColumnNames[targetVarIndex];
                int rowIdx = t.BasicVariables.IndexOf(varName);

                if (rowIdx == -1) 
                    break; // Failsafe

                int totalCols = t.Rows[0].Count;
                List<double> cutRow = new List<double>(new double[totalCols + 1]); 
                
                int cutColIndex = totalCols - 1; // Insert before RHS
                
                t.ColumnNames.Insert(cutColIndex, "s_cut_" + (t.Rows.Count));
                
                for (int i = 0; i < t.Rows.Count; i++)
                {
                    t.Rows[i].Insert(cutColIndex, 0.0);
                }
                
                totalCols++;
                int rhsCol = totalCols - 1;
                
                double b = t.Rows[rowIdx][rhsCol];
                double b_frac = GetFractionalPart(b);
                
                for (int j = 0; j < totalCols; j++)
                {
                    if (j == cutColIndex)
                    {
                        cutRow[j] = 1.0; 
                    }
                    else if (j == rhsCol)
                    {
                        cutRow[j] = -b_frac;
                    }
                    else
                    {
                        double a = t.Rows[rowIdx][j];
                        double a_frac = GetFractionalPart(a);
                        cutRow[j] = -a_frac;
                    }
                }
                
                t.Rows.Add(cutRow);
                t.RowNames.Add("cut_" + (t.Rows.Count - 1));
                t.BasicVariables.Add(t.ColumnNames[cutColIndex]);
                
                t.TableNumber = "t-" + (currentResult.IterationHistory.Count + 1);
                currentResult.IterationHistory.Add(CloneTableau(t));
                
                bool infeasible = DualSimplex(t, currentResult.IterationHistory);
                if (infeasible)
                {
                    currentResult.IsInfeasible = true;
                    currentResult.IsOptimal = false;
                    return currentResult;
                }
                
                var (isOptimal, isUnbounded) = RunPrimalLoop(t, currentResult.IterationHistory);
                
                currentResult.FinalTableau = t;
                currentResult.IsUnbounded = isUnbounded;
                currentResult.IsOptimal = isOptimal;
                
                if (isOptimal && !isUnbounded)
                {
                    currentResult.ObjectiveValue = t.Rows[0][rhsCol];
                    for (int j = 0; j < model.DecisionVariableCount; j++)
                    {
                        int basicRowIndex = t.BasicVariables.IndexOf(t.ColumnNames[j]);
                        currentResult.VariableValues[j] = basicRowIndex == -1 ? 0 : t.Rows[basicRowIndex][rhsCol];
                    }
                }
            }

            return currentResult;
        }

        private double GetFractionalPart(double v)
        {
            double f = v - Math.Floor(v + 1e-8);
            if (f > 1 - 1e-8) f = 0;
            return f;
        }


        
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

        private List<int> GetIntegerVariableIndices(LpModel model) {
            int count = model.SignRestrictions.Length;
            var indices = new List<int>();
            for (int i = 0; i < count; i++)
            {
                if (model.SignRestrictions[i] == "int")
                {
                    indices.Add(i);
                }
                else if (model.SignRestrictions[i] == "bin")
                {
                    indices.Add(i);
                }
                else
                {
                    indices.Add(-1); // Placeholder for non-integer variables
                }
            }
            return indices;
        }

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
