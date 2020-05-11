using Terraria;
using Terraria.ModLoader;

namespace MoreMechanisms.Items {
    public class DropperItem : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Dropper");
            Tooltip.SetDefault("Drops items placed inside when powered.");
        }

        public override void SetDefaults() {
            item.value = Item.buyPrice(0, 20, 0, 0);

            item.useAnimation = 15;
            item.useTime = 10;
            item.maxStack = 999;
            item.rare = 1;
            item.createTile = mod.TileType("DropperTile");
            item.mech = true;

            item.useStyle = 1;
            item.useTurn = true;
            item.autoReuse = true;
            item.consumable = true;
            item.placeStyle = 0;
            item.width = 12;
            item.height = 12;
            // Set other item.X values here
        }

        public override void AddRecipes() {
            // Recipes here. See Basic Recipe Guide
        }
    }
}