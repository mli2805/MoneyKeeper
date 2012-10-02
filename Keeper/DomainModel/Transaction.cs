using System;

namespace Keeper.DomainModel
{
  class Transaction
  {
    private DateTime _timestamp;
    private OperationType _operationType;
    private decimal _amount;
    private CurrencyCodes _currency;
    private Account _account;
    private Account _article;
    private String _comment;
  }
}