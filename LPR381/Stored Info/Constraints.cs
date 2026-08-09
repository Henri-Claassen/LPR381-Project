using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPR381.Stored_Info
{
    internal class Constraints
    {
        public double[] Coefficients { get; set; }
        public string Relation { get; set; }   // "<=", ">=", or "="
        public double RHS { get; set; }
    }
}
