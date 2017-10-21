using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Security.AccessControl;
using System.Threading;
using IdiotGui.Core;
using IdiotGui.Core.BasicTypes;

namespace IdiotGui.Sample
{
  class Program
  {
    private static void Main(string[] args)
    {
      var quit = false;
      var windowOne = new Window("Window One", 600, 600);
      var windowTwo = new Window("Window Two", 600, 600) {Location = new Point(600, 600)};

      windowOne.Closing += (sender, eventArgs) => quit = true;
      windowTwo.Closing += (sender, eventArgs) => quit = true;

      while (!quit)
      {
        windowOne.Update();
        windowTwo.Update();
        Thread.Sleep(20);
      }
    }
  }
}
