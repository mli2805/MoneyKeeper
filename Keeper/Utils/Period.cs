using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keeper.Utils
{
    class Period
    {
        private DateTime _start;
        private DateTime _finish;

        public Period(DateTime start, DateTime finish)
        {
            _start = start;
            _finish = finish;
        }

        public bool IsDateIn(DateTime checkDate)
        {
            if (checkDate.Date >= _start.Date && checkDate.Date <= _finish.Date) return true;
            else return false;
        }
    }
}
