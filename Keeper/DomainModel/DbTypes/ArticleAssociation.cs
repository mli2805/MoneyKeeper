using System;
using System.Windows.Media;
using Keeper.DomainModel.Enumes;

namespace Keeper.DomainModel.DbTypes
{
    [Serializable]
    public class ArticleAssociation : IComparable
    {
        public int Id { get; set; }
        public Account ExternalAccount { get; set; }
        public OperationType OperationType { get; set; }
        public Account AssociatedArticle { get; set; }
        public bool IsTwoWay { get; set; }

        public int CompareTo(object obj)
        {
            return String.Compare(ExternalAccount.Name, ((Account)obj).Name, StringComparison.Ordinal);
        }
        public Brush FontColor => OperationType.FontColor();
    }

}
