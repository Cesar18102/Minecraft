using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao;
using Tao.FreeGlut;
using Tao.OpenGl;
using Tao.Platform;

namespace Minecraft {

    public class GlWindow {

        public delegate void InitGraphics();
        public delegate void RenderFunction();
        public delegate void ReshapeFunction(int width, int height);
        public delegate void KeyDownFunction(byte key, int x, int y);
        public delegate void MouseClickFunction(int button, int state, int x, int y);
        public delegate void MouseMoveFunction(int x, int y);
        public delegate void SpecialKeyDownFunction(int key, int x, int y);
        public delegate void SpecialKeyUpFunction(int key, int x, int y);

        public InitGraphics Init;
        public RenderFunction Render;
        public ReshapeFunction Reshape;
        public KeyDownFunction KeyDown;
        public MouseClickFunction MouseClick;
        public MouseMoveFunction MouseMove;
        public SpecialKeyDownFunction SpecialKeyDown;
        public SpecialKeyUpFunction SpecialKeyUp;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public int PositionX { get; private set; }
        public int PositionY { get; private set; }

        public string Name { get; private set; }
        public int FPS { get; private set; }

        private Camera CAM { get; set; }

        public GlWindow(int Width, int Height, int PositionX, int PositionY, string Name, int FPS, 
                        InitGraphics Init, RenderFunction Render, ReshapeFunction Reshape, 
                        KeyDownFunction KeyDown, MouseClickFunction MouseClick, MouseMoveFunction MouseMove, 
                        SpecialKeyDownFunction SpecialKeyDown, SpecialKeyUpFunction SpecialKeyUp, Camera CAM) {

            this.Width = Width;
            this.Height = Height;

            this.PositionX = PositionX;
            this.PositionY = PositionY;

            this.Name = Name;
            this.FPS = FPS;

            this.Init = Init;
            this.Render = Render;
            this.Reshape = Reshape;
            this.KeyDown = KeyDown;
            this.MouseClick = MouseClick;
            this.MouseMove = MouseMove;
            this.SpecialKeyDown = SpecialKeyDown;
            this.SpecialKeyUp = SpecialKeyUp;

            this.CAM = CAM;
        }

        public void Open() {

            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_RGBA | Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(this.Width, this.Height);
            Glut.glutInitWindowPosition(this.PositionX, this.PositionY);
            Glut.glutCreateWindow(this.Name);
            
            Glut.glutFullScreen();

            Gl.glEnable(Gl.GL_TEXTURE_2D);
            Gl.glEnable(Gl.GL_ALPHA_TEST);
            Gl.glEnable(Gl.GL_BLEND);
            Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
            Gl.glDisable(Gl.GL_CULL_FACE);
            Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_FILL);

            Gl.glClearDepth(1000f);
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glDepthFunc(Gl.GL_LEQUAL);

            Glut.glutDisplayFunc(RenderFunc);
            Glut.glutIdleFunc(RenderFunc);
            Glut.glutTimerFunc(1000 / FPS, TimerFunc, 0);
            Glut.glutReshapeFunc(ReshapeFunc);
            Glut.glutKeyboardFunc(KeyDownFunc);
            Glut.glutMouseFunc(MouseClickFunc);
            Glut.glutPassiveMotionFunc(MouseMoveFunc);
            Glut.glutSpecialFunc(SpecialKeyDownFunc);
            Glut.glutSpecialUpFunc(SpecialKeyUpFunc);

            this.InitFunc();
        }

        public void RenderStart() {

            Glut.glutMainLoop();
        }

        public void InitFunc() {

            Gl.glLoadIdentity();
            this.Init();
        }

        public void RenderFunc() {

            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glLoadIdentity();
            UpdateCamera();

            this.Render();

            Glut.glutSwapBuffers();
        }

        public void TimerFunc(int time) {

            Gl.glLoadIdentity();
            Glut.glutPostRedisplay();
            Glut.glutTimerFunc(1000 / FPS, this.TimerFunc, 0);
        }

        public void ReshapeFunc(int width, int height) {

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Gl.glViewport(0, 0, width, height);
            Glu.gluPerspective(90, width * 0.97 / height, 0.01f, 100f);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);

            this.Reshape(width, height);

            this.Width = width;
            this.Height = height;
        }

        public void KeyDownFunc(byte key, int x, int y) {

            this.KeyDown(key, x, y);
        }

        public void SpecialKeyDownFunc(int key, int x, int y) {

            this.SpecialKeyDown(key, x, y);
        }

        public void SpecialKeyUpFunc(int key, int x, int y) {

            this.SpecialKeyUp(key, x, y);
        }

        public void UpdateCamera() {

            Glu.gluLookAt(CAM.Eye.DX, CAM.Eye.DY, CAM.Eye.DZ,
                          CAM.Target.DX, CAM.Target.DY, CAM.Target.DZ,
                          CAM.Normal.DX, CAM.Normal.DY, CAM.Normal.DZ);
        }

        public void MouseClickFunc(int button, int state, int x, int y) {

            this.MouseClick(button, state, x, y);
        }

        public void MouseMoveFunc(int x, int y) {

            this.MouseMove(x, y);
        }
    }
}
