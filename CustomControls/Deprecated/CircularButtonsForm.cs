using System.Collections.Generic;

namespace customControls
{
    public class CircularButtonsForm : TransparentForm
    {
        public CircularButtonsForm(Rhino.PlugIns.PlugIn plugin) : base(plugin)
        {
            // Create radial menu with rounded buttons
            var radialMenu = new customControls.RadialMenuControl();
            radialMenu.onButtonInfoUpdated += this.onRadialMenuItemUpdated;
            this.Size = new Eto.Drawing.Size(500, 500);
            this.Content = radialMenu;
            radialMenu.onCloseMenu += this.onCloseClickEvent;
            this.MouseMove += this.onMouseMove;
        }
       
        protected void onRadialMenuItemUpdated(RoundedButton sender, buttonInfoUpdatedEventArgs e)
        {
            var settings = this.mainPlugin.Settings;
            Rhino.PersistentSettings item;
            try
            {
                item = settings.GetChild(sender.ID);
            }
            catch (KeyNotFoundException)
            {
                item = settings.AddChild(sender.ID);
            }

            item.SetString("script", e.item.script);
            var iconPath = this.saveIconImage(sender, e.item.icon);
            item.SetString("icon_name", e.item.icon.ID);
            this.saveSettings();
        }
    }
}