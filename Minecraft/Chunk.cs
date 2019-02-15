using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Minecraft {

    public class Chunk {

        private UInt64 Id;
        public UInt64 ID { get { return Id; } }

        public Int64 PivotX { get; private set; }
        public Int64 PivotZ { get; private set; }

        private BlockInstance[, ,] Blocks = new BlockInstance[Constants.CHUNK_X, 
                                                              Constants.CHUNK_Y, 
                                                              Constants.CHUNK_Z];

        public BlockInstance this[UInt16 x, UInt16 y, UInt16 z] {
 
            get { return Blocks[x, y, z]; }
            set { Blocks[x, y, z] = value; }
        }

        public Chunk(Int64 PivotX, Int64 PivotZ, bool Generate) {

            Game.W.ReserveNewChunck(ref Id);

            this.PivotX = PivotX;
            this.PivotZ = PivotZ;

            if (Generate)
                GenerateChunk();
        }

        public void GenerateChunk() {

            Block B = ItemsSet.ITEMS[1] as Block;

            for (UInt16 i = 0; i < Constants.CHUNK_X; i++)
                for (UInt16 j = 0; j < Constants.CHUNK_Z; j++)
                    for (UInt16 k = 0; k < Constants.CHUNK_Y; k++)
                        Blocks[i, k, j] = new BlockInstance(B, i, k, j);

            for (UInt16 i = 0; i < Constants.CHUNK_X; i++)
                for (UInt16 j = 0; j < Constants.CHUNK_Z; j++)
                    for (UInt16 k = 0; k < Constants.CHUNK_Y; k++) {

                        Blocks[i, k, j].PlanesVisibility[1] = i == 0 || Blocks[i - 1, k, j] == null; // LEFT
                        Blocks[i, k, j].PlanesVisibility[3] = i == Constants.CHUNK_X - 1 || Blocks[i + 1, k, j] == null; // RIGHT
                        Blocks[i, k, j].PlanesVisibility[2] = j == 0 || Blocks[i, k, j - 1] == null; // FRONT
                        Blocks[i, k, j].PlanesVisibility[4] = j == Constants.CHUNK_Z - 1 || Blocks[i, k, j + 1] == null; // BACK
                        Blocks[i, k, j].PlanesVisibility[0] = k == 0 || Blocks[i, k - 1, j] == null; // TOP
                        Blocks[i, k, j].PlanesVisibility[5] = k == Constants.CHUNK_Y - 1 || Blocks[i, k + 1, j] == null; // BOTTOM
                    }
            //genaration
        }

        public void Draw() {

            Vector3D BlockSize = (ItemsSet.ITEMS[1] as Block).Size;

            for (int i = 0; i < Constants.CHUNK_X; i++)
                for (int j = 0; j < Constants.CHUNK_Z; j++)
                    for (int k = 0; k < Constants.CHUNK_Y; k++)
                        Blocks[i, k, j].Draw(new Vector3D((PivotX + 0.5f + i) * BlockSize.DX,
                                                          (-k - 0.5f) * BlockSize.DY,
                                                          (PivotZ + 0.5f + j) * BlockSize.DZ));
                                                          
            //draw segmentation
        }

        public void WriteFile(Stream S) {

            //saving
        }

        /*public bool PlayerInside() {

            
        }*/
    }
}
