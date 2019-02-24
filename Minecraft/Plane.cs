using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Tao.FreeGlut;
using Tao.OpenGl;
using Tao.Platform;

namespace Minecraft {

    public class Plane {

        public UInt32 GlMode { get; private set; }

        public string Texture_src { get; private set; }
        public int TEX_ID { get; private set; }

        public List<UInt16> PlanePointSquence = new List<UInt16>();
        public List<UInt16> TexturePointsSequense = new List<UInt16>();

        public Plane(Stream S) {

            this.GlMode = ByteParser.ConvertBytes<UInt32>(ByteParser.GetBytes(S, 4));

            UInt16 TextureSrcLen = ByteParser.ConvertBytes<UInt16>(ByteParser.GetBytes(S, 2));
            string TextureSrc = ByteParser.ConvertBytes<string>(ByteParser.GetBytes(S, TextureSrcLen));

            this.TEX_ID = ItemsSet.Add(new Texture(Constants.DataDir + TextureSrc, false, true));

            UInt16 PlanePointsSequnceCount = ByteParser.ConvertBytes<UInt16>(ByteParser.GetBytes(S, 2));

            for (int i = 0; i < PlanePointsSequnceCount; i++)
                this.PlanePointSquence.Add(ByteParser.ConvertBytes<UInt16>(ByteParser.GetBytes(S, 2)));

            for (int i = 0; i < PlanePointsSequnceCount; i++)
                this.TexturePointsSequense.Add(ByteParser.ConvertBytes<UInt16>(ByteParser.GetBytes(S, 2)));
        }
    }
}
