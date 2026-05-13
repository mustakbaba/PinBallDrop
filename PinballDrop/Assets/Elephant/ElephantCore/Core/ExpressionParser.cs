using System;
using System.Collections.Generic;

public class ExpressionParser
{
    private enum TokenType
    {
        Number, String, Boolean, Identifier, Operator, LeftParen, RightParen
    }

    private class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
    }

    private static readonly Dictionary<string, int> OperatorPrecedence = new Dictionary<string, int>
    {
        {"||", 1}, {"&&", 2},
        {"==", 3}, {"!=", 3}, {"<", 4}, {">", 4}, {"<=", 4}, {">=", 4},
        {"!", 5}
    };

    public bool EvaluateExpression(Dictionary<string, object> context, string expression)
    {
        try
        {
            var tokens = Tokenize(expression);
            var output = ShuntingYard(tokens);
            return EvaluateRPN(context, output);
        }
        catch
        {
            return false;
        }
    }

    private List<Token> Tokenize(string expression)
    {
        var tokens = new List<Token>();
        var currentToken = "";
        var inString = false;

        for (var i = 0; i < expression.Length; i++)
        {
            var c = expression[i];

            if (c == '"')
            {
                inString = !inString;
                currentToken += c;
                if (!inString)
                {
                    tokens.Add(new Token { Type = TokenType.String, Value = currentToken.Trim('"') });
                    currentToken = "";
                }
            }
            else if (inString)
            {
                currentToken += c;
            }
            else if (char.IsWhiteSpace(c))
            {
                if (string.IsNullOrEmpty(currentToken)) continue;
                AddToken(tokens, currentToken);
                currentToken = "";
            }
            else if (c == '(' || c == ')')
            {
                if (!string.IsNullOrEmpty(currentToken))
                {
                    AddToken(tokens, currentToken);
                    currentToken = "";
                }
                tokens.Add(new Token { Type = c == '(' ? TokenType.LeftParen : TokenType.RightParen, Value = c.ToString() });
            }
            else if (IsOperatorChar(c))
            {
                if (!string.IsNullOrEmpty(currentToken))
                {
                    AddToken(tokens, currentToken);
                    currentToken = "";
                }
                currentToken += c;
                if (i + 1 < expression.Length && IsOperatorChar(expression[i + 1]))
                {
                    currentToken += expression[++i];
                }
                tokens.Add(new Token { Type = TokenType.Operator, Value = currentToken });
                currentToken = "";
            }
            else
            {
                currentToken += c;
            }
        }

        if (!string.IsNullOrEmpty(currentToken))
        {
            AddToken(tokens, currentToken);
        }

        return tokens;
    }

    private bool IsOperatorChar(char c)
    {
        return c == '&' || c == '|' || c == '=' || c == '!' || c == '<' || c == '>';
    }

    private void AddToken(List<Token> tokens, string token)
    {
        if (OperatorPrecedence.ContainsKey(token))
        {
            tokens.Add(new Token { Type = TokenType.Operator, Value = token });
        }
        else if (token == "true" || token == "false")
        {
            tokens.Add(new Token { Type = TokenType.Boolean, Value = token });
        }
        else if (double.TryParse(token, out _))
        {
            tokens.Add(new Token { Type = TokenType.Number, Value = token });
        }
        else
        {
            tokens.Add(new Token { Type = TokenType.Identifier, Value = token });
        }
    }

    private Queue<Token> ShuntingYard(List<Token> tokens)
    {
        var output = new Queue<Token>();
        var operatorStack = new Stack<Token>();

        foreach (var token in tokens)
        {
            switch (token.Type)
            {
                case TokenType.Number:
                case TokenType.String:
                case TokenType.Boolean:
                case TokenType.Identifier:
                    output.Enqueue(token);
                    break;
                case TokenType.Operator:
                    while (operatorStack.Count > 0 &&
                           operatorStack.Peek().Type == TokenType.Operator &&
                           OperatorPrecedence[operatorStack.Peek().Value] >= OperatorPrecedence[token.Value])
                    {
                        output.Enqueue(operatorStack.Pop());
                    }
                    operatorStack.Push(token);
                    break;
                case TokenType.LeftParen:
                    operatorStack.Push(token);
                    break;
                case TokenType.RightParen:
                    while (operatorStack.Count > 0 && operatorStack.Peek().Type != TokenType.LeftParen)
                    {
                        output.Enqueue(operatorStack.Pop());
                    }
                    if (operatorStack.Count > 0 && operatorStack.Peek().Type == TokenType.LeftParen)
                    {
                        operatorStack.Pop();
                    }
                    break;
            }
        }

        while (operatorStack.Count > 0)
        {
            output.Enqueue(operatorStack.Pop());
        }

        return output;
    }

    private bool EvaluateRPN(Dictionary<string, object> context, Queue<Token> rpn)
    {
        try
        {
            var stack = new Stack<object>();

            foreach (var token in rpn)
            {
                switch (token.Type)
                {
                    case TokenType.Number:
                        stack.Push(double.Parse(token.Value));
                        break;
                    case TokenType.String:
                        stack.Push(token.Value);
                        break;
                    case TokenType.Boolean:
                        stack.Push(bool.Parse(token.Value));
                        break;
                    case TokenType.Identifier:
                        if (!context.TryGetValue(token.Value, out object value))
                        {
                            return false;
                        }
                        stack.Push(value);
                        break;
                    case TokenType.Operator:
                        if (token.Value == "!")
                        {
                            if (stack.Count < 1) return false;
                            var operand = stack.Pop();
                            var result = ApplyOperator(token.Value, null, operand);
                            stack.Push(result);
                        }
                        else
                        {
                            if (stack.Count < 2) return false;
                            var right = stack.Pop();
                            var left = stack.Pop();
                            var result = ApplyOperator(token.Value, left, right);
                            stack.Push(result);
                        }
                        break;
                }
            }

            return stack.Count == 1 && Convert.ToBoolean(stack.Pop());
        }
        catch
        {
            return false;
        }
    }

    private bool ApplyOperator(string op, object left, object right)
    {
        try
        {
            switch (op)
            {
                case "||": return Convert.ToBoolean(left) || Convert.ToBoolean(right);
                case "&&": return Convert.ToBoolean(left) && Convert.ToBoolean(right);
                case "==": return EqualsWithTypeConversion(left, right);
                case "!=": return !EqualsWithTypeConversion(left, right);
                case "<": return Compare(left, right) < 0;
                case ">": return Compare(left, right) > 0;
                case "<=": return Compare(left, right) <= 0;
                case ">=": return Compare(left, right) >= 0;
                case "!": return !Convert.ToBoolean(right);
                default: return false;
            }
        }
        catch
        {
            return false;
        }
    }

    private bool EqualsWithTypeConversion(object left, object right)
    {
        if (IsNumeric(left) && IsNumeric(right))
        {
            var leftDouble = Convert.ToDouble(left);
            var rightDouble = Convert.ToDouble(right);
            return Math.Abs(leftDouble - rightDouble) < double.Epsilon;
        }

        return Equals(left, right);
    }

    private int Compare(object left, object right)
    {
        try
        {
            if (left == null || right == null)
            {
                return Comparer<object>.Default.Compare(left, right);
            }

            if (IsNumeric(left) && IsNumeric(right))
            {
                double leftDouble = Convert.ToDouble(left);
                double rightDouble = Convert.ToDouble(right);
                return leftDouble.CompareTo(rightDouble);
            }

            if (left is IComparable comparableLeft && right is IComparable comparableRight)
            {
                if (left.GetType() == right.GetType())
                {
                    return comparableLeft.CompareTo(right);
                }

                var leftString = Convert.ToString(left);
                var rightString = Convert.ToString(right);
                return string.Compare(leftString, rightString, StringComparison.Ordinal);
            }

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    private bool IsNumeric(object value)
    {
        return value is sbyte
               || value is byte
               || value is short
               || value is ushort
               || value is int
               || value is uint
               || value is long
               || value is ulong
               || value is float
               || value is double
               || value is decimal;
    }
}