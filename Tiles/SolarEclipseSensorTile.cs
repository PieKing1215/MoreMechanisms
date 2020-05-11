using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace MoreMechanisms.Tiles {

    public class TESolarEclipseSensor : TESensorBase<TESolarEclipseSensor> {

        public override int GetTileType() => mod.TileType("TESolarEclipseSensor");

        public override bool GetState() => Main.eclipse;
    }

    public class SolarEclipseSensorTile : ModTile {
        public override void SetDefaults() {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileMergeDirt[Type] = false;
            Main.tileBlockLight[Type] = false;
            Main.tileLighted[Type] = true;
            Main.tileNoAttach[Type] = true;

            TileID.Sets.HasOutlines[Type] = true;

            dustType = mod.DustType("Sparkle");
            drop = mod.ItemType("SolarEclipseSensorItem");
            
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.None, 0, 0);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(GetInstance<TESolarEclipseSensor>().Hook_AfterPlacement, -1, 0, true);
            TileObjectData.addTile(Type);
            
            AddMapEntry(new Color(200, 200, 200));
        }
        
        public override bool NewRightClick(int i, int j) {
            
            Tile tile = Main.tile[i, j];
            int left = i;
            int top = j;
            
            int index = GetInstance<TESolarEclipseSensor>().Find(left, top);
            if (index != -1) {
                TESolarEclipseSensor ent = (TESolarEclipseSensor)TileEntity.ByID[index];
                
            }
            
            return true;
        }
        
    }
}