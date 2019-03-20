using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft.Support {

    public class IntPair : IComparable {

        public int X { get; set; }
        public int Y { get; set; }

        public IntPair(int X, int Y) {

            this.X = X;
            this.Y = Y;
        }

        public override bool Equals(object obj) {

            if (!obj.GetType().Equals(typeof(IntPair)))
                return false;

            IntPair IP = obj as IntPair;

            return IP.X == X && IP.Y == Y;
        }

        public int CompareTo(object obj) {

            IntPair V = obj as IntPair;

            double L1 = Math.Sqrt(this.X * this.X + this.Y * this.Y);
            double L2 = Math.Sqrt(V.X * V.X + V.Y * V.Y);

            return L1 > L2 ? 1 : (L1 < L2 ? -1 : 0);
        }
    }
}
