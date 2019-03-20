using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Minecraft.Rendering;
using Minecraft.Data;
using Minecraft.Items;

namespace Minecraft.Structure {

    public class Chunk {

        private UInt64 Id;
        public UInt64 ID { get { return Id; } }

        public Int64 PivotX { get; private set; }
        public Int64 PivotZ { get; private set; }

        public Int64 RdX { get; private set; }
        public Int64 RdZ { get; private set; }

        private BlockInstance[, ,] Blocks = new BlockInstance[Constants.CHUNK_X, 
                                                              Constants.CHUNK_Y, 
                                                              Constants.CHUNK_Z];

        private RenderChunk[] Render = new RenderChunk[Constants.CHUNK_Y];
        private Chunk[] Neighbours = new Chunk[4];

        public BlockInstance this[UInt16 x, UInt16 y, UInt16 z] {
 
            get { return Blocks[x, y, z]; }
        }

        public RenderChunk this[int H] {

            get { return H >= 0 && H < Render.Length ? Render[H] : null; }
        }

        public Chunk(Int64 PivotX, Int64 PivotZ, bool Generate) {

            Game.W.ReserveNewChunck(ref Id);

            this.PivotX = PivotX;
            this.PivotZ = PivotZ;

            this.RdX = Math.Abs(PivotX / Constants.CHUNK_X);
            this.RdZ = Math.Abs(PivotZ / Constants.CHUNK_Z);

            if (Generate)
                GenerateChunk();
        }

        public void GenerateChunk() {

            Block B = ItemsSet.ITEMS[1] as Block;

            for (UInt16 k = 0; k < Constants.CHUNK_Y; k++) {
                for (UInt16 i = 0; i < Constants.CHUNK_X; i++)
                    for (UInt16 j = 0; j < Constants.CHUNK_Z; j++)
                        Blocks[i, k, j] = new BlockInstance(1, i, k, j);

                //Blocks[0, k, 0] = null;
                Render[k] = new RenderChunk(this, k, (RdX < Constants.ShortRenderDistance && 
                                                      RdZ < Constants.ShortRenderDistance) ? 
                                                        true : 
                                                        k >= Constants.DYRender * ((int)Math.Sqrt(RdX * RdX + RdZ + RdZ) - Constants.ShortRenderDistance));

                Render[k].BlockDestroyed += Chunk_BlockDestroyed;
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

        private void Chunk_BlockDestroyed(int x, int z, int h) {

            if (h != 0 && Render[h - 1][x, z] != null) Render[h - 1].V[x, z, Constants.Planes.TOP] = true;
            if (h != Constants.CHUNK_Y - 1 && Render[Constants.CHUNK_Y - 1][x, z] != null) Render[h + 1].V[x, z, Constants.Planes.BOTTOM] = true;

            int L = Constants.BlockIDs.GetLength(0);
            for (int i = 0; i < L; i++) {

                int NX = x + Constants.BlockIDs[i, 0];
                int NZ = z + Constants.BlockIDs[i, 1];

                if (NX >= 0 && NX < Constants.CHUNK_X && NZ >= 0 && NZ < Constants.CHUNK_Z) {

                    if (Render[h][NX, NZ] != null)
                        Render[h].V[NX, NZ, Constants.BlockPlaneIDsBack[i]] = true;
                }
                else {

                    int NID = Constants.BlockToChunkIDs[i];
                    int PID = Constants.BlockPlaneIDsBack[i];
                    int NNX = Constants.LoopOverflow(NX, Constants.CHUNK_X);
                    int NNZ = Constants.LoopOverflow(NZ, Constants.CHUNK_Z);

                    if (Neighbours[NID] != null && Neighbours[NID][h][NNX, NNZ] != null)
                    {

                        Neighbours[NID].Render[h].V[NNX, NNZ, PID] = true;
                        Neighbours[NID].Render[h].Rebuild();
                    }
                }
            }
        }

        public void LoadVisibility(Chunk CUp, Chunk CDown, Chunk CLeft, Chunk CRight) {

            this.Neighbours[0] = CUp;
            this.Neighbours[1] = CDown;
            this.Neighbours[2] = CLeft;
            this.Neighbours[3] = CRight;

            for (int i = 0; i < Constants.CHUNK_Y; i++)
                this.Render[i].LoadVisibility(
                    new RenderChunk[]{

                        i == Constants.CHUNK_Y - 1 ? null : (this.Render[i + 1].Visible ? this.Render[i + 1] : null),
                        CLeft == null ? null : (CLeft.Render[i].Visible ? CLeft.Render[i] : null), 
                        CUp == null ? null : (CUp.Render[i].Visible ? CUp.Render[i] : null), 
                        CRight == null ? null : (CRight.Render[i].Visible ? CRight.Render[i] : null), 
                        CDown == null ? null : (CDown.Render[i].Visible ? CDown.Render[i] : null),
                        i == 0 ? null : (this.Render[i - 1].Visible ? this.Render[i - 1] : null)
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
