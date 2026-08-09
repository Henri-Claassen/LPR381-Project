using LPR381.Stored_Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPR381.Solving
{
    internal class BranchNode
    {
        public LpModel SubProblemModel { get; set; }
        public SolverResult SubProblemResult { get; set; }
        public BranchNode Parent { get; set; }
        public string BranchDescription { get; set; }   // e.g. "x2 <= 3"
        public bool IsFathomed { get; set; }
        public string FathomReason { get; set; }         // "infeasible", "integer solution", "worse than incumbent"
    }
}
