using System.Collections.Generic;
using System.Threading;
using IdiotGui.Core;
using IdiotGui.Core.BasicTypes;
using IdiotGui.Core.Elements;

namespace IdiotGui.Sample
{
  internal class Program
  {
    private static void Main()
    {
      var quit = false;
      var windowOne = new Window("Window One", 1680, 1050)
      {
        Children = new List<Element>
        {
          new Element
          {
            Background = Color.DarkRed,
            Width = (SFixed) 300,
            ChildAlignment = ChildAlignments.Vertical,
            Children = new List<Element>
            {
              new Element
              {
                Background = Color.DarkOrange,
                ChildAlignment = ChildAlignments.Vertical,
                Children = new List<Element>
                {
                  new Element
                  {
                    Background = Color.DarkMagenta,
                    Height = (SFixed) 50,
                    Border = new BorderStyle(1, Color.White, 5)
                  },
                  new Button(),
                  new Button(),
                  new Button(),
                  new Button(),
                  new Element
                  {
                    ChildAlignment = ChildAlignments.Horizontal,
                    Background = Color.DarkSlateGray,
                    Children = new List<Element>
                    {
                      new Button(),
                      new Button(),
                      new Button()
                    }
                  }
                }
              }
            }
          },
          new Element {Background = Color.Chartreuse, Width = new SFill()},
          new Element {Background = Color.DarkGreen, Width = (SFixed) 400}
        }
      };
      windowOne.Closing += (sender, eventArgs) => quit = true;

      var windowTwo = new Window("Window Two", 1000, 1000)
      {
        Children = new List<Element>
        {
          new Label
          {
            Background = Color.DarkGreen,
            Width = (SFixed) 150,
            Text = "This is a long string that should get truncated because it's only one line and doesn't all fit on one line.",
          },
          new Label
          {
            Background = Color.DarkRed,
            Width = new SFill(),
            Text = "This is a long string that should wrap around. It also has explicit line-breaks in it like right here\nand also some spaces and tabs like right here\t. This text should wrap without a bottom bounds.",
            MultiLine = true
          },
          new Label
          {
            Background = Color.DarkBlue,
            Width = (SFixed) 150,
            Text = "This should fit fine."
          }
        }
      };
      windowTwo.Closing += (sender, eventArgs) => quit = true;
      while (!quit)
      {
        windowOne.Update();
        windowOne.Render();
        windowTwo.Update();
        windowTwo.Render();
        Thread.Sleep(10);
      }
      windowOne.Dispose();
      windowTwo.Dispose();
    }
  }
}