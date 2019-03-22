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

        public BlockInstance(UInt64 BID, UInt16 CX, UInt16 CY, UInt16 CZ) : base(ItemsSet.ITEMS[BID] as Block)  {

            this.DurabilityValue = Durability;
            this.TemperatureValue = Temperature;
            this.PressureValue = Pressure;

            this.X = CX;
            this.Y = CY;
            this.Z = CZ;
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

                Vector3D P1 = ModelPoints[RelativePoints[0]];
                Vector3D P2 = ModelPoints[RelativePoints[1]];
                Vector3D P3 = ModelPoints[RelativePoints[2]];

                double A = P1.DY * P2.DZ - P2.DY * P1.DZ - P1.DY * P3.DZ + P3.DY * P1.DZ + P2.DY * P3.DZ - P3.DY * P2.DZ;
                double B = P2.DX * P1.DZ - P1.DX * P2.DZ + P1.DX * P3.DZ - P3.DX * P1.DZ - P2.DX * P3.DZ + P3.DX * P2.DZ;
                double C = P1.DX * P2.DY - P2.DX * P1.DY - P1.DX * P3.DY + P3.DX * P1.DY + P2.DX * P3.DY - P3.DX * P2.DY;
                double D = P1.DX * P3.DY * P2.DZ - P1.DX * P2.DY * P3.DZ + P2.DX * P1.DY * P3.DZ - P2.DX * P3.DY * P1.DZ - P3.DX * P1.DY * P2.DZ + P3.DX * P2.DY * P1.DZ;

                double Length = (A * V.DX + B * V.DY + C * V.DZ + D) / Math.Sqrt(A * A + B * B + C * C);
                Scope += S * Length / 3;
            }

            return Math.Abs(Scope - Size.DX * Size.DY * Size.DZ) < Constants.FAULT;
        }
    }
}
