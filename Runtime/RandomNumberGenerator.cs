using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Audune.Utils.Random
{
  /// <summary>
  /// Class that defines a seeded random number generator,
  /// </summary>
  public sealed class RandomNumberGenerator
  {
    /// <summary>
    /// Returns the default seed dependent on the current time.
    /// </summary>
    public static int defaultSeed => (int)DateTime.Now.Ticks;


    /// <summary>
    /// The seed of the seeded random number generator.
    /// </summary>
    public readonly int seed;

    /// <summary>
    /// The stored state of the seeded random number generator.
    /// </summary>
    private UnityEngine.Random.State _randomState;


    /// <summary>
    /// Constructs a seeded random number generator with the specified seed.
    /// </summary>
    public RandomNumberGenerator(int seed)
    {
      this.seed = seed;

      var originalState = UnityEngine.Random.state;
      UnityEngine.Random.InitState(seed);
      _randomState = UnityEngine.Random.state;
      UnityEngine.Random.state = originalState;
    }
    
    /// <summary>
    /// Constructs a seeded random number generator with the specified seed, or the default seed if it is null.
    /// </summary>
    /// <param name="seed">The seed to use for the generator.</param>
    public RandomNumberGenerator(int? seed) : this(seed ?? defaultSeed)
    {
    }

    /// <summary>
    /// Constructs a seeded random number generator with the default seed.
    /// </summary>
    public RandomNumberGenerator() : this(defaultSeed)
    {
    }


    /// <summary>
    /// Invokes the specified function with the seeded random context.
    /// </summary>
    /// <typeparam name="T">The type of the result of the function to invoke.</typeparam>
    /// <param name="func">The function to invoke.</param>
    /// <returns>The result of the invoked function.</returns>
    private T WithRandom<T>(Func<T> func)
    {
      // Set the 
      var originalState = UnityEngine.Random.state;
      UnityEngine.Random.state = _randomState;

      // Invoke the function
      var result = func();

      // Reset the state back to the original state
      _randomState = UnityEngine.Random.state;
      UnityEngine.Random.state = originalState;

      // Return the result
      return result;
    }


    #region Returning random values within a range
    /// <summary>
    /// Returns a random <c>float</c> between 0 and 1 inclusive.
    /// </summary>
    /// <returns>A random <c>float</c> between 0 and 1 inclusive</returns>
    public float Value()
    {
      return WithRandom(() => UnityEngine.Random.value);
    }

    /// <summary>
    /// Returns a random <c>float</c> between <paramref name="minInclusive"/> and <paramref name="maxInclusive"/>.
    /// </summary>
    /// <param name="minInclusive">The minimal value of the range.</param>
    /// <param name="maxInclusive">The maximal value of the range.</param>
    /// <returns>A random <c>float</c> between <paramref name="minInclusive"/> and <paramref name="maxInclusive"/>.</returns>
    public float Range(float minInclusive, float maxInclusive)
    {
      return WithRandom(() => UnityEngine.Random.Range(minInclusive, maxInclusive));
    }

    /// <summary>
    /// Returns a random <c>int</c> between <paramref name="minInclusive"/> and <paramref name="maxExclusive"/>.
    /// </summary>
    /// <param name="minInclusive">The minimal value of the range.</param>
    /// <param name="maxExclusive">The maximal value of the range.</param>
    /// <returns>A random <c>int</c> between <paramref name="minInclusive"/> and <paramref name="maxExclusive"/>.</returns>
    public int Range(int minInclusive, int maxExclusive)
    {
      return WithRandom(() => UnityEngine.Random.Range(minInclusive, maxExclusive));
    }
    #endregion

    #region Returning random values based on arguments
    /// <summary>
    /// Return the result of tossing a coin.
    /// </summary>
    /// <returns>The result of tossing a coin.</returns>
    public bool Toss()
    {
      return Range(0, 2) == 0;
    }

    /// <summary>
    /// Return the result of probing the specified probability.
    /// </summary>
    /// <param name="probability">The probability to probe against.</param>
    /// <returns>The result of probing the specified probability</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the probability is not greater than 0.</exception>
    public bool Probe(float probability)
    {
      if (probability <= 0.0f)
        throw new ArgumentOutOfRangeException(nameof(probability), "probability must be greater than 0");

      return probability >= 0.0f && Value() <= probability;
    }

    /// <summary>
    /// Returns the result of rolling one die with the specified sides.
    /// </summary>
    /// <param name="sides">The sides of the die to roll with.</param>
    /// <returns>The result of rolling one die with the specified sides.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If sides is not at least 2.</exception>
    public int Roll(int sides)
    {
      if (sides < 2)
        throw new ArgumentOutOfRangeException(nameof(sides), "sides must be at least 2");

      return Range(1, sides + 1);
    }

    /// <summary>
    /// Returns the result of rolling the specified amount of dice with the specified sides.
    /// </summary>
    /// <param name="amount">The amount of dice to roll with.</param>
    /// <param name="sides">The sides of the dice to roll with.</param>
    /// <returns>The result of rolling the specified amount of dice with the specified sides.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If amount is not at least 1, or sides is not at least 2.</exception>
    public int Roll(int amount, int sides)
    {
      if (amount < 1)
        throw new ArgumentOutOfRangeException(nameof(amount), "amount must be at least 1");
      if (sides < 2)
        throw new ArgumentOutOfRangeException(nameof(sides), "sides must be at least 2");

      var sum = 0;
      for (var i = 0; i < amount; i++)
        sum += Range(1, sides + 1);
      return sum;
    }
    #endregion

    #region Returning random elements from enumerables
    /// <summary>
    /// Chooses a random element from an enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="enumerable">The enumerable to pick an element from.</param>
    /// <returns>The element at a random index between 0 and the amount of elements in the enumerable.</returns>
    /// <exception cref="ArgumentNullException">If the enumerable is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">If the enumerable has no elements.</exception>
    public T Choose<T>(IEnumerable<T> enumerable)
    {
      if (enumerable == null)
        throw new ArgumentNullException(nameof(enumerable));

      var list = enumerable as List<T> ?? enumerable.ToList();
      var count = list.Count;
      if (count == 0)
        throw new ArgumentException("enumerable has no elements", nameof(enumerable));

      return list.ElementAt(Range(0, count));
    }

    /// <summary>
    /// Returns if a random element that matches the specified predicate can be chosed from the enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="enumerable">The enumerable to pick an element from.</param>
    /// <param name="chosenElement">The random element that got picked.</param>
    /// <param name="predicate">A function to test the picked element for a condition.</param>
    /// <returns>Whether a random element that matches the specified predicate can be picked from the enumerable.</returns>
    /// <exception cref="ArgumentNullException">If the enumerable is <c>null</c>.</exception>
    public bool TryChoose<T>(IEnumerable<T> enumerable, out T chosenElement, Func<T, bool> predicate = null)
    {
      if (enumerable == null)
        throw new ArgumentNullException(nameof(enumerable));

      chosenElement = default;

      var matchingElements = predicate != null ? enumerable.Where(predicate).ToList() : enumerable.ToList();
      var matchingElementsCount = matchingElements.Count;
      if (matchingElementsCount == 0)
        return false;

      chosenElement = matchingElements[Range(0, matchingElementsCount)];
      return true;
    }

    /// <summary>
    /// Samples the specified amount of elements that match the specified predicate from the enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="enumerable">The enumerable to pick an element from.</param>
    /// <param name="amount">The amount of elements to sample.</param>
    /// <param name="predicate">A function to test the picked element for a condition.</param>
    /// <returns>A list containing the sampled elements.</returns>
    /// <exception cref="ArgumentNullException">If the enumerable is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">If amount is not at least 1</exception>
    public IReadOnlyList<T> Sample<T>(IEnumerable<T> enumerable, int amount, Func<T, bool> predicate = null)
    {
      if (enumerable == null)
        throw new ArgumentNullException(nameof(enumerable));
      if (amount < 1)
        throw new ArgumentOutOfRangeException(nameof(amount), "amount must be at least 1");

      var chosenElements = new List<T>();

      var matchingElements = predicate != null ? enumerable.Where(predicate).ToList() : enumerable.ToList();
      var matchingElementsCount = matchingElements.Count;
      if (matchingElementsCount == 0)
        return chosenElements;

      var n = matchingElementsCount;
      var t = 0;
      var m = amount;
      while (m > 0 && t < matchingElementsCount)
      {
        if (n * Value() < m)
        {
          chosenElements.Add(matchingElements[t]);
          m--;
        }

        t++;
        n--;
      }

      return chosenElements;
    }

    /// <summary>
    /// Shuffles the enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="enumerable">The enumerable to pick an element from.</param>
    /// <returns>A list containing the shuffled elements of the enumerable.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public IReadOnlyList<T> Shuffle<T>(IEnumerable<T> enumerable)
    {
      if (enumerable == null)
        throw new ArgumentNullException(nameof(enumerable));

      var list = enumerable as List<T> ?? enumerable.ToList();
      var n = list.Count;
      var result = new List<T>(list);
      while (n > 1)
      {
        n--;
        var k = Range(0, n + 1);
        (result[n], result[k]) = (result[k], result[n]);
      }

      return result;
    }
    #endregion

    #region Returning random elements from weighted enumerables
    /// <summary>
    /// Chooses a random weighted item from an enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="enumerable">The enumerable to pick an element from.</param>
    /// <param name="sumOfWeights">The sum of the weights of the items. If <c>0.0f</c>, this will be calculated during choosing.</param>
    /// <returns>The element picked by weight.</returns>
    /// <exception cref="ArgumentNullException">If the enumerable is <c>null</c>.</exception>
    public T ChooseWeighted<T>(IEnumerable<T> enumerable, ref float sumOfWeights) where T : IWeightedElement
    {
      if (enumerable == null)
        throw new ArgumentNullException(nameof(enumerable));

      var list = enumerable as List<T> ?? enumerable.ToList();
      var count = list.Count;
      if (count == 0)
        throw new ArgumentException("enumerable has no elements", nameof(enumerable));

      if (sumOfWeights <= 0.0f)
      {
        sumOfWeights = 0.0f;
        foreach (var element in list)
        {
          if (element.weight <= 0.0f)
            throw new ArgumentException("weight of all elements in enumerable must be greater than 0", nameof(enumerable));
          sumOfWeights += element.weight;
        }
      }
      
      var randomWeight = Range(0, sumOfWeights);
      foreach (var element in list)
      {
        randomWeight -= element.weight;
        if (randomWeight <= 0.0f)
          return element;
      }

      return list.LastOrDefault();
    }
    
    /// <summary>
    /// Chooses a random weighted item from an enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="enumerable">The enumerable to pick an element from.</param>
    /// <returns>The element picked by weight.</returns>
    /// <exception cref="ArgumentNullException">If the enumerable is <c>null</c>.</exception>
    public T ChooseWeighted<T>(IEnumerable<T> enumerable) where T : IWeightedElement
    {
      if (enumerable == null)
        throw new ArgumentNullException(nameof(enumerable));

      var list = enumerable as List<T> ?? enumerable.ToList();
      var count = list.Count;
      if (count == 0)
        throw new ArgumentException("enumerable has no elements", nameof(enumerable));

      var sumOfWeights = 0.0f;
      return ChooseWeighted(list, ref sumOfWeights);
    }
    #endregion
  
    #region Converting seeds
    /// <summary>
    /// Convert a seed string to an <c>int</c> to be usable when constructing a random number generator.
    /// </summary>
    /// <param name="seedString">The string to convert.</param>
    /// <returns>The <c>int</c> value parsed from the string if any, or the hashed string value, or <c>null</c> if the string is empty.</returns>
    public static int ConvertSeed(string seedString)
    {
      // Check if the string is empty
      if (string.IsNullOrEmpty(seedString))
        return 0;

      // Check if the string can be parsed as an integer
      if (int.TryParse(seedString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seed))
        return seed;

      // Return a hash of the string
      using var md5 = MD5.Create();
      var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(seedString));
      return BitConverter.ToInt32(hash, 0);
    }
    #endregion
  }
}