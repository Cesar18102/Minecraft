using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Tao.OpenGl;

namespace Minecraft {

    public class RenderPiece {

        public Int64 PivotX { get; private set; }
        public Int64 PivotZ { get; private set; }

        public int W { get; private set; }
        public int H { get; private set; }

        public int UniTexW { get; private set; }
        public int UniTexH { get; private set; }

        public int PY { get; private set; }

        private List<BlockInstance> Blocks = new List<BlockInstance>();
        private BlockInstance[,] BlocksMap;
        private double[] Color = new double[3];

        private int MinX = Constants.CHUNK_X + 1;
        private int MaxX = -1;

        private int MinZ = Constants.CHUNK_Z + 1;
        private int MaxZ = -1;

        private Vector3D BlockSize;

        private int BlockIDsLen = Constants.BlockIDs.GetLength(0);
        private int FullBlockIDsLen = Constants.FullBlockIDs.GetLength(0);

        private List<int> SideTexIDs = new List<int>();

        private int[] T = new int[3]; // TOP, BOT, SIDE
        private bool[] Visible = new bool[3] { true, true, true };
        private List<Vector3D>[] MODEL_PTS = new List<Vector3D>[3];
        private List<Vector2D>[] TEX_PTS = new List<Vector2D>[3];

        private Polygon[] POLYS = new Polygon[2];

        public BlockInstance this[int i] { get { return Blocks != null && i >= 0 && i < Blocks.Count ? Blocks[i] : null; } }
        public int BlocksCount { get; private set; }

        private int TotalModelPointsCount = 0;

        public enum MODEL_SIDE : int {

            TOP,
            BOT,
            SIDE
        }

        public int[,] CornerIDs = new int[4, 4] {

            { 3, 2, 1, 0 },
            { 0, 3, 2, 1 },
            { 2, 1, 0, 3 },
            { 1, 0, 3, 2 }
        };

        public int[,] CornerDS = new int[4, 2] {

            {  1,  0  },
            {  0, -1  },
            {  0,  1  },
            { -1,  0  }
        };

        public RenderPiece(double[] Color, Int64 PivotX, Int64 PivotZ, int PY) {

            this.PivotX = PivotX;
            this.PivotZ = PivotZ;
            this.PY = PY;

            Array.Copy(Color, this.Color, Color.Length);
        }

        public void SetVisibility(bool TOP, bool BOT, bool SIDE) {

            Visible[(int)MODEL_SIDE.TOP] = TOP;
            Visible[(int)MODEL_SIDE.BOT] = BOT;
            Visible[(int)MODEL_SIDE.SIDE] = SIDE;
        }

        public void AddBlock(BlockInstance B) {

            this.Blocks.Add(B);

            if (B.X > MaxX) MaxX = B.X;
            if (B.X < MinX) MinX = B.X;
            if (B.Z > MaxZ) MaxZ = B.Z;
            if (B.Z < MinZ) MinZ = B.Z;

            this.BlockSize = B.Size;
            BlocksCount = Blocks.Count;
        }

        public void BlocksAdded() {

            this.W = MaxX - MinX + 1;
            this.H = MaxZ - MinZ + 1;

            this.UniTexW = ItemsSet.TEXTURES[Blocks[0].Planes[0].TEX_ID].Width;
            this.UniTexH = ItemsSet.TEXTURES[Blocks[0].Planes[0].TEX_ID].Height;

            this.BlocksMap = new BlockInstance[this.W, this.H];

            int XM = this.W;
            int ZM = this.H;

            foreach (BlockInstance B in Blocks) {

                int AbsX = B.X - MinX;
                int AbsZ = B.Z - MinZ;

                this.BlocksMap[AbsX, AbsZ] = B;

                if (AbsX < XM && AbsZ < ZM)
                {

                    XM = AbsX;
                    ZM = AbsZ;
                }
            }

            Blocks = null;

            for (int i = 0; i < 3; i++) {

                MODEL_PTS[i] = new List<Vector3D>();
                TEX_PTS[i] = new List<Vector2D>();
            }

            Vector3D P0 = GetRealPoints(XM, ZM, XM - 1, ZM, 0)[3];

            int LastTex = BlocksMap[XM, ZM].Planes[1].TEX_ID;

            MODEL_PTS[0].Add(P0);
            PerimeterPointSearch(XM, ZM, XM - 1, ZM, new bool[this.W, this.H, 4], P0, ref MODEL_PTS[0], ref SideTexIDs);
            MODEL_PTS[0].Add(P0);
            SideTexIDs.Add(LastTex);

            TotalModelPointsCount = MODEL_PTS[0].Count - 1;

            //removing imtermediate points
            for (int i = 0; i < MODEL_PTS[0].Count; i++) {

                int CX = 0;
                int CZ = 0;

                for (int j = i + 1; j < MODEL_PTS[0].Count; j++)
                {

                    if (MODEL_PTS[0][j].DX == MODEL_PTS[0][i].DX) CX++;
                    else if (CX != 0) break;

                    if (MODEL_PTS[0][j].DZ == MODEL_PTS[0][i].DZ) CZ++;
                    else if (CZ != 0) break;
                }

                int C = Math.Max(CX, CZ) - 1;

                for (int j = 1; j <= C; j++)
                    MODEL_PTS[0].RemoveAt(i + 1);
            }

            // constructing bottom
            for (int i = 0; i < MODEL_PTS[0].Count; i++)
                MODEL_PTS[1].Add(GetBottomPoint(MODEL_PTS[0][i]));

            // constructing side 
            for (int i = 0; i < MODEL_PTS[0].Count; i++)
                MODEL_PTS[2].Add(MODEL_PTS[0][i]);

            for (int i = MODEL_PTS[0].Count - 1; i >= 0; i--)
                MODEL_PTS[2].Add(MODEL_PTS[1][i]);

            float DX = (PivotX + MinX) * BlockSize.DX;
            float DZ = (PivotZ + MinZ) * BlockSize.DZ;

            // constructing texture points
            for (int i = 0; i < MODEL_PTS[0].Count; i++) {

                TEX_PTS[0].Add(new Vector2D(

                    (MODEL_PTS[0][i].DX - DX) / (this.W * BlockSize.DX),
                    (MODEL_PTS[0][i].DZ - DZ) / (this.H * BlockSize.DZ)
                ));

                TEX_PTS[1].Add(TEX_PTS[0].Last());
            }
        }

        public void CreateTextures() {

            Dictionary<IntPair, int> TOP = new Dictionary<IntPair, int>();
            Dictionary<IntPair, int> BOT = new Dictionary<IntPair, int>();
            Dictionary<IntPair, int> SIDE = new Dictionary<IntPair, int>();

            for (int i = 0; i < this.W; i++)
                for (int j = 0; j < this.H; j++)
                    if (this.BlocksMap[i, j] != null) {

                        TOP.Add(new IntPair(i, j), this.BlocksMap[i, j].Planes[0].TEX_ID);
                        BOT.Add(new IntPair(i, j), this.BlocksMap[i, j].Planes[5].TEX_ID);
                    }
                        
            T[0] = ItemsSet.Add(new Texture(TOP, this.W, this.H, this.UniTexW * this.W, this.UniTexH * this.H, Color, false));
            T[1] = ItemsSet.Add(new Texture(BOT, this.W, this.H, this.UniTexW * this.W, this.UniTexH * this.H, Color, false));

            for (int i = 0; i < SideTexIDs.Count; i++)
                SIDE.Add(new IntPair(i, 0), SideTexIDs[i]);

            T[2] = ItemsSet.Add(new Texture(SIDE, SideTexIDs.Count, 1, this.UniTexW * SideTexIDs.Count, this.UniTexH, Color, false));

            float TEX_DX = 0;

            for (int i = 0; i < MODEL_PTS[2].Count / 2; i++) {

                float D = Math.Max(Math.Abs(MODEL_PTS[2][i].DX - MODEL_PTS[2][i + 1].DX), Math.Abs(MODEL_PTS[2][i].DZ - MODEL_PTS[2][i + 1].DZ)) / (TotalModelPointsCount * BlockSize.DX) + TEX_DX;

                TEX_PTS[2].Add(new Vector2D(TEX_DX, 0));
                TEX_PTS[2].Add(new Vector2D(D, 0));
                TEX_PTS[2].Add(new Vector2D(D, 1));
                TEX_PTS[2].Add(new Vector2D(TEX_DX, 1));

                TEX_DX = D % 1.0f;
            }
        }

        public void Triangulate() {

            for (int i = 0; i < 2; i++) {

                POLYS[i] = new Polygon(MODEL_PTS[i], TEX_PTS[i]);
                POLYS[i].Triangulate();
            }
        }

        private void PerimeterPointSearch(int X, int Z, int OX, int OZ, bool[,,] V, Vector3D LastPoint, ref List<Vector3D> PTS_BUFFER, ref List<int> TexSideIDs, int PlaneID = 0) {

            List<Vector3D> P = GetRealPoints(X, Z, OX, OZ, PlaneID);
            int LastPID = P.IndexOf(LastPoint);
            V[X, Z, GetAbsCornerID(X, Z, OX, OZ, LastPID)] = true;

            for (int i = (LastPID + 1) % P.Count; i < P.Count; i++) {

                bool Outer = true;

                for (int j = 0; j < this.BlockIDsLen; j++) {

                    int NX = X + Constants.BlockIDs[j, 0];
                    int NZ = Z + Constants.BlockIDs[j, 1];

                    if (Math.Abs(new Vector2D(P[i].DX - LastPoint.DX, P[i].DZ - LastPoint.DZ).Length - BlockSize.DX) > 0.001) {

                        Outer = false;
                        break;
                    }

                    if (NX >= 0 && NX < W && NZ >= 0 && NZ < H && this.BlocksMap[NX, NZ] != null) {

                        List<Vector3D> NP = GetRealPoints(NX, NZ, X, Z, PlaneID);
                        if (NP.Contains(LastPoint) && NP.Contains(P[i])) {

                            Outer = false;
                            break;
                        }
                    }
                }

                int AbsID = GetAbsCornerID(X, Z, OX, OZ, i);

                if (Outer && !V[X, Z, AbsID]) {

                    PTS_BUFFER.Add(P[i]);

                    for (int j = 1; j < 5; j++) {

                        List<Vector3D> SP = GetRealPoints(X, Z, OX, OZ, j);

                        if (SP.Contains(LastPoint) && SP.Contains(P[i])) {

                            TexSideIDs.Add(BlocksMap[X, Z].Planes[j].TEX_ID);
                            break;
                        }
                    }

                    LastPoint = P[i];
                    V[X, Z, AbsID] = true;
                }
                else
                    break;
            }

            int NNX = X, NNZ = Z;

            for (int j = 0; j < this.BlockIDsLen; j++) {

                int RNX = X + Constants.BlockIDs[j, 0];
                int RNZ = Z + Constants.BlockIDs[j, 1];

                if (RNX >= 0 && RNX < W && RNZ >= 0 && RNZ < H && this.BlocksMap[RNX, RNZ] != null) {

                    List<Vector3D> NP = GetRealPoints(RNX, RNZ, X, Z, PlaneID);
                    int PID = NP.IndexOf(LastPoint);

                    if (PID != -1 && !V[RNX, RNZ, GetAbsCornerID(RNX, RNZ, X, Z, PID)]) {

                        NNX = RNX;
                        NNZ = RNZ;
                        break;
                    }
                }
            }

            if (NNX == X && NNZ == Z)
                return;

            PerimeterPointSearch(NNX, NNZ, X, Z, V, LastPoint, ref PTS_BUFFER, ref TexSideIDs);
        }

        private List<Vector3D> GetRealPoints(int X, int Z, int OX, int OZ, int PlaneID) {

            if (this.BlocksMap[X, Z] == null)
                return new List<Vector3D>();

            List<Vector3D> PTS_Ratio = this.BlocksMap[X, Z].ModelPoints;
            List<UInt16> PTS_IDs = this.BlocksMap[X, Z].Planes[PlaneID].PlanePointSquence;
            Vector3D SZ = this.BlocksMap[X, Z].Size;

            List<Vector3D> RP = new List<Vector3D>();

            int Dir = 0;
            for (; Dir < CornerDS.GetLength(0) && (CornerDS[Dir, 0] != X - OX || CornerDS[Dir, 1] != Z - OZ); Dir++) ;

            for (int i = 0; i < CornerIDs.GetLength(1); i++)
                RP.Add(new Vector3D(PTS_Ratio[PTS_IDs[CornerIDs[Dir, i]]].DX + (PivotX + MinX + X + 0.5f) * SZ.DX, 
                                    PTS_Ratio[PTS_IDs[CornerIDs[Dir, i]]].DY + (PY - 0.5f) * SZ.DY, 
                                    PTS_Ratio[PTS_IDs[CornerIDs[Dir, i]]].DZ + (PivotZ + MinZ + Z + 0.5f) * SZ.DZ));

            return RP;
        }

        private Vector3D GetBottomPoint(Vector3D P) {

            return new Vector3D(P.DX, P.DY - BlockSize.DY, P.DZ);
        }

        private int GetAbsCornerID(int X, int Z, int OX, int OZ, int CI) {

            int Dir = 0;
            for (; Dir < CornerDS.GetLength(0) && (CornerDS[Dir, 0] != X - OX || CornerDS[Dir, 1] != Z - OZ); Dir++) ;

            return this.CornerIDs[Dir, CI];
        }

        private void DrawTOP() {


        }

        public void Draw() { // draw polygon without "ears", remove holes, do not render invisible pieces

            Gl.glColor3d(Color[0], Color[1], Color[2]);
            //Gl.glEnable(Gl.GL_LIGHT0);

            Gl.glLightModelfv(Gl.GL_LIGHT_MODEL_AMBIENT, new float[] { 1f, 1f, 1f, 1f });
            
            /*Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, new float[] { 5, 10, 5, 1 });
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_SPOT_DIRECTION, new float[] { 0, -1, 0 });
            //Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_CONSTANT_ATTENUATION, new float[] { 0.00001f });

            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, new float[] { 0.5f, 0.5f, 0, 1f });
            //Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_SPECULAR, new float[] { 0.5f, 0.5f, 0.5f , 1f });
            //Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, new float[] { 0.2f, 0.2f, 0.2f, 1f });

            Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_DIFFUSE, new float[] { 0.5f, 0.5f, 0, 1f });
            //Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_SPECULAR, new float[] { 0.5f, 0.5f, 0.5f, 1f });
            //Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_AMBIENT, new float[] { 0.2f, 0.2f, 0.2f, 1f });*/

            for (int i = 0; i < 2; i++) {

                if (!Visible[i])
                    continue;

                ItemsSet.TEXTURES[T[i]].Bind();

                for (int j = 0; j < POLYS[i].MTR.Count; j++) {

                    Gl.glBegin(Gl.GL_POLYGON);

                    for (int k = 0; k < POLYS[i].MTR[j].V.Length; k++) {

                        Gl.glTexCoord2f(POLYS[i].TTR[j].V[k].DX, POLYS[i].TTR[j].V[k].DY);
                        Gl.glVertex3f(POLYS[i].MTR[j].V[k].DX, POLYS[i].MTR[j].V[k].DY, POLYS[i].MTR[j].V[k].DZ);
                    }

                    Gl.glEnd();
                }
            }

            for (int j = 0; j < MODEL_PTS[2].Count / 2 - 1; j++) {

                ItemsSet.TEXTURES[T[2]].Bind();

                Gl.glBegin(Gl.GL_POLYGON);

                    Gl.glTexCoord2f(TEX_PTS[2][4 * j].DX, TEX_PTS[2][4 * j].DY);
                    Gl.glVertex3f(MODEL_PTS[2][j].DX, MODEL_PTS[2][j].DY, MODEL_PTS[2][j].DZ);

                    Gl.glTexCoord2f(TEX_PTS[2][4 * j + 1].DX, TEX_PTS[2][4 * j + 1].DY);
                    Gl.glVertex3f(MODEL_PTS[2][j + 1].DX, MODEL_PTS[2][j + 1].DY, MODEL_PTS[2][j + 1].DZ);

                    Gl.glTexCoord2f(TEX_PTS[2][4 * j + 2].DX, TEX_PTS[2][4 * j + 2].DY);
                    Gl.glVertex3f(MODEL_PTS[2][MODEL_PTS[2].Count - j - 2].DX, MODEL_PTS[2][MODEL_PTS[2].Count - j - 2].DY, MODEL_PTS[2][MODEL_PTS[2].Count - j - 2].DZ);

                    Gl.glTexCoord2f(TEX_PTS[2][4 * j + 3].DX, TEX_PTS[2][4 * j + 3].DY);
                    Gl.glVertex3f(MODEL_PTS[2][MODEL_PTS[2].Count - j - 1].DX, MODEL_PTS[2][MODEL_PTS[2].Count - j - 1].DY, MODEL_PTS[2][MODEL_PTS[2].Count - j - 1].DZ);

                Gl.glEnd();
            }
        }
    }
}
