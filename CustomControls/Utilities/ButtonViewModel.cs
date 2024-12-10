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
            get => _properties;
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
            get
            {
                return _data;
            }
            set
            {
                _data.PropertyChanged -= onDataChanged;
                _data.properties.PropertyChanged -= onPropertiesChanged;
                _data = value;
                _data.PropertyChanged += onDataChanged;
                _data.properties.PropertyChanged += onPropertiesChanged;
                onDataPropertiesChanged(nameof(data)); // Notify property changed
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

        protected void onDataPropertiesChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Property changed event handler for properties in object "data"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onDataChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ButtonModelData.sectorData): break;
                case nameof(ButtonModelData.properties): // Update properties in persisent settings
                    SettingsHelper.Instance.setProperties(rhinoPersistentSettingsNode, this);
                    break;
                default: break;
            }
        }

        /// <summary>
        /// Property event handler for properties changed in object "data.properties"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onPropertiesChanged(object sender, PropertyChangedEventArgs e)
        {
            SettingsHelper.Instance.setProperties(rhinoPersistentSettingsNode, this);
        }

        public Model(string buttonID, Model parent = null) : base()
        {
            if (buttonID == "" || buttonID == null)
            {
                throw new System.Exception("Button ID cannot by empty or null");
            }
            // Init main object data
            Parent = parent;
            _data.buttonID = buttonID;
            // Load properties from rhino settings
            _data.properties = SettingsHelper.Instance.getProperties(rhinoPersistentSettingsNode);
            // Property changed event handler
            _data.PropertyChanged += onDataChanged;
            _data.properties.PropertyChanged += onPropertiesChanged;
        }
    }
}
