# Fixed Camera Prototype

A survival-horror prototype in Unity built around **fixed camera angles**: authored shots that cut as the player moves between zones. It includes camera-relative and tank-style movement, aiming and shooting with aim assist, zombies with perception and idle behaviors, inventory with item examine, readable documents, interactions, locked doors, and a full main menu -> level -> pause loop.

The codebase follows a strict set of architecture rules (see [Coding Rules](#coding-rules)). They are written for both human and AI-agent contributors.

---

## Requirements

| Item | Version |
| :--- | :--- |
| Unity Editor | `6000.3.28f1` (Unity 6.3) |
| Render pipeline | URP `17.3.0` |
| Input | Input System `1.20.1` |
| Camera | Cinemachine `3.1.7` |
| Navigation | AI Navigation `2.0.16` |
| Editor automation | `com.unity.pipeline` `1.0.0` (used by agent skills) |

## Getting Started

1. Clone the repo and open the folder in Unity Hub with Editor `6000.3.28f1`.
2. Open `Assets/_Project/Scenes/MainMenu.unity` and press Play, or open `Level01_Refuge.unity` to start in the level directly.
3. Make sure both scenes are in Build Settings so the menu can load the level.

## Controls

| Action | Keyboard | Gamepad |
| :--- | :--- | :--- |
| Move | WASD | Left stick |
| Tank move / aim adjust | Arrow keys | - |
| Sprint | Left Shift | Left stick press |
| Aim | Q | - |
| Fire | Space | - |
| Reload | R | - |
| Interact | E | North button |
| Inventory | Tab | Select |
| Pause | Escape | Start |

Bindings live in `Assets/_Project/Shared/Settings/InputSystem_Actions.inputactions`.

---

## Project Structure

All project content lives under `Assets/_Project/`. It is organized **by feature**, not by file type. Third-party assets stay at the `Assets/` root and are never edited.

```text
Assets/
├── _Project/
│   ├── Scenes/                  # Shipping scenes only
│   │   ├── MainMenu.unity
│   │   └── Level01_Refuge.unity
│   ├── Shared/                  # Code and assets used by 2+ features
│   │   ├── Scripts/             # FixedCameraPrototype.Shared.asmdef
│   │   ├── Data/                # Shared event channel assets
│   │   ├── Settings/            # URP assets, volume profiles, input actions
│   │   ├── Shaders/             # PackedLit shader graphs
│   │   └── UI/                  # Runtime theme, shared USS
│   └── Features/
│       └── <Feature>/
│           ├── Scripts/         # FixedCameraPrototype.<Feature>.asmdef
│           ├── Prefabs/
│           ├── Data/            # ScriptableObjects (items, event channels)
│           ├── UI/              # UXML / USS / PanelSettings
│           ├── Animations/, Audio/, Vfx/
│           └── Sandbox/         # <Feature>Sandbox.unity + throwaway assets
├── CyberCity/, CyberGirl/, Kevin Iglesias/, NewPunch/,
│   FImpossible Creations/, Sci-fi Pistol/, Scifi Guns SFX Pack/   # Third-party, untouched
.agents/
├── rules/architecture/          # Architecture rules (source of truth)
├── rules/conventions/           # Per-engine coding conventions
└── skills/                      # Agent skills (unity-pipeline, unity-cli, ...)
AGENTS.md                        # Agent entry point (CLAUDE.md symlinks to it)
```

### Features

| Feature | Responsibility | Key scripts |
| :--- | :--- | :--- |
| **FixedCamera** | Picks the live shot from trigger zones; broadcasts camera cuts. Most recently entered zone wins. | `CameraZone`, `CameraZoneDirector`, `CameraCutBroadcaster` |
| **PlayerCharacter** | Movement, rotation, stamina, health HUD, hurt stagger, death. Camera-relative input is latched across cuts so the player does not reverse direction. | `PlayerMovement`, `CameraRelativeInputBasis`, `InputBasisLatch`, `PlayerStamina` |
| **Combat** | Aiming, aim assist with tiered targeting, firing, magazine and reload, shot recovery, and presenters for audio, muzzle flash, blood, and animation. | `PlayerAim`, `AimAssist`, `PlayerWeapon`, `WeaponMagazine`, `PlayerReload` |
| **Enemy** | Zombie state machine (idle, chase, attack, stagger, dead), perception, NavMesh locomotion, hit zones, pluggable idle behaviors (stationary, patrol, wander). | `EnemyBrain`, `EnemyPerception`, `EnemyLocomotion`, `IEnemyIdleBehavior` |
| **Interaction** | Finds nearby interactables, picks the target, shows indicators, and runs pick-up, inspect, read, and item-lock interactions. | `PlayerInteractor`, `InteractionTargetSelector`, `Interactable`, `ItemLockInteraction` |
| **Inventory** | Item storage, inventory screen, item selection and examine with 3D previews. | `PlayerInventory`, `InventoryScreenHud`, `ItemExamineSession`, `ItemPreviewStage` |
| **Reading** | Full-screen document reader for notes. | `DocumentReader`, `DocumentReaderHud` |
| **LevelFlow** | Level scripting through event channels: activate objects on gameplay events, level-complete trigger, reload on player death. | `ActivateOnGameplayEvent`, `LevelCompleteTrigger`, `ReloadSceneOnPlayerDeath` |
| **MainMenu** | Title menu session and HUD. | `MainMenuSession`, `MainMenuHud` |
| **PauseMenu** | In-game pause menu. | `PauseMenuSession`, `PauseMenuHud` |

### Shared Layer

`Shared/` holds the contracts that let features talk without referencing each other:

- **Capability interfaces:** `IDamageable`, `IAimAssistTarget`, `IPursuitTarget`, `IItemReceiver`, `IItemStock`, `IItemUseTarget`, `IDocumentReader`, `IModalScreen`, and others.
- **`CapabilityRoot`:** marks the top of an entity. Features find each other's components through it by interface, wherever they sit in the hierarchy.
- **Event channels:** `GameplayEventChannel` and `CameraCutEventChannel` are ScriptableObject assets. One feature raises, another listens.
- **Shared authorities:** `Health`, `ModalStack` (which full-screen screen is open; pauses the game and locks control), `GamePause`, `CharacterControlLock`, `MovementLock`.
- **Data:** `ItemDefinition`, `SceneReference`, menu helpers (`MenuOptionCursor`, `MenuOptionListView`, `MenuInputReader`).

### Assembly Dependencies

Every feature has its own `.asmdef`, and each one references only `FixedCameraPrototype.Shared` plus Unity packages. The compiler enforces that features never reference each other.

```text
FixedCameraPrototype.<Feature>  ──►  FixedCameraPrototype.Shared  ──►  Unity packages
```

---

## Coding Rules

The full rules, with Unity, Godot, and Unreal examples, live in [`.agents/rules/`](.agents/rules). [`AGENTS.md`](AGENTS.md) is the entry point.

**North star:** clean code is code that is easy to understand and easy to modify. The rules serve that goal and are not dogma. If applying a rule makes code harder to follow, don't apply it.

### Architecture Rules

#### 1. [Naming Is Architecture](.agents/rules/architecture/naming-is-architecture.md)
Names state intent and ownership.
- Descriptive and verbose names. No abbreviations or acronyms (`distance`, not `dst`).
- No single-letter variables except `i`/`j`/`k` in short loops.
- No generic names (`DoStuff`, `HandleData`, `Manager`).
- Booleans read as `is`/`has`/`can`; events are `On...`.

#### 2. [Single Responsibility Principle](.agents/rules/architecture/single-responsibility-principle.md)
Each class and function does one thing.
- **"AND" test:** if describing it needs "and", split it.
- Entities are built from focused components (`PlayerMovement`, `PlayerStamina`, `WeaponAudioPresenter`), never a god object.

#### 3. [Decoupling & Event-Driven Presentation](.agents/rules/architecture/decoupling-event-driven-presentation.md)
Logic never drives presentation directly.
- Logic raises C# events. Presenters (`*Presenter`, `*Hud`) subscribe and handle visuals, audio, and UI.
- **Delete test:** removing all UI, audio, and animation must still leave the logic compiling and running.

#### 4. [Information Hiding](.agents/rules/architecture/information-hiding-encapsulation.md)
Keep the public surface minimal.
- Everything is `private` by default. Inspector exposure uses `[SerializeField] private`.
- Expose read-only properties. Mutate state only through validated methods (`ApplyDamage`, `TryConsume`).

#### 5. [Single Source of Truth](.agents/rules/architecture/single-source-of-truth.md)
Each piece of state lives in exactly one place.
- Clear authorities (`Health`, `PlayerInventory`, `ModalStack`, `CameraZoneDirector`).
- Presenters never cache copies. They read the authority or use the event payload.
- Derive dependent values (`IsDead => m_currentHealth <= 0`) instead of storing flags.

#### 6. [Ban Magic Numbers & String Identifiers](.agents/rules/architecture/ban-magic-numbers-string-identifiers.md)
- Every literal becomes a named `k_` constant or a serialized field with units in its name (`m_rangeMeters`).
- No `GameObject.Find`, tags, or string lookups. Use serialized references, `TryGetComponent`, interfaces, and LayerMasks.
- Strings the engine requires (Animator, shader properties) are pre-hashed into `s_` statics.

#### 7. [Composition Over Inheritance](.agents/rules/architecture/composition-over-inheritance.md)
- Inheritance depth is at most 1-2 levels.
- Build entities from components. Interact through small capability interfaces (`IDamageable`, `IItemUseTarget`, `IEnemyIdleBehavior`).

#### 8. [Self-Documenting Code & Intentional Comments](.agents/rules/architecture/self-documenting-code-intentional-comments.md)
- Pull dense conditions out into named predicates (`CanBeStaggered`).
- Delete comments that repeat the code.
- Comment only the **why**: design rationale, lifecycle traps, engine workarounds.

#### 9. [Continuous Iterative Refactoring](.agents/rules/architecture/continuous-iterative-refactoring.md)
Make it work, then make it clean, then iterate. Before a feature is done, do a 5-step cleanup pass:
1. Rename
2. Privatize and encapsulate
3. Extract single responsibilities
4. Remove over-abstraction
5. Delete dead code, unused `using`s, and debug logs

#### 10. [Feature-First Folder Structure](.agents/rules/architecture/feature-first-folder-structure.md)
- `Features/<Feature>/` owns everything the feature uses.
- Something moves to `Shared/` only once a second feature needs it.
- Features never reference each other. One `.asmdef` per feature.
- `Scenes/` is for shipping scenes only. Test scenes go in `Features/<Feature>/Sandbox/`.
- PascalCase names, no spaces.

### Unity C# Conventions

From [`csharp-unity-conventions.md`](.agents/rules/conventions/csharp-unity-conventions.md):

| Identifier | Convention | Example |
| :--- | :--- | :--- |
| Private / serialized fields | `m_` + camelCase | `[SerializeField] private float m_walkSpeed;` |
| Private static fields | `s_` + camelCase | `s_overlapResults` |
| Constants | `k_` + camelCase | `k_maximumOverlapResults` |
| Properties, methods, types | PascalCase | `CurrentState`, `ApplyDamage()` |
| Parameters, locals | camelCase | `deltaTime`, `hitCollider` |
| Interfaces | `I` + PascalCase | `IDamageable` |
| Events | `On` + PascalCase | `OnStateChanged` |
| Namespace | Matches asmdef | `FixedCameraPrototype.Combat` |

**Class layout order:**
1. Constants and statics
2. Serialized fields
3. Private fields
4. Properties
5. Events
6. Unity lifecycle methods
7. Public methods
8. Private methods

**Formatting:** Allman braces, explicit access modifiers, attributes on their own line.

Conventions for Godot and Unreal are also included in [`.agents/rules/conventions/`](.agents/rules/conventions), so the same rules carry over to other engines.

---

## Verification Workflow

This project has **no unit tests**. All behavior is checked in the running Unity Editor.

1. **Compile in the Editor** (through the `unity-pipeline` skill):
   `unity command script recompile` and `script recompile_status`.
2. **Open the feature's Sandbox scene**, e.g. `Features/Combat/Sandbox/CombatSandbox.unity`.
3. **Enter Play Mode** (`editor.playmode start`). Inspect runtime state with `script eval`, take screenshots, and read the console.
4. **Stop Play Mode** (`editor.playmode stop`).

A change is done only when it compiles cleanly and the Sandbox Play Mode check shows the expected behavior with no console errors. Don't use standalone `dotnet build`, batch-mode launches, or test assemblies to verify.

### Sandbox Scenes

| Feature | Scene |
| :--- | :--- |
| Combat | `Features/Combat/Sandbox/CombatSandbox.unity` |
| Enemy | `Features/Enemy/Sandbox/EnemySandbox.unity` |
| FixedCamera | `Features/FixedCamera/Sandbox/FixedCameraSandbox.unity` |
| Interaction | `Features/Interaction/Sandbox/InteractionSandbox.unity` |
| Inventory | `Features/Inventory/Sandbox/InventorySandbox.unity` |
| LevelFlow | `Features/LevelFlow/Sandbox/LevelFlowSandbox.unity` |
| MainMenu | `Features/MainMenu/Sandbox/MainMenuSandbox.unity` |
| PauseMenu | `Features/PauseMenu/Sandbox/PauseMenuSandbox.unity` |
| Reading | `Features/Reading/Sandbox/ReadingSandbox.unity` |

---

## Adding a Feature

1. Create `Assets/_Project/Features/<Feature>/Scripts/` with `FixedCameraPrototype.<Feature>.asmdef`. It should reference `FixedCameraPrototype.Shared` only, plus any Unity packages it needs.
2. Use the namespace `FixedCameraPrototype.<Feature>`.
3. If the feature needs to talk to another feature, add an interface or event channel to `Shared/`. Never reference the other feature directly.
4. Create `Features/<Feature>/Sandbox/<Feature>Sandbox.unity` and verify there.
5. Do the 5-step cleanup pass before opening a PR.
