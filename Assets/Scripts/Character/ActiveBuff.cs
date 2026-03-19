using GreedDungeon.ScriptableObjects;

namespace GreedDungeon.Character
{
    public class ActiveBuff
    {
        public BuffType Type { get; }
        public float Value { get; }
        public int RemainingDuration { get; set; }

        public ActiveBuff(BuffType type, float value, int duration)
        {
            Type = type;
            Value = value;
            RemainingDuration = duration;
        }
    }
}