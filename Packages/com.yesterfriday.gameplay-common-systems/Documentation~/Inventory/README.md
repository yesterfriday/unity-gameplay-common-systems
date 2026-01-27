# README — Inventory (v0.1)

## 요약

이 시스템은 Unity 프로젝트에서 **그리드 기반 인벤토리(슬롯/스택)**를 재사용 가능한 형태로 제공하는 **UPM 패키지 모듈**입니다. v0.1에서는 **Add/Remove/Move/Query**의 핵심 규칙과 “부분 성공(Partial Success)” 반환 규약, 그리고 **OnInventoryChanged 단일 이벤트**로 확장 지점을 확보합니다. 외부 에셋 없이 핵심 로직만 구현하며 v0.2에서 UI/세이브/네트워크 등으로 확장합니다.

## Type

- Inventory

## Version

- v0.1

## Goals

- 다른 Unity 프로젝트에 **바로 붙여서 사용 가능한 형태(UPM 패키지)**로 제공
- 인벤토리의 핵심 동작을 **명세(규칙) 기반**으로 구현(Add/Remove/Move/Query)
- v0.2에서 **UI/세이브/네트워크** 확장이 가능하도록 **단일 변경 이벤트 규약** 제공
- 외부 에셋 없이 **로직 중심**, 테스트/데모로 동작 검증 가능하게 구성

## Data Model

- 아이템 타입: ScriptableObject 기반 `ItemDefinition`
- 슬롯: 고정 그리드(가로: 6, 세로: 6)
    - 그리드 고정 이유: 슬롯 인덱스가 안정적이라 **저장/로드** 및 **UI 매핑**이 단순해짐
    - 가변 슬롯은 v0.2에서 확장
- 스택: 스택 가능, 최대 스택 수는 **아이템별**로 관리(예: 9)

## Scope (v0.1)

- Add / Remove / Move / Query

## Behavior Rules

- 용어
    - **entry**: 슬롯에 저장되는 `ItemStack`(아이템 + 수량)
- Add
    - 슬롯에 아이템을 추가
    - 기존 엔트리를 먼저 채운 후, 빈 슬롯에 새 엔트리를 생성
        - 채우기/배치 순서: **좌 → 우, 상 → 하(row-major)**
    - 공간 부족 시 Add는 **부분 성공** 가능
        - 처리된 수량을 `added`로 반환
        - `added == requested`: success
        - `0 < added < requested`: partial success (insufficient space)
        - `added == 0`: failure
- Remove
    - 슬롯에서 아이템을 제거
    - 수량 부족 시 Remove는 **부분 성공** 가능
        - 처리된 수량을 `removed`로 반환
        - `removed == requested`: success
        - `0 < removed < requested`: partial success (insufficient amount)
        - `removed == 0`: failure
- Move
    - 슬롯 간 이동 API 제공
    - 조건
        - 빈 슬롯: 이동
        - 동일 아이템: 병합 + 남는 수량은 원 슬롯에 유지(부분 이동 허용)
        - 다른 아이템: `amount`는 무시하고 **전체 스택 스왑만 수행**(스왑 불가 시 실패)
    - Drag & Drop 기반 UI 이동은 v0.2에서 제공
- Query
    - `GetCount(item)`는 총 수량을 반환
    - 아이템이 없으면 `0` 반환

## Failure Conditions (v0.1)

- Add 실패
    - 인벤토리에 공간이 전혀 없는 경우: `added == 0`
    - 부분 추가 후 더 이상 공간이 없으면: `0 < added < requested` (partial success)
- Remove 실패
    - 제거 가능한 수량이 0인 경우: `removed == 0`
    - 부분 제거 후 더 이상 제거할 수 없으면: `0 < removed < requested` (partial success)
- Move 실패
    - Invalid slot index
    - empty source slot
    - `amount <= 0`
    - `amount > source amount`
    - 다른 아이템 스왑이 불가능한 경우(정책에 의해) 실패

## Events

- `OnInventoryChanged(IReadOnlyList<int> changedIndices)`
    - `N = width * height`
    - `changedIndices`: 변경된 슬롯 인덱스(0..N-1)
    - 인덱스 기준: **좌→우, 상→하(row-major)**
- v0.2 확장 포인트: UI / Save / Network

## API (v0.1)

- `bool TryAdd(ItemDefinition item, int requested, out int added)`
- `bool TryRemove(ItemDefinition item, int requested, out int removed)`
- `bool TryMove(int from, int to, int amount)`
- `int GetCount(ItemDefinition item)`

## Notes (Implementation Guidance)

- 반환값 규약(권장)
    - `true` if any change was applied
        - `added > 0`, `removed > 0`, 또는 move/swap/merge 성공
- entry/stack 개념을 분리하면 확장(v0.2: UI 드래그, 세이브 직렬화, 네트워크 동기화)이 쉬워짐
- 고정 그리드 + row-major 인덱스는 **저장/로드 + UI 바인딩**에서 가장 단순하고 안정적인 선택

## Sample (Demo)

- 샘플 씬에서 동작 확인
    - `Assets/Samples/Gameplay Common Systems/0.0.1/Inventory Sample/Scenes/Inventory_Sample.unity`

## Roadmap (v0.2+)

- 가변 슬롯(가로/세로 변경, 슬롯 확장)
- Drag & Drop UI 이동(부분 이동 UX 포함)
- Save/Load(슬롯 인덱스 기반 직렬화)
- Network Sync(변경 슬롯 기반 델타 전송)
- 필터/정렬/태그, 장비 슬롯/전용 슬롯 등 특수 규칙 확장

## Changelog

- v0.1: Grid + Stack 기반 인벤토리 코어(Add/Remove/Move/Query), Partial Success 규약, 단일 변경 이벤트 제공