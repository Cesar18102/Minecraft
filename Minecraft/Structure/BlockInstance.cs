using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;
using Minecraft.Items;
using Minecraft.Support;
using Minecraft.Data;

namespace Minecraft.Structure {

    public class BlockInstance : Block {

        public UInt32 DurabilityValue { get; private set; }
        public float TemperatureValue { get; private set; }
        public float PressureValue { get; private set; }

        public UInt16 X { get; private set; }
        public UInt16 Y { get; private set; }
        public UInt16 Z { get; private set; }

        public Int64 PivotX { get; private set; }
        public Int64 PivotZ { get; private set; }

        public Vector3D MaxP { get; private set; }
        public Vector3D MinP { get; private set; }
        public Vector3D Middle { get; private set; }

        public BlockInstance(UInt64 BID, UInt16 CX, UInt16 CY, UInt16 CZ, Int64 PivotX, Int64 PivotZ) : base(ItemsSet.ITEMS[BID] as Block)  {

            this.DurabilityValue = Durability;
            this.TemperatureValue = Temperature;
            this.PressureValue = Pressure;

            this.X = CX;
            this.Y = CY;
            this.Z = CZ;

            this.PivotX = PivotX;
            this.PivotZ = PivotZ;

            List<Vector3D> AbsPoints = new List<Vector3D>();

            for (int i = 0; i < Planes.Count; i++) {

                List<UInt16> RelativePoints = Planes[i].PlanePointSquence;

                for (int j = 0; j < RelativePoints.Count; j++) {

                    Vector3D P = ModelPoints[RelativePoints[j]];
                    Vector3D V = new Vector3D(P.DX + (PivotX + X + 0.5f) * Size.DX, P.DY + (Y - 0.5f) * Size.DY, P.DZ + (PivotZ + Z + 0.5f) * Size.DZ);

                    if(!AbsPoints.Contains(V))
                        AbsPoints.Add(V);
                }
            }

            AbsPoints.Sort((V1, V2) => (V1.DX >= V2.DX && V1.DY >= V2.DY && V1.DZ >= V2.DZ) ? 1 :
                                      ((V1.DX <= V2.DX && V1.DY <= V2.DY && V1.DZ <= V2.DZ) ? -1 : 0));

            this.MinP = AbsPoints[0];
            this.MaxP = AbsPoints.Last();
            this.Middle = new Vector3D((MaxP.DX + MinP.DX) / 2, (MaxP.DY + MinP.DY) / 2, (MaxP.DZ + MinP.DZ) / 2);
        }

        public bool IsPointInside(Vector3D V) {

            double Scope = 0;

            for (int i = 0; i < Planes.Count; i++) {

                List<UInt16> RelativePoints = Planes[i].PlanePointSquence;

                List<double> L = new List<double>() { new Vector3D(ModelPoints[RelativePoints.Last()], 
                                                                   ModelPoints[RelativePoints[0]]).Length };
                double P = L[0];

                for (int j = 0; j < Planes[i].PlanePointSquence.Count - 1; j++) {

                    L.Add(new Vector3D(ModelPoints[RelativePoints[j]], ModelPoints[RelativePoints[j + 1]]).Length);
                    P += L.Last();
                }

                double HalfP = P / 2;
                double S_Pow2 = 1;

                for(int j = 0; j < L.Count; j++)
                    S_Pow2 *= (HalfP - L[j]);

                double S = Math.Sqrt(S_Pow2);
                
                Vector3D MP1 = ModelPoints[RelativePoints[0]];
                Vector3D MP2 = ModelPoints[RelativePoints[1]];
                Vector3D MP3 = ModelPoints[RelativePoints[2]];

                Vector3D P1 = new Vector3D(MP1.DX + (PivotX + X + 0.5f) * Size.DX, MP1.DY + (Y - 0.5f) * Size.DY, MP1.DZ + (PivotX + Z + 0.5f) * Size.DZ);
                Vector3D P2 = new Vector3D(MP2.DX + (PivotX + X + 0.5f) * Size.DX, MP2.DY + (Y - 0.5f) * Size.DY, MP2.DZ + (PivotX + Z + 0.5f) * Size.DZ);
                Vector3D P3 = new Vector3D(MP3.DX + (PivotX + X + 0.5f) * Size.DX, MP3.DY + (Y - 0.5f) * Size.DY, MP3.DZ + (PivotX + Z + 0.5f) * Size.DZ);

                double A = P1.DY * P2.DZ - P2.DY * P1.DZ - P1.DY * P3.DZ + P3.DY * P1.DZ + P2.DY * P3.DZ - P3.DY * P2.DZ;
                double B = P2.DX * P1.DZ - P1.DX * P2.DZ + P1.DX * P3.DZ - P3.DX * P1.DZ - P2.DX * P3.DZ + P3.DX * P2.DZ;
                double C = P1.DX * P2.DY - P2.DX * P1.DY - P1.DX * P3.DY + P3.DX * P1.DY + P2.DX * P3.DY - P3.DX * P2.DY;
                double D = P1.DX * P3.DY * P2.DZ - P1.DX * P2.DY * P3.DZ + P2.DX * P1.DY * P3.DZ - P2.DX * P3.DY * P1.DZ - P3.DX * P1.DY * P2.DZ + P3.DX * P2.DY * P1.DZ;

                double Length = Math.Abs(A * V.DX + B * V.DY + C * V.DZ + D) / Math.Sqrt(A * A + B * B + C * C);
                Scope += S * Length / 3;
            }

            return Math.Abs(Scope - Size.DX * Size.DY * Size.DZ) < Constants.FAULT;
        }
    }
}
