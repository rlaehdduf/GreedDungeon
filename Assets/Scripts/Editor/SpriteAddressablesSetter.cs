using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.Collections.Generic;

public class SpriteAddressablesSetter : EditorWindow
{
    private static AddressableAssetSettings settings;

    private static readonly Dictionary<string, string> SpriteToAddress = new Dictionary<string, string>
    {
        {"Slime", "Monsters/Slime"},
        {"Rat", "Monsters/Rat"},
        {"Spider", "Monsters/Spider"},
        {"Skull", "Monsters/Skull"},
        {"Cerberus", "Monsters/Cerberus"},
    };

    private static readonly string MonsterLabel = "MonsterSprites";

    private static readonly Dictionary<string, (string addressPrefix, string label)> FolderSettings = new Dictionary<string, (string, string)>
    {
        {"Assets/Sprites/WeaponIcon", ("Weapons", "WeaponIcon")},
        {"Assets/Sprites/ArmorIcon", ("Armors", "ArmorIcon")},
        {"Assets/Sprites/Accessory", ("Accessories", "AccessoryIcon")},
        {"Assets/Sprites/ItemIcon", ("Items", "ItemIcon")},
        {"Assets/Sprites/SkillIcon", ("Skills", "SkillIcon")},
        {"Assets/Sprites/Elementer", ("Elements", "ElementIcon")},
        {"Assets/Sprites/Monster", ("Monsters", "MonsterSprite")},
    };

    [MenuItem("Tools/Addressables/Set All Sprite Addresses")]
    public static void SetAllSpriteAddresses()
    {
        settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Asset Settings not found!");
            return;
        }

        int totalCount = 0;

        foreach (var folderSetting in FolderSettings)
        {
            string folder = folderSetting.Key;
            string addressPrefix = folderSetting.Value.addressPrefix;
            string label = folderSetting.Value.label;

            int count = SetSpritesInFolder(folder, addressPrefix, label);
            totalCount += count;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"═══ 모든 Sprite 주소 설정 완료! 총 {totalCount}개 ═══");
    }

    private static int SetSpritesInFolder(string folder, string addressPrefix, string label)
    {
        int count = 0;
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);

            if (addressPrefix == "Elements" && fileName.StartsWith("Icon_"))
            {
                fileName = fileName.Substring(5);
            }

            string address = $"{addressPrefix}/{fileName}";
            SetSpriteAddress(guid, address, label);
            count++;
            Debug.Log($"[{addressPrefix}] {fileName} -> {address}");
        }

        Debug.Log($"[{addressPrefix}] {count}개 설정 완료");
        return count;
    }

    private static void SetSpriteAddress(string guid, string address, string label)
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

    [MenuItem("Tools/Addressables/Set Monster Sprite Addresses")]
    public static void SetMonsterSpriteAddresses()
    {
        settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Asset Settings not found!");
            return;
        }

        int count = SetSpritesInFolder("Assets/Sprites/Monster", "Monsters", MonsterLabel);

        AssetDatabase.SaveAssets();
        Debug.Log($"═══ Monster Sprite 주소 설정 완료! 총 {count}개 ═══");
    }

    [MenuItem("Tools/Addressables/Show Monster Sprite Addresses")]
    public static void ShowMonsterSpriteAddresses()
    {
        settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Asset Settings not found!");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites/Monster" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string spriteName = System.IO.Path.GetFileNameWithoutExtension(path);
            AddressableAssetEntry entry = settings.FindAssetEntry(guid);

            if (entry != null)
            {
                string labels = string.Join(", ", entry.labels);
                Debug.Log($"{spriteName} -> 주소: {entry.address}, 라벨: [{labels}]");
            }
            else
            {
                Debug.Log($"{spriteName} -> (Addressable 아님)");
            }
        }
    }
}