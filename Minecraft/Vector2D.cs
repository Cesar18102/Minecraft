using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft {

    public class Vector2D : IVector {

        public float DX { get; private set; }
        public float DY { get; private set; }

        public float Length { get { return (float)Math.Sqrt(DX * DX + DY * DY); } }

        public Vector2D(float dX, float dY) {

            this.DX = dX;
            this.DY = dY;
        }

        public Vector2D GetRotatedVector(float angle) {

            float CA = (float)Math.Cos(angle);
            float SA = (float)Math.Sin(angle);

            return new Vector2D(this.DX * CA + this.DY * SA, -this.DX * SA + this.DY * CA);
        }
    }
}
