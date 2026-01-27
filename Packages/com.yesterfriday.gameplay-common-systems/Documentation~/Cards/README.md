# README — Cards (v0.1) 수정

## 요약

이 모듈은 Unity 프로젝트에서 **카드 덱·핸드·디스카드**의 핵심 동작(Reset/Shuffle/Draw/Discard/Play/Query)을 **재사용 가능한 UPM 패키지** 형태로 제공하는 공용 시스템입니다. v0.1에서는 **결정론적 셔플(Seed 기반)**과 **부분 성공(Partial Success) 드로우 규약**, 그리고 확장 지점을 위한 **단일 변경 이벤트**를 중심으로 설계합니다. 외부 에셋 없이 로직만 구현하며 v0.2에서 UI/세이브/네트워크 및 카드 효과 시스템을 확장합니다.

## Type

- Cards (Deck · Hand)

## Version

- v0.1

## Goals

- 다른 Unity 프로젝트에 **바로 붙여서 사용 가능한 형태(UPM 패키지)**로 제공
- 카드 시스템의 핵심(덱/핸드/디스카드) 동작을 **명세(규칙) 기반**으로 구현
- 셔플은 **결정론 옵션(Seed)**을 제공해 테스트/디버깅/리플레이에 유리하게 구성
- v0.2 확장을 고려해 **단일 변경 이벤트**로 UI/저장/네트워크 연결 포인트 확보
- 외부 에셋 없이 **핵심 기능만 컴팩트**하게 구현

## Data Model

- CardDefinition
    - Type: `ScriptableObject` (`CardDefinition`)
    - Purpose: 카드의 **불변(immutable) 메타데이터**(Id, DisplayName)
    - Identity Rule: 카드 식별은 **Id 기준**(또는 SO 레퍼런스 기준을 일관되게 유지)
- CardInstance (v0.1 minimal)
    - v0.1에서는 “한 장”을 **`CardDefinition` 레퍼런스**로 표현(런타임 상태 없음)
    - v0.2+에서 강화 가능: 업그레이드 레벨, 랜덤 값, 옵션/어픽스 등
- Collections / Zones
    - Deck: 드로우 더미(Top은 index 0)
    - Hand: 보유 카드
    - Discard: 버린 카드 더미

## Scope (v0.1)

- Reset (decklist로 초기화)
- Shuffle (Deck만, seed 기반 결정론 옵션)
- Draw (Deck → Hand)
- Discard (Hand → Discard)
- Play (Hand → Discard)
    - v0.1에서는 Discard와 동작 동일(효과 처리 제외)
- Query
    - `GetCount(zone)`
    - `Peek(n)` (Deck Top N, 상태 변경 없음)

## Behavior Rules

- Reset
    - `Reset(deckList)`는 다음을 수행
        - Deck = deckList 복사
        - Hand = empty
        - Discard = empty
    - 입력 리스트(deckList)를 직접 변경하지 않는다(복사 사용)
- Shuffle
    - Shuffle은 **Deck만** 섞는다(Hand/Discard 불변)
    - v0.1은 결정론 옵션 제공(예: `Shuffle(int seed)`)
    - 구현 권장: Fisher–Yates
- Draw
    - Draw는 **Deck → Hand** 이동
    - Deck이 부족할 때:
        - `allowReshuffleDiscard == false`: 가능한 만큼만 뽑고 종료(부분 성공 가능)
        - `allowReshuffleDiscard == true`:
            - Deck이 비면 Discard → Deck 이동 후 Shuffle, 그리고 계속 Draw 시도
    - 반환 규약(v0.1):
        - `drawn`은 실제로 뽑힌 장수
        - `true` iff `drawn > 0` (한 장이라도 뽑히면 성공)
- Discard / Play
    - Discard/Play는 **Hand → Discard** 이동
    - v0.1에서 Play는 의미만 다르고 동작은 Discard와 동일(효과 시스템 제외)
- Query
    - `GetCount(zone)`는 해당 존의 카드 수 반환
    - `Peek(n)`은 Deck의 Top에서 최대 N장을 반환(상태 변경 없음)

## Failure Conditions (v0.1)

- Reset
    - `deckList == null` → failure
    - `deckList`에 `null` 요소 포함 → failure
- Shuffle
    - `Deck.Count < 2` → no-op (변경 없음, `false` 반환 권장)
- Draw
    - `requested <= 0` → failure
    - 한 장도 뽑을 수 없음(Deck empty + 재셔플 불가/Discard empty) → failure (`drawn == 0`)
- Discard / Play
    - handIndex 범위 밖 → failure
    - (권장) 해당 인덱스 카드가 `null` → failure
- Peek
    - `n <= 0` 또는 Deck empty → empty list 반환(실패라기보단 조회 결과 없음)

## Events

- v0.1 권장: 단일 이벤트로 변경을 알림(확장 포인트)
    - `OnCardsChanged()`
- 이벤트 규약(권장)
    - 상태 변경이 적용된 경우에만 발행
    - 가능하면 **API 1회 호출당 이벤트 1회**(변경을 집계해 한 번에 통지)
- v0.2 확장 아이디어(참고)
    - payload 포함한 변경 정보(Zone, Operation, Delta 등)로 고도화 가능

## API (v0.1)

- `bool Reset(IReadOnlyList<CardDefinition> deckList)`
- `bool Shuffle(int seed)`
- `bool TryDraw(int requested, out int drawn, bool allowReshuffleDiscard = true)`
- `bool TryDiscard(int handIndex)`
- `bool TryPlay(int handIndex)`
- `int GetCount(CardsZone zone)`
- `IReadOnlyList<CardDefinition> Peek(int n)`

## Notes (Implementation Guidance)

- v0.1에서 의도적으로 제외한 범위
    - 타겟팅/효과/코스트/턴/에너지(마나) 시스템
    - UI 드래그 & 드롭
    - Save/Load 포맷
    - 네트워크 동기화 규칙
- 결정론 셔플(Seed)은
    - 테스트 자동화(예상 결과 비교)
    - 리플레이/디버깅
    - 데모 재현(발표에서 “같은 결과” 보여주기)
        
        에서 강력한 장점이 있음
        
- v0.1에서는 `CardDefinition` 레퍼런스만으로 “한 장”을 표현하고,
    
    v0.2에서 CardInstance를 확장하는 방식이 가장 안정적임
    

## Sample (Demo)

- Package Manager → Samples → Import → `Cards_Demo` 씬에서 동작 확인
- 데모에서 확인할 것
    - 고정 seed(예: 123) 셔플 후 Peek 결과 재현
    - Draw/Discard/Play 버튼으로 Deck/Hand/Discard 카운트 변화 확인
    - 변경 이벤트 발생 시 UI/로그 갱신

## Roadmap (v0.2+)

- CardInstance 런타임 상태 확장(업그레이드/랜덤 값/옵션)
- 효과 시스템(코스트, 타겟팅, 실행 스택)
- UI(드래그&드롭, 카드 선택/하이라이트)
- Save/Load(덱/핸드/디스카드 상태 직렬화)
- Network Sync(변경 이벤트 기반 델타 동기화)
- 덱 빌더/가중치/확률 드로우 등 고급 규칙

## Changelog

- v0.1: Cards core(Reset/Shuffle/Draw/Discard/Play/Query), seed 기반 셔플 옵션, 단일 변경 이벤트(확장 포인트)