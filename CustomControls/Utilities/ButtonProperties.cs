using System.ComponentModel;

namespace customControls
{
    public class ButtonProperties : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _rhinoScript;
        public string rhinoScript
        {
            get => _rhinoScript; set
            {
                _rhinoScript = value; 
                OnPropertyChanged(nameof(rhinoScript));
            }
        }


        Eto.Drawing.Icon _icon;
        public Eto.Drawing.Icon icon { get => _icon; set { _icon = value; OnPropertyChanged(nameof(icon)); } }


        bool _isFolder = false;
        public bool isFolder { get => _isFolder; set { _isFolder = value; OnPropertyChanged(nameof(isFolder)); } }


        bool _isActive = true;
        public bool isActive { get => _isActive; set { _isActive = value; OnPropertyChanged(nameof(isActive)); } }


        public ButtonProperties() : base()
        {
            icon = null;
            isActive = false;
            rhinoScript = "";
        }
        public ButtonProperties(string script, Eto.Drawing.Icon icon) : this()
        {
            rhinoScript = script;
            this.icon = icon;
        }
        public ButtonProperties(string script, Eto.Drawing.Icon icon, bool isActive) : this(script, icon)
        {
            this.isActive = isActive;
        }
    }
}