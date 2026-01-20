# README

**Type: Cards (Deck · Hand)**

**Version: v0.1**

## Overview

이 모듈은 Unity 프로젝트를 위한 재사용 가능한 **카드 툴킷**을 제공하며, 최소한의 결정론적 핵심에 중점을 둡니다: **카드 정의(데이터)**, **덱(뽑기/버리기)**, **핸드(보유/플레이)**.

**UPM 패키지 재사용**을 위해 설계되었으며, **외부 애셋 없이** 구현되었고, 향후 버전에서 UI, 저장/로드, 네트워킹을 위해 쉽게 확장 가능한 API를 제공합니다.

---

## Data Model

### CardDefinition

- **Type**: `ScriptableObject` (`CardDefinition`)
- **Purpose**: Immutable card metadata (ID, display name, optional tags/costs later)
- **Identity rule**: A card is identified by its `Id` (or by ScriptableObject reference consistently)

### CardInstance (v0.1 minimal)

- Represents “one copy” of a card in a deck/hand.
- v0.1 default: the instance is just a reference to `CardDefinition` (no runtime state).
- v0.2+ can extend with runtime fields (upgrade level, rolled values, affixes, etc.)

### Collections

- **Deck**: main draw pile
- **Discard**: discard pile
- **Hand**: cards currently held

---

## Scope (v0.1)

- Deck build/reset from a list (initialization)
- Shuffle (deterministic option recommended)
- Draw (Deck → Hand)
- Discard (Hand → Discard)
- Play (Hand → Discard) (semantically same as discard in v0.1)
- Query
    - counts (deck/hand/discard)
    - contains checks
    - peek (top N of deck)

---

## Behavior Rules

### Initialization

- `Reset(deckList)` sets:
    - Deck = deckList (copied)
    - Hand = empty
    - Discard = empty
- Reset does not mutate the source list.

### Shuffle

- Shuffle affects **Deck only**.
- v0.1 requires a deterministic implementation option:
    - Provide `Shuffle(int seed)` or accept an injected RNG.
- Shuffle does not change Hand/Discard.

### Draw

- Draw moves cards **Deck → Hand**.
- If `Deck` has insufficient cards:
    - If `allowReshuffleDiscard` is **false** → partial success
    - If `allowReshuffleDiscard` is **true**:
        - When Deck is empty, move Discard → Deck, shuffle, continue drawing
- Return rule (v0.1):
    - Return `true` if at least one card was drawn
    - Output `drawn` indicates the number of cards actually drawn

### Discard / Play

- Discard/Play moves cards **Hand → Discard**.
- Failure conditions:
    - Invalid hand index
    - Hand is empty at index
- v0.1: Play is a semantic alias of discard (no effect resolution included)

### Query

- `GetCount(zone)` returns the total cards in a zone.
- `Peek(n)` returns up to N cards from the top of deck (does not mutate).

---

## Failure Conditions (v0.1)

- **Reset**
    - `deckList == null` → failure
- **Shuffle**
    - Deck count < 2 → no-op (still valid)
- **Draw**
    - requested <= 0 → failure
    - cannot draw any card (Deck empty and reshuffle disabled, or all zones empty) → failure
- **Discard/Play**
    - invalid hand index → failure

---

## Events

### OnCardsChanged(IReadOnlyList changes)

v0.1 provides a single event to support UI/Save/Network as extension points.

- `CardsChange` includes:
    - `Zone` (Deck/Hand/Discard)
    - `Operation` (Reset/Shuffle/Draw/Discard/Play)
    - `CountDelta` (optional)
- Event fires **once per API call** with aggregated changes.

> Note: v0.1 may simplify this to OnCardsChanged() with no payload; v0.2 can extend to structured change info.
> 

---

## API (v0.1)

### Core

- `bool Reset(IReadOnlyList<CardDefinition> deckList)`
- `bool Shuffle(int seed)`
- `bool TryDraw(int requested, out int drawn, bool allowReshuffleDiscard = true)`
- `bool TryDiscard(int handIndex)`
- `bool TryPlay(int handIndex)`
- `int GetCount(CardsZone zone)`
- `IReadOnlyList<CardDefinition> Peek(int n)`

### Zones

- `CardsZone.Deck`
- `CardsZone.Hand`
- `CardsZone.Discard`

---

## Notes

- v0.1 intentionally excludes:
    - targeting, effects, costs, turns, energy/mana systems
    - UI drag & drop
    - save/load formats
    - networking replication rules
- These are intended as v0.2+ modules built on top of the stable core.

---

## Sample

- Package Manager → Samples → Import → `Cards_Demo` scene (planned)
- Demo includes:
    - Create a starter deck (ScriptableObjects)
    - Shuffle with fixed seed
    - Draw / Discard / Play buttons
    - Logs for deck/hand/discard counts and change events