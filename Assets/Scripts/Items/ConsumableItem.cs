using GreedDungeon.ScriptableObjects;

namespace GreedDungeon.Items
{
    public class ConsumableItem
    {
        public ConsumableDataSO Data { get; }
        public int Quantity { get; private set; }

        public ConsumableItem(ConsumableDataSO data, int quantity = 1)
        {
            Data = data;
            Quantity = quantity;
        }

        public void Add(int amount = 1)
        {
            Quantity += amount;
        }

        public bool Use()
        {
            if (Quantity <= 0) return false;
            Quantity--;
            return true;
        }
    }
}