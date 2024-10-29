
using System;
using System.Drawing;
using System.Reflection;
using Eto.Forms;
using Grasshopper.Kernel.Special;

namespace customControls
{
    public class RhinoToolBarItem
    {
        public string script { get; set; }
        public Eto.Drawing.Icon icon { get; set; }
        public RhinoToolBarItem(string script, Eto.Drawing.Icon icon)
        {
            this.script = script;
            this.icon = icon;
        }
    }
    public class buttonInfoUpdatedEventArgs
    {
        public RhinoToolBarItem item { get; }
        public buttonInfoUpdatedEventArgs(RhinoToolBarItem item)
        {
            this.item = item;
        }
    };
    public class RoundedButton : Eto.Forms.Drawable
    {
        /// <summary>
        /// Button click event
        /// </summary>
        public event System.EventHandler onclickEvent;

        /// <summary>
        /// Button infos is updated event
        /// </summary>
        public event buttonInfoUpdatedEvent onButtonInfoUpdated;

        /// <summary>
        /// Delegate for event buttonInfoUpdatedEvent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void buttonInfoUpdatedEvent(RoundedButton sender, buttonInfoUpdatedEventArgs e);


        // Radius of circle button
        public int radius = 50;

        // Pen size for external circle
        public int pen_size = 2;

        // button base border color
        public Eto.Drawing.Color borderColor = Eto.Drawing.Colors.Blue;

        // button mouse Hover border color
        public Eto.Drawing.Color hoverBorderColor = Eto.Drawing.Colors.Red;

        // Background color
        public Eto.Drawing.Color backgroundColor = Eto.Drawing.Colors.White;

        // current button border color
        protected Eto.Drawing.Color currentBorderColor;

        protected RhinoToolBarItem buttonItemInfos;
        public RoundedButton() : base()
        {
            // Event handler for mouse down => Will raise onclick event of this class
            this.MouseDown += this.onMouseDown;
            this.MouseEnter += this.onMouseEnter;
            this.MouseLeave += this.onMouseLeave;
            this.Size = new Eto.Drawing.Size(this.radius, this.radius);
            this.currentBorderColor = this.borderColor;
            this.DragLeave += this.testDragLeave;
            // this.DragOver += this.testDragOver; // Should be helpfull when dragging occurs and we need to re-arrange Radial Menu items (in the futur)
            this.AllowDrop = true; // Mandatory for drag & drop feature to work
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
            var backgroundBrush2 = new Eto.Drawing.SolidBrush(this.currentBorderColor);
            var borderPen = new Eto.Drawing.Pen(this.currentBorderColor, pen_size);
            var rPen = new Eto.Drawing.Pen(Eto.Drawing.Colors.Black, 2);
            var size = new Eto.Drawing.Rectangle(0, 0, radius - 1, radius - 1);
            var size2 = new Eto.Drawing.Rectangle(pen_size / 2, pen_size / 2, radius - 1 - pen_size, radius - 1 - pen_size);
            graphic.FillEllipse(backgroundBrush, size);
            graphic.DrawEllipse(borderPen, size2);
            graphic.DrawRectangle(rPen, size);
            if (this.buttonItemInfos != null)
            {
                var img = this.buttonItemInfos.icon.GetFrame(1).Bitmap;
                var posx = (this.radius - img.Width) / 2;
                var posy = (this.radius - img.Height) / 2;
                graphic.DrawImage(img, new Eto.Drawing.PointF(posx, posy));
            }
        }
        /**
        When mouse click down, raise "onClick" event of this class
        **/
        protected void onMouseDown(object sender, MouseEventArgs e)
        {
            this.onclickEvent?.Invoke(this, e); // Raise onclick event to be handled by delegate
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

        protected void testDragEnter(object sender, DragEventArgs e)
        {

        }
        protected void testDragLeave(object sender, DragEventArgs e)
        {
            var r = this.getDroppedToolbarItem(e);
            if (r != null)
            {
                this.buttonItemInfos = r;
                this.onButtonInfoUpdated?.Invoke(this, new buttonInfoUpdatedEventArgs(this.buttonItemInfos)); // Send event
                this.Invalidate(); // Readraw control
            }
        }


        /**
        <summary>
            When a drop event occurs (dragLeave), check if drop item is a Rhino toolbar item
            If so, try to get the script command macro and the associated icon
        </summary>
        <returns>
            RhinoToolBarItem or null if drop item is not type of "Rhino.UI.Internal.TabPanels.Controls.ToolBarControlItem" or command or icon is not found
        </returns>
        **/
        private RhinoToolBarItem getDroppedToolbarItem(DragEventArgs e)
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
                    var icon = (Eto.Drawing.Icon)iconCreateMethod.Invoke(lMacro, new object[] { new Eto.Drawing.Size(28, 28), true });
                    if (icon != null)
                    {
                        // Get the macro "script" command
                        var macroScript = lMacro.GetType().GetProperty("Script").GetValue(lMacro, null);
                        return new RhinoToolBarItem((string)macroScript, (Eto.Drawing.Icon)icon);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            return null;
        }
    }
}