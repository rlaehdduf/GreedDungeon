using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.Collections.Generic;

public class ScriptableObjectAddressablesSetter : EditorWindow
{
    private static AddressableAssetSettings settings;

    private static readonly Dictionary<string, string> SOToAddress = new Dictionary<string, string>
    {
        {"Monster_1_슬라임", "Monsters/Slime"},
        {"Monster_2_역병쥐", "Monsters/Rat"},
        {"Monster_3_거미", "Monsters/Spider"},
        {"Monster_4_해골", "Monsters/Skull"},
        {"Monster_5_켈베로스", "Monsters/Cerberus"},
        
        {"Skill_1_강타", "Skills/Smash"},
        {"Skill_2_이중타격", "Skills/DoubleSlash"},
        {"Skill_3_가로베기", "Skills/Slash"},
        {"Skill_4_십자베기", "Skills/CrossSlash"},
        {"Skill_5_마나볼", "Skills/ManaBall"},
        {"Skill_6_파이어볼", "Skills/FireBall"},
        {"Skill_7_얼음비", "Skills/IceRain"},
        {"Skill_8_공격력증가", "Skills/PowerBuff"},
        {"Skill_9_방어력증가", "Skills/DefenseBuff"},
        {"Skill_10_체력증가", "Skills/HealthBuff"},
        {"Skill_11_속도증가", "Skills/SpeedBuff"},
        {"Skill_12_공격력버프", "Skills/AttackBuff"},
        {"Skill_13_방어력버프", "Skills/DefenseSkillBuff"},
        {"Skill_14_속도버프", "Skills/SpeedSkillBuff"},
        {"Skill_15_회복", "Skills/Heal"},
        
        {"Equipment_1_막대기", "Weapons/Stick"},
        {"Equipment_2_검", "Weapons/Sword"},
        {"Equipment_3_방패", "Weapons/Shield"},
        {"Equipment_4_완드", "Weapons/Wand"},
        {"Equipment_5_도끼", "Weapons/Axe"},
        {"Equipment_101_가죽갑옷", "Armors/LeatherArmor"},
        {"Equipment_102_사슬갑옷", "Armors/ChainArmor"},
        {"Equipment_103_강철갑옷", "Armors/PlateArmor"},
        {"Equipment_104_가시갑옷", "Armors/SpikeArmor"},
        {"Equipment_105_깃털갑옷", "Armors/FeatherArmor"},
        {"Equipment_201_반지", "Accessories/Ring"},
        {"Equipment_202_목걸이", "Accessories/Necklace"},
        {"Equipment_203_망토", "Accessories/Cloak"},
        {"Equipment_204_보석", "Accessories/Jewel"},
        {"Equipment_205_왕관", "Accessories/Crown"},
        
        {"Consumable_1_회복포션(소)", "Items/Potion"},
        {"Consumable_2_회복포션(중)", "Items/PotionMedium"},
        {"Consumable_3_회복포션(대)", "Items/PotionLarge"},
        {"Consumable_4_해독제", "Items/Antidote"},
        {"Consumable_5_힘의물약", "Items/PowerPotion"},
        {"Consumable_6_철의물약", "Items/IronPotion"},
        {"Consumable_7_저주의물약", "Items/CursePotion"},
        {"Consumable_8_화염병", "Items/FlameBottle"},
        {"Consumable_9_마법화살", "Items/MagicArrow"},
        {"Consumable_10_폭발의서", "Items/ExplosionBook"},
        
        {"Rarity_1_Common", "Rarities/Common"},
        {"Rarity_2_UnCommon", "Rarities/Uncommon"},
        {"Rarity_3_Rare", "Rarities/Rare"},
        {"Rarity_4_Epic", "Rarities/Epic"},
        {"Rarity_5_Legend", "Rarities/Legend"},
        
        {"StatusEffect_1_Burn", "StatusEffects/Burn"},
        {"StatusEffect_2_Poison", "StatusEffects/Poison"},
        {"StatusEffect_3_Stun", "StatusEffects/Stun"},
    };

    private static readonly Dictionary<string, string> FolderToLabel = new Dictionary<string, string>
    {
        {"Monsters", "MonsterData"},
        {"Skills", "SkillData"},
        {"Equipments", "EquipmentData"},
        {"Consumables", "ConsumableData"},
        {"Rarities", "RarityData"},
        {"StatusEffects", "StatusEffectData"},
    };

    [MenuItem("Tools/Addressables/Set All SO Labels")]
    public static void SetAllSOLabels()
    {
        settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Asset Settings not found!");
            return;
        }

        int count = 0;
        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/ScriptableObjects/Data" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string soName = System.IO.Path.GetFileNameWithoutExtension(path);
            string folderName = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(path));

            if (SOToAddress.TryGetValue(soName, out string address))
            {
                string label = FolderToLabel.ContainsKey(folderName) ? FolderToLabel[folderName] : folderName;
                SetAddress(guid, address, label);
                count++;
            }
            else
            {
                Debug.LogWarning($"매핑 없음: {soName} (폴더: {folderName})");
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"═══ ScriptableObject 주소/라벨 설정 완료! 총 {count}개 ═══");
    }

    private static void SetAddress(string guid, string address, string label)
    {
        AddressableAssetEntry entry = settings.FindAssetEntry(guid);
        
        if (entry == null)
        {
            AddressableAssetGroup group = settings.DefaultGroup;
            entry = settings.CreateOrMoveEntry(guid, group);
        }

        entry.address = address;
        
        if (!string.IsNullOrEmpty(label))
        {
            if (!settings.GetLabels().Contains(label))
            {
                settings.AddLabel(label);
            }
            entry.SetLabel(label, true);
        }
        
        EditorUtility.SetDirty(settings);
    }

    [MenuItem("Tools/Addressables/Show All SO Addresses")]
    public static void ShowAllSOAddresses()
    {
        settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Asset Settings not found!");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/ScriptableObjects/Data" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string soName = System.IO.Path.GetFileNameWithoutExtension(path);
            AddressableAssetEntry entry = settings.FindAssetEntry(guid);

            if (entry != null)
            {
                string labels = string.Join(", ", entry.labels);
                Debug.Log($"{soName} -> 주소: {entry.address}, 라벨: [{labels}]");
            }
            else
            {
                Debug.Log($"{soName} -> (Addressable 아님)");
            }
        }
    }
}