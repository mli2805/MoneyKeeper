﻿using System.Windows;
using System.Windows.Controls;
using Keeper.DomainModel.WorkTypes;

namespace Keeper.Controls.TagPickingControl
{
    /// <summary>
    /// Interaction logic for TagPicker.xaml
    /// </summary>
    public partial class TagPicker : UserControl
    {
        public TagPicker()
        {
            InitializeComponent();
        }

        private void DeleteTagOnClick(object sender, RoutedEventArgs e)
        {
            var tagPickerVm = (TagPickerVm)DataContext;
            var accName = (AccName)((Button)sender).Tag;

            tagPickerVm.Tags.Remove(accName);

        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var tagPickerVm = (TagPickerVm)DataContext;
            tagPickerVm.Tags.Add(tagPickerVm.TagSelectorVm.MyAccName);
        }
    }
}
