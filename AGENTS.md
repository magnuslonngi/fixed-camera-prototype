# Clean Game Architecture & Coding Guidelines

## Core Philosophy (The North Star)

- **Definition of Clean Code:** Code that is easy to understand and easy to modify.
- **Primary Goal:** Minimize cognitive load and debugging pain so complex game projects can scale to completion without collapsing under their own weight.
- **Pragmatism Over Dogma:** These guidelines are not unbreakable laws. Always ask: *"Does applying this rule make the code easier to understand and modify?"* If splitting a 15-line function into five 3-line functions makes the flow harder to trace, keep it together.

---

## 1. Clean Architecture Rules (`.agents/rules/architecture/`)

Enforced design principles across all game projects with implementations in **Unity (C#)**, **Godot (GDScript)**, and **Unreal Engine (C++)**:

1. [Naming Is Architecture](.agents/rules/architecture/naming-is-architecture.md)
   - Descriptive and expressive naming over brevity.
   - Zero abbreviations or acronyms (`distance`, not `dst`; `velocity`, not `vel`).
   - No single-letter variables outside localized loop iterators (`i`, `j`, `k`).
   - Strict per-engine casing and symbol conventions.

2. [Single Responsibility Principle (SRP)](.agents/rules/architecture/single-responsibility-principle.md)
   - Every class, script, node, and function does one thing and one thing only.
   - The "AND" heuristic: If describing what a method or class does requires "and", split it.
   - Granular, component-based decomposition (`Movement`, `Health`, `AudioPresenter`, `VisualPresenter`).

3. [Decoupling & Event-Driven Presentation](.agents/rules/architecture/decoupling-event-driven-presentation.md)
   - Strict separation of core gameplay logic from visual, audio, and UI systems.
   - Observer pattern (C# Events, GDScript Signals, Unreal Delegates) for state broadcasting.
   - The "Delete Test": Core logic must compile and run even if all UI, audio, and animations are removed.

4. [Information Hiding (Encapsulation)](.agents/rules/architecture/information-hiding-encapsulation.md)
   - Default all fields, variables, and helper methods to `private`.
   - Safe editor exposure (`[SerializeField] private`, `@export` with backing vars, `UPROPERTY` with `AllowPrivateAccess`).
   - Read-only getters with validated, authoritative mutation methods.

5. [Single Source of Truth (SSOT)](.agents/rules/architecture/single-source-of-truth.md)
   - Every piece of state exists in one place only; never cache duplicate copies across UI, audio, or save systems.
   - Establish clear authority classes (`PlayerHealth`, `InventorySystem`, `Wallet`).
   - Derive dependent values on demand rather than maintaining synchronized flags.

6. [Ban Magic Numbers & String Identifiers](.agents/rules/architecture/ban-magic-numbers-string-identifiers.md)
   - Replace unexplained numeric literals with descriptive constants.
   - Derive bounds and dimensions dynamically from authoritative sources.
   - Ban string-based scene lookups and tags; use strong types, interfaces, component queries, or pre-hashed IDs/GameplayTags.

7. [Composition Over Inheritance](.agents/rules/architecture/composition-over-inheritance.md)
   - Avoid deep inheritance trees (max depth 1-2).
   - Compose entities using attachable child nodes/components and capability interfaces (`IInteractable`, `IDamageable`, `ILockable`).

8. [Self-Documenting Code & Intentional Comments](.agents/rules/architecture/self-documenting-code-intentional-comments.md)
   - Extract dense conditions into clear boolean helper methods.
   - Delete redundant and obvious comments that parrot the code.
   - Comment strictly the "WHY" (architectural rationale, lifecycle traps, engine workarounds), never the "WHAT".

9. [Continuous Iterative Refactoring](.agents/rules/architecture/continuous-iterative-refactoring.md)
   - Follow the 3-step cycle: 1. Make it work -> 2. Make it clean -> 3. Iterate.
   - Perform the standard 5-step cleanup pass before completing any feature: rename, encapsulate, extract SRP, prune over-abstraction, eradicate dead code.

10. [Feature-First Folder Structure](.agents/rules/architecture/feature-first-folder-structure.md)
   - Organize by feature, not file type: `Features/<Feature>/` owns its scripts, prefabs/scenes, art, audio, data.
   - `Shared/` only for 2+ features; features never reference each other directly.
   - One project root separate from third-party code; global `Scenes/` for shipping scenes/levels only.
   - Per-feature `Sandbox/` is the mandatory home for every test scene, playground, and greybox asset.
   - Unity: one `.asmdef` per feature. Unreal: one game module with feature folders (modules only for plugins).

---

## 2. Language & Engine Writing Conventions (`.agents/rules/conventions/`)

Detailed syntax, casing, layout, and language-specific rules:

1. [Unity C# Conventions](.agents/rules/conventions/csharp-unity-conventions.md)
   - Official Unity style standards: `m_` prefix for private/serialized instance fields, `s_` for private statics, `k_` for constants.
   - Standard file layout, serialized field encapsulation, Allman braces.

2. [Godot GDScript Conventions](.agents/rules/conventions/gdscript-godot-conventions.md)
   - Official Godot 4.x style guide: `_` prefix for private members/helpers, `snake_case` functions/variables, `PascalCase` nodes/classes.
   - Script section ordering, strict static type annotations.

3. [Unreal Engine C++ Conventions](.agents/rules/conventions/cpp-unreal-conventions.md)
   - Official Epic Games UE5 coding standard: Type prefixes (`A`, `U`, `F`, `T`, `I`, `E`, `b`).
   - PascalCase everywhere, `TObjectPtr` for UObject members, standard header layout.

---

## 3. Editor Verification (Mandatory)

All verification happens by driving the running Unity Editor directly through the **`unity-pipeline` skill** (`.agents/skills/unity-pipeline/`, backed by the installed `com.unity.pipeline` package) and the **`unity-cli` skill**. Load the skill before any compile or Editor interaction.

1. **No Unit Tests**
   - Never write unit tests. Do not create EditMode/PlayMode test fixtures, `Tests/` folders, or test `.asmdef`s, and do not run `unity command test ...`.
   - Behavior is verified by interacting with the Editor, not by test code.

2. **Verify Inside the Editor Only**
   - Compile with `unity command script recompile` / `script recompile_status`.
   - Verify in the feature's Sandbox scene (`Features/<Feature>/Sandbox/<Feature>Sandbox.unity`): open it, enter Play Mode (`editor.playmode start`), inspect runtime state with `script eval`, capture screenshots, read the console, then `editor.playmode stop`.
   - Never verify by standalone `dotnet build`/`msbuild`, batch-mode Unity launches, or by reading code and assuming it works.
   - A change is not done until it compiles cleanly in the Editor and a Play Mode check in its Sandbox scene shows the expected behavior with no console errors.

3. **Use the Skills, Not Ad-Hoc Tools**
   - All Editor interaction (scene edits, running C#, inspecting objects, reading the console) goes through `unity-pipeline` commands (`script eval`, `script run`, `editor.playmode ...`).
   - Do not use the Unity MCP server or hand-edit `.unity`/`.prefab`/`.asset` YAML when a skill command can do it.
   - For tasks outside `unity-pipeline` scope (Editor/package/project management, API docs lookup), use the `unity-cli` skill.

4. **Sandbox Scenes, Never Global Scenes**
   - Every test scene, playground, and greybox material is created in `Features/<Feature>/Sandbox/`, never in the global `Scenes/` folder.
   - See [Feature-First Folder Structure](.agents/rules/architecture/feature-first-folder-structure.md).

5. **Editor Unreachable**
   - If the Editor or pipeline server is not reachable (`unity status --until-ready` fails), stop and ask the user to open the project. Do not fall back to out-of-Editor verification.

6. **Report Honestly**
   - Report actual Editor results: compile errors, console errors, and what Play Mode showed, with their output. Never claim something works without checking it in the Editor.
