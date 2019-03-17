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
        private Visibility V = null;
        public delegate bool SearchPredicate(int x, int z);

        public bool[][,] Visited = new bool[3][,] {

            new bool[Constants.CHUNK_X, Constants.CHUNK_Z],
            new bool[Constants.CHUNK_X, Constants.CHUNK_Z],
            new bool[Constants.CHUNK_X, Constants.CHUNK_Z]
        };

        public List<RenderPiece>[] Pieces = new List<RenderPiece>[3] {

            new List<RenderPiece>(),
            new List<RenderPiece>(),
            new List<RenderPiece>(),
        };

        public SearchPredicate[] SearchConditions = new SearchPredicate[3];

        public UInt16 H { get; private set; }

        public Int64 PivotX { get; private set; }
        public Int64 PivotZ { get; private set; }

        private delegate void Adder(int x, int z);

        public RenderChunk(Chunk C, UInt16 H) {

            this.PivotX = C.PivotX;
            this.PivotZ = C.PivotZ;
            this.H = H;
            
            for (UInt16 i = 0; i < Constants.CHUNK_X; i++)
                for (UInt16 j = 0; j < Constants.CHUNK_Z; j++)
                    Layer[i, j] = C[i, H, j];
        }

        public void LoadVisibility(RenderChunk[] RC) {

            V = new Visibility(this, RC);
            SearchConditions[0] = (x, z) => V.V[x, z][(int)Constants.Planes.TOP];
            SearchConditions[1] = (x, z) => V.V[x, z][(int)Constants.Planes.BOTTOM];
            SearchConditions[2] = (x, z) => V.V[x, z][(int)Constants.Planes.BACK] || V.V[x, z][(int)Constants.Planes.FRONT] ||
                                            V.V[x, z][(int)Constants.Planes.LEFT] || V.V[x, z][(int)Constants.Planes.RIGHT];
        }

        public void GenerateRenderPieces() {

            for (Constants.MODEL_SIDE i = 0; (int)i < 3; i++) {

                int X = 0;
                int Z = 0;
                int I = Convert.ToInt32(i);

                while (true) {

                    for (; X < Constants.CHUNK_X && (Layer[X, Z] == null || Visited[I][X, Z] || !SearchConditions[I](X, Z)); Z++, X += (UInt16)(Z / Constants.CHUNK_Z), Z %= Constants.CHUNK_Z) ; // V

                    if (X >= Constants.CHUNK_X)
                        break;

                    double R = Constants.R.NextDouble();
                    double G = Constants.R.NextDouble();
                    double B = Constants.R.NextDouble();

                    RenderPiece RP = new RenderPiece(new double[] { R, G, B }, PivotX, PivotZ, H);
                    PlaneDrawSearch(I, X, Z, new Queue<IntPair>(), RP, SearchConditions[I]);
                    Pieces[I].Add(RP);
                }

                foreach (RenderPiece P in Pieces[I])
                    P.BlocksAdded(i, this.V);
            }
        }

        private void PlaneDrawSearch(int Side, int XS, int ZS, Queue<IntPair> ToVisit, RenderPiece P, SearchPredicate AddCond) { 

            Visited[Side][XS, ZS] = true;
            P.AddBlock(Layer[XS, ZS]);

            int[,] Deltas = Constants.BlockIDs;
            int L = Deltas.GetLength(0);

            for (int i = 0; i < Deltas.GetLength(0); i++)
                if (XS + Deltas[i, 0] >= 0 && XS + Deltas[i, 0] < Constants.CHUNK_X &&
                    ZS + Deltas[i, 1] >= 0 && ZS + Deltas[i, 1] < Constants.CHUNK_Z &&
                    Layer[XS + Deltas[i, 0], ZS + Deltas[i, 1]] != null &&
                    !Visited[Side][XS + Deltas[i, 0], ZS + Deltas[i, 1]]) {
                        
                        if (AddCond != null && !AddCond(XS + Deltas[i, 0], ZS + Deltas[i, 1]))
                            continue;

                        IntPair N = new IntPair(XS + Deltas[i, 0], ZS + Deltas[i, 1]);
                        if (!ToVisit.Contains(N))
                            ToVisit.Enqueue(N);
                }

            if (ToVisit.Count == 0)
                return;

            IntPair NEW = ToVisit.Dequeue();

            PlaneDrawSearch(Side, NEW.X, NEW.Y, ToVisit, P, AddCond);
        }

        public void CreateTextures() {

            for(int i = 0; i < 3; i++)
                foreach (RenderPiece RP in Pieces[i]) {

                    RP.CreateTextures();
                    RP.Triangulate();
                }
        }

        public void Draw() {

            for(int i = 0; i < 3; i++)
                for (int j = 0; j < Pieces[i].Count; j++)
                    this.Pieces[i][j].Draw();
        }
    }
}
