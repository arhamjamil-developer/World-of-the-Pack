# World of the Pack — Official Unity Partner & Antigravity Configuration

## 1. Project Context & Stack
* **Game Title:** World of the Pack
* **Genre & Format:** 2D top-down card tactics and atmospheric wolf pack survival; portrait orientation (9:16 aspect target for iOS/mobile).
* **Engine Version:** Unity 6 (`6000.3.13f1`)
* **Render Pipeline:** Universal Render Pipeline (URP 2D, `17.3.0`)
* **Input System:** Unity Input System (`1.19.0`)
* **UI Systems:** uGUI (`2.0.0`) and UI Toolkit (`com.unity.modules.uielements`)
* **Target Platform:** iOS / Mobile

---

## 2. Official Unity Partner Integration (Unity CLI & Pipeline)
This project is connected directly via the **official Unity Technologies Partner integration** (`unity` CLI and `com.unity.pipeline` package), avoiding third-party MCP bridges.

* **Project Path:** `B:\World of the Pack`
* **Connection State:** Active via `com.unity.pipeline` on localhost (State: `ready`)
* **CLI Command Guidelines:**
  * Target project explicitly: `--project-path "B:\World of the Pack"`
  * Always pass `--caller plugin --skill <skill_name>` when invoking `unity command`.
  * **Check Editor status:** `unity status --project-path "B:\World of the Pack"`
  * **Recompile & check errors:** `unity recompile --project-path "B:\World of the Pack"`
  * **Play Mode control:** `unity command editor.playmode start --project-path "B:\World of the Pack"`
  * **Screenshots / Visual verification:** `unity command capture.screenshot --project-path "B:\World of the Pack"`
  * **Script Evaluation:** `unity command script eval --caller plugin --skill unity-cli '<C# Code>' --project-path "B:\World of the Pack"`

---

## 3. Core Gameplay & Architecture Conventions
* **Roster (Seven Lead Characters):**
  * Hero Wolvy, Princess Elva, Fairy Lulu, Captain Bobo, Scout Pip, Wizard Momo, Tinker Tilly.
* **Card & Unit Spawning:**
  * Persistent unit cards live in the player's hand (`GameCardHandCanvas`).
  * Each card maintains an independent energy meter recharged by combat triggers (defeating enemy soldiers).
  * Full energy enables unit deployment, resetting only that card's meter.
* **Camera & Viewport:**
  * Top-down 2D camera with subtle character tilt for readability.
  * Allied pack advances upward from screen bottom; enemies approach from the top.

---

## 4. C# & Unity Coding Standards
* **C# Version & Features:** Modern C# on .NET Standard 2.1 (Unity 6).
* **Naming Conventions:**
  * Types, methods, public properties, events: `PascalCase`.
  * Private/protected fields: `_camelCase` with `[SerializeField]` if exposed in the Inspector.
  * Constants & Enum values: `PascalCase`.
* **Performance Best Practices:**
  * Avoid `GameObject.Find()`, `FindObjectOfType()`, or `GetComponent()` calls inside `Update()` or hot loops. Cache references in `Awake()` or `Start()`.
  * Use object pooling or instantiate sparingly for frequent spawns.
  * Use `TryGetComponent` to avoid null reference exceptions and allocations.
* **UI Best Practices:**
  * Keep gameplay logic decoupled from UI display via C# events or observable state.
  * Support dynamic screen scaling for both iPhone and iPad aspect ratios.
