﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Minecraft.Rendering;
using Minecraft.Data;
using Minecraft.Items;

namespace Minecraft.Structure {

    public class Chunk {

        private UInt64 Id;
        public UInt64 ID { get { return Id; } }

        public Int64 PivotX { get; private set; }
        public Int64 PivotZ { get; private set; }

        public Int64 RdX { get; private set; }
        public Int64 RdZ { get; private set; }

        private BlockInstance[, ,] Blocks = new BlockInstance[Constants.CHUNK_X, 
                                                              Constants.CHUNK_Y, 
                                                              Constants.CHUNK_Z];

        private RenderChunk[] Render = new RenderChunk[Constants.CHUNK_Y];

        public BlockInstance this[UInt16 x, UInt16 y, UInt16 z] {
 
            get { return Blocks[x, y, z]; }
            set { Blocks[x, y, z] = value; }
        }

        public Chunk(Int64 PivotX, Int64 PivotZ, bool Generate) {

            Game.W.ReserveNewChunck(ref Id);

            this.PivotX = PivotX;
            this.PivotZ = PivotZ;

            this.RdX = Math.Abs(PivotX / Constants.CHUNK_X);
            this.RdZ = Math.Abs(PivotZ / Constants.CHUNK_Z);

            if (Generate)
                GenerateChunk();
        }

        public void GenerateChunk() {

            Block B = ItemsSet.ITEMS[1] as Block;

            for (UInt16 k = 0; k < Constants.CHUNK_Y; k++) {
                for (UInt16 i = 0; i < Constants.CHUNK_X; i++)
                    for (UInt16 j = 0; j < Constants.CHUNK_Z; j++)
                        Blocks[i, k, j] = new BlockInstance(1, i, k, j);

                //Blocks[0, k, 0] = null;
                Render[k] = new RenderChunk(this, k, (RdX < Constants.ShortRenderDistance && 
                                                      RdZ < Constants.ShortRenderDistance) ? 
                                                        true : 
                                                        k >= Constants.DYRender * ((int)Math.Sqrt(RdX * RdX + RdZ + RdZ) - Constants.ShortRenderDistance));
            }

            /*for (Int16 i = (Int16)(Constants.CHUNK_Y - 1); i >= 0; i--) {
                for (int j = 0; j < 1024; j++) {

                    UInt16 X = (UInt16)Constants.R.Next(0, Constants.CHUNK_X);
                    UInt16 Z = (UInt16)Constants.R.Next(0, Constants.CHUNK_Z);

                    if (Blocks[X, i, Z] == null)
                        Blocks[X, i, Z] = new BlockInstance(1, X, (UInt16)i, Z);
                }

                Render[i] = new RenderChunk(this, (UInt16)i);
            }*/
        }

        public void LoadVisibility(Chunk CUp, Chunk CDown, Chunk CLeft, Chunk CRight) {

            for (int i = 0; i < Constants.CHUNK_Y; i++)
                this.Render[i].LoadVisibility(
                    new RenderChunk[]{

                        i == Constants.CHUNK_Y - 1 ? null : (this.Render[i + 1].Visible ? this.Render[i + 1] : null),
                        CLeft == null ? null : (CLeft.Render[i].Visible ? CLeft.Render[i] : null), 
                        CUp == null ? null : (CUp.Render[i].Visible ? CUp.Render[i] : null), 
                        CRight == null ? null : (CRight.Render[i].Visible ? CRight.Render[i] : null), 
                        CDown == null ? null : (CDown.Render[i].Visible ? CDown.Render[i] : null),
                        i == 0 ? null : (this.Render[i - 1].Visible ? this.Render[i - 1] : null)
                    }
                );
        }

        public void GenerateRenderPieces() {

            for (int i = 0; i < Constants.CHUNK_Y; i++)
                Render[i].GenerateRenderPieces();
        }

        public void CreateTextures() {

            foreach (RenderChunk RC in Render)
                RC.CreateTextures();
        }

        public void Draw() {

            foreach (RenderChunk R in Render)
                R.Draw();
        }

        public void WriteFile(Stream S) {

            //saving
        }
    }
}