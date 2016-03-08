using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.DomainModel.WorkTypes;
using Keeper.ViewModels.TransWithTags;

namespace Keeper.Controls
{
    class TestControlVm : PropertyChangedBase
    {
        public List<ButtonViewModel> Buttons { get; set; }

        private AccName _myAccName;
        private List<AccName> _accNamesListForExpense;

        public AccName MyAccName
        {
            get { return _myAccName; }
            set
            {
                if (Equals(value, _myAccName)) return;
                _myAccName = value;
                NotifyOfPropertyChange();
            }
        }

        public List<AccName> AccNamesListForExpense
        {
            get { return _accNamesListForExpense; }
            set
            {
                if (Equals(value, _accNamesListForExpense)) return;
                _accNamesListForExpense = value;
                NotifyOfPropertyChange();
            }
        }

        public string ControlTitle { get; set; }

    }
}
