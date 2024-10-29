using System;
using Eto.Drawing;
using Rhino.PlugIns;
using System.Collections.Generic;
using System.Reflection;

namespace customControls
{
    public class TransparentForm : Eto.Forms.Form
    {
        protected PlugIn mainPlugin;

        public TransparentForm(Rhino.PlugIns.PlugIn plugin) : base()
        {
            ///
            /// Original code for Radial pie menu
            ///
            /* this.Title = "PIE Menu 0.1";
            this.WindowStyle = Eto.Forms.WindowStyle.None;
            this.AutoSize = false;
            this.Resizable = false;
            this.Topmost = true;
            this.ShowActivated = false;
            this.Size = new Eto.Drawing.Size(300, 300);
            this.Padding = new Eto.Drawing.Padding(20);
            this.MovableByWindowBackground = false;
             */


            // Settings to test a form
            this.Title = "PIE Menu 0.1 in C#";
            this.WindowStyle = Eto.Forms.WindowStyle.None;
            this.AutoSize = false;
            this.Resizable = true;
            this.Topmost = true;
            this.ShowActivated = true;
            this.Padding = new Eto.Drawing.Padding(0);
            this.MovableByWindowBackground = true;
            this.Styles.Add<customControls.TransparentForm>("Transparent", this.transparentStyle);
            this.Style = "Transparent";
            this.mainPlugin = plugin;

            // var layout = new Eto.Forms.PixelLayout();

            //
            /* 
            var btn = new Eto.Forms.Button();
            btn.Size = new Eto.Drawing.Size(50, 50);
             */

            //
            // var btn = new customControls.RoundedButton();

            // construct layout
            // layout.Add(btn, 10, 10);
            var radialMenu = new customControls.RadialMenuControl();
            radialMenu.onButtonInfoUpdated += this.onRadialMenuItemUpdated;

            this.Size = new Eto.Drawing.Size(500, 500);
            this.Content = radialMenu;
            radialMenu.onCloseMenu += this.closeBtnClick;
        }
        /**
         close the form when clicked
        **/
        protected void closeBtnClick(object sender, EventArgs e)
        {
            this.Close();
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
        protected void saveSettings()
        {
            this.mainPlugin.SaveSettings();
        }
        private string saveIconImage(RoundedButton forButton, Icon icon)
        {
            var frame = icon.GetFrame(2);
            var pluginSettingsPath = this.mainPlugin.SettingsDirectoryAllUsers + "/Icons/" + forButton.ID;
            frame.Bitmap.Save(pluginSettingsPath, ImageFormat.Bitmap);
            return pluginSettingsPath;
        }
        protected void transparentStyle(customControls.TransparentForm form)
        {
            this.BackgroundColor = Eto.Drawing.Colors.Transparent;
            var win = form.ControlObject;
            var transparentNSColor = AppKit.NSColor.Clear;
            win.GetType().GetProperty("BackgroundColor").SetValue(win, transparentNSColor);
        }
    }
}