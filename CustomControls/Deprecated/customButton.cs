
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;

namespace customControls
{
    public class RhinoToolBarItem
    {
        public string script { get; set; }
        public Eto.Drawing.Icon icon { get; set; }
        public bool isActive = true;
        public RhinoToolBarItem(string script, Eto.Drawing.Icon icon)
        {
            this.script = script;
            this.icon = icon;
        }
        public RhinoToolBarItem(string script, Eto.Drawing.Icon icon, bool isActive) : base()
        {
            this.script = script;
            this.icon = icon;
            this.isActive = isActive;
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
        public event buttonClickEvent onclickEvent;

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

        /// <summary>
        /// Event for button click
        /// </summary>
        /// <param name="sender"></param>
        public delegate void buttonClickEvent(RoundedButton sender);

        // Radius of circle button
        public int radius = 50;

        // Pen size for external circle
        public int pen_size = 4;

        // button base border color
        public Eto.Drawing.Color borderColor = Eto.Drawing.Colors.Blue;

        // button mouse Hover border color
        public Eto.Drawing.Color hoverBorderColor = Eto.Drawing.Colors.Red;

        // Background color
        public Eto.Drawing.Color backgroundColor = Eto.Drawing.Colors.White;

        // current button border color
        protected Eto.Drawing.Color currentBorderColor;

        protected RhinoToolBarItem buttonItemInfos = new RhinoToolBarItem("", null, false);

        public RoundedButton() : base()
        {
            // Event handler for mouse down => Will raise onclick event of this class
            MouseDown += onMouseDown;
            MouseEnter += onMouseEnter;
            MouseLeave += onMouseLeave;
            Size = new Eto.Drawing.Size(radius, radius);
            currentBorderColor = borderColor;
            DragLeave += onDragLeave;
            DragEnter += onDragEnter;
            DragDrop += onDragDrop;
            AllowDrop = true; // Mandatory for drag & drop feature to work
        }
        public RoundedButton(string id) : this()
        {
            ID = id;
        }
        public void setButtonIcon(Icon icon)
        {
            buttonItemInfos.icon = icon;
            Invalidate();
        }
        public void activateButtonInfo(bool state)
        {
            buttonItemInfos.isActive = state;
            Invalidate();
        }
        /**
         Draw the control
        **/
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            drawControl(e.Graphics);
        }
        private void drawControl(Eto.Drawing.Graphics graphic)
        {
            var innerBrushColor = new Eto.Drawing.SolidBrush(backgroundColor);
            var borderBrushColor = new Eto.Drawing.SolidBrush(currentBorderColor);
            var borderSize = new Eto.Drawing.Rectangle(0, 0, radius, radius);
            var innerSize = new Eto.Drawing.Rectangle(pen_size, pen_size, radius - (pen_size * 2), radius - (pen_size * 2));

            // Draw the icon shape
            graphic.FillEllipse(borderBrushColor, borderSize);
            graphic.FillEllipse(innerBrushColor, innerSize);

            // Draw icon if any
            if (buttonItemInfos.isActive == true && buttonItemInfos.icon != null)
            {
                var img = buttonItemInfos.icon.GetFrame(1).Bitmap;
                var posx = (radius - buttonItemInfos.icon.Width) / 2;
                var posy = (radius - buttonItemInfos.icon.Height) / 2;
                graphic.DrawImage(buttonItemInfos.icon, new Eto.Drawing.PointF(posx, posy));
            }
        }


        /// <summary>
        /// Will allow drag effect "move" only for Rhino ToolBarControlItems or for button icon/command remove cancellation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void onDragEnter(object sender, DragEventArgs e)
        {
            if (e.Source.GetType() == typeof(customControls.RoundedButton) && e.Source.ID == ID)
            {
                e.Effects = DragEffects.Link;
                buttonItemInfos.isActive = true;
                Invalidate(false);
            }
            else
            {
                var obj = e.Source;
                if (obj != null)
                {
                    if (obj.GetType().ToString() == "Rhino.UI.Internal.TabPanels.Controls.ToolBarControlItem")
                    {
                        e.Effects = DragEffects.Copy; // Allow effects here, show a "plus" on the icon
                                                      // get toolbar item icon and draw it in control
                        var item = getDroppedToolbarItem(e);
                        buttonItemInfos = item;
                        Invalidate(false);
                    }
                }
            }
        }
        /// <summary>
        /// Drop item in button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void onDragDrop(object sender, DragEventArgs e)
        {
            // Event "onDragLeave" clear "isDragging" and is always called before this event
            // So, here we check that the source of dragging is from this class -> This means that user drop the icon back into the button
            if (e.Source.GetType() == typeof(customControls.RoundedButton) && e.Source.ID == ID)
            {
                buttonItemInfos.isActive = true; // don't remove the icon/command
            }
            else
            {
                var r = getDroppedToolbarItem(e);
                if (r != null)
                {
                    buttonItemInfos = r;
                    buttonItemInfos.isActive = true;
                    onButtonInfoUpdated?.Invoke(this, new buttonInfoUpdatedEventArgs(buttonItemInfos)); // Send event
                    Invalidate(); // Readraw control
                }
            }
        }
        protected void onDragLeave(object sender, DragEventArgs e)
        {
            if (e.Source.GetType() == typeof(customControls.RoundedButton))
            {
                if (e.Source.ID == ID)
                {
                    buttonItemInfos.isActive = false; // remove the icon/command for this button
                    Invalidate(false);
                }
            }
            else if (e.Source.GetType().ToString() == "Rhino.UI.Internal.TabPanels.Controls.ToolBarControlItem")
            {
                // clear button toolbar icon and command for now (will be updated and saved in "onDragDrop" method)
                buttonItemInfos.isActive = false;
                Invalidate(false);
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

                    // Seems that "CreateIcon" is a good condidate to get the Rhino toolbar item icon
                    var iconCreateMethod = lMacro.GetType().GetMethod("CreateIcon");
                    var icon = (Eto.Drawing.Icon)iconCreateMethod.Invoke(lMacro, new object[] { new Eto.Drawing.Size(28, 28), true });
                    if (icon != null)
                    {
                        // Get the macro "script" command
                        var macroScript = lMacro.GetType().GetProperty("Script").GetValue(lMacro, null);
                        return new RhinoToolBarItem((string)macroScript, icon);
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
        protected void onMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Modifiers == Keys.Application)
            {
                DataObject eventObj = new DataObject();
                // eventObj.SetObject(this.buttonItemInfos, "");
                DoDragDrop(eventObj, DragEffects.Link, buttonItemInfos.icon, new Eto.Drawing.PointF(10, 10));
            }
            else
            {
                onclickEvent?.Invoke(this); // Raise onclick event to be handled by delegate
                if (buttonItemInfos.isActive)
                {
                    if (buttonItemInfos.script != "")
                    {
                        Rhino.RhinoApp.RunScript(buttonItemInfos.script, false);
                    }
                }
            }
        }

        protected void onMouseEnter(object sender, MouseEventArgs e)
        {

            currentBorderColor = this.hoverBorderColor;
            Invalidate(false); // redraw button
        }
        protected void onMouseLeave(object sender, MouseEventArgs e)
        {
            currentBorderColor = this.borderColor;
            Invalidate(false); // redraw button
        }

        private enum DragObjectSource
        {
            roundedButton = 0,
            rhinoToolbarItem = 1,
            unknown = 2
        }
        private DragObjectSource dragSourceType(DragEventArgs e)
        {
            if (e.Source.GetType().ToString() == "Rhino.UI.Internal.TabPanels.Controls.ToolBarControlItem")
            {
                return DragObjectSource.rhinoToolbarItem;
            }
            if (e.Source.GetType() == typeof(RoundedButton))
            {
                return DragObjectSource.roundedButton;
            }
            return DragObjectSource.unknown;
        }
    }
}