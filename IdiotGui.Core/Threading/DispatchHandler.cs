using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdiotGui.Core.Threading
{
  /// <summary>
  /// Attribute class for marking up a dispatch handler. These can be applied to both static functions as well as methods.
  /// </summary>
  public class DispatchHandler : Attribute
  {
    public DispatchHandler(Dispatch.Phase phase)
    {
    }
  }
}
