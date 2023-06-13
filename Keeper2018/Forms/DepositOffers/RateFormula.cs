using System.Linq;

namespace Keeper2018
{
    public static class RateFormula
    {
        /// <summary>
        /// parse formula like СР + 4.2
        /// for now formula could operate only with СР
        /// </summary>
        /// <param name="str">string to parse</param>
        /// <param name="operation">math operation sign: * + / - </param>
        /// <param name="k">double const</param>
        /// <returns></returns>
        public static bool TryParse(string str, out string operation, out double k)
        {
            operation = "*";
            k = -1;
            var ss = str.Split(' ').Where(s=>!string.IsNullOrEmpty(s)).ToArray();
            if (ss.Length != 3) return false;

            operation = ss[1].Trim();
            if (!double.TryParse(ss[2].Trim(), out k)) return false;

            return true;
        }


        public static double Calculate(string formula, double cp)
        {
            if (!TryParse(formula, out string operation, out double k)) return 0;

            switch (operation)
            {
                case "*": return cp * k;
                case "+": return cp + k;
                case "/": return cp / k;
                case "-": return cp - k;
            }
            return 0;
        }
    }
}
