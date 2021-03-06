﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft.Support {

    public class Vector3D : IComparable, IVector {

        public float DX { get; private set; }
        public float DY { get; private set; }
        public float DZ { get; private set; }

        public float Length { get { return (float)Math.Sqrt(DX * DX + DY * DY + DZ * DZ); } }

        public Vector3D(float dX, float dY, float dZ) {

            this.DX = dX;
            this.DY = dY;
            this.DZ = dZ;
        }

        public Vector3D GetRotatedVectorZX(float OXZA) {

            Vector2D OXZ = new Vector2D(DX, DZ).GetRotatedVector(OXZA);

            return new Vector3D(OXZ.DX, DY, OXZ.DY).GetUnityVector();
        }

        public Vector3D GetRotatedVectorY(float OYA, float XZA) {

            Vector2D OKY = new Vector2D((float)Math.Sqrt(DX * DX + DZ * DZ), DY).GetRotatedVector(OYA);

            return new Vector3D(OKY.DX, OKY.DY, 0).GetRotatedVectorZX(XZA).GetUnityVector();
        }

        public Vector3D(Vector3D E, Vector3D S) {

            this.DX = E.DX - S.DX;
            this.DY = E.DY - S.DY;
            this.DZ = E.DZ - S.DZ;
        }

        public Vector3D PointsTo(Vector3D FROM) {

            return new Vector3D(FROM.DX + this.DX,
                                FROM.DY + this.DY,
                                FROM.DZ + this.DZ);
        }

        public Vector3D GetUnityVector() {

            float L = (float)Math.Sqrt(DX * DX + DY * DY + DZ * DZ);
            return new Vector3D(DX / L, DY / L, DZ / L);
        }

        public override bool Equals(object obj) {

            if (!obj.GetType().Equals(typeof(Vector3D)))
                return false;

            Vector3D V3D = obj as Vector3D;

            return V3D.DX == this.DX && V3D.DY == this.DY && V3D.DZ == this.DZ;
        }

        public int CompareTo(object obj) {

            if (obj == null)
                return 1;

            Vector3D V = obj as Vector3D;

            return this.Length > V.Length ? 1 : (this.Length < V.Length ? -1 : 0);
        }
    }
}
