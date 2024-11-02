using Rhino;
using Rhino.Commands;
using customControls;

namespace RadialMenu
{
    public class RadialMenuCommand : Command
    {
        public RadialMenuCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static RadialMenuCommand Instance { get; private set; }

        // The main form

        protected SectorRadialMenuForm form = null;


        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// The command name as it appears on the Rhino command line.
        /// </returns>
        public override string EnglishName => "TigrouRadialMenu";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            form = new SectorRadialMenuForm(this.PlugIn);
            var m = Rhino.UI.MouseCursor.Location;
            var formSize = form.Size;
            form.Location = new Eto.Drawing.Point((int)m.X - (formSize.Width / 2), (int)m.Y - (formSize.Height / 2));
            form.Show();
            return Result.Nothing;
        }
    }
}