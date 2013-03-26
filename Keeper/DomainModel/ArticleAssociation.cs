using System;

namespace Keeper.DomainModel
{
  [Serializable]
  public class ArticleAssociation : IComparable
  {
    public int Id { get; set; }
    public Account ExternalAccount { get; set; }
    public OperationType OperationType { get; set; }
    public Account AssociatedArticle { get; set; }

    public string ToDumpWithNames()
    {
      return ExternalAccount + " ; " + OperationType + " ; " + AssociatedArticle;
    }

    public int CompareTo(object obj)
    {
      return String.Compare(ExternalAccount.Name, ((Account) obj).Name, StringComparison.Ordinal);
    }
  }

}
