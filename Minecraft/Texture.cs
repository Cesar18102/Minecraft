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

    public class Texture {/*TODO*/

        public const int TexturesCount = 1;
        public uint[] TEXTURES = new uint[TexturesCount];

        private Bitmap B;
        private BitmapData BD;

        public Texture(string filename) {

            if (!File.Exists(filename))
                return;

            B = new Bitmap(filename);

            BD = B.LockBits(new Rectangle(0, 0, B.Width, B.Height),
                            ImageLockMode.ReadOnly,
                            PixelFormat.Format32bppRgb);
        }

        public void Upload() {

            for (int i = 0; i < TexturesCount; i++) {

                Gl.glEnable(Gl.GL_TEXTURE_2D);
                Gl.glGenTextures(TexturesCount, TEXTURES);
                Gl.glBindTexture(Gl.GL_TEXTURE_2D, TEXTURES[i]);

                Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA,
                                B.Width, B.Height, 0, Gl.GL_RGBA,
                                Gl.GL_UNSIGNED_BYTE, BD.Scan0);

                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);

                B.UnlockBits(BD);
            }
        }

        public void Bind() {

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, TEXTURES[0]);
        }

        public void UnBind() {

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, 0);
        }
    }
}
