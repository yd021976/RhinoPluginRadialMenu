using Rhino;
using Rhino.Commands;
using RadialMenuPlugin.Controls;
using Eto.Forms;

namespace RadialMenuPlugin
{
    public class RadialMenuCommand : Rhino.Commands.Command
    {
        /// <summary>
        /// Main plugin form
        /// </summary>
        protected TransparentForm _Form;
        public RadialMenuCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;

            // When ESC is pressed in rhino AND command is running -> Send keyUp to radial menu
            RhinoApp.EscapeKeyPressed += (s, e) =>
            {
                if (_Form != null && _Form.Visible) // If radial menu exist and is showing
                {
                    _Form.KeyPress(new KeyEventArgs(Keys.Escape, KeyEventType.KeyUp)); // Send ESC key up event to form
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
            _Form = _Form == null ? new RadialMenuForm(PlugIn) : _Form;
            var m = Rhino.UI.MouseCursor.Location;
            var formSize = _Form.Size;
            _Form.Location = new Eto.Drawing.Point((int)m.X - (formSize.Width / 2), (int)m.Y - (formSize.Height / 2));
            _Form.Show();
            return Result.Nothing;
        }
    }
}