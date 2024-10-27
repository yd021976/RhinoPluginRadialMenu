using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Input;
using Rhino.Input.Custom;
using Eto.Forms;
using System.Reflection;


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
            this.myForm();
            this.form.Show();
            return Result.Success;
        }
        protected void myForm()
        {
            this.form = new TransparentForm();
        }
    }

    public class TransparentForm : Eto.Forms.Form
    {
        public TransparentForm()
        {
            // Original code for Radial pie menu

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
            this.Size = new Eto.Drawing.Size(500,500);
            this.Content = radialMenu;
            radialMenu.onCloseMenu += this.BtnClick;
            // btn.onclick += this.BtnClick;
            this.DragDrop += this.testDD;
            this.DragEnter += this.testDD;
            this.DragLeave+= this.testDD;
            this.DragOver+= this.testDD;

        }
        /**
         close the form when clicked
        **/
        protected void BtnClick(object sender, EventArgs e)
        {
            this.Close();
        }

        protected void testDD(object sender, DragEventArgs e) {
            var a = 0;
        }
    }
}
