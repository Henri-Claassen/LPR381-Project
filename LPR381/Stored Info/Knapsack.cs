using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPR381.Stored_Info
{
    internal class KnapsackTableRow
    {
        public string ItemName { get; set; }
        public double Value { get; set; }
        public double Weight { get; set; }
        public double Ratio { get; set; }
        public double Decision { get; set; }       // 0, 1, or fraction
        public double RemainingCapacityAfter { get; set; }
        public String Status { get; set; } // not used, used, fractional
    }
    internal class KnapsackSubproblemTable
    {
        public string NodeDescription { get; set; }      // matches BranchNode.BranchDescription
        public double Capacity { get; set; }
        public List<KnapsackTableRow> Rows { get; set; } = new List<KnapsackTableRow>();
        public double ObjectiveValue { get; set; }
        public int BranchVariableIndex { get; set; } = -1; // -1 if none needed
    }
    internal class KnapsackItem
    {
        public int originalIndex;
        public double value;
        public double weight;
    }
}