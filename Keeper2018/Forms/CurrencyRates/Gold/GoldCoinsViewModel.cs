using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class GoldCoinsViewModel : Screen
    {
        private readonly KeeperDataModel _keeperDataModel;
        public List<GoldCoinsModel> Rows { get; set; }

        public GoldCoinsViewModel(KeeperDataModel keeperDataModel)
        {
            _keeperDataModel = keeperDataModel;
        }

        public void Initialize()
        {
            Rows = _keeperDataModel.MetalRates
                .Where(m => m.Metal == Metal.Gold && m.Proba == 900)
                .Select(m => new GoldCoinsModel()
                {
                    Id = m.Id, Date = m.Date, MinfinGold900Rate = m.Price, BynUsd = _keeperDataModel.GetRate(m.Date, CurrencyCode.BYN).Value
                })
                .ToList();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Золотые монеты";
        }

        public override void CanClose(Action<bool> callback)
        {
            _keeperDataModel.MetalRates = Rows.Select(l => new MinfinMetalRate()
            {
                Id = l.Id, Date = l.Date, Metal = Metal.Gold, Proba = 900, Price = l.MinfinGold900Rate
            }).ToList();
            base.CanClose(callback);
        }
    }
}
