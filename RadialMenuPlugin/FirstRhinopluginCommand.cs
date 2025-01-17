using Rhino;
using Rhino.Commands;
using RadialMenuPlugin.Controls;
using Eto.Forms;
using Rhino.UI;
using System;
using RadialMenuPlugin.Data;

namespace RadialMenuPlugin
{
    public class RadialMenuCommand : Rhino.Commands.Command
    {
        /// <summary>
        /// Main plugin form
        /// </summary>
        protected TransparentForm _Form;
        protected int _PrevKeyPress = 0;
        public RadialMenuCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;

            RhinoApp.KeyboardEvent += (key) =>
            {
                var etoKey = RhinoKeyToEto.toEtoKey(key);
                if (etoKey != Keys.None)
                {
                    //TODO: Add hability to customize wich keyboard special key to give menu focus back
                    if (etoKey == Keys.Application) // If "command" key is pressed, give menu focus 
                    {
                        // FIXME: for special keys like Command, Control, Option, Rhino send the event twice.
                        // Check previous key press was not the same to avoid sending Menu the event twice
                        if (_PrevKeyPress != key)
                        {
                            _Form.KeyPress(new KeyEventArgs(etoKey, KeyEventType.KeyDown)); // No char as we only send special key
                        }
                    }
                }
                 _PrevKeyPress = key;
                // Console.SetOut(RhinoApp.CommandLineOut);
                // Console.WriteLine("Key code: " + key.ToString());
            };
            // When ESC is pressed in rhino AND command is running -> Send keyUp to radial menu
            RhinoApp.EscapeKeyPressed += (s, e) =>
            {
                if (_Form != null && _Form.Visible) // If radial menu exist and is showing
                {
                    _Form.KeyPress(new KeyEventArgs(Keys.Escape, KeyEventType.KeyDown)); // Send ESC key up event to form
                }
            };
        }

        ///<summary>The only instance of this command.</summary>
        public static RadialMenuCommand Instance { get; private set; }

        /// <summary>
        /// Name of the command in Rhino
        /// </summary>
        /// <returns>
        /// The command name as it appears on the Rhino command line.
        /// </returns>
        public override string EnglishName => "TigrouRadialMenu";

        /// <summary>
        /// Shows radial menu at mouse cursor position
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // RhinoApp.CommandWindowCaptureEnabled = false;
            _Form = _Form == null ? new RadialMenuForm(PlugIn) : _Form;
            var m = MouseCursor.Location;
            var formSize = _Form.Size;
            _Form.Location = new Eto.Drawing.Point((int)m.X - (formSize.Width / 2), (int)m.Y - (formSize.Height / 2));
            _Form.Show();
            return Result.Nothing;
        }
    }
}