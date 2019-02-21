using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft {

    public class IntPair {

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
    }
}
