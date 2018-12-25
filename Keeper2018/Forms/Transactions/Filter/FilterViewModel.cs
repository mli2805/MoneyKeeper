using Caliburn.Micro;

namespace Keeper2018
{
    public class FilterViewModel : Screen
    {
        private int _left;
        public int Left
        {
            get { return _left; }
            set
            {
                if (value == _left) return;
                _left = value;
                NotifyOfPropertyChange();
            }
        }

        private int _top;
        public int Top
        {
            get { return _top; }
            set
            {
                if (value == _top) return;
                _top = value;
                NotifyOfPropertyChange();
            }
        }

        private int _width;
        public int Width
        {
            get { return _width; }
            set
            {
                if (value == _width) return;
                _width = value;
                NotifyOfPropertyChange();
            }
        }

        private FilterModel _filterModel;
        public FilterModel FilterModel
        {
            get { return _filterModel; }
            set
            {
                if (Equals(value, _filterModel)) return;
                _filterModel = value;
                NotifyOfPropertyChange();
            }
        }

        public FilterViewModel(FilterModel filterModel)
        {
            FilterModel = filterModel;
        }

        public void PlaceIt(int left, int top, int width)
        {
            Left = left - width - 1;
            Top = top + 40;
            Width = width;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Фильтр";
        }

        public void CleanProperty(int propertyNumber)
        {
            FilterModel.CleanProperty(propertyNumber);
        }
    }
}
