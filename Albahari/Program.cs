using System;

namespace Albahari
{
  class Program
  {
    static void Main()
    {
      Func<int> natural = Natural();
      Console.WriteLine(natural());
      Console.WriteLine(natural());

      Console.ReadKey();
    }

    static Func<int> Natural()
    {
      int seed = 0;
      return () => seed++;
    }

  
  }
}
