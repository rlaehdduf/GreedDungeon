using System.Collections.Generic;
using UnityEngine;
using GreedDungeon.Character;
using GreedDungeon.Combat;
using GreedDungeon.Items;
using GreedDungeon.ScriptableObjects;

public class BattleSceneTester : MonoBehaviour
{
    [Header("테스트 설정")]
    [SerializeField] private MonsterDataSO _testMonster;
    [SerializeField] private StatusEffectDataSO _burnEffect;
    [SerializeField] private StatusEffectDataSO _poisonEffect;
    [SerializeField] private StatusEffectDataSO _stunEffect;
    [SerializeField] private ConsumableDataSO _healPotionSmall;
    [SerializeField] private ConsumableDataSO _healPotionLarge;
    [SerializeField] private ConsumableDataSO _attackBuffPotion;
    [SerializeField] private bool _autoPlay = true;
    [SerializeField] private float _turnDelay = 1f;

    private Player _player;
    private Monster _monster;
    private BattleManager _battleManager;
    private TurnManager _turnManager;
    private DamageCalculator _damageCalculator;
    private float _timer;
    private bool _battleStarted;

    private void Start()
    {
        InitializeBattle();
    }

private void InitializeBattle()
        {
            Debug.Log("═══════════════════════════════════════════════════════════");
            Debug.Log("                    전투 테스트 시작                        ");
            Debug.Log("═══════════════════════════════════════════════════════════");

            _damageCalculator = new DamageCalculator();
            _turnManager = new TurnManager();
            _battleManager = new BattleManager(_damageCalculator, _turnManager, null);

        _player = new Player();
        
        if (_healPotionSmall != null) _player.AddItem(_healPotionSmall, 3);
        if (_healPotionLarge != null) _player.AddItem(_healPotionLarge, 1);
        if (_attackBuffPotion != null) _player.AddItem(_attackBuffPotion, 2);
        
        LogPlayerStats();

        if (_testMonster == null)
        {
            Debug.LogError("[오류] 테스트할 몬스터가 할당되지 않았습니다!");
            return;
        }

        StatusEffectDataSO effectData = GetStatusEffectForMonster(_testMonster);
        _monster = new Monster(_testMonster, effectData);
        LogMonsterStats();

        _battleManager.StartBattle(_player, _monster);
        _battleStarted = true;

        LogTurnOrder();
        LogSeparator();
    }

    private void Update()
    {
        if (!_battleStarted || _battleManager.IsBattleOver) return;

        if (_autoPlay)
        {
            _timer += Time.deltaTime;
            if (_timer >= _turnDelay)
            {
                _timer = 0f;
                ExecuteTurn();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            ExecuteTurn();
        }
    }

    private void ExecuteTurn()
    {
        if (_battleManager.IsBattleOver) return;

        var current = _turnManager.CurrentEntity;
        if (current == null) return;

        LogTurnStart(current);

        if (current == _player)
        {
            ExecutePlayerTurn();
        }
        else
        {
            ExecuteMonsterTurn();
        }

        if (!_battleManager.IsBattleOver)
        {
            _battleManager.EndTurn();
            LogEntityStatus(_player, "플레이어");
            LogEntityStatus(_monster, _monster.Name);
            LogSeparator();
        }
    }

    private void ExecutePlayerTurn()
    {
        bool lowHp = _player.CurrentHP < _player.TotalStats.MaxHP * 0.4f;
        bool hasHealPotion = _player.GetItemCount(1) > 0 || _player.GetItemCount(3) > 0;

        if (lowHp && hasHealPotion && Random.value < 0.6f)
        {
            var item = _player.GetItem(1) ?? _player.GetItem(3);
            if (item != null)
            {
                _battleManager.ExecuteItem(item, _player);
                return;
            }
        }

        if (Random.value < 0.15f && _player.Buffs.Count == 0 && _attackBuffPotion != null)
        {
            var item = _player.GetItem(_attackBuffPotion.ID);
            if (item != null)
            {
                _battleManager.ExecuteItem(item, _player);
                return;
            }
        }

        if (Random.value < 0.2f && _player.CurrentHP < _player.TotalStats.MaxHP * 0.5f)
        {
            _battleManager.ExecuteDefend(_player);
            return;
        }

        var skills = _player.Skills;
        SkillDataSO selectedSkill = null;

        if (skills.Count > 0)
        {
            int index = Random.Range(0, skills.Count);
            selectedSkill = skills[index];
            Debug.Log($"[플레이어] 스킬 선택: {selectedSkill.Name} (MP {selectedSkill.MPCost} 소모)");
        }
        else
        {
            Debug.Log("[플레이어] 기본 공격!");
        }

        _battleManager.ExecuteAttack(_player, _monster, selectedSkill);
    }

    private void ExecuteMonsterTurn()
    {
        Debug.Log($"[{_monster.Name}] 공격!");
        _battleManager.ExecuteAttack(_monster, _player, null);
    }

    private void LogPlayerStats()
    {
        var stats = _player.TotalStats;
        Debug.Log("┌─────────────────────────────────────────────────────────┐");
        Debug.Log("│ [플레이어 스탯]                                         │");
        Debug.Log($"│ HP: {stats.MaxHP} | MP: {stats.MaxMP} | ATK: {stats.Attack} | DEF: {stats.Defense} │");
        Debug.Log($"│ SPD: {stats.Speed} | CRIT: {stats.CriticalRate}% | RES: {stats.Resistance}% │");
        Debug.Log("└─────────────────────────────────────────────────────────┘");
    }

    private void LogMonsterStats()
    {
        var stats = _monster.BaseStats;
        Debug.Log("┌─────────────────────────────────────────────────────────┐");
        Debug.Log($"│ [{_monster.Name} 스탯]                                      │");
        Debug.Log($"│ HP: {stats.MaxHP} | ATK: {stats.Attack} | DEF: {stats.Defense} │");
        Debug.Log($"│ SPD: {stats.Speed} | CRIT: {stats.CriticalRate}% | 속성: {_monster.Element} │");
        Debug.Log($"│ 보스: {_monster.IsBoss} | 골드: {_monster.Data.GoldDropMin}~{_monster.Data.GoldDropMax}G │");
        if (_monster.HasStatusEffectAttack)
        {
            Debug.Log($"│ 상태이상 공격: {_monster.Data.StatusEffectID} ({_monster.Data.StatusEffectChance}%) │");
        }
        Debug.Log("└─────────────────────────────────────────────────────────┘");
    }

    private void LogTurnOrder()
    {
        Debug.Log("┌─────────────────────────────────────────────────────────┐");
        Debug.Log($"│ [턴 순서] 턴 {_turnManager.CurrentTurnNumber}                                        │");
        for (int i = 0; i < _turnManager.TurnOrder.Count; i++)
        {
            var entity = _turnManager.TurnOrder[i];
            string current = entity == _turnManager.CurrentEntity ? " ◄ 현재" : "";
            Debug.Log($"│ {i + 1}. {entity.Name} (SPD: {entity.TotalStats.Speed}){current} │");
        }
        Debug.Log("└─────────────────────────────────────────────────────────┘");
    }

    private void LogTurnStart(IBattleEntity entity)
    {
        Debug.Log($"───────────────── 턴 {_turnManager.CurrentTurnNumber}: {entity.Name} ─────────────────");
    }

    private void LogEntityStatus(IBattleEntity entity, string name)
    {
        string defenseStatus = entity.IsDefending ? " [방어중]" : "";
        Debug.Log($"[{name}] HP: {entity.CurrentHP}/{entity.TotalStats.MaxHP} | MP: {entity.CurrentMP}/{entity.TotalStats.MaxMP}{defenseStatus}");
        
        if (entity.Buffs.Count > 0)
        {
            var buffs = new List<string>();
            foreach (var buff in entity.Buffs)
            {
                buffs.Add($"{buff.Type}(+{buff.Value}%, {buff.RemainingDuration}턴)");
            }
            Debug.Log($"  버프: {string.Join(", ", buffs)}");
        }
        
        if (entity.StatusEffects.Count > 0)
        {
            var effects = new List<string>();
            foreach (var effect in entity.StatusEffects)
            {
                effects.Add($"{effect.Data.Name}({effect.RemainingDuration}턴)");
            }
            Debug.Log($"  상태이상: {string.Join(", ", effects)}");
        }
    }

    private void LogSeparator()
    {
        Debug.Log("───────────────────────────────────────────────────────────");
    }

    private StatusEffectDataSO GetStatusEffectForMonster(MonsterDataSO data)
    {
        if (string.IsNullOrEmpty(data.StatusEffectID)) return null;
        
        return data.StatusEffectID switch
        {
            "Burn" => _burnEffect,
            "Poison" => _poisonEffect,
            "Stun" => _stunEffect,
            _ => null
        };
    }
}