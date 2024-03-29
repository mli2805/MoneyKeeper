using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using KeeperDomain;
using KeeperDomain.Exchange;

namespace Keeper2018
{
    public class DbLoader
    {
        private readonly LogFile _logFile;
        private readonly KeeperDataModel _keeperDataModel;
        private readonly IWindowManager _windowManager;
        private readonly DbLoadingViewModel _dbLoadingViewModel;

        public DbLoader(LogFile logFile, KeeperDataModel keeperDataModel, IWindowManager windowManager,
            DbLoadingViewModel dbLoadingViewModel)
        {
            _logFile = logFile;
            _keeperDataModel = keeperDataModel;
            _windowManager = windowManager;
            _dbLoadingViewModel = dbLoadingViewModel;
        }

        public async Task<bool> LoadAndExpand()
        {
            var loadResult = await LoadFromBinOrTxt();
            if (!loadResult.IsSuccess) return false;

            Map((KeeperBin)loadResult.Payload);

            return true;
        }

        private async Task<LibResult> LoadFromBinOrTxt()
        {
            var path = PathFactory.GetDbFullPath();
            LibResult result;
            string question;
            if (File.Exists(path))
            {
                result = await BinSerializer.Deserialize(path);
                if (result.IsSuccess)
                {
                    return result;
                }
                question = result.Exception.Message;
            }
            else
            {
                question = $"Файл {path} не найден";
                result = new LibResult(new Exception(question));
            }
            var vm = new DbAskLoadingViewModel(question);
            _windowManager.ShowDialog(vm);
            if (!vm.Result)
                return result;

            _windowManager.ShowDialog(_dbLoadingViewModel);
            return _dbLoadingViewModel.LoadResult;
        }

        private void Map(KeeperBin bin)
        {
            _keeperDataModel.OfficialRates = new Dictionary<DateTime, OfficialRates>();
            foreach (var rate in bin.OfficialRates)
                _keeperDataModel.OfficialRates.Add(rate.Date, rate);

            _keeperDataModel.ExchangeRates = new Dictionary<DateTime, ExchangeRates>();
            foreach (var exchangeRate in bin.ExchangeRates)
                _keeperDataModel.ExchangeRates.Add(exchangeRate.Date, exchangeRate);

            _keeperDataModel.MetalRates = bin.MetalRates;
            _keeperDataModel.RefinancingRates = bin.RefinancingRates;

            _keeperDataModel.FillInAccountTreeAndDict(bin);

            _keeperDataModel.AssetRates = bin.TrustAssetRates;

            _keeperDataModel.TrustAccounts = bin.TrustAccounts;
            _keeperDataModel.InvestmentAssets = bin.TrustAssets.Select(a => a.Map(_keeperDataModel)).ToList();
            _keeperDataModel.InvestTranModels =
                bin.TrustTransactions.Select(t => t.Map(_keeperDataModel)).ToList();

            _keeperDataModel.Transactions = new Dictionary<int, TransactionModel>();
            foreach (var transaction in bin.Transactions)
                _keeperDataModel.Transactions.Add(transaction.Id, transaction.Map(_keeperDataModel.AcMoDict));

            //TransactionTransformation2024();

            _keeperDataModel.FuellingJoinTransaction(bin.Fuellings);

            _keeperDataModel.Cars = bin.JoinCarParts();
            _keeperDataModel.DepositOffers = bin.JoinDepoParts(_keeperDataModel.AcMoDict);

            if (bin.CardBalanceMemos == null)
                bin.CardBalanceMemos = new List<CardBalanceMemo>();
            _keeperDataModel.CardBalanceMemoModels =
                bin.CardBalanceMemos.Select(m => m.Map(_keeperDataModel.AcMoDict[m.AccountId])).ToList();

            _keeperDataModel.ButtonCollections = bin.ButtonCollections
                .Select(b => b.Map(_keeperDataModel.AcMoDict)).ToList();
            _keeperDataModel.SalaryChanges = bin.SalaryChanges;
            _keeperDataModel.LargeExpenseThresholds = bin.LargeExpenseThresholds;
        }

        private void TransactionTransformation2024()
        {
            _logFile.AppendLine("TransactionTransformation2024");
            FillTransform();

            foreach (var tran in _keeperDataModel.Transactions.Values)
            {
                if (tran.Operation == OperationType.Обмен)
                {
                    var counterparty = tran.Tags.FirstOrDefault(t => t.IsInside(NickNames.External));
                    if (counterparty != null)
                    {
                        tran.Tags.Remove(counterparty);
                        tran.Counterparty = counterparty;
                    }
                    else
                    {
                        counterparty = _keeperDataModel.AcMoDict[839];
                        tran.Counterparty = counterparty;
                        _logFile.AppendLine($"{tran.Timestamp}  {tran.Amount} => {tran.AmountInReturn} {tran.Comment}");
                    }
                  
                }

                if (tran.Operation == OperationType.Доход
                    || tran.Operation == OperationType.Расход)
                {
                    var counterparty = tran.Tags.Single(t => t.IsInside(NickNames.External));
                    tran.Tags.Remove(counterparty);

                    var category = tran.Tags.Single(t => !t.IsInside(NickNames.External));
                    tran.Tags.Remove(category);
                    tran.Counterparty = counterparty;

                    if (_transformation.TryGetValue(category.Id, out var tuple))
                    {
                        tran.Category = _keeperDataModel.AcMoDict[tuple.Item1];
                        //var logTag = "";
                        if (tuple.Item2 != -1)
                        {
                            tran.Tags.Add(_keeperDataModel.AcMoDict[tuple.Item2]);
                            //logTag = $" + {tran.Tags[0].Name}";
                        }

                        //_logFile.AppendLine($"{tran.Timestamp}  {category.Name} => {tran.Category.Name}{logTag}");
                    }
                    else
                    {
                        tran.Category = category;
                    }
                }
            }
        }

        private Dictionary<int, Tuple<int, int>> _transformation;

        private void FillTransform()
        {
            FillAuto();
            FillKommun();
        }

        private void FillAuto()
        {
            _transformation = new Dictionary<int, Tuple<int, int>>
            {
                [707] = new Tuple<int, int>(1027, 1024),
                [712] = new Tuple<int, int>(1028, 1024),

                [709] = new Tuple<int, int>(1027, 1025),
                [710] = new Tuple<int, int>(1028, 1025),

                [713] = new Tuple<int, int>(1027, 1026),
                [747] = new Tuple<int, int>(1029, 1026),
                [714] = new Tuple<int, int>(1030, 1026),
                [715] = new Tuple<int, int>(1032, 1026),
                [748] = new Tuple<int, int>(1031, 1026),
                [749] = new Tuple<int, int>(1033, 1026),
                [720] = new Tuple<int, int>(1034, 1026),

                [717] = new Tuple<int, int>(1027, 1020),
                [727] = new Tuple<int, int>(1029, 1020),
                [718] = new Tuple<int, int>(1030, 1020),
                [719] = new Tuple<int, int>(1032, 1020),
                [728] = new Tuple<int, int>(1031, 1020),
                [750] = new Tuple<int, int>(1033, 1020),
                [721] = new Tuple<int, int>(1034, 1020),

                [729] = new Tuple<int, int>(1022, -1),
            };
        }

        private void FillKommun()
        {
            _transformation[418] = new Tuple<int, int>(1023, 1043);
            _transformation[419] = new Tuple<int, int>(1036, 1043);
            _transformation[420] = new Tuple<int, int>(1016, 1043);

            _transformation[278] = new Tuple<int, int>(1023, 1018);
            _transformation[279] = new Tuple<int, int>(1036, 1018);
            _transformation[280] = new Tuple<int, int>(1016, 1018);
            _transformation[281] = new Tuple<int, int>(1035, 1018);
            _transformation[282] = new Tuple<int, int>(1037, 1018);

            _transformation[364] = new Tuple<int, int>(1016, 1019);
            _transformation[365] = new Tuple<int, int>(1035, 1019);
            _transformation[366] = new Tuple<int, int>(1038, 1019);
            _transformation[367] = new Tuple<int, int>(1039, 1019);
            _transformation[519] = new Tuple<int, int>(1040, 1019);
            _transformation[599] = new Tuple<int, int>(1023, 1019);
            _transformation[981] = new Tuple<int, int>(1041, 1019);
        }
    }
}