using Terraria;

namespace MoreMechanisms {
    /// <summary>
    /// Utility class for filtering items
    /// </summary>
    public class ItemFilter {
        public Item[] filterItems;
        public bool filterWhitelist = true;

        public ItemFilter(int num) {
            filterItems = new Item[num];

            for (int i = 0; i < filterItems.Length; i++) {
                filterItems[i] = new Item();
                filterItems[i].SetDefaults(0);
            }
            filterWhitelist = true;

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

    }
}
