using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GreedDungeon.Core
{
    public interface IAssetLoader
    {
        Task<T> LoadAssetAsync<T>(string address) where T : Object;
        Task<IList<T>> LoadAssetsAsync<T>(IList<string> addresses) where T : Object;
        Task<IList<T>> LoadAllAssetsByLabelAsync<T>(string label) where T : Object;
        void ReleaseAsset<T>(T asset) where T : Object;
        void ReleaseAssets<T>(IList<T> assets) where T : Object;
    }
}