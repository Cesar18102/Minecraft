using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Tao.OpenGl;
using Minecraft.Structure;
using Minecraft.Data;
using Minecraft.Support;

namespace Minecraft.Rendering {

    public class RenderChunk {

        private delegate bool SearchPredicate(int x, int z);
        private delegate void Adder(int x, int z);

        public delegate void BlockDestroy(int x, int z, int h);
        public event BlockDestroy BlockDestroyed;

        private BlockInstance[,] Layer = new BlockInstance[Constants.CHUNK_X, Constants.CHUNK_Z];

        public bool Visible { get; private set; }
        public Visibility V { get; private set; }

        public Int64 PivotX { get; private set; }
        public Int64 PivotZ { get; private set; }

        public UInt16 H { get; private set; }

        public BlockInstance this[int X, int Z] {

            get { return X >= 0 && X < Constants.CHUNK_X && Z >= 0 && Z < Constants.CHUNK_Z ? Layer[X, Z] : null; }
        }

        private List<RenderPiece>[] Pieces = new List<RenderPiece>[3] {

            new List<RenderPiece>(),
            new List<RenderPiece>(),
            new List<RenderPiece>(),
        };

        private SearchPredicate[] SearchConditions = new SearchPredicate[3];

        public RenderChunk(Chunk C, UInt16 H, bool Visible) {

            this.PivotX = C.PivotX;
            this.PivotZ = C.PivotZ;
            this.Visible = Visible;
            this.H = H;
            
            for (UInt16 i = 0; i < Constants.CHUNK_X; i++)
                for (UInt16 j = 0; j < Constants.CHUNK_Z; j++)
                    Layer[i, j] = C[i, H, j];
        }

        public void Rebuild() {

            for (int i = 0; i < 3; i++)
                this.Pieces[i].Clear();

            GenerateRenderPieces();
            CreateTextures();
        }

        public void DestroyBlock(UInt16 X, UInt16 Z) {

            Layer[X, Z] = null;

            for (int i = 0; i < 6; i++)
                V[X, Z, i] = false;

            if (this.BlockDestroyed != null)
                BlockDestroyed(X, Z, H);

            Rebuild();
        }

        public void LoadVisibility(RenderChunk[] RC) {

            V = new Visibility(this, RC);
            SearchConditions[0] = (x, z) => V[x, z, Constants.Planes.TOP];
            SearchConditions[1] = (x, z) => V[x, z, Constants.Planes.BOTTOM];
            SearchConditions[2] = (x, z) => V[x, z, Constants.Planes.BACK] || V[x, z, Constants.Planes.FRONT] ||
                                            V[x, z, Constants.Planes.LEFT] || V[x, z, Constants.Planes.RIGHT];
        }

        public void GenerateRenderPieces() {

            for (Constants.MODEL_SIDE i = 0; (int)i < 3; i++) {

                int X = 0;
                int Z = 0;
                int I = Convert.ToInt32(i);
                bool[,] Visited = new bool[Constants.CHUNK_X, Constants.CHUNK_Z];

                List<IntPair> IPMap = new List<IntPair>();

                if (i == Constants.MODEL_SIDE.SIDE) {

                    for(int j = 0; j < Constants.CHUNK_X; j++)
                        for(int k = 0; k < Constants.CHUNK_Z; k++)
                            if(Layer[j, k] != null && SearchConditions[I](j, k))
                                IPMap.Add(new IntPair(j, k));

                    IPMap.Sort();
                }

                while (true) {

                    if (i == Constants.MODEL_SIDE.SIDE) {

                        bool Found = false;
                        for(int j = 0; j < IPMap.Count; j++)
                            if (!Visited[IPMap[j].X, IPMap[j].Y]) {

                                X = IPMap[j].X;
                                Z = IPMap[j].Y;
                                Found = true;
                                break;
                            }

                        if (!Found)
                            break;
                    }
                    else {

                        for (; X < Constants.CHUNK_X && (Layer[X, Z] == null || Visited[X, Z] || !SearchConditions[I](X, Z)); Z++, X += (UInt16)(Z / Constants.CHUNK_Z), Z %= Constants.CHUNK_Z) ; // V

                        if (X >= Constants.CHUNK_X)
                            break;
                    }

                    double R = Constants.R.NextDouble();
                    double G = Constants.R.NextDouble();
                    double B = Constants.R.NextDouble();

                    RenderPiece RP = new RenderPiece(new double[] { R, G, B }, PivotX, PivotZ, H);
                    PlaneDrawSearch(X, Z, ref Visited, new Queue<IntPair>(), RP, SearchConditions[I]);
                    Pieces[I].Add(RP);
                }

                foreach (RenderPiece P in Pieces[I])
                    P.BlocksAdded(i, this.V);
            }
        }

        private void PlaneDrawSearch(int XS, int ZS, ref bool[,] Visited, Queue<IntPair> ToVisit, RenderPiece P, SearchPredicate AddCond) { 

            Visited[XS, ZS] = true;
            P.AddBlock(Layer[XS, ZS]);

            int[,] Deltas = Constants.BlockIDs;
            int L = Deltas.GetLength(0);

            for (int i = 0; i < Deltas.GetLength(0); i++)
                if (XS + Deltas[i, 0] >= 0 && XS + Deltas[i, 0] < Constants.CHUNK_X &&
                    ZS + Deltas[i, 1] >= 0 && ZS + Deltas[i, 1] < Constants.CHUNK_Z &&
                    Layer[XS + Deltas[i, 0], ZS + Deltas[i, 1]] != null &&
                    !Visited[XS + Deltas[i, 0], ZS + Deltas[i, 1]]) {
                        
                        if (AddCond != null && !AddCond(XS + Deltas[i, 0], ZS + Deltas[i, 1]))
                            continue;

                        IntPair N = new IntPair(XS + Deltas[i, 0], ZS + Deltas[i, 1]);
                        if (!ToVisit.Contains(N))
                            ToVisit.Enqueue(N);
                }

            if (ToVisit.Count == 0)
                return;

            IntPair NEW = ToVisit.Dequeue();

            PlaneDrawSearch(NEW.X, NEW.Y, ref Visited, ToVisit, P, AddCond);
        }

        public void CreateTextures() {

            for(int i = 0; i < 3; i++)
                foreach (RenderPiece RP in Pieces[i]) {

                    RP.CreateTextures();
                    RP.Triangulate();
                }
        }

        public void Draw() {

            if (!Visible)
                return;

            for(int i = 0; i < 3; i++)
                for (int j = 0; j < Pieces[i].Count; j++)
                    this.Pieces[i][j].Draw();
        }
    }
}
