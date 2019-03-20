using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Minecraft.Items {

    public abstract class Item {

        public UInt64 ID { get; protected set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }

        public UInt32 Stack { get; protected set; }
    }
}
