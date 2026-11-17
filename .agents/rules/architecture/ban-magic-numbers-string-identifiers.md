---
trigger: always_on
description: "Ban Magic Numbers & String Identifiers"
---

# Rule: Ban Magic Numbers & String Identifiers

Hardcoded numeric literals and arbitrary string identifiers bypass compiler and static type checks. They cause silent runtime failures due to typos, whitespace errors, or forgotten refactors.

---

## Core Guidelines

1. **No Magic Numbers**
   - Never embed unexplained numeric literals directly into formulas, conditions, or loops (e.g., `if (distance < 3.5f)` or `velocity * 1.2f`).
   - Extract every numeric literal into a named constant (`k_` prefix in Unity C#, `CONSTANT_CASE` in Godot, `constexpr` in Unreal C++) or serialized configuration variable with clear units (e.g., `k_interactionRangeMeters`, `SPRINT_SPEED_MULTIPLIER`).

2. **Derive Values Dynamically from Authorities**
   - Do not hardcode assumed dimensions or lengths (e.g., `if (enteredPin.Length == 4)`).
   - Query the authoritative source dynamically: `if (enteredPin.Length == targetPin.Length)`.

3. **No Strings as System Identifiers**
   - Ban string-based hierarchy lookups:
     - ❌ Unity: `GameObject.Find("Player")`, `FindGameObjectWithTag("Enemy")`
     - ❌ Godot: `get_node("/root/World/Player")`, `is_in_group("enemy")`
     - ❌ Unreal: `ActorHasTag(FName("Enemy"))`
   - **Use Strongly-Typed References:**
     - Direct inspector references / exported NodePaths.
     - Strong component type checks (`TryGetComponent<Enemy>()`, `is Enemy`, `Cast<AEnemyCharacter>()`).
     - Strongly-typed Enums, Gameplay Tags (`FGameplayTag`), or LayerMasks.

4. **Isolate Mandatory Engine Strings into Constants or Pre-Hashed IDs**
   - When an engine API mandates strings (e.g., Animator parameters, Shader properties, Input action names):
     - **Unity:** Pre-hash via `s_` static readonly integer hashes (`s_isOpenAnimHash = Animator.StringToHash("IsOpen")`) or `Shader.PropertyToID("_BaseColor")`.
     - **Godot:** Use compile-time `StringName` literals (e.g., `&"move_forward"`).
     - **Unreal Engine:** Use `FName` constants or `FGameplayTag` instead of ad-hoc `FString`.

---

## Examples

### 1. Unity (C#)

```csharp
// ==========================================
// ❌ BAD: Magic numbers, string tags, string Find, and string animator parameters
// ==========================================
public class FragileDoor : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // String tag lookup: silent fail on typos like "Plalyer"
        if (other.CompareTag("Player"))
        {
            // Magic number: what does 3.2f mean?
            if (Vector3.Distance(transform.position, other.transform.position) < 3.2f)
            {
                // Magic string parameter for animator
                GetComponent<Animator>().SetBool("IsOpen", true);
            }
        }
    }
}

// ==========================================
// ✅ GOOD: Strong types, cached integer hashes, and k_/s_/m_ conventions
// ==========================================
public class RobustDoor : MonoBehaviour
{
    private const float k_minimumInteractionDistanceMeters = 3.2f;

    // Pre-hashed animator parameter: fast integer lookup, typo-free
    private static readonly int s_isOpenAnimHash = Animator.StringToHash("IsOpen");

    [SerializeField] private Animator m_doorAnimator;

    private void OnTriggerEnter(Collider other)
    {
        // Strong type check instead of string tag
        if (!other.TryGetComponent<PlayerController>(out var player))
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= k_minimumInteractionDistanceMeters)
        {
            m_doorAnimator.SetBool(s_isOpenAnimHash, true);
        }
    }
}
```

---

### 2. Godot (GDScript)

```gdscript
# ==========================================
# ❌ BAD: String paths, magic numbers, string groups
# ==========================================
class_name FragileCombatZone extends Area3D

func _on_body_entered(body: Node3D) -> void:
	# Magic string group check
	if body.is_in_group("enemies"):
		# Magic number 45.0
		if body.get("health") < 45.0:
			# Magic string animation name
			$AnimationPlayer.play("alert_red")

# ==========================================
# ✅ GOOD: Strong class typing, StringName literals, named constants
# ==========================================
class_name RobustCombatZone extends Area3D

const LOW_HEALTH_THRESHOLD_PERCENT: float = 0.45
const ANIM_ALERT_ACTIVE: StringName = &"alert_red"
const ACTION_INTERACT: StringName = &"interact"

@export var alert_animation_player: AnimationPlayer

func _on_body_entered(body: Node3D) -> void:
	# Strongly typed check against class_name, not a string
	var enemy: EnemyCharacter = body as EnemyCharacter
	if enemy == null:
		return

	if enemy.get_health_percentage() < LOW_HEALTH_THRESHOLD_PERCENT:
		alert_animation_player.play(ANIM_ALERT_ACTIVE)
```

---

### 3. Unreal Engine (C++)

```cpp
// ==========================================
// ❌ BAD: Magic numbers and string Actor tags
// ==========================================
void ABadWeapon::OnHit(AActor* HitActor)
{
    // ActorHasTag with raw string
    if (HitActor->ActorHasTag(FName("Enemy")))
    {
        // Magic numbers for damage and armor penetration
        float Dmg = 45.0f * 1.35f;
        UGameplayStatics::ApplyDamage(HitActor, Dmg, nullptr, this, nullptr);
    }
}

// ==========================================
// ✅ GOOD: Gameplay Tags, Interface/Cast check, and named constants
// ==========================================
#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "GameplayTagContainer.h"
#include "RobustWeapon.generated.h"

UCLASS()
class GAME_API ARobustWeapon : public AActor
{
    GENERATED_BODY()

public:
    void ProcessHitTarget(AActor* HitActor);

private:
    static constexpr float BaseDamageUnits = 45.0f;
    static constexpr float CriticalHitMultiplier = 1.35f;

    // Gameplay tags instead of raw string comparison
    UPROPERTY(EditDefaultsOnly, Category = "Combat|Tags")
    FGameplayTag EnemyTargetTag;
};

// In RobustWeapon.cpp:
void ARobustWeapon::ProcessHitTarget(AActor* HitActor)
{
    if (!HitActor) return;

    // Type-safe Cast or Interface check
    if (ICombatTargetInterface* CombatTarget = Cast<ICombatTargetInterface>(HitActor))
    {
        const float TotalCalculatedDamage = BaseDamageUnits * CriticalHitMultiplier;
        CombatTarget->Execute_ReceiveDamage(HitActor, TotalCalculatedDamage);
    }
}
```
