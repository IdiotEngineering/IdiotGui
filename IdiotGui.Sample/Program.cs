using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using IdiotGui.Core;
using IdiotGui.Core.BasicTypes;
using IdiotGui.Core.Elements;
using IdiotGui.Core.Utilities;

namespace IdiotGui.Sample
{
  internal class Program
  {
    private static void Main()
    {
      var quit = false;
      //var windowOne = new Window("Window One", 1680, 1050)
      //{
      //  Children = new List<Element>
      //  {
      //    new Element
      //    {
      //      Background = Color.DarkRed,
      //      Width = (SFixed) 300,
      //      ChildAlignment = ChildAlignments.Vertical,
      //      Children = new List<Element>
      //      {
      //        new Element
      //        {
      //          Background = Color.DarkOrange,
      //          ChildAlignment = ChildAlignments.Vertical,
      //          Children = new List<Element>
      //          {
      //            new Element
      //            {
      //              Background = Color.DarkMagenta,
      //              Height = (SFixed) 50,
      //              Border = new BorderStyle(1, Color.White, 5)
      //            },
      //            new Button(),
      //            new Button(),
      //            new Button(),
      //            new Button(),
      //            new Element
      //            {
      //              ChildAlignment = ChildAlignments.Horizontal,
      //              Background = Color.DarkSlateGray,
      //              Children = new List<Element>
      //              {
      //                new Button(),
      //                new Button(),
      //                new Button()
      //              }
      //            }
      //          }
      //        }
      //      }
      //    },
      //    new Element {Background = Color.Chartreuse, Width = new SFill()},
      //    new Element {Background = Color.DarkGreen, Width = (SFixed) 400}
      //  }
      //};
      //windowOne.Closing += (sender, eventArgs) => quit = true;

      //var windowTwo = new Window("Window Two", 1000, 1000)
      //{
      //  Height = new SFill(),
      //  Width = new SFill(),
      //  ChildAlignment = ChildAlignments.Vertical,
      //  Children = new List<Element>
      //  {
      //    new Element
      //    {
      //      ChildAlignment = ChildAlignments.Horizontal,
      //      Height = new SWeighted(2),
      //      Border = new BorderStyle(1, Color.Pink),
      //      Children = new[] {TextVerticalPosition.Top, TextVerticalPosition.Middle, TextVerticalPosition.Bottom}
      //        .Select(vert =>
      //          new Element
      //          {
      //            Width = new SWeighted(),
      //            Border = new BorderStyle(1, Color.Red),
      //            ChildAlignment = ChildAlignments.Vertical,
      //            Children = new[]
      //                {TextHorizontalPosition.Left, TextHorizontalPosition.Middle, TextHorizontalPosition.Right}
      //              .Select(horiz => (Element) new Label
      //              {
      //                Height = new SWeighted(),
      //                VerticalTextPostion = vert,
      //                HorizontalPosition = horiz,
      //                Text = vert + " : " + horiz,
      //                Border = new BorderStyle(1, Color.White)
      //              }).ToList()
      //          }
      //        ).ToList()
      //    },
      //    new Element
      //    {
      //      ChildAlignment = ChildAlignments.Horizontal,
      //      Height = new SWeighted(1),
      //      Width = new SFill(),
      //      Border = new BorderStyle(1, Color.Blue),
      //      Children = new[] {TextHorizontalPosition.Left, TextHorizontalPosition.Middle, TextHorizontalPosition.Right}
      //        .Select(horiz => (Element) new Label
      //        {
      //          Height = new SFill(),
      //          Width = new SWeighted(),
      //          HorizontalPosition = horiz,
      //          MultiLine = true,
      //          Text =
      //            "This is a long string that should start wrapping around. It also has explicit newlines in it like at the end of this line.\n It also has some tabs like [\t] and stuff.",
      //          Border = new BorderStyle(1, Color.White)
      //        }).ToList()
      //    }
      //  }
      //};
      // A normal array of strings
      var thingsToAdd = new[] {"The kitchen sink", "More indirection", "More confusing stuff", "CSS? lol"};

      var windowTwo = new Window("IdiotGui Todo List", 300, 200)
      {
        ChildAlignment = ChildAlignments.Vertical,
        // Children is just a LINQ expression now, no 10 layers of indirection.
        Children = thingsToAdd.Select(todo =>
          new Label
          {
            Height = (SFixed) 25,
            Border = new BorderStyle(1, Color.Cyan),
            Text = todo
          }
        )
      };

      windowTwo.Closing += (sender, eventArgs) => quit = true;
      while (!quit)
      {
        //windowOne.Update();
        //windowOne.Render();
        windowTwo.Update();
        windowTwo.Render();
        Thread.Sleep(10);
      }
      //windowOne.Dispose();
      windowTwo.Dispose();
    }
  }
}