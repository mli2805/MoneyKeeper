using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    /// <summary>
    /// Метод наименьших квадратов
    /// </summary>
    public static class LeastSquares
    {
        /// <summary>
        /// для нахождения линейной функции апроксимации надо решить систему 2 уравнений с 2 неизвестными
        /// a * sum(x*x) + b * sum(x) = sum(x*y)
        /// a * sum(x) + b*n = sum(y)
        /// где sum - сумма значений, n - количество точек
        /// a и b коэффициенты результирующей функции
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static (double, double) GetLinear(List<(int, double)> data)
        {
            // составляем систему 2 уравнений с 2 неизвестными
            // u1 и u2 - первое и второе уравнение
            // т.е. получаем
            // a * u11 + b * u12 = u13
            // a * u21 + b * u22 = u23 
            double u11 = data.Select(p => p.Item1 * p.Item1).Sum();
            double u12 = data.Select(p => p.Item1).Sum();
            double u13 = data.Select(p => p.Item1 * p.Item2).Sum();

            double u21 = u12;
            double u22 = data.Count;
            double u23 = data.Select(p=>p.Item2).Sum();

            // выражаем а из первого уравнения
            // a = u13 / u11 - b * u12 / u11, для краткости a = v1 - b * v2
            // подставляем во второе
            // (v1 - b * v2) * u21 + b * u22 = u23 
            // v1 * u21 - b * v2 * u21 + b * u22 = u23
            // u13 / u11 * u21 - b * u12 / u11 * u21 + b * u22 = u23
            // b * (u22 - u12 / u11 * u21) = u23 - u13 / u11 * u21
            // таким образом:
            var b = (u23 - u13 / u11 * u21) / (u22 - u12 / u11 * u21);
            var a = u13 / u11 - b * u12 / u11;
            return (a, b);
        }
    }
}
