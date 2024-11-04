using Eto.Drawing;
using Eto.Forms;
using Rhino.UI;

namespace customControls
{
    class SectorArcButton : Drawable
    {
        protected SectorData _sectorData;
        protected bool isHovering = false;
        public new string ID { get { return _sectorData.buttonID; } set { base.ID = value; } }
        protected bool isDraggingIcon = false;
        protected ButtonProperties properties = new ButtonProperties();
        /// <summary>
        /// Button click event
        /// </summary>
        public event buttonClickEvent onclickEvent;
        /// <summary>
        /// Event for button click
        /// </summary>
        /// <param name="sender"></param>

        public delegate void buttonRequestFocus(SectorArcButton sender);
        /// <summary>
        /// Button click event
        /// </summary>
        public event buttonRequestFocus onRequestFocusEvent;
        /// <summary>
        /// Event for button click
        /// </summary>
        /// <param name="sender"></param>
        public delegate void buttonClickEvent(SectorArcButton sender);

        public SectorArcButton(SectorData sectorData) : base()
        {
            _sectorData = sectorData;
            Size = _sectorData.size;
            MouseMove += mouseMoveHandler;
            MouseLeave += mouseLeaveHandler;
            MouseEnter += mouseEnterHandler;
            MouseDown += mouseDownHandler;

            // DragDrop
            AllowDrop = true;
            DragEnter += dragEnterHandler;
            DragOver += dragOverHandler;
            DragLeave += dragLeaveHandler;
            DragDrop += dragDropHandler;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw arc sector image
            if (!isHovering)
            {
                if (_sectorData.normalStateImage != null)
                {
                    e.Graphics.DrawImage(_sectorData.normalStateImage, 0, 0);
                }
            }
            else
            {
                if (_sectorData.overStateImage != null)
                {
                    e.Graphics.DrawImage(_sectorData.overStateImage, 0, 0);
                }
            }

            // Draw arc sector icon
            if (properties.isActive && properties.icon != null)
            {
                var arcCenterWorld = _sectorData.sectorCenter();
                var arcCenterLocal = _sectorData.convertWorldToLocal(arcCenterWorld);

                e.Graphics.DrawImage(properties.icon, arcCenterLocal.X - (properties.icon.Size.Width / 2), arcCenterLocal.Y - (properties.icon.Size.Height / 2));
            }
        }

        private void mouseDownHandler(object sender, MouseEventArgs e)
        {
            if (!isDraggingIcon)
            {
                if (e.Modifiers == Keys.Application && properties.isActive)
                {
                    DataObject eventObj = new DataObject();
                    DoDragDrop(eventObj, DragEffects.All, properties.icon, new PointF(10, 10));

                }
                else
                {
                    if (properties.isActive)
                    {
                        if (properties.rhinoScript != "")
                        {
                            onclickEvent?.Invoke(this); // Raise onclick event to be handled by delegate when rhino command is executed
                            Rhino.RhinoApp.RunScript(properties.rhinoScript, false);
                        }
                    }
                }
            }
        }
        private void mouseMoveHandler(object sender, MouseEventArgs e)
        {
            // Ensure main plugin windo has focus -> workaround for click event that doesn't work if main window has no focus
            // If main window has no focus, the click event gives focus to main window and no click event on button occurs
            this.onRequestFocusEvent?.Invoke(this);

            var new_isHovering = _sectorData.isPointInShape(e.Location);
            if (new_isHovering != isHovering)
            {
                isHovering = new_isHovering;
                Invalidate();
            }
        }
        private void mouseEnterHandler(object sender, MouseEventArgs e)
        {
            var new_isHovering = _sectorData.isPointInShape(e.Location);
            if (new_isHovering != isHovering)
            {
                isHovering = new_isHovering;
                Invalidate();
            }
        }
        private void mouseLeaveHandler(object sender, MouseEventArgs e)
        {
            isHovering = false;
            Invalidate();
        }

        private void dragEnterHandler(object sender, DragEventArgs e)
        {
            // Ensure mouse cursor if hovering arc sector
            OnMouseMove(new MouseEventArgs(e.Buttons, e.Modifiers, e.Location));
            if (isHovering)
            {
                isDraggingIcon = true;
                if (dragSourceType(e.Source) == sourceTypes.self)
                {
                    if (((SectorArcButton)e.Source).ID == ID)
                    {
                        e.Effects = DragEffects.Link;
                    }
                }
            }
        }

        private void dragLeaveHandler(object sender, DragEventArgs e)
        {
            // Ensure mouse cursor if leaving arc sector
            OnMouseLeave(new MouseEventArgs(e.Buttons, e.Modifiers, e.Location));
            e.Effects = DragEffects.All;
            if (!isHovering)
            {
                isDraggingIcon = false;
                e.Effects = DragEffects.All;
            }

            var dSource = dragSourceType(e.Source);
            if ((dSource == sourceTypes.self && ((SectorArcButton)e.Source).ID == ID) || dSource == sourceTypes.rhinoItem)
            {
                properties.isActive = false;
                Invalidate();
            }
        }
        private void dragOverHandler(object sender, DragEventArgs e)
        {
            // Ensure mouse cursor if hovering arc sector
            OnMouseMove(new MouseEventArgs(e.Buttons, e.Modifiers, e.Location));
            if (!isHovering)
            {
                isDraggingIcon = false;
            }
            else
            {
                e.Effects = DragEffects.Copy;
            }
        }
        private void dragDropHandler(object sender, DragEventArgs e)
        {
            // Ensure mouse cursor if hovering arc sector
            OnMouseMove(new MouseEventArgs(e.Buttons, e.Modifiers, e.Location));

            if (isHovering)
            {
                switch (dragSourceType(e.Source))
                {
                    case sourceTypes.self:
                        if (((SectorArcButton)e.Source).ID == ID)
                        {
                            // If drag drop and button had previously an icon/script, restore it
                            properties.isActive = properties.icon != null ? true : false;
                            Invalidate();
                        }
                        else
                        {
                            properties.icon = ((SectorArcButton)e.Source).properties.icon; //TODO: Ensure icon data are copied to new properties
                            properties.rhinoScript = ((SectorArcButton)e.Source).properties.rhinoScript;
                            properties.isActive = true;
                            Invalidate();
                        }
                        break;
                    case sourceTypes.rhinoItem:
                        var rhinoToolbarItem = getDroppedToolbarItem(e);
                        properties.icon = rhinoToolbarItem.icon;
                        properties.rhinoScript = rhinoToolbarItem.script;
                        properties.isActive = true;
                        Invalidate();
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Get type of object dragged into this control
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private sourceTypes dragSourceType(Control source)
        {
            if (source.GetType() == typeof(SectorArcButton))
            {
                return sourceTypes.self;
            }
            else if (source.GetType().ToString() == "Rhino.UI.Internal.TabPanels.Controls.ToolBarControlItem")
            {
                return sourceTypes.rhinoItem;
            }
            else
            {
                return sourceTypes.unknown;
            }
        }

        /// <summary>
        ///  <para>When a drop event occurs (dragLeave), check if drop item is a Rhino toolbar item</para>
        ///  <para>If so, try to get the script command macro and the associated icon</para>
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
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
    }
}