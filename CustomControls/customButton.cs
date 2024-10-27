
using System.Drawing;
using System.Reflection;
using Eto.Forms;
using Grasshopper.Kernel.Special;

namespace customControls
{
    class RoundedButton : Eto.Forms.Drawable
    {
        // click event
        public event System.EventHandler onclick;

        // Radius of circle button
        public int radius = 35;

        // button base border color
        public Eto.Drawing.Color borderColor = Eto.Drawing.Colors.Gray;

        // button mouse Hover border color
        public Eto.Drawing.Color hoverBorderColor = Eto.Drawing.Colors.Red;

        // Background color
        public Eto.Drawing.Color backgroundColor = Eto.Drawing.Colors.White;

        // current button border color
        protected Eto.Drawing.Color currentBorderColor;

        protected string macroCommand;
        protected Eto.Drawing.Icon icon;

        public RoundedButton() : base()
        {
            // Event handler for mouse down => Will raise onclick event of this class
            this.MouseDown += this.onMouseDown;
            this.MouseEnter += this.onMouseEnter;
            this.MouseLeave += this.onMouseLeave;
            this.Size = new Eto.Drawing.Size(this.radius, this.radius);
            this.currentBorderColor = this.borderColor;

            this.DragDrop += this.testDragDrop;
            this.DragEnter += this.testDragEnter;
            this.DragLeave += this.testDragLeave;
            this.DragOver += this.testDragOver;
            this.DragEnd += this.testDragDrop;
            this.AllowDrop = true;
        }
        public RoundedButton(string id) : this()
        {
            this.ID = id;
        }
        /**
        Draw the control
        **/
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            this.drawControl(e.Graphics);
        }
        private void drawControl(Eto.Drawing.Graphics graphic)
        {
            // var g = this.CreateGraphics();
            var backgroundBrush = new Eto.Drawing.SolidBrush(this.backgroundColor);
            var borderPen = new Eto.Drawing.Pen(this.currentBorderColor);
            var size = new Eto.Drawing.Rectangle(0, 0, radius, radius);
            graphic.FillEllipse(backgroundBrush, size);
            graphic.DrawEllipse(borderPen, size);
            if (this.icon != null)
            {
                var img = this.icon.GetFrame(1).Bitmap;
                graphic.DrawImage(img, new Eto.Drawing.PointF(0, 0));
            }
        }
        /**
        When mouse click down, raise "onClick" event of this class
        **/
        protected void onMouseDown(object sender, MouseEventArgs e)
        {
            this.onclick?.Invoke(this, e); // Raise onclick event to be handled by delegate
        }
        protected void onMouseEnter(object sender, MouseEventArgs e)
        {
            this.currentBorderColor = this.hoverBorderColor;
            this.Invalidate(false); // redraw button
        }
        protected void onMouseLeave(object sender, MouseEventArgs e)
        {
            this.currentBorderColor = this.borderColor;
            this.Invalidate(false); // redraw button
        }
        protected void testDragDrop(object sender, DragEventArgs e)
        {
            var test = 0;
        }
        protected void testDragEnter(object sender, DragEventArgs e)
        {
            var obj = e.Source;
            if (obj != null)
            {
                if (obj.GetType().ToString() == "Rhino.UI.Internal.TabPanels.Controls.ToolBarControlItem")
                {
                    // Get the macro of the dropped toolbar item
                    var lMacro = obj.GetType().GetProperty("LeftMacro").GetValue(obj, null);

                    // List methods of ToolBarItemController
                    var toolbarItemController = lMacro.GetType().GetMethods(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public);

                    // Seems that "CreateIcon" is a good condidate to get the Rhino toolbar item icon
                    var iconCreateMethod = lMacro.GetType().GetMethod("CreateIcon");
                    var icon = iconCreateMethod.Invoke(lMacro, new object[] { new Eto.Drawing.Size(28, 28), true });
                    if (icon != null)
                    {
                        this.icon = (Eto.Drawing.Icon)icon;
                        this.Invalidate(); // readraw button control (testing)
                    }
                    // Get the macro "script" command
                    var macroScript = lMacro.GetType().GetProperty("Script").GetValue(lMacro, null);
                }
            }
        }
        protected void testDragLeave(object sender, DragEventArgs e)
        {
            var test = 0;
        }
        protected void testDragOver(object sender, DragEventArgs e)
        {
            var test = 0;
        }
    }
}