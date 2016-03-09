using System.Collections.Generic;
using System.Composition;
using Keeper.Controls.AccNameSelectionControl;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class MyAccNameSelectionControlInitializer
    {
        public AccNameSelectorVm ForExpense(string activeAccountName)
        {
            return new AccNameSelectorVm
            {
                ControlTitle = "Откуда",
                Buttons = new List<AccNameButtonVm>
                {
                    new AccNameButtonVm("мк",
                        ListsForComboTrees.MyAccNamesForExpense.FindThroughTheForest("Мой кошелек")),
                    new AccNameButtonVm("биб",
                        ListsForComboTrees.MyAccNamesForExpense.FindThroughTheForest("БИБ Сберка Моцная")),
                    new AccNameButtonVm("газ",
                        ListsForComboTrees.MyAccNamesForExpense.FindThroughTheForest("БГПБ Сберегательная")),
                    new AccNameButtonVm("юк",
                        ListsForComboTrees.MyAccNamesForExpense.FindThroughTheForest("Юлин кошелек"))
                },
                AccNamesListForExpense = ListsForComboTrees.MyAccNamesForExpense,
                MyAccName = ListsForComboTrees.MyAccNamesForExpense.FindThroughTheForest(activeAccountName)
                            ?? ListsForComboTrees.MyAccNamesForExpense.FindThroughTheForest("Мой кошелек")
            };
        }

    }
}
