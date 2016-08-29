using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Trans;

namespace Keeper.ViewModels.TransWithTags
{
    public class TransModel : PropertyChangedBase
    {
        public KeeperDb Db { get; set; }

        private ObservableCollection<TranWrappedForDatagrid> _rows;
        public ObservableCollection<TranWrappedForDatagrid> Rows
        {
            get { return _rows; }
            set
            {
                if (Equals(value, _rows)) return;
                _rows = value;
                NotifyOfPropertyChange();
            }
        }

        public ICollectionView SortedRows { get; set; }

        private TranWrappedForDatagrid _selectedTranWrappedForDatagrid;
        public TranWrappedForDatagrid SelectedTranWrappedForDatagrid
        {
            get { return _selectedTranWrappedForDatagrid; }
            set
            {
                if (Equals(value, _selectedTranWrappedForDatagrid)) return;
                _selectedTranWrappedForDatagrid = value;
                NotifyOfPropertyChange();
            }
        }
        public int SelectedTranIndex { get; set; }

        public DateTime AskedDate { get; set; } = DateTime.Now;
    }
}
