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

        public Texture[] T = new Texture[3];
        public List<Vector3D> MODEL_PTS = new List<Vector3D>();
        public List<Vector2D> TEX_PTS = new List<Vector2D>();

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

        public int[] CornerABSes = new int[4] { 4, 5, 1, 0 };

        public RenderPiece(double[] Color, Int64 PivotX, Int64 PivotZ, int PY) {

            this.PivotX = PivotX;
            this.PivotZ = PivotZ;
            this.PY = PY;

            Array.Copy(Color, this.Color, Color.Length);
        }

        public void AddBlock(BlockInstance B) {

            this.Blocks.Add(B);

            if (B.X > MaxX) MaxX = B.X;
            if (B.X < MinX) MinX = B.X;
            if (B.Z > MaxZ) MaxZ = B.Z;
            if (B.Z < MinZ) MinZ = B.Z;

            this.BlockSize = B.Size;
        }

        public void CreateTextures() {

            this.W = MaxX - MinX + 1;
            this.H = MaxZ - MinZ + 1;

            this.BlocksMap = new BlockInstance[this.W, this.H];
            Bitmap[,] TOP = new Bitmap[this.W, this.H];

            int W = Blocks[0].Planes[0].TEXTURE.BUP.Width;
            int H = Blocks[0].Planes[0].TEXTURE.BUP.Height;

            int XM = this.W;
            int ZM = this.H;

            foreach (BlockInstance B in Blocks) {

                this.BlocksMap[B.X - MinX, B.Z - MinZ] = B;
                TOP[B.X - MinX, B.Z - MinZ] = B.Planes[0].TEXTURE.BUP;

                if (B.X - MinX < XM && B.Z - MinZ < ZM) {

                    XM = B.X - MinX;
                    ZM = B.Z - MinZ;
                }
            }

            T[0] = new Texture(ref TOP, W * this.W, H * this.H, Color);
            TOP = null;
            ItemsSet.TEXTURES.Add(T[0]);

            Vector3D P0 = GetRealPoints(XM, ZM, XM - 1, ZM)[3];

            MODEL_PTS.Add(P0);
            PerimeterPointSearch(XM, ZM, XM - 1, ZM, new bool[this.W, this.H, 4], P0);
            MODEL_PTS.Add(P0);

            for (int i = 0; i < MODEL_PTS.Count; i++) {

                int CX = 0;
                int CZ = 0;

                for (int j = i + 1; j < MODEL_PTS.Count; j++) {

                    if (MODEL_PTS[j].DX == MODEL_PTS[i].DX) CX++;
                    else if (CX != 0) break;

                    if (MODEL_PTS[j].DZ == MODEL_PTS[i].DZ) CZ++;
                    else if (CZ != 0) break;
                }

                int C = Math.Max(CX, CZ) - 1;

                for (int j = 1; j <= C; j++)
                    MODEL_PTS.RemoveAt(i + 1);
            }

            float DX = (PivotX + MinX) * BlockSize.DX;
            float DZ = (PivotZ + MinZ) * BlockSize.DZ;

            for (int i = 0; i < MODEL_PTS.Count; i++)
                TEX_PTS.Add(new Vector2D(

                    (MODEL_PTS[i].DX - DX) / (this.W * BlockSize.DX), 
                    (MODEL_PTS[i].DZ - DZ) / (this.H * BlockSize.DZ)
                ));
        }

        private void PerimeterPointSearch(int X, int Z, int OX, int OZ, bool[,,] V, Vector3D LastPoint) {

            List<Vector3D> P = GetRealPoints(X, Z, OX, OZ);
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

                        List<Vector3D> NP = GetRealPoints(NX, NZ, X, Z);
                        if (NP.Contains(LastPoint) && NP.Contains(P[i])) {

                            Outer = false;
                            break;
                        }
                    }
                }

                int AbsID = GetAbsCornerID(X, Z, OX, OZ, i);

                if (Outer && !V[X, Z, AbsID]) {

                    MODEL_PTS.Add(P[i]);
                    LastPoint = P[i];
                    V[X, Z, AbsID] = true;
                }
                else
                    break;
            } // Passing around quad

            int NNX = X, NNZ = Z;

            for (int j = 0; j < this.BlockIDsLen; j++) {

                int RNX = X + Constants.BlockIDs[j, 0];
                int RNZ = Z + Constants.BlockIDs[j, 1];

                if (RNX >= 0 && RNX < W && RNZ >= 0 && RNZ < H && this.BlocksMap[RNX, RNZ] != null) {

                    List<Vector3D> NP = GetRealPoints(RNX, RNZ, X, Z);
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

            PerimeterPointSearch(NNX, NNZ, X, Z, V, LastPoint);
        }

        private List<Vector3D> GetRealPoints(int X, int Z, int OX, int OZ) {

            if (this.BlocksMap[X, Z] == null)
                return new List<Vector3D>();

            List<Vector3D> PTS_Ratio = this.BlocksMap[X, Z].ModelPoints;
            List<UInt16> PTS_IDs = this.BlocksMap[X, Z].Planes[0].PlanePointSquence;
            Vector3D SZ = this.BlocksMap[X, Z].Size;

            List<Vector3D> RP = new List<Vector3D>();

            int Dir = 0;
            for (; Dir < CornerDS.GetLength(0) && (CornerDS[Dir, 0] != X - OX || CornerDS[Dir, 1] != Z - OZ); Dir++) ;// something wrong

            for (int i = 0; i < CornerIDs.GetLength(1); i++)
                RP.Add(new Vector3D(PTS_Ratio[PTS_IDs[CornerIDs[Dir, i]]].DX + (PivotX + MinX + X + 0.5f) * SZ.DX, 
                                    PTS_Ratio[PTS_IDs[CornerIDs[Dir, i]]].DY + (PY - 0.5f) * SZ.DY, 
                                    PTS_Ratio[PTS_IDs[CornerIDs[Dir, i]]].DZ + (PivotZ + MinZ + Z + 0.5f) * SZ.DZ));

            return RP;
        }

        private int GetAbsCornerID(int X, int Z, int OX, int OZ, int CI) {

            int Dir = 0;
            for (; Dir < CornerDS.GetLength(0) && (CornerDS[Dir, 0] != X - OX || CornerDS[Dir, 1] != Z - OZ); Dir++) ;

            return this.CornerIDs[Dir, CI];
        }

        public void Draw() {

            Gl.glColor3d(Color[0], Color[1], Color[2]);

            /*for (int i = 0; i < Blocks.Count; i++)
                Blocks[i].Draw(PivotX, PivotZ);*/

            T[0].Bind();

            Gl.glBegin(Gl.GL_POLYGON);

                for (int i = 0; i < MODEL_PTS.Count; i++) {

                    Gl.glTexCoord2f(TEX_PTS[i].DX, TEX_PTS[i].DY);
                    Gl.glVertex3f(MODEL_PTS[i].DX, MODEL_PTS[i].DY, MODEL_PTS[i].DZ);
                }

            Gl.glEnd();
        }
    }
}
