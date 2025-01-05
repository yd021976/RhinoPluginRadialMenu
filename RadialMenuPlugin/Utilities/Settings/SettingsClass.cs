using System.Collections.Generic;
using RadialMenuPlugin.Data;

namespace RadialMenuPlugin.Utilities.Settings
{
    /// <summary>
    /// Mapping between c# properties to Rhino settings XML file
    /// </summary>
    public class SettingsClass
    {
        public RadialButtonStateColors ButtonColors = new RadialButtonStateColors();
        public IDictionary<string, ButtonProperties> ButtonProperties = new Dictionary<string, ButtonProperties>();
    }
}