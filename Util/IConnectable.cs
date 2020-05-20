using Terraria;
using Terraria.DataStructures;

namespace MoreMechanisms {

    public enum ConnectableType {
        Input, Output
    }

    public interface IConnectable {

        Item[] GetItems(ConnectableType type);

        bool Accepts(Item item, ConnectableType type);

        void TransferredItem(Item transferred, int index, ConnectableType type);
    }

    public class Connectable {
        /// <summary>
        /// Gets an IConnectable for the tile at the given coordinates. Returns null if this tile cannot be connected to.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>An IConnectable for this tile, or null if the tile cannot be connected to.</returns>
        public static IConnectable Find(int x, int y) {
            Tile tile = Framing.GetTileSafely(x, y);
            if (!tile.active()) return null;

            TileEntity te;
            bool ent = TileEntity.ByPosition.TryGetValue(new Point16(x, y), out te);
            if (ent && te != null) {
                if (te is IConnectable) return te as IConnectable;
            }

            if (Main.tileContainer[tile.type]) {
                int chest = -1;
                if ((chest = Chest.FindChest(x, y)) != -1) {
                    return new ChestConnectable(chest);
                }
            }

            return null;
        }
    }

    public class ChestConnectable : IConnectable {

        Chest chest;
        public ChestConnectable(int id) {
            this.chest = Main.chest[id];
        }

        public bool Accepts(Item item, ConnectableType type) {
            return true;
        }

        public Item[] GetItems(ConnectableType type) {
            return chest.item;
        }

        public void TransferredItem(Item transferred, int index, ConnectableType type) {
            
        }
    }

}
