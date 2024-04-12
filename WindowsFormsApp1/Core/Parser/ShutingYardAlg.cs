using Antlr4.Runtime;
using System.Collections.Generic;
using System.Text;

namespace WindowsFormsApp1.Core.Parser
{
    public class SemanticResult
    {
        public string Operations { get; set; }
        public double Result { get; set; }
    }

    public class ShutingYardAlg
    {
        Stack<double> output = new Stack<double>();
        Stack<int> input = new Stack<int>();
        CommonTokenStream tokens;
        StringBuilder expression = new StringBuilder();

        public ShutingYardAlg(CommonTokenStream tokens)
        {
            this.tokens = tokens;
        }

        public SemanticResult run()
        {
            SemanticResult resultSemantic = new SemanticResult();
            for (int i = 0; i < tokens.Size; i++)
            {
                IToken token = tokens.Get(i);
                if (token.Type == 1)
                {
                    output.Push(double.Parse(token.Text));
                    expression.Append(token.Text).Append(" ");
                }
                if (token.Type >= 2 && token.Type <= 5)
                {
                    while (input.Count > 0 && (input.Peek() >= 2 && input.Peek() <= 5) && token.Type < 4)
                    {
                        executeOperator(input.Pop());
                    }
                    input.Push(token.Type);
                }
                if(token.Type == 6)
                    input.Push(token.Type);

                if (token.Type == 7)
                {
                    while(input.Peek() != 6)
                    {
                        executeOperator(input.Pop());
                    }
                    input.Pop();
                }

            }
            while (input.Count > 0)
            {
                executeOperator(input.Pop());
            }
            resultSemantic.Result = output.Pop();
            resultSemantic.Operations = expression.ToString();
            return resultSemantic;
        }

        public void executeOperator(int op)
        {
            double b = output.Pop();
            double a = output.Pop();

            if (op == 2)
            {
                output.Push(a + b);
                expression.Append("+ ");
            }
            else if (op == 3)
            {
                output.Push(a - b);
                expression.Append("- ");
            }
            else if (op == 4)
            {
                output.Push(a * b);
                expression.Append("* ");
            }
            else if (op == 5)
            {
                output.Push(a / b);
                expression.Append("/ ");
            }
        }
    }

}

