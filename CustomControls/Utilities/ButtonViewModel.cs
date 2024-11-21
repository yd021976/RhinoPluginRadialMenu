using System.ComponentModel;

namespace customControls
{
    /// <summary>
    /// Model class for "arc buttons"
    /// </summary>
    public class ButtonModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        /// <summary>
        /// ID of the button associated to this sector data
        /// </summary>
        public string buttonID = "";

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
                _properties.PropertyChanged += onButtonPropertyChangedHandler; // Add property changed handler
                updateRhinoSettings(); // Update button properties in rhino plugin settings
                OnPropertyChanged(nameof(properties)); // Notify property changed
            }
        }
        /// <summary>
        /// Property changed event handler for object type "ButtonProperties"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onButtonPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            updateRhinoSettings();
        }

        /// <summary>
        /// Update button properties into Rhino plugin settings
        /// </summary>
        private void updateRhinoSettings()
        {
            // Save button properties into Rhino plugin properties
            if (buttonID != "")
            {
                SettingsHelper.Instance.saveButtonProperties(buttonID, _properties);
            }
        }
    }
}
