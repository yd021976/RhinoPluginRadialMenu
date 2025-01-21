using System.ComponentModel;
using AppKit;
using Eto.Drawing;
using Eto.Forms;
using RadialMenuPlugin.Data;

namespace RadialMenuPlugin.Controls.Buttons
{
    public class CenterMenuButton : RoundButtonBase
    {
        protected Icon _LeftMouseClick;
        protected Icon _RightMouseClick;
        protected ButtonModelData _ButtonModelData;
        public NSView NsView { get => _GetNSView(); }
        public BindableBinding<CenterMenuButton, ButtonModelData> modelBinding => new BindableBinding<CenterMenuButton, ButtonModelData>(
            this,
            (CenterMenuButton obj) => obj._ButtonModelData,
            delegate (CenterMenuButton obj, ButtonModelData value)
            {
                if (obj._ButtonModelData != null) obj._ButtonModelData.Properties.PropertyChanged -= _ModelDataChangedHandler;
                obj._ButtonModelData = value;
                if (obj._ButtonModelData != null) obj._ButtonModelData.Properties.PropertyChanged += _ModelDataChangedHandler;
                Invalidate();
            }
        );
        /// <summary>
        /// 
        /// </summary>
        public CenterMenuButton() : base()
        {
            _LeftMouseClick = Bitmap.FromResource("RadialMenu.Bitmaps.mouse_left_click.png").WithSize(16, 16);
            _RightMouseClick = Bitmap.FromResource("RadialMenu.Bitmaps.mouse_right_click.png").WithSize(16, 16);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            _DrawMacroTooltips(e.Graphics);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        protected void _DrawMacroTooltips(Graphics g)
        {
            if (_ButtonModelData == null) return;
            var fontSize = 8;
            var font = Fonts.Sans(fontSize);
            var brush = new SolidBrush(Colors.Black);

            var iconPosX = 10; 
            var iconWidth = iconPosX + _LeftMouseClick.Width;
            var posY = (Height / 2) - fontSize; 
            var posYinc = 18; 
            var posX = iconWidth + 3;

            var text = new FormattedText();
            text.ForegroundBrush = brush;
            text.Font = font;
            text.Alignment = FormattedTextAlignment.Left;
            text.MaximumWidth = Width - iconWidth - 5; // keep 5px right margin
            text.Trimming = FormattedTextTrimming.CharacterEllipsis;

            //TODO: compute center of icon+text
            if (_ButtonModelData.Properties.LeftMacro.Tooltip != "")
            {
                text.Text = _ButtonModelData.Properties.LeftMacro.Tooltip;
                var textSize = text.Measure();
                g.DrawText(text, new PointF(posX, posY));
                g.DrawImage(_LeftMouseClick, new PointF(iconPosX, posY + (textSize.Height / 2) - (_LeftMouseClick.Height / 2)));
                posY += posYinc;
            }
            if (_ButtonModelData.Properties.RightMacro.Tooltip != "")
            {
                text.Text = _ButtonModelData.Properties.RightMacro.Tooltip;
                var textSize = text.Measure();
                g.DrawText(text, new PointF(posX, posY));
                g.DrawImage(_RightMouseClick, new PointF(iconPosX, posY + (textSize.Height / 2) - (_LeftMouseClick.Height / 2)));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void _ModelDataChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ButtonModelData.Properties.LeftMacro):
                case nameof(ButtonModelData.Properties.RightMacro):
                    Invalidate(); // Redraw tooltip
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Return the native MacOS NSView object
        /// </summary>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        protected NSView _GetNSView()
        {
            var ctrlProp = Handler.GetType().GetProperty("Control");
            return (NSView)ctrlProp.GetValue(Handler, null);
        }
    }
}