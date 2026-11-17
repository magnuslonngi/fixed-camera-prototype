---
trigger: always_on
description: "Single Responsibility Principle (SRP)"
---

# Rule: Single Responsibility Principle (SRP)

Every class, script, node, component, and function must do **one thing and one thing only**. When a module has only one reason to change, bugs are localized, refactoring is low-risk, and tests remain simple.

---

## Core Guidelines

1. **The "AND" Heuristic**
   - If describing what a function, method, or class does requires the conjunction **"and"** (e.g., `MovePlayerAndPlayFootstepSound()`, `ValidateInputAndSubmitScore()`, `TakeDamageAndPlaySoundEffect()`), it violates SRP.
   - Split compound operations into separate, single-purpose units.

2. **Granular Component-Based Modules**
   - Avoid the "God Object" anti-pattern (such as a 2,500-line `Player.cs` / `player.gd` / `APlayerCharacter.cpp` that handles movement, input, health, inventory, audio, UI, and save state).
   - Decompose entities into dedicated, single-purpose components:
     - `PlayerMovement`: handles velocity, acceleration, and physical translation.
     - `PlayerHealth`: handles damage arithmetic, healing, and death state.
     - `PlayerAudioPresenter`: listens to movement and damage events to play sounds.
     - `PlayerVisualPresenter`: handles meshes, particle triggers, and animation states.

3. **Function-Level Atomicity**
   - Keep functions focused on a single logical level of abstraction. A high-level coordination function delegates details to private helper functions.

---

## Examples

### 1. Unity (C#)

```csharp
// ==========================================
// ❌ BAD: Monolithic class handling physics, health, audio, and UI
// ==========================================
public class MonolithicPlayer : MonoBehaviour
{
    public float health = 100f;
    public float speed = 5f;
    public AudioSource audioSource;
    public AudioClip damageClip;
    public UnityEngine.UI.Slider healthSlider;

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        transform.Translate(Vector3.right * (h * speed * Time.deltaTime));
    }

    public void TakeDamageAndPlaySound(float damage)
    {
        health -= damage;
        audioSource.PlayOneShot(damageClip);
        healthSlider.value = health / 100f;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}

// ==========================================
// ✅ GOOD: Split into focused, single-purpose components with m_ conventions
// ==========================================

// 1. Movement logic only
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float m_movementSpeed = 5.0f;
    [SerializeField] private CharacterController m_characterController;

    public void Move(Vector2 inputDirection)
    {
        Vector3 displacement = new Vector3(inputDirection.x, 0.0f, inputDirection.y) * (m_movementSpeed * Time.deltaTime);
        m_characterController.Move(displacement);
    }
}

// 2. Health authority only
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float m_maxHealth = 100.0f;
    private float m_currentHealth;

    public event System.Action<float, float> OnHealthChanged;
    public event System.Action OnPlayerDied;

    private void Awake() => m_currentHealth = m_maxHealth;

    public void ApplyDamage(float damageAmount)
    {
        if (m_currentHealth <= 0.0f) return;

        m_currentHealth = Mathf.Max(0.0f, m_currentHealth - damageAmount);
        OnHealthChanged?.Invoke(m_currentHealth, m_maxHealth);

        if (m_currentHealth <= 0.0f)
        {
            OnPlayerDied?.Invoke();
        }
    }
}

// 3. Audio reaction only
public class PlayerAudioFeedback : MonoBehaviour
{
    [SerializeField] private PlayerHealth m_playerHealth;
    [SerializeField] private AudioSource m_audioSource;
    [SerializeField] private AudioClip m_damageAudioClip;

    private void OnEnable() => m_playerHealth.OnHealthChanged += HandleHealthChanged;
    private void OnDisable() => m_playerHealth.OnHealthChanged -= HandleHealthChanged;

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        m_audioSource.PlayOneShot(m_damageAudioClip);
    }
}
```

---

### 2. Godot (GDScript)

```gdscript
# ==========================================
# ❌ BAD: One script doing physics, health, sound, and animation
# ==========================================
class_name MonolithicCharacter extends CharacterBody2D

var health: int = 100
@onready var anim_player: AnimationPlayer = $AnimationPlayer
@onready var audio_player: AudioStreamPlayer2D = $AudioStreamPlayer2D

func _physics_process(delta: float) -> void:
	var dir = Input.get_axis("ui_left", "ui_right")
	velocity.x = dir * 200.0
	move_and_slide()

func hit_and_update_all(damage_amount: int) -> void:
	health -= damage_amount
	audio_player.play()
	anim_player.play("hurt")
	if health <= 0:
		queue_free()

# ==========================================
# ✅ GOOD: Granular nodes each owning a single responsibility
# ==========================================

# Attached to: Player/MovementController (Node)
class_name MovementController extends Node

@export var character_body: CharacterBody2D
@export var movement_speed: float = 200.0

func handle_movement(input_direction: float) -> void:
	if character_body == null:
		return
	character_body.velocity.x = input_direction * movement_speed
	character_body.move_and_slide()

# Attached to: Player/HealthComponent (Node)
class_name HealthComponent extends Node

signal health_depleted
signal health_updated(current: int, maximum: int)

@export var maximum_health: int = 100
var _current_health: int

func _ready() -> void:
	_current_health = maximum_health

func apply_damage(damage_amount: int) -> void:
	if _current_health <= 0:
		return
	_current_health = maxi(0, _current_health - damage_amount)
	health_updated.emit(_current_health, maximum_health)
	if _current_health == 0:
		health_depleted.emit()

# Attached to: Player/AudioFeedback (Node)
class_name CharacterAudioFeedback extends Node

@export var health_component: HealthComponent
@export var hurt_sound_player: AudioStreamPlayer2D

func _ready() -> void:
	if health_component:
		health_component.health_updated.connect(_on_health_updated)

func _on_health_updated(_current: int, _max: int) -> void:
	hurt_sound_player.play()
```

---

### 3. Unreal Engine (C++)

```cpp
// ==========================================
// ❌ BAD: APlayerCharacter handles raw input, damage calculation, audio, and UI widgets
// ==========================================
// MonolithicCharacter.cpp
void AMonolithicCharacter::TakeDamageAndPlaySound(float Damage)
{
    CurrentHealth -= Damage;
    UGameplayStatics::PlaySoundAtLocation(this, HurtSound, GetActorLocation());
    if (HealthWidget)
    {
        HealthWidget->UpdateHealthBar(CurrentHealth / MaxHealth);
    }
}

// ==========================================
// ✅ GOOD: Modular Actor Components with single responsibilities
// ==========================================

// --- HealthComponent.h ---
#pragma once
#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "HealthComponent.generated.h"

DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FOnHealthChangedDelegate, float, CurrentHealth, float, MaxHealth);
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnDeathDelegate);

UCLASS(ClassGroup=(Custom), meta=(BlueprintSpawnableComponent))
class GAME_API UHealthComponent : public UActorComponent
{
    GENERATED_BODY()

public:
    UHealthComponent();

    void ApplyDamage(float InDamageAmount);

    UPROPERTY(BlueprintAssignable, Category = "Health|Events")
    FOnHealthChangedDelegate OnHealthChanged;

    UPROPERTY(BlueprintAssignable, Category = "Health|Events")
    FOnDeathDelegate OnDeath;

private:
    UPROPERTY(EditDefaultsOnly, Category = "Health")
    float MaxHealth = 100.0f;

    UPROPERTY(VisibleInstanceOnly, Category = "Health")
    float CurrentHealth = 100.0f;
};

// --- CharacterAudioComponent.h ---
#pragma once
#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "CharacterAudioComponent.generated.h"

class UHealthComponent;
class USoundBase;

UCLASS(ClassGroup=(Custom), meta=(BlueprintSpawnableComponent))
class GAME_API UCharacterAudioComponent : public UActorComponent
{
    GENERATED_BODY()

public:
    void InitializeAudioListeners(UHealthComponent* InHealthComponent);

private:
    UFUNCTION()
    void HandleHealthChanged(float CurrentHealth, float MaxHealth);

    UPROPERTY(EditDefaultsOnly, Category = "Audio")
    TObjectPtr<USoundBase> HurtSound;
};
```
