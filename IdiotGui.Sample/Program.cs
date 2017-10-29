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
                Background = Color.DarkMagenta,
                Height = (SFixed) 50,
                Border = new BorderStyle(1, Color.White, 5),
                MouseEnter = elem => elem.Background = Color.CornflowerBlue,
                MouseLeave = elem => elem.Background = Color.DarkMagenta
              },
              new Button(),
              new Button()
            }
          },
          new Element {Background = Color.Chartreuse, Width = new SFill()},
          new Element {Background = Color.DarkGreen, Width = (SFixed) 400}
        }
      };
      var windowTwo = new Window("Window Two", 200, 200)
      {
        Children = new List<Element>
        {
          //new Element {Background = Color.DarkRed, Width = (SFixed) 10},
          new Element {Background = Color.Chartreuse, Width = new SFill()},
          new Element {Background = Color.Chocolate, Width = new SFill()},
          new Element {Background = Color.CadetBlue, Width = new SFill()},
          new Element {Background = Color.Gainsboro, Width = new SFill()},
          //new Element {Background = Color.DarkGreen, Width = (SFixed) 20}
        }
      };

      windowOne.Closing += (sender, eventArgs) => quit = true;
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