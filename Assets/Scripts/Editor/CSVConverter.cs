using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using GreedDungeon.ScriptableObjects;

public class CSVConverter : EditorWindow
{
    private const string CSV_PATH = "Assets/EditorData/Data";
    private const string OUTPUT_PATH = "Assets/ScriptableObjects/Data";

    [MenuItem("Tools/CSV/Convert All")]
    public static void ConvertAll()
    {
        int total = 0;
        total += ConvertStatusEffects();
        total += ConvertRarities();
        total += ConvertSkills();
        total += ConvertEquipments();
        total += ConvertMonsters();
        total += ConvertConsumables();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"CSV 변환 완료! 총 {total}개");
    }

    [MenuItem("Tools/CSV/Convert StatusEffects")]
    public static int ConvertStatusEffects()
    {
        string csvFile = Path.Combine(CSV_PATH, "StatusEffect.csv");
        if (!File.Exists(csvFile))
        {
            Debug.LogWarning($"파일 없음: {csvFile}");
            return 0;
        }

        string outputPath = Path.Combine(OUTPUT_PATH, "StatusEffects");
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        var lines = ReadCSV(csvFile);
        int count = 0;
        for (int i = 1; i < lines.Count; i++)
        {
            var values = lines[i];
            if (values.Count < 7 || !int.TryParse(values[0], out int id)) continue;

            var data = ScriptableObject.CreateInstance<StatusEffectDataSO>();
            data.ID = id;
            data.Name = values[1];
            data.DamagePerTurn = int.TryParse(values[2], out int dpt) ? dpt : 0;
            data.DamageCurrentPercent = float.TryParse(values[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float dcp) ? dcp : 0;
            data.DamageMaxPercent = float.TryParse(values[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float dmp) ? dmp : 0;
            data.Duration = int.TryParse(values[5], out int dur) ? dur : 0;
            data.SkipTurn = values[6].ToLower() == "true";

            string assetPath = Path.Combine(outputPath, $"StatusEffect_{data.ID}_{data.Name}.asset");
            AssetDatabase.CreateAsset(data, assetPath);
            count++;
        }
        
        Debug.Log($"StatusEffect 변환 완료: {count}개 (파일에서 {lines.Count}줄 읽음)");
        return count;
    }

    [MenuItem("Tools/CSV/Convert Rarities")]
    public static int ConvertRarities()
    {
        string csvFile = Path.Combine(CSV_PATH, "RarityData.csv");
        if (!File.Exists(csvFile))
        {
            Debug.LogWarning($"파일 없음: {csvFile}");
            return 0;
        }

        string outputPath = Path.Combine(OUTPUT_PATH, "Rarities");
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        var lines = ReadCSV(csvFile);
        int count = 0;
        for (int i = 1; i < lines.Count; i++)
        {
            var values = lines[i];
            if (values.Count < 7 || !int.TryParse(values[0], out int id)) continue;

            var data = ScriptableObject.CreateInstance<RarityDataSO>();
            data.ID = id;
            data.Name = values[1];
            data.StatMultiplier = float.TryParse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float sm) ? sm : 1;
            data.HasSkill = values[3].ToLower() == "true";
            data.SkillTierMin = int.TryParse(values[4], out int stmin) ? stmin : 0;
            data.SkillTierMax = int.TryParse(values[5], out int stmax) ? stmax : 0;
            data.DropWeight = int.TryParse(values[6], out int dw) ? dw : 0;

            string assetPath = Path.Combine(outputPath, $"Rarity_{data.ID}_{data.Name}.asset");
            AssetDatabase.CreateAsset(data, assetPath);
            count++;
        }
        
        Debug.Log($"Rarity 변환 완료: {count}개 (파일에서 {lines.Count}줄 읽음)");
        return count;
    }

    [MenuItem("Tools/CSV/Convert Skills")]
    public static int ConvertSkills()
    {
        string csvFile = Path.Combine(CSV_PATH, "SkillData.csv");
        if (!File.Exists(csvFile))
        {
            Debug.LogWarning($"파일 없음: {csvFile}");
            return 0;
        }

        string outputPath = Path.Combine(OUTPUT_PATH, "Skills");
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        var lines = ReadCSV(csvFile);
        int count = 0;
        for (int i = 1; i < lines.Count; i++)
        {
            var values = lines[i];
            if (values.Count < 12 || !int.TryParse(values[0], out int id))
            {
                Debug.LogWarning($"Skill 라인 {i} 건너뜀: 필드 수={values.Count}");
                continue;
            }

            var data = ScriptableObject.CreateInstance<SkillDataSO>();
            data.ID = id;
            data.Name = values[1];
            data.Type = ParseSkillType(values[2]);
            data.MPCost = int.TryParse(values[3], out int mp) ? mp : 0;
            data.Description = values[4];
            data.EffectType = ParseEffectType(values[5].Replace("Dagame", "Damage"));
            data.EffectValue = float.TryParse(values[6], NumberStyles.Float, CultureInfo.InvariantCulture, out float ev) ? ev : 0;
            data.Duration = int.TryParse(values[7], out int dur) ? dur : 0;
            data.Target = ParseTargetType(values[8]);
            data.StatusEffectID = values[9] == "None" ? "" : values[9];
            data.StatusEffectChance = float.TryParse(values[10], NumberStyles.Float, CultureInfo.InvariantCulture, out float sec) ? sec : 0;
            data.Tier = int.TryParse(values[11], out int tier) ? tier : 1;

            string assetPath = Path.Combine(outputPath, $"Skill_{data.ID}_{data.Name}.asset");
            AssetDatabase.CreateAsset(data, assetPath);
            count++;
        }
        
        Debug.Log($"Skill 변환 완료: {count}개 (파일에서 {lines.Count}줄 읽음)");
        return count;
    }

    [MenuItem("Tools/CSV/Convert Equipments")]
    public static int ConvertEquipments()
    {
        string csvFile = Path.Combine(CSV_PATH, "EquipmentData.csv");
        if (!File.Exists(csvFile))
        {
            Debug.LogWarning($"파일 없음: {csvFile}");
            return 0;
        }

        string outputPath = Path.Combine(OUTPUT_PATH, "Equipments");
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        var lines = ReadCSV(csvFile);
        int count = 0;
        for (int i = 1; i < lines.Count; i++)
        {
            var values = lines[i];
            if (values.Count < 12 || !int.TryParse(values[0], out int id)) continue;

            var data = ScriptableObject.CreateInstance<EquipmentDataSO>();
            data.ID = id;
            data.Name = values[1];
            data.Type = ParseEquipmentType(values[2]);
            data.HP = int.TryParse(values[3], out int hp) ? hp : 0;
            data.MP = int.TryParse(values[4], out int mp) ? mp : 0;
            data.Attack = int.TryParse(values[5], out int atk) ? atk : 0;
            data.Defense = int.TryParse(values[6], out int def) ? def : 0;
            data.Speed = int.TryParse(values[7], out int spd) ? spd : 0;
            data.CriticalRate = float.TryParse(values[8], NumberStyles.Float, CultureInfo.InvariantCulture, out float cr) ? cr : 0;
            data.SkillPoolType = ParseSkillPoolType(values[9]);
            data.BuyPrice = int.TryParse(values[10], out int bp) ? bp : 0;
            data.SellPrice = int.TryParse(values[11], out int sp) ? sp : 0;

            string assetPath = Path.Combine(outputPath, $"Equipment_{data.ID}_{data.Name}.asset");
            AssetDatabase.CreateAsset(data, assetPath);
            count++;
        }
        
        Debug.Log($"Equipment 변환 완료: {count}개 (파일에서 {lines.Count}줄 읽음)");
        return count;
    }

    [MenuItem("Tools/CSV/Convert Monsters")]
    public static int ConvertMonsters()
    {
        string csvFile = Path.Combine(CSV_PATH, "MonsterData.csv");
        if (!File.Exists(csvFile))
        {
            Debug.LogWarning($"파일 없음: {csvFile}");
            return 0;
        }

        string outputPath = Path.Combine(OUTPUT_PATH, "Monsters");
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        var lines = ReadCSV(csvFile);
        int count = 0;
        for (int i = 1; i < lines.Count; i++)
        {
            var values = lines[i];
            if (values.Count < 14 || !int.TryParse(values[0], out int id)) continue;

            var data = ScriptableObject.CreateInstance<MonsterDataSO>();
            data.ID = id;
            data.Name = values[1];
            data.Element = ParseElement(values[2]);
            data.MaxHP = int.TryParse(values[3], out int mhp) ? mhp : 0;
            data.Attack = int.TryParse(values[4], out int atk) ? atk : 0;
            data.Defense = int.TryParse(values[5], out int def) ? def : 0;
            data.Speed = int.TryParse(values[6], out int spd) ? spd : 0;
            data.CriticalRate = float.TryParse(values[7], NumberStyles.Float, CultureInfo.InvariantCulture, out float cr) ? cr : 0;
            data.GoldDropMin = int.TryParse(values[8], out int gmin) ? gmin : 0;
            data.GoldDropMax = int.TryParse(values[9], out int gmax) ? gmax : 0;
            data.StatusEffectID = values[10] == "None" ? "" : values[10].Replace("Brun", "Burn");
            data.StatusEffectChance = float.TryParse(values[11], NumberStyles.Float, CultureInfo.InvariantCulture, out float sec) ? sec : 0;
            data.SpecialSkill = values[12];
            data.IsBoss = values[13].ToLower().Replace("fasle", "false") == "true";

            string assetPath = Path.Combine(outputPath, $"Monster_{data.ID}_{data.Name}.asset");
            AssetDatabase.CreateAsset(data, assetPath);
            count++;
        }
        
        Debug.Log($"Monster 변환 완료: {count}개 (파일에서 {lines.Count}줄 읽음)");
        return count;
    }

    [MenuItem("Tools/CSV/Convert Consumables")]
    public static int ConvertConsumables()
    {
        string csvFile = Path.Combine(CSV_PATH, "ConsumableData.csv");
        if (!File.Exists(csvFile))
        {
            Debug.LogWarning($"파일 없음: {csvFile}");
            return 0;
        }

        string outputPath = Path.Combine(OUTPUT_PATH, "Consumables");
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        var lines = ReadCSV(csvFile);
        int count = 0;
        for (int i = 1; i < lines.Count; i++)
        {
            var values = lines[i];
            if (values.Count < 10 || !int.TryParse(values[0], out int id)) continue;

            var data = ScriptableObject.CreateInstance<ConsumableDataSO>();
            data.ID = id;
            data.Name = values[1];
            data.EffectType = ParseConsumableEffectType(values[2]);
            data.EffectValue = float.TryParse(values[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float ev) ? ev : 0;
            data.Target = ParseConsumableTarget(values[4]);
            data.StatusEffectID = values[5] == "None" ? "" : values[5];
            data.Duration = int.TryParse(values[6], out int dur) ? dur : 0;
            data.BuyPrice = int.TryParse(values[7], out int bp) ? bp : 0;
            data.SellPrice = int.TryParse(values[8], out int sp) ? sp : 0;
            data.Description = values[9];

            string assetPath = Path.Combine(outputPath, $"Consumable_{data.ID}_{data.Name}.asset");
            AssetDatabase.CreateAsset(data, assetPath);
            count++;
        }
        
        Debug.Log($"Consumable 변환 완료: {count}개 (파일에서 {lines.Count}줄 읽음)");
        return count;
    }

    private static List<List<string>> ReadCSV(string path)
    {
        var result = new List<List<string>>();
        
        byte[] bytes = File.ReadAllBytes(path);
        string text;
        
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            text = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        }
        else
        {
            text = Encoding.UTF8.GetString(bytes);
        }
        
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);
        
        for (int lineNum = 0; lineNum < lines.Length; lineNum++)
        {
            string line = lines[lineNum];
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            var values = new List<string>();
            var fields = line.Split(',');
            
            foreach (var field in fields)
            {
                values.Add(field.Trim());
            }
            
            if (values.Count > 0 && !string.IsNullOrEmpty(values[0]))
                result.Add(values);
        }
        
        return result;
    }

    private static Element ParseElement(string value)
    {
        return value switch
        {
            "불" => Element.Fire,
            "물" => Element.Water,
            "풀" => Element.Grass,
            _ => Element.None
        };
    }

    private static SkillType ParseSkillType(string value)
    {
        return value switch
        {
            "Common" => SkillType.Common,
            "Melee" => SkillType.Melee,
            "Magic" => SkillType.Magic,
            "Passive" => SkillType.Passive,
            "Buff" => SkillType.Buff,
            _ => SkillType.Common
        };
    }

    private static EffectType ParseEffectType(string value)
    {
        return value switch
        {
            "Damage" => EffectType.Damage,
            "Passive" => EffectType.Passive,
            "Buff" => EffectType.Buff,
            _ => EffectType.Damage
        };
    }

    private static TargetType ParseTargetType(string value)
    {
        return value switch
        {
            "Single" => TargetType.Single,
            "All" => TargetType.All,
            "Player" => TargetType.Player,
            _ => TargetType.Single
        };
    }

    private static EquipmentType ParseEquipmentType(string value)
    {
        return value switch
        {
            "Weapon" => EquipmentType.Weapon,
            "Armor" => EquipmentType.Armor,
            "accessories" => EquipmentType.Accessory,
            "Accessory" => EquipmentType.Accessory,
            _ => EquipmentType.Weapon
        };
    }

private static SkillPoolType ParseSkillPoolType(string value)
        {
            return value switch
            {
                "Common" => SkillPoolType.Common,
                "Melee" => SkillPoolType.Melee,
                "Magic" => SkillPoolType.Magic,
                "Passive" => SkillPoolType.Passive,
                "Random" => SkillPoolType.Random,
                _ => SkillPoolType.Common
            };
        }

        private static ConsumableEffectType ParseConsumableEffectType(string value)
        {
            return value switch
            {
                "Heal" => ConsumableEffectType.Heal,
                "Cleanse" => ConsumableEffectType.Cleanse,
                "Buff" => ConsumableEffectType.Buff,
                "Poison" => ConsumableEffectType.Poison,
                "Burn" => ConsumableEffectType.Burn,
                "Attack" => ConsumableEffectType.Attack,
                _ => ConsumableEffectType.Heal
            };
        }

        private static ConsumableTarget ParseConsumableTarget(string value)
        {
            return value switch
            {
                "Player" => ConsumableTarget.Player,
                "Single" => ConsumableTarget.Single,
                "All" => ConsumableTarget.All,
                _ => ConsumableTarget.Player
            };
        }
}