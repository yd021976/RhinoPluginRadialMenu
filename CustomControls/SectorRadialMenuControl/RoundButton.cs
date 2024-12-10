using Eto.Drawing;
using Eto.Forms;
using RadialMenu;

namespace customControls
{
    public class RoundButton : Drawable
    {
        private Icon icon;
        private int pen_size = 2;
        private Color borderColor;

        /// <summary>
        /// Button click event
        /// </summary>
        public event buttonClickEvent onclickEvent;
        /// <summary>
        /// Event for button click
        /// </summary>
        /// <param name="sender"></param>
        public delegate void buttonClickEvent(RoundButton sender);

        public RoundButton() : base()
        {
            MouseDown += onMouseDown;
            MouseEnter += onMouseEnter;
            MouseLeave += onMouseLeave;
            borderColor = RadialMenuPlugin.Instance.settingsHelper.settings.buttonColors.normal.pen;
        }

        public void setButtonIcon(Icon icon)
        {
            this.icon = icon;
            Invalidate();
        }
        /**
         Draw the control
        **/
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var innerBrushColor = new SolidBrush(RadialMenuPlugin.Instance.settingsHelper.settings.buttonColors.normal.fill);
            var borderBrushColor = new SolidBrush(borderColor);
            var borderSize = new Rectangle(0, 0, Width, Height);
            var innerSize = new Rectangle(pen_size, pen_size, Width - (pen_size * 2), Height - (pen_size * 2));

            // Draw the icon shape
            e.Graphics.FillEllipse(borderBrushColor, borderSize);
            e.Graphics.FillEllipse(innerBrushColor, innerSize);

            if (icon != null)
            {
                e.Graphics.DrawImage(icon, Width / 2 - icon.Width / 2, Height / 2 - icon.Height / 2);
            }
        }

        protected void onMouseDown(object sender, MouseEventArgs e)
        {
            onclickEvent?.Invoke(this); // Raise onclick event to be handled by delegate
        }
        protected void onMouseEnter(object sender, MouseEventArgs e)
        {

            borderColor = RadialMenuPlugin.Instance.settingsHelper.settings.buttonColors.hover.pen;
            Invalidate(false); // redraw button
        }
        protected void onMouseLeave(object sender, MouseEventArgs e)
        {
            borderColor = RadialMenuPlugin.Instance.settingsHelper.settings.buttonColors.normal.pen;
            Invalidate(false); // redraw button
        }
    }
}