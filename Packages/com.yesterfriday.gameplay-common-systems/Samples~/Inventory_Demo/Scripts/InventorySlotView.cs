using TMPro;
using UnityEngine;
using Yesterfriday.GameplayCommonSystems.Inventory;

public sealed class InventorySlotView : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    private int index;

    public void Init(int slotIndex)
    {
        index = slotIndex;
        name = $"Slot_{index:00}";
    }

    public void Render(ItemStack stack)
    {
        if (stack.IsEmpty)
        {
            label.text = $"{index}\n-";
            return;
        }

        // 표시용: Id + 수량
        label.text = $"{index}\n{stack.Item.Id}\nx{stack.Amount}";
    }
}