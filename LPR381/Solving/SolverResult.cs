using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPR381.Solving
{
    internal class SolverResult
    {
        public bool IsOptimal { get; set; }
        public bool IsInfeasible { get; set; }
        public bool IsUnbounded { get; set; }
        public double ObjectiveValue { get; set; }
        public double[] VariableValues { get; set; }
        public List<Tableau> IterationHistory { get; set; } = new List<Tableau>();
        public Tableau FinalTableau { get; set; }
        public List<BranchNode> AllNodes { get; set; }   // null unless this came from a B&B run
    }
}
