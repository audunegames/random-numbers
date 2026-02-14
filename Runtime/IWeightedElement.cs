namespace Audune.Utils.Random
{
  /// <summary>
  /// Interface that defines an element for a weighted  enumerable
  /// </summary>
  public interface IWeightedElement
  {
    /// <summary>
    /// Return the weight of the element
    /// </summary>
    public float weight { get; }
  }
}