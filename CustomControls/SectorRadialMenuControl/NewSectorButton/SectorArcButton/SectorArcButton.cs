using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Eto.Drawing;
using Eto.Forms;
using Rhino;

namespace customControls
{
    public partial class SectorArcButton : PixelLayout
    {
        public override bool Enabled
        {
            get => Handler.Enabled;
            set
            {
                if (value == false) animateDisableEffect(); else animateEnableEffect();
                Handler.Enabled = value;
            }
        }
        protected enum buttonsTypeName
        {
            normal = 0,
            over = 1,
            disabled = 2,
            icon = 3
        }
        /// <summary>
        /// Drawables used for animating button UI state changes
        /// </summary>
        protected Dictionary<buttonsTypeName, ArcDrawableButton> buttons = new Dictionary<buttonsTypeName, ArcDrawableButton>();

        /// <summary>
        /// Data Model Binding
        /// </summary>
        ButtonModel model = new ButtonModel();
        public BindableBinding<SectorArcButton, ButtonModel> ButtonModelBinding => new BindableBinding<SectorArcButton, ButtonModel>(
            this,
            (SectorArcButton obj) => obj.model,
            // Update "model" property with new value and register a property changed event handler of the "model" object
            delegate (SectorArcButton obj, ButtonModel value)
            {
                model.PropertyChanged -= modelChangedHandler; // Remove property changed event handler on current "model" object
                obj.model = value; // update property
                model.PropertyChanged += modelChangedHandler; // Add property changed handler on "model"
            });

        /// <summary>
        /// Custom and override of ID to generate button ID
        /// </summary>
        public new string ID { get { return model.buttonID; } set { base.ID = value; } }

        public SectorArcButton() : base()
        {
            var w = Stopwatch.StartNew();

            // Create buttons
            buttons[buttonsTypeName.normal] = new ArcDrawableButton();
            buttons[buttonsTypeName.over] = new ArcDrawableButton();
            buttons[buttonsTypeName.disabled] = new ArcDrawableButton();
            buttons[buttonsTypeName.icon] = new ArcDrawableButton();

            base.Size = this.model.sectorData.size;

            // Add button to layout
            Add(buttons[buttonsTypeName.normal], 0, 0);
            Add(buttons[buttonsTypeName.over], 0, 0);
            Add(buttons[buttonsTypeName.disabled], 0, 0);
            Add(buttons[buttonsTypeName.icon], 0, 0);



            buttons[buttonsTypeName.over]._nsViewObject.AlphaValue = 0;
            buttons[buttonsTypeName.disabled]._nsViewObject.AlphaValue = 0;

            // Mouse events
            MouseMove += mouseMoveHandler;
            MouseLeave += mouseLeaveHandler;
            MouseEnter += mouseEnterHandler;
            MouseDown += mouseDownHandler;

            // DragDrop events
            AllowDrop = true;
            DragEnter += dragEnterHandler;
            DragOver += dragOverHandler;
            DragLeave += dragLeaveHandler;
            DragDrop += dragDropHandler;
            
            w.Stop();
            Console.SetOut(RhinoApp.CommandLineOut);
            Console.WriteLine("SectorArcButton takes:" + w.ElapsedMilliseconds);
        }


        /// <summary>
        /// Update button display as soon as a new Model is binded to this class. When occurs, the "model" property has already been updated, so we can use it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void modelChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            // Update buttons image
            buttons[buttonsTypeName.normal].setImage(model.sectorData.images.normalStateImage, model.sectorData.size);
            buttons[buttonsTypeName.over].setImage(model.sectorData.images.overStateImage, model.sectorData.size);
            buttons[buttonsTypeName.disabled].setImage(model.sectorData.images.disabledStateImage, model.sectorData.size);
            buttons[buttonsTypeName.icon].setImage(model.properties.icon, model.sectorData.size);

            // Update self data
            Size = model.sectorData.size;


            // Update Rhino icon
            updateIcon();
        }
       
        ~SectorArcButton()
        {
        }
        private void mouseDownHandler(object sender, MouseEventArgs e)
        {
            if (!_isDraggingIcon)
            {
                if (e.Modifiers == Keys.Application && model.properties.isActive)
                {
                    DataObject eventObj = new DataObject();
                    DoDragDrop(eventObj, DragEffects.All, model.properties.icon, new PointF(10, 10));

                }
                else
                {
                    if (model.properties.isActive)
                    {
                        if (model.properties.rhinoScript != "")
                        {
                            onButtonClickEvent?.Invoke(this); // Raise onclick event to be handled by delegate when rhino command is executed
                            RhinoApp.RunScript(model.properties.rhinoScript, false);//FIXME: Should be executed by the Form control, not by button
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handler for mouse move in control. Note that we check and fire custom mouse "leave" and "over" here because
        /// the shape is not a rectangle. So the mouse can already "enter" in rectangle but not in the control shape
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouseMoveHandler(object sender, MouseEventArgs e)
        {
            // Ensure main plugin windo has focus -> workaround for click event that doesn't work if main window has no focus
            // If main window has no focus, the click event gives focus to main window and no click event on button occurs
            onButtonRequestFocusEvent?.Invoke(this);

            var new_isHovering = model.sectorData.isPointInShape(e.Location);
            if (new_isHovering != isHovering)
            {
                isHovering = new_isHovering;
                if (isHovering)
                {
                    onButtonMouseOverButton?.Invoke(this); // Notify mouse is over the button
                    animateHoverEffect();
                }
                else
                {
                    onbuttonMouseLeaveButton?.Invoke(this);
                    animateHoverEffect();
                }

                // update button
                // Invalidate();
            }
        }


        private void mouseEnterHandler(object sender, MouseEventArgs e)
        {
            // var new_isHovering = _sectorData.isPointInShape(e.Location);
            // if (new_isHovering != isHovering)
            // {
            //     isHovering = new_isHovering;
            //     Invalidate();
            // }

        }
        private void mouseLeaveHandler(object sender, MouseEventArgs e)
        {
            if (isHovering) // Avoid sending "leave" event twice
            {
                isHovering = false;
                onbuttonMouseLeaveButton?.Invoke(this);
                animateHoverEffect();
            }
        }

        private void dragEnterHandler(object sender, DragEventArgs e)
        {
            // Ensure mouse cursor if hovering arc sector
            OnMouseMove(new MouseEventArgs(e.Buttons, e.Modifiers, e.Location));
            if (isHovering)
            {
                _isDraggingIcon = true;
                if (dragSourceType(e.Source) == DragSourceTypes.self)
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
                _isDraggingIcon = false;
                e.Effects = DragEffects.All;
            }

            var dSource = dragSourceType(e.Source);
            if (dSource == DragSourceTypes.self && ((SectorArcButton)e.Source).ID == ID)
            {
                model.properties.isActive = false;
                onButtonNewIconAdded?.Invoke(this); // update settings
                updateIcon();
                Invalidate();
            }
        }
        private void dragOverHandler(object sender, DragEventArgs e)
        {
            // Ensure mouse cursor if hovering arc sector
            OnMouseMove(new MouseEventArgs(e.Buttons, e.Modifiers, e.Location));
            if (!isHovering)
            {
                _isDraggingIcon = false;
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
                    case DragSourceTypes.self:
                        if (((SectorArcButton)e.Source).ID == ID)
                        {
                            // If drag drop and button had previously an icon/script, restore it
                            model.properties.isActive = model.properties.icon != null ? true : false;
                            Invalidate();
                        }
                        else
                        {
                            model.properties.icon = ((SectorArcButton)e.Source).model.properties.icon; //TODO: Ensure icon data are copied to new properties
                            model.properties.rhinoScript = ((SectorArcButton)e.Source).model.properties.rhinoScript;
                            model.properties.isActive = true;
                            Invalidate();
                        }
                        break;
                    case DragSourceTypes.rhinoItem:
                        var rhinoToolbarItem = getDroppedToolbarItem(e);
                        model.properties.icon = rhinoToolbarItem.icon;
                        model.properties.rhinoScript = rhinoToolbarItem.script;
                        model.properties.isActive = true;
                        Invalidate();
                        break;
                    default:
                        break;
                }
                onButtonNewIconAdded?.Invoke(this);
            }
            updateIcon(); // Update icon image and position
        }

        /// <summary>
        /// Get type of object dragged into this control
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private DragSourceTypes dragSourceType(Control source)
        {
            if (source.GetType() == typeof(SectorArcButton))
            {
                return DragSourceTypes.self;
            }
            else if (source.GetType().ToString() == "Rhino.UI.Internal.TabPanels.Controls.ToolBarControlItem")
            {
                return DragSourceTypes.rhinoItem;
            }
            else
            {
                return DragSourceTypes.unknown;
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
                    var icon = (Eto.Drawing.Icon)iconCreateMethod?.Invoke(lMacro, new object[] { new Eto.Drawing.Size(28, 28), true });
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

        /// <summary>
        /// Update icon display and position
        /// </summary>
        private void updateIcon()
        {
            // Update icon image. set to "null" if icon shouldn't be displayed
            if (model.properties.isActive) buttons[buttonsTypeName.icon].setImage(model.properties.icon); else buttons[buttonsTypeName.icon].setImage(null);

            // if icon exists, update its position
            if (model.properties.icon != null && model.properties.isActive)
            {
                // Compute icon position in layout
                var arcCenterWorld = model.sectorData.sectorCenter();
                var arcCenterLocal = model.sectorData.convertWorldToLocal(arcCenterWorld);
                var posX = arcCenterLocal.X - (model.properties.icon.Size.Width / 2);
                var posY = arcCenterLocal.Y - (model.properties.icon.Size.Height / 2);
                Move(buttons[buttonsTypeName.icon], (int)posX, (int)posY); // update icon location
            }
        }
    }
}