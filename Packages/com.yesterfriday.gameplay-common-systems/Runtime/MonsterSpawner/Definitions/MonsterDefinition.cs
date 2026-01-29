using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.MonsterSpawner
{
    [CreateAssetMenu(fileName = "MonsterDefinition", menuName = "Common Systems/MonsterSpawner/MonsterDefinition", order = 1)]
    public sealed class MonsterDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private GameObject _prefab;

        public string Id => _id;
        public string DisplayName => _displayName;
        public GameObject Prefab => _prefab;
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_id != null)
            {
                _id = _id.Trim();
            }

            if (string.IsNullOrWhiteSpace(_id))
            {
                Debug.LogWarning($"[MonsterDefinition] Id is empty: {name}", this);
            }

            if (_prefab == null)
            {
                Debug.LogWarning($"[MonsterDefinition] Prefab is not assigned: {name}", this);
            }
        }
#endif
    }
}