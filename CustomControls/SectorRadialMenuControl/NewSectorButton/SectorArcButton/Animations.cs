using System.Collections.Generic;
using AppKit;

namespace customControls
{
    public partial class SectorArcButton
    {
        private void animateHoverEffect()
        {
            NSAnimationContext.RunAnimation(
                (context) =>
                {
                    context.Duration = 0.5;
                    context.AllowsImplicitAnimation = true;
                    buttons[buttonsTypeName.over]._nsViewObject.AlphaValue = isHovering ? 1 : 0;
                    buttons[buttonsTypeName.normal]._nsViewObject.AlphaValue = isHovering ? 0 : 1;
                });
        }
        private void animateDisableEffect()
        {
            NSAnimationContext.RunAnimation(
                (context) =>
                {
                    context.Duration = 0.5;
                    context.AllowsImplicitAnimation = true;
                    buttons[buttonsTypeName.normal]._nsViewObject.AlphaValue = 0;
                    buttons[buttonsTypeName.over]._nsViewObject.AlphaValue = 0;
                    buttons[buttonsTypeName.disabled]._nsViewObject.AlphaValue = 1;
                });
        }
        private void animateEnableEffect()
        {
            NSAnimationContext.RunAnimation(
                (context) =>
                {
                    context.Duration = 0.5;
                    context.AllowsImplicitAnimation = true;
                    buttons[buttonsTypeName.normal]._nsViewObject.AlphaValue = isHovering ? 0 : 1;
                    buttons[buttonsTypeName.over]._nsViewObject.AlphaValue = isHovering ? 1 : 0;
                    buttons[buttonsTypeName.disabled]._nsViewObject.AlphaValue = 0;
                });
        }
    }
}