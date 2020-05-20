using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace MoreMechanisms.Tiles {

    public class TEItemDuct : ModTileEntity {

        public static int TICK_DELAY = 6;

        public enum DuctType {
            None,
            In,
            Out
        }

        public List<Tuple<Item, Direction>> flowItems = new List<Tuple<Item, Direction>>();
        public List<Tuple<Item, Direction>> removeItems = new List<Tuple<Item, Direction>>();
        public List<Tuple<Item, Direction>> addItems = new List<Tuple<Item, Direction>>();
        public static List<TEItemDuct> needUpdate = new List<TEItemDuct>();

        internal ItemFilter filter = new ItemFilter(5, false);

        internal DuctType ductType = DuctType.None;

        internal bool changed = false;
        
        public override void PreGlobalUpdate() {
            if (needUpdate == null) needUpdate = new List<TEItemDuct>();

            base.PreGlobalUpdate();
        }
        
        public override void PostGlobalUpdate() {
            base.PostGlobalUpdate();

            if (Main.GameUpdateCount % TICK_DELAY != 0) return;

            //Main.NewText("#needUpdate = " + needUpdate.Count);
            foreach (TEItemDuct id in needUpdate) {
                //Main.NewText("PostGlobalUpdate " + id.Position.X + " " + id.Position.Y + " " + id.removeItems.Count + " " + id.addItems.Count);
                foreach (Tuple<Item, Direction> r in id.removeItems) {
                    id.flowItems.Remove(r);
                }
                id.removeItems.Clear();

                foreach (Tuple<Item, Direction> r in id.addItems) {
                    id.flowItems.Add(r);
                }
                id.addItems.Clear();
                
                id.UpdateFrame(id.Position.X, id.Position.Y);
            }
            needUpdate.Clear();
        }

        public override void Update() {

            if (Main.GameUpdateCount % TICK_DELAY != 0) return;

            switch (this.ductType) {
                case TEItemDuct.DuctType.None:
                    break;
                case TEItemDuct.DuctType.In:
                    foreach(Direction d in Direction.DIRECTIONS) {

                        int left = Position.X + d.dx;
                        int top = Position.Y + d.dy;

                        Tile tile = Framing.GetTileSafely(left, top);

                        if (SpecialConnects(tile, left, top)) {
                            if (tile.frameX % 36 != 0) {
                                left--;
                            }
                            if (tile.frameY != 0) {
                                top--;
                            }

                            IConnectable conn = Connectable.Find(left, top);
                            if (conn != null) {

                                Item[] items = conn.GetItems(ConnectableType.Input);
                                Func<Item, bool> validItemFunc = (Item it) => {
                                    return conn.Accepts(it, ConnectableType.Input);
                                };

                                for (int ind = 0; ind < items.Length; ind++) {
                                    Item i = items[ind];
                                    if (i.active && i.type != ItemID.None && filter.FilterAccepts(i)) {
                                        if ((flowItems.Count + addItems.Count) < 4) {
                                            Item it = i.Clone();
                                            it.stack = 1;
                                            addItems.Add(Tuple.Create(it, Direction.NONE));

                                            if (i.stack > 1) {
                                                i.stack--;
                                            } else {
                                                i.TurnToAir();
                                            }
                                            
                                            conn.TransferredItem(i, ind, ConnectableType.Input);

                                            break;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    }
                    break;
                case TEItemDuct.DuctType.Out:
                    if (flowItems.Count == 0) break;
                    foreach (Direction d in Direction.DIRECTIONS) {

                        int left = Position.X + d.dx;
                        int top = Position.Y + d.dy;

                        Tile tile = Framing.GetTileSafely(left, top);

                        if (SpecialConnects(tile, left, top)) {
                            if (tile.frameX % 36 != 0) {
                                left--;
                            }
                            if (tile.frameY != 0) {
                                top--;
                            }
                            
                            IConnectable conn = Connectable.Find(left, top);
                            if (conn != null) {

                                Item[] items = conn.GetItems(ConnectableType.Output);
                                Func<Item, bool> validItemFunc = (Item it) => {
                                    return conn.Accepts(it, ConnectableType.Output);
                                };

                                //Main.NewText("out searching chest");
                                foreach (Tuple<Item, Direction> it in flowItems) {
                                    if (!validItemFunc(it.Item1)) continue;
                                    int putItem = -1;
                                    Item newStack = null;
                                    for (int ind = 0; ind < items.Length; ind++) {
                                        Item i = items[ind];
                                        if (i.active && i.type != 0) {
                                            if(i.stack < i.maxStack) {
                                                if(it.Item1.type == i.type) {
                                                    //Main.NewText("out found stack");
                                                    i.stack++;
                                                    removeItems.Add(it);
                                                    putItem = ind;
                                                    newStack = i;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    //Main.NewText("out no stack");
                                    if (putItem == -1) {
                                        for (int ind = 0; ind < items.Length; ind++) {
                                            Item i = items[ind];
                                            if (i.IsAir) {
                                                //Main.NewText("out put");
                                                items[ind] = it.Item1;
                                                removeItems.Add(it);
                                                putItem = ind;
                                                newStack = it.Item1;
                                                break;
                                            }
                                        }
                                    }

                                    if (putItem != -1 && newStack != null) {
                                        conn.TransferredItem(newStack, putItem, ConnectableType.Output);
                                    }
                                }
                            }
                            break;
                        }
                    }
                    break;
            }

            //if(flowItems.Count > 0) Main.NewText("#" + flowItems.Count);
            if (ductType == DuctType.None || ductType == DuctType.In) {
                removeItems.AddRange(flowItems.FindAll(((Tuple<Item, Direction> ti) => {

                    Item i = ti.Item1;
                    //Main.NewText(i.HoverName);

                    List<Tuple<TEItemDuct, Direction>> possible = new List<Tuple<TEItemDuct, Direction>>();
                    Tuple<TEItemDuct, Direction> reverse = null;
                    foreach (Direction dir in Direction.DIRECTIONS) {
                        Tile tile = Framing.GetTileSafely(Position.X + dir.dx, Position.Y + dir.dy);

                        int left = Position.X + dir.dx;
                        int top = Position.Y + dir.dy;

                        if (tile.active() && tile.type == MoreMechanisms.instance.TileType("ItemDuctTile")) {

                            int index = GetInstance<TEItemDuct>().Find(left, top);
                            if (index != -1) {
                                TEItemDuct ent = (TEItemDuct)TileEntity.ByID[index];

                                //Main.NewText("adjacent duct " + ((Direction)d) + " " + ent.flowItems.Count + " " + ent.addItems.Count);
                                if ((ent.flowItems.Count + ent.addItems.Count) < 4 && ent.filter.FilterAccepts(i)) {
                                    //Main.NewText("add item");
                                    if(ti.Item2 == dir) {
                                        reverse = Tuple.Create(ent, dir.Opposite);
                                    } else {
                                        possible.Add(Tuple.Create(ent, dir.Opposite));
                                    }
                                }
                            }
                        }
                    }

                    if(possible.Count > 0) {
                        Tuple<TEItemDuct, Direction> sel = possible[Main.rand.Next(possible.Count)];
                        sel.Item1.addItems.Add(Tuple.Create(i, sel.Item2));
                        return true;
                    } else if(reverse != null){
                        reverse.Item1.addItems.Add(Tuple.Create(i, reverse.Item2));
                        return true;
                    }

                    return false;
                })));
            }

            needUpdate.Add(this);
            
            if (changed) {
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
                changed = false;
            }
        }
        
        public bool SpecialConnects(Tile t, int x, int y) {

            if (!t.active()) return false;

            if (t.frameX % 36 != 0) {
                x--;
            }
            if (t.frameY != 0) {
                y--;
            }
            
            if(this.ductType == DuctType.None) {
                return false;
            } else {
                IConnectable conn = Connectable.Find(x, y);
                if (conn != null) {
                    return conn.GetItems(this.ductType == DuctType.In ? ConnectableType.Input : ConnectableType.Output) != null;
                }
            }

            return false;
        }

        public override void NetReceive(BinaryReader reader, bool lightReceive) {
            this.ductType = (DuctType)reader.ReadByte();
            this.filter = ItemFilter.Read(reader, lightReceive);

            flowItems = new List<Tuple<Item, Direction>>();
            int nFlow = reader.ReadInt32();
            for(int i = 0; i < nFlow; i++) {
                flowItems.Add(Tuple.Create<Item, Direction>(reader.ReadItem(), Direction.DIRECTIONS[reader.ReadInt32()]));
            }

            addItems = new List<Tuple<Item, Direction>>();
            int nAdd = reader.ReadInt32();
            for (int i = 0; i < nAdd; i++) {
                addItems.Add(Tuple.Create<Item, Direction>(reader.ReadItem(), Direction.DIRECTIONS[reader.ReadInt32()]));
            }

            removeItems = new List<Tuple<Item, Direction>>();
            int nRemove = reader.ReadInt32();
            for (int i = 0; i < nRemove; i++) {
                removeItems.Add(Tuple.Create<Item, Direction>(reader.ReadItem(), Direction.DIRECTIONS[reader.ReadInt32()]));
            }

        }

        public override void NetSend(BinaryWriter writer, bool lightSend) {
            writer.Write((byte)this.ductType);
            this.filter.Write(writer, lightSend);

            writer.Write(flowItems.Count);
            for(int i = 0; i < flowItems.Count; i++) {
                writer.WriteItem(flowItems[i].Item1);
                writer.Write(Direction.DIRECTIONS.IndexOf(flowItems[i].Item2));
            }

            writer.Write(addItems.Count);
            for (int i = 0; i < addItems.Count; i++) {
                writer.WriteItem(addItems[i].Item1);
                writer.Write(Direction.DIRECTIONS.IndexOf(addItems[i].Item2));
            }

            writer.Write(removeItems.Count);
            for (int i = 0; i < removeItems.Count; i++) {
                writer.WriteItem(removeItems[i].Item1);
                writer.Write(Direction.DIRECTIONS.IndexOf(removeItems[i].Item2));
            }

        }

        public override TagCompound Save() {
            TagCompound tag = new TagCompound
            {
                {"ductType", (byte)this.ductType}
            };

            tag.Add("filter", this.filter);

            List<TagCompound> flow = new List<TagCompound>();
            for(int i = 0; i < flowItems.Count; i++) {
                flow.Add(new TagCompound { { "item", flowItems[i].Item1 }, { "direction", Direction.DIRECTIONS.IndexOf(flowItems[i].Item2) } });
            }
            tag.Add("flowItems", flow);

            List<TagCompound> add = new List<TagCompound>();
            for (int i = 0; i < addItems.Count; i++) {
                add.Add(new TagCompound { { "item", addItems[i].Item1 }, { "direction", Direction.DIRECTIONS.IndexOf(addItems[i].Item2) } });
            }
            tag.Add("addItems", add);

            List<TagCompound> remove = new List<TagCompound>();
            for (int i = 0; i < removeItems.Count; i++) {
                remove.Add(new TagCompound { { "item", removeItems[i].Item1 }, { "direction", Direction.DIRECTIONS.IndexOf(removeItems[i].Item2) } });
            }
            tag.Add("removeItems", remove);

            return tag;
        }

        public override void Load(TagCompound tag) {
            this.ductType = (DuctType)tag.Get<byte>("ductType");
            if(tag.ContainsKey("filter")) this.filter = (ItemFilter)tag.Get<ItemFilter>("filter");

            if(tag.ContainsKey("flowItems")) this.flowItems = tag.GetList<TagCompound>("flowItems").Select((TagCompound t) => {
                int d = t.Get<int>("direction");
                return Tuple.Create<Item, Direction>(t.Get<Item>("item"), d == -1 ? Direction.NONE : Direction.DIRECTIONS[d]);
            }).ToList();

            if (tag.ContainsKey("addItems")) this.addItems = tag.GetList<TagCompound>("addItems").Select((TagCompound t) => {
                int d = t.Get<int>("direction");
                return Tuple.Create<Item, Direction>(t.Get<Item>("item"), d == -1 ? Direction.NONE : Direction.DIRECTIONS[d]);
            }).ToList();

            if (tag.ContainsKey("removeItems")) this.removeItems = tag.GetList<TagCompound>("removeItems").Select((TagCompound t) => {
                int d = t.Get<int>("direction");
                return Tuple.Create<Item, Direction>(t.Get<Item>("item"), d == -1 ? Direction.NONE : Direction.DIRECTIONS[d]);
            }).ToList();

        }

        public override bool ValidTile(int i, int j) {
            Tile tile = Main.tile[i, j];
            return tile.active() && tile.type == mod.TileType("ItemDuctTile");
        }

        public override int Hook_AfterPlacement(int x, int y, int type, int style, int direction) {
            
            if (Main.netMode == 1) {
                NetMessage.SendTileSquare(Main.myPlayer, x, y, 1, TileChangeType.None);
                NetMessage.SendData(87, -1, -1, null, x, (float)y, 2f, 0f, 0, 0, 0);
                return -1;
            }
            int num = Place(x, y);
            return num;
        }

        public void UpdateFrame(int x, int y) {

            int tfrX = 0;
            int tfrY = 0;

            // choose correct orientation
            Tile uTile = Framing.GetTileSafely(x, y - 1);
            Tile dTile = Framing.GetTileSafely(x, y + 1);
            Tile lTile = Framing.GetTileSafely(x - 1, y);
            Tile rTile = Framing.GetTileSafely(x + 1, y);
            
            bool uCon = uTile.type == mod.TileType("ItemDuctTile");
            bool dCon = dTile.type == mod.TileType("ItemDuctTile");
            bool lCon = lTile.type == mod.TileType("ItemDuctTile");
            bool rCon = rTile.type == mod.TileType("ItemDuctTile");

            int nCon = (uCon ? 1 : 0) + (dCon ? 1 : 0) + (lCon ? 1 : 0) + (rCon ? 1 : 0);

            tfrY = nCon;
            switch (nCon) {
                case 0:
                    // 0, 0
                    break;
                case 1:
                    tfrX = (uCon ? 0 : (dCon ? 1 : (lCon ? 2 : 3)));
                    break;
                case 2:
                    if(uCon && dCon) {
                        tfrX = 0;
                    }else if(lCon && rCon) {
                        tfrX = 1;
                    } else {
                        tfrX = 2 + (dCon ? 2 : 0) + (rCon ? 1 : 0);
                    }
                    break;
                case 3:
                    tfrX = (!dCon ? 0 : (!uCon ? 1 : (!rCon ? 2 : 3)));
                    break;
                case 4:
                    // 0, 4
                    break;
            }

            // offset to get correct tileset

            // tile type
            switch (this.ductType) {
                case DuctType.In:
                    tfrY += 1 * 5;
                    break;
                case DuctType.Out:
                    tfrY += 2 * 5;
                    break;
                case DuctType.None:
                default:
                    break;
            }

            // has item
            if (flowItems.Count > 0) tfrX += 6;

            // set frame
            Main.tile[x, y].frameX = (short)(tfrX * 18);
            Main.tile[x, y].frameY = (short)(tfrY * 18);

            //Main.tile[x, y].frameX = (short)((byte)this.ductType * 18);
            //Main.tile[x, y].frameY = (short)(flowItems.Count > 0 ? 18 : 0);
        }

        public override void OnKill() {
            base.OnKill();
            //Main.NewText("OnKill");
            foreach(Tuple<Item, Direction> ti in flowItems) {
                Item i = ti.Item1;
                //Main.NewText("NewItem " + Position.X * 16 + " " + Position.Y * 16 + " " + 1 + " " + 1 + " " + i.type + " " + i.stack + " " + false + " " + i.prefix + " " + false + " " + false);
                Item.NewItem(Position.X * 16, Position.Y * 16, 1, 1, i.type, i.stack, false, i.prefix, false, false);
            }
        }
    }

    public class ItemDuctTile : ModTile {
        public override void SetDefaults() {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileMergeDirt[Type] = false;
            Main.tileBlockLight[Type] = false;
            Main.tileLighted[Type] = true;
            Main.tileNoAttach[Type] = true;

            TileID.Sets.HasOutlines[Type] = true;

            dustType = mod.DustType("Sparkle");
            drop = mod.ItemType("ItemDuctItem");
            
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.None, 0, 0);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(GetInstance<TEItemDuct>().Hook_AfterPlacement, -1, 0, true);
            TileObjectData.addTile(Type);

            //disableSmartCursor = true;
            //disableSmartInteract = true;

            AddMapEntry(new Color(200, 200, 200));
            // Set other values here

            //dustType = DustType<Sparkle>();
            //drop = ItemType<Items.Placeable.ExamplePlatform>();
        }

        public override bool HasSmartInteract() {
            return false;
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
            base.KillTile(i, j, ref fail, ref effectOnly, ref noItem);
            //Main.NewText("KillTile");
            GetInstance<TEItemDuct>().Kill(i, j);
        }

        public override bool NewRightClick(int i, int j) {
            
            Tile tile = Main.tile[i, j];
            int left = i;
            int top = j;
            
            int index = GetInstance<TEItemDuct>().Find(left, top);
            if (index != -1) {
                TEItemDuct ent = (TEItemDuct)TileEntity.ByID[index];

                foreach (Tuple<Item, Direction> t in ent.flowItems) {
                    Main.NewText(t.Item1.HoverName + " " + t.Item2);
                }

                //if (ent.ductType == TEItemDuct.DuctType.In || ent.ductType == TEItemDuct.DuctType.None) {
                if (!MoreMechanisms.instance.ItemFilterUIVisible()) {
                    MoreMechanisms.instance.itemFilterUIState.i = i * 16;
                    MoreMechanisms.instance.itemFilterUIState.j = j * 16;

                    MoreMechanisms.instance.ShowItemFilterUI(ent.filter.filterItems, ent.filter.filterWhitelist, (Item[] items, bool whitelist) => {
                        ent.filter.filterItems = items;
                        ent.filter.filterWhitelist = whitelist;
                    });
                }
                //}

            }
            
            return true;
        }

        public override bool Slope(int i, int j) {

            //Main.NewText("Slope");

            Tile tile = Main.tile[i, j];
            int left = i;
            int top = j;

            int index = GetInstance<TEItemDuct>().Find(left, top);
            if (index != -1) {
                TEItemDuct ent = (TEItemDuct)TileEntity.ByID[index];
                Main.PlaySound(SoundID.Dig);
                //Main.NewText("ent");

                switch (ent.ductType) {
                    case TEItemDuct.DuctType.None:
                        ent.ductType = TEItemDuct.DuctType.In;
                        if (ent.filter.IsEmpty()) ent.filter.filterWhitelist = true;
                        break;
                    case TEItemDuct.DuctType.In:
                        ent.ductType = TEItemDuct.DuctType.Out;
                        if (ent.filter.IsEmpty()) ent.filter.filterWhitelist = false;
                        break;
                    case TEItemDuct.DuctType.Out:
                        ent.ductType = TEItemDuct.DuctType.None;
                        if (ent.filter.IsEmpty()) ent.filter.filterWhitelist = false;
                        break;
                }

                ent.UpdateFrame(i, j);

                if (Main.netMode == 1) {
                    NetMessage.SendTileSquare(-1, Player.tileTargetX, Player.tileTargetY, 1, TileChangeType.None);
                }
            }
            
            return false;
        }
        
        public override bool PreDraw(int i, int k, SpriteBatch spriteBatch) {

            Tile tile = Main.tile[i, k];
            if (tile.active() && tile.type == mod.TileType("ItemDuctTile")) {
                TileEntity tileEntity = default(TileEntity);

                if (TileEntity.ByPosition.TryGetValue(new Point16(i, k), out tileEntity)) {
                    TEItemDuct es = tileEntity as TEItemDuct;

                    // draw connectors

                    Tile uTile = Framing.GetTileSafely(i, k - 1);
                    Tile dTile = Framing.GetTileSafely(i, k + 1);
                    Tile lTile = Framing.GetTileSafely(i - 1, k);
                    Tile rTile = Framing.GetTileSafely(i + 1, k);
                    
                    bool uCon = es.SpecialConnects(uTile, i, k - 1);
                    bool dCon = es.SpecialConnects(dTile, i, k + 1);
                    bool lCon = es.SpecialConnects(lTile, i - 1, k);
                    bool rCon = es.SpecialConnects(rTile, i + 1, k);

                    if (uCon || dCon || lCon || rCon) {
                        bool[] con = new bool[] { uCon, dCon, lCon, rCon };

                        Color lightCol = Lighting.GetColor(i, k);

                        //TODO: why does the spritebatch seem to draw offset 12 tiles to the -x and -y?
                        int oi = i;
                        int ok = k;

                        i += 12;
                        k += 12;
                        
                        int tfrX = es.flowItems.Count > 0 ? 1 : 0;
                        int frY = 0;

                        switch (es.ductType) {
                            case TEItemDuct.DuctType.In:
                                frY = 15 * 18;
                                break;
                            case TEItemDuct.DuctType.Out:
                                frY = 15 * 18 + 34;
                                break;
                            case TEItemDuct.DuctType.None:
                            default:
                                break;
                        }

                        Texture2D texture = (!Main.canDrawColorTile(tile.type, tile.color())) ? Main.tileTexture[tile.type] : Main.tileAltTexture[tile.type, tile.color()];
                        Vector2 start = new Vector2((float)(i * 16 - (32 - 16) / 2 + 16), (float)(k * 16 - (32 - 16) / 2 + 16));
                        for (int d = 0; d < con.Length; d++) {
                            if (!con[d]) continue;

                            float angle = new float[] { 0, 180, 270, 90 }[d];
                            spriteBatch.End();
                            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                            spriteBatch.Draw(texture, start - Main.screenPosition, new Rectangle(tfrX * 34, frY, 32, 32), lightCol, angle * (float)(Math.PI / 180), new Vector2(16, 16), 1f, SpriteEffects.None, 1f);
                            spriteBatch.End();
                            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                            if (d == 0) Util.DrawWorldTile(spriteBatch, oi, ok - 1, 12 * 16, 12 * 16);
                            if (d == 1) Util.DrawWorldTile(spriteBatch, oi, ok + 1, 12 * 16, 12 * 16);
                            if (d == 2) Util.DrawWorldTile(spriteBatch, oi - 1, ok, 12 * 16, 12 * 16);
                            if (d == 3) Util.DrawWorldTile(spriteBatch, oi + 1, ok, 12 * 16, 12 * 16);
                        }
                    }
                }
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            return base.PreDraw(i, k, spriteBatch);
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            base.PostDraw(i, j, spriteBatch);
        }

    }
}