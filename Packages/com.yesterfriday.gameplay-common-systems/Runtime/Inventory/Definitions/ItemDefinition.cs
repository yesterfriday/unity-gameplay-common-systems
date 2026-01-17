using UnityEngine;

[CreateAssetMenu(fileName = "ItemDefinition", menuName = "Common Systems/Inventory/Item Definition", order = 1)]
public class ItemDefinition : ScriptableObject
{
    public string Id;
    [SerializeField] private string DisplayName;
    [Min(1)] public int MaxStack = 9;

#if UnityEditor
    private void OnValidate()
    {
        if(maxStack < 1)
        {
            maxStack = 1;
        }
    }
#endif
}