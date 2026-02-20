# GameA Sample (v0.1) — Loop Proof Mini Game

GameA는 **스폰 → Registry 추적 → 웨이브 클리어 → 보상 드랍 → 픽업 → 인벤 반영 → 다음 웨이브**의 “게임 루프 증명”을 목표로 하는 최소 샘플입니다.

> ✅ Source of Truth(SoT): `Runtime/GameA/*`  
> ⚠️ `Assets/Samples/...` 경로는 Import 결과물(복제본)이며, 수정/커밋 대상이 아닙니다.

---

## 1) Import 방법
1. Unity 메뉴: **Window > Package Manager**
2. 좌측 상단에서 본 패키지 선택: `com.yesterfriday.gameplay-common-systems`
3. 우측 패널: **Samples** 섹션에서 **GameA** → **Import**

---

## 2) 10초 재현(Quick Start)
1. 씬 열기: `Samples~/GameA/Scenes/GameA_Main.unity`
2. Play
3. 웨이브가 시작되고 적이 스폰됨
4. 적을 처치하면 웨이브가 종료되고 보상이 드랍됨
5. 플레이어가 보상을 먹으면 카운터가 증가하고 다음 웨이브가 시작됨

## 설계 의도(Dependency Graph / Why this layout?)
GameA는 **이벤트 기반 루프**를 최소 구성으로 증명하기 위해 시스템을 `@System` 오브젝트에 집중 배치합니다.  
핵심 흐름은 다음과 같습니다:

1) `MonsterSpawner`가 적을 스폰/디스폰 → `SpawnerToEnemyRegistryBridge`가 이를 `EnemyRegistry`에 **Register/Unregister**  
2) `WaveEndCondition_EliminateAll`은 `EnemyRegistry`의 상태(AliveCount)를 기반으로 **WaveCleared**를 판정  
3) `GameAWaveRewardBridge`는 WaveCleared를 입력으로 받아 `LootDropper`에 드랍을 요청하고, 픽업 결과를 `GameAInventoryCounter`에 반영  
4) `GameAFlowCoordinator`는 `WaveController/Spawner/Registry/EndCondition/Targeting`을 묶어 **“웨이브 시작/반복 + 디버그 스폰(F1)/시작(F2)”**을 제공  
5) `TargetingController2D`는 Camera + Enemy 마스크로 타겟을 선택하고, `GameACardPlayController`는 선택 타겟(Strike) 또는 플레이어(Heal)에 효과를 적용  
6) `@Player`는 최소 물리/충돌(Rigidbody2D/Collider2D)을 가져 **LootPickup2D Trigger 픽업**이 가능해야 합니다.

> 요약: **Spawner → Registry → EndCondition → RewardBridge(LootDropper) → Pickup → InventoryProof** 가 GameA의 “루프 증명” 최소 의존 그래프입니다.

---

## 3) 필수 에셋/씬 경로
- Scene: `Samples~/GameA/Scenes/GameA_Main.unity`
- Monster Prefab: `Samples~/GameA/Prefabs/@Monster.prefab`
- Loot Pickup Prefab: `Samples~/GameA/Prefabs/LootPickup2D.prefab`
- Loot Table: `Samples~/GameA/Prefabs/LootTableDefinition.asset`
- Monster Definition: `Samples~/GameA/Prefabs/MonsterDefinition.asset`

---

## 4) 필수 세팅(중요)
### 4.1 LootTableDefinition 설정
- `Samples~/GameA/Prefabs/LootTableDefinition.asset` 열기
- **Entries**가 비어있으면 드랍이 발생하지 않습니다.
- 각 Entry에 `Weight > 0` 값을 부여하세요.

### 4.2 LootPickup2D 충돌/트리거 전제
- `LootPickup2D.prefab`는 플레이어와 충돌(Trigger)로 픽업됩니다.
- 플레이어/픽업 오브젝트의 `Collider2D`, `Rigidbody2D` 설정이 올바른지 확인하세요.
  - 일반적으로 Trigger 픽업은 “한쪽에 Rigidbody2D”가 필요합니다.

### 4.3 (사용 중이라면) LayerMask/레이어
- 픽업이 안 되면, 플레이어 레이어 및 LootPickup2D의 LayerMask(사용 시)가 일치하는지 확인하세요.

---

## 5) Inspector Wiring (필수 참조 연결)

GameA_Main 씬은 `@System` 아래에 시스템 컴포넌트를 모아두는 구조입니다.  
아래 항목은 **Inspector 필드(라벨) 기준으로 “무엇을 어디에 드래그해야 하는지”**를 고정한 체크리스트입니다.

> Scene Root: `@System`, `@Player`, `@UI`, `@SpawnPoints`, `MainCamera`

---

### 5.1 WaveEndCondition_EliminateAll
오브젝트: `@System / WaveEndCondition_EliminateAll`

- **Enemy Registry** → `@System / EnemyRegistry (EnemyRegistry)`
- **Require At Least One ...** → 체크(권장)  
  - 첫 웨이브에 스폰이 0인 경우 “즉시 클리어”되는 걸 방지하기 위함

---

### 5.2 LootDropper
오브젝트: `@System / LootDropper`

**Table**
- **Loot Table** → `Samples~/GameA/Prefabs/LootTableDefinition.asset`

**Spawn**
- **Pickup Prefab** → `Samples~/GameA/Prefabs/LootPickup2D.prefab`
- **Drop Origin** → (선택) 드랍 기준 Transform  
  - 비워두면( None ) 기본 위치(오브젝트/월드 원점 등)로 드랍될 수 있으니, 필요하면 드랍 기준점을 지정
- **Drop Count** → 기본 `1`
- **Scatter Radius** → 기본 `0.4`

---

### 5.3 GameAFlowCoordinator
오브젝트: `@System / GameAFlowCoordinator`

**Refs (Systems)**
- **Wave Controller** → `@System / WaveController`
- **Monster Spawner** → `@System / MonsterSpawner`
- **Enemy Registry** → `@System / EnemyRegistry`
- **Wave End Condition** → `@System / WaveEndCondition_EliminateAll`
- **Targeting** → `@System / TargetingController2D`

**Debug Spawn**
- **Debug Monster** → `Samples~/GameA/Prefabs/MonsterDefinition.asset`
- **Spawn Key** → `F1`
- **Start Wave Key** → `F2`
- **Auto Start Wave On ...** → 체크(권장)
- **Auto Start Next W...** → 체크(권장)

---

### 5.4 SpawnerToEnemyRegistryBridge
오브젝트: `@System / SpawnerToEnemyRegistryBridge`

- **Spawner** → `@System / MonsterSpawner`
- **Registry** → `@System / EnemyRegistry`

---

### 5.5 TargetingController2D
오브젝트: `@System / TargetingController2D`

**Refs**
- **Camera** → `MainCamera`

**Config**
- **Target Mask** → `Enemy` 레이어(또는 적을 포함하는 레이어 마스크)

---

### 5.6 GameACardPlayController
오브젝트: `@System / GameACardPlayController`

**Refs**
- **Targeting** → `@System / TargetingController2D`
- **Player Health** → `@Player` 오브젝트의 `Health` 컴포넌트

**Config**
- **Strike Damage** → 기본 `3`
- **Heal Amount** → 기본 `2`

---

### 5.7 GameAWaveRewardBridge
오브젝트: `@System / GameAWaveRewardBridge`

**Wave Source**
- **End Condition** → `@System / WaveEndCondition_EliminateAll`
- **Wave Controller** → `@System / WaveController`

**Loot**
- **Loot Dropper** → `@System / LootDropper`

**Inventory (proof)**
- **Inventory** → `@System / InventoryCounter` 오브젝트의 `GameAInventoryCounter` 컴포넌트

---

## 6) @Player 최소 조건
오브젝트: `@Player`

- `Rigidbody2D`
- `Collider2D`
- `Renderer` (표시용)

> LootPickup2D는 보통 Trigger 기반으로 작동하므로, 플레이어/픽업 중 최소 한쪽에 Rigidbody2D가 필요합니다.

---

## 7) 트러블슈팅(자주 나오는 원인)
- **드랍이 안 나와요**
  - `LootTableDefinition.asset`의 Entries가 비었거나 Weight가 0일 수 있습니다.
- **픽업이 안 돼요**
  - Collider2D / Rigidbody2D / Trigger 조건 확인
  - (사용 중이라면) Layer/LayerMask 불일치 확인
- **웨이브가 끝나지 않아요**
  - EnemyRegistry가 적을 제대로 Unregister 하지 못했을 수 있습니다.
  - SpawnerToEnemyRegistryBridge 연결 상태 확인
- **타겟팅이 이상해요**
  - Targetable2D가 적 프리팹에 붙어있는지, TargetingController2D가 씬에 1개인지 확인
- **카드가 적용되지 않아요**
  - GameACardPlayController가 Targeting/Health 참조를 제대로 받고 있는지 확인

---

## 8) 확장 포인트(P1/P2 아이디어)
- P1: 타겟팅 UX 개선(하이라이트/우선순위/취소/가드)
- P1: 보상 테이블 다양화(희귀도/조건부 드랍)
- P2: 덱/카드 시스템을 GameB에 연결 가능한 공용 규약으로 확장