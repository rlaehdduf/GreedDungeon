using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace GreedDungeon.Editor
{
    public static class FixPrefabCanvas
    {
        [MenuItem("Tools/Fix/Remove Canvas from StatusEffectSlot")]
        public static void RemoveCanvasFromPrefab()
        {
            string prefabPath = "Assets/Prefabs/IconSlot/StatusEffectSlot.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab == null)
            {
                Debug.LogError("Prefab not found: " + prefabPath);
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(prefab);
            var prefabInstance = PrefabUtility.LoadPrefabContents(assetPath);

            bool changed = false;
            var canvases = prefabInstance.GetComponentsInChildren<Canvas>(true);
            foreach (var canvas in canvases)
            {
                Debug.Log($"Removing Canvas: {canvas.gameObject.name}");
                Object.DestroyImmediate(canvas, true);
                changed = true;
            }

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, assetPath);
                Debug.Log("Prefab saved successfully!");
            }
            else
            {
                Debug.Log("No Canvas found to remove.");
            }

            PrefabUtility.UnloadPrefabContents(prefabInstance);
        }
    }
}