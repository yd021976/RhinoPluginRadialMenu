using System.ComponentModel;
using Rhino;

namespace customControls
{
    public class ButtonModelData : INotifyPropertyChanged
    {
        #region Notify interface implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion


        /// <summary>
        /// ID of the button associated to this sector data
        /// </summary>
        string _buttonID = "";
        public string buttonID
        {
            get => _buttonID;
            set
            {
                _buttonID = value;
                OnPropertyChanged(nameof(buttonID));
            }
        }

        SectorData _sectorData = new SectorData();
        public SectorData sectorData
        {
            get
            {
                return _sectorData;
            }
            set
            {
                _sectorData = value;
                OnPropertyChanged(nameof(sectorData));
            }
        }

        ButtonProperties _properties = new ButtonProperties();
        public ButtonProperties properties
        {
            get
            {
                // If a ButtonID is set, try to get button properties from Rhino settings and sets internal class property
                if (buttonID != "")
                {
                    if (SettingsHelper.Instance.settings.buttonProperties.TryGetValue(buttonID, out var rhinoSettingsProperties))
                    {
                        _properties = rhinoSettingsProperties;
                    }
                }
                return _properties;
            }
            set
            {
                _properties = value; // Update property object
                OnPropertyChanged(nameof(properties)); // Notify property changed
            }
        }
    }
    /// <summary>
    /// Model class for "arc buttons"
    /// </summary>
    public class Model : INotifyPropertyChanged
    {
        public Model Parent;
        ButtonModelData _data = new ButtonModelData();
        public ButtonModelData data
        {
            get => _data;
            set
            {
                _data.PropertyChanged -= onButtonPropertyChangedHandler;
                _data = value;
                _data.PropertyChanged += onButtonPropertyChangedHandler;
                OnPropertyChanged(nameof(data)); // Notify property changed
            }
        }
        protected PersistentSettings rhinoPersistentSettingsNode
        {
            get
            {
                if (_data.buttonID != "")
                {
                    return SettingsHelper.Instance.getChildNode(_data.buttonID, Parent?.rhinoPersistentSettingsNode ?? null);
                }
                else
                {
                    return null;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Property changed event handler for object type "ButtonProperties"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onButtonPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ButtonModelData.sectorData): break;
                case nameof(ButtonModelData.properties): break;
                default: break;
            }
            // updateRhinoSettings();
        }

        /// <summary>
        /// Update button properties into Rhino plugin settings
        /// </summary>
        private void updateRhinoSettings()
        {
            // Save button properties into Rhino plugin properties
            if (data.buttonID != "")
            {
                SettingsHelper.Instance.saveButtonProperties(data.buttonID, data.properties);
            }
        }
        public Model() : base()
        {
            Parent = null;
        }
        public Model(Model parent = null) : this()
        {
            Parent = parent;
        }
    }
}
