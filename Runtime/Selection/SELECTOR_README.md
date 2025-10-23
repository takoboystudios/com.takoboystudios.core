# Selector - Weighted Random Selection System

A feature-complete, production-grade weighted random selection system for Unity games.

## 📦 Files

- **ISelectable.cs** - Interface for selectable items
- **Selector.cs** - Main public API
- **Selector.Internal.cs** - Internal implementation (partial class)

## ✨ Features

### Core Selection
- ✅ **Single item selection** - Select one item based on weights
- ✅ **Multiple item selection** - Select multiple items (with replacement)
- ✅ **Distinct selection** - Select multiple unique items (without replacement)
- ✅ **Try methods** - Non-throwing alternatives for error handling

### Bonus System
- ✅ **Weight adjustment** - Push weights toward mean or median
- ✅ **Flexible pivots** - Choose between Mean or Median as pivot point
- ✅ **Range control** - Adjust weights BelowPivot, AbovePivot, or All
- ✅ **Asymptotic adjustment** - Safe adjustments that never overshoot

### Advanced Features
- ✅ **Seeded random** - Reproducible results for testing/debugging
- ✅ **Selection history** - Track recent selections
- ✅ **Probability calculation** - Get selection chances for each item
- ✅ **Weight caching** - Optimized performance for repeated selections
- ✅ **Builder pattern** - Fluent configuration API

### Quality
- ✅ **Comprehensive validation** - Clear error messages
- ✅ **Performance optimized** - Binary search O(log n) selection
- ✅ **Fully documented** - XML docs with examples
- ✅ **Production ready** - Thorough error handling

## 🚀 Quick Start

### Basic Usage

```csharp
using TakoBoyStudios.Core;

// 1. Create items that implement ISelectable
public class LootItem : ISelectable
{
    public string Name { get; set; }
    public float SelectionWeight { get; set; }
    
    public LootItem(string name, float weight)
    {
        Name = name;
        SelectionWeight = weight;
    }
}

// 2. Create a collection
var loot = new List<LootItem>
{
    new LootItem("Common Sword", 70f),    // 70% chance
    new LootItem("Rare Armor", 25f),      // 25% chance
    new LootItem("Legendary Ring", 5f)    // 5% chance
};

// 3. Create selector and select
var selector = new Selector();
var selectedItem = selector.SelectSingle(loot);
Debug.Log($"You got: {selectedItem.Name}");
```

## 📖 Usage Examples

### Single Selection

```csharp
// Basic selection
var selector = new Selector();
var item = selector.SelectSingle(items);

// With bonus (makes selection more balanced)
var item = selector.SelectSingle(items, bonus: 50f);

// Safe selection without exceptions
if (selector.TrySelectSingle(items, out var result))
{
    Debug.Log($"Selected: {result.Name}");
}
```

### Multiple Selection

```csharp
var selector = new Selector();

// Select 5 items (duplicates possible)
var items = selector.SelectMultiple(candidates, 5);

// Select 5 items with bonus
var items = selector.SelectMultiple(candidates, 5, bonus: 30f);

// Select 5 unique items (no duplicates)
var uniqueItems = selector.SelectDistinct(candidates, 5);
```

### Bonus System

The bonus system adjusts weights to make selections more balanced:

```csharp
// Create selector with specific configuration
var selector = new Selector(
    pivot: SelectionPivot.Mean,     // Push toward average
    range: SelectionRange.All       // Adjust all weights
);

// Higher bonus = more balanced selection
var item = selector.SelectSingle(items, bonus: 100f);
```

**Bonus values:**
- `0` - No adjustment (pure weighted selection)
- `50` - Moderate balancing
- `100` - Strong balancing
- `200+` - Very strong balancing (approaches equal probability)

**Pivot types:**
- `SelectionPivot.Mean` - Push weights toward the average
- `SelectionPivot.Median` - Push weights toward the middle value (less affected by outliers)

**Range types:**
- `SelectionRange.BelowPivot` - Only boost low-weight items
- `SelectionRange.AbovePivot` - Only reduce high-weight items
- `SelectionRange.All` - Adjust all items toward balance

### Seeded Random (Reproducible Results)

```csharp
// Create selector with seed
var selector = new Selector(seed: 12345);

// Will always return the same sequence of selections
var item1 = selector.SelectSingle(items); // Always same
var item2 = selector.SelectSingle(items); // Always same
var item3 = selector.SelectSingle(items); // Always same
```

**Use cases for seeded random:**
- Unit testing
- Replays and demo modes
- Debugging specific scenarios
- Synchronized multiplayer

### Selection History

```csharp
// Create selector with history tracking
var selector = Selector.CreateBuilder()
    .WithHistorySize(10)
    .Build();

// Make selections
var item1 = selector.SelectSingle(items);
var item2 = selector.SelectSingle(items);

// Check history
var history = selector.GetHistory<LootItem>();
Debug.Log($"Selected {history.Length} items");

// Check if item was recently selected
if (selector.WasRecentlySelected(someItem))
{
    Debug.Log("This item was selected recently");
}

// Clear history
selector.ClearHistory();
```

### Probability Analysis

```csharp
var selector = new Selector();

// Get selection probabilities (without bonus)
var probabilities = selector.GetProbabilities(items);
foreach (var kvp in probabilities)
{
    Debug.Log($"{kvp.Key.Name}: {kvp.Value * 100:F2}% chance");
}

// Get probabilities with bonus
var adjustedProbs = selector.GetProbabilities(items, bonus: 50f);

// Get adjusted weights
var weights = selector.GetAdjustedWeights(items, bonus: 50f);
```

### Builder Pattern

```csharp
var selector = Selector.CreateBuilder()
    .WithSeed(12345)                        // Reproducible results
    .WithPivot(SelectionPivot.Median)       // Use median as pivot
    .WithRange(SelectionRange.BelowPivot)   // Only boost low weights
    .WithHistorySize(20)                    // Track last 20 selections
    .Build();

var item = selector.SelectSingle(items, bonus: 50f);
```

## 🎮 Real-World Examples

### Loot Drop System

```csharp
public class LootDropper : MonoBehaviour
{
    [SerializeField] private List<LootItem> possibleLoot;
    private Selector selector;

    void Start()
    {
        // Create selector with history to avoid repetition
        selector = Selector.CreateBuilder()
            .WithHistorySize(5)
            .Build();
    }

    public void DropLoot()
    {
        // Bonus increases with player's luck stat
        float bonus = playerLuckStat * 10f;
        
        var loot = selector.SelectSingle(possibleLoot, bonus);
        SpawnLootInWorld(loot);
    }
}
```

### Enemy Spawn System

```csharp
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<EnemyType> enemyTypes;
    private Selector selector;

    void Start()
    {
        // Seeded selector for consistent waves
        selector = new Selector(seed: GameManager.Instance.WaveSeed);
    }

    public void SpawnWave(int enemyCount)
    {
        // Get diverse enemy composition
        var enemies = selector.SelectDistinct(enemyTypes, enemyCount);
        
        foreach (var enemy in enemies)
        {
            SpawnEnemy(enemy);
        }
    }
}
```

### Reward Wheel

```csharp
public class RewardWheel : MonoBehaviour
{
    [SerializeField] private List<Reward> rewards;
    private Selector selector;

    void Start()
    {
        selector = new Selector(
            SelectionPivot.Mean,
            SelectionRange.All
        );
    }

    public void Spin(bool isLuckyBonus)
    {
        // Lucky spin increases bonus
        float bonus = isLuckyBonus ? 100f : 0f;
        
        var reward = selector.SelectSingle(rewards, bonus);
        GiveRewardToPlayer(reward);
    }

    public void ShowOdds()
    {
        var probabilities = selector.GetProbabilities(rewards);
        
        foreach (var kvp in probabilities)
        {
            Debug.Log($"{kvp.Key.Name}: {kvp.Value:P2}");
        }
    }
}
```

## 🔧 Advanced Usage

### Custom Weight Calculations

```csharp
public class DynamicLoot : ISelectable
{
    public string Name { get; set; }
    private float baseWeight;
    private int playerLevel;

    public float SelectionWeight
    {
        get => CalculateWeight();
        set => baseWeight = value;
    }

    private float CalculateWeight()
    {
        // Weight increases with player level
        return baseWeight * (1f + playerLevel * 0.1f);
    }
}
```

### Weighted Selection with Filters

```csharp
// Filter items before selection
var availableItems = allItems.Where(i => i.IsUnlocked).ToList();
var selected = selector.SelectSingle(availableItems);
```

### Batch Selection with Analysis

```csharp
// Select multiple and analyze distribution
var selections = new List<Item>();
for (int i = 0; i < 1000; i++)
{
    selections.Add(selector.SelectSingle(items));
}

// Analyze results
var distribution = selections
    .GroupBy(s => s.Name)
    .Select(g => new { Name = g.Key, Count = g.Count() })
    .OrderByDescending(x => x.Count);

foreach (var dist in distribution)
{
    Debug.Log($"{dist.Name}: {dist.Count} times");
}
```

## ⚡ Performance Tips

1. **Reuse selectors** - Create once, use many times
2. **Cache results** - Weights are cached internally
3. **Clear cache** - Call `ClearCache()` if weights change
4. **Use arrays** - Arrays are more efficient than Lists
5. **Distinct selection** - More expensive than regular selection

## 🐛 Troubleshooting

### All items have the same selection chance
- Check that weights are different
- Verify bonus isn't too high (>500 effectively equalizes)

### Getting ArgumentException
- Ensure at least one item has positive weight
- Check collection isn't empty
- Verify count doesn't exceed collection size (for distinct selection)

### Selection seems biased
- Use `GetProbabilities()` to verify actual chances
- Consider using seeded random for reproducible testing
- Check if history size is affecting results

## 📊 Algorithm Details

### Selection Algorithm
- Uses cumulative weights for O(log n) binary search
- Supports both Unity.Random and System.Random
- Handles negative weights (clamped to zero)

### Bonus Adjustment Formula
```
adjustedWeight = weight ± (pivot * (bonus / (bonus + 100)))
```

This ensures:
- Asymptotic approach (never overshoots pivot)
- Infinite bonus approaches equal distribution
- Zero bonus leaves weights unchanged

## 📝 License

Part of TakoBoyStudios.Core package - use freely in your projects!

## 🙋 Support

For issues or questions, check the inline XML documentation or examine the provided examples.
