using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;

namespace Minecraft {

    public class BlockInstance : Block {

        public UInt32 DurabilityValue { get; private set; }
        public float TemperatureValue { get; private set; }
        public float PressureValue { get; private set; }

        public UInt16 X { get; private set; }
        public UInt16 Y { get; private set; }
        public UInt16 Z { get; private set; }

        public bool[] PlanesVisibility;

        public BlockInstance(Block B, UInt16 CX, UInt16 CY, UInt16 CZ) : base(B)  {

            this.DurabilityValue = Durability;
            this.TemperatureValue = Temperature;
            this.PressureValue = Pressure;

            this.X = CX;
            this.Y = CY;
            this.Z = CZ;

            this.PlanesVisibility = new bool[B.Planes.Count];

            for (int i = 0; i < B.Planes.Count; this.PlanesVisibility[i++] = true) ;
        }

        public void Draw(Int64 PivotX, Int64 PivotZ) {

            Gl.glPushMatrix();

            Gl.glTranslatef((PivotX + X + 0.5f) * Size.DX, (Constants.CHUNK_Y - Y - 0.5f) * Size.DY, (PivotZ + Z + 0.5f) * Size.DZ);

            //Gl.glColor4f(DefaultColor.R / 255f, DefaultColor.G / 255f, DefaultColor.B / 255f, DefaultColor.A / 255f);

            for (int i = 0; i < Planes.Count; i++) {

                if (!PlanesVisibility[i])
                    continue;

                Planes[i].TEXTURE.Bind();

                Gl.glBegin(/*(int)Planes[i].GlMode*/ Gl.GL_POLYGON);

                for (int j = 0; j < Planes[i].PlanePointSquence.Count; j++) {

                    Vector2D TexPnt = TexturePoints[Planes[i].TexturePointsSequense[j]];
                    Vector3D ModelPnt = ModelPoints[Planes[i].PlanePointSquence[j]];

                    Gl.glTexCoord2d(TexPnt.DX, TexPnt.DY);
                    Gl.glVertex3f(ModelPnt.DX, ModelPnt.DY, ModelPnt.DZ);
                }

                Gl.glEnd();
            }

            Gl.glPopMatrix();
        }
    }
}
