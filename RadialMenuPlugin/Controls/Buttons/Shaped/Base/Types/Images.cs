using System.Collections.Generic;
using Eto.Drawing;

namespace RadialMenuPlugin.Controls.Buttons.Shaped.Base.Types.Images
{
    /// <summary>
    /// Image names
    /// </summary>
    public class ShapedButtonImageNames : EnumKey
    {
        public static readonly ShapedButtonImageNames Default = new ShapedButtonImageNames("default");
        public static readonly ShapedButtonImageNames Hover = new ShapedButtonImageNames("hover");
        public static readonly ShapedButtonImageNames Selected = new ShapedButtonImageNames("selected");
        public static readonly ShapedButtonImageNames disabled = new ShapedButtonImageNames("disabled");
        public static readonly ShapedButtonImageNames mask = new ShapedButtonImageNames("mask");
        protected ShapedButtonImageNames(string key) : base(key) { }
    }
    /// <summary>
    /// 
    /// </summary>
    public class ImageData
    {
        public Image Image;
        public float Alpha;
        public ImageData(Image image, float alpha)
        {
            Image = image;
            Alpha = alpha;
        }
    }
    /// <summary>
    /// List of images
    /// </summary>
    public class ShapedButtonImageList : Dictionary<IBaseEnumKey, ImageData> { }
    /// <summary>
    /// List of image buttons controls
    /// </summary>
    public class ShapedButtonImageButtonList : Dictionary<IBaseEnumKey, ImageButton> { }

}