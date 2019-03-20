using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minecraft.Support;

namespace Minecraft.Rendering {

    public class Camera {

        public Vector3D Eye { get; set; }
        public Vector3D Target { get; set; }
        public Vector3D Normal { get; set; }

        public float AXZ { get; private set; }
        public float AZY { get; private set; }

        public Camera(float EyeX, float EyeY, float EyeZ,
                      float TargetX, float TargetY, float TargetZ,
                      float NormalX, float NormalY, float NormalZ) {

            this.Eye = new Vector3D(EyeX, EyeY, EyeZ);
            this.Target = new Vector3D(TargetX, TargetY, TargetZ);
            this.Normal = new Vector3D(NormalX, NormalY, NormalZ);
        }

        public void Rotate(float DAXZ, float DAY) {

            if ((AZY + DAY) % (Math.PI * 2) < -1.5 || (AZY + DAY) % (Math.PI * 2) > 1.3)
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
        }
    }
}
