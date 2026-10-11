
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Backend;

public static class ExpressionEvaluator
{
    public static double Evalute(string infix) => Evaluate(infix);

    public static double Evaluate(string infix)
    {
        if (string.IsNullOrWhiteSpace(infix))
            throw new ArgumentException("La expresion esta vacia.");

        return EvaluatePostfix(ToPostfix(infix));
    }

    private static List<string> ToPostfix(string infix)
    {
        var output = new List<string>();
        var operators = new Stack<char>();

        int i = 0;
        bool esperaOperando = true;

        while (i < infix.Length)
        {
            char c = infix[i];

            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            if (char.IsDigit(c) || c == '.')
            {
                if (!esperaOperando)
                    throw new ArgumentException(
                        "Falta un operador entre numeros.");

                int inicio = i;
                int puntos = 0;

                while (i < infix.Length &&
                       (char.IsDigit(infix[i]) || infix[i] == '.'))
                {
                    if (infix[i] == '.')
                        puntos++;

                    if (puntos > 1)
                        throw new ArgumentException(
                            "Numero decimal invalido.");

                    i++;
                }

                string numero = infix.Substring(inicio, i - inicio);

                if (numero == "." ||
                    !double.TryParse(
                        numero,
                        NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture,
                        out _))
                {
                    throw new ArgumentException("Numero invalido.");
                }

                output.Add(numero);
                esperaOperando = false;
                continue;
            }

            if (c == '(')
            {
                if (!esperaOperando)
                    throw new ArgumentException(
                        "Falta un operador antes del parentesis.");

                operators.Push(c);
                i++;
                esperaOperando = true;
                continue;
            }

            if (c == ')')
            {
                if (esperaOperando)
                    throw new ArgumentException(
                        "Parentesis vacio o expresion incompleta.");

                while (operators.Count > 0 &&
                       operators.Peek() != '(')
                {
                    output.Add(operators.Pop().ToString());
                }

                if (operators.Count == 0)
                    throw new ArgumentException(
                        "Falta un parentesis de apertura.");

                operators.Pop();
                i++;
                esperaOperando = false;
                continue;
            }

            if (EsOperador(c))
            {
                if (esperaOperando)
                {
                    // Representa el signo negativo como operador unario.
                    if (c != '-')
                        throw new ArgumentException(
                            "Operador en posicion invalida.");

                    operators.Push('~');
                    i++;
                    continue;
                }

                while (operators.Count > 0 &&
                       operators.Peek() != '(' &&
                       (Prioridad(operators.Peek()) > Prioridad(c) ||
                        (Prioridad(operators.Peek()) == Prioridad(c)
                         && c != '^')))
                {
                    output.Add(operators.Pop().ToString());
                }

                operators.Push(c);
                i++;
                esperaOperando = true;
                continue;
            }

            throw new ArgumentException(
                $"Caracter no valido: {c}");
        }

        if (esperaOperando)
            throw new ArgumentException(
                "La expresion esta incompleta.");

        while (operators.Count > 0)
        {
            char op = operators.Pop();

            if (op == '(')
                throw new ArgumentException(
                    "Falta un parentesis de cierre.");

            output.Add(op.ToString());
        }

        return output;
    }
    private static int Prioridad(char op) => op switch
    {
        '^' => 4,
        '~' => 3,
        '*' or '/' => 2,
        '+' or '-' => 1,
        _ => 0
    };


    private static bool EsOperador(char c) =>
        c == '^' || c == '*' || c == '/' ||
        c == '+' || c == '-';

    private static double EvaluatePostfix(List<string> postfix)
    {
        var numbers = new Stack<double>();

        foreach (string token in postfix)
        {
            if (token == "~")
            {
                if (numbers.Count < 1)
                    throw new ArgumentException(
                        "Falta un numero para el signo negativo.");

                numbers.Push(-numbers.Pop());
            }
            else if (token.Length == 1 &&
                     EsOperador(token[0]))
            {
                if (numbers.Count < 2)
                    throw new ArgumentException(
                        "Faltan operandos.");

                double b = numbers.Pop();
                double a = numbers.Pop();

                numbers.Push(Calculate(a, b, token[0]));
            }
            else
            {
                if (!double.TryParse(
                    token,
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out double number))
                {
                    throw new ArgumentException(
                        "Numero invalido: " + token);
                }

                numbers.Push(number);
            }
        }

        if (numbers.Count != 1)
            throw new ArgumentException(
                "La expresion no es valida.");

        double result = numbers.Pop();

        if (double.IsNaN(result) || double.IsInfinity(result))
            throw new ArithmeticException(
                "El resultado no es un numero finito.");

        return result;
    }

    private static double Calculate(double a, double b, char op)
    {
        double resultado = op switch
        {
            '*' => a * b,
            '/' => b == 0
                ? throw new DivideByZeroException(
                    "No se puede dividir entre cero.")
                : a / b,
            '+' => a + b,
            '-' => a - b,
            '^' => Math.Pow(a, b),
            _ => throw new ArgumentException(
                "Operador invalido.")
        };

        if (double.IsNaN(resultado) ||
            double.IsInfinity(resultado))
        {
            throw new ArithmeticException(
                "La operacion no produce un resultado finito.");
        }

        return resultado;
    }
}
