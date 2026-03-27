using UnityEngine;

namespace GreedDungeon.Dungeon
{
    public class EncounterSystem
    {
        private const int BATTLE_CHANCE = 60;
        private const int TREASURE_CHANCE = 25;
        private const int SHOP_CHANCE = 15;
        
        private const int POST_BATTLE_TREASURE_CHANCE = 50;
        private const int POST_BATTLE_BATTLE_CHANCE = 30;
        private const int POST_BATTLE_SHOP_CHANCE = 20;
        
        private readonly DungeonProgress _progress;
        
        public EncounterSystem(DungeonProgress progress)
        {
            _progress = progress;
        }
        
        public EncounterType GetNextEncounter(bool afterBattle = false)
        {
            if (afterBattle)
            {
                return GetPostBattleEncounter();
            }
            
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
        
        private EncounterType GetPostBattleEncounter()
        {
            int roll = Random.Range(0, 100);
            
            if (roll < POST_BATTLE_TREASURE_CHANCE)
            {
                return EncounterType.Treasure;
            }
            else if (roll < POST_BATTLE_TREASURE_CHANCE + POST_BATTLE_BATTLE_CHANCE)
            {
                return EncounterType.Battle;
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