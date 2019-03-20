using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minecraft.Items;
using Minecraft.Rendering;

namespace Minecraft {

    public static class ItemsSet {

        public static Dictionary<UInt64, Item> ITEMS = new Dictionary<UInt64, Item>();
        public static List<Texture> TEXTURES = new List<Texture>();
        public static List<int> T_BUFFER = new List<int>();

        public static int Add(Item I) {

            ITEMS.Add(I.ID, I);
            return ITEMS.Count - 1;
        }

        public static int Add(Texture T) {

            TEXTURES.Add(T);
            return TEXTURES.Count - 1;
        }
    }
}
