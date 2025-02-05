using System;
using System.Collections.Generic;
using Eto.Drawing;
using RadialMenuPlugin.Controls.Buttons.Shaped.Base;
using RadialMenuPlugin.Controls.Buttons.Shaped.Base.States;
using RadialMenuPlugin.Controls.Buttons.Shaped.Base.Types;
using RadialMenuPlugin.Controls.Buttons.Shaped.Base.Types.Images;
using RadialMenuPlugin.Controls.Buttons.Shaped.Form.States;
using RadialMenuPlugin.Data;

namespace RadialMenuPlugin.Controls.Buttons.Shaped.Form
{
    public class FormCenterTooltipNames : EnumKey
    {
        public static readonly FormCenterTooltipNames Tooltip = new FormCenterTooltipNames("tooltip");
        protected FormCenterTooltipNames(string key) : base(key) { }
    }
    public class FormCenterButton : BaseShapedButton
    {
        #region Protected properties
        /// <summary>
        /// Current tootltip left macro
        /// </summary>
        protected Macro LeftTooltip = new Macro();
        /// <summary>
        /// Current tootltip right macro
        /// </summary>
        protected Macro RightTooltip = new Macro();
        /// <summary>
        /// left mouse click icon
        /// </summary>
        private Icon LeftMouseIcon;
        /// <summary>
        /// right mouse click icon
        /// </summary>
        private Icon RightMouseIcon;
        /// <summary>
        /// Current state for tooltip display
        /// </summary>
        protected State CurrentTooltipState;
        /// <summary>
        /// List of state instances
        /// </summary>
        protected StatePool TooltipStatePool;
        /// <summary>
        /// Constant for tooltip icon height
        /// </summary>
        protected const int TooltipIconHeight = 20;
        /// <summary>
        /// Constant for tooltip X position inside control
        /// </summary>
        protected const int TooltipLeftPos = 5;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size"></param>
        public FormCenterButton(Size size) : base()
        {
            Size = size;

            LeftMouseIcon = Icon.FromResource("RadialMenu.Bitmaps.mouse_left_click.png").WithSize(TooltipIconHeight, TooltipIconHeight);
            RightMouseIcon = Icon.FromResource("RadialMenu.Bitmaps.mouse_right_click.png").WithSize(TooltipIconHeight, TooltipIconHeight);

            //FIXME Tooltip animation is glitchy in most of the cases
            // Two main reasons:
            // 1 - We doesn't manage correctly state changes from "tooltip" to "tooltip" -> Think we need 2 tooltip controls and swap them
            // 2 - 
            TooltipStatePool = new StatePool() {
                {
                    typeof(States.DefaultState), new States.DefaultState(RunAnimation, new List<Action>(){HideTooltip}.ToArray())
                },
                {
                    typeof(TooltipState), new TooltipState(RunAnimation, new List<Action>(){BuildTooltipImage,ShowTooltip}.ToArray())
                },
            };

            CurrentTooltipState = TooltipStatePool[typeof(States.DefaultState)];
            SetImageList(CreateImages());
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Update tooltip. Set both <paramref name="leftAction"/> and <paramref name="rightAction"/> "Tooltip" property to empty string (i.e. "") to hide tooltip
        /// </summary>
        /// <param name="leftAction"></param>
        /// <param name="rightAction"></param>
        public void SetTooltip(Macro leftAction = null, Macro rightAction = null)
        {
            LeftTooltip = leftAction;
            RightTooltip = rightAction;
            if (LeftTooltip == null && RightTooltip == null) // if no tooltip, hide tooltip control
            {
                CurrentTooltipState = CurrentTooltipState.NextState(TooltipEvent.Default, TooltipStatePool);
            }
            else
            {
                // Switch to next step depending if we want to display button tooltip or if we want to display a radial menu tooltip
                CurrentTooltipState = CurrentTooltipState.NextState(TooltipEvent.Tooltip, TooltipStatePool);
            }
        }
        #endregion

        #region  Protected methods
        /// <summary>
        /// Build tooltip image
        /// </summary>
        /// <param name="button"></param>
        protected void BuildTooltipImage()
        {
            var bitmap = new Bitmap(new Size(Width - TooltipLeftPos, TooltipIconHeight * 2 + 4), PixelFormat.Format32bppRgba);
            var gc = new Graphics(bitmap);

            var ft = new FormattedText()
            {
                Alignment = FormattedTextAlignment.Left,
                Trimming = FormattedTextTrimming.CharacterEllipsis,
                Font = Fonts.Sans(8),
                ForegroundBrush = new SolidBrush(Colors.Black),
                MaximumWidth = bitmap.Width - LeftMouseIcon.Width - 3
            };
            var ypos = 0;
            if (LeftTooltip != null && LeftTooltip?.Tooltip != "")
            {
                gc.DrawImage(LeftMouseIcon, 0, ypos);
                ft.Text = LeftTooltip.Tooltip;
                gc.DrawText(ft, new Point(TooltipIconHeight + 3, ypos));
            }
            if (RightTooltip != null && RightTooltip?.Tooltip != "")
            {
                ypos += TooltipIconHeight + 2;
                gc.DrawImage(RightMouseIcon, 0, ypos);
                ft.Text = RightTooltip.Tooltip;
                gc.DrawText(ft, new Point(TooltipIconHeight + 3, ypos));
            }
            Buttons[FormCenterTooltipNames.Tooltip].SetImage(bitmap, bitmap.Size);
            gc.Dispose();
        }
        ShapedButtonImageList CreateImages()
        {
            var penSize = 2; var offset = 4;
            var images = new ShapedButtonImageList() {
                {ShapedButtonImageNames.Default,DrawImage(Width, penSize, Colors.LightGrey, Colors.Gray,0.2f)},
                {ShapedButtonImageNames.Hover,DrawImage(Width, penSize, Colors.LightSkyBlue, Colors.Gray,0.5f)},
                {ShapedButtonImageNames.Selected,DrawImage(Width, penSize, Colors.OrangeRed, Colors.Gray,0.7f)},
                {ShapedButtonImageNames.disabled,DrawImage(Width, penSize, Colors.DarkGray, Colors.DarkGray,0.2f)},
                {ShapedButtonImageNames.mask,DrawImage(Width-offset, penSize, Colors.Blue, Colors.Blue,1f)}
            };
            return images;
        }
        /// <summary>
        /// Set tooltip images location
        /// </summary>
        protected override void InitButtons()
        {
            base.InitButtons();
            var tooltipButton = new ImageButton();
            Buttons.Add(FormCenterTooltipNames.Tooltip, tooltipButton);
            Add(Buttons[FormCenterTooltipNames.Tooltip], new Point(5, (Height / 2) - ((TooltipIconHeight * 2 + 4) / 2)));
        }
        #endregion

        #region Button circle image generators
        ImageData DrawImage(int diameter, int penSize, Color borderColor, Color innerColor, float alpha)
        {
            var bitmap = new Bitmap(new Size(Width, Height), PixelFormat.Format32bppRgba);
            var g = new Graphics(bitmap);

            var innerBrushColor = new SolidBrush(innerColor);
            var borderBrushColor = new SolidBrush(borderColor);

            var outerCirclePos = new Point((Width - diameter) / 2, (Height - diameter) / 2);
            var outerCircleGp = DrawCircle(outerCirclePos, diameter);

            var innerCirclePos = new Point(outerCirclePos.X + (penSize / 2), outerCirclePos.Y + (penSize / 2));
            var innerCircleGp = DrawCircle(innerCirclePos, diameter - penSize);
            g.PixelOffsetMode = PixelOffsetMode.None;
            g.FillPath(borderBrushColor, outerCircleGp);
            g.FillPath(innerBrushColor, innerCircleGp);
            g.Dispose();
            return new ImageData(bitmap, alpha);
        }
        GraphicsPath DrawCircle(Point location, int radius)
        {
            var gp = new GraphicsPath();
            // Draw shape
            gp.AddEllipse(location.X, location.Y, radius, radius);
            return gp;
        }
        #endregion

        #region Tooltip animations
        /// <summary>
        /// Animation context rule : Hiding tooltip controls 
        /// </summary>
        protected void HideTooltip()
        {
            Buttons[FormCenterTooltipNames.Tooltip].NsViewObject.AlphaValue = 0;
        }
        /// <summary>
        /// Animation context rule : Fade in/out by switch first <--> second button controls to smooth fade in/out
        /// </summary>
        protected void ShowTooltip()
        {
            Buttons[FormCenterTooltipNames.Tooltip].NsViewObject.AlphaValue = 1;
        }
        #endregion

        #region Round shaped animations
        protected override void AnimationNormalHandler()
        {
            try
            {
                Buttons[ShapedButtonImageNames.Default].NsViewObject.AlphaValue = ImageList[ShapedButtonImageNames.Default].Alpha;
                Buttons[ShapedButtonImageNames.Selected].NsViewObject.AlphaValue = 0;
                Buttons[ShapedButtonImageNames.disabled].NsViewObject.AlphaValue = 0;
                Buttons[ShapedButtonImageNames.Hover].NsViewObject.AlphaValue = 0;
            }
            catch (Exception e) { Logger.Error(e); }
        }
        protected override void AnimationHoverHandler()
        {
            try
            {
                Buttons[ShapedButtonImageNames.Default].NsViewObject.AlphaValue = 0;
                Buttons[ShapedButtonImageNames.Selected].NsViewObject.AlphaValue = 0;
                Buttons[ShapedButtonImageNames.disabled].NsViewObject.AlphaValue = 0;
                Buttons[ShapedButtonImageNames.Hover].NsViewObject.AlphaValue = ImageList[ShapedButtonImageNames.Hover].Alpha;
            }
            catch (Exception e) { Logger.Error(e); }
        }
        protected override void AnimationSelectedHandler()
        {
            try
            {
                Buttons[ShapedButtonImageNames.Default].NsViewObject.AlphaValue = 0;
                Buttons[ShapedButtonImageNames.Selected].NsViewObject.AlphaValue = ImageList[ShapedButtonImageNames.Selected].Alpha;
                Buttons[ShapedButtonImageNames.disabled].NsViewObject.AlphaValue = 0;
                Buttons[ShapedButtonImageNames.Hover].NsViewObject.AlphaValue = 0;
            }
            catch (Exception e) { Logger.Error(e); }
        }
        protected override void AnimationDisabledHandler()
        {
            try
            {
                Buttons[ShapedButtonImageNames.Default].NsViewObject.AlphaValue = 0;
                Buttons[ShapedButtonImageNames.Selected].NsViewObject.AlphaValue = 0;
                Buttons[ShapedButtonImageNames.disabled].NsViewObject.AlphaValue = ImageList[ShapedButtonImageNames.disabled].Alpha;
                Buttons[ShapedButtonImageNames.Hover].NsViewObject.AlphaValue = 0;
            }
            catch (Exception e) { Logger.Error(e); }
        }
        #endregion
    }
}