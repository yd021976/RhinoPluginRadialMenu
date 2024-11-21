using System.ComponentModel;

namespace customControls
{
    public partial class SectorArcButton : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }


        private bool _isDraggingIcon = false;
        private bool _isHovering = false;
        private bool _isSelected = false;

        public bool isHovering
        {
            get { return _isHovering; }
            set
            {
                // Change class property value and notify change 
                _isHovering = value;
                OnPropertyChanged(nameof(isHovering));
            }
        }


        public bool isSelected
        {
            get { return _isSelected; }
            set
            {
                // Change class property value and notify change 
                _isSelected = value;
                OnPropertyChanged(nameof(isSelected));
            }
        }

        public bool isDraggingIcon
        {
            get => _isDraggingIcon;
            set
            {
                _isDraggingIcon = value;
                OnPropertyChanged(nameof(isDraggingIcon));
            }
        }
    }
}