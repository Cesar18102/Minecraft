using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft {

    public class World {

        public List<Chunk> Chunk_Buffer = new List<Chunk>();//do not save
        private bool Saving = false;

        public string Name { get; private set; }

        private UInt64 LastChunkId = 1;

        public void ReserveNewChunck(ref UInt64 CID) {

            CID = LastChunkId++;
        }

        /*public Chunk this[UInt64 ID]
        {
            get { return Chunk_Buffer[ID]; }
            //read from file
        }*/
        
        public World(string name) {

            this.Name = name;
        }

        public void AddChunk(Chunk C) {

            Chunk_Buffer.Add(C);

            if (!Saving) {

                Saving = true;
                Task.Factory.StartNew(SaveBuffer);
            }
        }

        public void Draw() {

            for (int i = 0; i < Chunk_Buffer.Count; i++)
                Chunk_Buffer[i].Draw();
        }

        public void SaveBuffer() {

            //saving
            Saving = false;
        }
    }
}
