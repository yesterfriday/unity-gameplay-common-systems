using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Yesterfriday.GameplayCommonSystems.Inventory;

public sealed class InventoryDemoController : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private Transform gridRoot;          // GridLayoutGroup이 붙은 오브젝트
    [SerializeField] private InventorySlotView slotPrefab; // Button + Text 붙은 프리팹
    [SerializeField] private TMP_Text logText;

    [Header("Items")]
    [SerializeField] private ItemDefinition itemA;
    [SerializeField] private ItemDefinition itemB;

    private InventoryModel model;
    private InventorySlotView[] slots;

    private void Start()
    {
        model = new InventoryModel(6, 6);
        model.OnInventoryChanged += OnChanged;

        BuildGrid();
        FullRender();

        Log("Demo started. Use buttons or keys: 1(AddA) 2(RemA) 3(Move0->1) 4(AddB) 5(Swap 0<->2)");
    }

    private void BuildGrid()
    {
        int n = model.SlotCount;
        slots = new InventorySlotView[n];

        for (int i = 0; i < n; i++)
        {
            var view = Instantiate(slotPrefab, gridRoot);
            view.Init(i);
            slots[i] = view;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) AddA();
        if (Input.GetKeyDown(KeyCode.Alpha2)) RemoveA();
        if (Input.GetKeyDown(KeyCode.Alpha3)) Move01();
        if (Input.GetKeyDown(KeyCode.Alpha4)) AddB();
        if (Input.GetKeyDown(KeyCode.Alpha5)) Swap02();
    }

    // ---- Button hooks ----
    public void AddA()
    {
        bool ok = model.TryAdd(itemA, 7, out int added);
        Log($"TryAdd A 7 => ok={ok}, added={added}");
        // 변경 슬롯만 OnChanged에서 렌더됨
    }

    public void RemoveA()
    {
        bool ok = model.TryRemove(itemA, 5, out int removed);
        Log($"TryRemove A 5 => ok={ok}, removed={removed}");
    }

    public void Move01()
    {
        bool ok = model.TryMove(0, 1, 3);
        Log($"TryMove 0->1 amount=3 => ok={ok}");
    }

    public void AddB()
    {
        bool ok = model.TryAdd(itemB, 9, out int added);
        Log($"TryAdd B 9 => ok={ok}, added={added}");
    }

    public void Swap02()
    {
        bool ok = model.TryMove(0, 2, 1); // 다른 아이템이면 amount 무시, 전체 스왑
        Log($"TryMove 0->2 amount=1 (swap full if different) => ok={ok}");
    }

    private void OnChanged(IReadOnlyList<int> changed)
    {
        for (int i = 0; i < changed.Count; i++)
        {
            int idx = changed[i];
            slots[idx].Render(model.GetSlot(idx));
        }
        Log($"OnInventoryChanged: [{string.Join(",", changed)}]");
    }

    private void FullRender()
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i].Render(model.GetSlot(i));
    }

    private void Log(string msg)
    {
        Debug.Log(msg);
        if (logText != null) logText.text = msg;
    }
}
