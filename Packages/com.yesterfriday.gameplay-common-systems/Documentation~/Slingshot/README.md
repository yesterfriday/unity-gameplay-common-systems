# README — Slingshot (v0.1)

## 요약

Slingshot(v0.1)은 **2D 탑뷰(Top-Down)**에서 마우스/터치 드래그를 **월드 XY 평면의 Pull 벡터**로 변환해 발사체를 발사하는 재사용 모듈입니다. 입력(스크린 좌표) → 월드 좌표 매핑 규칙을 명확히 정의하고, 쿨다운/실패 조건/이벤트를 제공해 **다른 Unity 프로젝트에 바로 붙여서** 사용할 수 있도록 설계합니다.

## Type

- Slingshot (2D Top-Down Input-to-Launch Module)
- UPM Package 형태의 재사용 가능한 Runtime 컴포넌트 + Sample

## Version

- v0.1

## Goals

- 2D 탑뷰에서 “드래그(당김) → 발사” 입력을 표준화한 **재사용 가능한 발사 모듈**
- 스크린 입력을 **월드 XY 평면**으로 일관되게 매핑(카메라 종류에 무관한 규칙)
- v0.1에서는 핵심 기능(입력, 힘 계산, 발사, 쿨다운, 이벤트)만 제공
- v0.2+에서 에임 UI/예측 궤적/풀링/네트워크 등 확장 가능하도록 설계

## Data Model

- `SlingshotDefinition2D` (ScriptableObject)
    - `Id` (string)
    - `DisplayName` (string)
    - `ProjectilePrefab` (GameObject) — **Rigidbody2D 필수**
    - `MaxPullDistance` (float) — 월드 단위(유니티 유닛) 기준 최대 당김 거리
    - `MinImpulse` / `MaxImpulse` (float) — Rigidbody2D에 적용할 Impulse 범위
    - `CooldownSeconds` (float)
    - `MinPullDistance` (float) — 너무 짧은 드래그는 발사 실패 처리
    - `SpawnOffset2D` (Vector2) — Origin 기준 스폰 오프셋
- `SlingshotLauncher2D` (MonoBehaviour)
    - `Camera InputCamera` (기본: Main Camera)
    - `Transform Origin` — 발사 기준점(슬링샷 위치)
    - 내부 상태: Pull 세션 시작점/현재점/쿨다운 타이머
- Projectile Prefab 요구사항(v0.1)
    - `Rigidbody2D` 필수
    - `GravityScale = 0` (탑뷰에서 “떨어지는” 현상 방지)
    - `Collider2D`

## Scope (v0.1)

- Pointer Down/Drag/Up 기반 Pull 세션 처리(마우스/단일 터치)
- 스크린 좌표 → 월드 좌표(탑뷰 XY 평면) 매핑 규칙 제공
- Pull 벡터 클램프 + Impulse 계산 + `Rigidbody2D.AddForce(..., Impulse)` 발사
- 쿨다운 적용
- 최소 이벤트 제공(발사 성공/실패, Pull 변화)

## Behavior Rules

- **월드 평면과 입력 매핑 규칙(권장, v0.1 고정 규칙)**
    - 탑뷰 2D는 **월드 XY 평면에서 플레이**한다고 가정하고, Z는 고정값으로 취급한다.
    - 스크린 포인터 위치를 월드로 변환할 때는 카메라 종류(Orthographic/Perspective)에 관계없이 **Ray-Plane 교차**를 사용한다.
        - 평면: `z = Origin.position.z` (Origin이 있는 Z 높이)
        - `ray = InputCamera.ScreenPointToRay(screenPos)`
        - 교차점 `worldPos`를 구해 XY만 사용(혹은 Z는 평면 Z로 고정)
- **Pull 정의**
    - Pull 시작점 `pullStartWorld`는 `TryBeginPull` 시점의 월드 교차점
    - Pull 현재점 `pullNowWorld`는 드래그 중 포인터의 월드 교차점
    - Pull Vector는 **“당긴 방향”**을 의미하도록 다음을 권장:
        - `pull = pullStartWorld - pullNowWorld`
        - 즉, 사용자가 마우스를 오른쪽으로 드래그하면(pullNow가 오른쪽), `pull`은 왼쪽을 가리켜 “당김”이 된다.
- **클램프**
    - `pull.magnitude`는 `MaxPullDistance`를 넘지 않도록 클램프
    - `pull.magnitude < MinPullDistance`면 발사 실패 처리(의도치 않은 탭 방지)
- **힘(Impulse) 매핑**
    - `t = clamp01(pull.magnitude / MaxPullDistance)`
    - `impulse = lerp(MinImpulse, MaxImpulse, t)`
- **발사**
    - Projectile은 `Origin.position + (Vector3)SpawnOffset2D`에 Instantiate
    - `direction = pull.normalized`
    - `Rigidbody2D.AddForce(direction * impulse, ForceMode2D.Impulse)`
    - 발사 성공 시 쿨다운 시작: `nextLaunchTime = Time.time + CooldownSeconds`

## Failure Conditions (v0.1)

- `Definition == null` → failure
- `ProjectilePrefab == null` → failure
- `Origin == null` → failure
- `InputCamera == null` (MainCamera도 없음) → failure
- 쿨다운 중(`Time.time < nextLaunchTime`) → failure
- 스크린 좌표가 월드 평면과 교차하지 않음(레이가 평면과 평행 등) → failure
- `MaxPullDistance <= 0` 또는 `MaxImpulse <= 0` 등 파라미터가 유효하지 않음 → failure
- `pull.magnitude <= 0` 또는 `pull.magnitude < MinPullDistance`(설정 시) → failure
- Instantiate는 되었으나 `Rigidbody2D`가 없음 → v0.1에서는 **failure(즉시 Destroy 권장)**

## Events

- `OnPullChanged(Vector2 pullWorld, float normalizedPull)`
    - Pull 세션 중 드래그가 변할 때 호출(UI/에임 확장 포인트)
- `OnLaunched(SlingshotDefinition2D def, GameObject projectile, Vector2 direction, float impulse)`
    - 발사 성공 시 1회
- `OnLaunchFailed(SlingshotFailReason reason)`
    - 발사 실패 시 1회(실패 원인 전달)
- `OnCooldownChanged(bool isCoolingDown)`
    - 쿨다운 시작/종료 시점(발표/디버그에 유용)

## API (v0.1) — Suggested

- `bool TryBeginPull(Vector2 screenPosition)`
    - 성공: Pull 세션 시작(기준점 저장)
    - 실패: 카메라/Origin/쿨다운/교차 실패 등
- `bool TryUpdatePull(Vector2 screenPosition, out Vector2 pullWorld, out float normalizedPull)`
    - 성공: pullWorld/normalizedPull 갱신 + `OnPullChanged` 발행
- `bool TryEndPull(Vector2 screenPosition, out GameObject projectile, out float impulseApplied)`
    - 성공: 발사 실행 + 이벤트 발행 + 쿨다운 갱신
    - 실패: 조건 불만족 시 발사 없음
- `bool TryLaunch(Vector2 pullWorld, out GameObject projectile, out float impulseApplied)`
    - 입력 시스템과 분리해 “계산된 pull로 발사만” 수행하고 싶을 때
- `bool IsCoolingDown { get; }`

## FailReason Mapping (v0.1)

| FailReason | 의미 | 대표 원인/대응 |
|---|---|---|
| None | 실패 없음 | 정상 상태 |
| NullDefinition | Definition 미할당 | `SlingshotDefinition2D` 에셋을 Launcher에 연결 |
| NullPrefab | ProjectilePrefab 미할당 | Definition의 `ProjectilePrefab` 지정 |
| NullOrigin | Origin 미할당 | Launcher의 `Origin` Transform 연결 |
| NullCamera | InputCamera 미할당 | `InputCamera` 연결 또는 MainCamera 태그 확인 |
| CoolingDown | 쿨다운 중 | `Time.time < nextLaunchTime` / `CooldownSeconds` 확인 |
| NoPlaneHit | Screen→World 교차 실패 | Ray-Plane 교차 실패(카메라/Origin.z/평행) |
| NotPulling | Pull 세션이 없음 | Begin 없이 Update/End 호출 또는 Begin 실패 |
| PullTooSmall | 당김이 너무 짧음 | `pull.magnitude < MinPullDistance` → MinPullDistance 조정 |
| InvalidParams | 파라미터가 유효하지 않음 | `MaxPullDistance<=0`, `MaxImpulse<=0`, `MaxImpulse<MinImpulse` 등 |
| NoRigidbody2D | 프리팹에 Rigidbody2D 없음 | ProjectilePrefab에 Rigidbody2D 추가(없으면 발사체 Destroy 권장) |
| AlreadyPulling | 이미 Pull 중 | Pull 세션 중 Begin 재호출 방지 |

## Notes (Implementation Guidance)

- **Ray-Plane 교차를 고정 규칙으로 추천하는 이유**
    - Orthographic/ Perspective, 카메라 거리 변화에도 “스크린 → 월드”가 일관됨
    - `ScreenToWorldPoint`는 z 거리 인자가 필요해 카메라 셋업에 따라 실수가 잦음
- **탑뷰 2D에서 물리 안정성**
    - Projectile의 `Rigidbody2D.gravityScale = 0` 권장
    - Drag/Launch는 XY만 사용하고, Z는 고정(정렬용으로만 사용)
- **Material/이펙트/UI는 v0.1에서 제외**
    - 대신 `OnPullChanged`를 제공해 라인 렌더러/에임 UI는 v0.2+로 확장
- **테스트(최소)**
    - 쿨다운 중 발사 실패
    - `MaxPullDistance` 클램프 확인
    - impulse 매핑(Min/Max) 확인
    - Rigidbody2D 누락 시 실패 + 생성물 정리(Destroy)

## Sample (Slingshot2D_Demo)
> Demo 권장: `MinImpulse < MaxImpulse` (예: 2~10), `MaxPullDistance=3~6`, `MinPullDistance=0.1~0.3`

### Import & Run (재현 방법)

1. Package Manager에서 `com.yesterfriday.gameplay-common-systems` 추가
2. Package Manager > Samples에서 **Slingshot2D Demo** Import
3. 씬 열기: `Samples~/Slingshot2D_Demo/Scenes/Slingshot2D_Demo.unity`
4. Play → 드래그(당김) → 마우스/터치 릴리즈로 발사 확인
5. TMP UI에서 `normalizedPull / impulse / cooldown / lastFailReason` 값 변화 확인

### Scene 구성(핵심 오브젝트)

- **Launcher**
  - `SlingshotLauncher2D` (Runtime)
  - `Definition` : `SlingshotDefinition2D_Demo` 에셋
  - `Origin` : 발사 기준점 Transform
  - `InputCamera` : 보통 Main Camera
- **InputBridge**
  - `SlingshotDemoInputBridge2D` : Mouse/Touch → `TryBegin/Update/EndPull` 연결
- **UI (TMP)**
  - `SlingshotDemoUI` : 상태/FailReason/Impulse 표시
- **Visualizer**
  - `SlingshotDemoVisualizer2D`
  - Pull Line(막대형 LineRenderer) + Trajectory Dots(예측 점) 표시  
  - *AimLine은 사용하지 않음(궤적 점으로 방향/강도를 시각화)*

### Demo에서 확인할 것

- 드래그 길이에 따라 `normalizedPull`/`impulse`가 증가하는지
- `CooldownSeconds` 동안 연속 발사가 막히는지(`CoolingDown`)
- 드래그가 너무 짧으면 실패 처리되는지(`PullTooSmall`)
- Prefab에 `Rigidbody2D`가 없으면 실패 처리되는지(`NoRigidbody2D`)
- FailReason이 UI에 명확히 표시되는지(`LastFailReason`)

## Roadmap (v0.2+)

- Aim Line(라인 렌더러), Trajectory Preview(탑뷰용 예측 점선)
- 입력 추상화(마우스/터치/패드 통합), 멀티 카메라/멀티 터치 지원
- Projectile Pooling(Instantiate 비용 감소)
- 충돌/데미지/상태이상 등 발사체 확장 컴포넌트 체계
- 네트워크(서버 권위 발사, 예측/보정)
- 발사 모드 확장(차지, 홀드, 연사, 곡사/편차)

## Changelog

- v0.1
    - 2D 탑뷰 기준 스크린→월드(XY 평면) 매핑 규칙(Ray-Plane)
    - Pull 클램프 + Impulse 매핑 + Rigidbody2D 발사
    - 쿨다운/실패 조건/이벤트 제공