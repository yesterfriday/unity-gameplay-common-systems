using UnityEngine;

[CreateAssetMenu(fileName = "ItemDefinition", menuName = "Common Systems/Inventory/Item Definition", order = 1)]
public class ItemDefinition : ScriptableObject
{
    [SerializeField] private string Id;
    [SerializeField] private string DisplayName;
    [SerializeField] [Min(1)] private int MaxStack = 9;

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