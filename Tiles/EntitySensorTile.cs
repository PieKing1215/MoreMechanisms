using Microsoft.Xna.Framework;
using MoreMechanisms.ExtensionMethods;
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

    public class TEEntitySensor : TESensorBase<TEEntitySensor> {
        
        private List<Rectangle> playerBox = new List<Rectangle>();
        
        public int left   = 2; //2
        public int right  = 2; //2
        public int top    = 10; //10
        public int bottom = -1; //-1

        public EntityFilter filter;

        public override int GetTileType() => mod.TileType("EntitySensorTile");

        public void ClientSendServer() {
            ModPacket myPacket = MoreMechanisms.instance.GetPacket();
            myPacket.Write((byte)3); // id
            myPacket.Write((short)Position.X);
            myPacket.Write((short)Position.Y);
            NetSend(myPacket, false);
            myPacket.Send();
        }

        private void FillPlayerHitboxes() {
            playerBox.Clear();
            playerBox.AddRange(filter.GetMatchingEntities().Select(e => e.GetRect()));
        }

        public override void Update() {
            FillPlayerHitboxes();

            base.Update();
        }

        public override bool GetState() {
            Rectangle value = new Rectangle(Position.X * 16 - (16 * left) - 1, Position.Y * 16 - (16 * top) - 1, (right + left + 1) * 16 + 2, (bottom + top + 1) * 16 + 2);
            foreach (Rectangle rec in playerBox) {
                if (rec.Intersects(value)) {
                    return true;
                }
            }
            return false;
        }
        
        public override void NetReceive(BinaryReader reader, bool lightReceive) {
            base.NetReceive(reader, lightReceive);
            
            this.left = reader.ReadInt32();
            this.right = reader.ReadInt32();
            this.top = reader.ReadInt32();
            this.bottom = reader.ReadInt32();

            this.filter = new EntityFilter();
            this.filter.triggerPlayers     = reader.ReadBoolean();
            this.filter.triggerNPCs        = reader.ReadBoolean();
            this.filter.triggerEnemies     = reader.ReadBoolean();
            this.filter.triggerItems       = reader.ReadBoolean();
            this.filter.triggerCoins       = reader.ReadBoolean();
            this.filter.triggerProjectiles = reader.ReadBoolean();
            this.filter.triggerPets        = reader.ReadBoolean();
            this.filter.triggerLightPets   = reader.ReadBoolean();
            this.filter.triggerMinions     = reader.ReadBoolean();
            this.filter.triggerSentries    = reader.ReadBoolean();
        }

        public override void NetSend(BinaryWriter writer, bool lightSend) {
            base.NetSend(writer, lightSend);
            
            writer.Write(this.left);
            writer.Write(this.right);
            writer.Write(this.top);
            writer.Write(this.bottom);

            writer.Write(this.filter.triggerPlayers);
            writer.Write(this.filter.triggerNPCs);
            writer.Write(this.filter.triggerEnemies);
            writer.Write(this.filter.triggerItems);
            writer.Write(this.filter.triggerCoins);
            writer.Write(this.filter.triggerProjectiles);
            writer.Write(this.filter.triggerPets);
            writer.Write(this.filter.triggerLightPets);
            writer.Write(this.filter.triggerMinions);
            writer.Write(this.filter.triggerSentries);
            
        }

        public override TagCompound Save() {
            TagCompound tag = base.Save();
            
            tag.Add("left", this.left);
            tag.Add("right", this.right);
            tag.Add("top", this.top);
            tag.Add("bottom", this.bottom);

            tag.Add("filter", this.filter);

            return tag;
        }

        public override void Load(TagCompound tag) {
            base.Load(tag);
            
            this.left   = tag.Get<int>("left");
            this.right  = tag.Get<int>("right");
            this.top    = tag.Get<int>("top");
            this.bottom = tag.Get<int>("bottom");

            this.filter = tag.Get<EntityFilter>("filter");
        }
        
        public override void FigureCheckState() {
            if (this.filter == null) {
                this.filter = new EntityFilter();
                this.filter.triggerPlayers = true;
            }
        }

    }

    public class EntitySensorTile : ModTile {
        public override void SetDefaults() {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileMergeDirt[Type] = false;
            Main.tileBlockLight[Type] = false;
            Main.tileLighted[Type] = true;
            Main.tileNoAttach[Type] = true;

            TileID.Sets.HasOutlines[Type] = true;

            dustType = mod.DustType("Sparkle");
            drop = mod.ItemType("EntitySensorItem");
            
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.None, 0, 0);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(GetInstance<TEEntitySensor>().Hook_AfterPlacement, -1, 0, true);
            TileObjectData.addTile(Type);
            
            AddMapEntry(new Color(200, 200, 200));
        }

        public override bool HasSmartInteract() {
            return true;
        }

        public override void HitWire(int i, int j) {
            Tile tile = Main.tile[i, j];
            int left = i - tile.frameX % 36 / 18;
            int top = j - tile.frameY / 18;

            int index = GetInstance<TEEntitySensor>().Find(left, top);
            if (index != -1) {
                TEEntitySensor speakerEnt = (TEEntitySensor)TileEntity.ByID[index];
                
            }
        }

        public override bool NewRightClick(int i, int j) {
            
            Tile tile = Main.tile[i, j];
            int left = i;
            int top = j;
            
            int index = GetInstance<TEEntitySensor>().Find(left, top);
            if (index != -1) {
                TEEntitySensor ent = (TEEntitySensor)TileEntity.ByID[index];

                MoreMechanisms.instance.entitySensorUIState.i = i * 16;
                MoreMechanisms.instance.entitySensorUIState.j = j * 16;
                if (!MoreMechanisms.instance.EntitySensorUIVisible())  MoreMechanisms.instance.ShowEntitySensorUI();
            }
            
            return true;
        }

        public override void MouseOver(int i, int j) {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.showItemIcon = true;
            player.showItemIcon2 = mod.ItemType("EntitySensorItem");
        }
        
    }
}