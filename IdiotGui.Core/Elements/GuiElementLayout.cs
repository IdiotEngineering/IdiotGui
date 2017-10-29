using System;
using System.Collections.Generic;
using System.IO;
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
    public BorderSize Margin = 10;

    /// <summary>
    ///   The padding inside this element, between the Border and the ContentArea.
    /// </summary>
    public BorderSize Padding = 0;

    /// <summary>
    ///   The border style of this element, much like the CSS property "border".
    /// </summary>
    public BorderStyle Border = new BorderStyle(0, Theme.Colors.DefaultBorder);

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

    /// <summary>
    ///   The Computed margin (after margin-collapsing)
    /// </summary>
    protected BorderSize ComputedMargin;

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

    /// <summary>
    ///   Lays out children components based on our own size. By the time this is called for any Element, the current element
    ///   has already had it's ContentArea set.
    /// </summary>
    protected void LayoutChildren()
    {
      ContentArea = BoxArea - ComputedMargin - Border.Size - Padding;
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
            var paddingHeight = child.ComputedMargin.Top + child.ComputedMargin.Bottom + child.Border.Size.Top +
                                child.Border.Size.Bottom + Padding.Top + Padding.Bottom;
            var paddingWidth = child.ComputedMargin.Left + child.ComputedMargin.Right + child.Border.Size.Left +
                               child.Border.Size.Right + Padding.Left + Padding.Right;
            var width = child.Width is SFixed @fixed ? @fixed.Size + paddingWidth : ContentArea.Width;
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
                // The size is just the min size. Note that padding is already built into that.
                child.BoxArea = new Rectangle(childBoxOffsetForVertical,
                  new Size(width, child.ComputedMinSize.Height));
                break;
            }
            childBoxOffsetForVertical.Top += child.BoxArea.Height;
            child.LayoutChildren();
          }
          break;
        case ChildAlignments.Horizontal:
          // Just like the above but for horizontal
          var minChildrenWidth = Children.Select(c => c.ComputedMinSize.Width).Sum();
          var remainingContentWidth = ContentArea.Width - minChildrenWidth;
          var totalChildrenWeightedWidth = Children
            .Where(c => c.Width is SWeighted)
            .Select(c => ((SWeighted) c.Width).Weight)
            .Sum();
          var childBoxOffsetForHorizontal = ContentArea.Location;
          foreach (var child in Children)
          {
            var paddingHeight = child.ComputedMargin.Top + child.ComputedMargin.Bottom + child.Border.Size.Top +
                                child.Border.Size.Bottom + Padding.Top + Padding.Bottom;
            var paddingWidth = child.ComputedMargin.Left + child.ComputedMargin.Right + child.Border.Size.Left +
                               child.Border.Size.Right + Padding.Left + Padding.Right;
            var height = child.Height is SFixed @fixed ? @fixed.Size + paddingHeight : ContentArea.Height;
            switch (child.Width)
            {
              case SFixed sFixed:
                child.BoxArea = new Rectangle(childBoxOffsetForHorizontal,
                  new Size(paddingWidth + sFixed.Size, height));
                break;
              case SWeighted sWeighted:
                var relativeWeight = sWeighted.Weight / totalChildrenWeightedWidth;
                var possibleOverflow = (int) (remainingContentWidth * relativeWeight);
                child.BoxArea = new Rectangle(childBoxOffsetForHorizontal,
                  new Size(Math.Max(child.ComputedMinSize.Width, paddingWidth + possibleOverflow), height));
                break;
              case SFitChildren _:
                child.BoxArea = new Rectangle(childBoxOffsetForHorizontal,
                  new Size(child.ComputedMinSize.Width, height));
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
    protected void ComputeSizes()
    {
      // Compute margin-collapsing on the way down the tree
      foreach (var child in Children) child.ComputedMargin = child.Margin;
      switch (ChildAlignment)
      {
        case ChildAlignments.Horizontal:
          // Collapse horizontal
          foreach (var childPair in Children.Zip(Children.Skip(1), (lchild, rchild) => new {lchild, rchild}))
          {
            // Collapse all middle-margins to 1/2 the value of the max of the two margins
            childPair.lchild.ComputedMargin.Right =
              Math.Max(childPair.lchild.Margin.Right, childPair.rchild.Margin.Left) / 2.0f;
            childPair.rchild.ComputedMargin.Left = childPair.lchild.ComputedMargin.Right;
          }
          break;
        case ChildAlignments.Vertical:
          // Collapse vertical
          foreach (var childPair in Children.Zip(Children.Skip(1), (lchild, rchild) => new {lchild, rchild}))
          {
            // Collapse all middle-margins to 1/2 the value of the max of the two margins
            childPair.lchild.ComputedMargin.Bottom =
              Math.Max(childPair.lchild.Margin.Bottom, childPair.rchild.Margin.Top) / 2.0f;
            childPair.rchild.ComputedMargin.Top = childPair.lchild.ComputedMargin.Bottom;
          }
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      // Compute min size on the way up the tree
      foreach (var child in Children) child.ComputeSizes();
      // The min size starts off a the margin + border + padding
      var minShellSize = ComputedMargin + Border.Size + Padding;
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
          switch (ChildAlignment)
          {
            case ChildAlignments.Horizontal:
              // If our children are in a line then just find the largest of them
              ComputedMinSize.Height += Math.Max(MinSize.Height,
                Children.Select(c => c.ComputedMinSize.Height).DefaultIfEmpty().Max());
              break;
            case ChildAlignments.Vertical:
              // Otherwise sum them up
              ComputedMinSize.Height += Math.Max(MinSize.Height,
                Children.Select(c => c.ComputedMinSize.Height).Sum());
              break;
            default:
              throw new ArgumentOutOfRangeException();
          }
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
          switch (ChildAlignment)
          {
            case ChildAlignments.Horizontal:
              // Otherwise sum them up
              ComputedMinSize.Width += Math.Max(MinSize.Width,
                Children.Select(c => c.ComputedMinSize.Width).Sum());
              break;
            case ChildAlignments.Vertical:
              // If our children are in a line then just find the largest of them
              ComputedMinSize.Width += Math.Max(MinSize.Width,
                Children.Select(c => c.ComputedMinSize.Width).DefaultIfEmpty().Max());
              break;
            default:
              throw new ArgumentOutOfRangeException();
          }
          break;
      }
    }

    internal void DumpLayout(int indent = 0)
    {
      Console.WriteLine(string.Concat(Enumerable.Repeat("  ", indent)) + BoxArea + " - " + ContentArea);
      foreach (var child in Children) child.DumpLayout(indent + 1);
    }
  }
}