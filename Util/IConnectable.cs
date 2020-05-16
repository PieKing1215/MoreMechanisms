using Terraria;

namespace MoreMechanisms {

    public enum ConnectableType {
        Input, Output
    }

    public interface IConnectable {

        Item[] GetItems(ConnectableType type);

        bool Accepts(Item item, ConnectableType type);

        void TransferredItem(Item transferred, int index, ConnectableType type);
    }
    
}
