using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Tao.OpenGl;

namespace Minecraft {

    public class RenderChunk {

        public BlockInstance[,] Layer = new BlockInstance[Constants.CHUNK_X, Constants.CHUNK_Z];
        public bool[,] Visited = new bool[Constants.CHUNK_X, Constants.CHUNK_Z];
        public bool[,] UpperMask = new bool[Constants.CHUNK_X, Constants.CHUNK_Z];
        public bool[,] LowerMask = new bool[Constants.CHUNK_X, Constants.CHUNK_Z];

        public List<RenderPiece> Pieces = new List<RenderPiece>();
        public UInt16 H { get; private set; }
        public Chunk C { get; private set; }

        public Int64 PivotX { get; private set; }
        public Int64 PivotZ { get; private set; }

        private delegate void Adder(int x, int z);

        public RenderChunk(Chunk C, UInt16 H) {

            this.PivotX = C.PivotX;
            this.PivotZ = C.PivotZ;
            this.H = H;
            this.C = C;
            
            for (UInt16 i = 0; i < Constants.CHUNK_X; i++)
                for (UInt16 j = 0; j < Constants.CHUNK_Z; j++)
                    Layer[i, j] = C[i, H, j];
        }

        public void LoadChunkVisibility(bool[,] UMask, bool[,] LMask) {

            Array.Copy(UMask, UpperMask, UMask.Length);
            Array.Copy(LMask, LowerMask, LMask.Length);

            Adder Upper = (x, z) => { UpperMask[x, z] = (C[(UInt16)x, (UInt16)(H + 1), (UInt16)z] != null); };
            if (H >= Constants.CHUNK_Y - 1)
                Upper = (x, z) => { return; };

            Adder Lower = (x, z) => { LowerMask[x, z] = (C[(UInt16)x, (UInt16)(H - 1), (UInt16)z] != null); };
            if (H <= 0)
                Lower = (x, z) => { return; };

            for (UInt16 i = 0; i < Constants.CHUNK_X; i++)
                for (UInt16 j = 0; j < Constants.CHUNK_Z; j++) {

                    Upper(i, j);
                    Lower(i, j);
                }
        }

        public void GenerateRenderPieces() {

            int X = 0;
            int Z = 0;

            while (true) {

                for (; X < Constants.CHUNK_X && (Layer[X, Z] == null || Visited[X, Z]); Z++, X += (UInt16)(Z / Constants.CHUNK_Z), Z %= Constants.CHUNK_Z) ;

                if (X >= Constants.CHUNK_X)
                    break;

                double R = Constants.R.NextDouble();
                double G = Constants.R.NextDouble();
                double B = Constants.R.NextDouble();

                RenderPiece RP = new RenderPiece(new double[] { R, G, B }, PivotX, PivotZ, H);
                PlaneDrawSearch(X, Z, new Queue<IntPair>(), RP);
                Pieces.Add(RP);
            }

            foreach (RenderPiece P in Pieces) {

                bool TOP_VISIBLE = H == Constants.CHUNK_Y - 1;
                bool BOT_VISIBLE = H == 0;

                for (int i = 0; i < P.BlocksCount; i++)
                    if (!UpperMask[P[i].X, P[i].Z]) {

                        TOP_VISIBLE = true;
                        break;
                }

                for (int i = 0; i < P.BlocksCount; i++)
                    if (!LowerMask[P[i].X, P[i].Z]) {

                        BOT_VISIBLE = true;
                        break;
                }

                P.SetVisibility(TOP_VISIBLE, BOT_VISIBLE, true);
                P.BlocksAdded();
            }
        }

        private void PlaneDrawSearch(int XS, int ZS, Queue<IntPair> ToVisit, RenderPiece P) {

            Visited[XS, ZS] = true;
            P.AddBlock(Layer[XS, ZS]);

            for (int i = 0; i < Constants.BlockIDs.GetLength(0); i++)
                if (XS + Constants.BlockIDs[i, 0] >= 0 && XS + Constants.BlockIDs[i, 0] < Constants.CHUNK_X &&
                    ZS + Constants.BlockIDs[i, 1] >= 0 && ZS + Constants.BlockIDs[i, 1] < Constants.CHUNK_Z &&
                    Layer[XS + Constants.BlockIDs[i, 0], ZS + Constants.BlockIDs[i, 1]] != null &&
                    !Visited[XS + Constants.BlockIDs[i, 0], ZS + Constants.BlockIDs[i, 1]]) {

                    IntPair N = new IntPair(XS + Constants.BlockIDs[i, 0], ZS + Constants.BlockIDs[i, 1]);
                    if (!ToVisit.Contains(N))
                        ToVisit.Enqueue(N);
                }

            if (ToVisit.Count == 0)
                return;

            IntPair NEW = ToVisit.Dequeue();

            PlaneDrawSearch(NEW.X, NEW.Y, ToVisit, P);
        }

        public void CreateTextures() {

            foreach (RenderPiece RP in Pieces) {

                RP.CreateTextures();
                RP.Triangulate();
            }
        }

        public void Draw() {

            for (int i = 0; i < Pieces.Count; i++)
                this.Pieces[i].Draw();
        }
    }
}
