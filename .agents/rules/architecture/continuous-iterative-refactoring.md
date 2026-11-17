---
trigger: always_on
description: "Continuous Iterative Refactoring"
---

# Rule: Continuous Iterative Refactoring

First drafts exist to validate functionality and explore mechanics; subsequent refactoring passes make the architecture sustainable and maintainable. A feature is never "done" until its code has undergone a formal cleanup pass.

---

## Core Guidelines

1. **The Three-Step Development Cycle**
   1. **Make it Work:** Prototype the mechanic quickly to prove fun, game feel, and mathematical correctness.
   2. **Make it Clean:** Refactor into clear names, encapsulated properties, and decoupled events before merging.
   3. **Iterate:** Revisit and adapt the design as project scale demands.

2. **Standard 5-Step Refactoring Checklist**
   Before marking any feature, ticket, or pull request as complete:
   1. **Rename:** Rename vague abbreviations or single-letter variables using IDE symbol refactoring.
   2. **Privatize & Encapsulate:** Make exposed fields private; replace direct cross-script mutation with events, signals, or delegates.
   3. **Extract Single Responsibilities:** Move audio, visual presentation, or secondary calculations out of the core actor/controller into dedicated components or helper methods.
   4. **Prune Over-Abstraction:** If an abstraction (e.g., 5 micro-classes for a simple 10-line math calculation) creates more cognitive overhead than clarity, simplify and re-combine it. Pragmatism over dogma!
   5. **Eradicate Dead Code:** Delete commented-out code blocks, unused `using`/`import` statements, obsolete prototype fields, and debug logs. Version control preserves history—code files must remain clean.

---

## Examples

### 1. Unity (C#)

```csharp
// ==========================================
// ❌ STEP 1: Prototype Draft ("Make it Work")
// Messy, works at runtime, but will collapse under scale
// ==========================================
public class PlayerDraft : MonoBehaviour
{
    public float s = 5f;
    public float hp = 100f;
    // public float oldSpeed = 2f; // unused prototype leftover
    public AudioSource aud;

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        transform.position += new Vector3(x * s * Time.deltaTime, 0, 0);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            hp -= 10;
            aud.Play(); // Directly coupled
            Debug.Log("Hurt: " + hp);
        }
    }
}

// ==========================================
// ✅ STEP 2: Production Clean Pass ("Make it Clean")
// Encapsulated, decoupled, strongly typed with m_ conventions, zero dead code
// ==========================================

// Core health logic
public class CharacterHealth : MonoBehaviour
{
    [SerializeField] private float m_maxHealth = 100.0f;
    private float m_currentHealth;

    public event System.Action<float, float> OnHealthChanged;

    private void Awake() => m_currentHealth = m_maxHealth;

    public void ApplyDamage(float damageAmount)
    {
        if (m_currentHealth <= 0.0f) return;
        m_currentHealth = Mathf.Max(0.0f, m_currentHealth - damageAmount);
        OnHealthChanged?.Invoke(m_currentHealth, m_maxHealth);
    }
}

// Dedicated audio presenter
public class CharacterAudioPresenter : MonoBehaviour
{
    [SerializeField] private CharacterHealth m_characterHealth;
    [SerializeField] private AudioSource m_audioSource;
    [SerializeField] private AudioClip m_hurtAudioClip;

    private void OnEnable() => m_characterHealth.OnHealthChanged += HandleHealthChanged;
    private void OnDisable() => m_characterHealth.OnHealthChanged -= HandleHealthChanged;

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        m_audioSource.PlayOneShot(m_hurtAudioClip);
    }
}
```

---

### 2. Godot (GDScript)

```gdscript
# ==========================================
# ❌ STEP 1: Prototype Draft ("Make it Work")
# Hardcoded, monolithic, dead comments
# ==========================================
extends CharacterBody2D

var spd = 200
# var jump_force = 400 # old jump prototype
@onready var lbl = $"../UI/Label"

func _physics_process(delta):
	var dir = Input.get_axis("ui_left", "ui_right")
	velocity.x = dir * spd
	move_and_slide()
	lbl.text = str(velocity.length()) # Direct UI coupling

# ==========================================
# ✅ STEP 2: Production Clean Pass ("Make it Clean")
# Strong typing, signals, exported configuration, private _ helpers
# ==========================================
class_name PlayerPlatformerController extends CharacterBody2D

signal horizontal_speed_updated(new_speed_units: float)

@export var horizontal_movement_speed: float = 200.0

func _physics_process(_delta: float) -> void:
	_update_horizontal_movement()
	move_and_slide()
	_broadcast_movement_metrics()

func _update_horizontal_movement() -> void:
	var move_direction: float = Input.get_axis("ui_left", "ui_right")
	velocity.x = move_direction * horizontal_movement_speed

func _broadcast_movement_metrics() -> void:
	horizontal_speed_updated.emit(absf(velocity.x))
```

---

### 3. Unreal Engine (C++)

```cpp
// ==========================================
// ❌ STEP 1: Prototype Draft ("Make it Work")
// In AWeaponPrototype.cpp: Direct widget manipulation, raw numbers, dead code
// ==========================================
void AWeaponPrototype::Fire()
{
    // if (Bullets <= 0) return; // old check
    Bullets--;
    UGameplayStatics::PlaySoundAtLocation(this, Sound, GetActorLocation());
    if (AmmoText) AmmoText->SetText(FText::AsNumber(Bullets));
}

// ==========================================
// ✅ STEP 2: Production Clean Pass ("Make it Clean")
// Encapsulated component with delegates and validated consumption
// ==========================================

// Authoritative weapon firing component
#pragma once
#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "RangedWeaponComponent.generated.h"

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnWeaponFiredDelegate, int32, RemainingRounds);

UCLASS(ClassGroup=(Custom), meta=(BlueprintSpawnableComponent))
class GAME_API URangedWeaponComponent : public UActorComponent
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintCallable, Category = "Combat|Weapon")
    bool TryFireWeapon()
    {
        if (CurrentRoundsInMagazine <= 0)
        {
            return false;
        }

        CurrentRoundsInMagazine--;
        OnWeaponFired.Broadcast(CurrentRoundsInMagazine);
        return true;
    }

    UPROPERTY(BlueprintAssignable, Category = "Combat|Events")
    FOnWeaponFiredDelegate OnWeaponFired;

private:
    UPROPERTY(EditDefaultsOnly, Category = "Combat|Ammo")
    int32 MagazineCapacity = 30;

    int32 CurrentRoundsInMagazine = 30;
};
```
