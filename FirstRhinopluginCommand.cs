using Rhino;
using Rhino.Commands;
using Eto.Forms;
using customControls;

namespace FirstRhinoplugin
{
    public class FirstRhinopluginCommand : Rhino.Commands.Command
    {
        public FirstRhinopluginCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static FirstRhinopluginCommand Instance { get; private set; }

        // The main form

        protected TransparentForm form = null;

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "FirstRhinopluginCommand";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            this.form = new TransparentForm();
            this.form.Show();
            return Result.Nothing;
        }
    }
}