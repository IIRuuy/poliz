using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Core.Parser
{
    internal class RequireAnotherTokenException : Exception
    {
        public RequireAnotherTokenException(int pos)
            : base("Ожидался другой токен на позиции: " + pos) { }
        public RequireAnotherTokenException(string msg)
            : base(msg) { }
    }
}