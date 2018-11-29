using System;
using System.Windows;
using System.Windows.Controls;

namespace Keeper2018
{
    /// <summary>
    /// Interaction logic for ShellViewPeriodSelectControl.xaml
    /// </summary>
    public partial class PeriodSelectControl
    {
        public static readonly DependencyProperty SelectedPeriodProperty =
            DependencyProperty.Register("SelectedPeriod", typeof(Period),
                typeof(PeriodSelectControl), new FrameworkPropertyMetadata(new Period()));

        public Period SelectedPeriod
        {
            get { return (Period) GetValue(SelectedPeriodProperty); }
            set
            {
                SetValue(SelectedPeriodProperty, value);
                StartDatePicker.SelectedDate = value.StartDate;
                FinishDatePicker.SelectedDate = value.FinishMoment;
            }
        }

        public static readonly DependencyProperty ChangeControlTypeEventProperty =
            DependencyProperty.Register("ChangeControlTypeEvent", typeof(bool),
                typeof(PeriodSelectControl), new FrameworkPropertyMetadata(new bool()));

        public bool ChangeControlTypeEvent
        {
            get { return (bool) GetValue(ChangeControlTypeEventProperty); }
            set { SetValue(ChangeControlTypeEventProperty, value); }
        }

        public PeriodSelectControl()
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

        private void ChangeControlType(object sender, RoutedEventArgs e)
        {
            ChangeControlTypeEvent = !ChangeControlTypeEvent;
        }

        private void OneDayBeforeClick(object sender, RoutedEventArgs e)
        {
            SelectedPeriod = new Period(SelectedPeriod.StartDate.AddDays(-1), SelectedPeriod.FinishMoment.AddDays(-1));
        }

        private void OneMonthBeforeClick(object sender, RoutedEventArgs e)
        {
            var finish = SelectedPeriod.FinishMoment;
            finish = IsLastDayOfMonth(finish) ? finish.AddDays(-finish.Day) : finish.AddMonths(-1);
            SelectedPeriod = new Period(SelectedPeriod.StartDate.AddMonths(-1), finish);
        }

        private void OneYearBeforeClick(object sender, RoutedEventArgs e)
        {
            SelectedPeriod = new Period(SelectedPeriod.StartDate.AddYears(-1), SelectedPeriod.FinishMoment.AddYears(-1));
        }

        private void OneDayAfterClick(object sender, RoutedEventArgs e)
        {
            SelectedPeriod = new Period(SelectedPeriod.StartDate.AddDays(1), SelectedPeriod.FinishMoment.AddDays(1));
        }

        private void OneMonthAfterClick(object sender, RoutedEventArgs e)
        {
            DateTime finish;
            if (IsLastDayOfMonth(SelectedPeriod.FinishMoment))
            {
                finish = SelectedPeriod.FinishMoment.AddMonths(2);
                finish = finish.AddDays(-finish.Day);
            }
            else finish = SelectedPeriod.FinishMoment.AddMonths(1);

            SelectedPeriod = new Period(SelectedPeriod.StartDate.AddMonths(1), finish);
        }

        private void OneYearAfterClick(object sender, RoutedEventArgs e)
        {
            SelectedPeriod = new Period(SelectedPeriod.StartDate.AddYears(1), SelectedPeriod.FinishMoment.AddYears(1));
        }

        private void TodayClick(object sender, RoutedEventArgs e)
        {
            SelectedPeriod = new Period(DateTime.Today, DateTime.Today.AddDays(1).AddSeconds(-1));
        }

        private void FromVeryBeginingClick(object sender, RoutedEventArgs e)
        {
            SelectedPeriod = new Period(new DateTime(2001, 12, 31), DateTime.Today.AddDays(1).AddSeconds(-1));
        }

        private void ThisMonthClick(object sender, RoutedEventArgs e)
        {
            SelectedPeriod = new Period(DateTime.Today.AddDays(-DateTime.Today.Day + 1),
                DateTime.Today.AddDays(1).AddSeconds(-1));
        }

        private void LastMonthClick(object sender, RoutedEventArgs e)
        {
            var finish = DateTime.Today.AddDays(-DateTime.Today.Day + 1).AddSeconds(-1);
            SelectedPeriod = new Period(finish.AddDays(-finish.Day + 1), finish);
        }

        private void ThisYearPaymentsClick(object sender, RoutedEventArgs e)
        {
            SelectedPeriod = new Period(DateTime.Today.AddDays(-DateTime.Today.DayOfYear + 1), DateTime.Today);
        }

        private void LastYearPaymentsClick(object sender, RoutedEventArgs e)
        {
            SelectedPeriod = new Period(new DateTime(DateTime.Today.Year - 1, 1, 1),
                new DateTime(DateTime.Today.Year, 1, 1).AddSeconds(-1));
        }

        private bool IsLastDayOfMonth(DateTime date)
        {
            return date.Month != date.AddDays(1).Month;
        }

        private void StartDatePickerSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StartDatePicker.SelectedDate == null) StartDatePicker.SelectedDate = SelectedPeriod.StartDate;
            else
                SelectedPeriod = new Period(((DateTime) StartDatePicker.SelectedDate).GetStartOfDate(),
                    SelectedPeriod.FinishMoment);
        }

        public void FinishDatePickerSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FinishDatePicker.SelectedDate == null) FinishDatePicker.SelectedDate = SelectedPeriod.FinishMoment;
            else
                SelectedPeriod = new Period(SelectedPeriod.StartDate,
                    ((DateTime) FinishDatePicker.SelectedDate).GetEndOfDate());
        }
    }
}