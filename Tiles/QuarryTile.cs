using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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

    public class TEQuarry : ModTileEntity {

        internal bool on = false;
        internal bool mined = false;

        internal bool changed = false;

        internal Item pickaxe;
        public ItemFilter filter;
        
        public int left = 2; //2
        public int right = 2; //2
        public int top = 10; //10
        public int bottom = 0; //-1
        public bool hasFrame = false;

        public void Reset() {
            on = false;
            changed = false;
            pickaxe = new Item();
            pickaxe.SetDefaults(0);
            filter = new ItemFilter(5);
        }

        public override void PreGlobalUpdate() {
            base.PreGlobalUpdate();
        }
        
        public override void PostGlobalUpdate() {
            base.PostGlobalUpdate();
        }
        
        public void UpdateFrame() {
            //Main.NewText("UpdateFrame");
            List<Tuple<Tuple<int, int>, Direction>> lookForStart = new List<Tuple<Tuple<int, int>, Direction>>();
            lookForStart.Add(Tuple.Create(Tuple.Create(1, -1), Direction.LEFT));
            lookForStart.Add(Tuple.Create(Tuple.Create(3, 1), Direction.UP));
            lookForStart.Add(Tuple.Create(Tuple.Create(1, 3), Direction.RIGHT));
            lookForStart.Add(Tuple.Create(Tuple.Create(-1, 1), Direction.DOWN));

            foreach(Tuple<Tuple<int, int>, Direction> start in lookForStart) {
                int sx = Position.X + start.Item1.Item1;
                int sy = Position.Y + start.Item1.Item2;
                int minX = sx;
                int maxX = sx;
                int minY = sy;
                int maxY = sy;

                Tile st = Main.tile[sx, sy];
                if(st.type == TileType<QuarryScaffoldTile>()) {
                    Direction cdir = start.Item2;
                    int cx = sx;
                    int cy = sy;
                    while (true) {
                        Direction rdir = cdir.Clockwise;
                        if(Main.tile[cx + rdir.dx, cy + rdir.dy].type == TileType<QuarryScaffoldTile>()) {
                            cdir = rdir;
                            cx += cdir.dx;
                            cy += cdir.dy;
                        } else if(Main.tile[cx + cdir.dx, cy + cdir.dy].type == TileType<QuarryScaffoldTile>()) {
                            cx += cdir.dx;
                            cy += cdir.dy;
                        } else {
                            break;
                        }

                        minX = Math.Min(minX, cx);
                        maxX = Math.Max(maxX, cx);
                        minY = Math.Min(minY, cy);
                        maxY = Math.Max(maxY, cy);

                        if (cx == sx && cy == sy) {
                            hasFrame = true;

                            minX += start.Item1.Item1;
                            maxX += start.Item1.Item1;
                            minY += start.Item1.Item2;
                            maxY += start.Item1.Item2;

                            left = sx - minX;
                            right = maxX - sx;
                            top = sy - minY;
                            bottom = maxY - sy;

                            //Main.NewText(sx + " " + sy + " " + minX + " " + maxX + " " + minY + " " + maxY);
                            //Main.NewText(left + " " + right + " " + top + " " + bottom);
                            return;
                        }
                    }
                }
            }

            hasFrame = false;
        }

        public List<Item> GetItemsInFrame() {
            List<Item> items = new List<Item>();

            if (hasFrame) {
                foreach(Item i in Main.item) {
                    if (i.active) {
                        if(i.position.X > (Position.X - left) * 16 && i.position.X < (Position.X + right) * 16) {
                            if (i.position.Y > (Position.Y - top) * 16 && i.position.Y < (Position.Y + bottom) * 16) {
                                items.Add(i);
                            }
                        }
                    }
                }
            }

            return items;
        }

        public override void Update() {
            mined = false;

            if (Main.GameUpdateCount % 30 == 0) {
                UpdateFrame();
                //Main.NewText("hasFrame = " + hasFrame);
            }

            if (on && Main.GameUpdateCount % 30 == 0) {
                Main.PlaySound(SoundID.Item22, Position.X * 16, Position.Y * 16);
            }

            if(on && hasFrame && pickaxe != null && pickaxe.pick > 0) {
                if (Main.GameUpdateCount % pickaxe.useTime == 0) {
                    //Main.PlaySound(SoundID.Dig, Position.X * 16, Position.Y * 16);

                    for(int y = -top; y <= bottom; y++) {
                        for(int x = -left; x <= right; x++) {
                            int tx = Position.X + x;
                            int ty = Position.Y + y;
                            if (Main.tile[tx, ty] != null && Main.tile[tx, ty].active() && Main.tileSolid[Main.tile[tx, ty].type] && filter.FilterAccepts(Main.tile[tx, ty])) {
                                if(Util.CanPickTile(tx, ty, pickaxe.pick)) {
                                    List<Item> before = GetItemsInFrame();
                                    Util.PickTile(tx, ty, pickaxe.pick);
                                    List<Item> after = GetItemsInFrame();
                                    foreach(Item i in after) {
                                        if (!before.Contains(i)) {
                                            i.position.X = (Position.X + 1) * 16 - 8 + Main.rand.Next(16);
                                            i.position.Y = (Position.Y + 1) * 16 - 8 + Main.rand.Next(16);
                                            i.velocity.X /= 4;
                                            i.velocity.Y /= 4;
                                            i.instanced = true;
                                        }
                                    }
                                    mined = true;
                                    goto BrokeBlock;
                                }
                            }
                            
                        }
                    }
                BrokeBlock: { }
                }
            }
            
            if (changed) {
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
                changed = false;
            }
        }

        public override void NetReceive(BinaryReader reader, bool lightReceive) {
            Reset();
            this.on = reader.ReadBoolean();
            this.pickaxe = reader.ReadItem();
        }

        public override void NetSend(BinaryWriter writer, bool lightSend) {
            writer.Write(this.on);
            writer.WriteItem(pickaxe, true);
        }

        public override TagCompound Save() {
            TagCompound cpd = new TagCompound();
            cpd.Add("on", this.on);
            if(pickaxe != null && !pickaxe.IsAir) cpd.Add("pickaxe", pickaxe);
            return cpd;
        }

        public override void Load(TagCompound tag) {
            Reset();
            this.on = tag.Get<bool>("on");
            Item i = tag.Get<Item>("pickaxe");
            if (i != null) this.pickaxe = i;
        }

        public override bool ValidTile(int i, int j) {
            Tile tile = Main.tile[i, j];
            return tile.active() && tile.type == mod.TileType("QuarryTile");
        }

        public override int Hook_AfterPlacement(int x, int y, int type, int style, int direction) {
            if (Main.netMode == 1) {
                NetMessage.SendTileSquare(Main.myPlayer, x, y, 1, TileChangeType.None);
                NetMessage.SendData(87, -1, -1, null, x, (float)y, 2f, 0f, 0, 0, 0);
                return -1;
            }
            int num = Place(x, y);
            ((TEQuarry)TileEntity.ByID[num]).Reset();
            return num;
        }

        public override void OnKill() {
            base.OnKill();
            if (pickaxe != null) {
                Item.NewItem(Position.X * 16, Position.Y * 16, 1, 1, pickaxe.type, pickaxe.stack, false, pickaxe.prefix, false, false);
            }
        }
    }

    public class QuarryTile : ModTile {
        public override void SetDefaults() {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileMergeDirt[Type] = false;
            Main.tileBlockLight[Type] = false;
            Main.tileLighted[Type] = true;
            Main.tileNoAttach[Type] = true;

            animationFrameHeight = 54;

            TileID.Sets.HasOutlines[Type] = true;

            dustType = mod.DustType("Sparkle");
            //drop = mod.ItemType("QuarryItem");
            
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.None, 0, 0);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(GetInstance<TEQuarry>().Hook_AfterPlacement, -1, 0, true);
            TileObjectData.addTile(Type);
            
            //disableSmartCursor = true;
            //disableSmartInteract = true;

            AddMapEntry(new Color(200, 200, 200));
            // Set other values here

            //dustType = DustType<Sparkle>();
            //drop = ItemType<Items.Placeable.ExamplePlatform>();
        }

        public override bool HasSmartInteract() {
            return true;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) {
            Item.NewItem(i * 16, j * 16, 32, 32, mod.ItemType("QuarryItem"));
            GetInstance<TEQuarry>().Kill(i, j);
        }

        public override bool NewRightClick(int i, int j) {
            
            Tile tile = Main.tile[i, j];
            int left = i - tile.frameX / 18;
            int top = j - tile.frameY / 18;

            int index = GetInstance<TEQuarry>().Find(left, top);
            if (index != -1) {
                TEQuarry ent = (TEQuarry)TileEntity.ByID[index];

                if (!MoreMechanisms.instance.QuarryUIVisible()) {
                    MoreMechanisms.instance.quarryUIState.i = i * 16;
                    MoreMechanisms.instance.quarryUIState.j = j * 16;
                    
                    MoreMechanisms.instance.ShowQuarryUI(ent.pickaxe, ent.filter, (Item it) => {
                        ent.pickaxe = it;
                    });
                }
            }
            
            return true;
        }

        public override void HitWire(int i, int j) {
            base.HitWire(i, j);

            Tile tile = Main.tile[i, j];
            int left = i - tile.frameX / 18;
            int top = j - tile.frameY / 18;

            int index = GetInstance<TEQuarry>().Find(left, top);
            if (index != -1) {
                TEQuarry qe = (TEQuarry)TileEntity.ByID[index];

                qe.on = !qe.on;
                if(qe.on) Main.PlaySound(SoundID.Item23, left * 16, top * 16);
            }
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex) {
            base.DrawEffects(i, j, spriteBatch, ref drawColor, ref nextSpecialDrawIndex);

            Tile tile = Main.tile[i, j];
            int left = i - tile.frameX / 18;
            int top = j - tile.frameY / 18;

            int index = GetInstance<TEQuarry>().Find(left, top);
            if (index != -1) {
                TEQuarry qe = (TEQuarry)TileEntity.ByID[index];

                if (qe.on && Main.rand.Next(10) == 0) {
                    Vector2 pos = new Vector2(i * 16, j * 16);
                    int ppi = Dust.NewDust(pos, 16, 16, DustID.Smoke);
                    Main.dust[ppi].velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
                    Main.dust[ppi].alpha = 127;
                }

                if (qe.mined && tile.frameX == 18 && (tile.frameY % (18 * 3)) == 0) {
                    if (Main.rand.Next(1) == 0) {
                        Vector2 pos = new Vector2(i * 16, j * 16);
                        int ppi = Dust.NewDust(pos, 16, 4, DustID.Dirt);
                        Main.dust[ppi].velocity = Main.rand.NextVector2Circular(0.5f, 0.5f) + new Vector2(0, -2);
                        Main.dust[ppi].alpha = 127;
                    }
                }
            }
        }

        public override void AnimateTile(ref int frame, ref int frameCounter) {
            frameCounter++;
            if (frameCounter > 5)  //this is the frames speed, the bigger is the value the slower are the frames
            {
                frameCounter = 0;
                frame++;
                if (frame > 13)   //this is where you add how may frames your spritesheet has but -1, so if it has 4 frames you put 3 etc.
                {
                    frame = 0;
                }
            }
        }

    }
}