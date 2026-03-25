using GreedDungeon.Character;

namespace GreedDungeon.Dungeon
{
    public class DungeonProgress
    {
        private readonly Player _player;
        
        public int KillCount => _player.KillCount;
        public bool IsBossDefeated { get; private set; }
        
        public DungeonProgress(Player player)
        {
            _player = player;
        }
        
        public int GetBossProbability()
        {
            if (KillCount < 10) return 0;
            return (KillCount - 9) * 10;
        }
        
        public bool RollForBoss()
        {
            int probability = GetBossProbability();
            if (probability <= 0) return false;
            if (probability >= 100) return true;
            return UnityEngine.Random.Range(0, 100) < probability;
        }
        
        public void MarkBossDefeated()
        {
            IsBossDefeated = true;
        }
    }
}