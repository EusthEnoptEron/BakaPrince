using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BakaPrince
{
    class PrinceNotFoundException : Exception
    {
        public PrinceNotFoundException(string msg) : base(msg) { }

    }
}
