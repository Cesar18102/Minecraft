using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using Tao;
using Tao.OpenGl;
using System.IO;

namespace Minecraft {

    public class Texture {

        public uint[] TEXTURES = new uint[1];

        public Bitmap B { get; private set; }
        public Bitmap BUP { get; private set; }
        public BitmapData BD { get; private set; }

        private static int q = 0;

        public Texture(string filename) {

            if (!File.Exists(filename))
                return;

            this.B = new Bitmap(filename);
            this.BUP = new Bitmap(this.B);
            this.Lock();
        }

        public Texture(Bitmap B) {

            this.B = B;
            this.BUP = new Bitmap(this.B);
            this.Lock();
        }

        public Texture(ref Bitmap[,] B, int W, int H, double[] Color) {

            Bitmap SB = new Bitmap(W, H, PixelFormat.Format32bppRgb);
            Graphics G = Graphics.FromImage(SB);

            int BW = B.GetLength(0);
            int BH = B.GetLength(1);

            for (int i = 0; i < BW; i++)
                for (int j = 0; j < BH; j++)
                    if (B[i, j] != null)
                        G.DrawImage(B[i, j], i * W / BW, j * H / BH);

            G.Save();

            //SB.Save(Constants.DataDir + "/Textures/gen" + ++q + ".png");

            this.B = SB;
            this.BUP = new Bitmap(SB);
            this.Lock();
        }

        private void Lock() {

            this.BD = B.LockBits(new Rectangle(0, 0, B.Width, B.Height),
                                 ImageLockMode.ReadOnly,
                                 PixelFormat.Format32bppRgb);   
        }

        public void Upload() {

            Gl.glEnable(Gl.GL_TEXTURE_2D);
            Gl.glGenTextures(TEXTURES.Length, TEXTURES);

            for (int i = 0; i < TEXTURES.Length; i++) {

                Gl.glBindTexture(Gl.GL_TEXTURE_2D, TEXTURES[i]);

                Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA,
                                B.Width, B.Height, 0, Gl.GL_RGBA,
                                Gl.GL_UNSIGNED_BYTE, BD.Scan0);

                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
            }

            B.UnlockBits(BD);
        }

        public void Bind() {

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, TEXTURES[0]);
        }

        public void UnBind() {

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, 0);
        }
    }
}
