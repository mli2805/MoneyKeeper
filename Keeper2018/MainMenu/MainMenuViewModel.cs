using Caliburn.Micro;

namespace Keeper2018
{
    public class MainMenuViewModel : PropertyChangedBase
    {
        private readonly IWindowManager _windowManager;
        private readonly OfficialRatesViewModel _officialRatesViewModel;
        private readonly TransactionsViewModel _transactionsViewModel;
        private readonly DepositOffersViewModel _depositOffersViewModel;
        private readonly ArticlesAssociationsViewModel _articlesAssociationsViewModel;

        public MainMenuViewModel(IWindowManager windowManager,
            OfficialRatesViewModel officialRatesViewModel, TransactionsViewModel transactionsViewModel,
            DepositOffersViewModel depositOffersViewModel, ArticlesAssociationsViewModel articlesAssociationsViewModel)
        {
            _windowManager = windowManager;
            _officialRatesViewModel = officialRatesViewModel;
            _transactionsViewModel = transactionsViewModel;
            _depositOffersViewModel = depositOffersViewModel;
            _articlesAssociationsViewModel = articlesAssociationsViewModel;
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
                    // _windowManager.ShowDialog(_tagAssociationsViewModel);
                    _windowManager.ShowDialog(_articlesAssociationsViewModel);
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
