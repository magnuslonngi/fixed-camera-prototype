---
trigger: always_on
description: "Single Source of Truth (SSOT)"
---

# Rule: Single Source of Truth (SSOT)

Every piece of state in the game must exist in **one place and one place only**. Duplicating mutable state across multiple classes guarantees desynchronization bugs that are difficult to reproduce and trace.

---

## Core Guidelines

1. **Never Duplicate State**
   - Do not cache local copies of variables like `currentHealth`, `ammoCount`, `isDead`, or `goldBalance` across multiple presenter or listener classes (`HealthBarUI`, `PlayerAnimator`, `SaveGameSystem`).
   - If a presenter needs to display a value, either query the authoritative source on demand or process the value passed in an event payload without maintaining a persistent mutable copy.

2. **Establish Clear Authority**
   - Every system has exactly one authoritative owner:
     - `PlayerHealth` owns health and mortality state.
     - `InventorySystem` owns items and quantities.
     - `GameMode` / `GameManager` owns round timers and match state.
   - All other systems are clients of that authority.

3. **Derive, Do Not Store, Dependent Data**
   - Avoid maintaining separate flags that represent combinations of other values (e.g., maintaining a mutable `bool isDead` alongside `float currentHealth`).
   - Instead, compute derived values on demand: `bool IsDead => m_currentHealth <= 0;`.

---

## Examples

### 1. Unity (C#)

```csharp
// ==========================================
// ❌ BAD: UI duplicates and tracks its own copy of health state
// ==========================================
public class BadHealthBarUI : MonoBehaviour
{
    // Duplicate state: can easily get out of sync with PlayerHealth!
    private float m_cachedHealth;
    private bool m_cachedIsDead;

    public void OnDamageTaken(float amount)
    {
        m_cachedHealth -= amount; // Desynchronization danger!
        if (m_cachedHealth <= 0) m_cachedIsDead = true;
    }
}

// ==========================================
// ✅ GOOD: PlayerHealth is the single source of truth with m_ conventions
// ==========================================

// Authoritative state holder
public class AuthoritativePlayerHealth : MonoBehaviour
{
    [SerializeField] private float m_maxHealth = 100.0f;
    private float m_currentHealth;

    public float CurrentHealth => m_currentHealth;
    public float MaxHealth => m_maxHealth;
    public float HealthRatio => m_maxHealth > 0 ? m_currentHealth / m_maxHealth : 0f;
    public bool IsDead => m_currentHealth <= 0.0f; // Derived state, never duplicated

    public event System.Action<float, float> OnHealthChanged;

    private void Awake() => m_currentHealth = m_maxHealth;

    public void ApplyDamage(float damage)
    {
        if (IsDead) return;
        m_currentHealth = Mathf.Max(0.0f, m_currentHealth - damage);
        OnHealthChanged?.Invoke(m_currentHealth, m_maxHealth);
    }
}

// Client UI that reads directly from the authority or event payload
public class HealthBarView : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Slider m_healthSlider;
    [SerializeField] private AuthoritativePlayerHealth m_healthAuthority;

    private void Start()
    {
        // Initial setup directly from SSOT
        UpdateDisplay(m_healthAuthority.CurrentHealth, m_healthAuthority.MaxHealth);
    }

    private void OnEnable() => m_healthAuthority.OnHealthChanged += UpdateDisplay;
    private void OnDisable() => m_healthAuthority.OnHealthChanged -= UpdateDisplay;

    // Stateless consumer: displays payload, holds no mutable health copies
    private void UpdateDisplay(float current, float max)
    {
        m_healthSlider.value = max > 0f ? (current / max) : 0f;
    }
}
```

---

### 2. Godot (GDScript)

```gdscript
# ==========================================
# ❌ BAD: Audio controller and UI caching their own gold amount
# ==========================================
class_name BadGoldUI extends Label

var current_gold_copy: int = 0 # Out-of-sync risk if multiple events fire

func update_gold(added_amount: int) -> void:
	current_gold_copy += added_amount
	text = "Gold: %d" % current_gold_copy

# ==========================================
# ✅ GOOD: Wallet node is sole authority; UI and Audio read from it
# ==========================================

# --- Wallet (Single Source of Truth) ---
class_name PlayerWallet extends Node

signal balance_changed(new_balance: int)

@export var starting_gold: int = 0
var _gold_balance: int = 0

func _ready() -> void:
	_gold_balance = starting_gold

func get_gold_balance() -> int:
	return _gold_balance

func has_sufficient_funds(amount: int) -> bool:
	return _gold_balance >= amount

func deposit(amount: int) -> void:
	if amount <= 0:
		return
	_gold_balance += amount
	balance_changed.emit(_gold_balance)

func try_withdraw(amount: int) -> bool:
	if amount <= 0 or not has_sufficient_funds(amount):
		return false
	_gold_balance -= amount
	balance_changed.emit(_gold_balance)
	return true

# --- UI Consumer (Reads from SSOT, stores no duplicate balance) ---
class_name GoldDisplayUI extends Label

@export var wallet: PlayerWallet

func _ready() -> void:
	if wallet:
		wallet.balance_changed.connect(_on_balance_changed)
		_on_balance_changed(wallet.get_gold_balance())

func _on_balance_changed(new_balance: int) -> void:
	text = "Gold: %d" % new_balance
```

---

### 3. Unreal Engine (C++)

```cpp
// ==========================================
// ❌ BAD: Character caches AmmoCount in multiple components
// ==========================================
// APlayerCharacter has CurrentAmmo, Weapon has CurrentAmmo, HUD has CurrentAmmo.
// When weapon reloads, one of them inevitably fails to update.

// ==========================================
// ✅ GOOD: Dedicated AmmoComponent is the sole authority
// ==========================================
#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "AuthoritativeAmmoComponent.generated.h"

DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FOnAmmoChangedSignature, int32, CurrentAmmo, int32, MaxAmmo);

UCLASS(ClassGroup=(Custom), meta=(BlueprintSpawnableComponent))
class GAME_API UAuthoritativeAmmoComponent : public UActorComponent
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintPure, Category = "Ammo")
    int32 GetCurrentAmmo() const { return CurrentAmmo; }

    UFUNCTION(BlueprintPure, Category = "Ammo")
    int32 GetMaxAmmo() const { return MaxMagazineCapacity; }

    // Derived state: computed on the fly, zero sync bugs
    UFUNCTION(BlueprintPure, Category = "Ammo")
    bool CanFire() const { return CurrentAmmo > 0; }

    bool TryConsumeAmmo(int32 AmountToConsume = 1)
    {
        if (AmountToConsume <= 0 || CurrentAmmo < AmountToConsume)
        {
            return false;
        }

        CurrentAmmo -= AmountToConsume;
        OnAmmoChanged.Broadcast(CurrentAmmo, MaxMagazineCapacity);
        return true;
    }

    UPROPERTY(BlueprintAssignable, Category = "Ammo|Events")
    FOnAmmoChangedSignature OnAmmoChanged;

private:
    UPROPERTY(EditDefaultsOnly, Category = "Ammo")
    int32 MaxMagazineCapacity = 30;

    UPROPERTY(VisibleInstanceOnly, Category = "Ammo")
    int32 CurrentAmmo = 30;
};
```
