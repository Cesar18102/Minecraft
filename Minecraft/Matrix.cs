using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft {

    public class Matrix {

        public int W { get; private set; }
        public int H { get; private set; }

        private double[,] M;

        public double this[int i, int j] {

            get { return M[i, j]; }
            set { M[i, j] = value; }
        }

        public Matrix(int W, int H) {

            this.W = W;
            this.H = H;

            this.M = new double[H, W];
        }

        public static Matrix operator *(Matrix M1, Matrix M2) {

            if (M1.W != M2.H)
                throw new Exception("Incompatibale matrixes");

            Matrix M3 = new Matrix(M1.H, M2.W);

            for (int i = 0; i < M1.H; i++)
                for (int j = 0; j < M2.W; j++)
                    for (int k = 0; k < M1.W; k++)
                        M3[i, j] += M1[i, k] * M2[k, j];

            return M3;
        }
    }
}
