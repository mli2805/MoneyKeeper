using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class FuelViewModel : Screen
    {
        public List<Fuelling> Rows { get; set; }
        public string Total => $"Итого {Rows.Sum(f=>f.Volume)} литров";

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Автомобильное топливо";
        }
    }
}
