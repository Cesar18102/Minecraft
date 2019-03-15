using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft {

    public class Visibility {

        public static int[,] D = new int[6, 3]{

            {  0,  1,  0 },
            { -1,  0,  0 },
            {  0,  0,  1 },
            {  1,  0,  0 },
            {  0,  0, -1 },
            {  0, -1,  0 }
        };

        public static int[] Transform = new int[6] { 5, 3, 4, 1, 2, 0 };

        public bool[,][] V = new bool[Constants.CHUNK_X, Constants.CHUNK_Z][];

        public Visibility(RenderChunk R, RenderChunk[] RC) {

            for (int i = 0; i < Constants.CHUNK_X; i++)
                for (int j = 0; j < Constants.CHUNK_Z; j++) {

                    V[i, j] = new bool[6] { true, true, true, true, true, true };
                    for (int k = 0; k < 6; k++) {

                        int X = i + D[k, 0];
                        int Y = D[k, 1];
                        int Z = j + D[k, 2];

                        if (X >= 0 && X < Constants.CHUNK_X &&
                            Y >= 0 && Y < 1 &&
                            Z >= 0 && Z < Constants.CHUNK_Z) {

                            if (R.Layer[X, Z] != null)
                                V[i, j][k] = false;
                        }
                        else {

                            X = X < 0 ? (Constants.CHUNK_X - 1) : (X % Constants.CHUNK_X);
                            Y = 0;
                            Z = Z < 0 ? (Constants.CHUNK_Z - 1) : (Z % Constants.CHUNK_Z);

                            if (RC[k] != null && RC[k].Layer[X, Z] != null)
                                V[i, j][k] = false;
                        }
                    }
                }
        }
    }
}
