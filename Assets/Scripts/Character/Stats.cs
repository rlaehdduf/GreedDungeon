using System;

namespace GreedDungeon.Character
{
    [Serializable]
    public class Stats
    {
        public int MaxHP;
        public int MaxMP;
        public int Attack;
        public int Defense;
        public int Speed;
        public float CriticalRate;
        public float Resistance;

        public Stats() { }

        public Stats(int maxHP, int maxMP, int attack, int defense, int speed, float criticalRate = 0.05f, float resistance = 0f)
        {
            MaxHP = maxHP;
            MaxMP = maxMP;
            Attack = attack;
            Defense = defense;
            Speed = speed;
            CriticalRate = criticalRate;
            Resistance = resistance;
        }

        public Stats Clone()
        {
            return new Stats(MaxHP, MaxMP, Attack, Defense, Speed, CriticalRate, Resistance);
        }

        public static Stats operator +(Stats a, Stats b)
        {
            return new Stats(
                a.MaxHP + b.MaxHP,
                a.MaxMP + b.MaxMP,
                a.Attack + b.Attack,
                a.Defense + b.Defense,
                a.Speed + b.Speed,
                a.CriticalRate + b.CriticalRate,
                a.Resistance + b.Resistance
            );
        }

        public void ApplyMultiplier(float multiplier)
        {
            MaxHP = (int)(MaxHP * multiplier);
            MaxMP = (int)(MaxMP * multiplier);
            Attack = (int)(Attack * multiplier);
            Defense = (int)(Defense * multiplier);
            Speed = (int)(Speed * multiplier);
        }
    }
}