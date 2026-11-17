---
trigger: always_on
description: "Decoupling & Event-Driven Presentation"
---

# Rule: Decoupling & Event-Driven Presentation

Systems must know as little about each other as possible. Decoupling ensures you can alter, replace, or completely delete visual or audio presentations without touching core gameplay rules.

---

## Core Guidelines

1. **Strict Separation of Logic from Presentation**
   - Core gameplay logic (damage formulas, quest progress, inventory counts, door locks) must have **zero direct dependencies or references** to:
     - Renderers or mesh components
     - Animation controllers or animation players
     - Audio sources or sound managers
     - UI widgets, text fields, or canvas elements
   - Presentation listens to Logic. Logic never drives Presentation directly.

2. **The Observer Pattern (Events, Signals, Delegates)**
   - When core gameplay state mutates, the logic script publishes an event (e.g., `OnHealthChanged(current, max)`, `OnItemCollected(itemData)`).
   - Presentation layers (UI widgets, particle spawners, audio players) subscribe to the event and execute visual/audio feedback independently.

3. **The "Delete Test" (Acid Test for Decoupling)**
   - *Test:* If you delete or exclude the entire UI, Audio, and Animation folders from the project, the core gameplay logic **must still compile and execute cleanly in headless/server mode without throwing null references**.

---

## Examples

### 1. Unity (C#)

```csharp
// ==========================================
// ❌ BAD: Keypad lock directly couples to UI text and AudioSource
// ==========================================
public class KeypadLockCoupled : MonoBehaviour
{
    public TMPro.TextMeshProUGUI displayText;
    public AudioSource buzzerAudioSource;
    public Animator doorAnimator;

    private string enteredCode = "";
    private const string CORRECT_CODE = "1234";

    public void AddDigit(string digit)
    {
        enteredCode += digit;
        displayText.text = enteredCode; // Tight coupling to UI

        if (enteredCode.Length == CORRECT_CODE.Length)
        {
            if (enteredCode == CORRECT_CODE)
            {
                doorAnimator.SetTrigger("Open"); // Tight coupling to Animator
            }
            else
            {
                buzzerAudioSource.Play(); // Tight coupling to Audio
                enteredCode = "";
                displayText.text = "";
            }
        }
    }
}

// ==========================================
// ✅ GOOD: Pure logic keypad with event-driven presenters using m_ conventions
// ==========================================

// --- Core Logic (Zero UI/Audio references, passes Delete Test) ---
public class KeypadLock : MonoBehaviour
{
    [SerializeField] private string m_correctCombination = "1234";
    private string m_currentInput = "";

    public event System.Action<string> OnInputChanged;
    public event System.Action OnCodeAccepted;
    public event System.Action OnCodeRejected;

    public void EnterDigit(char digit)
    {
        if (m_currentInput.Length >= m_correctCombination.Length) return;

        m_currentInput += digit;
        OnInputChanged?.Invoke(m_currentInput);

        if (m_currentInput.Length == m_correctCombination.Length)
        {
            if (m_currentInput == m_correctCombination)
            {
                OnCodeAccepted?.Invoke();
            }
            else
            {
                m_currentInput = "";
                OnCodeRejected?.Invoke();
                OnInputChanged?.Invoke(m_currentInput);
            }
        }
    }
}

// --- UI Presenter (Subscribes to Logic events) ---
public class KeypadDisplayUI : MonoBehaviour
{
    [SerializeField] private KeypadLock m_keypadLock;
    [SerializeField] private TMPro.TextMeshProUGUI m_codeDisplayText;

    private void OnEnable() => m_keypadLock.OnInputChanged += UpdateDisplay;
    private void OnDisable() => m_keypadLock.OnInputChanged -= UpdateDisplay;

    private void UpdateDisplay(string currentInput)
    {
        m_codeDisplayText.text = currentInput;
    }
}
```

---

### 2. Godot (GDScript)

```gdscript
# ==========================================
# ❌ BAD: Core inventory directly modifies labels and plays sounds
# ==========================================
class_name CoupledInventory extends Node

var coin_count: int = 0
@onready var coin_label: Label = $"../UI/CoinLabel"
@onready var coin_audio: AudioStreamPlayer = $"../Audio/CoinSound"

func add_coin() -> void:
	coin_count += 1
	coin_label.text = "Coins: %d" % coin_count # Breaks if UI changes
	coin_audio.play()                          # Breaks if Audio node moves

# ==========================================
# ✅ GOOD: Logic emits signals; UI and Audio connect independently
# ==========================================

# --- Core Logic: res://scripts/inventory.gd ---
class_name Inventory extends Node

signal item_count_changed(item_identifier: StringName, current_count: int)

var _item_storage: Dictionary = {}

func add_item(item_identifier: StringName, amount: int = 1) -> void:
	var existing_amount: int = _item_storage.get(item_identifier, 0)
	var new_amount: int = existing_amount + amount
	_item_storage[item_identifier] = new_amount
	item_count_changed.emit(item_identifier, new_amount)

# --- UI Presenter: res://scripts/ui/inventory_label.gd ---
class_name InventoryItemLabelUI extends Label

@export var watched_item: StringName = &"gold_coin"
@export var inventory_source: Inventory

func _ready() -> void:
	if inventory_source:
		inventory_source.item_count_changed.connect(_on_item_count_changed)

func _on_item_count_changed(item_identifier: StringName, current_count: int) -> void:
	if item_identifier == watched_item:
		text = "Coins: %d" % current_count
```

---

### 3. Unreal Engine (C++)

```cpp
// ==========================================
// ❌ BAD: Gameplay actor directly controls Widget and Audio Component
// ==========================================
void ABadGameMode::CompleteObjective()
{
    Score += 100;
    // Direct pointer to UI widget and Audio inside core rule system
    if (ScoreWidget) ScoreWidget->SetScoreText(Score);
    if (VictoryAudio) VictoryAudio->Play();
}

// ==========================================
// ✅ GOOD: Delegate-based architecture separating logic from presentation
// ==========================================

// --- GameplayObjectiveComponent.h (Pure Gameplay Logic) ---
#pragma once
#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "GameplayObjectiveComponent.generated.h"

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnScoreChangedDelegate, int32, NewScore);
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnObjectiveCompletedDelegate);

UCLASS(ClassGroup=(Custom), meta=(BlueprintSpawnableComponent))
class GAME_API UGameplayObjectiveComponent : public UActorComponent
{
    GENERATED_BODY()

public:
    void AwardScore(int32 AddedPoints)
    {
        TotalScore += AddedPoints;
        OnScoreChanged.Broadcast(TotalScore);

        if (TotalScore >= TargetScore)
        {
            OnObjectiveCompleted.Broadcast();
        }
    }

    UPROPERTY(BlueprintAssignable, Category = "Events")
    FOnScoreChangedDelegate OnScoreChanged;

    UPROPERTY(BlueprintAssignable, Category = "Events")
    FOnObjectiveCompletedDelegate OnObjectiveCompleted;

private:
    UPROPERTY(EditDefaultsOnly, Category = "Objective")
    int32 TargetScore = 1000;

    int32 TotalScore = 0;
};

// --- ObjectiveWidgetPresenter.h (UI layer binds to delegate) ---
UCLASS()
class GAME_API UObjectiveWidgetPresenter : public UUserWidget
{
    GENERATED_BODY()

public:
    void BindToObjective(UGameplayObjectiveComponent* ObjectiveComponent)
    {
        if (ObjectiveComponent)
        {
            ObjectiveComponent->OnScoreChanged.AddDynamic(this, &UObjectiveWidgetPresenter::HandleScoreUpdated);
        }
    }

private:
    UFUNCTION()
    void HandleScoreUpdated(int32 NewScore);
};
```
