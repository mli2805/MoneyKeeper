using System.Collections.Generic;
using System.ComponentModel;

namespace Keeper.Controls.ComboboxTreeview
{
    public interface ITreeViewItemModel : INotifyPropertyChanged
    {
        string SelectedValuePath { get; }

        string DisplayValuePath { get; }

        bool IsExpanded { get; set; }

        bool IsSelected { get; set; }

        IEnumerable<ITreeViewItemModel> GetHierarchy();

        IEnumerable<ITreeViewItemModel> GetChildren();
    }
}