using System.Threading.Tasks;
using GreedDungeon.Core;
using GreedDungeon.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace GreedDungeon.Tests
{
    public class AddressablesTest : MonoBehaviour
    {
        [Header("Test UI")]
        [SerializeField] private Button _loadMonsterButton;
        [SerializeField] private Button _loadSkillButton;
        [SerializeField] private Button _loadAllDataButton;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Text _statusText;

        private IAssetLoader _assetLoader;
        private IGameDataManager _gameDataManager;

        private GameObject _spawnedPrefab;

        private void Awake()
        {
            _assetLoader = Services.Get<IAssetLoader>();
            _gameDataManager = Services.Get<IGameDataManager>();
        }

        private async void Start()
        {
            if (_gameDataManager == null)
            {
                UpdateStatus("에러: DI 초기화 실패\nGameInstaller가 실행되었는지 확인하세요");
                return;
            }
            await InitializeAsync();
            SetupButtons();
        }

        private async Task InitializeAsync()
        {
            UpdateStatus("초기화 중...");
            await _gameDataManager.InitializeAsync();
            UpdateStatus($"초기화 완료!\n몬스터: {_gameDataManager.GetAllMonsterData().Count}개\n스킬: {_gameDataManager.GetAllSkillData().Count}개");
        }

        private void SetupButtons()
        {
            if (_loadMonsterButton != null)
                _loadMonsterButton.onClick.AddListener(OnLoadMonsterClicked);

            if (_loadSkillButton != null)
                _loadSkillButton.onClick.AddListener(OnLoadSkillClicked);

            if (_loadAllDataButton != null)
                _loadAllDataButton.onClick.AddListener(OnLoadAllDataClicked);
        }

        private async void OnLoadMonsterClicked()
        {
            UpdateStatus("몬스터 프리팹 로딩 중...");
            
            var monsterData = _gameDataManager.GetMonsterData(1);
            if (monsterData == null)
            {
                UpdateStatus("에러: 몬스터 데이터 ID 1을 찾을 수 없음");
                return;
            }

            var prefab = await _assetLoader.LoadAssetAsync<GameObject>(monsterData.PrefabAddress);
            if (prefab == null)
            {
                UpdateStatus($"에러: 프리팹 로드 실패\n주소: {monsterData.PrefabAddress}");
                return;
            }

            if (_spawnedPrefab != null)
                Destroy(_spawnedPrefab);

            _spawnedPrefab = Instantiate(prefab, _spawnPoint.position, Quaternion.identity);
            UpdateStatus($"성공!\n{monsterData.Name} 스폰됨\n주소: {monsterData.PrefabAddress}");
        }

        private async void OnLoadSkillClicked()
        {
            UpdateStatus("스킬 아이콘 로딩 중...");

            var skillData = _gameDataManager.GetSkillData(1);
            if (skillData == null)
            {
                UpdateStatus("에러: 스킬 데이터 ID 1을 찾을 수 없음");
                return;
            }

            var prefab = await _assetLoader.LoadAssetAsync<GameObject>(skillData.IconAddress);
            if (prefab == null)
            {
                UpdateStatus($"에러: 아이콘 로드 실패\n주소: {skillData.IconAddress}");
                return;
            }

            if (_spawnedPrefab != null)
                Destroy(_spawnedPrefab);

            _spawnedPrefab = Instantiate(prefab, _spawnPoint.position, Quaternion.identity);
            UpdateStatus($"성공!\n{skillData.Name} 아이콘 스폰됨\n주소: {skillData.IconAddress}");
        }

        private void OnLoadAllDataClicked()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== 모든 데이터 ===");

            sb.AppendLine("\n[몬스터]");
            foreach (var m in _gameDataManager.GetAllMonsterData())
                sb.AppendLine($"  ID:{m.ID} {m.Name} - {m.PrefabAddress}");

            sb.AppendLine("\n[스킬]");
            foreach (var s in _gameDataManager.GetAllSkillData())
                sb.AppendLine($"  ID:{s.ID} {s.Name} - {s.IconAddress}");

            UpdateStatus(sb.ToString());
        }

        private void UpdateStatus(string message)
        {
            if (_statusText != null)
                _statusText.text = message;
            Debug.Log($"[AddressablesTest] {message}");
        }

        private void OnDestroy()
        {
            if (_spawnedPrefab != null)
                Destroy(_spawnedPrefab);
        }
    }
}