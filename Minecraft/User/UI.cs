using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;
using Minecraft.Support;
using Minecraft.Data;

namespace Minecraft.User {

    public class UI {

        public Vector3D Aim { get; private set; }

        public UI(Camera C) {

            Cam_Move(C.Eye, C.Target, C.Normal);
            C.Movement += Cam_Move;
            C.Rotation += Cam_Move;
        }

        private void Cam_Move(Vector3D Eye, Vector3D Target, Vector3D Normal) {

            Aim = new Vector3D(Eye.DX + (Target.DX - Eye.DX) / Constants.UiAimDistanceDevider,
                               Eye.DY + (Target.DY - Eye.DY) / Constants.UiAimDistanceDevider,
                               Eye.DZ + (Target.DZ - Eye.DZ) / Constants.UiAimDistanceDevider);
        }

        public void Draw() {

            Gl.glColor3f(Constants.UiAimColor.R, Constants.UiAimColor.G, Constants.UiAimColor.B);
            Gl.glPointSize(Constants.UiPointSize);

            Gl.glBegin(Gl.GL_POINTS);

                Gl.glVertex3f(Aim.DX, Aim.DY, Aim.DZ);               

            Gl.glEnd();

            Gl.glPointSize(Constants.DefaultPointSize);
        }
    }
}
