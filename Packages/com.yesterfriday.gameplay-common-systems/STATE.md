# STATE.md
- 최근 변경 시간 26.02.23 16:30

## 목표
- Unity 기반 **공용 시스템 Toolkit**을 UPM 패키지 형태로 제작해, 다른 Unity 프로젝트에 “바로 붙여서” 재사용 가능하게 만든다.
- 외부 에셋 없이 핵심 기능만 구현(v0.1)하고, v0.2+ 확장 가능하도록 명세/이벤트/구조를 먼저 확정한다.
- 포트폴리오 목적: “명세 → 구현 → 샘플 Import 증명 → 최소 테스트” 흐름을 Git 히스토리로 보여준다.

---

## 현재 진행
- 레포/프로젝트: `unity-gameplay-common-systems` (GitHub: yesterfriday)
- Unity: `2022.3.62f3` (3D 프로젝트 `commonSystem_Host`)
- 패키지: `com.yesterfriday.gameplay-common-systems` (UPM 구조)
- 샘플: Package Manager → Samples → Import 방식으로 데모 씬 제공
- Samples~ 개발 환경 세팅 완료:
  - `Packages/manifest.json`의 `file:` 등록 방식으로 로컬 패키지 정상 노출 확인
  - 개발 원칙 확정:
    - ✅ 코드는 `Packages/com.yesterfriday.gameplay-common-systems/Samples~`에서만 작성/수정
    - ❌ `Assets/Samples/...`는 Import 산출물(복제본)이라 Git 커밋/수정 대상 아님
- 학습형 진행 방식 확정:
  - “문제(과제) → 구현 → 해설 + 현업 리팩터링” 흐름으로 진행
- C1 완료(커밋):
  - 브랜치: `chore/c1-samples-common`
  - 커밋: `feat(samples-common): add C1 common scripts (bootstrap/log/health/waves)`
  - 포함: `Samples~/Common/Scripts/{Core,Gameplay,UI}` 4개 스크립트

---

## 완료
### MonsterSpawner (v0.1)
- “바로 붙여쓰기” 증명 완료(샘플 Import 후 Demo 동작 확인)
- 명세-구현 일치: 최소 테스트/수동 테스트로 **실패 로그/조건 확인**
- 시각성 개선: 스폰된 큐브 색상 랜덤 적용 확인
- 기능(핵심):
  - TrySpawn / (일부) Despawn 흐름 구현 진행
  - MaxAlive, Cooldown, SpawnPointSelection(Random/Sequential) 기반

### Slingshot (v0.1) — Demo/문서 마감
- `SlingshotDefinition2D.cs` 작성(Definition SO 기반 데이터 모델)
- `SlingshotFailReason.cs` 작성(enum 확정)
- `SlingshotFailReasonExtensions.cs` 작성(ToMessage 한글 출력; *NoRigidbody2D 케이스 포함 권장*)
- `SlingshotScreenToWorld2D.cs` 작성(Screen → World, Ray-Plane 교차, z=origin.z)
- `SlingshotLauncher2D` 구현 완료:
  - Begin/Update/End 상태 머신
  - 쿨다운/상태/FailReason 기반 안전장치
  - 이벤트 기반 UI 업데이트 구조
- Samples~/Slingshot2D_Demo 구성 및 재현 동선 확립
- Visualizer 정리(PullLine + TrajectoryDots), AimLine 제거 등 요구사항 반영
- Documentation~/Slingshot/README.md 보강(구성/재현/확장 포인트)

### SamplesCommon (C1) — Bootstrap/Log/Health/WaveController 커밋 완료
- `SampleBootstrap`, `SampleLog`, `Health`, `WaveController` 구현 및 샘플 공통 기반 정리

### C2 (Game Loop Common) — 커밋 완료 + 최소 테스트 통과
- EnemyRegistry(적 등록/해제/AliveCount 이벤트)
- WaveEndCondition_EliminateAll(>0→0 전이에서 WaveCleared 단발성 발행)
- Loot 시스템(2D, 레이어 기반):
  - LootStack, LootEntry, LootTableDefinition(SO) (가중치 롤 + 수량 범위)
  - LootDropper(드랍 생성), LootPickup2D(LayerMask 기반 트리거 픽업 이벤트)
- C2 최소 테스트 환경 구축 및 통과:
  - C2TestRig, SimplePlayerMover2D(키 입력 기반 테스트)
  - 실패 원인 해결: LootTableDefinition Entries 비어있음(드랍 실패) / Player 레이어 미설정(픽업 미발생)
  - Assets/Samples(Import 복제본) vs Packages/Samples~ 참조 충돌 정리
  - 테스트 케이스(Registry/WaveCleared/LootPickup) 모두 통과
---

## 진행중
### GameA (v0.1) — P0 진행 중(1씬 MVP “루프 증명”)
- GameA 샘플 Import 환경 정리 + 재현성(Import 100%) 강화
  - GameA 런타임 스크립트 전부를 `Runtime/GameA`로 이동(단일 소스 SoT)
  - `Samples~/GameA`는 데이터(씬/프리팹/리소스/README)만 유지하도록 정리(스크립트 삭제)
  - `Assets/Samples*`는 Import 산출물(복제본)이라 커밋/수정 금지(.gitignore 적용)
  - Samples~ 관련 누락 `.meta` 커밋으로 GUID 안정화(Import 시 참조 깨짐 리스크 감소)

- P0-2 완료: 스폰→Registry 추적 기반 구축 + 최소 테스트 통과
  - `SpawnerToEnemyRegistryBridge`: MonsterSpawner.OnSpawned/OnDespawned → Registry Register/Unregister 연결
  - AutoRegister가 자식에 붙는 구조 대응(루트 Transform 키 기반 Unregister 개선)

- P0-3 완료: 2D 클릭 타겟팅 + UX 규칙 확정
  - `TargetingController2D` + `Targetable2D` 기반 클릭 선택 구현/검증
  - UX: Esc=선택 클리어 / 우클릭=타겟팅 모드 종료(재진입 가능)

- P0-4 완료: 카드 루프(Strike/Heal) 완료 + 오동작 원인 해결
  - `GameACardPlayController` 적용/연결 누락으로 발생했던 타겟팅/힐 대상 꼬임 해결
  - Strike=타겟 필요 후 데미지 / Heal=타겟 없이 즉시 플레이어 회복(검증 완료)

- P0-5 완료: 웨이브 클리어 → 보상 드랍 → 픽업 → 인벤 반영(반복 웨이브 포함)
  - `GameAWaveRewardBridge`: WaveCleared 트리거로 TryDrop → PickupSpawned 구독 → LootPicked에서 인벤 반영
  - `LootTableDefinition` 엔트리/가중치(Weight) 세팅으로 TryDrop 성공 조건 확정
  - `LootPickup2D` 프리팹(Trigger/PlayerLayerMask/Rigidbody2D 조건) 세팅으로 픽업 이벤트 재현성 확보
  - `GameAInventoryCounter`로 픽업 카운트 증가 “증명” (초기값 문제로 Count 고정되던 이슈 해결)
  - `EnemyDeathHandler2D` 추가: HP=0 시 Enemy 루트 Disable → Registry 0 도달 → WaveCleared 정상 발화
  - `Health` 로직 정리: 데미지/힐 동작 및 이벤트 흐름 안정화
  - `GameAFlowCoordinator` 웨이브 진행 보완(TryStartNextWave 누락 수정)으로 반복 웨이브 재현
  - `@Monster` 프리팹 + `GameA_Main` 씬 wiring 업데이트(Import 환경에서 재현 확인)

- P0-6 완료: 문서/시연성(포트폴리오 필수)
  - `Samples~/GameA/Scripts/**` + `.meta` 전체 삭제 → SoT를 `Runtime/GameA/*`로 단일화, 중복/Import 컴파일 리스크 제거
  - `Samples~/GameA/README.md` 작성/추가 → Package Manager Samples Import 후 재현 가능한 가이드 제공
  - README에 Import 절차 + 10초 재현(Quick Start) 명시
  - README에 필수 세팅 명시:
    - `LootTableDefinition` Entries/Weight
    - `LootPickup2D` Trigger/Rigidbody2D/Collider2D 조건
    - (사용 시) Layer/LayerMask
  - README에 Hierarchy 기반 Wiring 체크리스트 명시: `@System/@Player/@UI/@SpawnPoints/MainCamera`
  - README에 Inspector 필수 참조(필드 라벨 → 연결 대상) 명시:
    - `GameAFlowCoordinator`(Wave/Spawner/Registry/EndCondition/Targeting, Debug keys/F1/F2)
    - `SpawnerToEnemyRegistryBridge`(Spawner↔Registry)
    - `TargetingController2D`(Camera/Target Mask)
    - `GameACardPlayController`(Targeting/Player Health, Strike/Heal config)
    - `GameAWaveRewardBridge`(EndCondition/WaveController/LootDropper/Inventory)
    - `LootDropper`(LootTable/PickupPrefab/Drop params)
  - README에 최소 테스트 5개 포함: Import 컴파일, 웨이브 루프, Registry 카운트, 드랍 생성, 픽업→카운터 반영

- 최소 테스트: 웨이브 클리어, 드랍 생성, 픽업, 카운트 증가, 반복 웨이브 전부 성공

### Cards (v0.1)
- `CardsModel` 주요 기능 구현 진행(Deck/Hand/Discard, Peek, Shuffle(Fisher–Yates), TryDraw, TryDiscard/TryPlay 등)
- `CardDefinition.cs` 직렬화 리네임 리스크 해결 완료:
  - `[SerializeField] private string displayName;` → `_displayName`으로 리네임
  - `FormerlySerializedAs("displayName")` 적용으로 기존 에셋 직렬화 값 유지
  - GitHub 업데이트 완료(마이그레이션 리스크 해소)

### Inventory (v0.1)
- Definition/Model/Grid 기반 설계 및 문서화 진행
- Add/Remove/Move/Query 명세(부분 성공, 이벤트 changedIndices 등) 확정 경험 기반 README 정리 완료

---

## 보류/리스크
- Git 상태에서 잡히는 불필요 파일(환경에 따라):
  - `Assets/TextMesh Pro/`, `Assets/Tests/`, `ProjectSettings/SceneTemplateSettings.json`, `nul` 등
  - 정책: 데모/테스트 목적이 아니라면 커밋 분리 또는 `.gitignore`로 관리
- Samples~ 폴더는 Project 창에서 기본적으로 숨겨질 수 있음:
  - 파일 탐색기/IDE에서 직접 구성 후 Import로 검증하는 방식 유지
- `Assets/Samples/...`는 Import 산출물(복제본)이라 커밋/수정 금지:
  - 정책: GitHub에는 “패키지 소스(특히 Packages/.../Samples~)”만 반영
  - 샘플 등록(`package.json` samples 엔트리)은 Import 재현성 목적일 때만 사용
- Import 환경 리스크(실제 이슈로 확인됨):
  - 특정 샘플(GameA)만 Import 시, 의존 샘플(Common/C2)이 Import되어 있지 않으면 타입 누락으로 컴파일 실패 가능
  - 해결: GameA 샘플 내에 “필요 공통 코드 번들링/구성 정리”로 단독 Import 실행 가능하도록 구성
- 검증/재현성 리스크(실패 원인으로 확인됨):
  - LootTableDefinition Entries 비어있으면 드랍이 발생하지 않음(테이블 세팅 체크 필수)
  - Player 레이어 미설정 시 LootPickup2D가 동작하지 않음(LayerMask/Layer 설정 체크 필수)
  - Assets/Samples(복제본)과 Packages/Samples~ 참조가 섞이면 연결이 꼬일 수 있음(패키지 소스 기준 고정)
- GameA EnemyAutoRegister2D(선택 A) 운용 리스크:
  - AutoRegister가 “자식”에 붙어 있으면 Registry 키 Transform 불일치로 Unregister not found 경고가 발생 가능
  - 해결: 루트 Transform 키로 Unregister하도록 개선(override/transform.root 사용)
- `Assets/Samples.meta` 등 Import/메타 파일이 untracked로 잡힐 수 있음:
  - 정책: 커밋 제외(원칙 유지), 필요 시 `.gitignore`로 명시 관리

---

## 결정사항(규칙/컨벤션)
### 공통 구조/패키지 규칙
- UPM 패키지 루트: `Packages/com.yesterfriday.gameplay-common-systems/`
- 모듈별 권장 구조:
  - `Runtime/<Module>/Definitions/`
  - `Runtime/<Module>/Definitions/Core/`
  - `Samples~/...` (Import 가능한 데모 씬/스크립트/프리팹)
  - `Documentation~/...` (명세/가이드 문서)

### 코드 컨벤션
- 네임스페이스:
  - `Yesterfriday.GameplayCommonSystems.<Module>`
- ScriptableObject Definition:
  - `[SerializeField] private` + `public` read-only 프로퍼티
  - `OnValidate()`에서 trim/warn 및 일부 값 보정(음수→0 등)
- API 스타일:
  - `Try*` 형태 우선
  - `bool` 반환: **상태 변화가 실제로 적용되었는지**(성공/부분성공 포함) 기준
  - 필요 시 `out`으로 처리 결과(예: added/removed/drawn) 제공
- 이벤트:
  - 변경 이벤트는 1차로 단순하게 제공(예: `OnInventoryChanged(changedIndices)`)
  - UI/세이브/네트워크 확장 포인트는 v0.2+로 분리

### Slingshot v0.1 규칙(확정)
- 2D 탑뷰, 월드 평면: `z = origin.z`
- Screen → World: Ray-Plane 교차 방식(Orthographic/Perspective 모두 호환)
- Pull: `pull = start - now`, `MaxPullDistance`로 클램프
- Impulse: `lerp(minImpulse, maxImpulse, clamp01(dist/maxDist))`
- ProjectilePrefab: `Rigidbody2D` 필수(없으면 실패)
- Cooldown: `Time.time < nextLaunchTime`이면 실패

---

## 다음 할 일(우선순위)
### P0 — GameA 1씬 MVP “루프 증명”
- (완료) P0-1: GameA 샘플 Import/컴파일 검증 + 씬 골격 기본 wiring
- (완료) P0-2: 스폰 → EnemyRegistry 추적 기반 구축(브릿지 + 안전장치) + 최소 테스트 통과
- (완료) P0-3: 클릭 타겟팅(2D) + UX(Esc 클리어 / 우클릭 종료)
- (완료) P0-4: 카드 → 타겟팅 → 효과 적용(Strike/Heal)
- (완료) P0-5: 웨이브 클리어 → 보상 드랍 → 픽업 → 인벤 반영(반복 웨이브 포함)

- (완료) P0-6: 문서/시연성(포트폴리오 필수)
  - `Samples~/GameA/Scripts/**` + `.meta` 전체 삭제 → SoT를 `Runtime/GameA/*`로 단일화, 중복/Import 컴파일 리스크 제거
  - `Samples~/GameA/README.md` 작성/추가 → Package Manager Samples Import 후 재현 가능한 가이드 제공
  - README에 Import 절차 + 10초 재현(Quick Start) 명시
  - README에 필수 세팅 명시:
    - `LootTableDefinition` Entries/Weight
    - `LootPickup2D` Trigger/Rigidbody2D/Collider2D 조건
    - (사용 시) Layer/LayerMask
  - README에 Hierarchy 기반 Wiring 체크리스트 명시: `@System/@Player/@UI/@SpawnPoints/MainCamera`
  - README에 Inspector 필수 참조(필드 라벨 → 연결 대상) 명시:
    - `GameAFlowCoordinator`(Wave/Spawner/Registry/EndCondition/Targeting, Debug keys/F1/F2)
    - `SpawnerToEnemyRegistryBridge`(Spawner↔Registry)
    - `TargetingController2D`(Camera/Target Mask)
    - `GameACardPlayController`(Targeting/Player Health, Strike/Heal config)
    - `GameAWaveRewardBridge`(EndCondition/WaveController/LootDropper/Inventory)
    - `LootDropper`(LootTable/PickupPrefab/Drop params)
  - README에 최소 테스트 5개 포함: Import 컴파일, 웨이브 루프, Registry 카운트, 드랍 생성, 픽업→카운터 반영
  - (선택) 10~20초 GIF/스크린샷: 웨이브 클리어→루트 드랍→픽업→인벤 반영

### P1 — GameA 품질/UX 강화
- (다음) P1-1: 타겟팅 UX 강화(가시성/실수 방지)
  - Hover/선택 대상 하이라이트(Outline/색상/스프라이트 교체 중 1) 적용
  - Target Mask 기반으로 유효 타겟만 선택(레이캐스트 필터/우선순위)
  - 취소 UX 명문화: Esc로 선택 해제, 우클릭으로 타겟팅 모드 종료(또는 동일 동작)
  - (선택) 가장 가까운 타겟/마우스 근접 타겟 우선 선택 규칙 추가

- (다음) P1-2: 웨이브/난이도 확장(“재미” 최소치)
  - 웨이브별 스폰 수/간격/종류(MonsterDefinition 리스트) 확장
  - 웨이브 진행 UI(현재 웨이브/남은 적/클리어 표시) 최소 1개 추가
  - (선택) 웨이브 간 딜레이/준비시간(프리-웨이브) 추가

- (다음) P1-3: 보상 테이블 다양화(의사결정 요소)
  - LootTableDefinition에 희귀도/가중치 프리셋(Common/Rare 등) 샘플 추가
  - 특정 조건(웨이브 N 이상/클리어 연속)에서 드랍 가중치 변화(간단 규칙)
  - (선택) 드랍 “연출” 1개(스폰 위치 튐/간단 애니메이션)

- (다음) P1-4: 카드 루프 확장(전략 요소 1개 추가)
  - 카드 1~2종 추가(예: Stun, Execute(체력 임계), Shield) 중 택1
  - 카드 사용 실패 사유(UI/로그) 통일(타겟 없음/쿨다운/자원 부족 등)
  - (선택) 카드 수치/설정 값을 SO로 분리해 “튜닝 가능”하게 만들기

### P2 — 재사용성/완성도 + GameB 착수
- GameA에서 검증된 컴포넌트 공통화/정리 → GameB 재사용
- Samples 허브/Documentation 체계화(목적/구성/체크리스트/확장 포인트)
- 간단 자동 테스트(EditMode/PlayMode) 추가로 회귀 방지
- GameB MVP 연결(Slingshot + MonsterSpawner + Cards 조합)