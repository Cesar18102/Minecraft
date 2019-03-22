using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minecraft.Support;
using Minecraft.Data;

namespace Minecraft.Structure {

    public class World {

        private Chunk[,] ChunkBuffer;
        private bool[,] ChunkDrawBuffer;
        private List<List<IntPair>> DrawSequence = new List<List<IntPair>>();

        public int BufH { get; private set; }
        public int BufW { get; private set; }
        private int Total = 0;

        public int RendDist { get; private set; }
        public string Name { get; private set; }

        private bool Saving = false;
        private UInt64 LastChunkId = 1;

        public delegate void LoadingLog(string message, int layer);
        public event LoadingLog LoadingLogging;

        public Chunk this[int i, int j] {

            get { return i >= 0 && i < BufW && j >= 0 && j < BufH ? ChunkBuffer[i, j] : null; }
        }

        public void ReserveNewChunck(ref UInt64 CID) {

            CID = LastChunkId++;
        }
        
        public World(string name, int RenderDistance) {

            this.Name = name;
            this.RendDist = RenderDistance;

            this.BufH = 2 * RendDist - 1;
            this.BufW = 2 * RendDist - 1;
            this.Total = 2 * RendDist * (RendDist - 1) + 1;

            this.ChunkBuffer = new Chunk[BufW, BufH];
            this.ChunkDrawBuffer = new bool[BufW, BufH];
        }

        public void GenerateChunk(IntPair IP, int Num) {

            if (!Constants.GraphicsBusy) {

                LoadingLogging("Generating chunk " + Num.ToString() + "/" + Total, 0);
                Chunk C = new Chunk(Convert.ToInt64(Constants.CHUNK_X * (IP.X - BufW / 2)), Convert.ToInt64(Constants.CHUNK_Z * (IP.Y - BufH / 2)), true);
                this.AddChunk(C, IP.X, IP.Y);
            }
        }

        public void GenerateView(int X, int Z) {

            List<IntPair> P = new List<IntPair>() { new IntPair(RendDist - 1, RendDist - 1) };
            int Count = 0;

            for (int i = 0; i < RendDist; i++) {

                foreach (IntPair IP in P)
                    GenerateChunk(IP, ++Count);

                DrawSequence.Add(P);
                P = Spread(P);
            }


            foreach (List<IntPair> LIP in DrawSequence)
                foreach (IntPair IP in LIP) {

                    if (ChunkBuffer[IP.X, IP.Y] != null) {

                        ChunkBuffer[IP.X, IP.Y].LoadVisibility(IP.Y == 0 ? null : ChunkBuffer[IP.X, IP.Y - 1],
                                                               IP.Y == BufH - 1 ? null : ChunkBuffer[IP.X, IP.Y + 1],
                                                               IP.X == 0 ? null : ChunkBuffer[IP.X - 1, IP.Y],
                                                               IP.X == BufW - 1 ? null : ChunkBuffer[IP.X + 1, IP.Y]);

                        ChunkBuffer[IP.X, IP.Y].GenerateRenderPieces();
                        ChunkBuffer[IP.X, IP.Y].CreateTextures();
                        ChunkDrawBuffer[IP.X, IP.Y] = true;
                    }
                }
        }

        public List<IntPair> Spread(List<IntPair> P) {

            List<IntPair> NextCircle = new List<IntPair>();

            int NC = P.Count;
            bool[] Used = new bool[NC];

            while(NC > 0) {

                int i = Constants.R.Next(0, P.Count);

                if (Used[i]) continue;

                Used[i] = true;
                NC--;

                int L = Constants.BlockIDs.GetLength(0);

                for (int j = 0; j < L; j++) {

                    int NX = P[i].X + Constants.BlockIDs[j, 0];
                    int NY = P[i].Y + Constants.BlockIDs[j, 1];
                    IntPair Pair = new IntPair(NX, NY);

                    if (NX >= 0 && NX < ChunkDrawBuffer.GetLength(0) && 
                        NY >= 0 && NY < ChunkDrawBuffer.GetLength(1) &&
                        ChunkBuffer[NX, NY] == null && !NextCircle.Contains(Pair))
                        NextCircle.Add(new IntPair(NX, NY));
                }
            }

            return NextCircle;
        }

        public void AddChunk(Chunk C, int X, int Z) {

            ChunkBuffer[X, Z] = C;

            if (!Saving) {

                Saving = true;
                Task.Factory.StartNew(SaveBuffer);
            }
        }

        public void Draw() {

            lock (DrawSequence) {

                foreach (List<IntPair> LIP in DrawSequence)
                    foreach (IntPair IP in LIP)
                        if (ChunkDrawBuffer[IP.X, IP.Y] && ChunkBuffer[IP.X, IP.Y] != null)
                            ChunkBuffer[IP.X, IP.Y].Draw();
            }
        }

        public void SaveBuffer() {

            //saving
            Saving = false;
        }
    }
}
