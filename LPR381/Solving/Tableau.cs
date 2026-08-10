using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPR381.Solving
{
    internal class Tableau
    {
        public List<List<double>> Rows { get; set; } = new List<List<double>>();
        public List<string> ColumnNames { get; set; } = new List<string>();
        public List<string> RowNames { get; set; } = new List<string>();
        public List<string> BasicVariables { get; set; } = new List<string>();
        public string TableNumber { get; set; }
        public bool IsMaximization { get; set; }
    }
}
