using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreMechanisms {
    public class VanillaNPCShop : GlobalNPC {
        public override void SetupShop(int type, Chest shop, ref int nextSlot) {
            switch (type) {
                case NPCID.Steampunker:
                    //shop.item[nextSlot++].SetDefaults(mod.ItemType("EntitySensorItem")); // crafted
                    shop.item[nextSlot++].SetDefaults(mod.ItemType("VacuumItem"));
                    shop.item[nextSlot++].SetDefaults(mod.ItemType("SellerItem"));
                    shop.item[nextSlot++].SetDefaults(mod.ItemType("QuarryItem"));
                    shop.item[nextSlot++].SetDefaults(mod.ItemType("QuarryScaffoldItem"));
                    break;
                case NPCID.Mechanic:
                    shop.item[nextSlot++].SetDefaults(mod.ItemType("DelayCircuitItem")); // will be crafted
                    shop.item[nextSlot++].SetDefaults(mod.ItemType("SpeakerItem"));
                    shop.item[nextSlot++].SetDefaults(mod.ItemType("BloodMoonSensorItem"));
                    if (NPC.downedMechBossAny) shop.item[nextSlot++].SetDefaults(mod.ItemType("SolarEclipseSensorItem"));
                    if (NPC.downedGoblins) shop.item[nextSlot++].SetDefaults(mod.ItemType("InvasionSensorItem"));
                    if (NPC.AnyNPCs(NPCID.ArmsDealer)) shop.item[nextSlot++].SetDefaults(mod.ItemType("TurretItem"));
                    shop.item[nextSlot++].SetDefaults(mod.ItemType("ItemDuctItem"));
                    shop.item[nextSlot++].SetDefaults(mod.ItemType("DropperItem"));
                    break;
            }
        }
    }
}