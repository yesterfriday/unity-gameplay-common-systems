using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.Slingshot
{
    [CreateAssetMenu (fileName = "SlingshotDefinition2D", menuName = "CommonSystems/Slingshot/Slingshot Definition2D", order = 1)]
    public sealed class SlingshotDefinition2D :  ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
          
        [Header("Projectile")]
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Vector2 _spawnOffset2D;
        
        [Header("Pull & Launch")]
        [Min(0f)] [SerializeField] private float _maxPullDistance = 3f;
        [Min(0f)] [SerializeField] private float _minPullDistance = 0.1f;
        
        [Min(0f)] [SerializeField] private float _maxImpulse = 1f;
        [Min(0f)] [SerializeField] private float _minImpulse = 8f;
        
        [Header("Cooldown")]
        [Min(0f)] [SerializeField] private float _cooldownSeconds = 0.25f;

        
        public string Id => _id;
        public string DisplayName => _displayName;
        public GameObject ProjectilePrefab => _projectilePrefab;
        public Vector2 SpawnOffset2D => _spawnOffset2D;
        public float MaxPullDistance => _maxPullDistance;
        public float MinPullDistance => _minPullDistance;
        public float MaxImpulse => _maxImpulse;
        public float MinImpulse => _minImpulse;
        public float CooldownSeconds => _cooldownSeconds;
        
#if UNITY_EDITOR

        private void OnValidate()
        {
            if (_id != null)
            {
                _id = _id.Trim();
            }
            
            if (string.IsNullOrWhiteSpace(_id))
            {
                Debug.LogWarning($"[SlingshotDefinition] Id is empty: {name}", this);
            }

            if (_projectilePrefab != null && _projectilePrefab.GetComponent<Rigidbody2D>() == null)
            {
                Debug.LogWarning("[SlingshotDefinition2D] ProjectilePrefab requires Rigidbody2D", this);
            }
            
            if (_maxPullDistance <= 0f)
            {
                Debug.LogWarning($"[SlingshotDefinition] MaxPullDistance must be > 0: {name}", this);
            }

            if (_maxImpulse <= 0f)
            {
                Debug.LogWarning($"[SlingshotDefinition] MaxImpulse must be > 0: {name}", this);
            }

            if (_maxImpulse < _minImpulse)
            {
                _maxImpulse = _minImpulse;
                Debug.LogWarning($"[SlingshotDefinition] MaxImpulse < MinImpulse. Clamping: {name}", this);
            }
        }

#endif
    }
}