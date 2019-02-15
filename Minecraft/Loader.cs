using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace Minecraft {

    public class MissingDataException : Exception {

        public MissingDataException(string Message) : base(Message) { }
    }

    public static class Loader {

        public delegate void LoadingLog(string message, int layer);
        public static event LoadingLog LoadingLogging;

        private const string BlocksSrcName = "Blocks_models_src";
        private const string MaterialsSrcName = "Materials_models_src";
        private const string ToolsSrcName = "Tools_models_src";

        private const string BlockName = "Block";
        private const string MaterialName = "Material";
        private const string ToolName = "Tool";

        private static ConstructorInfo BlockConstructor = typeof(Block).GetConstructor(new Type[] { typeof(string) });
        private static ConstructorInfo MaterialConstructor = typeof(Material).GetConstructor(new Type[] { typeof(string) });
        private static ConstructorInfo ToolConstructor = typeof(Tool).GetConstructor(new Type[] { typeof(string) });

        private static int LoadLayer = 0;
        public static int Layer { get { return LoadLayer - 1; } }

        private static Dictionary<string, ItemCategory> ItemCategories = new Dictionary<string, ItemCategory>() {

            { BlocksSrcName,    new ItemCategory(BlocksSrcName,    BlockName,    typeof(Block), BlockConstructor) },
            { MaterialsSrcName, new ItemCategory(MaterialsSrcName, MaterialName, typeof(Material), MaterialConstructor) },
            { ToolsSrcName,     new ItemCategory(ToolsSrcName,     ToolName,     typeof(Tool), ToolConstructor) }
        };

        public static void LoadPathes() {

            LoadMessage("items categories file pathes", ref LoadLayer, true);

            if (!Directory.Exists(Constants.DataDir))
                throw new MissingDataException("Data directory is missing");

            if(!File.Exists(Constants.initFilePath))
                throw new MissingDataException("conf.ini is missing");

            Stream initDataStream = new StreamReader(Constants.initFilePath).BaseStream;

            while (initDataStream.Position != initDataStream.Length) {

                UInt16 SrcNameLength = ByteParser.ConvertBytes<UInt16>(ByteParser.GetBytes(initDataStream, 2));
                string SrcName = ByteParser.ConvertBytes<String>(ByteParser.GetBytes(initDataStream, SrcNameLength));

                UInt16 SrcDirLength = ByteParser.ConvertBytes<UInt16>(ByteParser.GetBytes(initDataStream, 2));
                string SrcDir = ByteParser.ConvertBytes<String>(ByteParser.GetBytes(initDataStream, SrcDirLength));

                UInt16 ConfNameLength = ByteParser.ConvertBytes<UInt16>(ByteParser.GetBytes(initDataStream, 2));
                string ConfName = ByteParser.ConvertBytes<String>(ByteParser.GetBytes(initDataStream, ConfNameLength));

                string FullDirSrc = Constants.RootDir + SrcDir;

                if (!Directory.Exists(FullDirSrc))
                    throw new MissingDataException(SrcDir + " is missing");

                string FullConfSrc = Constants.RootDir + SrcDir + ConfName;

                if (!File.Exists(FullConfSrc))
                    throw new MissingDataException(SrcDir + ConfName + " is missing");

                ItemCategories[SrcName].SetSrc(FullDirSrc, FullConfSrc);
            }

            LoadMessage("Items categories file pathes", ref LoadLayer, false);
            LoadMessage("items info", ref LoadLayer, true);

            foreach (ItemCategory IC in ItemCategories.Values) {

                LoadMessage(IC.Name + "s", ref LoadLayer, true);

                Stream ItemSrcInfo = new StreamReader(IC.ConfigFile).BaseStream;

                while (ItemSrcInfo.Position != ItemSrcInfo.Length) {

                    UInt64 ID = ByteParser.ConvertBytes<UInt64>(ByteParser.GetBytes(ItemSrcInfo, 8));
                    UInt16 ModelSrcLen = ByteParser.ConvertBytes<UInt16>(ByteParser.GetBytes(ItemSrcInfo, 2));
                    string ModelSrc = ByteParser.ConvertBytes<String>(ByteParser.GetBytes(ItemSrcInfo, ModelSrcLen));
                    string FullModelSrc = IC.CategoryDir + ModelSrc;

                    if (!File.Exists(FullModelSrc))
                        throw new MissingDataException(FullModelSrc + " is missing");

                    LoadMessage(IC.Name + " id " + ID.ToString() + " from " + ModelSrc, ref LoadLayer, true);

                    Item I = IC.Constructor.Invoke(new object[] { FullModelSrc }) as Item;

                    if (ID != I.ID)
                        throw new Exception("ID missmatch");

                    LoadMessage(IC.Name + " \"" + I.Name + "\" id " + I.ID.ToString() + " from " + ModelSrc, ref LoadLayer, false);

                    ItemsSet.ITEMS.Add(I.ID, I);
                }

                LoadMessage(IC.Name + "s", ref LoadLayer, false);
            }

            LoadMessage("Items info", ref LoadLayer, false);

        }

        private static void LoadMessage(string obj, ref int layer, bool IsStart) {

            if (IsStart) 
                LoadingLogging("Loading " + obj, layer++);
            else
                LoadingLogging(obj + " are loaded", --layer);
        }
    }
}
