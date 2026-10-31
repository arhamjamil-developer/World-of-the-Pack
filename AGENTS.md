# Agent Operating Guidelines — World of the Pack

Follow the project directives in [GEMINI.md](./GEMINI.md).

### Directives for Official Unity Integration
- **Interaction Method:** Use the official Unity CLI (`unity command`, `unity recompile`, `unity status`) driven by `com.unity.pipeline`.
- **Target Project:** Always pass `--project-path "B:\World of the Pack"` (or set `UNITY_PROJECT_PATH`).
- **Compilation Check:** Always verify with `unity recompile` after writing C# scripts before attempting Play Mode.
- **Scene & Asset Safety:** Respect existing asset hierarchies (`Assets/Characters`, `Assets/UI/WorldOfThePack`, `Assets/Visuals`, `Assets/Scenes`).
