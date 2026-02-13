# C2 Verification (SamplesCommon)

C2(Game Loop Common) 기능을 **2D 기준**으로 빠르게 검증하기 위한 씬/프리팹/테스트 리그입니다.

검증 범위:
- EnemyRegistry: 적 등록/해제, AliveCount, 이벤트
- WaveEndCondition_EliminateAll: **>0 → 0 전이**에서 WaveCleared **단발성**
- Loot (2D, Layer 기반): LootDropper(드랍) + LootPickup2D(트리거 픽업)

> ⚠️ 원칙: 개발/수정/커밋은 `Packages/.../Samples~`만.  
> `Assets/Samples/...`는 Import 복제본이므로 수정/커밋 금지.

---

## Folder Structure (recommended)
- `Scenes/C2_Verification.unity`
- `Prefabs/LootPickup2D.prefab`
- `ScriptableObjects/LootTableDefinition_C2Test.asset`
- `Scripts` (or referenced from Samples~/Common/Scripts/Tests)
  - `C2TestRig.cs`
  - `SimplePlayerMover2D.cs`

---

## Prerequisites (must-do once)
### 1) Create Player Layer
1. `Project Settings > Tags and Layers`
2. `User Layer`에 `Player` 추가

### 2) Player setup
- Player 오브젝트:
  - `Layer = Player`
  - `Rigidbody2D` (Dynamic 권장, GravityScale=0)
  - `Collider2D` (BoxCollider2D 등)
  - `SimplePlayerMover2D` (WASD/화살표 이동)

### 3) LootPickup2D prefab setup
- `LootPickup2D.prefab`:
  - `Collider2D` with `IsTrigger = true`
  - `LootPickup2D._playerLayerMask`에 `Player` 레이어 포함

---

## Scene Wiring Checklist
Open: `Scenes/C2_Verification.unity`

In `@Systems`:
- EnemyRegistry
- WaveEndCondition_EliminateAll
  - `_enemyRegistry`에 EnemyRegistry 연결
- LootDropper
  - `_lootTable`에 `LootTableDefinition_C2Test.asset` 연결
  - `_pickupPrefab`에 `LootPickup2D.prefab` 연결
  - `_dropOrigin` 비어있으면 self 사용 (OK)
- C2TestRig
  - `_enemyRegistry`, `_waveEndCondition`, `_lootDropper` 연결

---

## How To Test (Play Mode)
Play 모드에서 아래 키로 검증합니다.

### Key Bindings
- `F1` : Dummy Enemy 1개 생성 + Register
- `F2` : 마지막 Dummy Enemy Unregister + Destroy
- `F3` : WaveEndCondition Arm
- `F4` : WaveEndCondition Disarm
- `F5` : LootDropper.TryDrop (픽업 생성)
- `F6` : 중복 Register + 잘못된 Unregister 테스트

### Expected Logs (examples)
- `[C2TestRig] AliveCountChanged -> N`
- `[C2TestRig] WaveCleared fired ...` (단, 아래 조건에서만 1회)
- `[C2TestRig] LootDropper.TryDrop -> True`
- `[C2TestRig] LootPicked -> <itemId> x<amount>`

---

## Minimal Test Cases (pass criteria)
1) Registry Count
- F1 3회 → AliveCount 1,2,3 증가
- F2 2회 → AliveCount 2,1 감소

2) Duplicate/Invalid Ops
- 적 1마리 이상 생성 후(F1) F6 실행
- 기대: AliveCount 증가 없음(duplicate register false), phantom unregister false

3) WaveCleared Non-False-Positive
- 적이 없는 상태에서 F3(Arm)만 누르고 대기
- 기대: WaveCleared 발생 X

4) WaveCleared One-shot (>0 -> 0)
- F3(Arm)
- F1로 적 2~3마리 등록(AliveCount > 0)
- F2 반복해서 0으로 만들기
- 기대: 0 되는 순간 WaveCleared 1회, 이후 추가 발생 X

5) Loot Pickup (Layer-based)
- F5로 픽업 생성
- Player를 픽업에 겹치게 이동
- 기대: LootPicked 로그 1회 + 픽업 오브젝트 Destroy

---

## Troubleshooting
### LootDropper.TryDrop -> False
- LootTableDefinition Entries가 비어있거나 Weight 합이 0일 수 있음
- ItemId가 빈 문자열/공백이면 실패할 수 있음
- 권장 최소 설정:
  - Entry 1개: `ItemId=coin, Weight=1, Min=1, Max=1`

### Loot spawns but LootPicked log not shown
- Player 오브젝트 `Layer=Player`인지 확인
- LootPickup2D `_playerLayerMask`에 Player 레이어 포함 확인
- Player에 `Rigidbody2D`가 있는지 확인 (TriggerEnter2D 안정성)

---

## Notes (Portfolio)
이 씬은 “샘플 Import 후 즉시 재현 가능한 검증 환경”을 제공하기 위한 것으로,
- 설정(레이어/마스크) 실수 포인트가 적고
- 결과가 로그로 명확하며
- 다른 샘플(GameA/GameB)의 기반(C2)을 빠르게 회귀 테스트할 수 있습니다.