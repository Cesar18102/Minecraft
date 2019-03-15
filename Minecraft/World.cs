using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft {

    public class World {

        private Chunk[,] ChunkBuffer;//do not save
        private bool[,] ChunkDrawBuffer;//do not save

        public int RendDist { get; private set; }
        public string Name { get; private set; }

        private bool Saving = false;
        private UInt64 LastChunkId = 1;

        public delegate void LoadingLog(string message, int layer);
        public event LoadingLog LoadingLogging;

        public Chunk this[int i, int j] {

            get { return i >= 0 && i < RendDist && j >= 0 && j < RendDist ? ChunkBuffer[i, j] : null; }
        }

        public void ReserveNewChunck(ref UInt64 CID) {

            CID = LastChunkId++;
        }

        /*public Chunk this[UInt64 ID]
        {
            get { return Chunk_Buffer[ID]; }
            //read from file
        }*/
        
        public World(string name, int RenderDistance) {

            this.Name = name;
            this.RendDist = RenderDistance;
            this.ChunkBuffer = new Chunk[RendDist, RendDist];
            this.ChunkDrawBuffer = new bool[RendDist, RendDist];
        }

        public void GenerateChunk(ref int WC, ref int HC, int WMN, int WMX, int HMN, int HMX, int W, int H, int X, int Z) {

            if (WC <= WMX && !Constants.GraphicsBusy) {

                LoadingLogging("Generating chunk " + ((WC + WMX) * H + HC + HMX + 1).ToString() + "/" + W * H, 0);
                Chunk C = new Chunk(Convert.ToInt64(Constants.CHUNK_X * WC), Convert.ToInt64(Constants.CHUNK_Z * HC), true);
                this.AddChunk(C, X, Z);

                if (HC++ == HMX) { WC++; HC = HMN; }
            }
        }

        public void GenerateView(int X, int Z) {

            int WCur = X - RendDist / 2;
            int WMin = X - RendDist / 2;
            int WMax = X + RendDist / 2;

            int HCur = Z - RendDist / 2;
            int HMin = Z - RendDist / 2;
            int HMax = Z + RendDist / 2;

            for (int i = 0; i < RendDist; i++)
                for(int j = 0; j < RendDist; j++)
                    GenerateChunk(ref WCur, ref HCur, WMin, WMax, HMin, HMax, RendDist, RendDist, i, j);

            for (int i = 0; i < RendDist; i++)
                for (int j = 0; j < RendDist; j++) {

                    ChunkBuffer[i, j].LoadVisibility(j == 0 ? null : ChunkBuffer[i, j - 1],
                                                      j == RendDist - 1 ? null : ChunkBuffer[i, j + 1],
                                                      i == 0 ? null : ChunkBuffer[i - 1, j],
                                                      i == RendDist - 1 ? null : ChunkBuffer[i + 1, j]);

                    ChunkBuffer[i, j].GenerateRenderPieces();
                    ChunkBuffer[i, j].CreateTextures();
                    ChunkDrawBuffer[i, j] = true;
                }
                    
        }

        public void AddChunk(Chunk C, int X, int Z) {

            ChunkBuffer[X, Z] = C;

            if (!Saving) {

                Saving = true;
                Task.Factory.StartNew(SaveBuffer);
            }
        }

        public void Draw() {

            for (int i = 0; i < RendDist; i++)
                for (int j = 0; j < RendDist; j++)
                    if (ChunkDrawBuffer[i, j] && ChunkBuffer[i, j] != null)
                        ChunkBuffer[i, j].Draw();
        }

        public void SaveBuffer() {

            //saving
            Saving = false;
        }
    }
}
