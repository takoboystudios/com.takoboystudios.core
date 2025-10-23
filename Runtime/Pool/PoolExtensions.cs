using UnityEngine;

namespace TakoBoyStudios.Core
{
    /// <summary>
    /// Extension methods for convenient object pooling operations.
    /// </summary>
    public static class PoolExtensions
    {
        /// <summary>
        /// Returns this GameObject to its pool.
        /// </summary>
        /// <param name="obj">The GameObject to return to pool</param>
        /// <returns>True if successfully returned to pool, false if not pooled or error occurred</returns>
        /// <example>
        /// <code>
        /// // Instead of:
        /// PoolManager.Instance.Release(bullet);
        /// 
        /// // You can do:
        /// bullet.ReturnToPool();
        /// </code>
        /// </example>
        public static bool ReturnToPool(this GameObject obj)
        {
            return PoolManager.Instance.Release(obj);
        }

        /// <summary>
        /// Returns this Component's GameObject to its pool.
        /// </summary>
        /// <param name="component">The component whose GameObject should be returned</param>
        /// <returns>True if successfully returned to pool</returns>
        /// <example>
        /// <code>
        /// public class Bullet : MonoBehaviour
        /// {
        ///     void OnCollision()
        ///     {
        ///         this.ReturnToPool(); // Returns the bullet GameObject to pool
        ///     }
        /// }
        /// </code>
        /// </example>
        public static bool ReturnToPool(this Component component)
        {
            return PoolManager.Instance.Release(component.gameObject);
        }

        /// <summary>
        /// Returns this GameObject to its pool after a delay.
        /// </summary>
        /// <param name="obj">The GameObject to return to pool</param>
        /// <param name="delay">Delay in seconds before returning</param>
        /// <example>
        /// <code>
        /// explosion.ReturnToPoolAfter(2f); // Returns to pool after 2 seconds
        /// </code>
        /// </example>
        public static void ReturnToPoolAfter(this GameObject obj, float delay)
        {
            if (obj.TryGetComponent<PoolDelayedReturn>(out var existing))
            {
                existing.CancelInvoke();
                Object.Destroy(existing);
            }

            var delayedReturn = obj.AddComponent<PoolDelayedReturn>();
            delayedReturn.Initialize(delay);
        }

        /// <summary>
        /// Returns this Component's GameObject to its pool after a delay.
        /// </summary>
        /// <param name="component">The component whose GameObject should be returned</param>
        /// <param name="delay">Delay in seconds before returning</param>
        public static void ReturnToPoolAfter(this Component component, float delay)
        {
            component.gameObject.ReturnToPoolAfter(delay);
        }
    }

    /// <summary>
    /// Internal helper component for delayed pool returns.
    /// </summary>
    internal class PoolDelayedReturn : MonoBehaviour
    {
        public void Initialize(float delay)
        {
            Invoke(nameof(ReturnNow), delay);
        }

        private void ReturnNow()
        {
            gameObject.ReturnToPool();
        }

        private void OnDisable()
        {
            CancelInvoke();
        }
    }
}
