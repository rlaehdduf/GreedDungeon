using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using GreedDungeon.ScriptableObjects;

public class CSVConverter : EditorWindow
{
    private const string CSV_PATH = "Assets/EditorData/Data/csv";
    private const string OUTPUT_PATH = "Assets/ScriptableObjects/Data";

    private static T FindExistingAsset<T>(int id, string folder) where T : UnityEngine.Object
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null && GetAssetID(asset) == id)
                return asset;
        }
        return null;
    }

    private static int GetAssetID(UnityEngine.Object asset)
    {
        return asset switch
        {
            StatusEffectDataSO s => s.ID,
            RarityDataSO r => r.ID,
            SkillDataSO sk => sk.ID,
            EquipmentDataSO e => e.ID,
            MonsterDataSO m => m.ID,
            ConsumableDataSO c => c.ID,
            _ => -1
        };
    }

    private static string GetAssetFileName<T>(int id, string name)
    {
        string prefix = typeof(T).Name.Replace("DataSO", "");
        return $"{prefix}_{id}_{name}.asset";
    }

    private static void RenameAssetIfNeeded<T>(T asset, int id, string newName, string folder) where T : UnityEngine.Object
    {
        string currentPath = AssetDatabase.GetAssetPath(asset);
        string expectedName = GetAssetFileName<T>(id, newName);
        string expectedPath = Path.Combine(folder, expectedName);
        
        if (currentPath != expectedPath)
        {
            string result = AssetDatabase.RenameAsset(currentPath, expectedName);
            if (string.IsNullOrEmpty(result))
            {
                Debug.Log($"에셋 이름 변경: {System.IO.Path.GetFileName(currentPath)} → {expectedName}");
            }
            else
            {
                Debug.LogWarning($"이름 변경 실패: {result}");
            }
        }
    }

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
        
        Debug.Log($"═══ CSV 변환 완료! 총 {total}개 ═══");
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
            if (values.Count < 8 || !int.TryParse(values[0], out int id)) continue;

            var data = FindExistingAsset<StatusEffectDataSO>(id, outputPath);
            bool isNew = data == null;
            
            if (isNew)
            {
                data = ScriptableObject.CreateInstance<StatusEffectDataSO>();
            }
            else
            {
                RenameAssetIfNeeded(data, id, values[1], outputPath);
            }

            data.ID = id;
            data.Name = values[1];
            data.DamagePerTurn = int.TryParse(values[2], out int dpt) ? dpt : 0;
            data.DamageCurrentPercent = float.TryParse(values[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float dcp) ? dcp : 0;
            data.DamageMaxPercent = float.TryParse(values[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float dmp) ? dmp : 0;
            data.Duration = int.TryParse(values[5], out int dur) ? dur : 0;
            data.SkipTurn = values[6].ToLower() == "true";
            data.IconAddress = values.Count > 7 ? values[7] : "";

            if (isNew)
            {
                string assetPath = Path.Combine(outputPath, GetAssetFileName<StatusEffectDataSO>(id, data.Name));
                AssetDatabase.CreateAsset(data, assetPath);
            }
            
            EditorUtility.SetDirty(data);
            count++;
        }
        
        Debug.Log($"StatusEffect 변환 완료: {count}개");
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
            if (values.Count < 8 || !int.TryParse(values[0], out int id)) continue;

            var data = FindExistingAsset<RarityDataSO>(id, outputPath);
            bool isNew = data == null;
            
            if (isNew)
            {
                data = ScriptableObject.CreateInstance<RarityDataSO>();
            }
            else
            {
                RenameAssetIfNeeded(data, id, values[1], outputPath);
            }

            data.ID = id;
            data.Name = values[1];
            data.StatMultiplier = float.TryParse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float sm) ? sm : 1;
            data.HasSkill = values[3].ToLower() == "true";
            data.SkillTierMin = int.TryParse(values[4], out int stmin) ? stmin : 0;
            data.SkillTierMax = int.TryParse(values[5], out int stmax) ? stmax : 0;
            data.DropWeight = int.TryParse(values[6], out int dw) ? dw : 0;
            data.Color = ParseColorHex(values[7]);

            if (isNew)
            {
                string assetPath = Path.Combine(outputPath, GetAssetFileName<RarityDataSO>(id, data.Name));
                AssetDatabase.CreateAsset(data, assetPath);
            }
            
            EditorUtility.SetDirty(data);
            count++;
        }
        
        Debug.Log($"Rarity 변환 완료: {count}개");
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
            if (values.Count < 16 || !int.TryParse(values[0], out int id))
            {
                Debug.LogWarning($"Skill 라인 {i} 건너뜀: 필드 수={values.Count}");
                continue;
            }

            var data = FindExistingAsset<SkillDataSO>(id, outputPath);
            bool isNew = data == null;
            
            if (isNew)
            {
                data = ScriptableObject.CreateInstance<SkillDataSO>();
            }
            else
            {
                RenameAssetIfNeeded(data, id, values[1], outputPath);
            }

            data.ID = id;
            data.Name = values[1];
            data.Type = ParseSkillType(values[2]);
            data.MPCost = int.TryParse(values[3], out int mp) ? mp : 0;
            data.Description = values[4];
            data.EffectType = ParseEffectType(values[5].Replace("Dagame", "Damage"));
            data.EffectValue = float.TryParse(values[6], NumberStyles.Float, CultureInfo.InvariantCulture, out float ev) ? ev : 0;
            data.ValueFloat = values[7];
            data.HitCount = int.TryParse(values[8], out int hc) ? hc : 1;
            data.Duration = int.TryParse(values[9], out int dur) ? dur : 0;
            data.Target = ParseTargetType(values[10]);
            data.StatusEffectID = values[11] == "None" ? "" : values[11];
            data.StatusEffectChance = float.TryParse(values[12], NumberStyles.Float, CultureInfo.InvariantCulture, out float sec) ? sec : 0;
            data.Cooldown = int.TryParse(values[13], out int cd) ? cd : 0;
            data.Tier = int.TryParse(values[14], out int tier) ? tier : 1;
            data.IconAddress = values[15];

            if (isNew)
            {
                string assetPath = Path.Combine(outputPath, GetAssetFileName<SkillDataSO>(id, data.Name));
                AssetDatabase.CreateAsset(data, assetPath);
            }
            
            EditorUtility.SetDirty(data);
            count++;
        }
        
        Debug.Log($"Skill 변환 완료: {count}개");
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
            if (values.Count < 14 || !int.TryParse(values[0], out int id)) continue;

            var data = FindExistingAsset<EquipmentDataSO>(id, outputPath);
            bool isNew = data == null;
            
            if (isNew)
            {
                data = ScriptableObject.CreateInstance<EquipmentDataSO>();
            }
            else
            {
                RenameAssetIfNeeded(data, id, values[1], outputPath);
            }

            data.ID = id;
            data.Name = values[1];
            data.Type = ParseEquipmentType(values[2]);
            data.Description = values[3];
            data.HP = int.TryParse(values[4], out int hp) ? hp : 0;
            data.MP = int.TryParse(values[5], out int mp) ? mp : 0;
            data.Attack = int.TryParse(values[6], out int atk) ? atk : 0;
            data.Defense = int.TryParse(values[7], out int def) ? def : 0;
            data.Speed = int.TryParse(values[8], out int spd) ? spd : 0;
            data.CriticalRate = float.TryParse(values[9], NumberStyles.Float, CultureInfo.InvariantCulture, out float cr) ? cr : 0;
            data.SkillPoolType = ParseSkillPoolType(values[10]);
            data.BuyPrice = int.TryParse(values[11], out int bp) ? bp : 0;
            data.SellPrice = int.TryParse(values[12], out int sp) ? sp : 0;
            data.IconAddress = values[13];

            if (isNew)
            {
                string assetPath = Path.Combine(outputPath, GetAssetFileName<EquipmentDataSO>(id, data.Name));
                AssetDatabase.CreateAsset(data, assetPath);
            }
            
            EditorUtility.SetDirty(data);
            count++;
        }
        
        Debug.Log($"Equipment 변환 완료: {count}개");
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
            if (values.Count < 17 || !int.TryParse(values[0], out int id)) continue;

            var data = FindExistingAsset<MonsterDataSO>(id, outputPath);
            bool isNew = data == null;
            
            if (isNew)
            {
                data = ScriptableObject.CreateInstance<MonsterDataSO>();
            }
            else
            {
                RenameAssetIfNeeded(data, id, values[1], outputPath);
            }

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
            data.PrefabAddress = values[14];
            data.ScaleX = float.TryParse(values[15], NumberStyles.Float, CultureInfo.InvariantCulture, out float sx) ? sx : 1f;
            data.ScaleY = float.TryParse(values[16], NumberStyles.Float, CultureInfo.InvariantCulture, out float sy) ? sy : 1f;

            if (isNew)
            {
                string assetPath = Path.Combine(outputPath, GetAssetFileName<MonsterDataSO>(id, data.Name));
                AssetDatabase.CreateAsset(data, assetPath);
            }
            
            EditorUtility.SetDirty(data);
            count++;
        }
        
        Debug.Log($"Monster 변환 완료: {count}개");
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
            if (values.Count < 11 || !int.TryParse(values[0], out int id)) continue;

            var data = FindExistingAsset<ConsumableDataSO>(id, outputPath);
            bool isNew = data == null;
            
            if (isNew)
            {
                data = ScriptableObject.CreateInstance<ConsumableDataSO>();
            }
            else
            {
                RenameAssetIfNeeded(data, id, values[1], outputPath);
            }

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
            data.BuffType = ParseBuffType(values[5], values[1]);
            data.IconAddress = values[10];

            if (isNew)
            {
                string assetPath = Path.Combine(outputPath, GetAssetFileName<ConsumableDataSO>(id, data.Name));
                AssetDatabase.CreateAsset(data, assetPath);
            }
            
            EditorUtility.SetDirty(data);
            count++;
        }
        
        Debug.Log($"Consumable 변환 완료: {count}개");
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
            "Neutral" => SkillType.Neutral,
            "Melee" => SkillType.Melee,
            "Magic" => SkillType.Magic,
            "Passive" => SkillType.Passive,
            "Buff" => SkillType.Buff,
            _ => SkillType.Neutral
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
                "Neutral" => SkillPoolType.Neutral,
                "Melee" => SkillPoolType.Melee,
                "Magic" => SkillPoolType.Magic,
                "Passive" => SkillPoolType.Passive,
                "Buff" => SkillPoolType.Buff,
                "Random" => SkillPoolType.Random,
                _ => SkillPoolType.Neutral
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

        private static BuffType ParseBuffType(string statusEffectId, string name)
        {
            if (name.Contains("힘의")) return BuffType.Attack;
            if (name.Contains("철의")) return BuffType.Defense;
            return BuffType.None;
        }

        private static Color ParseColorHex(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return Color.white;
            
            if (ColorUtility.TryParseHtmlString("#" + hex, out Color color))
                return color;
            
            return Color.white;
        }
}