using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minecraft.Support;
using Minecraft.Data;
using Minecraft.Structure;

namespace Minecraft.User {

    public class Camera {

        public delegate void AnyMove(Vector3D Eye, Vector3D Target, Vector3D Normal);
        public event AnyMove Rotation;
        public event AnyMove Movement;

        public Vector3D Eye { get; private set; }
        public Vector3D Target { get; private set; }
        public Vector3D Normal { get; private set; }
        public Ray SightRay { get; private set; }

        public float AXZ { get; private set; }
        public float AZY { get; private set; }

        public Camera(float EyeX, float EyeY, float EyeZ,
                      float TargetX, float TargetY, float TargetZ,
                      float NormalX, float NormalY, float NormalZ) {

            this.Eye = new Vector3D(EyeX, EyeY, EyeZ);
            this.Target = new Vector3D(TargetX, TargetY, TargetZ);
            this.Normal = new Vector3D(NormalX, NormalY, NormalZ);
            this.SightRay = new Ray(Eye, Target, 1); // MAGIC
        }

        public void Move(float DX, float DY, float DZ) {

            Eye = new Vector3D(Eye.DX + DX, Eye.DY + DY, Eye.DZ + DZ);
            Target = new Vector3D(Target.DX + DX, Target.DY + DY, Target.DZ + DZ);
            SightRay = new Ray(Eye, Target, 1); // MAGIC

            if (Movement != null)
                Movement(Eye, Target, Normal);
        }

        public void Rotate(float DAXZ, float DAY) {

            if ((AZY + DAY) % (Math.PI * 2) < Constants.MinCameraAngle || (AZY + DAY) % (Math.PI * 2) > Constants.MaxCameraAngle)
                return;

            AXZ = (float)((AXZ + DAXZ) % (Math.PI * 2));
            AZY = (float)((AZY + DAY) % (Math.PI * 2));

            Vector3D ViewVector = new Vector3D(this.Target, this.Eye);

            Vector3D NewViewVector = ViewVector.GetRotatedVectorZX(DAXZ);
            Vector3D NewNormalVector = this.Normal.GetRotatedVectorZX(DAXZ);

            Vector3D NewViewVectorNormilised = NewViewVector.GetRotatedVectorY(DAY, AXZ);
            Vector3D NewNormalVectorNormilised = NewNormalVector.GetRotatedVectorY(DAY, AXZ);

            this.Target = NewViewVectorNormilised.PointsTo(this.Eye);
            this.Normal = NewNormalVectorNormilised;

            SightRay = new Ray(Eye, Target, 1); //MAGIC

            if (Rotation != null)
                Rotation(Eye, Target, Normal);
        }
    }
}
