using System.ComponentModel;

namespace MarriageAgencyStatistics.DesktopClient
{
    public class CheckedListItem<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isChecked;
        private T _item;

        public CheckedListItem()
        {
        }

        public CheckedListItem(T item, bool isChecked = false)
        {
            this._item = item;
            this._isChecked = isChecked;
        }

        public T Item
        {
            get => _item;
            set
            {
                _item = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
            }
        }


        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsChecked"));
            }
        }
    }
}