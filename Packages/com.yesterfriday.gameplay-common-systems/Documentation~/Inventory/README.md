# README

**Type: Inventory**

**Version: v0.1**

**Data Model**

- 아이템 타입: ScriptableObject 기반(ItemDefinition)
- 슬롯: 고정 그리드로 설정(가로:6, 세로:6 // 가변 슬롯은 v0.2)
    - 그리드인 이유: 그리드 고정은 슬롯 인덱스가 안정적이라 저장/로드 및 UI 매핑이 단순해진다.
- 스택: 스택 가능, 최대 스택 수: 9(아이템별)

**Scope(v0.1)**

- Add / Remove / Move / Query

**Behavior Rules**

- **Add**
    - 슬롯에 아이템을 추가
    - “엔트리(entry)는 슬롯에 저장되는 `ItemStack`을 의미한다.”
    - 기존 엔트리를 먼저 채운 후, 빈 슬롯에 새 엔트리를 생성(좌 → 우, 상 → 하 순서)
    - 공간 부족 시 Add 부분 성공
        - 부분 추가 후 처리된 수량 반환 - 들어갈 수 있는 수량은 추가 후, 처리된 수량을 반환
        - added == requested: success
        - 0 < added < requested: partial success (insufficient space)
        - added == 0: failure
- **Remove**
    - 슬롯에서 아이템을 제거
    - 수량 부족 시 Remove 부분 성공
        - 부분 제거 후 처리된 수량 반환 - 제거 가능한 수량은 제거 후, 처리된 수량을 반환
        - removed == requested: success
        - 0 < removed < requested: partial success (insufficient amount)
        - removed == 0: failure
- **Move**
    - 슬롯 간 이동 API 제공
    - 조건
        - 빈 슬롯 - 이동
        - 동일 아이템 - 병합 + 남는 수량은 원 슬롯에 유지(부분 이동 허용)
        - 다른 아이템 - amount 무시하고 전체 스택 스왑만 수행(스왑 불가 시 실패)
    - (이동 방식 - Drag and Drop으로 아이템 이동 v0.2)
    - Failure conditions
        - Invalid slot index or empty source slot → failure
        - amount <= 0 → failure
        - amount > source amount → failure
- **Query**
    - `GetCount(item)`는 총 수량을 반환하며 없으면 `0`을 반환

**Events**

- OnInventoryChanged(IReadOnlyList<int> changedIndices)
    - N = width * height
    - changedIndices는 변경된 슬롯 인덱스(0..N-1)이며, 인덱스는 좌→우, 상→하(row-major) 기준
- (UI/세이브/네트워크 확장 포인트 v0.2)

**Notes**

- Return value: `true` if any change was applied (`added > 0`, `removed > 0`, or successful move/swap/merge).
- `GetCount(item)` returns total amount (0 if not present).
- Terminology: “entry” refers to an `ItemStack` stored in a slot.

**API (v0.1)**

- `bool TryAdd(ItemDefinition item, int requested, out int added)`
- `bool TryRemove(ItemDefinition item, int requested, out int removed)`
- `bool TryMove(int from, int to, int amount)`
- `int GetCount(ItemDefinition item)`

**Sample**

- `Assets/Samples/Gameplay Common Systems/0.0.1/Inventory Sample/Scenes Inventory_Sample.unity` 씬에서 동작 확인