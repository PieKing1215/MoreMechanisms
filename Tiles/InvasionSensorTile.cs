using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace MoreMechanisms.Tiles {

    public class TEInvasionSensor : TESensorBase<TEInvasionSensor> {

        public override int GetTileType() => mod.TileType("TEInvasionSensor");

        public override bool GetState() {
            return Main.invasionType != InvasionID.None && Math.Abs(Position.X - Main.invasionX) <= 150;
        }
    }

    public class InvasionSensorTile : ModTile {
        public override void SetDefaults() {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileMergeDirt[Type] = false;
            Main.tileBlockLight[Type] = false;
            Main.tileLighted[Type] = true;
            Main.tileNoAttach[Type] = true;

            TileID.Sets.HasOutlines[Type] = true;

            dustType = mod.DustType("Sparkle");
            drop = mod.ItemType("InvasionSensorItem");


            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.None, 0, 0);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(GetInstance<TEInvasionSensor>().Hook_AfterPlacement, -1, 0, true);
            TileObjectData.addTile(Type);
            
            //disableSmartCursor = true;
            //disableSmartInteract = true;

            AddMapEntry(new Color(200, 200, 200));
            // Set other values here

            //dustType = DustType<Sparkle>();
            //drop = ItemType<Items.Placeable.ExamplePlatform>();
        }
        
        public override bool NewRightClick(int i, int j) {
            
            Tile tile = Main.tile[i, j];
            int left = i;
            int top = j;
            
            int index = GetInstance<TEInvasionSensor>().Find(left, top);
            if (index != -1) {
                TEInvasionSensor ent = (TEInvasionSensor)TileEntity.ByID[index];
                
            }
            
            return true;
        }
        
    }
}