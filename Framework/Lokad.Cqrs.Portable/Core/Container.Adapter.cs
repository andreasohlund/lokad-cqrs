
using System;
using System.Collections.Generic;

namespace Funq
{
  public partial class Container
  {
      public Stack<IRegistrationSource> Sources = new Stack<IRegistrationSource>();


  }

  /// <summary>
  /// Allow delegation of dependencies to other IOC's
  /// </summary>
  public interface IRegistrationSource
  {
      bool Supports(Type type);
      Func<Container, object> GetProvider(Type type);
  }

}