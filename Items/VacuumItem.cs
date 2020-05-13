using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreMechanisms.Items {
    public class VacuumItem : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Vacuum");
            Tooltip.SetDefault("Sucks up items from the world.");
        }

        public override void SetDefaults() {
            item.value = Item.buyPrice(0, 0, 0, 0);

            item.useAnimation = 15;
            item.useTime = 10;
            item.maxStack = 999;
            item.rare = 1;
            item.createTile = mod.TileType("VacuumTile");
            item.mech = true;

            item.useStyle = 1;
            item.useTurn = true;
            item.autoReuse = true;
            item.consumable = true;
            item.placeStyle = 0;
            item.width = 12;
            item.height = 12;

            //TODO: try to see if there's a way to make an animated placeable item not have the 
            //      graphical bug where it displays the whole sheet on the cursor
            //Main.RegisterItemAnimation(item.type, new DrawAnimationVertical(10, 4));

            // Set other item.X values here
        }

        public override void AddRecipes() {
            // Recipes here. See Basic Recipe Guide
        }
    }
}