---
trigger: glob
globs: "**/*.cs"
description: "Unity C# naming, layout and formatting conventions."
paths:
  - "**/*.cs"
---

# Rule: Unity C# Coding & Writing Conventions

These conventions reflect the official Unity Technologies style standards and industry best practices for C# game development in Unity.

---

## 1. Naming Conventions

| Identifier Type | Convention | Example |
| :--- | :--- | :--- |
| **Private / Protected Fields** | `m_` prefix + `camelCase` | `m_currentHealth`, `m_movementSpeed` |
| **Serialized Fields** | `[SerializeField] private` + `m_` prefix + `camelCase` | `[SerializeField] private float m_walkSpeed;` |
| **Static Private Fields** | `s_` prefix + `camelCase` | `s_instanceCount`, `s_cachedMainCamera` |
| **Constants** | `k_` prefix + `camelCase` (or `k_PascalCase`) | `k_maxInventorySlots`, `k_defaultGravity` |
| **Public Properties** | `PascalCase` | `CurrentHealth`, `IsDead`, `MovementVelocity` |
| **Public Methods** | `PascalCase` | `ApplyDamage()`, `TryUnlock()`, `Initialize()` |
| **Private / Helper Methods** | `PascalCase` | `CalculateTraversalTime()`, `ValidateInput()` |
| **Method Parameters** | `camelCase` | `damageAmount`, `targetPosition`, `deltaTime` |
| **Local Variables** | `camelCase` | `remainingDistance`, `hitCollider`, `calculatedDamage` |
| **Classes & Structs** | `PascalCase` | `PlayerMovement`, `CombatAttributes` |
| **Interfaces** | `I` prefix + `PascalCase` | `IDamageable`, `IInteractable`, `ILockable` |
| **Enums & Enum Values** | `PascalCase` | `enum WeaponType { AssaultRifle, Shotgun }` |
| **Events & Delegates** | `On...` prefix in `PascalCase` | `OnHealthChanged`, `OnObjectiveCompleted` |

---

## 2. File & Class Layout Standard

Maintain consistent ordering within every class file:

1. **Static Fields & Constants** (`k_...`, `s_...`)
2. **Serialized Fields** (`[SerializeField] private ... m_...`)
3. **Private Non-Serialized Fields** (`private ... m_...`)
4. **Public / Internal Properties**
5. **Events & Delegates**
6. **Unity Lifecycle Methods** (`Awake`, `OnEnable`, `Start`, `Update`, `FixedUpdate`, `OnDisable`, `OnDestroy`)
7. **Public Methods**
8. **Private / Helper Methods**

---

## 3. Formatting & Idiomatic Unity Rules

- **Allman Braces:** Always place opening and closing braces on a new line.
- **Explicit Access Modifiers:** Always specify `private`, `protected`, or `public`. Do not rely on default implicit visibility.
- **Serialized Field Privacy:** Always declare serialized fields as `[SerializeField] private Type m_variableName;`. Never make fields `public` solely for Unity Inspector exposure.
- **Attributes on Separate Lines:** Put attributes on their own line above the member:
  ```csharp
  [Header("Movement Settings")]
  [SerializeField] private float m_movementSpeed = 5.0f;
  ```
- **Null Checking:** In performance-critical loops (`Update`, `FixedUpdate`), prefer `ReferenceEquals()` or boolean checks over heavy reflection, and use null-conditional operators sparingly with `UnityEngine.Object` subclasses due to Unity's custom equality operators.

---

## 4. Folder & Assembly Layout

Follows [Feature-First Folder Structure](../architecture/feature-first-folder-structure.md).

| Item | Convention | Example |
| :--- | :--- | :--- |
| **Project root** | `Assets/_Project/` | `Assets/_Project/Features/Combat/` |
| **Feature asmdef** | `<Project>.<Feature>` in `Features/<Feature>/Scripts/` | `MyGame.Combat.asmdef` |
| **Shared asmdef** | `<Project>.Shared` | `MyGame.Shared.asmdef` |
| **Editor asmdef** | `<Project>.<Feature>.Editor`, Editor platform only | `MyGame.Combat.Editor.asmdef` |
| **Sandbox asmdef** | `<Project>.<Feature>.Sandbox`, referenced by nothing | `MyGame.Combat.Sandbox.asmdef` |
| **Namespace** | Matches the asmdef / folder | `namespace MyGame.Combat` |
| **File & folder names** | `PascalCase`, no spaces | `WeaponPickup.cs` |

---

## Comprehensive Example

```csharp
using System;
using UnityEngine;

namespace CleanGame.Gameplay.Combat
{
    public class CharacterHealthController : MonoBehaviour, IDamageable
    {
        // 1. Constants & Statics
        private const float k_minimumDamageThreshold = 0.1f;
        private static int s_activeCharactersCount = 0;

        // 2. Serialized Fields
        [Header("Health Configuration")]
        [SerializeField] private float m_maxHealth = 100.0f;
        [SerializeField] private float m_invulnerabilityDurationSeconds = 0.5f;

        // 3. Private Fields
        private float m_currentHealth;
        private float m_invulnerabilityTimerSeconds;
        private bool m_isDead;

        // 4. Properties
        public float CurrentHealth => m_currentHealth;
        public float MaxHealth => m_maxHealth;
        public bool IsDead => m_isDead;
        public float HealthPercentage => m_maxHealth > 0.0f ? m_currentHealth / m_maxHealth : 0.0f;

        // 5. Events
        public event Action<float, float> OnHealthChanged;
        public event Action OnCharacterDied;

        // 6. Unity Lifecycle
        private void Awake()
        {
            m_currentHealth = m_maxHealth;
        }

        private void OnEnable()
        {
            s_activeCharactersCount++;
        }

        private void OnDisable()
        {
            s_activeCharactersCount--;
        }

        private void Update()
        {
            UpdateInvulnerabilityTimer(Time.deltaTime);
        }

        // 7. Public API Methods
        public void ApplyDamage(float damageAmount)
        {
            if (m_isDead || IsInvulnerable() || damageAmount < k_minimumDamageThreshold)
            {
                return;
            }

            m_currentHealth = Mathf.Max(0.0f, m_currentHealth - damageAmount);
            m_invulnerabilityTimerSeconds = m_invulnerabilityDurationSeconds;

            OnHealthChanged?.Invoke(m_currentHealth, m_maxHealth);

            if (m_currentHealth <= 0.0f)
            {
                HandleDeath();
            }
        }

        public void RestoreHealth(float healAmount)
        {
            if (m_isDead || healAmount <= 0.0f)
            {
                return;
            }

            m_currentHealth = Mathf.Min(m_maxHealth, m_currentHealth + healAmount);
            OnHealthChanged?.Invoke(m_currentHealth, m_maxHealth);
        }

        // 8. Private Helper Methods
        private bool IsInvulnerable()
        {
            return m_invulnerabilityTimerSeconds > 0.0f;
        }

        private void UpdateInvulnerabilityTimer(float deltaTime)
        {
            if (m_invulnerabilityTimerSeconds > 0.0f)
            {
                m_invulnerabilityTimerSeconds = Mathf.Max(0.0f, m_invulnerabilityTimerSeconds - deltaTime);
            }
        }

        private void HandleDeath()
        {
            m_isDead = true;
            OnCharacterDied?.Invoke();
        }
    }
}
```
