using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft {

    public static class Constants {

        public static string RootDir = Environment.CurrentDirectory;
        public static string DataDir = RootDir + "/Data";
        public static string initFilePath = DataDir + "/conf.ini";

        public static Random R = new Random();

        public static bool GraphicsBusy = false;

        public delegate bool StateBounds<T>(T val);

        public static UInt16 CHUNK_X = 16;
        public static UInt16 CHUNK_Y = 1;
        public static UInt16 CHUNK_Z = 16;

        public static int[,] BlockIDs = new int[4, 2] {

            {  0,  1  }, //BOTTOM
            {  1,  0  }, //RIGHT
            {  0, -1  }, //TOP
            { -1,  0  }  //LEFT
        };

        public enum BlockIDsIDs : int {

            Bottom = 0,
            Right = 1,
            Top = 2,
            Left = 3
        };

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

            Bottom = 0,
            Right = 1,
            Top = 2,
            Left = 3,
            Bottom_Right = 4,
            Top_Right = 5,
            Bottom_Left = 6,
            Top_Left = 7
        };

        public enum Planes {

            TOP = 0,
            LEFT = 1,
            FRONT = 2,
            RIGHT = 3,
            BACK = 4,
            BOTTOM = 5
        }
    }
}
