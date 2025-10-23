namespace TakoBoyStudios.Core
{
    /// <summary>
    /// Interface that must be implemented by objects to participate in weighted selection.
    /// </summary>
    /// <remarks>
    /// Any collection of ISelectable-implementing objects can be used with the Selector class
    /// as long as the collection implements IEnumerable&lt;T&gt;.
    /// 
    /// The SelectionWeight property determines the probability of selection - higher values
    /// result in more frequent selection compared to items with lower weights.
    /// </remarks>
    /// <example>
    /// <code>
    /// public class LootItem : ISelectable
    /// {
    ///     public string Name { get; set; }
    ///     public float SelectionWeight { get; set; }
    ///     
    ///     public LootItem(string name, float weight)
    ///     {
    ///         Name = name;
    ///         SelectionWeight = weight;
    ///     }
    /// }
    /// 
    /// var items = new List&lt;LootItem&gt;
    /// {
    ///     new LootItem("Common", 70f),
    ///     new LootItem("Rare", 25f),
    ///     new LootItem("Legendary", 5f)
    /// };
    /// 
    /// var selector = new Selector();
    /// var selected = selector.SelectSingle(items);
    /// </code>
    /// </example>
    public interface ISelectable
    {
        /// <summary>
        /// Gets or sets the selection weight for this item.
        /// </summary>
        /// <value>
        /// A float representing the item's selection weight. Higher values increase
        /// the probability of selection relative to other items in the pool.
        /// Must be non-negative. A weight of 0 effectively excludes the item from selection.
        /// </value>
        /// <remarks>
        /// The actual probability of selection is calculated as:
        /// probability = (item.SelectionWeight) / (sum of all SelectionWeights)
        /// 
        /// For example, if you have three items with weights [10, 20, 70]:
        /// - First item: 10/100 = 10% chance
        /// - Second item: 20/100 = 20% chance
        /// - Third item: 70/100 = 70% chance
        /// </remarks>
        float SelectionWeight { get; set; }
    }
}
