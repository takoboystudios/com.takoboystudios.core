using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TakoBoyStudios.Core
{
    /// <summary>
    /// Internal implementation of the Selector class.
    /// </summary>
    /// <remarks>
    /// This partial class contains the core selection algorithms, weight adjustment logic,
    /// and helper methods used by the public API in Selector.cs.
    /// </remarks>
    public partial class Selector
    {
        #region Core Selection Algorithm

        /// <summary>
        /// Performs the core weighted selection algorithm using cumulative weights.
        /// </summary>
        /// <typeparam name="T">The type of selectable item</typeparam>
        /// <param name="items">The items to select from</param>
        /// <returns>A randomly selected item based on weights</returns>
        /// <remarks>
        /// Uses a binary search on cumulative weights for O(log n) selection performance.
        /// Supports both Unity's Random and seeded System.Random depending on configuration.
        /// </remarks>
        private T SelectInternal<T>(T[] items) where T : ISelectable
        {
            if (items.Length == 1)
                return items[0];

            // Get cumulative weights for binary search
            float[] cumulativeWeights = GetCumulativeWeights(items);
            float totalWeight = cumulativeWeights[cumulativeWeights.Length - 1];

            // Generate random value
            float randomValue = GetRandomValue(0f, totalWeight);

            // Binary search to find selected index
            int index = Array.BinarySearch(cumulativeWeights, randomValue);

            // If exact match not found, BinarySearch returns bitwise complement of next higher index
            if (index < 0)
                index = ~index;

            // Clamp to valid range
            index = Mathf.Clamp(index, 0, items.Length - 1);

            return items[index];
        }

        /// <summary>
        /// Performs selection on weight-adjusted items.
        /// </summary>
        private WeightContainer<T> SelectInternal<T>(WeightContainer<T>[] items) where T : ISelectable
        {
            if (items.Length == 1)
                return items[0];

            float[] cumulativeWeights = GetCumulativeWeights(items);
            float totalWeight = cumulativeWeights[cumulativeWeights.Length - 1];

            float randomValue = GetRandomValue(0f, totalWeight);

            int index = Array.BinarySearch(cumulativeWeights, randomValue);

            if (index < 0)
                index = ~index;

            index = Mathf.Clamp(index, 0, items.Length - 1);

            return items[index];
        }

        #endregion

        #region Cumulative Weights Calculation

        /// <summary>
        /// Calculates cumulative weights for efficient weighted selection.
        /// </summary>
        /// <typeparam name="T">The type of selectable item</typeparam>
        /// <param name="items">The items to calculate weights for</param>
        /// <returns>An array of cumulative weights</returns>
        /// <remarks>
        /// Cumulative weights allow for O(log n) binary search selection.
        /// For items with weights [10, 20, 30], cumulative weights are [10, 30, 60].
        /// Negative weights are clamped to zero.
        /// </remarks>
        private float[] GetCumulativeWeights<T>(T[] items) where T : ISelectable
        {
            float[] cumulativeWeights = new float[items.Length];
            float runningTotal = 0f;

            for (int i = 0; i < items.Length; i++)
            {
                float weight = Mathf.Max(0f, items[i].SelectionWeight);
                runningTotal += weight;
                cumulativeWeights[i] = runningTotal;
            }

            return cumulativeWeights;
        }

        /// <summary>
        /// Calculates cumulative weights for weight containers.
        /// </summary>
        private float[] GetCumulativeWeights<T>(WeightContainer<T>[] items) where T : ISelectable
        {
            float[] cumulativeWeights = new float[items.Length];
            float runningTotal = 0f;

            for (int i = 0; i < items.Length; i++)
            {
                float weight = Mathf.Max(0f, items[i].Weight);
                runningTotal += weight;
                cumulativeWeights[i] = runningTotal;
            }

            return cumulativeWeights;
        }

        #endregion

        #region Bonus Adjustment System

        /// <summary>
        /// Applies bonus adjustments to items based on the selector's pivot and range configuration.
        /// </summary>
        /// <typeparam name="T">The type of selectable item</typeparam>
        /// <param name="items">The items to adjust</param>
        /// <param name="bonus">The bonus value (higher = more equalization)</param>
        /// <returns>An array of weight containers with adjusted weights</returns>
        /// <remarks>
        /// Creates weight containers to preserve original items while adjusting their weights.
        /// The adjustment pushes weights toward the pivot (mean or median) asymptotically,
        /// so infinite bonus would equalize all weights.
        /// </remarks>
        private WeightContainer<T>[] ApplyBonusAdjustments<T>(T[] items, float bonus) where T : ISelectable
        {
            // Create containers with normalized weights
            float totalWeight = items.Sum(i => Mathf.Max(0f, i.SelectionWeight));
            var containers = new WeightContainer<T>[items.Length];

            for (int i = 0; i < items.Length; i++)
            {
                float normalizedWeight = Mathf.Max(0f, items[i].SelectionWeight) / totalWeight;
                containers[i] = new WeightContainer<T>(items[i], normalizedWeight);
            }

            // Apply adjustment based on pivot type
            switch (_pivot)
            {
                case SelectionPivot.Mean:
                    AdjustWeightsTowardMean(containers, bonus);
                    break;

                case SelectionPivot.Median:
                    AdjustWeightsTowardMedian(containers, bonus);
                    break;

                default:
                    Debug.LogError($"[Selector] Unknown pivot type: {_pivot}. Weights not adjusted.");
                    break;
            }

            return containers;
        }

        /// <summary>
        /// Adjusts weights toward the mean (average) value.
        /// </summary>
        private void AdjustWeightsTowardMean<T>(WeightContainer<T>[] containers, float bonus) where T : ISelectable
        {
            float mean = containers.Average(c => c.Weight);

            for (int i = 0; i < containers.Length; i++)
            {
                containers[i].Weight = AdjustWeight(containers[i].Weight, bonus, mean);
            }
        }

        /// <summary>
        /// Adjusts weights toward the median (middle) value.
        /// </summary>
        private void AdjustWeightsTowardMedian<T>(WeightContainer<T>[] containers, float bonus) where T : ISelectable
        {
            float median = CalculateMedian(containers.Select(c => c.Weight).ToArray());

            for (int i = 0; i < containers.Length; i++)
            {
                containers[i].Weight = AdjustWeight(containers[i].Weight, bonus, median);
            }
        }

        /// <summary>
        /// Calculates the median of a set of values.
        /// </summary>
        private float CalculateMedian(float[] values)
        {
            if (values.Length == 0)
                return 0f;

            float[] sorted = values.OrderBy(v => v).ToArray();
            int midIndex = sorted.Length / 2;

            if (sorted.Length % 2 == 0)
            {
                // Even count: average of two middle values
                return (sorted[midIndex - 1] + sorted[midIndex]) / 2f;
            }
            else
            {
                // Odd count: middle value
                return sorted[midIndex];
            }
        }

        /// <summary>
        /// Adjusts a single weight toward the target based on the selector's range configuration.
        /// </summary>
        /// <param name="weight">The current weight</param>
        /// <param name="bonus">The bonus value</param>
        /// <param name="target">The target value (mean or median)</param>
        /// <returns>The adjusted weight</returns>
        /// <remarks>
        /// The adjustment formula pushes weights toward the target asymptotically:
        /// - BelowPivot: Only increases weights below target
        /// - AbovePivot: Only decreases weights above target
        /// - All: Adjusts all weights toward target
        /// 
        /// The formula is: weight ± (target * (bonus / (bonus + 100)))
        /// This ensures the adjustment never crosses the target, preventing overshooting.
        /// </remarks>
        private float AdjustWeight(float weight, float bonus, float target)
        {
            // Calculate adjustment factor (asymptotic approach to target)
            float adjustmentFactor = bonus / (bonus + 100f);
            float adjustment = target * adjustmentFactor;

            switch (_range)
            {
                case SelectionRange.BelowPivot:
                    // Only boost weights below the pivot
                    if (weight < target)
                        return weight + adjustment;
                    return weight;

                case SelectionRange.AbovePivot:
                    // Only reduce weights above the pivot
                    if (weight > target)
                        return weight - adjustment;
                    return weight;

                case SelectionRange.All:
                    // Adjust all weights toward the pivot
                    if (weight < target)
                        return weight + adjustment;
                    else if (weight > target)
                        return weight - adjustment;
                    return weight;

                default:
                    Debug.LogError($"[Selector] Unknown range type: {_range}. Weight not adjusted.");
                    return weight;
            }
        }

        #endregion

        #region Random Number Generation

        /// <summary>
        /// Generates a random value using either Unity's Random or the seeded System.Random.
        /// </summary>
        /// <param name="min">Minimum value (inclusive)</param>
        /// <param name="max">Maximum value (exclusive for Unity.Random, inclusive for System.Random)</param>
        /// <returns>A random float value in the specified range</returns>
        private float GetRandomValue(float min, float max)
        {
            if (_useSeededRandom)
            {
                // System.Random returns [0, 1), so we scale to [min, max)
                return min + (float)_random.NextDouble() * (max - min);
            }
            else
            {
                // Unity's Random.Range with floats is [min, max)
                return UnityEngine.Random.Range(min, max);
            }
        }

        #endregion

        #region Weight Container Class

        /// <summary>
        /// Internal container that wraps an ISelectable item with an adjustable weight.
        /// </summary>
        /// <typeparam name="T">The type of the wrapped item</typeparam>
        /// <remarks>
        /// This allows us to adjust weights for bonus calculations without modifying
        /// the original items' SelectionWeight properties. It also enables working
        /// with value types (structs) that implement ISelectable.
        /// </remarks>
        private class WeightContainer<T> where T : ISelectable
        {
            /// <summary>
            /// Gets the wrapped item.
            /// </summary>
            public T Item { get; private set; }

            /// <summary>
            /// Gets or sets the adjusted weight for this item.
            /// </summary>
            public float Weight { get; set; }

            /// <summary>
            /// Creates a new weight container.
            /// </summary>
            /// <param name="item">The item to wrap</param>
            /// <param name="weight">The initial weight</param>
            public WeightContainer(T item, float weight)
            {
                Item = item;
                Weight = Mathf.Max(0f, weight); // Ensure non-negative
            }
        }

        #endregion
    }
}
