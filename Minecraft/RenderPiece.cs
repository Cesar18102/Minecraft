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
        private Constants.Planes PL = Constants.Planes.TOP;
        private Constants.MODEL_SIDE MS = Constants.MODEL_SIDE.TOP;

        private int T = 0; 
        private Polygon POLY = null;
        private Visibility V = null;

        private List<Vector3D> MODEL_PTS = new List<Vector3D>();
        private List<Vector2D> TEX_PTS = new List<Vector2D>();

        public BlockInstance this[int i] { get { return Blocks != null && i >= 0 && i < Blocks.Count ? Blocks[i] : null; } }
        public int BlocksCount { get; private set; }

        private int TotalModelPointsCount = 0;

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
            BlocksCount = Blocks.Count;
        }

        public void BlocksAdded(Constants.MODEL_SIDE MS, Visibility V) {

            this.PL = MS == Constants.MODEL_SIDE.BOT ? Constants.Planes.BOTTOM : Constants.Planes.TOP;
            this.MS = MS;
            this.V = V;

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

                if (AbsX < XM && AbsZ < ZM) {

                    XM = AbsX;
                    ZM = AbsZ;
                }
            }

            Blocks = null;

            Vector3D P0 = GetRealPoints(XM, ZM, XM - 1, ZM, (int)PL)[3];
            int LastTex = BlocksMap[XM, ZM].Planes[1].TEX_ID;
            List<IntPair> SideTexCorners = new List<IntPair>();

            MODEL_PTS.Add(P0);
            PerimeterPointSearch(XM, ZM, XM - 1, ZM, new bool[this.W, this.H, 4], P0, ref MODEL_PTS, ref SideTexIDs, (int)PL);

            if (MS == Constants.MODEL_SIDE.SIDE) {

                MODEL_PTS.Add(P0);
                SideTexIDs.Add(LastTex);
            }

            TotalModelPointsCount = MODEL_PTS.Count - 1;

            //removing imtermediate points
            for (int i = 0, t = SideTexIDs.Count - 1; i < MODEL_PTS.Count; i++) {

                int CX = 0;
                int CZ = 0;

                for (int j = i + 1; j < MODEL_PTS.Count; j++) {

                    if (MODEL_PTS[j].DX == MODEL_PTS[i].DX) CX++;
                    else if (CX != 0) break;

                    if (MODEL_PTS[j].DZ == MODEL_PTS[i].DZ) CZ++;
                    else if (CZ != 0) break;
                }

                int C = Math.Max(CX, CZ) - 1;

                if (MS == Constants.MODEL_SIDE.SIDE) {

                    SideTexCorners.Add(new IntPair(t, (t + 1) % SideTexIDs.Count));
                    t = (t + C + 1) % SideTexIDs.Count;
                }

                for (int j = 1; j <= C; j++)
                    MODEL_PTS.RemoveAt(i + 1);
            }

            // constructing side 
            if (MS == Constants.MODEL_SIDE.SIDE) {

                TotalModelPointsCount = 0;
                SideTexCorners.RemoveAt(SideTexCorners.Count - 1);
                MODEL_PTS.RemoveAt(MODEL_PTS.Count - 1);

                List<IntPair> MP = new List<IntPair>();

                Vector3D[] PTS_CP = new Vector3D[MODEL_PTS.Count];
                Array.Copy(MODEL_PTS.ToArray(), PTS_CP, PTS_CP.Length);
                List<Vector3D> PTS_CPV = PTS_CP.ToList();
                PTS_CPV.Sort();

                int FP = 0;
                for (; FP < MODEL_PTS.Count && !Visible(PTS_CPV[FP]); FP++) ;
                int FPR = MODEL_PTS.IndexOf(PTS_CPV[FP]);

                SideVisiblePlanesSearch(MODEL_PTS, FPR, ref MP);

                List<Vector3D> NewMP = new List<Vector3D>();
                List<int> NewTexIds = new List<int>();

                for (int i = 0; i < MP.Count; i++) {

                    NewMP.Add(MODEL_PTS[MP[i].X]);
                    NewMP.Add(MODEL_PTS[MP[i].Y]);

                    int SID = MP[i].X == 0 && MP[i].Y == MODEL_PTS.Count - 1 ? MODEL_PTS.Count : MP[i].X;
                    int EID = MP[i].Y == 0 && MP[i].X == MODEL_PTS.Count - 1 ? MODEL_PTS.Count : MP[i].Y;

                    int D = EID - SID;

                    int SideTexIdStart = D > 0 ? SideTexCorners[MP[i].X].Y : SideTexCorners[MP[i].X].X;
                    int SideTexIdEnd = D > 0 ? SideTexCorners[MP[i].Y].X : SideTexCorners[MP[i].Y].Y;
                    int Delta = SideTexIdStart < SideTexIdEnd ? 1 : -1;
                    TotalModelPointsCount += Math.Abs(SideTexIdEnd - SideTexIdStart) + 1;

                    for (int j = SideTexIdStart; j != SideTexIdEnd + Delta; j += Delta)
                        NewTexIds.Add(SideTexIDs[j]);
                }

                for (int i = NewMP.Count - 1; i >= 0; i--)
                    NewMP.Add(GetBottomPoint(NewMP[i]));

                MODEL_PTS = NewMP;
                SideTexIDs = NewTexIds;
            }

            float DX = (PivotX + MinX) * BlockSize.DX;
            float DZ = (PivotZ + MinZ) * BlockSize.DZ;

            // constructing texture points
            if (MS == Constants.MODEL_SIDE.BOT || MS == Constants.MODEL_SIDE.TOP) {

                for (int i = 0; i < MODEL_PTS.Count; i++) {

                    TEX_PTS.Add(new Vector2D(

                        (MODEL_PTS[i].DX - DX) / (this.W * BlockSize.DX),
                        (MODEL_PTS[i].DZ - DZ) / (this.H * BlockSize.DZ)
                    ));
                }
            }
        }

        private void SideVisiblePlanesSearch(List<Vector3D> MP, int Current, ref List<IntPair> Result) {

            int Next = (Current + 1) % MODEL_PTS.Count;
            int Prev = Current == 0 ? MODEL_PTS.Count - 1 : Current - 1;

            bool NextFound = false;
            bool PrevFound = false;

            bool NextFailed = MODEL_PTS[Current].Equals(MODEL_PTS[Next]);
            bool PrevFailed = MODEL_PTS[Current].Equals(MODEL_PTS[Prev]);

            for (int j = 0; j < W; j++) {
                for (int k = 0; k < H; k++)
                    if (BlocksMap[j, k] != null) {
                        for (int q = 1; q < 5; q++) {

                            List<Vector3D> PS = GetRealPoints(j, k, j - 1, k, q);

                            IntPairVV NP = new IntPairVV(Current, Next);
                            IntPairVV PP = new IntPairVV(Current, Prev);

                            if (!PrevFound && !NextFound && !NextFailed && !Result.Contains(NP) && PS.Contains(MODEL_PTS[Next]) && this.V.V[MinX + j, MinZ + k][q]) {

                                Result.Add(NP);
                                NextFound = true;
                            }

                            if (!NextFound && !PrevFound && !PrevFailed && !Result.Contains(PP) && PS.Contains(MODEL_PTS[Prev]) && this.V.V[MinX + j, MinZ + k][q]) {

                                Result.Add(PP);
                                PrevFound = true;
                            }

                            if (NextFound || PrevFound) break;
                        }

                        if (NextFound || PrevFound) break;
                    }

                if (NextFound || PrevFound) break;
            }

            if(!NextFound && !PrevFound)
                return;

            SideVisiblePlanesSearch(MP, NextFound? Next : Prev, ref Result);
        }

        private bool Visible(Vector3D P) {

            for (int j = 0; j < W; j++)
                for (int k = 0; k < H; k++)
                    if (BlocksMap[j, k] != null)
                        for (int q = 1; q < 5; q++) {

                            List<Vector3D> PS = GetRealPoints(j, k, j - 1, k, q);

                            if (PS.Contains(P) && this.V.V[MinX + j, MinZ + k][q])
                                return true;
                        }

            return false;
        }

        public void CreateTextures() {

            Dictionary<IntPair, int> TEX = new Dictionary<IntPair, int>();

            if (MS == Constants.MODEL_SIDE.BOT || MS == Constants.MODEL_SIDE.TOP) {

                for (int i = 0; i < this.W; i++)
                    for (int j = 0; j < this.H; j++)
                        if (this.BlocksMap[i, j] != null)
                            TEX.Add(new IntPair(i, j), this.BlocksMap[i, j].Planes[(int)PL].TEX_ID);

                T = ItemsSet.Add(new Texture(TEX, this.W, this.H, this.UniTexW * this.W, this.UniTexH * this.H, Color, false));
            }
            else {

                for (int i = 0; i < SideTexIDs.Count; i++)
                    TEX.Add(new IntPair(i, 0), SideTexIDs[i]);

                T = ItemsSet.Add(new Texture(TEX, SideTexIDs.Count, 1, this.UniTexW * SideTexIDs.Count, this.UniTexH, Color, false));

                float TEX_DX = 0;

                for (int i = 0; i < MODEL_PTS.Count / 2; i++) {

                    float D = Math.Max(Math.Abs(MODEL_PTS[i].DX - MODEL_PTS[i + 1].DX), Math.Abs(MODEL_PTS[i].DZ - MODEL_PTS[i + 1].DZ)) / (TotalModelPointsCount * BlockSize.DX) + TEX_DX;

                    TEX_PTS.Add(new Vector2D(TEX_DX, 0));
                    TEX_PTS.Add(new Vector2D(D, 0));
                    TEX_PTS.Add(new Vector2D(D, 1));
                    TEX_PTS.Add(new Vector2D(TEX_DX, 1));

                    TEX_DX = D % 1.0f;
                }
            }
        }

        public void Triangulate() {

            if (MS == Constants.MODEL_SIDE.SIDE)
                return;

            POLY = new Polygon(MODEL_PTS, TEX_PTS);
            POLY.Triangulate();
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

            PerimeterPointSearch(NNX, NNZ, X, Z, V, LastPoint, ref PTS_BUFFER, ref TexSideIDs, PlaneID);
        }

        private List<Vector3D> GetRealPoints(int X, int Z, int OX, int OZ, int PlaneID) {

            if (this.BlocksMap[X, Z] == null)
                return new List<Vector3D>();

            List<Vector3D> PTS_Ratio = this.BlocksMap[X, Z].ModelPoints;
            List<UInt16> PTS_IDs = this.BlocksMap[X, Z].Planes[PlaneID].PlanePointSquence;
            Vector3D SZ = this.BlocksMap[X, Z].Size;

            List<Vector3D> RP = new List<Vector3D>();

            int Dir = 0;
            for (; Dir < Constants.CornerDS.GetLength(0) && (Constants.CornerDS[Dir, 0] != X - OX || Constants.CornerDS[Dir, 1] != Z - OZ); Dir++) ;

            for (int i = 0; i < Constants.CornerIDs.GetLength(1); i++)
                RP.Add(new Vector3D(PTS_Ratio[PTS_IDs[Constants.CornerIDs[Dir, i]]].DX + (PivotX + MinX + X + 0.5f) * SZ.DX,
                                    PTS_Ratio[PTS_IDs[Constants.CornerIDs[Dir, i]]].DY + (PY - 0.5f) * SZ.DY,
                                    PTS_Ratio[PTS_IDs[Constants.CornerIDs[Dir, i]]].DZ + (PivotZ + MinZ + Z + 0.5f) * SZ.DZ));

            if (PlaneID == (int)Constants.Planes.BOTTOM)
                RP.Reverse();

            return RP;
        }

        private Vector3D GetBottomPoint(Vector3D P) {

            return new Vector3D(P.DX, P.DY - BlockSize.DY, P.DZ);
        }

        private int GetAbsCornerID(int X, int Z, int OX, int OZ, int CI) {

            int Dir = 0;
            for (; Dir < Constants.CornerDS.GetLength(0) && (Constants.CornerDS[Dir, 0] != X - OX || Constants.CornerDS[Dir, 1] != Z - OZ); Dir++) ;

            return Constants.CornerIDs[Dir, CI];
        }

        private void DrawTOP() {


        }

        public void Draw()
        {

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

            if (MS == Constants.MODEL_SIDE.TOP || MS == Constants.MODEL_SIDE.BOT) {

                ItemsSet.TEXTURES[T].Bind();

                for (int j = 0; j < POLY.MTR.Count; j++) {

                    Gl.glBegin(Gl.GL_POLYGON);

                        for (int k = 0; k < POLY.MTR[j].V.Length; k++) {

                            Gl.glTexCoord2f(POLY.TTR[j].V[k].DX, POLY.TTR[j].V[k].DY);
                            Gl.glVertex3f(POLY.MTR[j].V[k].DX, POLY.MTR[j].V[k].DY, POLY.MTR[j].V[k].DZ);
                        }

                    Gl.glEnd();
                }

            }
            else {

                for (int j = 0; j < MODEL_PTS.Count / 2 - 1; j += 2) {

                    ItemsSet.TEXTURES[T].Bind();

                    Gl.glBegin(Gl.GL_POLYGON);

                        Gl.glTexCoord2f(TEX_PTS[4 * j].DX, TEX_PTS[4 * j].DY);
                        Gl.glVertex3f(MODEL_PTS[j].DX, MODEL_PTS[j].DY, MODEL_PTS[j].DZ);

                        Gl.glTexCoord2f(TEX_PTS[4 * j + 1].DX, TEX_PTS[4 * j + 1].DY);
                        Gl.glVertex3f(MODEL_PTS[j + 1].DX, MODEL_PTS[j + 1].DY, MODEL_PTS[j + 1].DZ);

                        Gl.glTexCoord2f(TEX_PTS[4 * j + 2].DX, TEX_PTS[4 * j + 2].DY);
                        Gl.glVertex3f(MODEL_PTS[MODEL_PTS.Count - j - 2].DX, MODEL_PTS[MODEL_PTS.Count - j - 2].DY, MODEL_PTS[MODEL_PTS.Count - j - 2].DZ);

                        Gl.glTexCoord2f(TEX_PTS[4 * j + 3].DX, TEX_PTS[4 * j + 3].DY);
                        Gl.glVertex3f(MODEL_PTS[MODEL_PTS.Count - j - 1].DX, MODEL_PTS[MODEL_PTS.Count - j - 1].DY, MODEL_PTS[MODEL_PTS.Count - j - 1].DZ);

                    Gl.glEnd();
                }
            }
        }
    }
}
