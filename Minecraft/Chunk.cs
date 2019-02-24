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

        private RenderChunk[] Render = new RenderChunk[Constants.CHUNK_Y];

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

            for (UInt16 k = 0; k < Constants.CHUNK_Y; k++) {
                for (UInt16 i = 0; i < Constants.CHUNK_X; i++)
                    for (UInt16 j = 0; j < Constants.CHUNK_Z; j++)
                        Blocks[i, k, j] = new BlockInstance(1, i, k, j);

                Render[k] = new RenderChunk(this, k);
            }

            /*for (UInt16 i = 0; i < Constants.CHUNK_Y; i++) {
                for (int j = 0; j < 512; j++) {

                    UInt16 X = (UInt16)Constants.R.Next(0, Constants.CHUNK_X);
                    UInt16 Z = (UInt16)Constants.R.Next(0, Constants.CHUNK_Z);

                    if (Blocks[X, i, Z] == null)
                        Blocks[X, i, Z] = new BlockInstance(1, X, i, Z);
                }

                Render[i] = new RenderChunk(this, i);
            }*/
        }

        public void CreateTextures() {

            foreach (RenderChunk RC in Render)
                RC.CreateTextures();
        }

        public void Draw() {

            foreach (RenderChunk R in Render)
                R.Draw();
        }

        public void WriteFile(Stream S) {

            //saving
        }
    }
}
