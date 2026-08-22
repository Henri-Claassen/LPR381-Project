using System;

namespace LPR381.Solving
{
    public class ExpressionParser
    {
        private string _expression;
        private int _pos;
        private double[] _variables;

        public double Evaluate(string expression, double[] variables)
        {
            _expression = expression.Replace(" ", "").ToLower();
            _pos = 0;
            _variables = variables;
            return ParseExpression();
        }

        private double ParseExpression()
        {
            double result = ParseTerm();
            while (_pos < _expression.Length)
            {
                char op = _expression[_pos];
                if (op == '+' || op == '-')
                {
                    _pos++;
                    double term = ParseTerm();
                    if (op == '+') result += term;
                    else result -= term;
                }
                else
                {
                    break;
                }
            }
            return result;
        }

        private double ParseTerm()
        {
            double result = ParseFactor();
            while (_pos < _expression.Length)
            {
                char op = _expression[_pos];
                if (op == '*' || op == '/')
                {
                    _pos++;
                    double factor = ParseFactor();
                    if (op == '*') result *= factor;
                    else result /= factor;
                }
                else
                {
                    break;
                }
            }
            return result;
        }

        private double ParseFactor()
        {
            double result = ParsePrimary();
            if (_pos < _expression.Length && _expression[_pos] == '^')
            {
                _pos++;
                double exponent = ParseFactor(); // Right-associative
                result = Math.Pow(result, exponent);
            }
            return result;
        }

        private double ParsePrimary()
        {
            if (_pos >= _expression.Length) return 0;

            if (_expression[_pos] == '(')
            {
                _pos++;
                double result = ParseExpression();
                if (_pos < _expression.Length && _expression[_pos] == ')') _pos++;
                return result;
            }

            if (_expression[_pos] == '-')
            {
                _pos++;
                return -ParsePrimary();
            }
            if (_expression[_pos] == '+')
            {
                _pos++;
                return ParsePrimary();
            }

            if (_expression[_pos] == 'x')
            {
                _pos++;
                int varIdx = 0;
                while (_pos < _expression.Length && char.IsDigit(_expression[_pos]))
                {
                    varIdx = varIdx * 10 + (_expression[_pos] - '0');
                    _pos++;
                }
                if (varIdx > 0 && varIdx <= _variables.Length)
                {
                    return _variables[varIdx - 1]; // 1-based indexing in strings
                }
                return 0; // default if index out of bounds
            }

            int startPos = _pos;
            while (_pos < _expression.Length && (char.IsDigit(_expression[_pos]) || _expression[_pos] == '.'))
            {
                _pos++;
            }
            if (startPos < _pos)
            {
                return double.Parse(_expression.Substring(startPos, _pos - startPos), System.Globalization.CultureInfo.InvariantCulture);
            }

            throw new Exception("Unexpected character: " + _expression[_pos] + " at position " + _pos);
        }
    }
}
