using Rhino.PlugIns;
using Eto.Forms;
using Eto.Drawing;

namespace customControls
{
    public class TransparentForm : Form
    {
        protected PlugIn mainPlugin;

        public TransparentForm(PlugIn plugin) : base()
        {
            // Make the form background transparent
            this.Title = "PIE Menu 0.2 in C#";
            this.WindowStyle = Eto.Forms.WindowStyle.None;
            this.AutoSize = false;
            this.Resizable = false;
            this.Topmost = true;
            this.ShowActivated = true;
            this.Padding = new Padding(0);
            this.MovableByWindowBackground = false;
            this.Styles.Add<TransparentForm>("Transparent", this.transparentStyle);
            this.Style = "Transparent";
            this.mainPlugin = plugin;
        }
        protected void transparentStyle(TransparentForm form)
        {
            this.BackgroundColor = Colors.Transparent;
            var win = form.ControlObject;
            var transparentNSColor = AppKit.NSColor.Clear;
            win.GetType().GetProperty("BackgroundColor").SetValue(win, transparentNSColor);
        }
        protected void onMouseMove(object sender, MouseEventArgs e)
        {
            if (!this.HasFocus)
            {
                this.Focus();
            }
        }
        protected void saveSettings()
        {
            this.mainPlugin.SaveSettings();
        }
        protected string saveIconImage(RoundedButton forButton, Icon icon)
        {
            var frame = icon.GetFrame(2);
            var pluginSettingsPath = this.mainPlugin.SettingsDirectoryAllUsers + "/Icons/" + forButton.ID;
            frame.Bitmap.Save(pluginSettingsPath, ImageFormat.Bitmap);
            return pluginSettingsPath;
        }

         /**
         close the form when clicked
        **/
        protected virtual void onCloseClickEvent(object sender)
        {
            this.Close();
        }
    }
}