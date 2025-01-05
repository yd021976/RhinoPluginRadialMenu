using System.ComponentModel;

namespace RadialMenuPlugin.Data
{
    public class ButtonProperties : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _RhinoScript;
        public string RhinoScript
        {
            get => _RhinoScript; set
            {
                _RhinoScript = value;
                OnPropertyChanged(nameof(RhinoScript));
            }
        }


        protected Eto.Drawing.Icon _Icon;
        public Eto.Drawing.Icon Icon { get => _Icon; set { _Icon = value; OnPropertyChanged(nameof(Icon)); } }


        bool _IsFolder = false;
        public bool IsFolder { get => _IsFolder; set { _IsFolder = value; OnPropertyChanged(nameof(IsFolder)); } }


        bool _IsActive = true;
        public bool IsActive { get => _IsActive; set { _IsActive = value; OnPropertyChanged(nameof(IsActive)); } }


        public ButtonProperties() : base()
        {
            Icon = null;
            IsActive = false;
            RhinoScript = "";
        }
        public ButtonProperties(string script, Eto.Drawing.Icon icon) : this()
        {
            RhinoScript = script;
            Icon = icon;
        }
        public ButtonProperties(string script, Eto.Drawing.Icon icon, bool isActive) : this(script, icon)
        {
            IsActive = isActive;
        }
        public ButtonProperties(string script, Eto.Drawing.Icon icon, bool isActive, bool isFolder) : this(script, icon, isActive)
        {
            IsFolder = isFolder;
        }
    }
}