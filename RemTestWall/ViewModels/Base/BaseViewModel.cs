using PropertyChanged;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RemTestWall.ViewModels.Base
{
    [AddINotifyPropertyChangedInterface]
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}