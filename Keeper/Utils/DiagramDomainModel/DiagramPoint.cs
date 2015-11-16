using System;

namespace Keeper.Utils.DiagramDomainModel
{
    public class DiagramPoint
    {
        public DateTime CoorXdate;
        public double CoorYdouble;

        public DiagramPoint(DateTime coorXdate, double coorYdouble)
        {
            CoorXdate = coorXdate;
            CoorYdouble = coorYdouble;
        }
    }
}