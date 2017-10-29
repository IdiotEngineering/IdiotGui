using System;
using System.Collections.Generic;
using System.Linq;
using IdiotGui.Core.BasicTypes;

namespace IdiotGui.Core.Elements
{
  /// <summary>
  ///   The base-most class for a GUI control, represents little more than the layout of the control and it's children and
  ///   input Chain Of Command handling.
  /// </summary>
  public partial class Element
  {
    #region Fields / Properties

    /// <summary>
    ///   The width of this control. This is CSS inspired. See Element Layout documentation.
    /// </summary>
    public SSizing Width = new SFitChildren();

    /// <summary>
    ///   The height of this control. This is CSS inspired. See Element Layout documentation.
    /// </summary>
    public SSizing Height = new SFitChildren();

    /// <summary>
    ///   The minimum size the control can shrink to.
    /// </summary>
    public Size MinSize;

    /// <summary>
    ///   How children controls will be placed inside the ContentArea.
    /// </summary>
    public ChildAlignments ChildAlignment = ChildAlignments.Horizontal;

    /// <summary>
    ///   The Margin around (outside) this element. This is the outer shell, before the border is applied.
    /// </summary>
    public BorderSize Margin = 6;

    /// <summary>
    ///   The padding inside this element, between the Border and the ContentArea.
    /// </summary>
    public BorderSize Padding = 0;

    /// <summary>
    ///   The border style of this element, much like the CSS property "border".
    /// </summary>
    public BorderStyle Border = new BorderStyle(1, Theme.Colors.DefaultBorder);

    /// <summary>
    ///   All children elements of this element (all drawn within ContentArea if overflow is hidden).
    /// </summary>
    public List<Element> Children = new List<Element>();

    /// <summary>
    ///   The total area of this element (including the surrounding margin)
    /// </summary>
    public Rectangle BoxArea { get; protected set; }

    /// <summary>
    ///   The area children will be drawn into. This is after the border and padding have been added.
    /// </summary>
    public Rectangle ContentArea { get; protected set; }

    /// <summary>
    ///   The computed min-size of this element (greater than or equal to MinSize).
    /// </summary>
    protected Size ComputedMinSize;

    #endregion

    public Element GetTopmostElementAtPoint(Point point)
    {
      // Out of bounds check.
      if (point.X < ContentArea.Left || point.Y < ContentArea.Top ||
          point.X > ContentArea.Left + ContentArea.Width ||
          point.Y > ContentArea.Top + ContentArea.Height) return null;
      // Return child that is top-most, or this if they are all out of bounds
      return Children.Select(c => c.GetTopmostElementAtPoint(point)).FirstOrDefault(c => c != null) ?? this;
    }

    internal void DumpLayout(int indent = 0)
    {
      Console.WriteLine(string.Concat(Enumerable.Repeat("  ", indent)) + BoxArea + " - " + ContentArea);
      foreach (var child in Children) child.DumpLayout(indent + 1);
    }

    /// <summary>
    ///   Lays out children components based on our own size. By the time this is called for any Element, the current element
    ///   has already had it's ContentArea set.
    /// </summary>
    protected void LayoutChildren()
    {
      ContentArea = BoxArea - Margin - Border.Size - Padding;
      // Quick short-circuit for leaf Elements
      if (Children.Count == 0) return;
      // Handle each orientation individual, it turned into too much of a mess to try and keep major and minor axises separate.
      switch (ChildAlignment)
      {
        case ChildAlignments.Vertical:
          var minChildrenHeight = Children.Select(c => c.ComputedMinSize.Height).Sum();
          // A positive value here means an under-flow (we can expand out any SWeighted children).
          // A negative value here means an overflow (we can't fit all children). Sad times.
          var remainingContentHeight = ContentArea.Height - minChildrenHeight;
          // Sum up all SWeighted children's weight (because our pleb user might not actually have made that sum to 1.0)
          var totalChildrenWeightedHeight = Children
            .Where(c => c.Height is SWeighted)
            .Select(c => ((SWeighted) c.Height).Weight)
            .Sum();
          // Layout each child. We allow all children to be >= their MinSize.Height, and expand out and SWeighted into
          // remainingHeight (if it's positive).
          var childBoxOffsetForVertical = ContentArea.Location;
          foreach (var child in Children)
          {
            var width = Math.Min(child.Width is SFixed @fixed ? @fixed.Size : ContentArea.Width, ContentArea.Width);
            var paddingHeight = child.Margin.Top + child.Margin.Bottom + child.Border.Size.Top +
                               child.Border.Size.Bottom + Padding.Top + Padding.Bottom;
            switch (child.Height)
            {
              case SFixed sFixed:
                // The size is fixed, the child is that size. End of story.
                child.BoxArea = new Rectangle(childBoxOffsetForVertical, new Size(width, paddingHeight + sFixed.Size));
                break;
              case SWeighted sWeighted:
                // The size is weighted, if remainingContentHeight is positive, use that. Otherwise the min height.
                var relativeWeight = sWeighted.Weight / totalChildrenWeightedHeight;
                var possibleOverflow = (int) (remainingContentHeight * relativeWeight);
                child.BoxArea = new Rectangle(childBoxOffsetForVertical,
                  new Size(width, Math.Max(child.ComputedMinSize.Height, paddingHeight + possibleOverflow)));
                break;
              case SFitChildren _:
                // The size is just the min size
                child.BoxArea = new Rectangle(childBoxOffsetForVertical,
                  new Size(width, paddingHeight + child.ComputedMinSize.Height));
                break;
            }
            childBoxOffsetForVertical.Top += child.BoxArea.Height;
            child.LayoutChildren();
          }
          break;
        case ChildAlignments.Horizontal:
          var minChildrenWidth = Children.Select(c => c.ComputedMinSize.Width).Sum();
          var remainingContentWidth = ContentArea.Width - minChildrenWidth;
          var totalChildrenWeightedWidth = Children
            .Where(c => c.Width is SWeighted)
            .Select(c => ((SWeighted) c.Width).Weight)
            .Sum();
          var childBoxOffsetForHorizontal = ContentArea.Location;
          foreach (var child in Children)
          {
            var height = Math.Min(child.Height is SFixed @fixed ? @fixed.Size : ContentArea.Height, ContentArea.Height);
            var paddingWidth = child.Margin.Left + child.Margin.Right + child.Border.Size.Left +
                               child.Border.Size.Right + Padding.Left + Padding.Right;
            switch (child.Width)
            {
              case SFixed sFixed:
                child.BoxArea = new Rectangle(childBoxOffsetForHorizontal, new Size(paddingWidth + sFixed.Size, height));
                break;
              case SWeighted sWeighted:
                var relativeWeight = sWeighted.Weight / totalChildrenWeightedWidth;
                var possibleOverflow = (int) (remainingContentWidth * relativeWeight);
                child.BoxArea = new Rectangle(childBoxOffsetForHorizontal,
                  new Size(Math.Max(child.ComputedMinSize.Width, paddingWidth + possibleOverflow), height));
                break;
              case SFitChildren _:
                child.BoxArea = new Rectangle(childBoxOffsetForHorizontal,
                  new Size(paddingWidth + child.ComputedMinSize.Width, height));
                break;
            }
            childBoxOffsetForHorizontal.Left += child.BoxArea.Width;
            child.LayoutChildren();
          }
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    /// <summary>
    ///   Computes the minimum size of all elements in the tree. Note that many will have zero dimensions but (hopefully) won't
    ///   be zero after the full layout.
    /// </summary>
    protected void ComputeMinimumSize()
    {
      // Compute min size on the way up the tree
      foreach (var child in Children) child.ComputeMinimumSize();
      // The min size starts off a the margin + border + padding
      var minShellSize = Margin + Border.Size + Padding;
      ComputedMinSize = new Size(minShellSize.Left + minShellSize.Right, minShellSize.Top + minShellSize.Bottom);
      switch (Height)
      {
        case SFixed sFixed:
          // The min size we can be is the fixed size, plus padding, border and margin
          ComputedMinSize.Height += sFixed.Size;
          break;
        case SWeighted _:
          // The min size we can be for weighted is just the MinSize
          ComputedMinSize.Height += MinSize.Height;
          break;
        case SFitChildren _:
          ComputedMinSize.Height += Math.Max(MinSize.Height, Children.Select(c => c.ComputedMinSize.Height).Sum());
          break;
      }
      // Same as above, but for Width
      switch (Width)
      {
        case SFixed sFixed:
          ComputedMinSize.Width += sFixed.Size;
          break;
        case SWeighted _:
          ComputedMinSize.Width += MinSize.Width;
          break;
        case SFitChildren _:
          ComputedMinSize.Width += Math.Max(MinSize.Width, Children.Select(c => c.ComputedMinSize.Width).Sum());
          break;
      }
    }
  }
}