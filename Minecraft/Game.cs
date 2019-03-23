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
using Minecraft.Rendering;
using Minecraft.Structure;
using Minecraft.Support;
using Minecraft.Data;
using Minecraft.User;

namespace Minecraft {

    public class Game {

        public static Player P = new Player();
        public static GlWindow GLW;
        public static World W;
        private Vector2D Mouse = null;

        private static int h = Constants.CHUNK_Y - 1;

        public static bool SHIFT = false;

        public delegate void LoadingLog(string message, int layer);
        public event LoadingLog LoadingLogging;

        public void Start() {

            GLW = new GlWindow(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height,
                               0, 0, "Minecraft", 60, InitGraphics, Render, Reshape, KeyDown,
                               MouseClick, MouseMove, SpecialKeyDown, SpecialKeyUp, P.CAM);

            Cursor.Position = new Point(GLW.Width / 2, GLW.Height / 2);
            Mouse = new Vector2D(GLW.Width / 2, GLW.Height / 2);

            LoadingLogging("Loading World", 0);

            W = new World("Test", Constants.RenderDistance);
            W.LoadingLogging += W_LoadingLogging;

            GLW.Open();

            GLW.RenderStart();
        }

        public void W_LoadingLogging(string message, int layer) {

            LoadingLogging(message, 0);
        }

        public async void InitGraphics() {

            Gl.glShadeModel(Gl.GL_SMOOTH);
            Gl.glHint(Gl.GL_PERSPECTIVE_CORRECTION_HINT, Gl.GL_NEAREST);

            Gl.glEnable(Gl.GL_LIGHTING);
            Gl.glEnable(Gl.GL_COLOR_MATERIAL);
            Gl.glEnable(Gl.GL_STENCIL_TEST);
            /*Gl.glColorMaterial(Gl.GL_FRONT, Gl.GL_DIFFUSE);*/

            Cursor.Position = new Point(GLW.Width / 2, GLW.Height / 2);

            await Task.Run(() => W.GenerateView(0, 0));
        }

        public void Render() {

            lock (ItemsSet.TEXTURES) {

                int L = ItemsSet.TEXTURES.Count;
                for (int i = 0; i < L; i++)
                    ItemsSet.TEXTURES[i].Upload();
            }

            Gl.glClearColor(255, 255, 255, 0);
            Cursor.Hide();
            W.Draw();
            P.UserInterface.Draw();
        }

        public void Reshape(int w, int h) {

        }

        public void KeyDown(byte key, int x, int y) { // update pressed

            double OldX = P.CAM.Target.DX;
            double OldZ = P.CAM.Target.DZ;

            double EyeX = P.CAM.Eye.DX;
            double EyeZ = P.CAM.Eye.DZ;

            double DX = OldX - EyeX;
            double DZ = OldZ - EyeZ;
            double L = Math.Sqrt(DX * DX + DZ * DZ);

            double SPEED = Constants.DefaultSpeed * (Glut.glutGetModifiers() == Glut.GLUT_ACTIVE_SHIFT ? Constants.SprintMultiplier : 1);
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

                    P.Move(-(float)DDXS[0], 0, -(float)DDZS[0]);
                    break;

                //D
                case 68  :
                case 100 :
                case 194 :
                case 226 :

                    P.Move((float)DDXS[0], 0, (float)DDZS[0]);
                    break;

                //W
                case 87  :
                case 119 :
                case 214 :
                case 246 :

                    P.Move(DDX, 0, DDZ);
                    break;

                //S
                case 83  :
                case 115 :
                case 219 :
                case 251 :

                    P.Move(-DDX, 0, -DDZ);
                    break;

                // \s
                case 32  :

                    P.Move(0, 0.1f, 0);
                    break;

                // X
                case 120:

                    P.Move(0, -0.1f, 0);
                    break;

                case 67:
                case 99:

                    if (h >= 0)
                    {
                        W[2, 2][h].DestroyBlock(0, 0);
                        W[2, 2][h].DestroyBlock(1, 0);
                        W[2, 2][h--].DestroyBlock(0, 1);
                        //W[2, 2][h--].Rebuild();
                    }
                break;
            }
        }

        public void SpecialKeyDown(int key, int x, int y)
        {

        }

        public void SpecialKeyUp(int key, int x, int y) {

        }

        public void MouseClick(int button, int state, int x, int y) {

            if (button != 0) // make async, apply only to chunks adjecent to player's one
                return;

            for (int i = 0; i < W.BufH; i++) {

                bool TargetBlockFound = false;
                for (int j = 0; j < W.BufW; j++) {

                    if (W[i, j] != null) {

                        List<BlockInstance> BL = W[i, j].GetBlocksPointInside(P.CAM.SightRay);
                        BL.Sort((B1, B2) => new Vector3D(B1.Middle, P.CAM.Eye).CompareTo(new Vector3D(B2.Middle, P.CAM.Eye)));

                        if (BL.Count > 0) {

                            W[i, j][BL[0].Y].DestroyBlock(BL[0].X, BL[0].Z);
                            TargetBlockFound = true;
                            break;
                        }
                    }
                }

                if (TargetBlockFound)
                    break;
            }
        }

        public void MouseMove(int x, int y) {

            Vector2D NewMouse = new Vector2D(x, y);

            float dX = NewMouse.DX - Mouse.DX;
            float dY = NewMouse.DY - Mouse.DY;

            float AXZ = (float)(Math.PI * -dX / GLW.Width);
            float AZY = (float)(Math.PI * dY / GLW.Height);

            P.CAM.Rotate(AXZ, AZY);

            Cursor.Position = new Point(GLW.Width / 2, GLW.Height / 2);
        }
    }
}
