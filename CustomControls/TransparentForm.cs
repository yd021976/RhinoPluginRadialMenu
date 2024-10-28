using System;
using Eto.Forms;

namespace customControls
{
    public class TransparentForm : Eto.Forms.Form
    {
        public TransparentForm()
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
            this.Size = new Eto.Drawing.Size(500, 500);
            this.Content = radialMenu;
            radialMenu.onCloseMenu += this.BtnClick;
        }
        /**
         close the form when clicked
        **/
        protected void BtnClick(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}