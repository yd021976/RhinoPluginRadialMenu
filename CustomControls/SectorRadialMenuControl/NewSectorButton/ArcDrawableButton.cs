using System;
using AppKit;
using CoreGraphics;
using Eto.Drawing;
using Eto.Forms;

namespace customControls
{
    public class ArcDrawableButton : Drawable
    {
        public NSView _nsViewObject { get => getNSView(); }

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
            Size = size;
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
            base.OnPaint(e);
            // e.Graphics.DrawRectangle(Colors.Black,0,0,Parent.Size.Width-1,Parent.Size.Height-1);
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