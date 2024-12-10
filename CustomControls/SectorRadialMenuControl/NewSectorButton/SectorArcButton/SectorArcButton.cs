using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using AppKit;
using Eto.Drawing;
using Eto.Forms;
using Rhino;

namespace customControls
{
    public class SectorArcButton : PixelLayout
    {
        #region Events declaration
        public class DropTargetArgs
        {
            public DragSourceTypes sourceType;
            public DragEventArgs dragEventArgs;
            public bool acceptTarget = true;
            public DropTargetArgs(DragSourceTypes sourceType, DragEventArgs d) : base()
            {
                this.sourceType = sourceType;
                dragEventArgs = d;
            }
        }
        /// <summary>
        /// Button click event
        /// </summary>
        public event buttonClickEvent onButtonClickEvent;
        public delegate void buttonClickEvent(SectorArcButton sender);

        /// <summary>
        /// Event for requesting radial menu to get focus
        /// </summary>
        public event buttonRequestFocus onButtonRequestFocusEvent;
        public delegate void buttonRequestFocus(SectorArcButton sender);

        /// <summary>
        /// Event to notify an icon has been added to button
        /// </summary>
        public event onDragDrop onButtonDragDrop;
        public delegate void onDragDrop(SectorArcButton sender, DropTargetArgs args);
        /// <summary>
        /// Event to notify an icon has been added to button
        /// </summary>
        public event onDragEnter onButtonDragEnter;
        public delegate void onDragEnter(SectorArcButton sender, DropTargetArgs args);
        /// <summary>
        /// Event to notify an icon has been added to button
        /// </summary>
        public event onDragOver onButtonDragOver;
        public delegate void onDragOver(SectorArcButton sender, DropTargetArgs args);
        /// <summary>
        /// Event to notify an icon has been added to button
        /// </summary>
        public event onDragLeave onButtonDragLeave;
        public delegate void onDragLeave(SectorArcButton sender, DropTargetArgs args);
        /// <summary>
        /// Event to accept/reject drop target
        /// </summary>
        public event buttonAcceptTarget onButtonAcceptTarget;
        public delegate void buttonAcceptTarget(SectorArcButton sender, DropTargetArgs args);

        /// <summary>
        /// Event to notify mouse is over button
        /// </summary>
        public event buttonMouseOver onButtonMouseOverButton;
        public delegate void buttonMouseOver(SectorArcButton sender);
        /// <summary>
        /// Event to notify mouse leaves a button
        /// </summary>
        public event buttonMouseLeaveButton onbuttonMouseLeaveButton;
        public delegate void buttonMouseLeaveButton(SectorArcButton sender);

        /// <summary>
        /// Event to notify mouse enters a button
        /// </summary>
        public event buttonMouseEnterButton onbuttonMouseEnterButton;
        public delegate void buttonMouseEnterButton(SectorArcButton sender);
        #endregion

        #region Button States
        public struct ButtonStates : INotifyPropertyChanged
        {

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string info)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(info));
                }
            }

            /// <summary>
            /// custom "isVisible" property. We use this because using ETO "Visible" property prevents control to correctly update the NSTrackingArea
            /// @see https://github.com/picoe/Eto/issues/2704
            /// </summary>
            public bool isVisible = true;

            private bool _isDraggingIcon = false;
            private bool _isHovering = false;
            private bool _isSelected = false;

            public bool isHovering
            {
                get { return _isHovering; }
                set
                {
                    // Change class property value and notify change 
                    _isHovering = value;
                    OnPropertyChanged(nameof(isHovering));
                }
            }


            public bool isSelected
            {
                get { return _isSelected; }
                set
                {
                    // Change class property value and notify change 
                    _isSelected = value;
                    OnPropertyChanged(nameof(isSelected));
                }
            }

            public bool isDraggingIcon
            {
                get => _isDraggingIcon;
                set
                {
                    _isDraggingIcon = value;
                    OnPropertyChanged(nameof(isDraggingIcon));
                }
            }
            public ButtonStates() { }
        }
        #endregion

        #region Button animations
        /// <summary>
        /// Duration of animations
        /// </summary>
        private double animationDuration = 0.5;

        /// <summary>
        /// Transparency for button disable state
        /// </summary>
        private float disabledAlpha = (float)0.1;

        /// <summary>
        /// Transparency of buttons
        /// </summary>
        private float buttonAlpha = (float)0.4;

        private void animateHoverEffect()
        {
            // Console.SetOut(RhinoApp.CommandLineOut);
            // Console.WriteLine($"Animate hover btn ${ID} -- mouse hover ${states.isHovering} -- selected ${states.isSelected}");
            NSAnimationContext.RunAnimation(
                (context) =>
                {
                    context.Duration = animationDuration;
                    context.AllowsImplicitAnimation = true;
                    if (!states.isHovering)
                    {
                        buttons[buttonsTypeName.over]._nsViewObject.AlphaValue = 0;
                        buttons[buttonsTypeName.normal]._nsViewObject.AlphaValue = Enabled ? (states.isSelected ? 0 : buttonAlpha) : 0;
                        buttons[buttonsTypeName.selected]._nsViewObject.AlphaValue = Enabled ? (states.isSelected ? buttonAlpha : 0) : 0;
                        buttons[buttonsTypeName.disabled]._nsViewObject.AlphaValue = Enabled ? 0 : disabledAlpha;
                    }
                    else
                    {
                        buttons[buttonsTypeName.over]._nsViewObject.AlphaValue = buttonAlpha;
                        buttons[buttonsTypeName.normal]._nsViewObject.AlphaValue = 0;
                        buttons[buttonsTypeName.selected]._nsViewObject.AlphaValue = 0;
                        buttons[buttonsTypeName.disabled]._nsViewObject.AlphaValue = 0;
                    }
                });
        }
        private void animateSelectedEffect()
        {
            NSAnimationContext.RunAnimation(
                (context) =>
                {
                    context.Duration = animationDuration;
                    context.AllowsImplicitAnimation = true;
                    buttons[buttonsTypeName.over]._nsViewObject.AlphaValue = 0;
                    buttons[buttonsTypeName.normal]._nsViewObject.AlphaValue = states.isSelected ? 0 : buttonAlpha;
                    buttons[buttonsTypeName.selected]._nsViewObject.AlphaValue = states.isSelected ? buttonAlpha : 0;
                    buttons[buttonsTypeName.disabled]._nsViewObject.AlphaValue = 0;
                });
        }
        private void animateDisableEffect()
        {
            NSAnimationContext.RunAnimation(
                (context) =>
                {
                    context.Duration = animationDuration;
                    context.AllowsImplicitAnimation = true;
                    buttons[buttonsTypeName.normal]._nsViewObject.AlphaValue = 0;
                    buttons[buttonsTypeName.over]._nsViewObject.AlphaValue = 0;
                    buttons[buttonsTypeName.selected]._nsViewObject.AlphaValue = 0;
                    buttons[buttonsTypeName.disabled]._nsViewObject.AlphaValue = disabledAlpha;
                });
        }
        private void animateEnableEffect()
        {
            NSAnimationContext.RunAnimation(
                (context) =>
                {
                    context.Duration = animationDuration;
                    context.AllowsImplicitAnimation = true;
                    if (states.isHovering)
                    {
                        buttons[buttonsTypeName.normal]._nsViewObject.AlphaValue = 0;
                        buttons[buttonsTypeName.over]._nsViewObject.AlphaValue = buttonAlpha;
                        buttons[buttonsTypeName.selected]._nsViewObject.AlphaValue = 0;
                        buttons[buttonsTypeName.disabled]._nsViewObject.AlphaValue = 0;
                    }
                    else
                    {
                        buttons[buttonsTypeName.normal]._nsViewObject.AlphaValue = states.isSelected ? 0 : buttonAlpha;
                        buttons[buttonsTypeName.over]._nsViewObject.AlphaValue = 0;
                        buttons[buttonsTypeName.selected]._nsViewObject.AlphaValue = states.isSelected ? buttonAlpha : 0;
                        buttons[buttonsTypeName.disabled]._nsViewObject.AlphaValue = 0;
                    }
                });
        }
        #endregion

        #region Button Properties
        public new string ID
        {
            get { return base.ID; }
            set
            {
                base.ID = value;
                model.buttonID = value;// We need to sets model buttonID to retrieve Plugin settings
            }
        }

        public override bool Enabled
        {
            get => Handler.Enabled;
            set
            {
                Handler.Enabled = value;
                if (value == false) animateDisableEffect(); else animateEnableEffect();
            }
        }
        protected enum buttonsTypeName
        {
            normal = 0,
            over = 1,
            selected = 2,
            disabled = 3,
            icon = 4,
        }
        /// <summary>
        /// Drawables used for animating button UI state changes
        /// </summary>
        protected Dictionary<buttonsTypeName, ImageButton> buttons = new Dictionary<buttonsTypeName, ImageButton>();

        /// <summary>
        /// Data Model Binding
        /// </summary>
        ButtonModelData model = new ButtonModelData();
        public ButtonStates states = new ButtonStates();

        public BindableBinding<SectorArcButton, ButtonModelData> ButtonModelBinding => new BindableBinding<SectorArcButton, ButtonModelData>(
            this,
            (SectorArcButton obj) => obj.model,
            // Update "model" property with new value and register a property changed event handler of the "model" object
            delegate (SectorArcButton obj, ButtonModelData value)
            {
                // Remove property changed event handler on current "model" object
                model.PropertyChanged -= modelChangedHandler;
                model.properties.PropertyChanged -= modelChangedHandler;
                // update property
                obj.model = value;
                // Add property changed handler on "model"
                model.PropertyChanged += modelChangedHandler;
                model.properties.PropertyChanged += modelChangedHandler;
            },
            // Add change event handler
            delegate (SectorArcButton btn, EventHandler<EventArgs> changeEventHandler)
            {
                var a = 0;
            },
            // remove change event handler
            delegate (SectorArcButton btn, EventHandler<EventArgs> changeEventHandler)
            {
                var a = 0;

            });

        #endregion


        /// <summary>
        /// Custom and override of ID to generate button ID
        /// </summary>
        // public new string ID { get { return model.buttonID; } set { base.ID = value; } }

        public SectorArcButton() : base()
        {
            var w = Stopwatch.StartNew();

            // Create buttons
            buttons[buttonsTypeName.normal] = new ImageButton();
            buttons[buttonsTypeName.over] = new ImageButton();
            buttons[buttonsTypeName.disabled] = new ImageButton();
            buttons[buttonsTypeName.icon] = new ImageButton();
            buttons[buttonsTypeName.selected] = new ImageButton();

            Size = model.sectorData.size;

            // Add button to layout
            Add(buttons[buttonsTypeName.normal], 0, 0);
            Add(buttons[buttonsTypeName.over], 0, 0);
            Add(buttons[buttonsTypeName.disabled], 0, 0);
            Add(buttons[buttonsTypeName.selected], 0, 0);
            Add(buttons[buttonsTypeName.icon], 0, 0);



            buttons[buttonsTypeName.over]._nsViewObject.AlphaValue = 0;
            buttons[buttonsTypeName.disabled]._nsViewObject.AlphaValue = 0;
            buttons[buttonsTypeName.selected]._nsViewObject.AlphaValue = 0;
            buttons[buttonsTypeName.normal]._nsViewObject.AlphaValue = buttonAlpha;


            // Mouse events
            MouseMove += mouseMoveHandler;
            MouseLeave += mouseLeaveHandler;
            MouseDown += mouseDownHandler;

            // DragDrop events
            AllowDrop = true;
            DragEnter += dragEnterHandler;
            DragOver += dragOverHandler;
            DragLeave += dragLeaveHandler;
            DragDrop += dragDropHandler;

            // States property change handler
            states.PropertyChanged += (obj, prop) =>
            {
                switch (prop.PropertyName)
                {
                    case nameof(states.isSelected): // Animate <selected> property changed
                        animateSelectedEffect();
                        break;
                    default: break;
                }
            };

            w.Stop();
            Console.SetOut(RhinoApp.CommandLineOut);
            Console.WriteLine("SectorArcButton takes:" + w.ElapsedMilliseconds);
        }

        /// <summary>
        /// Update button display as soon as a new Model is binded to this class. When occurs, the "model" property has already been updated, so we can use it
        /// <para>
        /// REMARK: Don't think we need to optimize props update by checking wich model property is updated. So we systematically update all
        /// </para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void modelChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ButtonModelData.sectorData):
                    // Update self data
                    Size = model.sectorData.size;
                    // Update buttons image
                    buttons[buttonsTypeName.normal].setImage(model.sectorData.images.normalStateImage, model.sectorData.size);
                    buttons[buttonsTypeName.over].setImage(model.sectorData.images.overStateImage, model.sectorData.size);
                    buttons[buttonsTypeName.disabled].setImage(model.sectorData.images.disabledStateImage, model.sectorData.size);
                    buttons[buttonsTypeName.selected].setImage(model.sectorData.images.selectedStateImage, model.sectorData.size);
                    buttons[buttonsTypeName.icon].setImage(model.properties.icon, model.sectorData.size);
                    // Update Rhino icon
                    updateIcon();
                    break;
                case nameof(ButtonModelData.properties):
                case nameof(ButtonProperties.icon):
                case nameof(ButtonProperties.isActive):
                    buttons[buttonsTypeName.icon].setImage(model.properties.icon, model.sectorData.size);
                    // Update Rhino icon
                    updateIcon();
                    break;
                default: break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouseDownHandler(object sender, MouseEventArgs e)
        {
            if (!states.isDraggingIcon)
            {
                // We can drag icons with "command" modifier + LMB => Drag is forbidden if button is a "folder"
                if (e.Modifiers == Keys.Application)
                {
                    if (model.properties.isActive && model.properties.isFolder == false)
                    {
                        DataObject eventObj = new DataObject();
                        DoDragDrop(eventObj, DragEffects.All, model.properties.icon, new PointF(10, 10));
                    }
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
        /// Handler for mouse move in control. Note that we check and fire custom mouse "leave", "over" and "enter" events here because
        /// the shape is not a rectangle. So we want to raise events only for the custom (arc) shape
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouseMoveHandler(object sender, MouseEventArgs e)
        {
            // Console.SetOut(RhinoApp.CommandLineOut);
            // Console.WriteLine($"Button mouse move {ID}");
            if (states.isVisible == true)
            {
                // Ensure main plugin window has focus -> workaround for click event that doesn't work if main window has no focus
                // If main window has no focus, the click event gives focus to main window and no click event on button occurs
                onButtonRequestFocusEvent?.Invoke(this);
                var oldHovering = mouseMoveUpdate(e);

                if (states.isHovering)
                { // Mouse is over the button
                    if (oldHovering) // Mouse was already over the button -> Invoke event mouse over
                    {
                        onButtonMouseOverButton?.Invoke(this); // Notify mouse is over the button
                    }
                    else // Mouse enters the button
                    {
                        animateHoverEffect();
                        onbuttonMouseEnterButton?.Invoke(this);
                    }
                }
                else // Mouse is not over the button
                {
                    if (oldHovering) // mouse was over the button -> Invoke leave event
                    {
                        animateHoverEffect();
                        onbuttonMouseLeaveButton?.Invoke(this);
                    }
                }
            }
        }


        // TODO: Check if it is relevant as leave event is handled in "onMouseMove" event
        private void mouseLeaveHandler(object sender, MouseEventArgs e)
        {
            if (states.isHovering) // Avoid sending "leave" event twice
            {
                states.isHovering = false;
                onbuttonMouseLeaveButton?.Invoke(this);
                animateHoverEffect();
            }
        }

        private void dragEnterHandler(object sender, DragEventArgs e)
        {
            // Console.SetOut(RhinoApp.CommandLineOut);
            // Console.WriteLine($"Button drag enter {ID} -- original event");
            // states.isDraggingIcon = true;
            // Ensure mouse cursor if hovering arc sector
            // OnMouseMove(new MouseEventArgs(e.Buttons, e.Modifiers, e.Location));

            // var oldHovering = mouseMoveUpdate(new MouseEventArgs(e.Buttons, e.Modifiers, e.Location, null));
            // if (oldHovering == false) // Mouse was not over the button
            // {
            //     if (states.isHovering == true)
            //     {
            //         var sourcetype = dragSourceType(e.Source);
            //         if (sourcetype == DragSourceTypes.self)
            //         {
            //             if (((SectorArcButton)e.Source).ID == ID)
            //             {
            //                 e.Effects = DragEffects.Link;
            //             }
            //         }
            //         onButtonDragEnter?.Invoke(this, new DropTargetArgs(sourcetype, e));
            //     }
            //     else
            //     {
            //         states.isDraggingIcon = false;
            //     }
            // }
        }
        /// <summary>
        /// Drag leave handler. As soon drag leaves frame rectangle, we are sure drag exit button.
        /// NOTE that we have to check the "leave" state is not already fired by @dragOverHandler method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dragLeaveHandler(object sender, DragEventArgs e)
        {
            states.isHovering = false;

            // If not already "leaved" from "dragOverHandler" method, fires event "leave"
            if (states.isDraggingIcon)
            {
                var args = new DropTargetArgs(dragSourceType(e.Source), e);
                onButtonDragLeave?.Invoke(this, args);
                states.isDraggingIcon = false;
            }
            states.isDraggingIcon = false;
            // Console.SetOut(RhinoApp.CommandLineOut);
            // Console.WriteLine($"Button drag leave {ID} -- real event");
            // states.isDraggingIcon = false;
            // // Ensure mouse cursor if leaving arc sector
            // // OnMouseLeave(new MouseEventArgs(e.Buttons, e.Modifiers, e.Location));
            // var oldHovering = mouseMoveUpdate(new MouseEventArgs(e.Buttons, e.Modifiers, e.Location));
            // e.Effects = DragEffects.All;
            // var dSource = dragSourceType(e.Source);
            // if (oldHovering) // Mouse was over the button
            // {
            //     if (!states.isHovering) // Mouse is now not over the button -> Fires drag leave
            //     {
            //         e.Effects = DragEffects.All;
            //         onButtonDragLeave?.Invoke(this, new DropTargetArgs(dSource, e));
            //     }
            //     else // Mouse is over the button
            //     {
            //         states.isDraggingIcon = true;
            //     }
            // }

            // if (dSource == DragSourceTypes.self && ((SectorArcButton)e.Source).ID == ID)
            // {
            //     model.properties.isActive = false;
            //     updateIcon();
            // }
        }
        private void dragOverHandler(object sender, DragEventArgs e)
        {
            // Ensure mouse cursor if hovering arc sector
            var oldHovering = states.isHovering ? true : false;
            mouseMoveUpdate(new MouseEventArgs(e.Buttons, e.Modifiers, e.Location));
            if (!states.isHovering) // Mouse is not over the button
            {
                states.isDraggingIcon = false;
                if (oldHovering == true)
                {
                    // Console.SetOut(RhinoApp.CommandLineOut);
                    // Console.WriteLine($"Button drag leave {ID}");
                    var args = new DropTargetArgs(dragSourceType(e.Source), e);
                    onButtonDragLeave?.Invoke(this, args);
                    states.isDraggingIcon = false;
                }
            }
            else // Mouse is over the button -> Fires drop accept target & drag move event
            {
                states.isDraggingIcon = true;
                if (oldHovering == false) // Drag enter as mouse was not over the button
                {
                    // Console.SetOut(RhinoApp.CommandLineOut);
                    // Console.WriteLine($"Button drag Enter {ID}");
                    states.isDraggingIcon = true;
                    onButtonDragEnter?.Invoke(this, new DropTargetArgs(dragSourceType(e.Source), e));
                }
                else
                {
                    // Console.SetOut(RhinoApp.CommandLineOut);
                    // Console.WriteLine($"Button drag over {ID}");
                    var args = new DropTargetArgs(dragSourceType(e.Source), e);
                    onButtonAcceptTarget?.Invoke(this, args);
                    onButtonDragOver?.Invoke(this, new DropTargetArgs(dragSourceType(e.Source), e));
                    e.Effects = args.acceptTarget ? DragEffects.Copy : DragEffects.None;
                }
            }
        }
        private void dragDropHandler(object sender, DragEventArgs e)
        {
            // Ensure mouse cursor if hovering arc sector
            // OnMouseMove(new MouseEventArgs(e.Buttons, e.Modifiers, e.Location));
            var oldHovereing = mouseMoveUpdate(new MouseEventArgs(e.Buttons, e.Modifiers, e.Location));

            if (states.isHovering)
            {
                var sourcetype = dragSourceType(e.Source);
                switch (sourcetype)
                {
                    case DragSourceTypes.self:
                        if (((SectorArcButton)e.Source).ID == ID)
                        {
                            // If drag drop and button had previously an icon/script, restore it
                            model.properties.isActive = model.properties.icon != null ? true : false;
                        }
                        else
                        {
                            model.properties.icon = ((SectorArcButton)e.Source).model.properties.icon; //TODO: Ensure icon data are copied to new properties
                            model.properties.rhinoScript = ((SectorArcButton)e.Source).model.properties.rhinoScript;
                            model.properties.isActive = true;
                        }
                        break;
                    case DragSourceTypes.rhinoItem:
                        var rhinoToolbarItem = getDroppedToolbarItem(e);
                        model.properties.icon = rhinoToolbarItem.icon;
                        model.properties.rhinoScript = rhinoToolbarItem.script;
                        model.properties.isActive = true;
                        break;
                    default:
                        break;
                }
                onButtonDragDrop?.Invoke(this, new DropTargetArgs(sourcetype, e));
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
        /// <summary>
        /// Update button Hovering state when mouse move over a button.
        /// Return the old "hovering" state to compare with new state
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool mouseMoveUpdate(MouseEventArgs e)
        {
            var oldHovering = states.isHovering ? true : false;

            if (states.isVisible == true)
            {
                var new_isHovering = model.sectorData.isPointInShape(e.Location);

                if (new_isHovering)  // Mouse is over the button
                {
                    if (states.isHovering) // Mouse was already over the button
                    {
                    }
                    else // Mouse enters the button
                    {
                        states.isHovering = true;
                        animateHoverEffect();
                    }
                }
                else // Mouse is not over the button
                {
                    if (states.isHovering) // mouse was over the button
                    {
                        states.isHovering = false;
                        animateHoverEffect();
                    }
                }
            }
            return oldHovering;
        }
    }
}