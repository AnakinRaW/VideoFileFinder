using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace VideoFileFinder
{
    public class DriveItemData : INotifyPropertyChanged
    {
        private bool _selected;
        private DriveInfo _driveInfo;

        public DriveInfo DriveInfo
        {
            get => _driveInfo;
            set
            {
                if (Equals(value, _driveInfo)) return;
                _driveInfo = value;
                OnPropertyChanged();
            }
        }

        public bool Selected
        {
            get => _selected;
            set
            {
                if (value == _selected) return;
                _selected = value;
                OnPropertyChanged();
            }
        }

        public DriveItemData(DriveInfo driveInfo, bool selected)
        {
            _driveInfo = driveInfo;
            _selected = selected;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}