using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;

namespace RadialMenuPlugin.Controls.Buttons.Base
{
    public enum EType
    {
        normal = 1,
        hover = 2,
        selected = 3,
        disabled = 4,
        mask = 5
    }
    public sealed class BaseShapeButtonImages : Dictionary<EType, Bitmap>
    { }

    public class BaseShapeButton : Drawable
    {
        #region Protected/Private properties
        private static readonly NLog.Logger _Logger = NLog.LogManager.GetCurrentClassLogger();
        BaseShapeButtonImages _Images = new BaseShapeButtonImages();
        #endregion

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public BaseShapeButton() : base() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        public BaseShapeButton(Size size) : this() { }
        #endregion

        #region Public methods
        #endregion

        #region Protected methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        protected bool _isPointInShape(Point location)
        {
            var isPointInShape = false;

            if (_Images[EType.mask] == null) {
                _Logger.Debug("_isPointInShape: No mask image");
                return isPointInShape;
            }
            
            var bitmapData = _Images[EType.mask].Lock();
            var pixel = bitmapData.GetPixel(location.X, location.Y);
            if (pixel.A != 0) isPointInShape = true;
            return isPointInShape;
        }
        #endregion
    }
}