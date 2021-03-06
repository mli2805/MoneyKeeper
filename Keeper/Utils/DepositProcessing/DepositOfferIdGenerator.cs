﻿using System.Composition;
using System.Linq;
using Keeper.DomainModel.DbTypes;

namespace Keeper.Utils.DepositProcessing
{
    [Export]
    public sealed class DepositOfferIdGenerator
    {
        private readonly KeeperDb _db;

        [ImportingConstructor]
        public DepositOfferIdGenerator(KeeperDb db)
        {
            _db = db;
        }

        public int GenerateNewBankDepositOfferId()
        {
            if (_db.BankDepositOffers.Count == 0) return 1;
            return (from offer in _db.BankDepositOffers
                select offer.Id).Max() + 1;
        }
    }
}