using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace MoreMechanisms {
    /// <summary>
    /// Utility class for filtering items
    /// </summary>
    public class ItemFilter : TagSerializable {
        public static readonly Func<TagCompound, ItemFilter> DESERIALIZER = Load;

        public Item[] filterItems;
        public bool filterWhitelist = true;

        public ItemFilter(int num) : this(num, true) {}

        public ItemFilter(int num, bool whitelist) {
            filterItems = new Item[num];

            for (int i = 0; i < filterItems.Length; i++) {
                filterItems[i] = new Item();
                filterItems[i].SetDefaults(0);
            }
            filterWhitelist = whitelist;

        }

        public bool IsEmpty() {
            for (int i = 0; i < filterItems.Length; i++) {
                if(!filterItems[i].IsAir) return false;
            }
            return true;
        }

        public bool FilterAccepts(Item i) {
            foreach (Item f in filterItems) {
                if (f != null) {
                    if (f.type == i.type) {
                        if (filterWhitelist) return true; // allow if on whitelist
                        if (!filterWhitelist) return false; // disallow if on blacklist
                    }
                }
            }

            return !filterWhitelist; // unspecified items are only allowed if it's a blacklist
        }

        public bool FilterAccepts(Tile t) {
            foreach (Item f in filterItems) {
                if (f != null) {
                    if (f.createTile == t.type) {
                        if (filterWhitelist) return true; // allow if on whitelist
                        if (!filterWhitelist) return false; // disallow if on blacklist
                    }
                }
            }

            return !filterWhitelist; // unspecified items are only allowed if it's a blacklist
        }

        public TagCompound SerializeData() {
            TagCompound tag = new TagCompound();
            
            tag.Add("filterItems", new List<Item>(filterItems));
            tag.Add("filterWhitelist", filterWhitelist);

            return tag;
        }

        public static ItemFilter Load(TagCompound tag) {
            Item[] it = tag.Get<List<Item>>("filterItems").ToArray();

            ItemFilter f = new ItemFilter(it.Length);
            f.filterItems = it;
            for (int i = 0; i < f.filterItems.Length; i++) {
                if (f.filterItems[i].type == ItemID.Count) {
                    f.filterItems[i] = new Item();
                    f.filterItems[i].SetDefaults(0);
                }
            }
            f.filterWhitelist = tag.Get<bool>("filterWhitelist");

            return f;
        }

        public static ItemFilter Read(BinaryReader reader, bool lightReceive) {
            int num = reader.ReadInt32();
            ItemFilter f = new ItemFilter(num);
            for(int i = 0; i < num; i++) {
                f.filterItems[i] = reader.ReadItem();
                if(f.filterItems[i].type == ItemID.Count) {
                    f.filterItems[i] = new Item();
                    f.filterItems[i].SetDefaults(0);
                }
            }
            f.filterWhitelist = reader.ReadBoolean();

            return f;
        }

        public void Write(BinaryWriter writer, bool lightSend) {
            writer.Write(filterItems.Length);
            for (int i = 0; i < filterItems.Length; i++) {
                writer.WriteItem(filterItems[i]);
            }
            writer.Write(filterWhitelist);
        }

    }
}
