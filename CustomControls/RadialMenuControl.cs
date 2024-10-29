using Eto.Forms;
using Eto.Drawing;
using System;
using System.Linq;
using System.Collections.Generic;

namespace customControls
{
    public class RadialMenuControl : Drawable
    {
        // close button click event
        public event System.EventHandler onCloseMenu;
        public delegate void buttonItemUpdatedEvent(RoundedButton sender, buttonInfoUpdatedEventArgs e);

        // Button infos is updated
        public event buttonItemUpdatedEvent onButtonInfoUpdated;

        // # of buttons
        public int buttonsNumber = 8;

        // Radius of menu
        public int menuRadius = 100;

        // inner layout
        protected PixelLayout layout = new Eto.Forms.PixelLayout();

        // Radial menu buttons
        private List<customControls.RoundedButton> buttons = new List<customControls.RoundedButton>();

        // The close menu button
        private customControls.RoundedButton closeBtn = new customControls.RoundedButton("CLOSE_MENU_BUTTON_00000000");

        public RadialMenuControl() : base()
        {
            this.Size = new Size(this.menuRadius, this.menuRadius);
            // Build buttons for radial menu
            for (int i = 1; i <= this.buttonsNumber; i++)
            {
                var btn = new customControls.RoundedButton(i.ToString());
                
                /// Raise event
                btn.onButtonInfoUpdated += (RoundedButton sender, customControls.buttonInfoUpdatedEventArgs e) =>
                {
                    this.onButtonInfoUpdated.Invoke(btn, e);
                };

                /// Add to layout
                this.buttons.Add(btn);
            }
            this.closeBtn.onclickEvent += onCloseClick;
            // test for drag drop event
            this.DragDrop += this.onDragDrop;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            this.paintRadialMenu();
        }

        /**
            Draw the menu
        **/
        private void paintRadialMenu()
        {
            var center_x = this.Parent.ClientSize.Width / 2;
            var center_y = this.Parent.ClientSize.Height / 2;

            // Draw radial menu buttons
            for (int i = 0; i < this.buttonsNumber; i++)
            {
                var angle = (2 * System.Math.PI / this.buttonsNumber) * (i + 1);
                var btn = this.buttons.ElementAt(i);
                var circle_center_x = (int)System.Math.Round(center_x + this.menuRadius * System.Math.Cos(angle));
                var circle_center_y = (int)System.Math.Round(center_y + this.menuRadius * System.Math.Sin(angle));
                if (this.layout.FindChild(btn.ID) != null)
                {
                    this.layout.Move(btn, circle_center_x - (btn.Width / 2), circle_center_y - (btn.Height / 2));
                }
                else
                {
                    this.layout.Add(btn, circle_center_x - (btn.Width / 2), circle_center_y - (btn.Height / 2));
                }
            }

            // Update the close button at the center of radial menu
            if (this.layout.FindChild(this.closeBtn.ID) == null)
            {
                this.layout.Add(this.closeBtn, center_x - (closeBtn.Width / 2), center_y - (closeBtn.Height / 2));
            }
            else
            {
                this.layout.Move(this.closeBtn, center_x - (closeBtn.Width / 2), center_y - (closeBtn.Height / 2));
            }
            this.Content = this.layout;
        }
        /**
            Raise "oncloseMenu" event when "close button" is clicked
        **/
        private void onCloseClick(object sender, EventArgs e)
        {
            this.onCloseMenu?.Invoke(this, e);
        }

        /**
            Drop traget test
        **/
        private void onDragDrop(object sender, DragEventArgs e)
        {
        }
    }
}