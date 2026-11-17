---
trigger: always_on
description: "Naming Is Architecture"
---

# Rule: Naming Is Architecture

Code is read 10x more than it is written. Treat naming as core architecture, not cosmetics. Clear names communicate system boundaries, ownership, and intent without needing to inspect implementation details.

---

## Core Guidelines

1. **Be Descriptive and Verbose**
   - Never sacrifice clarity for brevity. A 25-character name that states exact intent (`maxSprintStaminaConsumptionRate`) is always superior to a cryptic abbreviation (`sCons`).
   - If a name feels "too long", it often indicates the class or method has too many responsibilities (violating SRP), not that the name is bad.

2. **Zero Abbreviations or Acronyms**
   - Ban truncated words: use `distance` (not `dst`), `uiText` (not `txt`), `velocity` (not `vel`), and `manager` or `coordinator` (not `mgr`).
   - "Present You" knows what `currWpIdx` means; "Future You" or your teammate 6 months from now will lose time deciphering it.

3. **No Single-Letter Variables**
   - Never use `s`, `d`, `t`, or `p` for state or calculation variables.
   - **Sole Exception:** Standard integer loop counters (`i`, `j`, `k`) in localized, short loops (under 5 lines).

4. **No Generic or "Clever" Names**
   - Ban generic catch-all names: `DoStuff()`, `HandleData()`, `Process()`, `UpdateLogic()`, or `ManagerHelper`.
   - Ban humor, inside jokes, and slang in code symbols.
   - Names must state *what* the data represents (`remainingRespawnSeconds`) or *what* the action accomplishes (`CalculateRemainingRespawnTime()`).

5. **Enforce 100% Style Consistency Per Engine & Language**
   - Adhere strictly to engine-specific idioms and conventions. Never mix naming styles within a project.

---

## Engine Casing Standards

| Symbol Type | Unity (C#) | Godot (GDScript) | Unreal Engine (C++) |
| :--- | :--- | :--- | :--- |
| **Classes / Structs** | `PascalCase` (`PlayerMovement`) | `PascalCase` (`PlayerMovement`) | Prefix + `PascalCase` (`ACharacter`, `UComponent`, `FDataStruct`) |
| **Interfaces** | `IPascalCase` (`IDamageable`) | `IPascalCase` (`IDamageable`) | `IPascalCase` (`IInteractableInterface`) |
| **Public Methods / Functions** | `PascalCase` (`TakeDamage`) | `snake_case` (`take_damage`) | `PascalCase` (`TakeDamage`) |
| **Private Fields / Members** | `m_` prefix + `camelCase` (`m_currentHealth`) | `_` prefix + `snake_case` (`_current_health`) | `PascalCase` (`CurrentHealth`) |
| **Parameters & Locals** | `camelCase` (`damageAmount`) | `snake_case` (`damage_amount`) | `PascalCase` / `In` prefix (`DamageAmount`, `InDamageAmount`) |
| **Booleans** | `is/has/can` (`m_isGrounded`, `m_canJump`) | `is/has/can` (`_is_grounded`, `_can_jump`) | `b` Prefix + `PascalCase` (`bIsGrounded`, `bCanDoubleJump`) |
| **Constants** | `k_` prefix + `camelCase` (`k_maxSpeed`) | `CONSTANT_CASE` (`MAX_SPEED`) | `PascalCase` / `static constexpr` (`MaxSpeed`) |
| **Signals / Events / Delegates** | `On...` (`OnHealthChanged`) | `snake_case` past-tense (`health_changed`) | `On...` (`OnHealthChangedSignature`) |

---

## Examples

### 1. Unity (C#)

```csharp
// ==========================================
// ❌ BAD: Cryptic, abbreviated, vague names
// ==========================================
public class PlrMgr : MonoBehaviour
{
    public float spd = 5.0f;
    public float stm = 100f;
    private float d;
    private bool flg;

    void Update()
    {
        DoStuff();
    }

    void DoStuff()
    {
        // What does this calculate? What is d?
        d = spd * Time.deltaTime;
        if (flg) stm -= 10f * Time.deltaTime;
    }
}

// ==========================================
// ✅ GOOD: Expressive, clear architectural intent with m_ and k_ conventions
// ==========================================
public class PlayerStamina : MonoBehaviour
{
    private const float k_defaultRecoveryRate = 5.0f;

    [SerializeField] private float m_baseMovementSpeed = 5.0f;
    [SerializeField] private float m_sprintStaminaConsumptionRate = 10.0f;
    [SerializeField] private float m_currentStamina = 100.0f;

    private float m_traveledDistanceThisFrame;
    private bool m_isSprinting;

    public float CurrentStamina => m_currentStamina;
    public bool IsSprinting => m_isSprinting;

    private void Update()
    {
        UpdateStaminaConsumption(Time.deltaTime);
    }

    public void SetSprinting(bool isSprintRequested)
    {
        m_isSprinting = isSprintRequested && m_currentStamina > 0.0f;
    }

    private void UpdateStaminaConsumption(float deltaTime)
    {
        if (!m_isSprinting) return;

        m_currentStamina = Mathf.Max(0.0f, m_currentStamina - (m_sprintStaminaConsumptionRate * deltaTime));
    }
}
```

---

### 2. Godot (GDScript)

```gdscript
# ==========================================
# ❌ BAD: Inconsistent casing, abbreviations, generic names
# ==========================================
class_name CharController extends CharacterBody3D

var movSpd: float = 6.0
var curHp: int = 100
var d: Vector3
var p_mgr: Node

func proc_data():
	pass

func _physics_process(delta: float) -> void:
	proc_data()

# ==========================================
# ✅ GOOD: Idiomatic GDScript snake_case, private _ prefix, explicit intent
# ==========================================
class_name PlayerMovementController extends CharacterBody3D

const TERMINAL_FALL_VELOCITY: float = -50.0

@export var walk_movement_speed: float = 6.0
@export var sprint_movement_speed: float = 10.0
@export var acceleration_smoothing_rate: float = 12.0

var _is_sprinting_active: bool = false
var _target_horizontal_velocity: Vector3 = Vector3.ZERO

func _physics_process(delta: float) -> void:
	_apply_gravity_to_velocity(delta)
	_update_horizontal_movement(delta)
	move_and_slide()

func _apply_gravity_to_velocity(delta: float) -> void:
	if not is_on_floor():
		velocity.y = maxf(velocity.y - (ProjectSettings.get_setting("physics/3d/default_gravity") * delta), TERMINAL_FALL_VELOCITY)

func _update_horizontal_movement(delta: float) -> void:
	var active_movement_speed: float = sprint_movement_speed if _is_sprinting_active else walk_movement_speed
	var desired_velocity: Vector3 = _target_horizontal_velocity * active_movement_speed
	velocity.x = lerp(velocity.x, desired_velocity.x, acceleration_smoothing_rate * delta)
	velocity.z = lerp(velocity.z, desired_velocity.z, acceleration_smoothing_rate * delta)
```

---

### 3. Unreal Engine (C++)

```cpp
// ==========================================
// ❌ BAD: Missing Unreal prefixes, vague names, abbreviations
// ==========================================
// In Char.h
class Char : public AActor
{
    float spd;
    bool st;
    void DoAction();
};

// ==========================================
// ✅ GOOD: Standard UE prefixes (A, U, F, b), expressive naming
// ==========================================
// In PlayerCharacterMovementComponent.h
#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "PlayerCharacterMovementComponent.generated.h"

UCLASS(ClassGroup=(Custom), meta=(BlueprintSpawnableComponent))
class GAME_API UPlayerCharacterMovementComponent : public UActorComponent
{
    GENERATED_BODY()

public:
    UPlayerCharacterMovementComponent();

    void RequestSprintStart();
    void RequestSprintStop();

    UFUNCTION(BlueprintPure, Category = "Movement")
    bool IsSprintActive() const { return bIsSprintActive; }

protected:
    virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

private:
    UPROPERTY(EditDefaultsOnly, Category = "Movement|Speed")
    float WalkSpeedUnitsPerSecond = 600.0f;

    UPROPERTY(EditDefaultsOnly, Category = "Movement|Speed")
    float SprintSpeedUnitsPerSecond = 950.0f;

    UPROPERTY(VisibleInstanceOnly, Category = "Movement|State")
    bool bIsSprintActive = false;

    void ConsumeSprintStamina(float DeltaTime);
};
```
