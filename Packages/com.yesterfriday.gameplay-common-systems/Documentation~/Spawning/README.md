# README-Monster Spawner (v0.1)

> 요약
> 
> 
> 이 시스템은 Unity 프로젝트에서 **몬스터(프리팹)를 규칙에 따라 생성/관리**할 수 있는 **재사용 가능한 스포너 모듈**입니다.
> 
> v0.1에서는 **스폰 포인트 기반 생성**, **최대 동시 개체 수 제한**, **스폰 쿨다운**, **스폰/디스폰 이벤트 제공**까지를 핵심 범위로 하며, 이후 v0.2에서 **웨이브/가중치 테이블/오브젝트 풀링/네트워크 대응** 등으로 확장할 수 있도록 설계합니다.
> 

---

## Type

**Monster Spawner**

## Version

**v0.1**

---

## Goals

- 다른 Unity 프로젝트에 “바로 붙여쓰기” 가능한 형태(UPM 패키지 구조)로 제공
- 외부 에셋 없이, 스포너의 **핵심 규칙(생성/제한/쿨다운/이벤트)**을 명확히 구현
- v0.2+에서 웨이브/풀링/가중치 테이블 등 확장이 가능한 구조 확보

---

## Data Model

### Spawnable (MonsterDefinition)

- 몬스터는 **Prefab**(GameObject)로 스폰된다.
- 스폰 대상 정의는 ScriptableObject 기반을 권장한다.

**MonsterDefinition (권장 필드)**

- `Id: string` (고유 식별자)
- `DisplayName: string` (표시명)
- `Prefab: GameObject` (스폰 프리팹)
- (선택) `MaxAliveOverride: int?` (개별 몬스터 타입별 제한)
- (선택) `SpawnCost / Tags` 등은 v0.2+

### Spawn Points

- 스폰 위치는 **Transform 리스트**로 관리한다.
- 스폰 포인트 선택은 v0.1에서 단순 정책(Random/Sequential) 중 하나로 제공한다.

---

## Scope (v0.1)

- Spawn / Despawn
- Max Alive 제한(동시 존재 수)
- Spawn Cooldown
- Spawn Point 선택(Random 또는 Sequential)
- Events (스폰/디스폰/카운트 변경)

---

## Behavior Rules

### Spawn

- 입력: `MonsterDefinition` + (선택) spawn point index
- 생성 규칙:
    - `AliveCount >= MaxAlive`이면 Spawn 실패
    - 쿨다운 중이면 Spawn 실패
    - 성공 시 Prefab Instantiate
    - 스폰된 인스턴스는 내부에서 추적(Alive list)

### Despawn

- 입력: 스폰된 인스턴스(GameObject)
- 제거 규칙:
    - 추적 중인 인스턴스가 아니면 실패
    - 성공 시 Destroy(또는 v0.2에서 풀 반환)
    - Alive list에서 제거

### Cooldown

- 스폰 성공 시 `cooldownSeconds` 동안 다음 스폰 제한
- v0.1에서는 “스포너 단위 쿨다운”으로 단순화 (타입별 쿨다운은 v0.2 확장)

### Spawn Point Selection (v0.1)

- `Random`: 포인트 중 랜덤 선택
- `Sequential`: 0..N-1 순환 선택

---

## Failure Conditions (v0.1)

Spawn 실패:

- `definition == null`
- `definition.Prefab == null`
- `spawnPoints.Count == 0`
- `AliveCount >= MaxAlive`
- `cooldown` 미경과

Despawn 실패:

- `instance == null`
- 내부 추적 목록에 없는 인스턴스

---

## Events

v0.1에서는 확장 포인트를 위해 “핵심 이벤트”만 제공합니다.

- `OnSpawned(MonsterDefinition def, GameObject instance, int spawnPointIndex)`
- `OnDespawned(MonsterDefinition def, GameObject instance)`
- `OnAliveCountChanged(int aliveCount)`

> 이벤트 규약(권장): API 1회 호출당 이벤트는 필요한 만큼만 발행하되,
> 
> 
> UI/디버그 목적이라면 `OnAliveCountChanged`는 Spawn/Despawn 성공 시에만 호출.
> 

---

## API (v0.1)

> 실제 구현 시 네이밍은 프로젝트 컨벤션에 맞춰 조정 가능
> 
- `bool TrySpawn(MonsterDefinition def, out GameObject instance)`
- `bool TrySpawnAt(MonsterDefinition def, int spawnPointIndex, out GameObject instance)`
- `bool TryDespawn(GameObject instance)`
- `int GetAliveCount()`
- `IReadOnlyList<GameObject> GetAliveInstances()`

---

## Notes (Implementation Guidance)

- v0.1은 **Instantiate/Destroy** 기반으로 단순하게 구현하고, v0.2에서 오브젝트 풀링을 붙이는 구조가 가장 안전합니다.
- Alive 추적은 `List<GameObject>` + `Dictionary<GameObject, MonsterDefinition>` 같이 구성하면 디스폰 시 def를 빠르게 알 수 있어 이벤트/통계에 유리합니다.
- SpawnPointIndex가 필요한 이유:
    - 디버그/리플레이/로그에서 “어디서 스폰됐는지”를 추적 가능

---

## Sample (Demo)

- Package Manager → Samples → Import → `MonsterSpawner_Demo` 씬에서 동작 확인 *(추후 구체화)*
- 데모에서 확인할 것:
    - Spawn 버튼 → AliveCount 증가
    - MaxAlive 도달 시 Spawn 실패(로그/텍스트)
    - Despawn 버튼 → AliveCount 감소
    - Cooldown 중 Spawn 실패 확인

---

## Roadmap (v0.2+)

- Wave 시스템(라운드/시간/조건 기반)
- Spawn Table(가중치/확률)
- 오브젝트 풀링(Instantiate/Destroy 제거)
- NavMesh Agent 자동 초기화 옵션
- 네트워크(Authority/Spawn RPC) 대응 포인트
- Save/Load(현재 Alive 상태 저장)

---

## Changelog

- v0.1: Core spawner rules (spawn points, max alive, cooldown, events)