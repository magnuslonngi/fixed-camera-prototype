---
trigger: always_on
description: "Information Hiding (Encapsulation)"
---

# Rule: Information Hiding (Encapsulation)

Minimize the public surface area of every class, struct, node, and component. When state is hidden and strictly guarded, bugs cannot propagate uncontrollably across systems.

---

## Core Guidelines

1. **Default to Private**
   - Every field, variable, and helper function must be `private` by default.
   - Never declare fields `public` merely for lazy external access or debugging convenience.

2. **Safe Editor Exposure**
   - Expose configuration values to designers and the engine inspector without breaking code-level privacy:
     - **Unity (C#):** `[SerializeField] private float m_movementSpeed = 5.0f;`
     - **Godot (GDScript):** Use private backing variables with validated `@export` setters, or `@export_range(...)`.
     - **Unreal Engine (C++):** `UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Config", meta = (AllowPrivateAccess = "true")) private float BaseSpeed;`

3. **Controlled Accessors & Validated Mutators**
   - Provide external access only when strictly required, using read-only properties or dedicated getters (`public float CurrentHealth => m_currentHealth;`).
   - Route all mutations through authoritative methods (e.g., `TakeDamage(amount)`, `RestoreStamina(amount)`). Never let external classes assign raw values like `player.health = -50`.
   - Use setters and mutators to enforce boundary clamping, invariant checks, logging, and state change event broadcasting.

---

## Examples

### 1. Unity (C#)

```csharp
// ==========================================
// ❌ BAD: Public fields allow arbitrary external manipulation
// ==========================================
public class UnsafePlayerStats : MonoBehaviour
{
    // Any script can set health to negative numbers or bypass death checks
    public float currentHealth = 100f;
    public float maxHealth = 100f;
}

// ==========================================
// ✅ GOOD: Serialized private fields and read-only getters with validated mutation
// ==========================================
public class EncapsulatedPlayerStats : MonoBehaviour
{
    // Exposed safely to Unity Inspector, hidden from all other C# scripts
    [SerializeField] private float m_maxHealth = 100.0f;
    [SerializeField] private float m_minimumAllowedDamage = 1.0f;

    private float m_currentHealth;

    // Read-only externally: others can read, but only this class can modify
    public float CurrentHealth => m_currentHealth;
    public float MaxHealth => m_maxHealth;
    public bool IsDead => m_currentHealth <= 0.0f;

    public event System.Action<float> OnHealthChanged;

    private void Awake()
    {
        m_currentHealth = m_maxHealth;
    }

    public void ApplyDamage(float damageAmount)
    {
        if (IsDead) return;
        if (damageAmount < m_minimumAllowedDamage) return;

        // Clamping ensures internal invariants cannot be corrupted
        m_currentHealth = Mathf.Clamp(m_currentHealth - damageAmount, 0.0f, m_maxHealth);
        OnHealthChanged?.Invoke(m_currentHealth);
    }
}
```

---

### 2. Godot (GDScript)

```gdscript
# ==========================================
# ❌ BAD: Public variables without validation
# ==========================================
class_name UnsafeShield extends Node

var shield_points: int = 100 # Can be set to -9999 by any node
var is_depleted: bool = false

# ==========================================
# ✅ GOOD: Encapsulated backing variable with validation & exported range
# ==========================================
class_name EncapsulatedShield extends Node

signal shield_changed(new_value: int)
signal shield_broken

@export_range(10, 500, 5) var maximum_shield: int = 100:
	set(new_max):
		maximum_shield = maxi(10, new_max)
		if _current_shield > maximum_shield:
			_set_current_shield(maximum_shield)

var _current_shield: int = 100

func _ready() -> void:
	_current_shield = maximum_shield

func get_current_shield() -> int:
	return _current_shield

func is_broken() -> bool:
	return _current_shield <= 0

func absorb_damage(incoming_damage: int) -> int:
	if incoming_damage <= 0 or is_broken():
		return incoming_damage

	var remaining_damage: int = maxi(0, incoming_damage - _current_shield)
	_set_current_shield(_current_shield - incoming_damage)
	return remaining_damage

func _set_current_shield(new_value: int) -> void:
	var clamped_value: int = clampi(new_value, 0, maximum_shield)
	if clamped_value == _current_shield:
		return

	_current_shield = clamped_value
	shield_changed.emit(_current_shield)

	if _current_shield == 0:
		shield_broken.emit()
```

---

### 3. Unreal Engine (C++)

```cpp
// ==========================================
// ❌ BAD: Public mutable data in Actor
// ==========================================
class ABadPlayer : public AActor
{
public:
    // Any file including this header can corrupt this value
    float PlayerStamina = 100.0f;
};

// ==========================================
// ✅ GOOD: Strict Unreal encapsulation with AllowPrivateAccess and getters
// ==========================================
#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "EncapsulatedStaminaComponent.generated.h"

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnStaminaChangedSignature, float, NewStamina);

UCLASS(ClassGroup=(Custom), meta=(BlueprintSpawnableComponent))
class GAME_API UEncapsulatedStaminaComponent : public UActorComponent
{
    GENERATED_BODY()

public:
    UEncapsulatedStaminaComponent();

    UFUNCTION(BlueprintPure, Category = "Stamina")
    float GetCurrentStamina() const { return CurrentStamina; }

    UFUNCTION(BlueprintPure, Category = "Stamina")
    float GetMaxStamina() const { return MaxStamina; }

    UFUNCTION(BlueprintCallable, Category = "Stamina")
    bool TryConsumeStamina(float AmountToConsume);

    UFUNCTION(BlueprintCallable, Category = "Stamina")
    void RestoreStamina(float AmountToRestore);

    UPROPERTY(BlueprintAssignable, Category = "Stamina|Events")
    FOnStaminaChangedSignature OnStaminaChanged;

private:
    // Exposed to Details panel for designers, but fully private to C++ code
    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Stamina", meta = (AllowPrivateAccess = "true"))
    float MaxStamina = 100.0f;

    UPROPERTY(VisibleInstanceOnly, Category = "Stamina")
    float CurrentStamina = 100.0f;

    void SetStaminaInternal(float NewValue);
};

// Implementation in .cpp:
void UEncapsulatedStaminaComponent::SetStaminaInternal(float NewValue)
{
    const float ClampedStamina = FMath::Clamp(NewValue, 0.0f, MaxStamina);
    if (!FMath::IsNearlyEqual(ClampedStamina, CurrentStamina))
    {
        CurrentStamina = ClampedStamina;
        OnStaminaChanged.Broadcast(CurrentStamina);
    }
}
```
