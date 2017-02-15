using System.Collections.Generic;
using Keeper.DomainModel.Enumes;

namespace Keeper.Utils.DiagramDomainModel
{
    public class DiagramData
    {
        public string Caption;
        public List<DiagramSeries> Series;
        public DiagramMode Mode;
        public Every TimeInterval;
    }
}
