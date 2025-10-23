using System;
using System.Collections.Generic;
using UnityEngine;

namespace TakoBoyStudios.Core
{
    /// <summary>
    /// Extension methods for arrays and lists providing randomization and selection utilities.
    /// </summary>
    public static class ArrayExtensions
    {
        #region Random Selection

        /// <summary>
        /// Returns a random item from the list.
        /// </summary>
        /// <typeparam name="T">The type of items in the list</typeparam>
        /// <param name="list">The source list</param>
        /// <returns>A randomly selected item from the list</returns>
        /// <exception cref="ArgumentNullException">Thrown if the list is null</exception>
        /// <exception cref="ArgumentException">Thrown if the list is empty</exception>
        /// <example>
        /// <code>
        /// List&lt;string&gt; names = new List&lt;string&gt; { "Alice", "Bob", "Charlie" };
        /// string randomName = names.GetRandomItem();
        /// </code>
        /// </example>
        public static T GetRandomItem<T>(this List<T> list)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list), "Cannot get random item from null list");

            if (list.Count == 0)
                throw new ArgumentException("Cannot get random item from empty list", nameof(list));

            int randomIndex = UnityEngine.Random.Range(0, list.Count);
            return list[randomIndex];
        }

        /// <summary>
        /// Returns a random item from the array.
        /// </summary>
        /// <typeparam name="T">The type of items in the array</typeparam>
        /// <param name="array">The source array</param>
        /// <returns>A randomly selected item from the array</returns>
        /// <exception cref="ArgumentNullException">Thrown if the array is null</exception>
        /// <exception cref="ArgumentException">Thrown if the array is empty</exception>
        /// <example>
        /// <code>
        /// string[] names = { "Alice", "Bob", "Charlie" };
        /// string randomName = names.GetRandomItem();
        /// </code>
        /// </example>
        public static T GetRandomItem<T>(this T[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array), "Cannot get random item from null array");

            if (array.Length == 0)
                throw new ArgumentException("Cannot get random item from empty array", nameof(array));

            int randomIndex = UnityEngine.Random.Range(0, array.Length);
            return array[randomIndex];
        }

        /// <summary>
        /// Tries to get a random item from the list without throwing exceptions.
        /// </summary>
        /// <typeparam name="T">The type of items in the list</typeparam>
        /// <param name="list">The source list</param>
        /// <param name="item">The randomly selected item, or default(T) if unsuccessful</param>
        /// <returns>True if an item was successfully selected, false otherwise</returns>
        public static bool TryGetRandomItem<T>(this List<T> list, out T item)
        {
            item = default(T);

            if (list == null || list.Count == 0)
                return false;

            item = list[UnityEngine.Random.Range(0, list.Count)];
            return true;
        }

        /// <summary>
        /// Tries to get a random item from the array without throwing exceptions.
        /// </summary>
        /// <typeparam name="T">The type of items in the array</typeparam>
        /// <param name="array">The source array</param>
        /// <param name="item">The randomly selected item, or default(T) if unsuccessful</param>
        /// <returns>True if an item was successfully selected, false otherwise</returns>
        public static bool TryGetRandomItem<T>(this T[] array, out T item)
        {
            item = default(T);

            if (array == null || array.Length == 0)
                return false;

            item = array[UnityEngine.Random.Range(0, array.Length)];
            return true;
        }

        #endregion

        #region Randomization (Fisher-Yates Shuffle)

        /// <summary>
        /// Returns a new randomized list with items in random order using the Fisher-Yates shuffle algorithm.
        /// </summary>
        /// <typeparam name="T">The type of items in the list</typeparam>
        /// <param name="list">The source list</param>
        /// <returns>A new list with the same items in randomized order</returns>
        /// <exception cref="ArgumentNullException">Thrown if the list is null</exception>
        /// <remarks>
        /// This method does not modify the original list. It creates a copy and shuffles that.
        /// Uses the Fisher-Yates shuffle algorithm for uniform distribution.
        /// </remarks>
        /// <example>
        /// <code>
        /// List&lt;int&gt; numbers = new List&lt;int&gt; { 1, 2, 3, 4, 5 };
        /// List&lt;int&gt; shuffled = numbers.Randomize();
        /// // Original list is unchanged, shuffled contains items in random order
        /// </code>
        /// </example>
        public static List<T> Randomize<T>(this List<T> list)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list), "Cannot randomize null list");

            // Create a copy to avoid modifying the original
            List<T> newList = new List<T>(list);

            int n = newList.Count;
            System.Random random = new System.Random();

            // Fisher-Yates shuffle
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);

                // Swap elements
                T value = newList[k];
                newList[k] = newList[n];
                newList[n] = value;
            }

            return newList;
        }

        /// <summary>
        /// Returns a new randomized array with items in random order using the Fisher-Yates shuffle algorithm.
        /// </summary>
        /// <typeparam name="T">The type of items in the array</typeparam>
        /// <param name="array">The source array</param>
        /// <returns>A new array with the same items in randomized order</returns>
        /// <exception cref="ArgumentNullException">Thrown if the array is null</exception>
        /// <remarks>
        /// This method does not modify the original array. It creates a copy and shuffles that.
        /// Uses the Fisher-Yates shuffle algorithm for uniform distribution.
        /// </remarks>
        /// <example>
        /// <code>
        /// int[] numbers = { 1, 2, 3, 4, 5 };
        /// int[] shuffled = numbers.Randomize();
        /// // Original array is unchanged, shuffled contains items in random order
        /// </code>
        /// </example>
        public static T[] Randomize<T>(this T[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array), "Cannot randomize null array");

            // Create a copy to avoid modifying the original
            T[] newArray = (T[])array.Clone();

            int n = newArray.Length;
            System.Random random = new System.Random();

            // Fisher-Yates shuffle
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);

                // Swap elements
                T value = newArray[k];
                newArray[k] = newArray[n];
                newArray[n] = value;
            }

            return newArray;
        }

        /// <summary>
        /// Shuffles the list in place using the Fisher-Yates algorithm.
        /// </summary>
        /// <typeparam name="T">The type of items in the list</typeparam>
        /// <param name="list">The list to shuffle (modified in place)</param>
        /// <exception cref="ArgumentNullException">Thrown if the list is null</exception>
        /// <remarks>
        /// This method modifies the original list. Use Randomize() if you need to keep the original.
        /// </remarks>
        public static void Shuffle<T>(this List<T> list)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list), "Cannot shuffle null list");

            int n = list.Count;
            System.Random random = new System.Random();

            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);

                // Swap elements
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Shuffles the array in place using the Fisher-Yates algorithm.
        /// </summary>
        /// <typeparam name="T">The type of items in the array</typeparam>
        /// <param name="array">The array to shuffle (modified in place)</param>
        /// <exception cref="ArgumentNullException">Thrown if the array is null</exception>
        /// <remarks>
        /// This method modifies the original array. Use Randomize() if you need to keep the original.
        /// </remarks>
        public static void Shuffle<T>(this T[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array), "Cannot shuffle null array");

            int n = array.Length;
            System.Random random = new System.Random();

            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);

                // Swap elements
                T value = array[k];
                array[k] = array[n];
                array[n] = value;
            }
        }

        #endregion

        #region Utility

        /// <summary>
        /// Checks if the list is null or empty.
        /// </summary>
        /// <typeparam name="T">The type of items in the list</typeparam>
        /// <param name="list">The list to check</param>
        /// <returns>True if the list is null or has no elements</returns>
        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            return list == null || list.Count == 0;
        }

        /// <summary>
        /// Checks if the array is null or empty.
        /// </summary>
        /// <typeparam name="T">The type of items in the array</typeparam>
        /// <param name="array">The array to check</param>
        /// <returns>True if the array is null or has no elements</returns>
        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        /// <summary>
        /// Gets a random subset of items from the list.
        /// </summary>
        /// <typeparam name="T">The type of items in the list</typeparam>
        /// <param name="list">The source list</param>
        /// <param name="count">The number of items to select</param>
        /// <returns>A new list containing the randomly selected items</returns>
        /// <exception cref="ArgumentNullException">Thrown if the list is null</exception>
        /// <exception cref="ArgumentException">Thrown if count is negative or greater than list size</exception>
        public static List<T> GetRandomSubset<T>(this List<T> list, int count)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list), "Cannot get random subset from null list");

            if (count < 0)
                throw new ArgumentException("Count cannot be negative", nameof(count));

            if (count > list.Count)
                throw new ArgumentException($"Cannot select {count} items from list with {list.Count} items", nameof(count));

            if (count == 0)
                return new List<T>();

            if (count == list.Count)
                return new List<T>(list);

            // Shuffle a copy and take the first 'count' items
            List<T> shuffled = list.Randomize();
            return shuffled.GetRange(0, count);
        }

        /// <summary>
        /// Gets a random subset of items from the array.
        /// </summary>
        /// <typeparam name="T">The type of items in the array</typeparam>
        /// <param name="array">The source array</param>
        /// <param name="count">The number of items to select</param>
        /// <returns>A new array containing the randomly selected items</returns>
        /// <exception cref="ArgumentNullException">Thrown if the array is null</exception>
        /// <exception cref="ArgumentException">Thrown if count is negative or greater than array size</exception>
        public static T[] GetRandomSubset<T>(this T[] array, int count)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array), "Cannot get random subset from null array");

            if (count < 0)
                throw new ArgumentException("Count cannot be negative", nameof(count));

            if (count > array.Length)
                throw new ArgumentException($"Cannot select {count} items from array with {array.Length} items", nameof(count));

            if (count == 0)
                return new T[0];

            if (count == array.Length)
                return (T[])array.Clone();

            // Shuffle a copy and take the first 'count' items
            T[] shuffled = array.Randomize();
            T[] result = new T[count];
            Array.Copy(shuffled, result, count);
            return result;
        }

        #endregion
    }
}
