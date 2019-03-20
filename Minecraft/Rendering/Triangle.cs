using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minecraft.Support;

namespace Minecraft.Rendering {

    public class Triangle<T> where T : IVector {

        public T[] V = new T[3];

        public Triangle(params T[] V) {

            Array.Copy(V, this.V, V.Length);
        }
    }
}
