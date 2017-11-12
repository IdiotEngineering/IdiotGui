using System;
using System.Collections.Generic;
using System.Linq;

namespace IdiotGui.Core.Elements
{
  public static class Binding
  {
    public static ValueBinding<T> Closure<T>(Func<T> closure) => closure;

    public static EnumerableChildrenBinding<TSource> SelectAndBind<TSource>(this IEnumerable<TSource> source,
      Func<TSource, Element> factory) => new EnumerableChildrenBinding<TSource>(source, factory);
  }
  
  public class ValueBinding<T>
  {
    #region Fields / Properties

    public T Value => _action();
    private readonly Func<T> _action;

    #endregion

    private ValueBinding(T value) => _action = () => value;

    private ValueBinding(Func<T> action) => _action = action;

    // Constant Binding
    public static implicit operator ValueBinding<T>(T value) => new ValueBinding<T>(value);

    // Closure Binding
    public static implicit operator ValueBinding<T>(Func<T> closure) => new ValueBinding<T>(closure);
  }

  /// <summary>
  ///   The base (abstract) class of all bindings as well as the implicit operators for creating bindings from static sources
  ///   like a Element[] or List of Element
  /// </summary>
  public abstract class ChildrenBinding
  {
    #region Fields / Properties

    public abstract Element[] Elements { get; protected set; }

    #endregion

    public abstract void Update();

    public static implicit operator ChildrenBinding(Element[] staticSource) =>
      new StaticChildrenBinding(staticSource);

    public static implicit operator ChildrenBinding(List<Element> staticSource) =>
      new StaticChildrenBinding(staticSource.ToArray());
  }

  /// <inheritdoc />
  /// <summary>
  ///   A binding on static data. It's nothing more than a wrapper around an Element[]. This allows a single ChildrenBinding
  ///   field to bind either static data:
  ///   myElement.Children = new [] { new Label (...), ... };
  ///   Or bind-able data:
  ///   someData.Where(d =&gt; d.field).Skip(1).SelectAndBind(d =&gt; new Label(d.Text));
  /// </summary>
  public class StaticChildrenBinding : ChildrenBinding
  {
    #region Fields / Properties

    public sealed override Element[] Elements { get; protected set; }

    #endregion

    public StaticChildrenBinding(Element[] elements) => Elements = elements;

    public override void Update()
    {
      // Does nothing as the data is static
    }
  }

  /// <inheritdoc />
  /// <summary>
  ///   A dynamic binding that will be diffed and updated each layout pass. This allows for easy and efficient binding of
  ///   non-static data like the following:
  ///   var listThatChangesALot = ...;
  ///   new Window (...)
  ///   {
  ///   Children = listThatChangesALot
  ///   .Filter(...)
  ///   .Skip(...)
  ///   .SelectAndBind(intValue => new Label($"Int value is: {intValue}"));
  ///   }
  ///   Any additions/removals/re-arraignment oflistThatChangesALot will be reflected in the GUI (the GUI is not "reactive").
  ///   Important Note:
  ///   Elements must be comparable (element.Equals(otherElement)) in a meaningful way for this to work as expected,
  ///   otherwise the diff algorithm has no way of discerning one element from another and will assume nothing changed.
  /// </summary>
  /// <typeparam name="TSource">The type of the SOURCE data this binding is bound to. Int in the example above.</typeparam>
  public class EnumerableChildrenBinding<TSource> : ChildrenBinding
  {
    #region Fields / Properties

    public override Element[] Elements { get; protected set; } = new Element[0];
    private readonly IEnumerable<TSource> _inputSource;
    private readonly Func<TSource, Element> _factory;
    private TSource[] _lastInputs = new TSource[0];

    #endregion

    public EnumerableChildrenBinding(IEnumerable<TSource> source, Func<TSource, Element> factory)
    {
      _inputSource = source;
      _factory = factory;
    }

    public override void Update()
    {
      var sourceArray = _inputSource.ToArray();
      var existingMap = new Dictionary<TSource, Queue<int>>();
      for (var i = 0; i < _lastInputs.Length; i++)
      {
        if (!existingMap.TryGetValue(_lastInputs[i], out var indexes))
        {
          indexes = new Queue<int>();
          existingMap.Add(_lastInputs[i], indexes);
        }
        indexes.Enqueue(i);
      }
      Elements = sourceArray.Select(s =>
      {
        // Try to fetch an existing index
        var existingIndex = -1;
        if (existingMap.TryGetValue(s, out var indexes))
          existingIndex = indexes.Count > 0 ? indexes.Dequeue() : -1;
        return existingIndex >= 0 ? Elements[existingIndex] : _factory(s);
      }).ToArray();
      _lastInputs = sourceArray;
    }
  }
}