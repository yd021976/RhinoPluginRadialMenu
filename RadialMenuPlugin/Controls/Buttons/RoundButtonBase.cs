using Eto.Drawing;
using Eto.Forms;

namespace RadialMenuPlugin.Controls.Buttons
{
    public class RoundButtonBase : Drawable
    {
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
        public delegate void ButtonClickEvent(RoundButtonBase sender, MouseEventArgs e);

        public RoundButtonBase() : base()
        {
            MouseDown += _OnMouseDown;
            MouseEnter += _OnMouseEnter;
            MouseLeave += _OnMouseLeave;
            MouseMove += _OnMouseMove;
            _BorderColor = RadialMenuPlugin.Instance.SettingsHelper.Settings.ButtonColors.Normal.Pen;
        }

        /**
         Draw the control
        **/
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var innerBrushColor = new SolidBrush(RadialMenuPlugin.Instance.SettingsHelper.Settings.ButtonColors.Normal.Fill);
            var borderBrushColor = new SolidBrush(_BorderColor);

            var outerCircleGp = _DrawSingleCircle(new Point(0, 0), Width);
            var innerCircleGp = _DrawSingleCircle(new Point(_PenSize / 2, _PenSize / 2), Width - _PenSize);
            e.Graphics.PixelOffsetMode = PixelOffsetMode.None;
            e.Graphics.FillPath(borderBrushColor, outerCircleGp);
            e.Graphics.FillPath(innerBrushColor, innerCircleGp);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        protected GraphicsPath _DrawSingleCircle(Point location, int radius)
        {
            var gp = new GraphicsPath();
            // Draw shape
            gp.AddEllipse(location.X, location.Y, radius, radius);
            return gp;
        }
        protected virtual void _OnMouseDown(object sender, MouseEventArgs e)
        {
            OnclickEvent?.Invoke(this, e); // Raise onclick event to be handled by delegate
        }
        protected virtual void _OnMouseEnter(object sender, MouseEventArgs e)
        {
            _BorderColor = RadialMenuPlugin.Instance.SettingsHelper.Settings.ButtonColors.Hover.Pen;
            Invalidate(false); // redraw button
        }
        protected virtual void _OnMouseLeave(object sender, MouseEventArgs e)
        {
            _BorderColor = RadialMenuPlugin.Instance.SettingsHelper.Settings.ButtonColors.Normal.Pen;
            Invalidate(false); // redraw button
        }
        protected virtual void _OnMouseMove(object sender, MouseEventArgs e)
        { }
    }
}