using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft.Data {

    public static class Constants {

        public static string RootDir = Environment.CurrentDirectory;
        public static string DataDir = RootDir + "/Data";
        public static string initFilePath = DataDir + "/conf.ini";

        public static Random R = new Random();

        public static bool GraphicsBusy = false;

        public delegate bool StateBounds<T>(T val);

        public static UInt16 CHUNK_X = 16;
        public static UInt16 CHUNK_Y = 64;
        public static UInt16 CHUNK_Z = 16;

        public static UInt16 RenderDistance = 1;
        public static UInt16 ShortRenderDistance = 0;
        public static UInt16 DYRender = (UInt16)(CHUNK_Y / (RenderDistance - ShortRenderDistance));

        public static int[,] BlockIDs = new int[4, 2] {

            {  0,  1  }, //BOTTOM
            {  1,  0  }, //RIGHT
            {  0, -1  }, //TOP
            { -1,  0  }  //LEFT
        };

        public enum BlockIDsIDs : int {

            BOTTOM = 0,
            RIGHT = 1,
            TOP = 2,
            LEFT = 3
        };

        public static int[] BlockPlaneIDs = new int[4] { 

            (int)Planes.FRONT, 
            (int)Planes.RIGHT,
            (int)Planes.BACK,
            (int)Planes.LEFT
        };

        public static int[] BlockPlaneIDsBack = new int[4] { 

            (int)Planes.FRONT,
            (int)Planes.LEFT,
            (int)Planes.BACK,
            (int)Planes.RIGHT
        };

        public static int[] BlockToChunkIDs = new int[4] {

            (int)Chunks.DOWN,
            (int)Chunks.RIGHT,
            (int)Chunks.UP,
            (int)Chunks.LEFT
        };

        public static int LoopOverflow(int X, int Max) {

            return X < 0 ? Max - 1 : (X >= Max ? 0 : X);
        }

        public static int[,] FullBlockIDs = new int[8, 2] {

            {  0,  1  }, //BOTTOM
            {  1,  0  }, //RIGHT
            {  0, -1  }, //TOP
            { -1,  0  }, //LEFT
            {  1,  1  }, //BOTTOM_RIGHT
            {  1, -1  }, //TOP_RIGHT
            { -1,  1  }, //BOTTOM_LEFT
            { -1, -1  }, //TOP_LEFT
        };

        public enum FullBlockIDsIDs : int {

            BOTTOM = 0,
            RIGHT = 1,
            TOP = 2,
            LEFT = 3,
            BOTTOM_RIGTH = 4,
            TOP_RIGHT = 5,
            BOTTOM_LEFT = 6,
            TOP_LEFT = 7
        };

        public static int[,] DeltaPlane = new int[6, 3]{

            {  0,  1,  0 },
            { -1,  0,  0 },
            {  0,  0, -1 },
            {  1,  0,  0 },
            {  0,  0,  1 },
            {  0, -1,  0 }
        };

        public enum Planes : int {

            TOP = 0,
            LEFT = 1,
            FRONT = 2,
            RIGHT = 3,
            BACK = 4,
            BOTTOM = 5
        }

        public static int[,] ChunkIDs = new int[4, 2] {

            {  0, -1 },
            {  0,  1 },
            { -1,  0 },
            {  1,  0 }
        };

        public enum Chunks : int {

            UP = 0,
            DOWN = 1,
            LEFT = 2,
            RIGHT = 3
        }

        public enum MODEL_SIDE : int {

            TOP = 0,
            BOT = 1,
            SIDE = 2
        }

        public static int[,] CornerIDs = new int[4, 4] {

            { 3, 2, 1, 0 },
            { 0, 3, 2, 1 },
            { 2, 1, 0, 3 },
            { 1, 0, 3, 2 }
        };

        public static int[,] CornerDS = new int[4, 2] {

            {  1,  0  },
            {  0, -1  },
            {  0,  1  },
            { -1,  0  }
        };

        public static System.Drawing.Color UiAimColor = System.Drawing.Color.FromArgb(0, 0, 0);

        public static float DefaultPointSize = 1;
        public static float UiPointSize = 5;

        public static int UiAimDistanceDevider = 10;

        public const double MinCameraAngle = -1.55f;
        public const double MaxCameraAngle = 1.55;

        public const double DefaultSpeed = 0.02;
        public const double SprintMultiplier = 5;

        public const double FAULT = 0.01f;
    }
}
