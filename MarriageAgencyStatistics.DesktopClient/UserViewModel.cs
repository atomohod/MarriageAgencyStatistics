using System.ComponentModel;
using System.Runtime.CompilerServices;
using MarriageAgencyStatistics.DesktopClient.Annotations;

namespace MarriageAgencyStatistics.DesktopClient
{
    public class UserViewModel : INotifyPropertyChanged
    {
        private string _title;

        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}