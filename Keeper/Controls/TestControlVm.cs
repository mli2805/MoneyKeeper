using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Keeper.Controls
{
    class TestControlVm : PropertyChangedBase
    {
        private string _textProperty = "start";

        public string TextProperty
        {
            get { return _textProperty; }
            set
            {
                if (value == _textProperty) return;
                _textProperty = value;
                NotifyOfPropertyChange();
            }
        }

        public void PressButton()
        {
            TextProperty = TextProperty + "ButtonPressed";
        }

    }
}
