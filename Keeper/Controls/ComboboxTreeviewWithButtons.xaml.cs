using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Keeper.DomainModel.WorkTypes;

namespace Keeper.Controls
{
    /// <summary>
    /// Interaction logic for ComboboxTreeviewWithButtons.xaml
    /// </summary>
    public partial class ComboboxTreeviewWithButtons : UserControl
    {

        /// <summary>
        /// свойство в котором вернется выбранный элемент дерева
        /// на вход подается первоначальновыбранный элемент
        /// </summary>
        public static readonly DependencyProperty MySelectedItemProperty =
                 DependencyProperty.Register("MySelectedItem", typeof(AccName),
                 typeof(ComboboxTreeviewWithButtons), new FrameworkPropertyMetadata(new AccName()));
        public AccName MySelectedItem
        {
            get { return (AccName)GetValue(MySelectedItemProperty); }
            set { SetValue(MySelectedItemProperty, value); }
        }

        /// <summary>
        /// свойство для задания источника для древовидного комбобокса
        /// список корней деревьев, элементы дерева должны поддерживать ITreeViewItemModel
        /// </summary>
        public static DependencyProperty RootsListForComboProperty =
            DependencyProperty.Register("RootsListForCombo", typeof (List<AccName>),
                typeof(ComboboxTreeviewWithButtons), new FrameworkPropertyMetadata(new List<AccName>()));

        public List<AccName> RootsListForCombo
        {
            get { return (List<AccName>) GetValue(RootsListForComboProperty); }
            set { SetValue(RootsListForComboProperty, value);}
        }

        /// <summary>
        /// dictionary соответствия надписи на кнопке - элементу в комбобоксе
        /// </summary>
        public static DependencyProperty ButtonsDictionaryProperty =
            DependencyProperty.Register("ButtonsDictionary", typeof(Dictionary<string, AccName>),
                typeof(ComboboxTreeviewWithButtons), new FrameworkPropertyMetadata(new Dictionary<string, AccName>()));
        public Dictionary<string, AccName> ButtonsDictionary
        {
            get { return (Dictionary<string, AccName>) GetValue(ButtonsDictionaryProperty);  }
            set { SetValue(ButtonsDictionaryProperty, value);}
        }

        public ComboboxTreeviewWithButtons()
        {
            InitializeComponent();
            Loaded += ControlLoaded;
        }

        private void ControlLoaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
