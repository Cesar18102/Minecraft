using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using Tao;
using Tao.OpenGl;
using Tao.FreeGlut;
using System.IO;
using Minecraft.Support;
using Minecraft.Data;

namespace Minecraft.Rendering {

    public class Texture {

        public uint[] TEXTURES = new uint[1];

        public Bitmap B { get; private set; }
        public BitmapData BD { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public bool Prime { get; private set; }
        public bool Uploaded { get; private set; }

        private static int q = 0;

        public Texture(string filename, bool Prime) {

            if (!File.Exists(filename))
                return;

            this.B = new Bitmap(filename);
            this.Width = B.Width;
            this.Height = B.Height;
            this.Prime = Prime;
            if (!Prime) this.Lock();
        }

        public Texture(Bitmap B, bool Lock, bool Prime) {

            this.B = B;
            this.Width = B.Width;
            this.Height = B.Height;
            this.Prime = Prime;
            if (!Prime) this.Lock();
        }

        public Texture(Dictionary<IntPair, int> Map, int MapW, int MapH, int W, int H, double[] Color, bool Prime) {

            Bitmap SB = new Bitmap(W, H, PixelFormat.Format32bppArgb);
            //SB.MakeTransparent();
            this.Width = W;
            this.Height = H;

            Constants.GraphicsBusy = true;

            Graphics G = Graphics.FromImage(SB);

            for(int i = 0; i < Map.Count; i++) {

                KeyValuePair<IntPair, int> MI = Map.ElementAt(i);
                G.DrawImage(ItemsSet.TEXTURES[MI.Value].B, MI.Key.X * W / MapW, MI.Key.Y * H / MapH);
            }

            G.Dispose();

            Constants.GraphicsBusy = false;
            //SB.Save(Constants.DataDir + "/Textures/gen" + ++q + ".png");

            this.B = SB;
            this.Prime = Prime;
            if (!Prime) this.Lock();
        }

        private void Lock() {

            this.BD = B.LockBits(new Rectangle(0, 0, B.Width, B.Height),
                                 ImageLockMode.ReadOnly,
                                 PixelFormat.Format32bppArgb);
        }

        public void Upload() {

            if (Prime || Uploaded) return;

            Gl.glEnable(Gl.GL_TEXTURE_2D);
            Gl.glGenTextures(TEXTURES.Length, TEXTURES);

            for (int i = 0; i < TEXTURES.Length; i++) {

                Gl.glBindTexture(Gl.GL_TEXTURE_2D, TEXTURES[i]);

                Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA,
                                B.Width, B.Height, 0, Gl.GL_RGBA,
                                Gl.GL_UNSIGNED_BYTE, BD.Scan0);

                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
            }

            B.UnlockBits(BD);

            B.Dispose();
            BD = null;

            Uploaded = true;
        }

        public void Bind() {

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, TEXTURES[0]);
        }

        public void UnBind() {

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, 0);
        }
    }
}
