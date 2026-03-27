using System;
using System.Collections.Generic;
using System.Linq;

namespace GreedDungeon.Simulator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== GreedDungeon 전투 시뮬레이터 ===\n");
            Console.WriteLine("행동 게이지 시스템: EffectiveSpeed만큼 게이지 증가, 1000 도달 시 행동");
            Console.WriteLine("공식: min(Speed, 5) + sqrt(max(0, Speed-5)) * 0.8\n");

            Console.WriteLine("=== Speed 효과 테스트 ===");
            for (int spd = 5; spd <= 25; spd += 5)
            {
                float effective = GetEffectiveSpeed(spd);
                Console.WriteLine($"  Speed {spd} → 실제 {effective:F1} (vs Monster 5 비율 {effective/5:F2}:1)");
            }
            Console.WriteLine();

            // Player 시나리오 (기본 스탯 + 장비 보너스)
            // 기본: HP 110, ATK 12, DEF 12, SPD 5, CRIT 5%
            var scenarios = new (string Name, int HP, int ATK, int DEF, int SPD, float CRIT, bool HasSkill, int SkillTier)[]
            {
                ("시작 장비 (막대기)", 110, 15, 12, 5, 8f, false, 0),     // 기본 + 막대기
                ("장비 1개", 115, 17, 13, 5, 10f, false, 0),               // +HP 5, ATK 2, DEF 1
                ("장비 2개", 120, 19, 14, 5, 12f, false, 0),               // +HP 5, ATK 2, DEF 1
                ("장비 3개", 125, 21, 15, 6, 14f, false, 0),               // +HP 5, ATK 2, DEF 1, SPD 1
                ("전설 장비 1개", 170, 45, 20, 7, 25f, false, 0),
                ("전설 장비 1개 + 스킬", 170, 45, 20, 7, 25f, true, 2),
                ("전설 장비 2개", 210, 60, 24, 8, 40f, false, 0),
                ("전설 장비 2개 + 스킬", 210, 60, 24, 8, 40f, true, 3)
            };

            // 몬스터 데이터 (밸런스 조정 - 시작 장비 60% 목표)
            var monsters = new MonsterData[]
            {
                new MonsterData("슬라임", 90, 22, 6, 5, 5, 
                    new SkillData(101, "분열", "Heal", 0, 0, 0, 0, "", 0, 0, 25), 
                    new SkillData(5, "재생", "Heal", 0, 0, 0, 0, "", 0, 0, 20), 30),
                new MonsterData("역병쥐", 65, 22, 6, 7, 3, 
                    new SkillData(102, "역병", "Debuff", 1.0f, 1, 2, 30), 
                    new SkillData(2, "연속공격", "Attack", 0.4f, 2, 0, 0), 25),
                new MonsterData("거미", 72, 20, 5, 6, 5, 
                    new SkillData(103, "독침", "Debuff", 1.0f, 1, 2, 35), 
                    new SkillData(3, "독가스", "Attack", 0.7f, 1, 2, 15), 25),
                new MonsterData("해골", 58, 24, 4, 10, 12, 
                    new SkillData(104, "강력한일격", "Attack", 1.3f, 1, 3, 50), 
                    new SkillData(4, "분노", "Buff", 0, 0, 0, 0, "Attack", 25, 3), 30),
                new MonsterData("켈베로스", 350, 40, 18, 7, 10, 
                    new SkillData(105, "지옥불", "Attack", 0.5f, 2, 1, 30), 
                    new SkillData(1, "강타", "Attack", 1.0f, 1, 0, 0), 35)
            };

            // 전투 횟수 기반 난이도 시뮬레이션
            Console.WriteLine("=== 전투 횟수 기반 난이도 시스템 ===");
            Console.WriteLine("각 전투마다 몬스터 HP/ATK 5% 증가\n");

            int[] battleCounts = { 1, 3, 5, 7, 10 };
            int simulationCount = 500;

            foreach (var scenario in scenarios)
            {
                Console.WriteLine($"\n=== {scenario.Name} ===");
                Console.WriteLine($"Player: HP {scenario.HP}, ATK {scenario.ATK}, DEF {scenario.DEF}, SPD {scenario.SPD}, CRIT {scenario.CRIT:F0}%");
                if (scenario.HasSkill)
                    Console.WriteLine($"Skill: Tier {scenario.SkillTier} (데미지 배율 {GetSkillMultiplier(scenario.SkillTier):F1}x)");
                Console.WriteLine();

                foreach (int battleCount in battleCounts)
                {
                    float difficultyMult = 1 + (battleCount - 1) * 0.05f;
                    Console.WriteLine($"  [전투 {battleCount}회차] 난이도 배율: {difficultyMult:F0%}");

                    foreach (var baseMonster in monsters)
                    {
                        var monster = new MonsterData(
                            baseMonster.Name,
                            (int)(baseMonster.HP * difficultyMult),
                            (int)(baseMonster.ATK * difficultyMult),
                            baseMonster.DEF,
                            baseMonster.SPD,
                            baseMonster.CritRate,
                            baseMonster.UniqueSkill,
                            baseMonster.SharedSkill,
                            baseMonster.SkillChance
                        );

                        var result = SimulateBattle(scenario, monster, simulationCount);
                        Console.WriteLine($"    vs {monster.Name,-6}: 승률 {result.WinRate:F1}% (HP {result.AvgRemainingHP:F0} 남음)");
                    }
                }
            }

            // 켈베로스는 5회차부터 등장
            Console.WriteLine("\n=== 켈베로스 등장 조건 (전투 5회차 이상) ===");
            foreach (var scenario in scenarios.Where(s => s.HasSkill || s.ATK >= 45))
            {
                Console.WriteLine($"\n{scenario.Name}:");
                foreach (int battleCount in new[] { 5, 7, 10 })
                {
                    float difficultyMult = 1 + (battleCount - 1) * 0.05f;
                    var cerberus = new MonsterData(
                        "켈베로스",
                        (int)(350 * difficultyMult),
                        (int)(40 * difficultyMult),
                        18,
                        7,
                        10,
                        monsters[4].UniqueSkill,
                        monsters[4].SharedSkill,
                        35
                    );
                    var result = SimulateBattle(scenario, cerberus, simulationCount);
                    Console.WriteLine($"  [{battleCount}회차] 승률: {result.WinRate:F1}%");
                }
            }

            Console.WriteLine("\n=== 몬스터 스킬 상세 ===");
            foreach (var m in monsters)
            {
                Console.WriteLine($"{m.Name}: Unique={m.UniqueSkill.Name}({m.UniqueSkill.Type}), Shared={m.SharedSkill.Name}({m.SharedSkill.Type}), 발동률 {m.SkillChance}%");
            }

            Console.WriteLine("\n완료!");
        }

        static float GetSkillMultiplier(int tier) => tier switch
        {
            1 => 1.5f,
            2 => 2.5f,
            3 => 3.0f,
            _ => 1.0f
        };

        static (float WinRate, float AvgTurns, float AvgRemainingHP) SimulateBattle(
            (string Name, int HP, int ATK, int DEF, int SPD, float CRIT, bool HasSkill, int SkillTier) playerStats,
            MonsterData monster,
            int count)
        {
            int wins = 0;
            int totalTurns = 0;
            int totalRemainingHP = 0;

            Random rng = new Random();

            for (int i = 0; i < count; i++)
            {
                var result = SimulateSingleBattle(playerStats, monster, rng);
                if (result.PlayerWon)
                {
                    wins++;
                    totalRemainingHP += result.RemainingHP;
                }
                totalTurns += result.Turns;
            }

            float winRate = (float)wins / count * 100;
            float avgTurns = (float)totalTurns / count;
            float avgRemainingHP = wins > 0 ? (float)totalRemainingHP / wins : 0;

            return (winRate, avgTurns, avgRemainingHP);
        }

        static (bool PlayerWon, int Turns, int RemainingHP) SimulateSingleBattle(
            (string Name, int HP, int ATK, int DEF, int SPD, float CRIT, bool HasSkill, int SkillTier) playerStats,
            MonsterData monster,
            Random rng)
        {
            int playerHP = playerStats.HP;
            int playerMP = 40;
            int monsterHP = monster.HP;
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
                        playerGauge -= GAUGE_THRESHOLD;

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

                        if (skillCooldown > 0) skillCooldown--;

                        // 버프 지속시간 감소
                        if (monsterBuffDuration > 0)
                        {
                            monsterBuffDuration--;
                            if (monsterBuffDuration == 0) monsterATKBuff = 0;
                        }

                        if (monsterHP <= 0) break;
                    }
                    else if (monsterGauge >= GAUGE_THRESHOLD)
                    {
                        monsterGauge -= GAUGE_THRESHOLD;

                        // 몬스터 스킬 사용 결정
                        int monsterATK = monster.ATK + monsterATKBuff;
                        bool useSkill = rng.Next(100) < monster.SkillChance;
                        SkillData skill = useSkill ? (rng.Next(2) == 0 ? monster.UniqueSkill : monster.SharedSkill) : null;

                        int monsterDamage;
                        if (skill != null)
                        {
                            monsterDamage = ExecuteMonsterSkill(skill, monsterATK, playerStats.DEF, rng, ref monsterHP, ref monsterATKBuff, ref monsterBuffDuration);
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
            ref int monsterHP, ref int monsterATKBuff, ref int monsterBuffDuration)
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
                    // 상태이상은 시뮬레이션에서 간소화 (추가 데미지로 처리)
                    if (skill.StatusEffectChance > 0 && rng.Next(100) < skill.StatusEffectChance)
                    {
                        debuffDamage = (int)(debuffDamage * 1.2f); // 상태이상 시 20% 추가 데미지
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
                    monsterHP = Math.Min(monsterHP + (int)(monsterHP * skill.HealPercent / 100f), monsterHP);
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

        static float GetEffectiveSpeed(int speed)
        {
            return Math.Min(speed, 5) + (float)Math.Sqrt(Math.Max(0, speed - 5)) * 0.8f;
        }
    }

    // 데이터 클래스
    class MonsterData
    {
        public string Name;
        public int HP, ATK, DEF, SPD, CritRate;
        public SkillData UniqueSkill, SharedSkill;
        public int SkillChance;

        public MonsterData(string name, int hp, int atk, int def, int spd, int critRate,
            SkillData uniqueSkill, SkillData sharedSkill, int skillChance)
        {
            Name = name;
            HP = hp;
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