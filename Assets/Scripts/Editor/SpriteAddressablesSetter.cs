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

    [MenuItem("Tools/Addressables/Set Monster Sprite Addresses")]
    public static void SetMonsterSpriteAddresses()
    {
        settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Asset Settings not found!");
            return;
        }

        int count = 0;
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Prefabs/Monster" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string spriteName = System.IO.Path.GetFileNameWithoutExtension(path);

            if (SpriteToAddress.TryGetValue(spriteName, out string address))
            {
                SetAddress(guid, address);
                count++;
            }
            else
            {
                Debug.LogWarning($"매핑 없음: {spriteName}");
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"═══ Monster Sprite 주소 설정 완료! 총 {count}개 ═══");
    }

    private static void SetAddress(string guid, string address)
    {
        AddressableAssetEntry entry = settings.FindAssetEntry(guid);
        
        if (entry == null)
        {
            AddressableAssetGroup group = settings.DefaultGroup;
            entry = settings.CreateOrMoveEntry(guid, group);
        }

        entry.address = address;
        
        if (!settings.GetLabels().Contains(MonsterLabel))
        {
            settings.AddLabel(MonsterLabel);
        }
        entry.SetLabel(MonsterLabel, true);
        
        EditorUtility.SetDirty(settings);
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

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Prefabs/Monster" });

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