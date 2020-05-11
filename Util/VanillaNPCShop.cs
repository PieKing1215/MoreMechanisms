using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreMechanisms {
    public class VanillaNPCShop : GlobalNPC {
        public override void SetupShop(int type, Chest shop, ref int nextSlot) {
            switch (type) {
                case NPCID.Steampunker:
                    //shop.item[nextSlot++].SetDefaults(mod.ItemType("SpeakerItem"));
                    //shop.item[nextSlot++].SetDefaults(mod.ItemType("EntitySensorItem"));
                    //shop.item[nextSlot++].SetDefaults(mod.ItemType("BloodMoonSensorItem"));
                    //shop.item[nextSlot++].SetDefaults(mod.ItemType("SolarEclipseSensorItem"));
                    //shop.item[nextSlot++].SetDefaults(mod.ItemType("InvasionSensorItem"));
                    //shop.item[nextSlot++].SetDefaults(mod.ItemType("ItemDuctItem"));
                    //shop.item[nextSlot++].SetDefaults(mod.ItemType("QuarryItem"));
                    //shop.item[nextSlot++].SetDefaults(mod.ItemType("QuarryScaffoldItem"));
                    shop.item[nextSlot++].SetDefaults(mod.ItemType("VacuumItem"));
                    shop.item[nextSlot++].SetDefaults(mod.ItemType("TurretItem"));
                    shop.item[nextSlot++].SetDefaults(mod.ItemType("DelayCircuitItem"));
                    shop.item[nextSlot++].SetDefaults(mod.ItemType("SellerItem"));
                    shop.item[nextSlot++].SetDefaults(mod.ItemType("DropperItem"));
                    break;
            }
        }
    }
}