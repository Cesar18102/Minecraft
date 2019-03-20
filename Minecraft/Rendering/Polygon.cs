using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minecraft.Support;

namespace Minecraft.Rendering {
    public class Polygon {

        private List<Vector3D> V = new List<Vector3D>();
        private List<Vector2D> T = new List<Vector2D>();
        private bool[] Visited = null;

        public List<Triangle<Vector3D>> MTR = new List<Triangle<Vector3D>>();
        public List<Triangle<Vector2D>> TTR = new List<Triangle<Vector2D>>();

        public Polygon(IEnumerable<Vector3D> V, IEnumerable<Vector2D> T) {

            this.V = V.ToList();
            this.V.Reverse();

            this.T = T.ToList();
            this.T.Reverse();

            this.Visited = new bool[this.V.Count];
        }

        public void Triangulate() {

            int A = 0;
            int B = FindNextPosition(A + 1);
            int C = FindNextPosition(B + 1);

            int LeftPoints = V.Count;
            int Steps = 0;

            while (LeftPoints > 3) {

                if (IsLeft(V[A], V[B], V[C]) && InnerTriangle(A, B, C)) {

                    MTR.Add(new Triangle<Vector3D>(V[A], V[B], V[C]));
                    TTR.Add(new Triangle<Vector2D>(T[A], T[B], T[C])); 

                    Visited[B] = true;
                    LeftPoints--;
                    B = C;
                    C = FindNextPosition(C + 1);
                }
                else {

                    A = FindNextPosition(A + 1);
                    B = FindNextPosition(B + 1);
                    C = FindNextPosition(C + 1);
                }

                if (Steps > V.Count * V.Count) {

                    MTR.Clear();
                    TTR.Clear();
                    break;
                }

                Steps++;
            }

            if (MTR != null) {

                MTR.Add(new Triangle<Vector3D>(V[A], V[B], V[C]));
                TTR.Add(new Triangle<Vector2D>(T[A], T[B], T[C]));
            }
        }

        private int FindNextPosition(int POS) {

            POS = POS % V.Count;

            if (!Visited[POS])
                return POS;

            for (int i = (POS + 1) % V.Count; i != POS; i = (i + 1) % V.Count)
                if (!Visited[i])
                    return i;

            return -1;
        }

        private bool IsLeft(Vector3D A, Vector3D B, Vector3D C) {

            float abX = B.DX - A.DX;
            float abY = B.DZ - A.DZ;
            float acX = C.DX - A.DX;
            float acY = C.DZ - A.DZ;

            return abX * acY - acX * abY < 0;
        }

        private bool InnerTriangle(int A, int B, int C) {

            for (int i = 0; i < V.Count; i++)
                if (i != A && i != B && i != C)
                    if(IsPointInside(V[A], V[B], V[C], V[i]))
                        return false;
            return true;
        }

        private bool IsPointInside(Vector3D A, Vector3D B, Vector3D C, Vector3D P) {

            float ab = (A.DX - P.DX) * (B.DZ - A.DZ) - (B.DX - A.DX) * (A.DZ - P.DZ);
            float bc = (B.DX - P.DX) * (C.DZ - B.DZ) - (C.DX - B.DX) * (B.DZ - P.DZ);
            float ca = (C.DX - P.DX) * (A.DZ - C.DZ) - (A.DX - C.DX) * (C.DZ - P.DZ);

            return (ab >= 0 && bc >= 0 && ca >= 0) || (ab <= 0 && bc <= 0 && ca <= 0);
        }
    }
}
