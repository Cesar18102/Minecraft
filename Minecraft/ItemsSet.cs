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

        private static List<int> T_BUFFER = new List<int>();
        private static Stack<int> Free = new Stack<int>();

        public static int TexBufferCount { get { return T_BUFFER.Count; } }

        public static int Add(Item I) {

            ITEMS.Add(I.ID, I);
            return ITEMS.Count - 1;
        }

        public static int Add(Texture T) {

            int ID = Free.Count == 0 ? TEXTURES.Count : Free.Pop();

            if (Free.Count == 0) {

                TEXTURES.Add(T);

                if (!T.Uploaded && !T.Prime)
                    T_BUFFER.Add(ID);
            }
            else {

                TEXTURES[ID] = T;

                if (!T.Uploaded && !T.Prime)
                    T_BUFFER.Add(ID);
            }

            return ID;
        }

        public static void DestroyTexture(int ID) {

            TEXTURES[ID].Dispose();
            Free.Push(ID);
        }

        public static void UploadTextures() {

            lock (T_BUFFER) {

                int L = T_BUFFER.Count;

                for (int i = 0; i < L; i++) {

                    TEXTURES[T_BUFFER[0]].Upload();
                    T_BUFFER.RemoveAt(0);
                }
            }
        }
    }
}
