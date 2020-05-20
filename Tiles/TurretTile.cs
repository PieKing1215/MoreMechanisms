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

    public class TETurret : ModTileEntity, IConnectable {

        internal int range = 20 * 16;
        internal int damage = 20;
        internal int shootTime = 10;
        internal int shootCooldown = 0;

        internal bool on = true;
        internal bool changed = false;

        internal Item bullets;
        internal Texture2D circle;

        public void Reset() {
            on = true;
            bullets = new Item();
            bullets.SetDefaults(0);
        }

        public override void PreGlobalUpdate() {
            base.PreGlobalUpdate();
        }
        
        public override void PostGlobalUpdate() {
            base.PostGlobalUpdate();
        }
        
        public override void Update() {
            
            if(shootCooldown == 0 && bullets != null && !bullets.IsAir) {
                Vector2 pos = new Vector2(Position.X * 16 + 16, Position.Y * 16 + 16);

                List<NPC> npcs = new List<NPC>(Main.npc);
                npcs = npcs.Where((n) => {
                    return !n.townNPC && n.chaseable && !n.dontTakeDamage && !n.immortal;
                }).ToList();

                List<Entity> ents = new List<Entity>(/*Main.player*/);
                ents.AddRange(npcs);

                ents = ents.Where((n) => {
                    return n.active && (n.DistanceSQ(pos) < (range * range));
                }).ToList();

                ents.Sort((n1, n2) => {
                    float d1 = n1.DistanceSQ(pos);
                    float d2 = n2.DistanceSQ(pos);

                    return d1 == d2 ? 0 : (d1 < d2 ? -1 : 1);
                });

                if (ents.Count > 0) {
                    //Vector2 dir = new Vector2((float)Math.Sin(Main.GameUpdateCount / 40f), (float)Math.Cos(Main.GameUpdateCount / 40f));
                    float dist = (ents[0].Center - pos).Length();
                    Vector2 targetPos = ents[0].Center + ents[0].velocity * dist / 24f;
                    Vector2 dir = targetPos - pos;

                    dir.Normalize();
                    dir += Main.rand.NextVector2Square(-0.05f, 0.05f);
                    dir.Normalize();
                    float angle = MathHelper.ToDegrees((float)Math.Atan2(dir.Y, dir.X));
                    if (angle < 0) angle += 360;
                    dir = dir * 10;

                    
                    int n = (int)Math.Round((angle / 360f) * 12) % 12;
                    short fr1 = (short)(n * 18 * 2);
                    for(int fx = 0; fx < 2; fx++) {
                        for(int fy = 0; fy < 2; fy++) {
                            Main.tile[Position.X + fx, Position.Y + fy].frameX = (short)(fr1 + 18 * fx);
                            Main.tile[Position.X + fx, Position.Y + fy].frameY = (short)(18 * fy);
                        }
                    }
                    Main.PlaySound(SoundID.Item11.WithVolume(0.8f), pos);
                    Projectile.NewProjectile(pos, dir, bullets.shoot, damage + bullets.damage, bullets.knockBack, bullets.owner);

                    if (bullets.stack > 1) {
                        bullets.stack--;
                        if (MoreMechanisms.instance.TurretUIVisible()) {
                            MoreMechanisms.instance.turretUIState.UpdateTooltip();
                        }
                    } else {
                        bullets.TurnToAir();
                        if (MoreMechanisms.instance.TurretUIVisible()) {
                            MoreMechanisms.instance.turretUIState.UpdateTooltip();
                        }
                    }

                    shootCooldown = shootTime;
                }
            }

            if (shootCooldown > 0) shootCooldown--;

            if (changed) {
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
                changed = false;
            }
        }

        public override void NetReceive(BinaryReader reader, bool lightReceive) {
            Reset();
            this.on = reader.ReadBoolean();
            this.bullets = reader.ReadItem();
        }

        public override void NetSend(BinaryWriter writer, bool lightSend) {
            writer.Write(this.on);
            writer.WriteItem(bullets, true);
        }

        public override TagCompound Save() {
            TagCompound cpd = new TagCompound();
            cpd.Add("on", this.on);
            if(bullets != null && !bullets.IsAir) cpd.Add("bullets", bullets);
            return cpd;
        }

        public override void Load(TagCompound tag) {
            Reset();
            this.on = tag.Get<bool>("on");
            Item i = tag.Get<Item>("bullets");
            if (i != null) this.bullets = i;
        }

        public override bool ValidTile(int i, int j) {
            Tile tile = Main.tile[i, j];
            return tile.active() && tile.type == mod.TileType("TurretTile");
        }

        public override int Hook_AfterPlacement(int x, int y, int type, int style, int direction) {
            if (Main.netMode == 1) {
                NetMessage.SendTileSquare(Main.myPlayer, x, y, 1, TileChangeType.None);
                NetMessage.SendData(87, -1, -1, null, x, (float)y, 2f, 0f, 0, 0, 0);
                return -1;
            }
            int num = Place(x, y);
            ((TETurret)TileEntity.ByID[num]).Reset();
            return num;
        }

        public override void OnKill() {
            base.OnKill();
            if (bullets != null) {
                Item.NewItem(Position.X * 16, Position.Y * 16, 1, 1, bullets.type, bullets.stack, false, bullets.prefix, false, false);
            }
        }

        public Item[] GetItems(ConnectableType type) {
            if(type == ConnectableType.Output) {
                return new Item[] { bullets };
            }

            return null;
        }

        public bool Accepts(Item item, ConnectableType type) {
            if(type == ConnectableType.Output) {
                return item.IsAir || (!item.notAmmo && item.ammo == AmmoID.Bullet);
            }

            return false;
        }

        public void TransferredItem(Item transferred, int index, ConnectableType type) {
            if (type == ConnectableType.Output) {
                this.bullets = transferred;
                if (MoreMechanisms.instance.TurretUIVisible()) {
                    MoreMechanisms.instance.turretUIState.SetItem(this.bullets);
                }
            }
        }
    }

    public class TurretTile : ModTile {
        public override void SetDefaults() {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileMergeDirt[Type] = false;
            Main.tileBlockLight[Type] = false;
            Main.tileLighted[Type] = true;
            Main.tileNoAttach[Type] = true;

            TileID.Sets.HasOutlines[Type] = true;

            dustType = mod.DustType("Sparkle");
            //drop = mod.ItemType("TurretItem");
            
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.None, 0, 0);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(GetInstance<TETurret>().Hook_AfterPlacement, -1, 0, true);
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
            Item.NewItem(i * 16, j * 16, 32, 32, mod.ItemType("TurretItem"));
            GetInstance<TETurret>().Kill(i, j);
        }

        public override bool NewRightClick(int i, int j) {
            
            Tile tile = Main.tile[i, j];
            int left = i - (tile.frameX % 36) / 18;
            int top = j - (tile.frameY % 36) / 18;

            int index = GetInstance<TETurret>().Find(left, top);
            if (index != -1) {
                TETurret ent = (TETurret)TileEntity.ByID[index];

                if (!MoreMechanisms.instance.TurretUIVisible()) {
                    MoreMechanisms.instance.turretUIState.i = i * 16;
                    MoreMechanisms.instance.turretUIState.j = j * 16;
                    
                    MoreMechanisms.instance.ShowTurretUI(ent.bullets, (Item it) => {
                        ent.bullets = it;
                    });
                }
            }
            
            return true;
        }

        public override void HitWire(int i, int j) {
            base.HitWire(i, j);

            Tile tile = Main.tile[i, j];
            int left = i - (tile.frameX % 36) / 18;
            int top = j - (tile.frameY % 36) / 18;

            int index = GetInstance<TETurret>().Find(left, top);
            if (index != -1) {
                TETurret qe = (TETurret)TileEntity.ByID[index];

                qe.on = !qe.on;
                if(qe.on) Main.PlaySound(SoundID.Item23, left * 16, top * 16);
            }
        }

    }
}