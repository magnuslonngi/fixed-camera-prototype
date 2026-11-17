---
trigger: glob
globs: "**/*.h, **/*.hpp, **/*.cpp, **/*.inl"
description: "Unreal Engine 5 C++ naming, prefix and header conventions."
paths:
  - "**/*.{h,hpp,cpp,inl}"
---

# Rule: Unreal Engine C++ Coding & Writing Conventions

These conventions strictly follow the official Epic Games Unreal Engine Coding Standard for UE5 C++.

---

## 1. Naming Conventions & Type Prefixes

In Unreal Engine, names must be descriptive and PascalCase. Every type is prefixed according to its inheritance:

| Prefix | Type Description | Example |
| :--- | :--- | :--- |
| **`A`** | Actor-derived classes | `APlayerCharacter`, `ACombatGameMode` |
| **`U`** | `UObject`-derived classes / Components | `UHealthComponent`, `UPlayerAnimInstance` |
| **`F`** | Plain C++ structs / Non-UObject classes | `FCharacterAttributes`, `FHitDamageResult` |
| **`T`** | Template classes & containers | `TArray<T>`, `TMap<Key, Value>`, `TObjectPtr<T>` |
| **`I`** | Interface classes | `IInteractableInterface`, `IDamageableInterface` |
| **`E`** | Enumeration types | `enum class ECombatState : uint8` |
| **`b`** | Boolean member variables & locals | `bIsSprinting`, `bCanDoubleJump`, `bIsDead` |
| **`S`** | Slate widget classes | `SCompoundWidget`, `SHealthHUDWidget` |

---

## 2. Casing Standards

- **All Symbols are PascalCase:** Functions (`TakeDamage`), Properties (`MaxWalkSpeed`), Parameters (`InDamageAmount`), and Local Variables (`RemainingDistance`).
- **No Hungarian Notation:** Do not add type markers like `iIndex` or `fSpeed`. Rely on type safety and clear names.
- **Pointers in UE5:** Use `TObjectPtr<T>` for member variables of UObject/Actor types; use raw pointers (`T*`) for local variables and function parameters.

---

## 3. Header File Layout Standard

1. `#pragma once`
2. Includes (`CoreMinimal.h`, base class header, types, then `.generated.h` **always last**)
3. Delegates declaration (`DECLARE_DYNAMIC_MULTICAST_DELEGATE...`)
4. Class declaration with `UCLASS()` macro
5. `GENERATED_BODY()`
6. Public section:
   - Constructor
   - Public API methods (`UFUNCTION` macros as needed)
   - Public Delegates / Events
   - Getters/Setters
7. Protected section:
   - Lifecycle overrides (`BeginPlay`, `TickComponent`, `EndPlay`)
   - Protected helpers
8. Private section:
   - Configuration properties (`UPROPERTY(EditDefaultsOnly, ...)`)
   - Runtime state properties (`UPROPERTY(VisibleInstanceOnly, ...)`)
   - Private helper methods

---

## 4. Folder & Module Layout

Follows [Feature-First Folder Structure](../architecture/feature-first-folder-structure.md).

- Content: `Content/<Project>/{Maps,Shared,Features/<Feature>}`; marketplace packs stay at the `Content/` root.
- Source: one game module, `Source/<Project>/{Shared,Features/<Feature>}/{Public,Private}`. Headers in `Public/`, `.cpp` in `Private/`.
- Separate modules only for plugins: `Plugins/<Plugin>/Source/<Module>/`.
- Asset names: `Prefix_BaseName_Descriptor_Variant` (`BP_Weapon_Rifle`, `T_Weapon_Rifle_BaseColor`).

---

## Comprehensive Example

### Header: `CharacterHealthComponent.h`

```cpp
#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "CharacterHealthComponent.generated.h"

DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FOnHealthChangedSignature, float, CurrentHealth, float, MaxHealth);
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnCharacterDiedSignature);

UENUM(BlueprintType)
enum class EDamageTypeCategory : uint8
{
    Physical    UMETA(DisplayName = "Physical"),
    Energy      UMETA(DisplayName = "Energy"),
    Environmental UMETA(DisplayName = "Environmental")
};

UCLASS(ClassGroup=(Combat), meta=(BlueprintSpawnableComponent))
class GAME_API UCharacterHealthComponent : public UActorComponent
{
    GENERATED_BODY()

public:
    UCharacterHealthComponent();

    UFUNCTION(BlueprintCallable, Category = "Combat|Health")
    void ApplyDamage(float InDamageAmount, EDamageTypeCategory InDamageType);

    UFUNCTION(BlueprintCallable, Category = "Combat|Health")
    void RestoreHealth(float InHealAmount);

    UFUNCTION(BlueprintPure, Category = "Combat|Health")
    float GetCurrentHealth() const { return CurrentHealth; }

    UFUNCTION(BlueprintPure, Category = "Combat|Health")
    float GetMaxHealth() const { return MaxHealth; }

    UFUNCTION(BlueprintPure, Category = "Combat|Health")
    bool IsDead() const { return bIsDead; }

    UPROPERTY(BlueprintAssignable, Category = "Combat|Events")
    FOnHealthChangedSignature OnHealthChanged;

    UPROPERTY(BlueprintAssignable, Category = "Combat|Events")
    FOnCharacterDiedSignature OnCharacterDied;

protected:
    virtual void BeginPlay() override;

private:
    static constexpr float MinimumDamageThreshold = 0.1f;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Combat|Config", meta = (AllowPrivateAccess = "true"))
    float MaxHealth = 100.0f;

    UPROPERTY(VisibleInstanceOnly, BlueprintReadOnly, Category = "Combat|State", meta = (AllowPrivateAccess = "true"))
    float CurrentHealth = 100.0f;

    UPROPERTY(VisibleInstanceOnly, BlueprintReadOnly, Category = "Combat|State", meta = (AllowPrivateAccess = "true"))
    bool bIsDead = false;

    void HandleDeath();
};
```

### Source: `CharacterHealthComponent.cpp`

```cpp
#include "CharacterHealthComponent.h"

UCharacterHealthComponent::UCharacterHealthComponent()
{
    PrimaryComponentTick.bCanEverTick = false;
}

void UCharacterHealthComponent::BeginPlay()
{
    Super::BeginPlay();
    CurrentHealth = MaxHealth;
}

void UCharacterHealthComponent::ApplyDamage(float InDamageAmount, EDamageTypeCategory InDamageType)
{
    if (bIsDead || InDamageAmount < MinimumDamageThreshold)
    {
        return;
    }

    CurrentHealth = FMath::Clamp(CurrentHealth - InDamageAmount, 0.0f, MaxHealth);
    OnHealthChanged.Broadcast(CurrentHealth, MaxHealth);

    if (FMath::IsNearlyZero(CurrentHealth))
    {
        HandleDeath();
    }
}

void UCharacterHealthComponent::RestoreHealth(float InHealAmount)
{
    if (bIsDead || InHealAmount <= 0.0f)
    {
        return;
    }

    CurrentHealth = FMath::Clamp(CurrentHealth + InHealAmount, 0.0f, MaxHealth);
    OnHealthChanged.Broadcast(CurrentHealth, MaxHealth);
}

void UCharacterHealthComponent::HandleDeath()
{
    bIsDead = true;
    OnCharacterDied.Broadcast();
}
```
