using Eto.Drawing;
using Eto.Forms;

namespace RadialMenuPlugin.Controls.Buttons
{
    public class RoundButton : Drawable
    {
        protected Icon _Icon;
        protected int _PenSize = 2;
        protected Color _BorderColor;

        /// <summary>
        /// Button click event
        /// </summary>
        public event ButtonClickEvent OnclickEvent;
        /// <summary>
        /// Event for button click
        /// </summary>
        /// <param name="sender"></param>
        public delegate void ButtonClickEvent(RoundButton sender, MouseEventArgs e);

        public RoundButton() : base()
        {
            MouseDown += _OnMouseDown;
            MouseEnter += _OnMouseEnter;
            MouseLeave += _OnMouseLeave;
            _BorderColor = RadialMenuPlugin.Instance.SettingsHelper.Settings.ButtonColors.Normal.Pen;
        }

        public void SetButtonIcon(Icon icon)
        {
            _Icon = icon;
            Invalidate();
        }
        /**
         Draw the control
        **/
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var innerBrushColor = new SolidBrush(RadialMenuPlugin.Instance.SettingsHelper.Settings.ButtonColors.Normal.Fill);
            var borderBrushColor = new SolidBrush(_BorderColor);
            var borderSize = new Rectangle(0, 0, Width, Height);
            var innerSize = new Rectangle(_PenSize, _PenSize, Width - (_PenSize * 2), Height - (_PenSize * 2));

            // Draw the icon shape
            e.Graphics.FillEllipse(borderBrushColor, borderSize);
            e.Graphics.FillEllipse(innerBrushColor, innerSize);

            if (_Icon != null)
            {
                e.Graphics.DrawImage(_Icon, Width / 2 - _Icon.Width / 2, Height / 2 - _Icon.Height / 2);
            }
        }

        protected void _OnMouseDown(object sender, MouseEventArgs e)
        {
            OnclickEvent?.Invoke(this, e); // Raise onclick event to be handled by delegate
        }
        protected void _OnMouseEnter(object sender, MouseEventArgs e)
        {

            _BorderColor = RadialMenuPlugin.Instance.SettingsHelper.Settings.ButtonColors.Hover.Pen;
            Invalidate(false); // redraw button
        }
        protected void _OnMouseLeave(object sender, MouseEventArgs e)
        {
            _BorderColor = RadialMenuPlugin.Instance.SettingsHelper.Settings.ButtonColors.Normal.Pen;
            Invalidate(false); // redraw button
        }
    }
}