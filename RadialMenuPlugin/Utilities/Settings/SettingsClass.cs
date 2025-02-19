using System.Collections.Generic;
using System.ComponentModel;
using RadialMenuPlugin.Controls.Base.ContextMenu;
using RadialMenuPlugin.Data;

namespace RadialMenuPlugin.Utilities.Settings
{
    /// <summary>
    /// Mapping between c# properties to Rhino settings XML file
    /// </summary>
    public class SettingsClass:BaseINotifyPropertyChanged
    {
        public RadialButtonStateColors ButtonColors = new RadialButtonStateColors();
        public bool DisplaySegmentedCircles = false;
    }
}