using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minecraft.Data;

namespace Minecraft.Rendering {

    public class Visibility {

        private bool[,][] V = new bool[Constants.CHUNK_X, Constants.CHUNK_Z][];

        public bool this[int X, int Z, Constants.Planes P] {

            get { return this[X, Z, (int)P]; }
            set { this[X, Z, (int)P] = value; }
        }

        public bool this[int X, int Z, int PlaneID] {

            get { return X >= 0 && X < Constants.CHUNK_X && Z >= 0 && Z < Constants.CHUNK_Z ? V[X, Z][PlaneID] : false; }
            set { V[X % Constants.CHUNK_X, Z % Constants.CHUNK_Z][PlaneID] = value; }
        }

        public Visibility(RenderChunk R, RenderChunk[] RC) {

            for (int i = 0; i < Constants.CHUNK_X; i++)
                for (int j = 0; j < Constants.CHUNK_Z; j++) {

                    V[i, j] = new bool[6] { true, true, true, true, true, true };
                    for (int k = 0; k < 6; k++) {

                        int X = i + Constants.DeltaPlane[k, 0];
                        int Y = Constants.DeltaPlane[k, 1];
                        int Z = j + Constants.DeltaPlane[k, 2];

                        if (X >= 0 && X < Constants.CHUNK_X &&
                            Y >= 0 && Y < 1 &&
                            Z >= 0 && Z < Constants.CHUNK_Z) {

                            if (R[X, Z] != null)
                                V[i, j][k] = false;
                        }
                        else {

                            X = X < 0 ? (Constants.CHUNK_X - 1) : (X % Constants.CHUNK_X);
                            Y = 0;
                            Z = Z < 0 ? (Constants.CHUNK_Z - 1) : (Z % Constants.CHUNK_Z);

                            if (RC[k] != null && RC[k][X, Z] != null)
                                V[i, j][k] = false;
                        }
                    }
                }
        }
    }
}
