using System;
using System.Windows;
using System.Windows.Controls;
using Keeper.DomainModel.Extentions;
using Keeper.DomainModel.WorkTypes;

namespace Keeper.Controls
{
    /// <summary>
    /// Interaction logic for ShellViewPeriodSelectControl.xaml
    /// </summary>
    public partial class ShellViewPeriodSelectControl
    {
        public static readonly DependencyProperty SelectedPeriodProperty =
               DependencyProperty.Register("SelectedPeriod", typeof(Period),
               typeof(ShellViewPeriodSelectControl), new FrameworkPropertyMetadata(new Period()));

        public Period SelectedPeriod
        {
            get { return (Period)GetValue(SelectedPeriodProperty); }
            set
            {
                SetValue(SelectedPeriodProperty, value);
                StartDatePicker.SelectedDate = value.Start;
                FinishDatePicker.SelectedDate = value.Finish;
            }
        }

        public static readonly DependencyProperty ChangeControlTypeEventProperty =
              DependencyProperty.Register("ChangeControlTypeEvent", typeof(bool),
              typeof(ShellViewPeriodSelectControl), new FrameworkPropertyMetadata(new bool()));

        public bool ChangeControlTypeEvent
        {
            get { return (bool)GetValue(ChangeControlTypeEventProperty); }
            set { SetValue(ChangeControlTypeEventProperty, value); }
        }

        public ShellViewPeriodSelectControl()
        {
            InitializeComponent();

            Loaded += ShellViewPeriodSelectControlLoaded;
        }

        void ShellViewPeriodSelectControlLoaded(object sender, RoutedEventArgs e)
        {
            SelectedPeriod = DateTime.Today.GetPassedPartOfMonthWithFullThisDate();
        }

        public void Connect(int connectionId, object target)
        {
            throw new NotImplementedException();
        }

        private void ChangeControlType(object sender, RoutedEventArgs e) { ChangeControlTypeEvent = !ChangeControlTypeEvent; }

        private void OneDayBeforeClick(object sender, RoutedEventArgs e) 
            { SelectedPeriod = new Period(SelectedPeriod.Start.AddDays(-1), SelectedPeriod.Finish.AddDays(-1)); }
        private void OneMonthBeforeClick(object sender, RoutedEventArgs e)
        {

            var finish = SelectedPeriod.Finish;
            finish = IsLastDayOfMonth(finish) ? finish.AddDays(-finish.Day) : finish.AddMonths(-1);
            SelectedPeriod = new Period(SelectedPeriod.Start.AddMonths(-1), finish);
        }

        private void OneYearBeforeClick(object sender, RoutedEventArgs e) 
            { SelectedPeriod = new Period(SelectedPeriod.Start.AddYears(-1), SelectedPeriod.Finish.AddYears(-1)); }
        private void OneDayAfterClick(object sender, RoutedEventArgs e) 
            { SelectedPeriod = new Period(SelectedPeriod.Start.AddDays(1), SelectedPeriod.Finish.AddDays(1)); }
        private void OneMonthAfterClick(object sender, RoutedEventArgs e)
        {
            DateTime finish;
            if (IsLastDayOfMonth(SelectedPeriod.Finish))
            {
                finish = SelectedPeriod.Finish.AddMonths(2);
                finish = finish.AddDays(-finish.Day);
            }
            else finish = SelectedPeriod.Finish.AddMonths(1);

            SelectedPeriod = new Period(SelectedPeriod.Start.AddMonths(1), finish);
        }
        private void OneYearAfterClick(object sender, RoutedEventArgs e) 
            { SelectedPeriod = new Period(SelectedPeriod.Start.AddYears(1), SelectedPeriod.Finish.AddYears(1)); }
        private void TodayClick(object sender, RoutedEventArgs e) 
            { SelectedPeriod = new Period(DateTime.Today, DateTime.Today.AddDays(1).AddSeconds(-1)); }
        private void FromVeryBeginClick(object sender, RoutedEventArgs e) 
            { SelectedPeriod = new Period(new DateTime(2001, 12, 31), DateTime.Today.AddDays(1).AddSeconds(-1)); }
        private void ThisMonthClick(object sender, RoutedEventArgs e) 
            { SelectedPeriod = new Period(DateTime.Today.AddDays(-DateTime.Today.Day + 1), DateTime.Today.AddDays(1).AddSeconds(-1)); }
        private void LastMonthClick(object sender, RoutedEventArgs e)
        {
            var finish = DateTime.Today.AddDays(-DateTime.Today.Day + 1).AddSeconds(-1);
            SelectedPeriod = new Period(finish.AddDays(-finish.Day + 1), finish);
        }
        private void ThisYearPaymentsClick(object sender, RoutedEventArgs e) 
            { SelectedPeriod = new Period(DateTime.Today.AddDays(-DateTime.Today.DayOfYear + 1), DateTime.Today); }
        private void LastYearPaymentsClick(object sender, RoutedEventArgs e)
        {
            SelectedPeriod = new Period(new DateTime(DateTime.Today.Year - 1, 1, 1), new DateTime(DateTime.Today.Year, 1, 1).AddSeconds(-1));
        }

        private bool IsLastDayOfMonth(DateTime date) { return date.Month != date.AddDays(1).Month; }

        private void StartDatePickerSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StartDatePicker.SelectedDate == null) StartDatePicker.SelectedDate = SelectedPeriod.Start;
            else
                SelectedPeriod = new Period(((DateTime)StartDatePicker.SelectedDate).GetStartOfDate(), SelectedPeriod.Finish);
        }

        public void FinishDatePickerSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FinishDatePicker.SelectedDate == null) FinishDatePicker.SelectedDate = SelectedPeriod.Finish;
            else
                SelectedPeriod = new Period(SelectedPeriod.Start, ((DateTime)FinishDatePicker.SelectedDate).GetEndOfDate());
        }

    }
}
