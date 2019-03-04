using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;

namespace Minecraft {

    public class BlockInstance : Block {

        public UInt32 DurabilityValue { get; private set; }
        public float TemperatureValue { get; private set; }
        public float PressureValue { get; private set; }

        public UInt16 X { get; private set; }
        public UInt16 Y { get; private set; }
        public UInt16 Z { get; private set; }

        public BlockInstance(UInt64 BID, UInt16 CX, UInt16 CY, UInt16 CZ) : base(ItemsSet.ITEMS[BID] as Block)  {

            this.DurabilityValue = Durability;
            this.TemperatureValue = Temperature;
            this.PressureValue = Pressure;

            this.X = CX;
            this.Y = CY;
            this.Z = CZ;
        }
    }
}
