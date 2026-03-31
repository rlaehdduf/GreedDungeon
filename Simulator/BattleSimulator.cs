using System;
using System.Collections.Generic;
using System.Linq;

namespace GreedDungeon.Simulator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== GreedDungeon Multi-Battle Simulator ===\n");
            Console.WriteLine("Settings: Difficulty +1% per battle, HP 70% recovery\n");

            var monsters = new MonsterData[]
            {
                new MonsterData("Slime", 60, 28, 6, 5, 5, 
                    new SkillData(101, "Fission", "Heal", 0, 0, 0, 0, "", 0, 0, 15), 
                    new SkillData(5, "Regeneration", "Heal", 0, 0, 0, 0, "", 0, 0, 8), 30),
                new MonsterData("Plague Rat", 65, 22, 6, 7, 3, 
                    new SkillData(102, "Plague", "Debuff", 1.0f, 1, 2, 30), 
                    new SkillData(2, "Double Strike", "Attack", 0.4f, 2, 0, 0), 25),
                new MonsterData("Spider", 72, 20, 5, 6, 5, 
                    new SkillData(103, "Venom Sting", "Debuff", 1.0f, 1, 2, 35), 
                    new SkillData(3, "Poison Gas", "Attack", 0.7f, 1, 2, 15), 25),
                new MonsterData("Skeleton", 58, 24, 4, 10, 12, 
                    new SkillData(104, "Devastating Blow", "Attack", 1.3f, 1, 3, 50), 
                    new SkillData(4, "Rage", "Buff", 0, 0, 0, 0, "Attack", 25, 3), 30),
                new MonsterData("Cerberus", 350, 40, 18, 7, 10, 
                    new SkillData(105, "Hellfire", "Attack", 0.5f, 2, 1, 30), 
                    new SkillData(1, "Power Strike", "Attack", 1.0f, 1, 0, 0), 35)
            };

            var playerScenarios = new (string Name, int HP, int ATK, int DEF, int SPD, float CRIT, bool HasSkill, int SkillTier)[]
            {
                ("Starting Equipment + Skill", 110, 15, 12, 5, 8f, true, 1),
                ("1 Equipment", 115, 17, 13, 5, 10f, false, 0),
                ("2 Equipment", 120, 19, 14, 5, 12f, false, 0),
                ("3 Equipment", 125, 21, 15, 6, 14f, false, 0),
                ("4 Equipment", 130, 23, 16, 6, 16f, false, 0),
            };

            float difficultyRate = 0.01f;
            float healRate = 0.5f;
            int simulationCount = 1000;
            
            foreach (var player in playerScenarios)
            {
                Console.WriteLine($"\n=== {player.Name} ===");
                Console.WriteLine($"HP {player.HP}, ATK {player.ATK}, DEF {player.DEF}, SPD {player.SPD}, CRIT {player.CRIT:F0}%\n");
                
                for (int battleCount = 1; battleCount <= 10; battleCount++)
                {
                    int survived = 0;
                    for (int sim = 0; sim < simulationCount; sim++)
                    {
                        if (SimulateMultipleBattles(player, monsters, battleCount, false, difficultyRate, healRate))
                            survived++;
                    }
                    float rate = (float)survived / simulationCount * 100;
                    Console.WriteLine($"  {battleCount} Battles Survived: {rate:F1}%");
                }
                
                float avgSurvival = 0;
                for (int sim = 0; sim < simulationCount; sim++)
                {
                    avgSurvival += CountSurvivedBattles(player, monsters, 10, false, difficultyRate, healRate);
                }
                Console.WriteLine($"\nAverage Battles Survived: {avgSurvival / simulationCount:F1}");
            }

            Console.WriteLine("\nComplete!");
        }

        static int CountSurvivedBattles(
            (string Name, int HP, int ATK, int DEF, int SPD, float CRIT, bool HasSkill, int SkillTier) playerStats,
            MonsterData[] monsters,
            int maxBattles,
            bool bossAtEnd,
            float difficultyRate,
            float healRate)
        {
            int currentHP = playerStats.HP;
            Random rng = new Random();

            for (int battle = 1; battle <= maxBattles; battle++)
            {
                float difficultyMult = 1 + (battle - 1) * difficultyRate;
                
                MonsterData monster;
                if (bossAtEnd && battle >= 5)
                {
                    monster = new MonsterData(
                        "Cerberus",
                        (int)(monsters[4].HP * difficultyMult),
                        (int)(monsters[4].ATK * difficultyMult),
                        monsters[4].DEF,
                        monsters[4].SPD,
                        monsters[4].CritRate,
                        monsters[4].UniqueSkill,
                        monsters[4].SharedSkill,
                        monsters[4].SkillChance
                    );
                }
                else
                {
                    int idx = rng.Next(monsters.Length - 1);
                    monster = new MonsterData(
                        monsters[idx].Name,
                        (int)(monsters[idx].HP * difficultyMult),
                        (int)(monsters[idx].ATK * difficultyMult),
                        monsters[idx].DEF,
                        monsters[idx].SPD,
                        monsters[idx].CritRate,
                        monsters[idx].UniqueSkill,
                        monsters[idx].SharedSkill,
                        monsters[idx].SkillChance
                    );
                }

                var result = SimulateSingleBattleWithHP(playerStats, monster, rng, currentHP);
                if (!result.PlayerWon)
                    return battle - 1;
                
                currentHP = Math.Min(playerStats.HP, result.RemainingHP + (int)(playerStats.HP * healRate));
            }

            return maxBattles;
        }

        static bool SimulateMultipleBattles(
            (string Name, int HP, int ATK, int DEF, int SPD, float CRIT, bool HasSkill, int SkillTier) playerStats,
            MonsterData[] monsters,
            int battleCount,
            bool bossAtEnd,
            float difficultyRate,
            float healRate)
        {
            int currentHP = playerStats.HP;
            Random rng = new Random();

            for (int battle = 1; battle <= battleCount; battle++)
            {
                float difficultyMult = 1 + (battle - 1) * difficultyRate;
                
                MonsterData monster;
                if (bossAtEnd && battle >= 5)
                {
                    monster = new MonsterData(
                        "Cerberus",
                        (int)(monsters[4].HP * difficultyMult),
                        (int)(monsters[4].ATK * difficultyMult),
                        monsters[4].DEF,
                        monsters[4].SPD,
                        monsters[4].CritRate,
                        monsters[4].UniqueSkill,
                        monsters[4].SharedSkill,
                        monsters[4].SkillChance
                    );
                }
                else
                {
                    int idx = rng.Next(monsters.Length - 1);
                    monster = new MonsterData(
                        monsters[idx].Name,
                        (int)(monsters[idx].HP * difficultyMult),
                        (int)(monsters[idx].ATK * difficultyMult),
                        monsters[idx].DEF,
                        monsters[idx].SPD,
                        monsters[idx].CritRate,
                        monsters[idx].UniqueSkill,
                        monsters[idx].SharedSkill,
                        monsters[idx].SkillChance
                    );
                }

                var result = SimulateSingleBattleWithHP(playerStats, monster, rng, currentHP);
                if (!result.PlayerWon)
                    return false;
                
                currentHP = Math.Min(playerStats.HP, result.RemainingHP + (int)(playerStats.HP * healRate));
            }

            return true;
        }

        static (bool PlayerWon, int Turns, int RemainingHP) SimulateSingleBattleWithHP(
            (string Name, int HP, int ATK, int DEF, int SPD, float CRIT, bool HasSkill, int SkillTier) playerStats,
            MonsterData monster,
            Random rng,
            int startingHP)
        {
            int playerHP = startingHP;
            int playerMP = 40;
            int monsterHP = monster.HP;
            int monsterMaxHP = monster.MaxHP;
            int monsterATKBuff = 0;
            int monsterBuffDuration = 0;

            float playerGauge = 0;
            float monsterGauge = 0;

            const int GAUGE_THRESHOLD = 1000;
            int turns = 0;
            
            float skillMultiplier = GetSkillMultiplier(playerStats.SkillTier);
            int skillMPCost = playerStats.SkillTier switch { 1 => 5, 2 => 15, 3 => 25, _ => 0 };
            int skillCooldown = 0;

            while (playerHP > 0 && monsterHP > 0)
            {
                playerGauge += GetEffectiveSpeed(playerStats.SPD);
                monsterGauge += GetEffectiveSpeed(monster.SPD);

                while (playerGauge >= GAUGE_THRESHOLD || monsterGauge >= GAUGE_THRESHOLD)
                {
                    if (playerGauge >= GAUGE_THRESHOLD && (monsterGauge < GAUGE_THRESHOLD || playerGauge >= monsterGauge))
                    {
                        turns++;
                        playerGauge = 0;
                        
                        if (skillCooldown > 0) skillCooldown--;

                        bool useSkillThisTurn = playerStats.HasSkill && skillCooldown == 0 && playerMP >= skillMPCost;

                        int damage;
                        if (useSkillThisTurn)
                        {
                            damage = CalculateDamage(playerStats.ATK, monster.DEF, playerStats.CRIT, skillMultiplier, rng);
                            playerMP -= skillMPCost;
                            skillCooldown = playerStats.SkillTier switch { 1 => 2, 2 => 3, 3 => 5, _ => 1 };
                        }
                        else
                        {
                            damage = CalculateDamage(playerStats.ATK, monster.DEF, playerStats.CRIT, 1.0f, rng);
                        }
                        
                        monsterHP -= damage;

                        if (monsterBuffDuration > 0)
                        {
                            monsterBuffDuration--;
                            if (monsterBuffDuration == 0) monsterATKBuff = 0;
                        }

                        if (monsterHP <= 0) break;
                    }
                    else if (monsterGauge >= GAUGE_THRESHOLD)
                    {
                        monsterGauge = 0;

                        int monsterATK = monster.ATK + monsterATKBuff;
                        bool useSkill = rng.Next(100) < monster.SkillChance;
                        SkillData skill = useSkill ? (rng.Next(2) == 0 ? monster.UniqueSkill : monster.SharedSkill) : null;

                        int monsterDamage;
                        if (skill != null)
                        {
                            monsterDamage = ExecuteMonsterSkill(skill, monsterATK, playerStats.DEF, rng, ref monsterHP, monsterMaxHP, ref monsterATKBuff, ref monsterBuffDuration);
                        }
                        else
                        {
                            monsterDamage = CalculateDamage(monsterATK, playerStats.DEF, monster.CritRate, 1.0f, rng);
                        }

                        playerHP -= monsterDamage;

                        if (playerHP <= 0) break;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return (playerHP > 0, turns, Math.Max(0, playerHP));
        }

        static int ExecuteMonsterSkill(SkillData skill, int monsterATK, int playerDEF, Random rng, 
            ref int monsterHP, int monsterMaxHP, ref int monsterATKBuff, ref int monsterBuffDuration)
        {
            switch (skill.Type)
            {
                case "Attack":
                    int totalDamage = 0;
                    for (int i = 0; i < skill.HitCount; i++)
                    {
                        totalDamage += CalculateDamage(monsterATK, playerDEF, 5f, skill.DamageMultiplier, rng);
                    }
                    return totalDamage;

                case "Debuff":
                    int debuffDamage = CalculateDamage(monsterATK, playerDEF, 5f, skill.DamageMultiplier, rng);
                    if (skill.StatusEffectChance > 0 && rng.Next(100) < skill.StatusEffectChance)
                    {
                        debuffDamage = (int)(debuffDamage * 1.2f);
                    }
                    return debuffDamage;

                case "Buff":
                    if (skill.BuffType == "Attack")
                    {
                        monsterATKBuff = (int)(monsterATK * skill.BuffValue / 100f);
                        monsterBuffDuration = skill.BuffDuration;
                    }
                    return 0;

                case "Heal":
                    int healAmount = (int)(monsterMaxHP * skill.HealPercent / 100f);
                    monsterHP = Math.Min(monsterHP + healAmount, monsterMaxHP);
                    return 0;

                default:
                    return CalculateDamage(monsterATK, playerDEF, 5f, 1.0f, rng);
            }
        }

        static int CalculateDamage(int attack, int defense, float critRate, float multiplier, Random rng)
        {
            int baseDamage = (int)(attack * multiplier);
            int damageAfterDef = Math.Max(1, baseDamage - defense / 2);

            bool isCrit = rng.Next(100) < critRate;
            if (isCrit)
            {
                damageAfterDef = (int)(damageAfterDef * 1.5f);
            }

            return damageAfterDef;
        }

        static float GetSkillMultiplier(int tier) => tier switch
        {
            1 => 1.5f,
            2 => 2.0f,
            3 => 2.5f,
            _ => 1.0f
        };

        static float GetEffectiveSpeed(int speed)
        {
            return Math.Min(speed, 5) + (float)Math.Sqrt(Math.Max(0, speed - 5)) * 0.8f;
        }
    }

    class MonsterData
    {
        public string Name;
        public int HP, MaxHP, ATK, DEF, SPD, CritRate;
        public SkillData UniqueSkill, SharedSkill;
        public int SkillChance;

        public MonsterData(string name, int hp, int atk, int def, int spd, int critRate,
            SkillData uniqueSkill, SkillData sharedSkill, int skillChance)
        {
            Name = name;
            HP = hp;
            MaxHP = hp;
            ATK = atk;
            DEF = def;
            SPD = spd;
            CritRate = critRate;
            UniqueSkill = uniqueSkill;
            SharedSkill = sharedSkill;
            SkillChance = skillChance;
        }
    }

    class SkillData
    {
        public int ID;
        public string Name, Type;
        public float DamageMultiplier;
        public int HitCount;
        public int StatusEffectID;
        public int StatusEffectChance;
        public string BuffType;
        public int BuffValue, BuffDuration, HealPercent;

        public SkillData(int id, string name, string type, float damageMultiplier, int hitCount, 
            int statusEffectID, int statusEffectChance)
        {
            ID = id;
            Name = name;
            Type = type;
            DamageMultiplier = damageMultiplier;
            HitCount = hitCount;
            StatusEffectID = statusEffectID;
            StatusEffectChance = statusEffectChance;
        }

        public SkillData(int id, string name, string type, float damageMultiplier, int hitCount,
            int statusEffectID, int statusEffectChance, string buffType, int buffValue, int buffDuration)
            : this(id, name, type, damageMultiplier, hitCount, statusEffectID, statusEffectChance)
        {
            BuffType = buffType;
            BuffValue = buffValue;
            BuffDuration = buffDuration;
        }

        public SkillData(int id, string name, string type, float damageMultiplier, int hitCount,
            int statusEffectID, int statusEffectChance, string buffType, int buffValue, int buffDuration, int healPercent)
            : this(id, name, type, damageMultiplier, hitCount, statusEffectID, statusEffectChance, buffType, buffValue, buffDuration)
        {
            HealPercent = healPercent;
        }

        public SkillData(int id, string name, string type, int healPercent)
        {
            ID = id;
            Name = name;
            Type = type;
            HealPercent = healPercent;
        }
    }
}