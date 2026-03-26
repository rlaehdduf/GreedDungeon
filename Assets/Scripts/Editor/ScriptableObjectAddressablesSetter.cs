using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.Build.Pipeline;
using System.Collections.Generic;

public class ScriptableObjectAddressablesSetter : EditorWindow
{
    private static AddressableAssetSettings settings;

    private static readonly Dictionary<string, string> SOToAddress = new Dictionary<string, string>
    {
    };

    private static readonly Dictionary<string, string> FolderToLabel = new Dictionary<string, string>
    {
        {"Monsters", "MonsterData"},
        {"Skills", "SkillData"},
        {"Equipments", "EquipmentData"},
        {"Consumables", "ConsumableData"},
        {"Rarities", "RarityData"},
        {"StatusEffects", "StatusEffectData"},
        {"MonsterSkills", "MonsterSkillData"},
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

            string label = FolderToLabel.ContainsKey(folderName) ? FolderToLabel[folderName] : folderName;
            SetLabelOnly(guid, label);
            count++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"═══ ScriptableObject 라벨 설정 완료! 총 {count}개 ═══");
    }

    private static void SetLabelOnly(string guid, string label)
    {
        AddressableAssetEntry entry = settings.FindAssetEntry(guid);

        if (entry == null)
        {
            AddressableAssetGroup group = settings.DefaultGroup;
            entry = settings.CreateOrMoveEntry(guid, group);
        }

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

    [MenuItem("Tools/Addressables/Set Skill Icon Sprites")]
    public static void SetSkillIconSprites()
    {
        settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Asset Settings not found!");
            return;
        }

        int count = 0;
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites/SkillIcon" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            
            string address = $"Skills/{fileName}";
            
            SetAddress(guid, address, "SkillIcon");
            count++;
            Debug.Log($"[SkillIcon] {fileName} -> {address}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"═══ 스킬 아이콘 Sprite 등록 완료! 총 {count}개 ═══");
    }

    [MenuItem("Tools/Addressables/Set All (SO + Sprites)")]
    public static void SetAllAddressables()
    {
        SetAllSOLabels();
        SpriteAddressablesSetter.SetAllSpriteAddresses();
        Debug.Log("═══ 모든 Addressables 설정 완료! ═══");
    }

    [MenuItem("Tools/Addressables/🔄 Setup & Build (All-in-One)")]
    public static void SetupAndBuildAll()
    {
        Debug.Log("=== Addressables 자동 설정 시작 ===");

        SetAllSOLabels();
        SpriteAddressablesSetter.SetAllSpriteAddresses();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("═══ 주소 설정 완료. 빌드 시작... ═══");

        BuildAddressables();

        Debug.Log("=== ✅ 모든 작업 완료! 게임 실행 가능 ===");
    }

    private static void BuildAddressables()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Settings 없음!");
            return;
        }

        AddressableAssetSettings.CleanPlayerContent(settings.ActivePlayerDataBuilder);
        AddressableAssetSettings.BuildPlayerContent();

        Debug.Log("═══ ✅ Addressables 빌드 완료! ═══");
    }

    [MenuItem("Tools/Addressables/Set Element Icons")]
    public static void SetElementIcons()
    {
        settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Asset Settings not found!");
            return;
        }

        int count = 0;
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites/Elementer" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            
            string elementName = fileName.Replace("Icon_", "");
            string address = $"Elements/{elementName}";
            
            SetAddress(guid, address, "ElementIcon");
            count++;
            Debug.Log($"[ElementIcon] {fileName} -> {address}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"═══ 속성 아이콘 Sprite 등록 완료! 총 {count}개 ═══");
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