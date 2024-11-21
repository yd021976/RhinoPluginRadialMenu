using System;
using AppKit;
using Eto.Drawing;
using Eto.Forms;

namespace customControls
{
    public class ArcDrawableButton : Drawable
    {
        /// <summary>
        /// Bindable to set mouse hover (isHovering) property with binding
        /// TODO: To be removed as it is unused
        /// </summary>
        public BindableBinding<ArcDrawableButton, bool> isHovering => new BindableBinding<ArcDrawableButton, bool>
        (
            this,
            (ArcDrawableButton b) => b._isHovering,
            delegate (ArcDrawableButton b, bool v)
            {
                b._isHovering = v;
            }
        );

        public NSView _nsViewObject { get => getNSView(); }

        private bool _isHovering;


#nullable enable
        private Image? _currentImage;
#nullable disable


        public ArcDrawableButton() : base() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="image"></param>
        /// <param name="size"></param>
        public ArcDrawableButton(Image image, Size size) : this()
        {
            _currentImage = image;
            base.Size = size;
        }
        public void setImage(Image image, Size? size = null)
        {
            if (size != null)
            {
                Size = (Size)size;
            }
            _currentImage = image;
            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            // base.OnPaint(e);
            if (_currentImage != null) // Could be null, so draw nothing
            {
                e.Graphics.DrawImage(_currentImage, 0, 0);
            }
        }

        /// <summary>
        /// Return the native MacOS NSView object
        /// </summary>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        private NSView getNSView()
        {
            var ctrlProp = Handler.GetType().GetProperty("Control");
            return (NSView)ctrlProp.GetValue(Handler, null);
        }
    }
}