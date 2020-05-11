using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace MoreMechanisms {
    /// <summary>
    /// Base class for tile entities that act as a "sensor" of some sort.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TESensorBase<T> : ModTileEntity where T : ModTileEntity {

        private static List<int> markedIDsForRemoval = new List<int>();

        public bool On = false;

        public int CountedData;

        internal bool changed = false;

        public override void PreGlobalUpdate() {
            base.PreGlobalUpdate();
        }
        
        public override void PostGlobalUpdate() {
            base.PostGlobalUpdate();

            foreach (int item in markedIDsForRemoval) {
                TileEntity tileEntity = default(TileEntity);
                if (TileEntity.ByID.TryGetValue(item, out tileEntity)) {
                    GetInstance<T>().Kill(tileEntity.Position.X, tileEntity.Position.Y);
                }
            }
            markedIDsForRemoval.Clear();
        }

        public override void Update() {
            if (!SanityCheck()) return;

            bool state = GetState();
            
            if (this.On != state) {
                this.ChangeState(state, true);
            }

            if (changed) {
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
                changed = false;
            }
        }

        public void ChangeState(bool onState, bool TripWire) {
            if (onState != this.On && !SanityCheck()) {
                return;
            }

            Main.tile[base.Position.X, base.Position.Y].frameX = (short)(onState ? 18 : 0);
            this.On = onState;
            GetFrame();

            if (Main.netMode == 2) {
                NetMessage.SendTileSquare(-1, base.Position.X, base.Position.Y, 1, TileChangeType.None);
            }

            if (TripWire && Main.netMode != 1) {
                Wiring.TripWire(base.Position.X, base.Position.Y, 1, 1);
            }
        }

        public void FigureCheckType(out bool on) {
            int x = base.Position.X;
            int y = base.Position.Y;

            on = false;
            if (!WorldGen.InWorld(x, y, 0)) {
                return;
            }
            Tile tile = Main.tile[x, y];
            if (tile == null) {
                return;
            }
            TileEntity tileEntity = default(TileEntity);
            if (TileEntity.ByPosition.TryGetValue(new Point16(x, y), out tileEntity) && tileEntity.type == 2) {
                TESensorBase<T> es = tileEntity as TESensorBase<T>;
                on = es.GetState();
            }
        }

        public abstract bool GetState();
        public abstract int GetTileType();

        public virtual void FigureCheckState() {
            FigureCheckType(out this.On);
            GetFrame();
        }

        public void GetFrame() {
            int x = base.Position.X;
            int y = base.Position.Y;
            
            Main.tile[x, y].frameX = (short)(this.On ? 18 : 0);
            Main.tile[x, y].frameY = 0;
        }

        public bool SanityCheck() {

            int x = Position.X;
            int y = Position.Y;
            
            if (Main.tile[x, y].active()) {
                TileEntity tileEntity = default(TileEntity);
                if (TileEntity.ByPosition.TryGetValue(new Point16(x, y), out tileEntity)) {
                    return true;
                }
            }
            markedIDsForRemoval.Add(this.ID);
            return false;
        }

        public override void NetReceive(BinaryReader reader, bool lightReceive) {
            this.On = reader.ReadBoolean();
        }

        public override void NetSend(BinaryWriter writer, bool lightSend) {
            writer.Write(this.On);
        }

        public override TagCompound Save() {
            return new TagCompound
            {
                {"On", this.On}
            };
        }

        public override void Load(TagCompound tag) {
            On = tag.Get<bool>("On");
        }

        public override bool ValidTile(int i, int j) {
            Tile tile = Main.tile[i, j];

            if(tile.active() && (tile.frameX == 0 || tile.frameX == 18) && tile.frameY == 0) {
                return tile.type == GetTileType();
            }

            return false;
        }

        public override int Hook_AfterPlacement(int x, int y, int type, int style, int direction) {
            if (Main.netMode == 1) {
                NetMessage.SendTileSquare(Main.myPlayer, x, y, 1, TileChangeType.None);
                NetMessage.SendData(87, -1, -1, null, x, (float)y, 2f, 0f, 0, 0, 0);
                return -1;
            }
            int num = Place(x, y);
            ((TESensorBase<T>)TileEntity.ByID[num]).FigureCheckState();
            return num;
        }

    }
}