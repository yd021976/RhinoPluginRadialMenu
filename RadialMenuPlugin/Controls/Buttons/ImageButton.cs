using AppKit;
using Eto.Drawing;
using Eto.Forms;

namespace RadialMenuPlugin.Controls.Buttons
{
    public class ImageButton : Drawable
    {
        public NSView NsViewObject { get => _GetNSView(); }

#nullable enable
        private Image? _CurrentImage;
#nullable disable

        public ImageButton() : base() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="image"></param>
        /// <param name="size"></param>
        public ImageButton(Image image, Size size) : this()
        {
            _CurrentImage = image;
            Size = size;
        }
        public void SetImage(Image image, Size? size = null)
        {
            if (size != null)
            {
                Size = (Size)size;
            }
            _CurrentImage = image;
            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // e.Graphics.DrawRectangle(Colors.Black,0,0,Parent.Size.Width-1,Parent.Size.Height-1);
            if (_CurrentImage != null) // Could be null, so draw nothing
            {
                e.Graphics.DrawImage(_CurrentImage, 0, 0);
            }
        }

        /// <summary>
        /// Return the native MacOS NSView object
        /// </summary>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        private NSView _GetNSView()
        {
            var ctrlProp = Handler.GetType().GetProperty("Control");
            return (NSView)ctrlProp.GetValue(Handler, null);
        }
    }
}