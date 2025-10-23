namespace TakoBoyStudios.Core
{
    /// <summary>
    /// Interface for objects that can be pooled and need lifecycle callbacks.
    /// </summary>
    /// <remarks>
    /// Implement this interface on MonoBehaviour components to receive notifications
    /// when the GameObject is acquired from or returned to the pool.
    /// 
    /// This allows objects to reset their state, disable components, clear references,
    /// and perform other cleanup/initialization tasks.
    /// </remarks>
    /// <example>
    /// <code>
    /// public class Bullet : MonoBehaviour, IPoolable
    /// {
    ///     private Rigidbody rb;
    ///     private TrailRenderer trail;
    ///     
    ///     public void OnCreated()
    ///     {
    ///         // Called once when first instantiated
    ///         rb = GetComponent&lt;Rigidbody&gt;();
    ///         trail = GetComponent&lt;TrailRenderer&gt;();
    ///     }
    ///     
    ///     public void OnAcquired()
    ///     {
    ///         // Called each time retrieved from pool
    ///         rb.velocity = Vector3.zero;
    ///         trail.Clear();
    ///     }
    ///     
    ///     public void OnReleased()
    ///     {
    ///         // Called when returned to pool
    ///         rb.velocity = Vector3.zero;
    ///         trail.Clear();
    ///     }
    ///     
    ///     public void OnDestroyed()
    ///     {
    ///         // Called when pool is cleared or object destroyed
    ///         // Cleanup any resources
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface IPoolable
    {
        /// <summary>
        /// Called once when the object is first instantiated and added to the pool.
        /// </summary>
        /// <remarks>
        /// Use this to cache components, initialize variables, or perform one-time setup.
        /// This is called immediately after instantiation, before the object is first used.
        /// </remarks>
        void OnCreated();

        /// <summary>
        /// Called each time the object is retrieved from the pool.
        /// </summary>
        /// <remarks>
        /// Use this to reset the object's state, re-enable components, or perform
        /// any initialization needed before the object is used again.
        /// The object will be SetActive(true) automatically after this is called.
        /// </remarks>
        void OnAcquired();

        /// <summary>
        /// Called when the object is returned to the pool.
        /// </summary>
        /// <remarks>
        /// Use this to reset state, disable components, clear references to prevent
        /// memory leaks, or perform cleanup before the object is pooled.
        /// The object will be SetActive(false) automatically after this is called.
        /// </remarks>
        void OnReleased();

        /// <summary>
        /// Called when the object is permanently destroyed (pool cleared or game ending).
        /// </summary>
        /// <remarks>
        /// Use this for final cleanup of resources, unsubscribing from events,
        /// or releasing assets. This is called before the GameObject is destroyed.
        /// </remarks>
        void OnDestroyed();
    }
}
