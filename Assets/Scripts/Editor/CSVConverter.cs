using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;
using GreedDungeon.ScriptableObjects;

public class CSVConverter : EditorWindow
{
    private const string CSV_PATH = "Assets/Resources/Data";
    private const string OUTPUT_PATH = "Assets/ScriptableObjects/Data";

    [MenuItem("Tools/CSV/Convert All")]
    public static void ConvertAll()
    {
        ConvertStatusEffects();
        ConvertRarities();
        ConvertSkills();
        ConvertEquipments();
        ConvertMonsters();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("CSV 변환 완료!");
    }

    [MenuItem("Tools/CSV/Convert StatusEffects")]
    public static void ConvertStatusEffects()
    {
        string csvFile = Path.Combine(CSV_PATH, "StatusEffect.csv");
        if (!File.Exists(csvFile))
        {
            Debug.LogWarning($"파일 없음: {csvFile}");
            return;
        }

        string outputPath = Path.Combine(OUTPUT_PATH, "StatusEffects");
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        var lines = ReadCSV(csvFile);
        for (int i = 1; i < lines.Count; i++)
        {
            var values = lines[i];
            if (values.Count < 7) continue;

            var data = ScriptableObject.CreateInstance<StatusEffectDataSO>();
            data.ID = int.Parse(values[0]);
            data.Name = values[1];
            data.DamagePerTurn = int.Parse(values[2]);
            data.DamageCurrentPercent = float.Parse(values[3]);
            data.DamageMaxPercent = float.Parse(values[4]);
            data.Duration = int.Parse(values[5]);
            data.SkipTurn = values[6].ToLower() == "true";

            string assetPath = Path.Combine(outputPath, $"StatusEffect_{data.ID}_{data.Name}.asset");
            AssetDatabase.CreateAsset(data, assetPath);
        }
        
        Debug.Log($"StatusEffect 변환 완료: {lines.Count - 1}개");
    }

    [MenuItem("Tools/CSV/Convert Rarities")]
    public static void ConvertRarities()
    {
        string csvFile = Path.Combine(CSV_PATH, "RarityData.csv");
        if (!File.Exists(csvFile))
        {
            Debug.LogWarning($"파일 없음: {csvFile}");
            return;
        }

        string outputPath = Path.Combine(OUTPUT_PATH, "Rarities");
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        var lines = ReadCSV(csvFile);
        for (int i = 1; i < lines.Count; i++)
        {
            var values = lines[i];
            if (values.Count < 7) continue;

            var data = ScriptableObject.CreateInstance<RarityDataSO>();
            data.ID = int.Parse(values[0]);
            data.Name = values[1];
            data.StatMultiplier = float.Parse(values[2]);
            data.HasSkill = values[3].ToLower() == "true";
            data.SkillTierMin = int.Parse(values[4]);
            data.SkillTierMax = int.Parse(values[5]);
            data.DropWeight = int.Parse(values[6]);

            string assetPath = Path.Combine(outputPath, $"Rarity_{data.ID}_{data.Name}.asset");
            AssetDatabase.CreateAsset(data, assetPath);
        }
        
        Debug.Log($"Rarity 변환 완료: {lines.Count - 1}개");
    }

    [MenuItem("Tools/CSV/Convert Skills")]
    public static void ConvertSkills()
    {
        string csvFile = Path.Combine(CSV_PATH, "SkillData.csv");
        if (!File.Exists(csvFile))
        {
            Debug.LogWarning($"파일 없음: {csvFile}");
            return;
        }

        string outputPath = Path.Combine(OUTPUT_PATH, "Skills");
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        var lines = ReadCSV(csvFile);
        for (int i = 1; i < lines.Count; i++)
        {
            var values = lines[i];
            if (values.Count < 12) continue;

            var data = ScriptableObject.CreateInstance<SkillDataSO>();
            data.ID = int.Parse(values[0]);
            data.Name = values[1];
            data.Type = ParseSkillType(values[2]);
            data.MPCost = int.Parse(values[3]);
            data.Description = values[4];
            data.EffectType = ParseEffectType(values[5].Replace("Dagame", "Damage"));
            data.EffectValue = float.Parse(values[6]);
            data.Duration = int.Parse(values[7]);
            data.Target = ParseTargetType(values[8]);
            data.StatusEffectID = values[9] == "None" ? "" : values[9];
            data.StatusEffectChance = float.Parse(values[10]);
            data.Tier = int.Parse(values[11]);

            string assetPath = Path.Combine(outputPath, $"Skill_{data.ID}_{data.Name}.asset");
            AssetDatabase.CreateAsset(data, assetPath);
        }
        
        Debug.Log($"Skill 변환 완료: {lines.Count - 1}개");
    }

    [MenuItem("Tools/CSV/Convert Equipments")]
    public static void ConvertEquipments()
    {
        string csvFile = Path.Combine(CSV_PATH, "EquipmentData.csv");
        if (!File.Exists(csvFile))
        {
            Debug.LogWarning($"파일 없음: {csvFile}");
            return;
        }

        string outputPath = Path.Combine(OUTPUT_PATH, "Equipments");
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        var lines = ReadCSV(csvFile);
        for (int i = 1; i < lines.Count; i++)
        {
            var values = lines[i];
            if (values.Count < 12) continue;

            var data = ScriptableObject.CreateInstance<EquipmentDataSO>();
            data.ID = int.Parse(values[0]);
            data.Name = values[1];
            data.Type = ParseEquipmentType(values[2]);
            data.HP = int.Parse(values[3]);
            data.MP = int.Parse(values[4]);
            data.Attack = int.Parse(values[5]);
            data.Defense = int.Parse(values[6]);
            data.Speed = int.Parse(values[7]);
            data.CriticalRate = float.Parse(values[8]);
            data.SkillPoolType = ParseSkillPoolType(values[9]);
            data.BuyPrice = int.Parse(values[10]);
            data.SellPrice = int.Parse(values[11]);

            string assetPath = Path.Combine(outputPath, $"Equipment_{data.ID}_{data.Name}.asset");
            AssetDatabase.CreateAsset(data, assetPath);
        }
        
        Debug.Log($"Equipment 변환 완료: {lines.Count - 1}개");
    }

    [MenuItem("Tools/CSV/Convert Monsters")]
    public static void ConvertMonsters()
    {
        string csvFile = Path.Combine(CSV_PATH, "MonsterData.csv");
        if (!File.Exists(csvFile))
        {
            Debug.LogWarning($"파일 없음: {csvFile}");
            return;
        }

        string outputPath = Path.Combine(OUTPUT_PATH, "Monsters");
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        var lines = ReadCSV(csvFile);
        for (int i = 1; i < lines.Count; i++)
        {
            var values = lines[i];
            if (values.Count < 14) continue;

            var data = ScriptableObject.CreateInstance<MonsterDataSO>();
            data.ID = int.Parse(values[0]);
            data.Name = values[1];
            data.Element = ParseElement(values[2]);
            data.MaxHP = int.Parse(values[3]);
            data.Attack = int.Parse(values[4]);
            data.Defense = int.Parse(values[5]);
            data.Speed = int.Parse(values[6]);
            data.CriticalRate = float.Parse(values[7]);
            data.GoldDropMin = int.Parse(values[8]);
            data.GoldDropMax = int.Parse(values[9]);
            data.StatusEffectID = values[10] == "None" ? "" : values[10].Replace("Brun", "Burn");
            data.StatusEffectChance = float.Parse(values[11]);
            data.SpecialSkill = values[12];
            data.IsBoss = values[13].ToLower().Replace("fasle", "false") == "true";

            string assetPath = Path.Combine(outputPath, $"Monster_{data.ID}_{data.Name}.asset");
            AssetDatabase.CreateAsset(data, assetPath);
        }
        
        Debug.Log($"Monster 변환 완료: {lines.Count - 1}개");
    }

    private static List<List<string>> ReadCSV(string path)
    {
        var result = new List<List<string>>();
        
        using (var reader = new StreamReader(path, Encoding.UTF8))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var values = new List<string>();
                var fields = line.Split(',');
                
                foreach (var field in fields)
                {
                    string value = field.Trim();
                    if (value.StartsWith("﻿"))
                        value = value.Substring(1);
                    values.Add(value);
                }
                
                result.Add(values);
            }
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
}