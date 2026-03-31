using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.Collections.Generic;

public class AddressablesSetter : EditorWindow
{
    private static AddressableAssetSettings settings;

    private static readonly Dictionary<string, string> PrefabToAddress = new Dictionary<string, string>
    {
        {"Slime(Leaf)", "Monsters/Slime"},
        {"Rat(None)", "Monsters/Rat"},
        {"Spider(Leaf)", "Monsters/Spider"},
        {"Skull(Fire)", "Monsters/Skull"},
        {"Cerberus(Fire)", "Monsters/Cerberus"},
        
        {"Smash", "Skills/Smash"},
        {"DoubleSlash", "Skills/DoubleSlash"},
        {"Slash", "Skills/Slash"},
        {"CrossSlash", "Skills/CrossSlash"},
        {"ManaBall", "Skills/ManaBall"},
        {"FireBall", "Skills/FireBall"},
        {"IceRain", "Skills/IceRain"},
        {"PowerBuff", "Skills/PowerBuff"},
        {"DefenseBuff", "Skills/DefenseBuff"},
        {"HealthBuff", "Skills/HealthBuff"},
        {"SpeedBuff", "Skills/SpeedBuff"},
        {"Heal", "Skills/Heal"},
        {"DoubleSmash", "Skills/DoubleSmash"},
        {"PoisonDeBuff", "Skills/PoisonDeBuff"},
        {"BurnDeBuff", "Skills/BurnDeBuff"},
        
        {"Stick", "Weapons/Stick"},
        {"Sword", "Weapons/Sword"},
        {"Shield", "Weapons/Shield"},
        {"Wand", "Weapons/Wand"},
        {"Axe", "Weapons/Axe"},
        
        {"LeatherArmor", "Armors/LeatherArmor"},
        {"ChainArmor", "Armors/ChainArmor"},
        {"PlateArmor", "Armors/PlateArmor"},
        {"SpikeArmor", "Armors/SpikeArmor"},
        {"FeatherArmor", "Armors/FeatherArmor"},
        
        {"Ring", "Accessories/Ring"},
        {"NeckGlass", "Accessories/NeckGlass"},
        {"Cloak", "Accessories/Cloak"},
        {"Jewel", "Accessories/Jewel"},
        {"Crown", "Accessories/Crown"},
        
        {"Potion", "Items/Potion"},
        {"Antidote", "Items/Antidote"},
        {"BuffPotion", "Items/BuffPotion"},
        {"DeBuffPotion", "Items/DeBuffPotion"},
        {"MagicArrow", "Items/MagicArrow"},
        {"Explosion", "Items/Explosion"},
        {"Coins", "Items/Coins"}
    };

    [MenuItem("Tools/Addressables/Set All Prefab Addresses")]
    public static void SetAllPrefabAddresses()
    {
        settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Asset Settings not found!");
            return;
        }

        int count = 0;
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string prefabName = System.IO.Path.GetFileNameWithoutExtension(path);

            if (PrefabToAddress.TryGetValue(prefabName, out string address))
            {
                SetAddress(guid, address);
                count++;
            }
            else
            {
                Debug.LogWarning($"No mapping found: {prefabName}");
            }
        }

        Debug.Log($"═══ Addressables addresses set! Total {count} ═══");
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
        EditorUtility.SetDirty(settings);
    }

    [MenuItem("Tools/Addressables/Show Current Addresses")]
    public static void ShowCurrentAddresses()
    {
        settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Asset Settings not found!");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string prefabName = System.IO.Path.GetFileNameWithoutExtension(path);
            AddressableAssetEntry entry = settings.FindAssetEntry(guid);

            if (entry != null)
            {
                Debug.Log($"{prefabName} -> {entry.address}");
            }
            else
            {
                Debug.Log($"{prefabName} -> (Not Addressable)");
            }
        }
    }
}