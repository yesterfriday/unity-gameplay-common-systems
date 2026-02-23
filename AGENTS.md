# Single Source of Truth (SoT)
- Treat these files as the only source of truth:
  - ./Packages/com.yesterfriday.gameplay-common-systems/Context.json
  - ./Packages/com.yesterfriday.gameplay-common-systems/STATE.md
- Do not invent APIs/fields/paths not present in SoT.
- If unclear, mark as [TBD] and list at the end.

# Project Constraints
- Unity 2022.3.62f3, C#, UPM: com.yesterfriday.gameplay-common-systems
- Namespace root: Yesterfriday.GameplayCommonSystems
- Try* bool rule: return true only if state actually changed (partial success included)
- Samples policy:
  - DO NOT edit/commit Assets/Samples/... (import artifacts)
  - Prefer changes under Packages/com.yesterfriday.gameplay-common-systems/...
- After changes:
  - Update STATE.md
  - Update Context.json only for highlights/status