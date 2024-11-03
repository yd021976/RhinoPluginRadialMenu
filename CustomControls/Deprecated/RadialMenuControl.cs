using Eto.Forms;
using Eto.Drawing;
using System.Linq;
using System.Collections.Generic;

namespace customControls
{
    public class RadialMenuControl : Drawable
    {
        // close button click event
        public event btnClickEvent onCloseMenu;
        public delegate void btnClickEvent(RadialMenuControl sender);
        public delegate void buttonItemUpdatedEvent(RoundedButton sender, buttonInfoUpdatedEventArgs e);

        // Button infos is updated
        public event buttonItemUpdatedEvent onButtonInfoUpdated;

        // # of buttons
        public int buttonsNumber = 8;

        // Radius of menu
        public int menuRadius = 100;

        // inner layout
        protected PixelLayout layout = new PixelLayout();

        // Radial menu buttons
        private List<RoundedButton> buttons = new List<RoundedButton>();

        // The close menu button
        private RoundedButton closeBtn = new RoundedButton("CLOSE_MENU_BUTTON_00000000");

        public RadialMenuControl() : base()
        {
            this.Size = new Size(menuRadius, menuRadius);
            // Build buttons for radial menu
            for (int i = 1; i <= this.buttonsNumber; i++)
            {
                var btn = new RoundedButton(i.ToString());

                /// Raise event
                btn.onButtonInfoUpdated += (RoundedButton sender, buttonInfoUpdatedEventArgs e) =>
                {
                    this.onButtonInfoUpdated.Invoke(btn, e);
                };
                btn.onclickEvent += onCloseClick;
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
            var center_x = Parent.ClientSize.Width / 2;
            var center_y = Parent.ClientSize.Height / 2;

            // Draw radial menu buttons
            for (int i = 0; i < buttonsNumber; i++)
            {
                var angle = ((2 * System.Math.PI) / buttonsNumber) * (i + 1);
                var btn = this.buttons.ElementAt(i);
                var circle_center_x = (int)System.Math.Round(center_x + menuRadius * System.Math.Cos(angle));
                var circle_center_y = (int)System.Math.Round(center_y + menuRadius * System.Math.Sin(angle));
                if (layout.FindChild(btn.ID) != null)
                {
                    layout.Move(btn, circle_center_x - (btn.Width / 2), circle_center_y - (btn.Height / 2));
                }
                else
                {
                    layout.Add(btn, circle_center_x - (btn.Width / 2), circle_center_y - (btn.Height / 2));
                }
            }

            // Update the close button at the center of radial menu
            if (layout.FindChild(closeBtn.ID) == null)
            {
                layout.Add(closeBtn, center_x - (closeBtn.Width / 2), center_y - (closeBtn.Height / 2));
            }
            else
            {
                layout.Move(closeBtn, center_x - (closeBtn.Width / 2), center_y - (closeBtn.Height / 2));
            }
            Content = layout;
        }
        /**
            Raise "oncloseMenu" event when "close button" is clicked
        **/
        private void onCloseClick(object sender)
        {
            onCloseMenu?.Invoke(this);
        }

        /**
            Drop traget test
        **/
        private void onDragDrop(object sender, DragEventArgs e)
        {
        }
    }
}