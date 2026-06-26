# CtATracker — Agent Guide


## Environment
It is likely, that opencode is executed in the WSL environment. This can be verified by running
```bash
wslinfo --version
```
and checking the return code. At the start of the session run this check and store the information for later.


## Build & Test

```bash
dotnet build CtATracker.sln -c Debug
dotnet test CtATracker.Tests/CtATracker.Tests.csproj
```

Windows-only (`net8.0-windows`, WPF). No CI, no linter, no formatter config.

Note: if you are executing in the WSL environment, use `dotnet.exe` instead of `dotnet`. Also apply this change for later mentions of the `dotnet` command.

## Project Structure

- **`CtATracker/`** — WPF app (WinExe). Entrypoint `App.xaml` → `MainWindow.xaml`.
- **`CtATracker.Tests/`** — MSTest tests. Mock file handlers live alongside tests.
- **`config/ConfigLoader.cs`** — typed YAML config loader (static singleton, auto-creates defaults).
- **`secondary_windows/SkillTimerManager.cs`** — timer state, countdown logic, skill time calculations (extracted from SummaryWindow).
- **`UI element prefabs/NumericTextBoxBehavior.cs`** — shared numeric input validation helper (extracted from SkillEntryControl/SynergySkill).
- No MVVM; UI built imperatively (direct `Children.Add`, no data binding).

## Runtime Config

- **`Skills.yml`** — skill definitions with NCalc duration formulas. Copied to output dir. Uses `{level}` and `{SynergyName}` placeholders.
- **`Characters.yml`** — auto-created character data (name, skill levels, hard points, hotkeys, gamepad button, control scheme).
- **`Config.yml`** — auto-created runtime config (gamepad poll rate/timeout/threshold, keyboard flash duration, overlay timer resolution, character defaults, battle command/skill shrine bonuses, timer colors). Backed up to `.bak` if malformed.

## Test Quirks

- Tests load `TestData/Characters.yml` and `TestData/Skills.yml` from `AppContext.BaseDirectory` + `"TestData/"`.
- Test data uses `CamelCaseNamingConvention` matching the serializer.
- `CharacterFileHandlerMock` and `SkillFileHandlerMock` are in-memory stubs.

## Known Bugs

Consult these tickets before making unrelated changes — many touch the same files:

- currently no bugs known

## Gotchas

- `GetSkill()` returns a struct `Skill` (not nullable). Prefer `TryGetSkill()`.
- `SkillConfig` uses public fields, not properties.
- `SkillEntryControl` uses static fields (`_listeningButton`) — only one instance can capture a key at a time.
- Overlay window has hardcoded demo XAML timer entries that are never used (dynamically replaced).
- Character `Name` is also the dictionary key — renaming not supported without delete+recreate.
- `ConfigLoader` is a static singleton; `Instance` returns a default-config instance if `Load()` was never called (safe for tests).
- `SkillTimerManager` owns `_skillTimes` and all countdown/calculation logic. SummaryWindow delegates to it via `TriggerSkill()` and `DecrementTimers()`.
- `NumericTextBoxBehavior` methods are static — used by both `SkillEntryControl` and `SynergySkill` for input validation.


## Workflow & Git Guardrails
- **Branch Protection:** Under no circumstances are you allowed to commit or push directly to the `master` or `main` branches. 
- **Feature Branches:** Before making any code modifications, check the current git branch. If you are on `master` or `main`, you must use the bash tool to create and switch to a new feature branch prefixed with `agent/` (e.g., `agent/fix-wpf-binding`). You one branch per feature, not individual ones for phases in a feature.
- **Atomic Commits:** Make small, logical commits as you complete sub-tasks rather than one massive commit at the end.

- **No Unauthorized Dependencies:** You are strictly forbidden from modifying the `.csproj` file to add new NuGet packages unless explicitly requested by the human. Use native .NET framework features or existing project libraries only.

- **Build-Only Constraint:** You may run `dotnet build` to verify code correctness via the compiler. However, do not attempt to execute or run the WPF GUI application (`dotnet run`) inside this WSL environment, as it lacks a desktop display server. 
