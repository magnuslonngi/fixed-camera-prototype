---
trigger: always_on
description: "Self-Documenting Code & Intentional Comments"
---

# Rule: Self-Documenting Code & Intentional Comments

The code itself is the only completely reliable documentation of *what* the software is doing. Comments that merely restate code quickly become outdated, misleading, and dangerous.

---

## Core Guidelines

1. **Write Self-Documenting Logic**
   - Instead of writing a comment explaining a dense boolean conditional:
     ```csharp
     // Bad:
     // Check if player is alive, has sufficient stamina, and isn't falling
     if (hp > 0 && stm >= 20f && vel.y == 0) { ... }
     ```
   - Extract the logic into an intention-revealing method or property:
     ```csharp
     // Good:
     if (CanPerformGroundDodge()) { ... }
     ```

2. **Delete Bad & Redundant Comments**
   - **Never state the obvious:** Delete comments that parrot the code (`score += 10; // Add 10 to score`).
   - **Never use comments to justify poor naming:** Do not write `float d; // distance to target`. Rename the variable to `distanceToTarget`.
   - Outdated comments are worse than missing comments because they actively deceive future maintainers.

3. **Comment the "WHY", Never the "WHAT"**
   - Reserve comments strictly for critical context that code alone cannot express:
     - **Architectural & Design Exceptions:** Why a conventional solution was intentionally avoided (e.g., *"Pathfinding grid is intentionally bypassed here because the arena is completely planar without obstacles"*).
     - **Engine Traps & Lifecycle Ordering:** Warnings about execution timing or engine quirks (e.g., *"Must update in LateUpdate / _physics_process priority to avoid 1-frame camera jitter behind CharacterController"*).
     - **External Dependencies & Workarounds:** Workarounds for known engine bugs with issue tracker links.

---

## Examples

### 1. Unity (C#)

```csharp
// ==========================================
// ❌ BAD: Noisy, redundant comments masking messy code
// ==========================================
public class BadCameraFollow : MonoBehaviour
{
    public Transform t; // target
    public float s = 5f; // speed

    // Update is called once per frame
    void Update()
    {
        // Check if target is not null and player is alive
        if (t != null && t.GetComponent<PlayerHealth>().currentHealth > 0)
        {
            // move position towards target
            transform.position = Vector3.Lerp(transform.position, t.position, s * Time.deltaTime);
        }
    }
}

// ==========================================
// ✅ GOOD: Self-documenting code with m_ conventions and "WHY" comments
// ==========================================
public class SmoothCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform m_targetFollowTransform;
    [SerializeField] private PlayerHealth m_targetPlayerHealth;
    [SerializeField] private float m_smoothFollowSpeed = 5.0f;

    // NOTE(Architecture): LateUpdate is strictly required here instead of Update.
    // Unity's CharacterController translates during Update; moving the camera here
    // eliminates sub-pixel micro-stutter caused by frame-scheduling races.
    private void LateUpdate()
    {
        if (!CanTrackTarget()) return;

        InterpolateCameraPosition(Time.deltaTime);
    }

    private bool CanTrackTarget()
    {
        return m_targetFollowTransform != null &&
               m_targetPlayerHealth != null &&
               !m_targetPlayerHealth.IsDead;
    }

    private void InterpolateCameraPosition(float deltaTime)
    {
        transform.position = Vector3.Lerp(
            transform.position,
            m_targetFollowTransform.position,
            m_smoothFollowSpeed * deltaTime
        );
    }
}
```

---

### 2. Godot (GDScript)

```gdscript
# ==========================================
# ❌ BAD: Redundant comments, unclear conditions
# ==========================================
class_name BadItemCollector extends Area2D

func _on_body_entered(body: Node2D) -> void:
	# Check if body is player, has inventory, and not full
	if body is Player && body.inv != null && body.inv.items.size() < 20:
		# add item
		body.inv.items.append(self)
		queue_free() # delete item

# ==========================================
# ✅ GOOD: Self-documenting predicates with "WHY" comments for engine quirks
# ==========================================
class_name CollectibleItem extends Area2D

@export var item_data: ResourceItemData

func _ready() -> void:
	body_entered.connect(_on_body_entered)

func _on_body_entered(body: Node2D) -> void:
	var player_character: PlayerCharacter = body as PlayerCharacter
	if not _can_be_collected_by(player_character):
		return

	_collect_item(player_character)

func _can_be_collected_by(player: PlayerCharacter) -> bool:
	return player != null and \
		   player.has_inventory_capacity_for(item_data) and \
		   not player.is_interaction_locked()

func _collect_item(player: PlayerCharacter) -> void:
	player.store_item(item_data)

	# NOTE(Engine): queue_free() must be deferred until the end of the physics step
	# to avoid crashing collision pairs currently being evaluated in this frame.
	set_deferred("monitoring", false)
	queue_free()
```

---

### 3. Unreal Engine (C++)

```cpp
// ==========================================
// ❌ BAD: Obvious comments and obscure inline conditions
// ==========================================
void ABadEnemy::Tick(float DeltaTime)
{
    Super::Tick(DeltaTime);

    // If dist < 300 and player not hidden and has line of sight
    if (FVector::Dist(GetActorLocation(), Player->GetActorLocation()) < 300.0f && !Player->bHidden && HasLoS())
    {
        // Attack player
        Attack();
    }
}

// ==========================================
// ✅ GOOD: Expressive helper functions with intentional rationale comments
// ==========================================
#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "IntentionalEnemyCharacter.generated.h"

UCLASS()
class GAME_API AIntentionalEnemyCharacter : public ACharacter
{
    GENERATED_BODY()

public:
    virtual void Tick(float DeltaTime) override;

private:
    static constexpr float MeleeAttackRangeUnits = 300.0f;

    bool CanExecuteMeleeAttack() const;
    bool HasUnobstructedLineOfSightToTarget() const;
    void ExecuteMeleeStrike();

    UPROPERTY(Transient)
    TObjectPtr<AActor> CurrentTargetActor;
};

// In IntentionalEnemyCharacter.cpp:
void AIntentionalEnemyCharacter::Tick(float DeltaTime)
{
    Super::Tick(DeltaTime);

    if (CanExecuteMeleeAttack())
    {
        ExecuteMeleeStrike();
    }
}

bool AIntentionalEnemyCharacter::CanExecuteMeleeAttack() const
{
    if (!CurrentTargetActor) return false;

    // NOTE(Design): Distance squared is intentionally used here to skip costly
    // square root operations on hundreds of active melee pawns.
    const float DistanceSquared = FVector::DistSquared(GetActorLocation(), CurrentTargetActor->GetActorLocation());
    const float AttackRangeSquared = FMath::Square(MeleeAttackRangeUnits);

    return (DistanceSquared <= AttackRangeSquared) && HasUnobstructedLineOfSightToTarget();
}
```
