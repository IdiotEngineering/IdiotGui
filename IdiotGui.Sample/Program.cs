using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdiotGui.Core;
using IdiotGui.Core.BasicTypes;
using IdiotGui.Core.Elements;

namespace IdiotGui.Sample
{
  internal class Bindable
  {
    #region Fields / Properties

    public Action Closure;

    #endregion
  }

  internal class Program
  {
    private void DoTheThings()
    {
      var quit = false;
      var data = "Hello, World";

      var windowTwo = new Window("IdiotGui Todo List", 300, 200)
      {
        ChildAlignment = ChildAlignments.Vertical,
        Children = new Element[]
        {
          new Label
          {
            Text = Binding.Closure(() => data)
          },
          new Label
          {
            Text = data
          }
        }
      };
      data = "FooBar";

      Task.Run(() =>
      {
        while (true) data = Console.ReadLine();
      });

      // A normal list of strings. Will always be rendered, even if it changes later (the GUI is "reactive").
      var changingList = new List<string> {"The kitchen sink", "More indirection", "More confusing stuff", "CSS? lol"};
      var window = new Window("IdiotGui Todo List", 300, 200)
      {
        ChildAlignment = ChildAlignments.Vertical,
        // Note that up until the special "SelectAndBind", this was just a normal LINQ expression
        Children = changingList
          .Where(str => !string.IsNullOrWhiteSpace(str))
          .Select(str => str.ToUpper())
          .SelectAndBind(todo =>
            new Label
            {
              Height = (SFixed) 25,
              Border = new BorderStyle(1, Color.Cyan),
              // Works exactly like you would expect, including later updates
              Text = todo
            }
          )
      };

      windowTwo.Closing += (sender, eventArgs) => quit = true;
      while (!quit)
      {
        windowTwo.Update();
        windowTwo.Render();
      }
      windowTwo.Dispose();
    }

    private static void Main()
    {
      new Program().DoTheThings();
    }
  }
}