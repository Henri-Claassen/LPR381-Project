using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPR381.Stored_Info
{
    internal class LpModel
    {
        public bool IsMaximization { get; set; }
        public double[] ObjectiveCoefficients { get; set; }
        public List<Constraints> Constraints { get; set; } = new List<Constraints>();
        public string[] SignRestrictions { get; set; }   // "+","-","urs","int","bin" per variable
        public string[] TempColNames { get; set; } //This is used in sign restrictions to give all the column names and then it is added to tableau later in buildcanonicalform
        public int DecisionVariableCount => ObjectiveCoefficients.Length;
    }
}
