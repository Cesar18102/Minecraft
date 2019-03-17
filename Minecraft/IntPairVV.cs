using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    public class IntPairVV : IntPair {

        public IntPairVV(int X, int Y) : base(X, Y) { }

        public override bool Equals(object obj) {

            if (!obj.GetType().Equals(typeof(IntPairVV)))
                return false;

            IntPairVV IP = obj as IntPairVV;

            return (IP.X == X && IP.Y == Y) || (IP.X == Y && IP.Y == X);
        }
    }
}
