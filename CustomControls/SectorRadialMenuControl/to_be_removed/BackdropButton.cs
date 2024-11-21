using Eto.Drawing;
using Eto.Forms;

namespace customControls
{
    public partial class SectorArcButton
    {
        protected class backdropButton : Drawable
        {
            protected Image _image;
            public backdropButton(Image image, Size size) : base()
            {
                _image = image;
                base.Size = size;
            }
            public void setImage(Image image)
            {
                _image = image;
                Invalidate();
            }
            protected override void OnPaint(PaintEventArgs drawingContext)
            {
                drawingContext.Graphics.DrawImage(_image, 0, 0);
            }
        }
    }
}