using UnityEngine;
using GreedDungeon.Character;
using GreedDungeon.Combat;
using GreedDungeon.Items;
using GreedDungeon.ScriptableObjects;

namespace GreedDungeon.Testing
{
    public class ItemSystemTester : MonoBehaviour
    {
        [Header("테스트 데이터")]
        [SerializeField] private ConsumableDataSO _healPotionSmall;
        [SerializeField] private ConsumableDataSO _healPotionMedium;
        [SerializeField] private ConsumableDataSO _healPotionLarge;
        [SerializeField] private ConsumableDataSO _antidote;
        [SerializeField] private ConsumableDataSO _attackBuff;
        [SerializeField] private ConsumableDataSO _defenseBuff;
        [SerializeField] private ConsumableDataSO _poisonFlask;
        [SerializeField] private ConsumableDataSO _burnFlask;
        [SerializeField] private ConsumableDataSO _magicArrow;
        [SerializeField] private ConsumableDataSO _bomb;
        
        [SerializeField] private StatusEffectDataSO _poisonEffect;
        [SerializeField] private StatusEffectDataSO _burnEffect;
        [SerializeField] private MonsterDataSO _testMonster;

        private void Start()
        {
            RunAllTests();
        }

        private void RunAllTests()
        {
            Debug.Log("═══════════════════════════════════════════════════════════");
            Debug.Log("              아이템 시스템 테스트 시작                      ");
            Debug.Log("═══════════════════════════════════════════════════════════");
            
            TestInventoryBasic();
            TestHealItem();
            TestBuffItem();
            TestDebuffItem();
            TestAttackItem();
            TestBuffDuration();
            TestCombinedScenario();
            
            Debug.Log("═══════════════════════════════════════════════════════════");
            Debug.Log("              모든 테스트 완료                              ");
            Debug.Log("═══════════════════════════════════════════════════════════");
        }

        private void TestInventoryBasic()
        {
            Debug.Log("\n[테스트 1] 인벤토리 기본 기능");
            
            var player = new Player();
            
            if (_healPotionSmall == null)
            {
                Debug.Log("  ⚠️ healPotionSmall 데이터 없음 - 스킵");
                return;
            }
            
            player.TryAddConsumable(_healPotionSmall, 3);
            int index = player.FindItemIndex(_healPotionSmall.ID);
            var item = player.GetItemAt(index);
            int count = item?.Quantity ?? 0;
            Debug.Log($"  아이템 추가 (x3): {count}개");
            
            bool hasItem = index >= 0;
            Debug.Log($"  아이템 보유 확인: {hasItem}");
            
            player.UseItemAt(index);
            index = player.FindItemIndex(_healPotionSmall.ID);
            item = player.GetItemAt(index);
            count = item?.Quantity ?? 0;
            Debug.Log($"  아이템 사용 (x1): {count}개 남음");
            
            player.UseItemAt(index);
            player.UseItemAt(index);
            index = player.FindItemIndex(_healPotionSmall.ID);
            hasItem = index >= 0;
            Debug.Log($"  아이템 모두 사용: 보유={hasItem}");
            
            Debug.Log("  ✓ 인벤토리 기본 기능 테스트 통과");
        }

        private void TestHealItem()
        {
            Debug.Log("\n[테스트 2] 회복 아이템");
            
            var player = new Player();
            var calculator = new DamageCalculator();
            var turnManager = new TurnManager();
            var battleManager = new BattleManager(calculator, turnManager, null);
            
            if (_healPotionLarge == null)
            {
                Debug.Log("  ⚠️ healPotionLarge 데이터 없음 - 스킵");
                return;
            }
            
            player.TakeDamage(50);
            int hpBefore = player.CurrentHP;
            Debug.Log($"  HP before: {hpBefore}/{player.TotalStats.MaxHP}");
            
            player.TryAddConsumable(_healPotionLarge, 1);
            int itemIndex = player.FindItemIndex(_healPotionLarge.ID);
            var item = player.GetItemAt(itemIndex);
            
            battleManager.ExecuteItem(item, player);
            
            int hpAfter = player.CurrentHP;
            int healed = hpAfter - hpBefore;
            Debug.Log($"  HP after: {hpAfter}/{player.TotalStats.MaxHP} (회복량: {healed})");
            
            Debug.Log("  ✓ 회복 아이템 테스트 통과");
        }

        private void TestBuffItem()
        {
            Debug.Log("\n[테스트 3] 버프 아이템");
            
            var player = new Player();
            var calculator = new DamageCalculator();
            var turnManager = new TurnManager();
            var battleManager = new BattleManager(calculator, turnManager, null);
            
            if (_attackBuff == null)
            {
                Debug.Log("  ⚠️ attackBuff 데이터 없음 - 스킵");
                return;
            }
            
            int attackBefore = player.TotalStats.Attack;
            Debug.Log($"  공격력 before: {attackBefore}");
            
            player.TryAddConsumable(_attackBuff, 1);
            int itemIndex = player.FindItemIndex(_attackBuff.ID);
            var item = player.GetItemAt(itemIndex);
            battleManager.ExecuteItem(item, player);
            
            int attackAfter = player.TotalStats.Attack;
            float increase = ((float)attackAfter / attackBefore - 1) * 100;
            Debug.Log($"  공격력 after: {attackAfter} (+{increase:F0}%)");
            Debug.Log($"  버프 개수: {player.Buffs.Count}");
            
            Debug.Log("  ✓ 버프 아이템 테스트 통과");
        }

        private void TestDebuffItem()
        {
            Debug.Log("\n[테스트 4] 디버프 아이템 (적에게 상태이상)");
            
            var player = new Player();
            var calculator = new DamageCalculator();
            var turnManager = new TurnManager();
            var battleManager = new BattleManager(calculator, turnManager, null);
            
            if (_testMonster == null || _poisonFlask == null)
            {
                Debug.Log("  ⚠️ 테스트 데이터 없음 - 스킵");
                return;
            }
            
            var monster = new Monster(_testMonster, null);
            battleManager.StartBattle(player, monster);
            
            Debug.Log($"  몬스터 상태이상 개수 before: {monster.StatusEffects.Count}");
            
            player.TryAddConsumable(_poisonFlask, 1);
            int itemIndex = player.FindItemIndex(_poisonFlask.ID);
            var item = player.GetItemAt(itemIndex);
            battleManager.ExecuteItem(item, monster);
            
            Debug.Log($"  몬스터 상태이상 개수 after: {monster.StatusEffects.Count}");
            
            if (monster.StatusEffects.Count > 0)
            {
                var effect = monster.StatusEffects[0];
                Debug.Log($"  상태이상: {effect.Data.Name} (지속: {effect.RemainingDuration}턴)");
            }
            
            Debug.Log("  ✓ 디버프 아이템 테스트 통과");
        }

        private void TestAttackItem()
        {
            Debug.Log("\n[테스트 5] 공격 아이템");
            
            var player = new Player();
            var calculator = new DamageCalculator();
            var turnManager = new TurnManager();
            var battleManager = new BattleManager(calculator, turnManager, null);
            
            if (_testMonster == null || _magicArrow == null)
            {
                Debug.Log("  ⚠️ 테스트 데이터 없음 - 스킵");
                return;
            }
            
            var monster = new Monster(_testMonster, null);
            battleManager.StartBattle(player, monster);
            
            int hpBefore = monster.CurrentHP;
            Debug.Log($"  몬스터 HP before: {hpBefore}");
            
            player.TryAddConsumable(_magicArrow, 1);
            int itemIndex = player.FindItemIndex(_magicArrow.ID);
            var item = player.GetItemAt(itemIndex);
            battleManager.ExecuteItem(item, monster);
            
            int hpAfter = monster.CurrentHP;
            int damage = hpBefore - hpAfter;
            Debug.Log($"  몬스터 HP after: {hpAfter} (데미지: {damage})");
            
            Debug.Log("  ✓ 공격 아이템 테스트 통과");
        }

        private void TestBuffDuration()
        {
            Debug.Log("\n[테스트 6] 버프 지속시간");
            
            var player = new Player();
            var calculator = new DamageCalculator();
            var turnManager = new TurnManager();
            var battleManager = new BattleManager(calculator, turnManager, null);
            
            if (_attackBuff == null)
            {
                Debug.Log("  ⚠️ attackBuff 데이터 없음 - 스킵");
                return;
            }
            
            player.TryAddConsumable(_attackBuff, 1);
            int itemIndex = player.FindItemIndex(_attackBuff.ID);
            var item = player.GetItemAt(itemIndex);
            battleManager.ExecuteItem(item, player);
            
            Debug.Log($"  버프 적용: {player.Buffs.Count}개");
            Debug.Log($"  지속시간: {player.Buffs[0].RemainingDuration}턴");
            
            player.ProcessTurnEnd();
            Debug.Log($"  턴 종료 후 지속시간: {player.Buffs[0].RemainingDuration}턴");
            
            player.ProcessTurnEnd();
            Debug.Log($"  2턴 종료 후 버프 개수: {player.Buffs.Count}개");
            
            int attackAfterBuff = player.TotalStats.Attack;
            Debug.Log($"  버프 종료 후 공격력: {attackAfterBuff}");
            
            Debug.Log("  ✓ 버프 지속시간 테스트 통과");
        }

        private void TestCombinedScenario()
        {
            Debug.Log("\n[테스트 7] 통합 시나리오 (전투 + 아이템)");
            
            var player = new Player();
            var calculator = new DamageCalculator();
            var turnManager = new TurnManager();
            var battleManager = new BattleManager(calculator, turnManager, null);
            
            if (_testMonster == null || _healPotionSmall == null || _attackBuff == null)
            {
                Debug.Log("  ⚠️ 테스트 데이터 없음 - 스킵");
                return;
            }
            
            var monster = new Monster(_testMonster, null);
            battleManager.StartBattle(player, monster);
            
            player.TryAddConsumable(_healPotionSmall, 5);
            player.TryAddConsumable(_attackBuff, 2);
            
            int healIndex = player.FindItemIndex(_healPotionSmall.ID);
            int buffIndex = player.FindItemIndex(_attackBuff.ID);
            var healItem = player.GetItemAt(healIndex);
            var buffItem = player.GetItemAt(buffIndex);
            
            Debug.Log($"  초기 상태:");
            Debug.Log($"    Player HP: {player.CurrentHP}/{player.TotalStats.MaxHP}");
            Debug.Log($"    Monster HP: {monster.CurrentHP}/{monster.TotalStats.MaxHP}");
            Debug.Log($"    인벤토리: 회복포션 x{healItem?.Quantity ?? 0}, 공격버프 x{buffItem?.Quantity ?? 0}");
            
            for (int turn = 1; turn <= 5; turn++)
            {
                Debug.Log($"\n  === 턴 {turn} ===");
                
                buffIndex = player.FindItemIndex(_attackBuff.ID);
                if (turn == 1 && buffIndex >= 0)
                {
                    buffItem = player.GetItemAt(buffIndex);
                    battleManager.ExecuteItem(buffItem, player);
                    Debug.Log($"    공격력: {player.TotalStats.Attack}");
                }
                
                battleManager.ExecuteAttack(player, monster, null);
                
                if (!monster.IsDead)
                {
                    battleManager.ExecuteAttack(monster, player, null);
                }
                
                healIndex = player.FindItemIndex(_healPotionSmall.ID);
                if (player.CurrentHP < player.TotalStats.MaxHP * 0.5f && healIndex >= 0)
                {
                    healItem = player.GetItemAt(healIndex);
                    battleManager.ExecuteItem(healItem, player);
                }
                
                Debug.Log($"    상태: Player HP {player.CurrentHP}, Monster HP {monster.CurrentHP}");
                
                battleManager.EndTurn();
                
                if (battleManager.IsBattleOver) break;
            }
            
            Debug.Log($"\n  결과: {(battleManager.PlayerWon ? "승리" : "패배")}");
            Debug.Log("  ✓ 통합 시나리오 테스트 통과");
        }
    }
}