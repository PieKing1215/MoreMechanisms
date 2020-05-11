using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace MoreMechanisms {
    /// <summary>
    /// Utility class for filtering entities
    /// </summary>
    public class EntityFilter : TagSerializable {
        public static readonly Func<TagCompound, EntityFilter> DESERIALIZER = Load;

        public bool triggerPlayers = false;
        public bool triggerNPCs = false;
        public bool triggerEnemies = false;
        public bool triggerItems = false;
        public bool triggerCoins = false;
        public bool triggerProjectiles = false;
        public bool triggerPets = false;
        public bool triggerLightPets = false;
        public bool triggerMinions = false;
        public bool triggerSentries = false;

        public bool FilterAccepts(Entity ent) {

            if (!ent.active) return false;

            if(ent is Player) {

                return triggerPlayers;
            }else if(ent is NPC) {

                return (ent as NPC).townNPC ? triggerNPCs : triggerEnemies;
            }else if(ent is Item) {
                Item item = ent as Item;
                bool coin = item.type == ItemID.CopperCoin || item.type == ItemID.SilverCoin || item.type == ItemID.GoldCoin || item.type == ItemID.PlatinumCoin;

                return coin ? triggerCoins : triggerItems;
            }else if(ent is Projectile) {
                Projectile projectile = ent as Projectile;
                if (ProjectileID.Sets.LightPet[projectile.type]) {

                    return triggerLightPets;
                }else if (Main.projPet[projectile.type]) {

                    return projectile.minion ? triggerMinions : triggerPets;
                } else if (projectile.sentry) {

                    return triggerSentries;
                }

                return triggerProjectiles;
            }

            return false;
        }

        public List<Entity> GetMatchingEntities() {
            List<Entity> ents = new List<Entity>();

            if (triggerPlayers) {
                ents.AddRange(Main.player);
            }

            if (triggerEnemies || triggerNPCs) {
                ents.AddRange(Main.npc);
            }

            if (triggerItems || triggerCoins) {
                ents.AddRange(Main.item);
            }

            if (triggerProjectiles || triggerPets || triggerLightPets || triggerMinions || triggerSentries) {
                ents.AddRange(Main.projectile);
            }

            return ents.FindAll(FilterAccepts);
        }

        public TagCompound SerializeData() {
            TagCompound tag = new TagCompound();

            tag.Add("triggerPlayers"    , triggerPlayers);
            tag.Add("triggerNPCs"       , triggerNPCs);
            tag.Add("triggerEnemies"    , triggerEnemies);
            tag.Add("triggerItems"      , triggerItems);
            tag.Add("triggerCoins"      , triggerCoins);
            tag.Add("triggerProjectiles", triggerProjectiles);
            tag.Add("triggerPets"       , triggerPets);
            tag.Add("triggerLightPets"  , triggerLightPets);
            tag.Add("triggerMinions"    , triggerMinions);
            tag.Add("triggerSentries"   , triggerSentries);

            return tag;
        }

        public static EntityFilter Load(TagCompound tag) {
            EntityFilter f = new EntityFilter();

            f.triggerPlayers     = tag.Get<bool>("triggerPlayers");
            f.triggerNPCs        = tag.Get<bool>("triggerNPCs");
            f.triggerEnemies     = tag.Get<bool>("triggerEnemies");
            f.triggerItems       = tag.Get<bool>("triggerItems");
            f.triggerCoins       = tag.Get<bool>("triggerCoins");
            f.triggerProjectiles = tag.Get<bool>("triggerProjectiles");
            f.triggerPets        = tag.Get<bool>("triggerPets");
            f.triggerLightPets   = tag.Get<bool>("triggerLightPets");
            f.triggerMinions     = tag.Get<bool>("triggerMinions");
            f.triggerSentries    = tag.Get<bool>("triggerSentries");

            return f;
        }
    }
}

namespace MoreMechanisms.ExtensionMethods {
    public static class EntityExtensions {
        public static Rectangle GetRect(this Entity ent) {
            // this is all the getRect functions in the specific classes do
            return new Rectangle((int)ent.position.X, (int)ent.position.Y, ent.width, ent.height);
        }
    }
}