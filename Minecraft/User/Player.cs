using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minecraft.Structure;
using Minecraft.Support;

namespace Minecraft.User {

    public class Player {

        public Camera CAM { get; private set; }
        public UI UserInterface { get; private set; }

        public Vector3D AbsolutePosition { get; private set; }
        public Chunk ChunkPlayerInto { get; private set; }
        public BlockInstance BlockPlayerOnto { get; private set; }

        public Player() {

            this.AbsolutePosition = new Vector3D(0, 13, 0);
            this.CAM = new Camera(0, 13, 0, 1, 13, 0, 0, 1, 0);
            this.CAM.Rotation += CAM_Rotation;
            this.UserInterface = new UI(CAM);

            UpdatePosition();
        }

        private void CAM_Rotation(Vector3D Eye, Vector3D Target, Vector3D Normal) {
            

        }

        public void LeftMouseButtonClick(World W) {

            for (int i = 0; i < W.BufH; i++) {

                bool TargetBlockFound = false;
                for (int j = 0; j < W.BufW; j++) {

                    if (W[i, j] != null) {

                        List<BlockInstance> BL = W[i, j].GetBlocksPointInside(CAM.SightRay);
                        BL.Sort((B1, B2) => new Vector3D(B1.Middle, CAM.Eye).CompareTo(new Vector3D(B2.Middle, CAM.Eye)));

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

        public void Move(float DX, float DY, float DZ) {

            this.AbsolutePosition = new Vector3D(this.AbsolutePosition.DX + DX, 
                                                 this.AbsolutePosition.DY + DY, 
                                                 this.AbsolutePosition.DZ + DZ);
            this.CAM.Move(DX, DY, DZ);
        }

        public void InitPosition() {


        }

        public void UpdatePosition() {


        }
    }
}
