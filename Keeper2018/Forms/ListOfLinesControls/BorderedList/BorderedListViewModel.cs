namespace Keeper2018.BorderedList
{
    public class BorderedListViewModel
    {
        public ListOfLines List { get; set; }

        public BorderedListViewModel(int maxWidth = 50)
        {
            List = new ListOfLines(maxWidth);
        }

        public BorderedListViewModel(ListOfLines list)
        {
            List = list;
        }
    }
}
