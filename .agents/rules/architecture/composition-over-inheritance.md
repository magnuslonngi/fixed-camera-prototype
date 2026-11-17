---
trigger: always_on
description: "Composition Over Inheritance"
---

# Rule: Composition Over Inheritance

Prefer composing game entities from modular components and focused interfaces over creating deep inheritance hierarchies. Inheritance tightly binds subclasses to parent implementations, leading to fragile base classes and bloated "God" base objects.

---

## Core Guidelines

1. **Avoid Deep Inheritance Trees (Max Depth: 1-2 Levels)**
   - Never force disparate objects into a shared base class just to reuse a fraction of functionality.
   - Example Anti-Pattern: Making `SecurityTerminal` inherit from `Door` or `OpenableObject` just because both have a lock mechanism, leaving `SecurityTerminal` with an irrelevant, dummy `Open()` method.

2. **Compose with Focused Components**
   - Game objects should be containers of components:
     - A locked door contains: `MeshRenderer`, `InteractableComponent`, `LockComponent`, `DoorMotorComponent`.
     - A locked computer contains: `MeshRenderer`, `InteractableComponent`, `LockComponent`, `TerminalScreenComponent`.
   - Both share the exact same `LockComponent` behavior without inheriting from each other.

3. **Use Lean Interfaces for Polymorphic Interactions**
   - Define minimal, capability-focused interfaces (e.g., `IInteractable`, `IDamageable`, `ILockable`).
   - Systems interact with interfaces, unaware of whether the target is an actor, pawn, vehicle, or breakable crate.

---

## Examples

### 1. Unity (C#)

```csharp
// ==========================================
// ❌ BAD: Deep, brittle inheritance forcing irrelevant methods
// ==========================================
public abstract class WorldEntity : MonoBehaviour
{
    public abstract void Interact();
    public abstract void Open(); // Breaks for objects that don't open!
    public abstract void Lock();
}

public class SecurityTerminalBad : WorldEntity
{
    public override void Interact() { /* Hack */ }
    public override void Open() { throw new System.NotImplementedException(); } // Junk code!
    public override void Lock() { /* Lock logic */ }
}

// ==========================================
// ✅ GOOD: Lean interfaces and attachable components with m_ conventions
// ==========================================

public interface IInteractable
{
    void Interact(GameObject instigator);
}

public interface ILockable
{
    bool IsLocked { get; }
    bool TryUnlock(string keycode);
}

// Reusable lock component attached to doors, chests, or terminals
public class LockComponent : MonoBehaviour, ILockable
{
    [SerializeField] private string m_requiredKeycode = "DELTA-9";
    private bool m_isLocked = true;

    public bool IsLocked => m_isLocked;

    public bool TryUnlock(string keycode)
    {
        if (keycode == m_requiredKeycode)
        {
            m_isLocked = false;
            return true;
        }
        return false;
    }
}

// Composed Terminal entity
public class SecurityTerminal : MonoBehaviour, IInteractable
{
    [SerializeField] private LockComponent m_lockComponent;

    public void Interact(GameObject instigator)
    {
        if (m_lockComponent.IsLocked)
        {
            // Prompt keycode UI
            return;
        }

        // Open terminal console
    }
}
```

---

### 2. Godot (GDScript)

```gdscript
# ==========================================
# ❌ BAD: Inheritance tree forcing unused methods
# ==========================================
# res://scripts/base_interactive_object.gd
class_name BaseInteractiveObject extends Node3D

func interact() -> void: pass
func open_door() -> void: pass # Irrelevant for a computer terminal!
func take_damage(amount: int) -> void: pass

# ==========================================
# ✅ GOOD: Modular child node components
# ==========================================

# --- InteractionArea Component (Child node in scene) ---
# res://scripts/components/interaction_component.gd
class_name InteractionComponent extends Area3D

signal interaction_triggered(instigator: Node3D)

@export var prompt_message: String = "Interact"
@export var is_interaction_enabled: bool = true

func interact(instigator: Node3D) -> void:
	if is_interaction_enabled:
		interaction_triggered.emit(instigator)

# --- Lock Component (Child node in scene) ---
# res://scripts/components/lock_component.gd
class_name LockComponent extends Node

signal unlocked
signal unlock_failed

@export var required_key_id: StringName = &"key_red"
var _is_locked: bool = true

func is_locked() -> bool:
	return _is_locked

func attempt_unlock(provided_key_id: StringName) -> bool:
	if provided_key_id == required_key_id:
		_is_locked = false
		unlocked.emit()
		return true
	unlock_failed.emit()
	return false

# --- Assembled Door (Composes Interaction and Lock child nodes) ---
# res://scripts/entities/vault_door.gd
class_name VaultDoor extends Node3D

@onready var interaction_component: InteractionComponent = $InteractionComponent
@onready var lock_component: LockComponent = $LockComponent

func _ready() -> void:
	interaction_component.interaction_triggered.connect(_on_interacted)

func _on_interacted(instigator: Node3D) -> void:
	if lock_component.is_locked():
		# Notify player door is locked
		return
	# Open door animation
```

---

### 3. Unreal Engine (C++)

```cpp
// ==========================================
// ❌ BAD: Deep Actor inheritance hierarchy
// ==========================================
// AActor -> AWorldObject -> AOpenableObject -> AComputerTerminal
// AComputerTerminal now inherits door meshes, rotation angles, and swing speeds.

// ==========================================
// ✅ GOOD: Unreal Interfaces + Actor Components
// ==========================================

// --- InteractableInterface.h ---
#pragma once
#include "CoreMinimal.h"
#include "UObject/Interface.h"
#include "InteractableInterface.generated.h"

UINTERFACE(MinimalAPI, Blueprintable)
class UInteractableInterface : public UInterface { GENERATED_BODY() };

class GAME_API IInteractableInterface
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintNativeEvent, BlueprintCallable, Category = "Interaction")
    void ExecuteInteraction(AActor* InstigatorActor);
};

// --- LockComponent.h (Reusable Actor Component) ---
#pragma once
#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "LockComponent.generated.h"

UCLASS(ClassGroup=(Custom), meta=(BlueprintSpawnableComponent))
class GAME_API ULockComponent : public UActorComponent
{
    GENERATED_BODY()

public:
    bool TryUnlockWithKey(FName KeyId)
    {
        if (KeyId == RequiredKeyId)
        {
            bIsLocked = false;
            return true;
        }
        return false;
    }

    bool IsLocked() const { return bIsLocked; }

private:
    UPROPERTY(EditAnywhere, Category = "Lock")
    FName RequiredKeyId = FName("Keycard_Alpha");

    bool bIsLocked = true;
};

// --- ComputerTerminalActor.h (Composed Actor) ---
#pragma once
#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "InteractableInterface.h"
#include "ComputerTerminalActor.generated.h"

class ULockComponent;

UCLASS()
class GAME_API AComputerTerminalActor : public AActor, public IInteractableInterface
{
    GENERATED_BODY()

public:
    AComputerTerminalActor()
    {
        LockComponent = CreateDefaultSubobject<ULockComponent>(TEXT("LockComponent"));
    }

    virtual void ExecuteInteraction_Implementation(AActor* InstigatorActor) override
    {
        if (LockComponent->IsLocked())
        {
            // Prompt keycode screen
            return;
        }
        // Access terminal
    }

private:
    UPROPERTY(VisibleAnywhere, Category = "Components")
    TObjectPtr<ULockComponent> LockComponent;
};
```
