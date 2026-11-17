---
trigger: always_on
description: "Feature-First Folder Structure"
---

# Rule: Feature-First Folder Structure

Organize project files by **feature**, not by file type. Everything a feature needs (code, prefabs/scenes, art, audio, data) lives in one folder, so the feature can be understood, moved, or deleted as a unit. Folder structure is architecture: it should reveal what the game *does*, not which file extensions it uses.

---

## Core Guidelines

1. **One Project Root, Separate From Third-Party Code**
   - All of your own content lives under a single project root (`Assets/_Project/`, `res://`, `Content/<ProjectName>/`).
   - Downloaded packages, plugins, and marketplace assets stay outside it, in their engine-default location. Never edit or reorganize vendor folders; wrap them from your own code instead.

2. **`Features/<FeatureName>/` Owns Everything the Feature Needs**
   - A feature folder contains its scripts, prefabs/scenes, art, audio, and data assets.
   - Inside a feature, add type subfolders (`Scripts/`, `Art/`, ...) only once the feature has enough files to need them.
   - Name features after gameplay concepts (`Combat`, `Inventory`, `Dialogue`), never after file types or vague buckets (`Misc`, `Stuff`, `Managers`).

3. **`Shared/` Holds Only What 2+ Features Use**
   - Move something to `Shared/` when the *second* feature needs it, not before.
   - `Shared/` never depends on `Features/`. Dependencies flow one way: `Features → Shared`.

4. **Features Never Reference Each Other Directly**
   - Features communicate through interfaces and events defined in `Shared/` (see [Decoupling & Event-Driven Presentation](decoupling-event-driven-presentation.md) and [Composition Over Inheritance](composition-over-inheritance.md)).
   - **Delete Test:** removing a feature folder must not break compilation or other features.

5. **Shipping Scenes / Levels Are Global**
   - Shipping scenes and levels compose many features, so they live in one global folder (`Scenes/`, `levels/`, `Maps/`), not inside a feature.
   - The global folder holds shipping scenes only. Never put test scenes, playgrounds, or greybox assets there.

6. **Per-Feature `Sandbox/` for Test Scenes (Mandatory)**
   - Every test scene, playground, and prototype for a feature lives in that feature's `Sandbox/` folder, named `<Feature>Sandbox` (e.g. `Features/Combat/Sandbox/CombatSandbox.unity`).
   - Greybox materials and throwaway assets used only by the sandbox live in `Sandbox/Materials/` (or similar) beside it.
   - A sandbox scene may use other features' prefabs (e.g. the player) to exercise its feature.
   - Production code and content must never reference anything inside `Sandbox/`.
   - Add a Sandbox `.asmdef` only when the sandbox contains scripts.

---

## Examples

### 1. Unity (C#)

```text
Assets/
├── _Project/                         # Underscore sorts it to the top; all project content
│   ├── Scenes/                       # Global shipping scenes only: Boot, MainMenu, Level01
│   ├── Shared/
│   │   ├── Scripts/
│   │   │   └── MyGame.Shared.asmdef
│   │   ├── Art/
│   │   └── Audio/
│   └── Features/
│       ├── Combat/
│       │   ├── Scripts/
│       │   │   ├── MyGame.Combat.asmdef          # References MyGame.Shared only
│       │   │   └── Editor/
│       │   │       └── MyGame.Combat.Editor.asmdef
│       │   ├── Prefabs/
│       │   ├── Art/
│       │   ├── Audio/
│       │   ├── Data/                 # ScriptableObjects
│       │   └── Sandbox/
│       │       ├── CombatSandbox.unity           # Test scene for this feature
│       │       ├── Materials/                    # Greybox materials used only here
│       │       └── MyGame.Combat.Sandbox.asmdef  # Only if sandbox has scripts; nothing references it
│       └── Inventory/
└── ThirdPartyPlugin/                 # Vendor content stays untouched (or in Packages/)
```

- **Assembly Definitions are required:** one `.asmdef` per feature (`<Project>.<Feature>`) plus one for `Shared`. The compiler then enforces the dependency rules: a feature asmdef references `Shared` only, never another feature.
- Editor-only code goes in an `Editor/` folder with its own `<Project>.<Feature>.Editor.asmdef` (Editor platform only).
- No spaces in file or folder names; use `PascalCase`.

```csharp
// ❌ BAD: Combat reaches directly into Inventory (cross-feature reference; fails the Delete Test)
using MyGame.Inventory;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private InventorySystem m_inventory;
}

// ✅ GOOD: Combat raises an event channel defined in Shared; Inventory listens to it
using MyGame.Shared;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private ItemEventChannel m_itemPickedUpChannel; // ScriptableObject in Shared
    [SerializeField] private ItemDefinition m_itemDefinition;

    private void OnTriggerEnter(Collider other)
    {
        m_itemPickedUpChannel.Raise(m_itemDefinition);
    }
}
```

### 2. Godot (GDScript)

```text
res://
├── levels/                           # Global levels/scenes
│   └── forest/forest.tscn
├── shared/
│   ├── components/health_component.gd
│   └── ui/
├── features/
│   ├── combat/
│   │   ├── weapon.tscn               # Scene, script, and assets side by side
│   │   ├── weapon.gd
│   │   ├── weapon_sprite.png
│   │   └── sandbox/
│   │       └── weapon_test.tscn
│   └── inventory/
└── addons/                           # Third-party plugins and assets
```

- Per the official Godot guide, keep assets **as close to the scenes that use them as possible**.
- `snake_case` for every folder and file (avoids case-sensitivity export bugs); C# scripts are the exception and match their `PascalCase` class name.
- Third-party content goes in `addons/`. An empty `.gdignore` stops Godot from importing a folder (e.g. `docs/`).

### 3. Unreal Engine (C++)

```text
MyGame/
├── Content/
│   ├── MyGame/                       # All project content
│   │   ├── Maps/                     # Global levels
│   │   ├── Shared/
│   │   └── Features/
│   │       ├── Combat/
│   │       │   ├── BP_Weapon_Rifle
│   │       │   ├── SM_Weapon_Rifle
│   │       │   ├── T_Weapon_Rifle_BaseColor
│   │       │   └── Sandbox/
│   │       └── Inventory/
│   └── MarketplacePack/              # Vendor content stays at the Content root
├── Source/
│   └── MyGame/                       # Single game module; folders only
│       ├── Shared/{Public,Private}/
│       └── Features/
│           ├── Combat/{Public,Private}/
│           └── Inventory/{Public,Private}/
└── Plugins/
    └── MyPlugin/Source/MyPluginModule/{Public,Private}/
```

- Game C++ uses **one module with feature folders**. Create separate modules only when developing plugins (`Plugins/<Plugin>/Source/<Module>/`).
- Asset names follow Epic's `Prefix_BaseName_Descriptor_Variant` pattern (`BP_`, `SM_`, `T_`, `M_`, `MI_`).
- No spaces or special characters in asset or folder names.

---

## Anti-Patterns

- ❌ Type-first project roots: `Scripts/`, `Prefabs/`, `Textures/` at the top, with one feature scattered across all of them.
- ❌ `Shared/` (or `Common/`, `Utils/`) growing into a dumping ground for code only one feature uses.
- ❌ Your own files inside vendor folders, or vendor files moved into `Features/`.
- ❌ Production code referencing a `Sandbox/` asset.
- ❌ Test scenes, playgrounds, or greybox materials in the global `Scenes/` folder instead of `Features/<Feature>/Sandbox/`.
- ❌ Unit test folders (`Tests/`) or test assemblies; verification is done by driving the Editor in the Sandbox scene.
- ❌ A level or scene placed inside one feature's folder when it composes several features.
