using System;
using LPR381.Stored_Info;

namespace LPR381.Solving
{
    public static class CuttingPlaneTest
    {
        public static void RunTest()
        {
            var model = new LpModel
            {
                IsMaximization = true,
                ObjectiveCoefficients = new double[] { 8, 5 },
                SignRestrictions = new string[] { "+", "+" }, // x1, x2 >= 0 and ints
                TempColNames = new string[] { "x1", "x2" }
            };

            model.Constraints.Add(new Constraints
            {
                Coefficients = new double[] { 1, 1 },
                Relation = "<=",
                RHS = 6
            });

            model.Constraints.Add(new Constraints
            {
                Coefficients = new double[] { 9, 5 },
                Relation = "<=",
                RHS = 45
            });

            Solver solver = new Solver();
            SolverResult result = solver.SolveCuttingPlane(model);

            if (!result.IsOptimal)
            {
                throw new Exception("Test Failed: Solution is not optimal.");
            }

            double x1 = result.VariableValues[0];
            double x2 = result.VariableValues[1];
            double z = result.ObjectiveValue;

            // Check if solution is integer
            if (Math.Abs(x1 - Math.Round(x1)) > 1e-5 || Math.Abs(x2 - Math.Round(x2)) > 1e-5)
            {
                throw new Exception($"Test Failed: Variables are not integer. x1={x1}, x2={x2}");
            }

            // Expected solution for Oakfield Corp with Branch and Bound / Cutting Plane:
            // x1 = 5, x2 = 0, z = 40 (Optimal Integer Solution)
            // Wait, let's check max z = 8x1 + 5x2.
            // (5, 0) -> 40. Is (4, 1) -> 37. Is (3, 3) -> 39. Is (0, 6) -> 30.
            // Optimal is x1=5, x2=0, max Z=40.
            
            if (Math.Abs(x1 - 5) > 1e-5 || Math.Abs(x2 - 0) > 1e-5 || Math.Abs(z - 40) > 1e-5)
            {
                throw new Exception($"Test Failed: Incorrect optimal values. Expected x1=5, x2=0, z=40. Got x1={x1}, x2={x2}, z={z}");
            }

            System.Windows.Forms.MessageBox.Show("Cutting Plane Algorithm Test Passed Successfully!\nx1 = 5, x2 = 0, z = 40", "Unit Test");
        }
    }
}
