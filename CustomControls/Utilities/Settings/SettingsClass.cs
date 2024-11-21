using System.Collections.Generic;

namespace customControls
{
    public class SettingsClass
    {
        public RadialButtonStateColors buttonColors = new RadialButtonStateColors();
        public IDictionary<string, ButtonProperties> buttonProperties = new Dictionary<string, ButtonProperties>();
        // TODO: Add sectors number property for radial menu to permit check/erase old icons 
    }
}