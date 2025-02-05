using System.ComponentModel;
using System;
using RadialMenuPlugin.Controls.Buttons.Shaped.Base.Types.Images;

namespace RadialMenuPlugin.Controls.Buttons.Shaped.Base
{
    /// <summary>
    /// 
    /// </summary>
    public class BaseShapedButton : ShapedButton, INotifyPropertyChanged
    {
        #region Protected/Private Properties
        #endregion

        #region Public properties
        #endregion

        #region Public methods
        #endregion

        #region  Protected/Private Methods
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
