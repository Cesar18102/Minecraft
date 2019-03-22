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
            this.UserInterface = new UI(CAM);

            UpdatePosition();
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
