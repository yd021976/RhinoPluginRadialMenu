using Eto.Drawing;
using Eto.Forms;

namespace RadialMenuPlugin.Controls.Buttons
{
    public class RoundIconButton : RoundButtonBase
    {
        protected Icon _Icon;

        public RoundIconButton() : base()
        { }

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
        }
        protected void _DrawIcon(Graphics g)
        {
            if (_Icon != null)
            {
                g.DrawImage(_Icon, Width / 2 - _Icon.Width / 2, Height / 2 - _Icon.Height / 2);
            }
        }
    }
}