---
trigger: glob
globs: "**/*.gd"
description: "Godot 4 GDScript naming, ordering and typing conventions."
paths:
  - "**/*.gd"
---

# Rule: Godot GDScript Coding & Writing Conventions

These conventions adhere strictly to the official Godot Engine GDScript Style Guide for Godot 4.x.

---

## 1. Naming Conventions

| Identifier Type | Convention | Example |
| :--- | :--- | :--- |
| **Files & Scenes** | `snake_case` | `player_character.gd`, `health_bar_ui.tscn` |
| **Classes & Nodes** | `PascalCase` | `class_name PlayerCharacter extends CharacterBody3D` |
| **Functions & Methods** | `snake_case` | `apply_damage()`, `calculate_path()` |
| **Private Functions & Callbacks** | `_` prefix + `snake_case` | `_update_stamina()`, `_on_body_entered()` |
| **Public Variables** | `snake_case` | `movement_speed`, `interaction_range` |
| **Private Member Variables** | `_` prefix + `snake_case` | `_current_health`, `_is_reloading` |
| **Constants** | `CONSTANT_CASE` | `MAX_INVENTORY_CAPACITY`, `GRAVITY_FORCE` |
| **Enums (Type Name)** | `PascalCase` | `enum WeaponType { PISTOL, RIFLE, SHOTGUN }` |
| **Enum Values** | `CONSTANT_CASE` | `PISTOL`, `RIFLE`, `SHOTGUN` |
| **Signals** | `snake_case` (past tense / event) | `health_changed`, `item_collected`, `state_entered` |
| **StringName Literals** | `&"name"` | `&"move_forward"`, `&"interact"` |

---

## 2. Standard Script Organization Order

GDScript files must organize sections in the exact order recommended by Godot:

1. `@tool` / `@icon` declaration (if applicable)
2. `class_name` and `extends`
3. Docstring / file comments
4. **Signals** (`signal health_changed(...)`)
5. **Enums** (`enum State { ... }`)
6. **Constants** (`const MAX_SPEED = ...`)
7. **Exported Variables** (`@export var ...`)
8. **Public Variables** (`var ...`)
9. **Private Variables** (`var _...`)
10. **Onready Variables** (`@onready var ...`)
11. **Optional Built-in Virtual Methods** (`_init`, `_ready`, `_enter_tree`)
12. **Engine Processing Callbacks** (`_process`, `_physics_process`, `_input`, `_unhandled_input`)
13. **Public Methods**
14. **Private Methods** (`func _helper_function() -> void:`)
15. **Signal Handlers** (`func _on_signal_emitted() -> void:`)

---

## 3. Static Typing Standards

- Always write explicit or inferred type annotations:
  - Explicit: `var target_health: float = 100.0`
  - Inferred (only when the right-hand side is unambiguously typed): `var initial_velocity := Vector3.ZERO`
  - Function parameters and return values must always be typed:
    ```gdscript
    func calculate_damage(base_damage: float, armor_rating: float) -> float:
        return maxf(0.0, base_damage - armor_rating)
    ```
- Use `as` for safe downcasting:
  ```gdscript
  var enemy := body as EnemyCharacter
  if enemy != null:
      enemy.apply_damage(10.0)
  ```

---

## 4. Folder & File Layout

Follows [Feature-First Folder Structure](../architecture/feature-first-folder-structure.md).

- Every folder and file uses `snake_case` (`features/combat/weapon_pickup.gd`). C# scripts are the exception and match their `PascalCase` class name.
- Scenes, scripts, and their assets sit side by side inside the feature folder.
- Global levels in `res://levels/`; third-party content in `res://addons/`.
- Add an empty `.gdignore` to folders Godot should not import (e.g. `docs/`).

---

## Comprehensive Example

```gdscript
class_name CharacterHealthManager
extends Node

## Manages character vitality, armor mitigation, and mortality state.

# 1. Signals
signal health_updated(current_health: float, maximum_health: float)
signal character_depleted

# 2. Enums
enum DamageChannel {
	PHYSICAL,
	ELEMENTAL,
	TRUE_DAMAGE
}

# 3. Constants
const MINIMUM_DAMAGE_VALUE: float = 0.5
const INVULNERABILITY_FLASH_RATE: float = 0.1

# 4. Exported Variables
@export_group("Vitality Settings")
@export_range(1.0, 1000.0, 1.0) var maximum_health: float = 100.0
@export_range(0.0, 5.0, 0.1) var invulnerability_duration_seconds: float = 0.5

# 5. Public Variables
var is_invulnerable: bool:
	get:
		return _invulnerability_timer > 0.0

# 6. Private Variables
var _current_health: float = 100.0
var _invulnerability_timer: float = 0.0
var _is_dead: bool = false

# 7. Lifecycle Methods
func _ready() -> void:
	_current_health = maximum_health

func _physics_process(delta: float) -> void:
	_update_invulnerability(delta)

# 8. Public Methods
func apply_damage(damage_amount: float, _channel: DamageChannel = DamageChannel.PHYSICAL) -> void:
	if _is_dead or is_invulnerable or damage_amount < MINIMUM_DAMAGE_VALUE:
		return

	_current_health = maxf(0.0, _current_health - damage_amount)
	_invulnerability_timer = invulnerability_duration_seconds

	health_updated.emit(_current_health, maximum_health)

	if _current_health <= 0.0:
		_handle_mortality()

func restore_health(heal_amount: float) -> void:
	if _is_dead or heal_amount <= 0.0:
		return

	_current_health = minf(maximum_health, _current_health + heal_amount)
	health_updated.emit(_current_health, maximum_health)

func get_current_health() -> float:
	return _current_health

func is_dead() -> bool:
	return _is_dead

# 9. Private Helper Methods
func _update_invulnerability(delta: float) -> void:
	if _invulnerability_timer > 0.0:
		_invulnerability_timer = maxf(0.0, _invulnerability_timer - delta)

func _handle_mortality() -> void:
	_is_dead = true
	character_depleted.emit()
```
