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

        public delegate bool StateBounds<T>(T val);

        public static UInt16 CHUNK_X = 16;
        public static UInt16 CHUNK_Y = 64;
        public static UInt16 CHUNK_Z = 16;
    }
}
