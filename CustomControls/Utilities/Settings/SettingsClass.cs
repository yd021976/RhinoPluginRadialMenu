using System.Collections.Generic;

namespace customControls
{
    /// <summary>
    /// Mapping between c# properties to Rhino settings XML file
    /// </summary>
    public class SettingsClass
    {
        public RadialButtonStateColors buttonColors = new RadialButtonStateColors();
        public IDictionary<string, ButtonProperties> buttonProperties = new Dictionary<string, ButtonProperties>();
    }
}