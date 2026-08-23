using System;
using System.Collections.Generic;

namespace LPR381.Solving
{
    public enum Convexity
    {
        Convex,
        Concave,
        SaddlePoint,
        Inconclusive
    }

    public class NonLinearSolver
    {
        public static double[] CalculateGradient(Func<double[], double> f, double[] x)
        {
            double h = 1e-5;
            double[] grad = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                double[] xPlus = (double[])x.Clone();
                double[] xMinus = (double[])x.Clone();
                xPlus[i] += h;
                xMinus[i] -= h;
                grad[i] = (f(xPlus) - f(xMinus)) / (2 * h);
            }
            return grad;
        }

        public static double[,] CalculateHessian(Func<double[], double> f, double[] x)
        {
            double h = 1e-4;
            int n = x.Length;
            double[,] hessian = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        double[] xPlus = (double[])x.Clone();
                        double[] xMinus = (double[])x.Clone();
                        xPlus[i] += h;
                        xMinus[i] -= h;
                        hessian[i, i] = (f(xPlus) - 2 * f(x) + f(xMinus)) / (h * h);
                    }
                    else
                    {
                        double[] xPlusPlus = (double[])x.Clone();
                        double[] xPlusMinus = (double[])x.Clone();
                        double[] xMinusPlus = (double[])x.Clone();
                        double[] xMinusMinus = (double[])x.Clone();

                        xPlusPlus[i] += h; xPlusPlus[j] += h;
                        xPlusMinus[i] += h; xPlusMinus[j] -= h;
                        xMinusPlus[i] -= h; xMinusPlus[j] += h;
                        xMinusMinus[i] -= h; xMinusMinus[j] -= h;

                        hessian[i, j] = (f(xPlusPlus) - f(xPlusMinus) - f(xMinusPlus) + f(xMinusMinus)) / (4 * h * h);
                    }
                }
            }
            return hessian;
        }

        /// <summary>
        /// Solves a single-variable unconstrained NLP using the Golden Section Search algorithm.
        /// </summary>
        public static double GoldenSectionSearch(Func<double, double> f, double a, double b, double epsilon, bool isMax)
        {
            double r = (Math.Sqrt(5) - 1) / 2;
            double x1 = b - r * (b - a);
            double x2 = a + r * (b - a);
            double f1 = f(x1);
            double f2 = f(x2);

            while (Math.Abs(b - a) > epsilon)
            {
                if (isMax ? (f1 > f2) : (f1 < f2))
                {
                    b = x2;
                    x2 = x1;
                    f2 = f1;
                    x1 = b - r * (b - a);
                    f1 = f(x1);
                }
                else
                {
                    a = x1;
                    x1 = x2;
                    f1 = f2;
                    x2 = a + r * (b - a);
                    f2 = f(x2);
                }
            }
            return (a + b) / 2;
        }

        /// <summary>
        /// Solves an n-variable unconstrained NLP using the Steepest Ascent/Descent algorithm.
        /// </summary>
        public static double[] SteepestAscentDescent(
            Func<double[], double> f,
            Func<double[], double[]> grad,
            double[] initialPoint,
            double epsilon,
            bool isMax)
        {
            double[] x = (double[])initialPoint.Clone();
            int maxIterations = 1000;
            int iterations = 0;

            while (iterations < maxIterations)
            {
                double[] g = grad(x);
                double norm = 0;
                for (int i = 0; i < g.Length; i++) norm += g[i] * g[i];
                if (Math.Sqrt(norm) < epsilon) break;

                double[] d = new double[g.Length];
                for (int i = 0; i < g.Length; i++) d[i] = isMax ? g[i] : -g[i];

                Func<double, double> lineSearchFunc = (alpha) =>
                {
                    double[] nextX = new double[x.Length];
                    for (int i = 0; i < x.Length; i++) nextX[i] = x[i] + alpha * d[i];
                    return f(nextX);
                };

                // Perform line search
                double bestAlpha = GoldenSectionSearch(lineSearchFunc, 0, 10, epsilon, isMax);

                for (int i = 0; i < x.Length; i++) x[i] += bestAlpha * d[i];
                iterations++;
            }
            return x;
        }

        /// <summary>
        /// Determines convexity of a function given its Hessian matrix at a point using principal minors.
        /// </summary>
        public static Convexity DetermineConvexity(double[,] hessian)
        {
            int n = hessian.GetLength(0);
            bool allNonNegative = true;
            bool alternateSigns = true; // Negative semi-definite (k odd <= 0, k even >= 0)

            // We check all principal minors (for semi-definiteness)
            for (int k = 1; k <= n; k++)
            {
                var combinations = GetCombinations(n, k);
                foreach (var indices in combinations)
                {
                    double[,] subMatrix = new double[k, k];
                    for (int i = 0; i < k; i++)
                        for (int j = 0; j < k; j++)
                            subMatrix[i, j] = hessian[indices[i], indices[j]];

                    double det = Determinant(subMatrix);
                    
                    if (det < 0) allNonNegative = false;
                    
                    if (k % 2 == 1 && det > 0) alternateSigns = false; // odd should be <= 0
                    if (k % 2 == 0 && det < 0) alternateSigns = false; // even should be >= 0
                }
            }

            if (allNonNegative && !alternateSigns) return Convexity.Convex;
            if (alternateSigns && !allNonNegative) return Convexity.Concave;
            if (!allNonNegative && !alternateSigns) return Convexity.SaddlePoint;
            
            // If it's the zero matrix, it can be both, but we can default to Inconclusive or Convex
            return Convexity.Inconclusive; 
        }

        private static List<int[]> GetCombinations(int n, int k)
        {
            var result = new List<int[]>();
            int[] combination = new int[k];
            GenerateCombinations(result, combination, 0, n, 0);
            return result;
        }

        private static void GenerateCombinations(List<int[]> result, int[] combination, int start, int n, int index)
        {
            if (index == combination.Length)
            {
                result.Add((int[])combination.Clone());
                return;
            }

            for (int i = start; i < n; i++)
            {
                combination[index] = i;
                GenerateCombinations(result, combination, i + 1, n, index + 1);
            }
        }

        private static double Determinant(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            if (n == 1) return matrix[0, 0];
            if (n == 2) return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];
            if (n == 3)
            {
                return matrix[0, 0] * (matrix[1, 1] * matrix[2, 2] - matrix[1, 2] * matrix[2, 1])
                     - matrix[0, 1] * (matrix[1, 0] * matrix[2, 2] - matrix[1, 2] * matrix[2, 0])
                     + matrix[0, 2] * (matrix[1, 0] * matrix[2, 1] - matrix[1, 1] * matrix[2, 0]);
            }

            double det = 0;
            for (int p = 0; p < n; p++)
            {
                double[,] subMatrix = new double[n - 1, n - 1];
                for (int i = 1; i < n; i++)
                {
                    int jCount = 0;
                    for (int j = 0; j < n; j++)
                    {
                        if (j == p) continue;
                        subMatrix[i - 1, jCount] = matrix[i, j];
                        jCount++;
                    }
                }
                det += Math.Pow(-1, p) * matrix[0, p] * Determinant(subMatrix);
            }
            return det;
        }
    }
}
