using Terraria;
using Terraria.ModLoader;

namespace MoreMechanisms.Items {
    public class InvasionSensorItem : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Invasion Sensor");
            Tooltip.SetDefault("Detects invasions.");
        }

        public override void SetDefaults() {
            item.value = Item.buyPrice(1, 0, 0, 0);

            item.useAnimation = 15;
            item.useTime = 10;
            item.maxStack = 999;
            item.rare = 1;
            item.createTile = mod.TileType("InvasionSensorTile");
            item.mech = true;

            item.useStyle = 1;
            item.useTurn = true;
            item.autoReuse = true;
            item.consumable = true;
            item.placeStyle = 0;
            item.width = 12;
            item.height = 12;
        }

        public override void AddRecipes() {
            // Recipes here. See Basic Recipe Guide
        }
    }
}