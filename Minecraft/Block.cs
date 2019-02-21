using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Tao;
using Tao.OpenGl;
using System.Drawing;

namespace Minecraft {

    public class Block : Item {

        public bool Establishable { get; private set; }
        public UInt32 Durability { get; private set; }
        public UInt32 Density { get; private set; }
        public float Temperature { get; private set; }
        public float Pressure { get; private set; }

        public bool Physical { get; private set; }
        public bool UsesGravity { get; private set; }

        private float fluidity = 0;
        public float Fluidity { get { return fluidity; } 
                                private set { fluidity = value < 0 ? 0 : (value > 1 ? 1 : value); } }

        private float light = 0;
        public float Light { get { return light; } 
                             private set { light = value < 0 ? 0 : (value > 1 ? 1 : value); } }

        public Dictionary<UInt64, Dictionary<UInt64, UInt32>> LootByTool { get; private set; }
        public Vector3D Size { get; private set; }

        public string State { get; private set; }

        public UInt64 UpperStateID { get; private set; }
        private List<Constants.StateBounds<Block>> UpperConditions = new List<Constants.StateBounds<Block>>();

        public UInt64 LowerStateID { get; private set; }
        private List<Constants.StateBounds<Block>> LowerConditions = new List<Constants.StateBounds<Block>>();

        public Color DefaultColor { get; private set; }
        public Color ShapeColor { get; private set; }

        public List<Plane> Planes = new List<Plane>();

        public List<Vector3D> ModelPoints = new List<Vector3D>();
        public List<Vector2D> TexturePoints = new List<Vector2D>();

        public Block(Block B) {

            this.ID = B.ID;
            this.Name = B.Name;
            this.Description = B.Description;
            this.Stack = B.Stack;
            this.Establishable = B.Establishable;
            this.Durability = B.Durability;
            this.Density = B.Density;
            this.Physical = B.Physical;
            this.UsesGravity = B.UsesGravity;
            this.Fluidity = B.Fluidity;
            this.Light = B.Light;
            this.Temperature = B.Temperature;
            this.Pressure = B.Pressure;
            this.Size = B.Size;
            this.LootByTool = B.LootByTool;
            this.UpperStateID = B.UpperStateID;
            this.UpperConditions = B.UpperConditions;
            this.LowerStateID = B.LowerStateID;
            this.LowerConditions = B.LowerConditions;
            this.DefaultColor = B.DefaultColor;
            this.ShapeColor = B.ShapeColor;
            this.ModelPoints = B.ModelPoints;
            this.TexturePoints = B.TexturePoints;
            this.Planes = B.Planes;
        }

        public Block(string filename) {

            Stream BlockInfo = new StreamReader(filename).BaseStream;

            this.ID = ByteParser.ConvertBytes<UInt64>(ByteParser.GetBytes(BlockInfo, 8));

            UInt16 NameLen = ByteParser.ConvertBytes<UInt16>(ByteParser.GetBytes(BlockInfo, 2));
            this.Name = ByteParser.ConvertBytes<string>(ByteParser.GetBytes(BlockInfo, NameLen));

            UInt16 DescriptionLen = ByteParser.ConvertBytes<UInt16>(ByteParser.GetBytes(BlockInfo, 2));
            this.Description = ByteParser.ConvertBytes<string>(ByteParser.GetBytes(BlockInfo, DescriptionLen));

            this.Stack = ByteParser.ConvertBytes<UInt32>(ByteParser.GetBytes(BlockInfo, 4));
            this.Establishable = ByteParser.ConvertBytes<bool>(ByteParser.GetBytes(BlockInfo, 1));

            this.Durability = ByteParser.ConvertBytes<UInt32>(ByteParser.GetBytes(BlockInfo, 4));
            this.Density = ByteParser.ConvertBytes<UInt32>(ByteParser.GetBytes(BlockInfo, 4));

            this.Physical = ByteParser.ConvertBytes<bool>(ByteParser.GetBytes(BlockInfo, 1));
            this.UsesGravity = ByteParser.ConvertBytes<bool>(ByteParser.GetBytes(BlockInfo, 1));

            this.Fluidity = ByteParser.ConvertBytes<float>(ByteParser.GetBytes(BlockInfo, 4));
            this.Light = ByteParser.ConvertBytes<float>(ByteParser.GetBytes(BlockInfo, 4));
            this.Temperature = ByteParser.ConvertBytes<float>(ByteParser.GetBytes(BlockInfo, 4));
            this.Pressure = ByteParser.ConvertBytes<float>(ByteParser.GetBytes(BlockInfo, 4));

            float XS = ByteParser.ConvertBytes<float>(ByteParser.GetBytes(BlockInfo, 4));
            float YS = ByteParser.ConvertBytes<float>(ByteParser.GetBytes(BlockInfo, 4));
            float ZS = ByteParser.ConvertBytes<float>(ByteParser.GetBytes(BlockInfo, 4));

            this.Size = new Vector3D(XS, YS, ZS);

            this.LootByTool = new Dictionary<UInt64, Dictionary<UInt64, UInt32>>();
            UInt16 ToolCount = ByteParser.ConvertBytes<UInt16>(ByteParser.GetBytes(BlockInfo, 2));

            for (int i = 0; i < ToolCount; i++) {

                UInt64 ToolID = ByteParser.ConvertBytes<UInt64>(ByteParser.GetBytes(BlockInfo, 8));
                UInt16 LootTypesCount = ByteParser.ConvertBytes<UInt16>(ByteParser.GetBytes(BlockInfo, 2));

                Dictionary<UInt64, UInt32> Loot = new Dictionary<UInt64, UInt32>();

                for (int j = 0; j < LootTypesCount; j++)
                    Loot.Add(ByteParser.ConvertBytes<UInt64>(ByteParser.GetBytes(BlockInfo, 8)),
                             ByteParser.ConvertBytes<UInt32>(ByteParser.GetBytes(BlockInfo, 4)));

                LootByTool.Add(ToolID, Loot);
            }

            this.UpperStateID = ByteParser.ConvertBytes<UInt64>(ByteParser.GetBytes(BlockInfo, 8));

            if (UpperStateID != 0)
                this.UpperConditions = ParseConditions(BlockInfo);

            this.LowerStateID = ByteParser.ConvertBytes<UInt64>(ByteParser.GetBytes(BlockInfo, 8));

            if (LowerStateID != 0)
                this.UpperConditions = ParseConditions(BlockInfo);

            this.DefaultColor = Color.FromArgb(ByteParser.ConvertBytes<byte>(ByteParser.GetBytes(BlockInfo, 1)),
                                               ByteParser.ConvertBytes<byte>(ByteParser.GetBytes(BlockInfo, 1)),
                                               ByteParser.ConvertBytes<byte>(ByteParser.GetBytes(BlockInfo, 1)),
                                               ByteParser.ConvertBytes<byte>(ByteParser.GetBytes(BlockInfo, 1)));

            this.ShapeColor = Color.FromArgb(ByteParser.ConvertBytes<byte>(ByteParser.GetBytes(BlockInfo, 1)),
                                             ByteParser.ConvertBytes<byte>(ByteParser.GetBytes(BlockInfo, 1)),
                                             ByteParser.ConvertBytes<byte>(ByteParser.GetBytes(BlockInfo, 1)),
                                             ByteParser.ConvertBytes<byte>(ByteParser.GetBytes(BlockInfo, 1)));

            UInt16 ModelPointsCount = ByteParser.ConvertBytes<UInt16>(ByteParser.GetBytes(BlockInfo, 2));

            for (int i = 0; i < ModelPointsCount; i++)
                ModelPoints.Add(new Vector3D(ByteParser.ConvertBytes<float>(ByteParser.GetBytes(BlockInfo, 4)),
                                             ByteParser.ConvertBytes<float>(ByteParser.GetBytes(BlockInfo, 4)),
                                             ByteParser.ConvertBytes<float>(ByteParser.GetBytes(BlockInfo, 4))));

            UInt16 TexturePointsCount = ByteParser.ConvertBytes<UInt16>(ByteParser.GetBytes(BlockInfo, 2));

            for (int i = 0; i < TexturePointsCount; i++)
                TexturePoints.Add(new Vector2D(ByteParser.ConvertBytes<float>(ByteParser.GetBytes(BlockInfo, 4)),
                                               ByteParser.ConvertBytes<float>(ByteParser.GetBytes(BlockInfo, 4))));

            UInt16 PlanesCount = ByteParser.ConvertBytes<UInt16>(ByteParser.GetBytes(BlockInfo, 2));

            for (int i = 0; i < PlanesCount; i++)
                this.Planes.Add(new Plane(BlockInfo));
        }

        private List<Constants.StateBounds<Block>> ParseConditions(Stream S) {

            UInt16 ConditionsCount = ByteParser.ConvertBytes<UInt16>(ByteParser.GetBytes(S, 2));
            List<Constants.StateBounds<Block>> Conditions = new List<Constants.StateBounds<Block>>();

            for (int i = 0; i < ConditionsCount; i++) {

                char Variable = ByteParser.ConvertBytes<char>(ByteParser.GetBytes(S, 1));
                bool Comparison = ByteParser.ConvertBytes<bool>(ByteParser.GetBytes(S, 1));
                float Value = ByteParser.ConvertBytes<float>(ByteParser.GetBytes(S, 4));

                switch (Variable) {

                    case 'T': Conditions.Add(B => (B.Temperature < Value) ^ Comparison); break;
                    case 'P': Conditions.Add(B => (B.Temperature < Value) ^ Comparison); break;
                }
            }

            return Conditions;
        }
    }
}
