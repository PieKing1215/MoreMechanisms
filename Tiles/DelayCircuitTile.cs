using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace MoreMechanisms.Tiles {

    public class TEDelayCircuit : ModTileEntity {

        public int delay = 60;
        public bool justHit;
        public bool[] buffer = new bool[0];

        internal bool changed = false;
        
        public void ClientSendServer() {
            ModPacket myPacket = MoreMechanisms.instance.GetPacket();
            myPacket.Write((byte)1); // id
            myPacket.Write((short)Position.X);
            myPacket.Write((short)Position.Y);
            NetSend(myPacket, false);
            myPacket.Send();
        }

        public override void Update() {

            if (buffer.Length == 0) {
                buffer = new bool[delay];
            }

            if (buffer[0]) {
                Wiring.TripWire(base.Position.X + 2, base.Position.Y, 1, 1);
            }

            for (int i = 1; i < buffer.Length; i++) {
                buffer[i - 1] = buffer[i];
            }
            buffer[delay - 1] = justHit;
            justHit = false;


            //string str = "";
            //for(int i = 0; i < buffer.Length; i++) {
            //    str += (buffer[i] ? "1" : "0") + " ";
            //}
            //Main.NewText(str);

            if (changed) {
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
                changed = false;
            }
        }

        public override void NetReceive(BinaryReader reader, bool lightReceive) {
        }

        public override void NetSend(BinaryWriter writer, bool lightSend) {
        }

        public override TagCompound Save() {
            return new TagCompound
            {
            };
        }

        public override void Load(TagCompound tag) {
        }

        public override bool ValidTile(int i, int j) {
            Tile tile = Main.tile[i, j];
            return tile.active() && tile.type == mod.TileType("DelayCircuitTile") && tile.frameX == 0 && tile.frameY == 0;
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction) {
            //Main.NewText("i " + i + " j " + j + " t " + type + " s " + style + " d " + direction);
            if (Main.netMode == 1) {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, 3);
                NetMessage.SendData(87, -1, -1, null, i, j, Type, 0f, 0, 0, 0);
                return -1;
            }
            return Place(i, j);
        }
        
    }

    public class DelayCircuitTile : ModTile {
        public override void SetDefaults() {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileMergeDirt[Type] = false;
            Main.tileBlockLight[Type] = false;
            Main.tileLighted[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;

            dustType = mod.DustType("Sparkle");
            //drop = mod.ItemType("DelayCircuitItem");

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.AnchorBottom = new AnchorData(TileObjectData.newTile.AnchorBottom.type, TileObjectData.newTile.Width, 0);
            
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(GetInstance<TEDelayCircuit>().Hook_AfterPlacement, -1, 0, true);
            TileObjectData.addTile(Type);

            //disableSmartCursor = true;
            //disableSmartInteract = true;

            AddMapEntry(new Color(200, 200, 200));
            // Set other values here
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) {
            Item.NewItem(i * 16, j * 16, 32, 32, mod.ItemType("DelayCircuitItem"));
            GetInstance<TEDelayCircuit>().Kill(i, j);
        }

        public override bool HasSmartInteract() {
            return true;
        }

        public override void HitWire(int i, int j) {
            Tile tile = Main.tile[i, j];
            if (tile.frameX != 0) return;
            int left = i - tile.frameX;
            int top = j - tile.frameY / 18;

            int index = GetInstance<TEDelayCircuit>().Find(left, top);
            if (index != -1) {
                TEDelayCircuit DelayCircuitEnt = (TEDelayCircuit)TileEntity.ByID[index];
                
                DelayCircuitEnt.justHit = true;
            }
        }
        
        public override void MouseOver(int i, int j) {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.showItemIcon = true;
            player.showItemIcon2 = mod.ItemType("DelayCircuitItem");
        }

        public override void PostDraw(int i, int k, SpriteBatch spriteBatch) {
            base.PostDraw(i, k, spriteBatch);

            Tile tile = Main.tile[i, k];
            if (tile.active() && tile.type == mod.TileType("DelayCircuitTile") && tile.frameX == 18*2) {
                TileEntity tileEntity = default(TileEntity);

                if (TileEntity.ByPosition.TryGetValue(new Point16(i-2, k), out tileEntity)) {
                    TEDelayCircuit es = tileEntity as TEDelayCircuit;
                    Color lightCol = Lighting.GetColor(i, k);

                    //TODO: why does the spritebatch seem to draw offset 12 tiles to the -x and -y?
                    i += 10;
                    k += 12;

                    Color drawCol = Microsoft.Xna.Framework.Color.DarkRed;
                    Color mergeColor = lightCol.MultiplyRGBA(drawCol);
                    for (int d = 0; d < es.delay; d++) {
                        if (es.buffer[d]) {
                            float thru = 1f - (float)d / es.delay;
                            Vector2 start = new Vector2((float)(i * 16 + 16 + (int)(thru * 19)), (float)(k * 16 + 4));
                            Vector2 end = new Vector2((float)(i * 16 + 16 + (int)(thru * 19)), (float)(k * 16 + 12));
                            Utils.DrawLine(spriteBatch, start, end, mergeColor, mergeColor, 2f);
                        }
                    }
                }
            }
        }
    }
}