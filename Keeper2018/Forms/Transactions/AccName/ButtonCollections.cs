using System.Collections.Generic;

namespace Keeper2018
{
    public static class ButtonCollections
    {
        public static readonly Dictionary<string, int> ForExpense =
            new Dictionary<string, int>
            {
                ["мк"] = 162,
                ["юк"] = 163,
                ["шка"] = 167,
                ["ярк"] = 947,
                ["юяр"] = 946,
                ["джо"] = 781,
                ["123"] = 884,
                ["шоп"] = 878,
            };

        public static readonly Dictionary<string, int> ForExpenseTags =
            new Dictionary<string, int>
            {
                ["pro"] = 249,
                ["евр"] = 523,
                ["вит"] = 744,
                ["гпп"] = 824,
                ["ома"] = 291,
                ["маг"] = 252,
                ["еда"] = 257,
                ["лек"] = 199,
                ["оде"] = 197,
                ["др"] = 256,
            };

        public static readonly Dictionary<string, int> ForReceiptTags =
            new Dictionary<string, int> { ["еда"] = 257, ["оде"] = 197, ["c/x"] = 446, ["др"] = 256, ["др-д"] = 362, };

        public static readonly Dictionary<string, int> ForIncome =
            new Dictionary<string, int>
            {
                ["алф"] = 695,
                ["шкф"] = 167,
                ["юк"] = 163,
                ["ярк"] = 947,
                ["юяр"] = 946,
                ["джо"] = 781,
                ["каш"] = 776,
                ["шоп"] = 878,
            };

        public static readonly Dictionary<string, int> ForIncomeTags =
            new Dictionary<string, int> { ["иит"] = 443, ["биб"] = 339, ["газ"] = 401, ["%%"] = 208, ["бэк"] = 701 };

        public static readonly Dictionary<string, int> ButtonsForTransfer =
            new Dictionary<string, int>
            {
                ["мк"] = 162,
                ["юк"] = 163,
                ["джо"] = 781,
                ["алф"] = 695,
                ["шкф"] = 167,
                ["ярк"] = 947,
                ["юяр"] = 946,
                ["123"] = 884,
                ["шоп"] = 878,
            };

        public static readonly Dictionary<string, int> ForTransferTags = new Dictionary<string, int>();

        public static readonly Dictionary<string, int> ForExchange =
            new Dictionary<string, int>
            {
                ["мк"] = 162,
                ["джо"] = 781,
                ["биб$"] = 864,
                ["бнб"] = 841,
                ["бнб$"] = 842,
                ["ярк"] = 947,
                ["при$"] = 948,
            };

        public static readonly Dictionary<string, int> ForExchangeTags = new Dictionary<string, int>();

        public static readonly Dictionary<string, int> IncomesForExternal =
            new Dictionary<string, int> { ["з.п"] = 204, ["юфр"] = 314, ["%%"] = 208, ["бэк"] = 701 };

        public static readonly Dictionary<string, int> ExpensesForExternal =
            new Dictionary<string, int>
            {
                ["еда"] = 257, 
                ["обе"] = 193,
                ["с/х"] = 446,
                ["дпр"] = 362,
                ["лек"] = 199,
                ["леч"] = 354,
                ["гад"] = 751,
                ["др"] = 256,
            };

        public static readonly Dictionary<string, int> ExternalForIncome =
            new Dictionary<string, int> { ["фсзн"] = 177, ["род"] = 225, };

        public static readonly Dictionary<string, int> ExternalForExpense =
            new Dictionary<string, int> { ["нал"] = 520, ["рикз"] = 668, ["род"] = 225, };

        public static readonly Dictionary<string, int> InvestAccounts = 
            new Dictionary<string, int> { ["usd"] = 829, ["rub"] = 892, ["byn"] = 695, ["alb"] = 696, };
    }
}