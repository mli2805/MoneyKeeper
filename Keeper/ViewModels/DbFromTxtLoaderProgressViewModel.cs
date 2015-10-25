using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Keeper.ViewModels
{
    class DbFromTxtLoaderProgressViewModel : Screen
    {
        private ObservableCollection<string> _previousOperations;
        private string _currentOperation;

        public ObservableCollection<string> PreviousOperations
        {
            get { return _previousOperations; }
            set
            {
                if (Equals(value, _previousOperations)) return;
                _previousOperations = value;
                NotifyOfPropertyChange();
            }
        }

        public string CurrentOperation
        {
            get { return _currentOperation; }
            set
            {
                if (value == _currentOperation) return;
                _currentOperation = value;
                NotifyOfPropertyChange();
            }
        }

        public DbFromTxtLoaderProgressViewModel()
        {
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Loading...";
//            PreviousOperations = new ObservableCollection<string>();
//            PreviousOperations.Add("bla-bla");
//            PreviousOperations.Add("bla-bla");
//            PreviousOperations.Add("bla-bla");
//            PreviousOperations.Add("bla-bla");
//
//            CurrentOperation = "wait...";

            base.OnViewLoaded(view);
        }

    }
}
