using Caliburn.Micro;

namespace Keeper2018
{
    public class MainMenuViewModel : PropertyChangedBase
    {
        private readonly IWindowManager _windowManager;
        private readonly OfficialRatesViewModel _officialRatesViewModel;
        private readonly TransactionsViewModel _transactionsViewModel;
        private readonly TagAssociationsViewModel _tagAssociationsViewModel;
        private readonly DepositOffersViewModel _depositOffersViewModel;

        public MainMenuViewModel(IWindowManager windowManager,
            OfficialRatesViewModel officialRatesViewModel, TransactionsViewModel transactionsViewModel,
            TagAssociationsViewModel tagAssociationsViewModel, DepositOffersViewModel depositOffersViewModel)
        {
            _windowManager = windowManager;
            _officialRatesViewModel = officialRatesViewModel;
            _transactionsViewModel = transactionsViewModel;
            _tagAssociationsViewModel = tagAssociationsViewModel;
            _depositOffersViewModel = depositOffersViewModel;
        }

        public void ActionMethod(MainMenuAction action)
        {
            switch (action)
            {
                case MainMenuAction.ShowOfficialRatesForm:
                    ShowOfficialRatesForm();
                    break;
                case MainMenuAction.ShowTransactionsForm:
                    ShowTransactionsForm();
                    break;
                case MainMenuAction.ShowMonthAnalysisForm:
                    break;
                case MainMenuAction.ShowDepositOffersForm:
                    _windowManager.ShowDialog(_depositOffersViewModel);
                    break;
                case MainMenuAction.ShowTagAssociationsForm:
                    _windowManager.ShowDialog(_tagAssociationsViewModel);
                    break;
            }

        }

        public void ShowOfficialRatesForm()
        {
            _windowManager.ShowDialog(_officialRatesViewModel);
        }

        public void ShowTransactionsForm()
        {
            _windowManager.ShowDialog(_transactionsViewModel);
        }
    }
}
