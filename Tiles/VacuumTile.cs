using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace MoreMechanisms.Tiles {

    public class TEVacuum : ModTileEntity {
        
        public bool On = false;
        
        internal bool changed = false;

        public int maxItems = 5;
        public List<Item> items = new List<Item>();

        public int grabRange = (int)(16 * 0.75);
        public int range = (int)(16 * 4);
        internal Texture2D circle;

        public override void PreGlobalUpdate() {
            base.PreGlobalUpdate();
        }
        
        public override void PostGlobalUpdate() {
            base.PostGlobalUpdate();
        }

        public override void Update() {

            items.RemoveAll((Item i) => i.IsAir);

            Vector2 pos = new Vector2(Position.X * 16, Position.Y * 16);
            foreach (Item i in Main.item) {
                if (i.active) {
                    if (i.DistanceSQ(pos) < (grabRange * grabRange)) {
                        bool merged = false;

                        foreach(Item it in items) {
                            if (it.IsTheSameAs(i)) {
                                if(it.stack + i.stack <= it.maxStack) {
                                    it.stack += i.stack;
                                    i.active = false;
                                    i.TurnToAir();
                                    merged = true;
                                    for(int pi = 0; pi < 10; pi++) {
                                        int ppi = Dust.NewDust(pos - new Vector2(i.width / 2, i.height / 2), i.width, i.height, DustID.Smoke);
                                        Main.dust[ppi].velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
                                    }
                                }
                            }
                        }

                        if(!merged && items.Count < maxItems) {
                            items.Add(i.Clone());
                            i.active = false;
                            i.TurnToAir();
                            for (int pi = 0; pi < 10; pi++) {
                                int ppi = Dust.NewDust(pos, i.width, i.height, DustID.Smoke);
                                Main.dust[ppi].velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
                            }
                        }
                    }else if(i.DistanceSQ(pos) < (range * range)) {
                        Vector2 dir = (pos - i.position);
                        dir.Normalize();
                        i.velocity += dir / 2f;
                        if(i.velocity.LengthSquared() > (4 * 4)) {
                            i.velocity.Normalize();
                            i.velocity *= 4;
                        }
                    }
                }
            }

            if (changed) {
                // Sending 86 aka, TileEntitySharing, triggers NetSend. Think of it like manually calling sync.
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
                changed = false;
            }
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
            return tile.active() && tile.type == mod.TileType("VacuumTile") && tile.frameX == 0 && tile.frameY == 0;
        }

        public override int Hook_AfterPlacement(int x, int y, int type, int style, int direction) {
            this.On = true;

            if (Main.netMode == 1) {
                NetMessage.SendTileSquare(Main.myPlayer, x, y, 1, TileChangeType.None);
                NetMessage.SendData(87, -1, -1, null, x, (float)y, 2f, 0f, 0, 0, 0);
                return -1;
            }

            int num = Place(x, y);
            return num;
        }

        public override void OnKill() {
            base.OnKill();
            foreach (Item i in items) {
                Item.NewItem(Position.X * 16, Position.Y * 16, 1, 1, i.type, i.stack, false, i.prefix, false, false);
            }
        }

    }

    public class VacuumTile : ModTile {
        public override void SetDefaults() {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileMergeDirt[Type] = false;
            Main.tileBlockLight[Type] = false;
            Main.tileLighted[Type] = true;
            Main.tileNoAttach[Type] = true;

            dustType = mod.DustType("Sparkle");
            
            drop = mod.ItemType("VacuumItem");
            
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.None, 0, 0);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(GetInstance<TEVacuum>().Hook_AfterPlacement, -1, 0, true);
            TileObjectData.addTile(Type);
            
            //disableSmartCursor = true;
            //disableSmartInteract = true;

            AddMapEntry(new Color(200, 200, 200));
            // Set other values here

            //dustType = DustType<Sparkle>();
            //drop = ItemType<Items.Placeable.ExamplePlatform>();
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
            base.KillTile(i, j, ref fail, ref effectOnly, ref noItem);
            GetInstance<TEVacuum>().Kill(i, j);
        }
        
    }
}