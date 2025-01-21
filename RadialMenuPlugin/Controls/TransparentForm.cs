using Rhino.PlugIns;
using Eto.Forms;
using Eto.Drawing;
using AppKit;

namespace RadialMenuPlugin.Controls
{
    public class TransparentForm : Form
    {
        protected PlugIn _MainPlugin;

        public TransparentForm(PlugIn plugin) : base()
        {
            // Make the form background transparent
            Title = "Radial Menu";
            WindowStyle = WindowStyle.None;
            AutoSize = false;
            Resizable = false;
            // Topmost = true;
            ShowActivated = true;
            Padding = new Padding(0);
            MovableByWindowBackground = false;
            Styles.Add<TransparentForm>("Transparent", _TransparentStyle);
            Style = "Transparent";
            _MainPlugin = plugin;

            // Keyboard events
            KeyUp += (s, e) =>
            {
                switch (e.Key)
                {
                    case Keys.Escape:
                        _OnEscapePressed(s, e);
                        break;
                    default:
                        break;
                }
            };
        }
       
        /// <summary>
        /// Send a key down event to form
        /// </summary>
        /// <param name="e"></param>
        public void KeyPress(KeyEventArgs e)
        {
            OnKeyDown(e);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="form"></param>
        protected void _TransparentStyle(TransparentForm form)
        {
            BackgroundColor = Colors.Transparent; // Set ETO window background transparent

            var win = form.ControlObject;
            // var transparentNSColor = NSColor.Clear;
            var transparentNSColor = NSColor.FromRgba(0, 0, 0, 0);
            win.GetType().GetProperty("BackgroundColor").SetValue(win, transparentNSColor);

            // Remove window shadow to avoid animation artefacts
            var ctrlProp = form.Handler.GetType().GetProperty("Control");
            var nswindow = (NSWindow)ctrlProp.GetValue(Handler, null);
            nswindow.HasShadow = false;
        }

        /// <summary>
        /// Get focus when mouse move over the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void _OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!HasFocus)
            {
                Focus();
            }
        }

        /// <summary>
        /// Close the form when clicked
        /// </summary>
        /// <param name="sender"></param>
        protected virtual void _OnCloseClickEvent(object sender)
        {
            Visible = false;
        }
        /// <summary>
        /// Override this method to handle ESC keypress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void _OnEscapePressed(object sender, KeyEventArgs e) { }
    }
}