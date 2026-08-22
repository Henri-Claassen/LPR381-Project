using System;

namespace LPR381.Stored_Info
{
    public class NlpModel
    {
        public bool IsMaximization { get; set; }
        public string ObjectiveFunction { get; set; }
        public double[] InitialPoint { get; set; }
    }
}
