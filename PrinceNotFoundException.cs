using System;

namespace BakaPrince
{
    class PrinceNotFoundException : Exception
    {
        public PrinceNotFoundException(string msg) : base(msg) { }

    }
}
