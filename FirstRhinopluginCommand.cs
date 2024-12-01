using Rhino;
using Rhino.Commands;
using customControls;
using AppKit;
using CoreGraphics;
using Foundation;
using Eto;
using Testing;
using System;

namespace RadialMenu
{
    public class RadialMenuCommand : Command
    {
        /// <summary>
        /// Main plugin form
        /// </summary>
        private TransparentForm _form;
        public RadialMenuCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
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

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            _form = _form == null ? new SectorRadialMenuForm(this.PlugIn) : _form;
            var m = Rhino.UI.MouseCursor.Location;
            var formSize = _form.Size;
            _form.Location = new Eto.Drawing.Point((int)m.X - (formSize.Width / 2), (int)m.Y - (formSize.Height / 2));
            _form.Show();
            
            
            //
            // Testing
            //
            // var form = new TestForm2();
            // form.Show();


            return Result.Nothing;
        }
    }
}