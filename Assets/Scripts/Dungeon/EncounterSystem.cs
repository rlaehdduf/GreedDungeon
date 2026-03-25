using UnityEngine;

namespace GreedDungeon.Dungeon
{
    public class EncounterSystem
    {
        private const int BATTLE_CHANCE = 60;
        private const int TREASURE_CHANCE = 25;
        private const int SHOP_CHANCE = 15;
        
        private readonly DungeonProgress _progress;
        
        public EncounterSystem(DungeonProgress progress)
        {
            _progress = progress;
        }
        
        public EncounterType GetNextEncounter()
        {
            int roll = Random.Range(0, 100);
            
            if (roll < BATTLE_CHANCE)
            {
                return EncounterType.Battle;
            }
            else if (roll < BATTLE_CHANCE + TREASURE_CHANCE)
            {
                return EncounterType.Treasure;
            }
            else
            {
                return EncounterType.Shop;
            }
        }
        
        public bool ShouldSpawnBoss()
        {
            return _progress.RollForBoss();
        }
    }
}