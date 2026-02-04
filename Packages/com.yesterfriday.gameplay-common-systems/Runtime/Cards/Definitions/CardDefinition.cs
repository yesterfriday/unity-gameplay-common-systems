using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.Cards
{
    [CreateAssetMenu(fileName = "CardDefinition", menuName = "Common Systems/Cards/Card Definition", order = 1)]

    public sealed class CardDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;

        public string Id => _id;
        public string DisplayName => string.IsNullOrWhiteSpace(_displayName) ? name : _displayName;
            
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_id != null)
            {
                _id = _id.Trim();
            }

            if (string.IsNullOrWhiteSpace(_id))
            {
                Debug.LogWarning($"[CardDefinition] Id is empty: {name}", this);
            }
        }
#endif
    }

}