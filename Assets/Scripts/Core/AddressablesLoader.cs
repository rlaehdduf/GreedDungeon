using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GreedDungeon.Core
{
    public class AddressablesLoader : IAssetLoader
    {
        private readonly Dictionary<Object, AsyncOperationHandle> _handles = new Dictionary<Object, AsyncOperationHandle>();

        public async Task<T> LoadAssetAsync<T>(string address) where T : Object
        {
            var handle = Addressables.LoadAssetAsync<T>(address);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[AddressablesLoader] Failed to load: {address}");
                return null;
            }

            var asset = handle.Result;
            _handles[asset] = handle;
            return asset;
        }

        public async Task<IList<T>> LoadAssetsAsync<T>(IList<string> addresses) where T : Object
        {
            var results = new List<T>();
            var handles = new List<AsyncOperationHandle<T>>();

            foreach (var address in addresses)
            {
                var handle = Addressables.LoadAssetAsync<T>(address);
                handles.Add(handle);
            }

            foreach (var handle in handles)
            {
                await handle.Task;
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    results.Add(handle.Result);
                    _handles[handle.Result] = handle;
                }
            }

            return results;
        }

        public async Task<IList<T>> LoadAllAssetsByLabelAsync<T>(string label) where T : Object
        {
            var handle = Addressables.LoadAssetsAsync<T>(label, null);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[AddressablesLoader] Failed to load label: {label}");
                return new List<T>();
            }

            var results = handle.Result;
            foreach (var asset in results)
            {
                _handles[asset] = handle;
            }
            return results;
        }

        public void ReleaseAsset<T>(T asset) where T : Object
        {
            if (asset == null) return;

            if (_handles.TryGetValue(asset, out var handle))
            {
                Addressables.Release(handle);
                _handles.Remove(asset);
            }
        }

        public void ReleaseAssets<T>(IList<T> assets) where T : Object
        {
            if (assets == null) return;

            foreach (var asset in assets)
            {
                ReleaseAsset(asset);
            }
        }
    }
}