using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace Minecraft {

    public class ItemCategory {

        public string Name { get; private set; }
        public string SrcName { get; private set; }

        public string CategoryDir { get; private set; }
        public string ConfigFile { get; private set; }

        public bool ValidSrc { get; private set; }

        public Type T { get; private set; }
        public ConstructorInfo Constructor { get; private set; }

        public ItemCategory(string SrcName, string Name, 
                            Type T, ConstructorInfo Constructor) {

            this.SrcName = SrcName;
            this.Name = Name;
            this.T = T;
            this.Constructor = Constructor;
        }

        public void SetSrc(string CategoryDir, string ConfigFile) {

            ValidSrc = Directory.Exists(CategoryDir) && File.Exists(ConfigFile);

            this.CategoryDir = CategoryDir;
            this.ConfigFile = ConfigFile;
        }
    }
}
