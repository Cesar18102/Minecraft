using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao;
using Tao.FreeGlut;
using Tao.OpenGl;
using Tao.Platform;
using System.Windows.Forms;
using System.Drawing;

namespace Minecraft {

    public class Game {

        public static GlWindow GLW;
        public static Camera CAM;
        public static World W;
        private Vector2D Mouse = null;

        public static bool SHIFT = false;

        public delegate void LoadingLog(string message, int layer);
        public event LoadingLog LoadingLogging;

        private static int Width = 5;
        private static int Height = 5;

        int WCur = -Width / 2;
        int WMax = Width / 2;

        int HCur = -Height / 2;
        int HMax = Height / 2;

        public void Start() {

            CAM = new Camera(0, 0.2f, 0, 1, 0, 1, 0, 1, 0);

            GLW = new GlWindow(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height,
                               0, 0, "Minecraft", 60, InitGraphics, Render, Reshape, KeyDown,
                               MouseClick, MouseMove, SpecialKeyDown, SpecialKeyUp, CAM);

            Cursor.Position = new Point(GLW.Width / 2, GLW.Height / 2);
            Mouse = new Vector2D(GLW.Width / 2, GLW.Height / 2);

            LoadingLogging("Loading World", 0);

            W = new World("Test");

            GLW.Open();
            GLW.RenderStart();
        }

        public void InitGraphics() {

            Gl.glShadeModel(Gl.GL_SMOOTH);
            Gl.glHint(Gl.GL_PERSPECTIVE_CORRECTION_HINT, Gl.GL_NEAREST);

            for (int i = -Width / 2; i <= Width / 2; i++)
                for (int j = -Height / 2; j <= Height / 2; j++)
                {

                    LoadingLogging("Generating chunk " + ((i + Width / 2) * Height + j + Height / 2 + 1).ToString() + "/" + Width * Height, 0);

                    Chunk C = new Chunk(Convert.ToInt64(Constants.CHUNK_X * i), Convert.ToInt64(Constants.CHUNK_Z * j), true);
                    C.CreateTextures();
                    W.AddChunk(C);
                }

            Cursor.Position = new Point(GLW.Width / 2, GLW.Height / 2);
        }

        public void Render() {

            Gl.glClearColor(255, 255, 255, 0);
            Cursor.Hide();

            /*if (WCur <= WMax) {

                Task.Factory.StartNew(() => {

                    LoadingLogging("Generating chunk " + ((WCur + Width / 2) * Height + HCur + Height / 2 + 1).ToString() + "/" + Width * Height, 0);
                    Chunk C = new Chunk(Convert.ToInt64(Constants.CHUNK_X * WCur), Convert.ToInt64(Constants.CHUNK_Z * HCur), true);
                    C.CreateTextures();
                    W.AddChunk(C);
                });

                HCur++;
                WCur += HCur / HMax;
                HCur %= HMax;
            }*/

            W.Draw();
        }

        public void Reshape(int w, int h) {

        }

        public void KeyDown(byte key, int x, int y) { // update pressed

            double OldX = CAM.Target.DX;
            double OldZ = CAM.Target.DZ;

            double EyeX = CAM.Eye.DX;
            double EyeZ = CAM.Eye.DZ;

            double DX = OldX - EyeX;
            double DZ = OldZ - EyeZ;
            double L = Math.Sqrt(DX * DX + DZ * DZ);

            double SPEED = 0.02 * (Glut.glutGetModifiers() == Glut.GLUT_ACTIVE_SHIFT ? 5 : 1);
            float DDX = (float)(SPEED * DX / L);
            float DDZ = (float)(SPEED * DZ / L);

            double[] DZS = new double[] {

                OldX - EyeX + EyeZ,
                -OldX + EyeX + EyeZ
            };

            double[] DDXS = new double[] {
 
                -(OldZ - EyeZ) * (DZS[0] - EyeZ) / (50 * (OldX - EyeX)),
                -(OldZ - EyeZ) * (DZS[1] - EyeZ) / (50 * (OldX - EyeX))
            };

            double[] DDZS = new double[] {

                (OldX - EyeX) / 50,
                (-OldX + EyeX) / 50
            };

            switch (key) {

                //A
                case 65  : 
                case 97  :
                case 212 :
                case 244 :

                    CAM.Eye = new Vector3D(CAM.Eye.DX - (float)DDXS[0], CAM.Eye.DY, CAM.Eye.DZ - (float)DDZS[0]);
                    CAM.Target = new Vector3D(CAM.Target.DX - (float)DDXS[0], CAM.Target.DY, CAM.Target.DZ - (float)DDZS[0]);

                    break;

                //D
                case 68  :
                case 100 :
                case 194 :
                case 226 :

                    CAM.Eye = new Vector3D(CAM.Eye.DX + (float)DDXS[0], CAM.Eye.DY, CAM.Eye.DZ + (float)DDZS[0]);
                    CAM.Target = new Vector3D(CAM.Target.DX + (float)DDXS[0], CAM.Target.DY, CAM.Target.DZ + (float)DDZS[0]);

                    break;

                //W
                case 87  :
                case 119 :
                case 214 :
                case 246 :

                    CAM.Eye = new Vector3D(CAM.Eye.DX + DDX, CAM.Eye.DY, CAM.Eye.DZ + DDZ);
                    CAM.Target = new Vector3D(CAM.Target.DX + DDX, CAM.Target.DY, CAM.Target.DZ + DDZ);

                    break;

                //S
                case 83  :
                case 115 :
                case 219 :
                case 251 :

                    CAM.Eye = new Vector3D(CAM.Eye.DX - DDX, CAM.Eye.DY, CAM.Eye.DZ - DDZ);
                    CAM.Target = new Vector3D(CAM.Target.DX - DDX, CAM.Target.DY, CAM.Target.DZ - DDZ);

                    break;

                // \s
                case 32  :

                    CAM.Eye = new Vector3D(CAM.Eye.DX, CAM.Eye.DY + 0.1f, CAM.Eye.DZ);
                    CAM.Target = new Vector3D(CAM.Target.DX, CAM.Target.DY + 0.1f, CAM.Target.DZ);

                    break;

                // X
                case 120:

                    CAM.Eye = new Vector3D(CAM.Eye.DX, CAM.Eye.DY - 0.1f, CAM.Eye.DZ);
                    CAM.Target = new Vector3D(CAM.Target.DX, CAM.Target.DY - 0.1f, CAM.Target.DZ);

                    break;
            }
        }

        public void SpecialKeyDown(int key, int x, int y) {

        }

        public void SpecialKeyUp(int key, int x, int y) {

        }

        public void MouseClick(int button, int state, int x, int y) {


        }

        public void MouseMove(int x, int y) {

            Vector2D NewMouse = new Vector2D(x, y);

            float dX = NewMouse.DX - Mouse.DX;
            float dY = NewMouse.DY - Mouse.DY;

            float AXZ = (float)(Math.PI * -dX / GLW.Width);
            float AZY = (float)(Math.PI * dY / GLW.Height);

            CAM.Rotate(AXZ, AZY);

            Cursor.Position = new Point(GLW.Width / 2, GLW.Height / 2);
        }
    }
}
