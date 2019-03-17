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

                int Count = Constants.R.Next(0, 10);

                for (int q = 0; q < Count; q++)
                    if(Constants.R.NextDouble() > 0.5)
                        Blocks[0, k, Constants.R.Next(0, Constants.CHUNK_Z)] = null; 
                    else
                        Blocks[Constants.R.Next(0, Constants.CHUNK_Z), k, 0] = null; 
                
                Render[k] = new RenderChunk(this, k);
            }

            /*for (Int16 i = (Int16)(Constants.CHUNK_Y - 1); i >= 0; i--) {
                for (int j = 0; j < 1024; j++) {

                    UInt16 X = (UInt16)Constants.R.Next(0, Constants.CHUNK_X);
                    UInt16 Z = (UInt16)Constants.R.Next(0, Constants.CHUNK_Z);

                    if (Blocks[X, i, Z] == null)
                        Blocks[X, i, Z] = new BlockInstance(1, X, (UInt16)i, Z);
                }

                Render[i] = new RenderChunk(this, (UInt16)i);
            }*/
        }

        public void LoadVisibility(Chunk CUp, Chunk CDown, Chunk CLeft, Chunk CRight) {

            for (int i = 0; i < Constants.CHUNK_Y; i++)
                this.Render[i].LoadVisibility(
                    new RenderChunk[]{

                        i == Constants.CHUNK_Y - 1 ? null : this.Render[i + 1],
                        CLeft == null ? null : CLeft.Render[i], 
                        CUp == null ? null : CUp.Render[i], 
                        CRight == null ? null : CRight.Render[i], 
                        CDown == null ? null : CDown.Render[i],
                        i == 0 ? null : this.Render[i - 1]
                    }
                );
        }

        public void GenerateRenderPieces() {

            for (int i = 0; i < Constants.CHUNK_Y; i++)
                Render[i].GenerateRenderPieces();
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
