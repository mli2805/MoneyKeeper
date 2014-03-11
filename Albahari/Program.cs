using System;

namespace Albahari
{
  class Program
  {
    static private int _start = 0;
    static void Main()
    {
      Func<int> natural = Natural();
      Console.WriteLine(natural());
      Console.WriteLine(natural());

// my implementation    
      natural = Nat;
      Console.WriteLine(natural());
      Console.WriteLine(natural());

      Console.ReadKey();
    }

    static Func<int> Natural()
    {
      int seed = 0;
      return () => seed++;
    }

    static int Nat()
    {
      return _start++;
    }
  
  }
}
