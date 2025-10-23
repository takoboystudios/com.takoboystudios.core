using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TakoBoyStudios.Core
{
    /// <summary>
    /// Defines how the pivot value is calculated when applying bonus adjustments to selection weights.
    /// </summary>
    public enum SelectionPivot
    {
        /// <summary>
        /// Uses the median (middle value) of all weights as the pivot point.
        /// </summary>
        /// <remarks>
        /// The median is less affected by outliers than the mean, making it useful
        /// when you have a few items with extremely high or low weights.
        /// </remarks>
        Median,

        /// <summary>
        /// Uses the mean (average) of all weights as the pivot point.
        /// </summary>
        /// <remarks>
        /// The mean considers all weight values equally, which can be affected by outliers
        /// but provides a true average of the weight distribution.
        /// </remarks>
        Mean
    }

    /// <summary>
    /// Defines which subset of items will have bonus adjustments applied to their weights.
    /// </summary>
    public enum SelectionRange
    {
        /// <summary>
        /// Applies bonus adjustments only to items with weights below the pivot.
        /// </summary>
        /// <remarks>
        /// Increases the selection probability of lower-weighted items, making
        /// the selection more balanced without affecting high-weight items.
        /// </remarks>
        BelowPivot,

        /// <summary>
        /// Applies bonus adjustments only to items with weights above the pivot.
        /// </summary>
        /// <remarks>
        /// Decreases the selection probability of higher-weighted items, making
        /// the selection more balanced without affecting low-weight items.
        /// </remarks>
        AbovePivot,

        /// <summary>
        /// Applies bonus adjustments to all items regardless of their relationship to the pivot.
        /// </summary>
        /// <remarks>
        /// Pushes all weights toward the pivot value, creating a more pronounced
        /// equalization effect than BelowPivot or AbovePivot modes.
        /// </remarks>
        All
    }

    /// <summary>
    /// Performs weighted random selection from collections of ISelectable objects with support
    /// for bonus adjustments, seeded randomization, selection history, and various selection modes.
    /// </summary>
    /// <remarks>
    /// The Selector class provides a flexible system for weighted random selection with features including:
    /// - Single and multiple item selection
    /// - Bonus system to adjust weights toward mean/median
    /// - Seeded random for reproducible results
    /// - Selection history tracking to avoid recent duplicates
    /// - Selection with or without replacement
    /// - Probability calculation and diagnostics
    /// - Builder pattern for complex configurations
    /// 
    /// The bonus system allows you to "sweeten" selections by pushing weights toward a pivot point
    /// (mean or median), making selections more balanced. Higher bonus values drive weights closer
    /// to uniform distribution, while bonus of 0 maintains original weights.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Basic usage
    /// var selector = new Selector();
    /// var item = selector.SelectSingle(items);
    /// 
    /// // With bonus to balance selection
    /// var selector = new Selector(SelectionPivot.Mean, SelectionRange.All);
    /// var item = selector.SelectSingle(items, bonus: 50f);
    /// 
    /// // Seeded for reproducible results
    /// var selector = new Selector(seed: 12345);
    /// var items = selector.SelectMultiple(candidates, 3);
    /// 
    /// // With builder pattern
    /// var selector = Selector.CreateBuilder()
    ///     .WithSeed(12345)
    ///     .WithPivot(SelectionPivot.Median)
    ///     .WithRange(SelectionRange.BelowPivot)
    ///     .WithHistorySize(10)
    ///     .Build();
    /// </code>
    /// </example>
    public partial class Selector
    {
        #region Private Fields

        private readonly SelectionPivot _pivot;
        private readonly SelectionRange _range;
        private readonly System.Random _random;
        private readonly bool _useSeededRandom;
        private readonly Queue<object> _selectionHistory;
        private readonly int _maxHistorySize;
        private readonly Dictionary<string, float[]> _cachedWeights;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the selection pivot mode used by this selector.
        /// </summary>
        public SelectionPivot Pivot => _pivot;

        /// <summary>
        /// Gets the selection range mode used by this selector.
        /// </summary>
        public SelectionRange Range => _range;

        /// <summary>
        /// Gets whether this selector uses a seeded random generator.
        /// </summary>
        public bool IsSeeded => _useSeededRandom;

        /// <summary>
        /// Gets the maximum number of selections tracked in history.
        /// </summary>
        public int MaxHistorySize => _maxHistorySize;

        /// <summary>
        /// Gets the current number of selections in history.
        /// </summary>
        public int HistoryCount => _selectionHistory.Count;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a selector with default settings (Mean pivot, All range, Unity's random).
        /// </summary>
        public Selector() : this(SelectionPivot.Mean, SelectionRange.All)
        {
        }

        /// <summary>
        /// Creates a selector with the specified pivot and range settings.
        /// </summary>
        /// <param name="pivot">The pivot point for bonus adjustments</param>
        /// <param name="range">The range of items to apply bonus adjustments to</param>
        public Selector(SelectionPivot pivot, SelectionRange range)
        {
            _pivot = pivot;
            _range = range;
            _useSeededRandom = false;
            _random = null;
            _selectionHistory = new Queue<object>();
            _maxHistorySize = 0;
            _cachedWeights = new Dictionary<string, float[]>();
        }

        /// <summary>
        /// Creates a selector with a seed for reproducible random results.
        /// </summary>
        /// <param name="seed">The seed value for the random number generator</param>
        /// <param name="pivot">The pivot point for bonus adjustments</param>
        /// <param name="range">The range of items to apply bonus adjustments to</param>
        public Selector(int seed, SelectionPivot pivot = SelectionPivot.Mean, SelectionRange range = SelectionRange.All)
        {
            _pivot = pivot;
            _range = range;
            _useSeededRandom = true;
            _random = new System.Random(seed);
            _selectionHistory = new Queue<object>();
            _maxHistorySize = 0;
            _cachedWeights = new Dictionary<string, float[]>();
        }

        /// <summary>
        /// Internal constructor used by the builder pattern.
        /// </summary>
        private Selector(SelectionPivot pivot, SelectionRange range, int? seed, int historySize)
        {
            _pivot = pivot;
            _range = range;
            _maxHistorySize = historySize;
            _selectionHistory = new Queue<object>(historySize);
            _cachedWeights = new Dictionary<string, float[]>();

            if (seed.HasValue)
            {
                _useSeededRandom = true;
                _random = new System.Random(seed.Value);
            }
            else
            {
                _useSeededRandom = false;
                _random = null;
            }
        }

        #endregion

        #region Single Selection

        /// <summary>
        /// Selects a single item from the collection based on weight.
        /// </summary>
        /// <typeparam name="T">The type of selectable item</typeparam>
        /// <param name="candidates">The collection to select from</param>
        /// <returns>A single selected item</returns>
        /// <exception cref="ArgumentNullException">Thrown if candidates is null</exception>
        /// <exception cref="ArgumentException">Thrown if candidates is empty or contains only zero/negative weights</exception>
        public T SelectSingle<T>(IEnumerable<T> candidates) where T : ISelectable
        {
            return SelectSingle(candidates, 0f);
        }

        /// <summary>
        /// Selects a single item from the collection based on weight, adjusted by a bonus value.
        /// </summary>
        /// <typeparam name="T">The type of selectable item</typeparam>
        /// <param name="candidates">The collection to select from</param>
        /// <param name="bonus">The degree to which weights are pushed toward the pivot (0 = no adjustment, higher = more balanced)</param>
        /// <returns>A single selected item</returns>
        /// <exception cref="ArgumentNullException">Thrown if candidates is null</exception>
        /// <exception cref="ArgumentException">Thrown if candidates is empty or contains only zero/negative weights</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if bonus is negative</exception>
        public T SelectSingle<T>(IEnumerable<T> candidates, float bonus) where T : ISelectable
        {
            ValidateInput(candidates, bonus);

            T[] itemArray = candidates as T[] ?? candidates.ToArray();
            
            if (bonus <= 0f)
            {
                T result = SelectInternal(itemArray);
                TrackSelection(result);
                return result;
            }

            // Apply bonus adjustments
            var adjustedItems = ApplyBonusAdjustments(itemArray, bonus);
            T selected = SelectInternal(adjustedItems).Item;
            TrackSelection(selected);
            return selected;
        }

        /// <summary>
        /// Attempts to select a single item without throwing exceptions.
        /// </summary>
        /// <typeparam name="T">The type of selectable item</typeparam>
        /// <param name="candidates">The collection to select from</param>
        /// <param name="result">The selected item, or default(T) if selection failed</param>
        /// <returns>True if selection succeeded, false otherwise</returns>
        public bool TrySelectSingle<T>(IEnumerable<T> candidates, out T result) where T : ISelectable
        {
            return TrySelectSingle(candidates, 0f, out result);
        }

        /// <summary>
        /// Attempts to select a single item with bonus adjustment without throwing exceptions.
        /// </summary>
        /// <typeparam name="T">The type of selectable item</typeparam>
        /// <param name="candidates">The collection to select from</param>
        /// <param name="bonus">The bonus adjustment value</param>
        /// <param name="result">The selected item, or default(T) if selection failed</param>
        /// <returns>True if selection succeeded, false otherwise</returns>
        public bool TrySelectSingle<T>(IEnumerable<T> candidates, float bonus, out T result) where T : ISelectable
        {
            result = default(T);

            try
            {
                result = SelectSingle(candidates, bonus);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Multiple Selection

        /// <summary>
        /// Selects multiple items from the collection based on weight (with replacement - duplicates possible).
        /// </summary>
        /// <typeparam name="T">The type of selectable item</typeparam>
        /// <param name="candidates">The collection to select from</param>
        /// <param name="count">The number of items to select</param>
        /// <returns>An array of selected items</returns>
        /// <exception cref="ArgumentNullException">Thrown if candidates is null</exception>
        /// <exception cref="ArgumentException">Thrown if candidates is empty or contains only zero/negative weights</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if count is less than 1</exception>
        public T[] SelectMultiple<T>(IEnumerable<T> candidates, int count) where T : ISelectable
        {
            return SelectMultiple(candidates, count, 0f);
        }

        /// <summary>
        /// Selects multiple items from the collection with bonus adjustment (with replacement - duplicates possible).
        /// </summary>
        /// <typeparam name="T">The type of selectable item</typeparam>
        /// <param name="candidates">The collection to select from</param>
        /// <param name="count">The number of items to select</param>
        /// <param name="bonus">The degree to which weights are pushed toward the pivot</param>
        /// <returns>An array of selected items</returns>
        /// <exception cref="ArgumentNullException">Thrown if candidates is null</exception>
        /// <exception cref="ArgumentException">Thrown if candidates is empty or contains only zero/negative weights</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if count is less than 1 or bonus is negative</exception>
        public T[] SelectMultiple<T>(IEnumerable<T> candidates, int count, float bonus) where T : ISelectable
        {
            ValidateInput(candidates, bonus);

            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be at least 1");

            T[] itemArray = candidates as T[] ?? candidates.ToArray();
            T[] results = new T[count];

            if (bonus <= 0f)
            {
                for (int i = 0; i < count; i++)
                {
                    results[i] = SelectInternal(itemArray);
                    TrackSelection(results[i]);
                }
            }
            else
            {
                var adjustedItems = ApplyBonusAdjustments(itemArray, bonus);

                for (int i = 0; i < count; i++)
                {
                    var selected = SelectInternal(adjustedItems);
                    results[i] = selected.Item;
                    TrackSelection(selected.Item);
                }
            }

            return results;
        }

        /// <summary>
        /// Selects multiple unique items without replacement (no duplicates).
        /// </summary>
        /// <typeparam name="T">The type of selectable item</typeparam>
        /// <param name="candidates">The collection to select from</param>
        /// <param name="count">The number of items to select</param>
        /// <returns>An array of unique selected items</returns>
        /// <exception cref="ArgumentNullException">Thrown if candidates is null</exception>
        /// <exception cref="ArgumentException">Thrown if candidates is empty or count exceeds available items</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if count is less than 1</exception>
        public T[] SelectDistinct<T>(IEnumerable<T> candidates, int count) where T : ISelectable
        {
            return SelectDistinct(candidates, count, 0f);
        }

        /// <summary>
        /// Selects multiple unique items without replacement with bonus adjustment (no duplicates).
        /// </summary>
        /// <typeparam name="T">The type of selectable item</typeparam>
        /// <param name="candidates">The collection to select from</param>
        /// <param name="count">The number of items to select</param>
        /// <param name="bonus">The degree to which weights are pushed toward the pivot</param>
        /// <returns>An array of unique selected items</returns>
        /// <exception cref="ArgumentNullException">Thrown if candidates is null</exception>
        /// <exception cref="ArgumentException">Thrown if candidates is empty or count exceeds available items</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if count is less than 1 or bonus is negative</exception>
        public T[] SelectDistinct<T>(IEnumerable<T> candidates, int count, float bonus) where T : ISelectable
        {
            ValidateInput(candidates, bonus);

            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be at least 1");

            T[] itemArray = candidates as T[] ?? candidates.ToArray();

            if (count > itemArray.Length)
                throw new ArgumentException($"Cannot select {count} distinct items from a collection of {itemArray.Length} items", nameof(count));

            var remaining = new List<T>(itemArray);
            T[] results = new T[count];

            for (int i = 0; i < count; i++)
            {
                T selected;

                if (bonus <= 0f)
                {
                    selected = SelectInternal(remaining.ToArray());
                }
                else
                {
                    var adjustedItems = ApplyBonusAdjustments(remaining.ToArray(), bonus);
                    selected = SelectInternal(adjustedItems).Item;
                }

                results[i] = selected;
                remaining.Remove(selected);
                TrackSelection(selected);
            }

            return results;
        }

        /// <summary>
        /// Attempts to select multiple items without throwing exceptions.
        /// </summary>
        /// <typeparam name="T">The type of selectable item</typeparam>
        /// <param name="candidates">The collection to select from</param>
        /// <param name="count">The number of items to select</param>
        /// <param name="results">The selected items, or empty array if selection failed</param>
        /// <returns>True if selection succeeded, false otherwise</returns>
        public bool TrySelectMultiple<T>(IEnumerable<T> candidates, int count, out T[] results) where T : ISelectable
        {
            return TrySelectMultiple(candidates, count, 0f, out results);
        }

        /// <summary>
        /// Attempts to select multiple items with bonus without throwing exceptions.
        /// </summary>
        /// <typeparam name="T">The type of selectable item</typeparam>
        /// <param name="candidates">The collection to select from</param>
        /// <param name="count">The number of items to select</param>
        /// <param name="bonus">The bonus adjustment value</param>
        /// <param name="results">The selected items, or empty array if selection failed</param>
        /// <returns>True if selection succeeded, false otherwise</returns>
        public bool TrySelectMultiple<T>(IEnumerable<T> candidates, int count, float bonus, out T[] results) where T : ISelectable
        {
            results = new T[0];

            try
            {
                results = SelectMultiple(candidates, count, bonus);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Probability & Diagnostics

        /// <summary>
        /// Calculates the selection probability for each item without bonus.
        /// </summary>
        /// <typeparam name="T">The type of selectable item</typeparam>
        /// <param name="candidates">The collection to analyze</param>
        /// <returns>A dictionary mapping each item to its selection probability (0-1)</returns>
        /// <exception cref="ArgumentNullException">Thrown if candidates is null</exception>
        /// <exception cref="ArgumentException">Thrown if candidates is empty</exception>
        public Dictionary<T, float> GetProbabilities<T>(IEnumerable<T> candidates) where T : ISelectable
        {
            return GetProbabilities(candidates, 0f);
        }

        /// <summary>
        /// Calculates the selection probability for each item with bonus adjustment.
        /// </summary>
        /// <typeparam name="T">The type of selectable item</typeparam>
        /// <param name="candidates">The collection to analyze</param>
        /// <param name="bonus">The bonus adjustment value</param>
        /// <returns>A dictionary mapping each item to its selection probability (0-1)</returns>
        /// <exception cref="ArgumentNullException">Thrown if candidates is null</exception>
        /// <exception cref="ArgumentException">Thrown if candidates is empty</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if bonus is negative</exception>
        public Dictionary<T, float> GetProbabilities<T>(IEnumerable<T> candidates, float bonus) where T : ISelectable
        {
            ValidateInput(candidates, bonus);

            T[] itemArray = candidates as T[] ?? candidates.ToArray();
            var result = new Dictionary<T, float>();

            if (bonus <= 0f)
            {
                float totalWeight = itemArray.Sum(i => Mathf.Max(0f, i.SelectionWeight));

                if (totalWeight <= 0f)
                    throw new ArgumentException("Total weight must be greater than zero", nameof(candidates));

                foreach (var item in itemArray)
                {
                    result[item] = Mathf.Max(0f, item.SelectionWeight) / totalWeight;
                }
            }
            else
            {
                var adjustedItems = ApplyBonusAdjustments(itemArray, bonus);
                float totalWeight = adjustedItems.Sum(i => Mathf.Max(0f, i.Weight));

                foreach (var adjusted in adjustedItems)
                {
                    result[adjusted.Item] = Mathf.Max(0f, adjusted.Weight) / totalWeight;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the adjusted weights for each item after bonus application.
        /// </summary>
        /// <typeparam name="T">The type of selectable item</typeparam>
        /// <param name="candidates">The collection to analyze</param>
        /// <param name="bonus">The bonus adjustment value</param>
        /// <returns>A dictionary mapping each item to its adjusted weight</returns>
        /// <exception cref="ArgumentNullException">Thrown if candidates is null</exception>
        /// <exception cref="ArgumentException">Thrown if candidates is empty</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if bonus is negative</exception>
        public Dictionary<T, float> GetAdjustedWeights<T>(IEnumerable<T> candidates, float bonus) where T : ISelectable
        {
            ValidateInput(candidates, bonus);

            T[] itemArray = candidates as T[] ?? candidates.ToArray();

            if (bonus <= 0f)
            {
                return itemArray.ToDictionary(i => i, i => i.SelectionWeight);
            }

            var adjustedItems = ApplyBonusAdjustments(itemArray, bonus);
            return adjustedItems.ToDictionary(i => i.Item, i => i.Weight);
        }

        #endregion

        #region History Management

        /// <summary>
        /// Gets the selection history as a read-only collection.
        /// </summary>
        /// <returns>An array of previously selected items (most recent last)</returns>
        public object[] GetHistory()
        {
            return _selectionHistory.ToArray();
        }

        /// <summary>
        /// Gets the selection history of a specific type.
        /// </summary>
        /// <typeparam name="T">The type to filter history by</typeparam>
        /// <returns>An array of previously selected items of type T</returns>
        public T[] GetHistory<T>() where T : ISelectable
        {
            return _selectionHistory.OfType<T>().ToArray();
        }

        /// <summary>
        /// Clears the selection history.
        /// </summary>
        public void ClearHistory()
        {
            _selectionHistory.Clear();
        }

        /// <summary>
        /// Checks if an item was recently selected.
        /// </summary>
        /// <typeparam name="T">The type of item</typeparam>
        /// <param name="item">The item to check</param>
        /// <returns>True if the item is in the selection history</returns>
        public bool WasRecentlySelected<T>(T item) where T : ISelectable
        {
            return _selectionHistory.Contains(item);
        }

        /// <summary>
        /// Tracks a selection in history if history tracking is enabled.
        /// </summary>
        private void TrackSelection<T>(T item) where T : ISelectable
        {
            if (_maxHistorySize <= 0)
                return;

            _selectionHistory.Enqueue(item);

            while (_selectionHistory.Count > _maxHistorySize)
            {
                _selectionHistory.Dequeue();
            }
        }

        #endregion

        #region Cache Management

        /// <summary>
        /// Clears the internal weight calculation cache.
        /// </summary>
        /// <remarks>
        /// The selector caches cumulative weight calculations for performance.
        /// Call this if you modify the weights of items between selections.
        /// </remarks>
        public void ClearCache()
        {
            _cachedWeights.Clear();
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates input parameters for selection methods.
        /// </summary>
        private void ValidateInput<T>(IEnumerable<T> candidates, float bonus) where T : ISelectable
        {
            if (candidates == null)
                throw new ArgumentNullException(nameof(candidates), "Candidates collection cannot be null");

            T[] itemArray = candidates as T[] ?? candidates.ToArray();

            if (itemArray.Length == 0)
                throw new ArgumentException("Candidates collection cannot be empty", nameof(candidates));

            float totalWeight = itemArray.Sum(i => Mathf.Max(0f, i.SelectionWeight));

            if (totalWeight <= 0f)
                throw new ArgumentException("At least one item must have a positive weight", nameof(candidates));

            if (bonus < 0f)
                throw new ArgumentOutOfRangeException(nameof(bonus), "Bonus cannot be negative");
        }

        #endregion

        #region Builder Pattern

        /// <summary>
        /// Creates a new selector builder for fluent configuration.
        /// </summary>
        /// <returns>A new SelectorBuilder instance</returns>
        public static SelectorBuilder CreateBuilder()
        {
            return new SelectorBuilder();
        }

        /// <summary>
        /// Builder class for constructing Selector instances with fluent syntax.
        /// </summary>
        public class SelectorBuilder
        {
            private SelectionPivot _pivot = SelectionPivot.Mean;
            private SelectionRange _range = SelectionRange.All;
            private int? _seed = null;
            private int _historySize = 0;

            /// <summary>
            /// Sets the selection pivot mode.
            /// </summary>
            public SelectorBuilder WithPivot(SelectionPivot pivot)
            {
                _pivot = pivot;
                return this;
            }

            /// <summary>
            /// Sets the selection range mode.
            /// </summary>
            public SelectorBuilder WithRange(SelectionRange range)
            {
                _range = range;
                return this;
            }

            /// <summary>
            /// Sets a seed for reproducible random results.
            /// </summary>
            public SelectorBuilder WithSeed(int seed)
            {
                _seed = seed;
                return this;
            }

            /// <summary>
            /// Enables selection history tracking with the specified size.
            /// </summary>
            public SelectorBuilder WithHistorySize(int size)
            {
                _historySize = Mathf.Max(0, size);
                return this;
            }

            /// <summary>
            /// Builds the configured Selector instance.
            /// </summary>
            public Selector Build()
            {
                return new Selector(_pivot, _range, _seed, _historySize);
            }
        }

        #endregion
    }
}
