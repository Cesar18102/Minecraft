using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minecraft.Structure;
using Minecraft.Data;

namespace Minecraft.Support {

    public class Ray {

        public Vector3D Start { get; private set; }
        public Vector3D End { get; private set; }

        public Vector3D Direction { get; private set; }
        public float Length { get; private set; }

        public Ray(Vector3D Start, Vector3D End, float Length) {

            this.Start = Start;
            this.End = End;
            this.Direction = new Vector3D(End, Start);
            this.Length = Length;
        }

        public bool IsIntersects(BlockInstance B) {

            if (Start.DX >= B.MinP.DX && Start.DX <= B.MaxP.DX && Start.DY >= B.MinP.DY && Start.DY <= B.MaxP.DY && Start.DZ >= B.MinP.DZ && Start.DZ <= B.MaxP.DZ)
                return true;

            float[] T1 = new float[] { 

                (B.MinP.DX - Start.DX) / Direction.DX,
                (B.MinP.DY - Start.DY) / Direction.DY,
                (B.MinP.DZ - Start.DZ) / Direction.DZ
            };

            float[] T2 = new float[] { 

                (B.MaxP.DX - Start.DX) / Direction.DX,
                (B.MaxP.DY - Start.DY) / Direction.DY,
                (B.MaxP.DZ - Start.DZ) / Direction.DZ
            };

            float[] S = new float[] { Start.DX, Start.DY, Start.DZ };
            float[] D = new float[] { Direction.DX, Direction.DY, Direction.DZ };

            float[] MnP = new float[] { B.MinP.DX, B.MinP.DY, B.MinP.DZ };
            float[] MxP = new float[] { B.MaxP.DX, B.MaxP.DY, B.MaxP.DZ };

            float TNear = float.MinValue;
            float TFar = float.MaxValue;

            for (int i = 0; i < 3; i++) {

                if (Math.Abs(D[i]) < Constants.FAULT)
                    continue;

                if (T1[i] > T2[i]) {

                    float temp = T1[i];
                    T1[i] = T2[i];
                    T2[i] = temp;
                }

                if (T1[i] > TNear)
                    TNear = T1[i];

                if (T2[i] < TFar)
                    TFar = T2[i];

                if (TNear > TFar || TFar < 0)
                    return false;
            }

            return TNear <= TFar && TFar >= 0 && TNear <= Length;
        }
    }
}
