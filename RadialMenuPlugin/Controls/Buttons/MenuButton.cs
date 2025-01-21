using System;
using System.Collections.Generic;
using System.ComponentModel;
using AppKit;
using Eto.Drawing;
using Eto.Forms;
using RadialMenuPlugin.Data;
using RadialMenuPlugin.Utilities.Events;

namespace RadialMenuPlugin.Controls.Buttons.MenuButton
{
    #region Event Handler and Args classes
    using DragDropHandler = AppEventHandler<MenuButton, DragEventArgs>; // Used for dragdrop events
    using DragDropStartHandler = AppEventHandler<MenuButton>; // Used for DoDrag start event
    using MouseEventHandler = AppEventHandler<MenuButton, MouseEventArgs>; // Used for mouse events

    /// <summary>
    /// 
    /// </summary>
    public class DropTargetArgs : DragEventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public bool acceptTarget = true;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        public DropTargetArgs(DragEventArgs d) : base(d.Source, d.Data, d.AllowedEffects, d.Location, d.Modifiers, d.Buttons, d.ControlObject)
        { }
    }
    #endregion

    /// <summary>
    /// Menu button class
    /// </summary>
    public class MenuButton : PixelLayout
    {
        #region Events declaration
        /// <summary>
        /// Button click event
        /// </summary>
        public event MouseEventHandler OnButtonClickEvent;
        /// <summary>
        /// Event to notify an icon has been added to button
        /// </summary>
        public event DragDropHandler OnButtonDragDrop;
        /// <summary>
        /// Event to notify an icon has been added to button
        /// </summary>
        public event DragDropHandler OnButtonDragEnter;
        /// <summary>
        /// Event to notify an icon has been added to button
        /// </summary>
        public event DragDropHandler OnButtonDragOver;
        /// <summary>
        /// Event to notify an icon has been added to button
        /// </summary>
        public event DragDropHandler OnButtonDragLeave;
        /// <summary>
        /// Event to notify to start button icon drag
        /// </summary>
        public event DragDropStartHandler OnButtonDragDropStart;
        /// <summary>
        /// Event to notify button icon drag ended
        /// </summary>
        public event DragDropHandler OnButtonDragDropEnd;
        /// <summary>
        /// Event to notify mouse is over button
        /// </summary>
        public event MouseEventHandler OnButtonMouseMove;
        /// <summary>
        /// Event to notify mouse leaves a button
        /// </summary>
        public event MouseEventHandler OnButtonMouseLeave;
        /// <summary>
        /// Event to notify mouse enters a button
        /// </summary>
        public event MouseEventHandler OnButtonMouseEnter;
        /// <summary>
        /// Event to notify button request showing context menu
        /// </summary>
        public event MouseEventHandler OnButtonContextMenu;
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
            public bool IsVisible = true;

            private bool _IsHovering = false;
            private bool _IsSelected = false;
            private bool _IsEditMode = false;

            public bool IsHovering
            {
                get { return _IsHovering; }
                set
                {
                    // Change class property value and notify change 
                    _IsHovering = value;
                    OnPropertyChanged(nameof(IsHovering));
                }
            }

            public bool IsEditMode
            {
                get => _IsEditMode;
                set
                {
                    _IsEditMode = value;
                    OnPropertyChanged(nameof(IsEditMode));
                }
            }
            public bool IsSelected
            {
                get { return _IsSelected; }
                set
                {
                    // Change class property value and notify change 
                    _IsSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }

            public ButtonStates() { }
        }
        #endregion
        #region Button animations
        /// <summary>
        /// Duration of animations
        /// </summary>
        private double _AnimationDuration = 0.5;

        /// <summary>
        /// Transparency for button disable state
        /// </summary>
        private float _DisabledAlpha = (float)0.1;

        /// <summary>
        /// Transparency of buttons
        /// </summary>
        private float _ButtonAlpha = (float)0.4;

        private void _AnimateHoverEffect()
        {
            // Console.SetOut(RhinoApp.CommandLineOut);
            // Console.WriteLine($"Animate hover btn ${ID} -- mouse hover ${states.isHovering} -- selected ${states.isSelected}");
            NSAnimationContext.RunAnimation(
                (context) =>
                {
                    context.Duration = _AnimationDuration;
                    context.AllowsImplicitAnimation = true;
                    if (!States.IsHovering)
                    {
                        _Buttons[_ButtonType.over].NsViewObject.AlphaValue = 0;
                        _Buttons[_ButtonType.normal].NsViewObject.AlphaValue = Enabled ? (States.IsSelected ? 0 : _ButtonAlpha) : 0;
                        _Buttons[_ButtonType.selected].NsViewObject.AlphaValue = Enabled ? (States.IsSelected ? _ButtonAlpha : 0) : 0;
                        _Buttons[_ButtonType.disabled].NsViewObject.AlphaValue = Enabled ? 0 : _DisabledAlpha;
                    }
                    else
                    {
                        _Buttons[_ButtonType.over].NsViewObject.AlphaValue = _ButtonAlpha;
                        _Buttons[_ButtonType.normal].NsViewObject.AlphaValue = 0;
                        _Buttons[_ButtonType.selected].NsViewObject.AlphaValue = 0;
                        _Buttons[_ButtonType.disabled].NsViewObject.AlphaValue = 0;
                    }
                    _Buttons[_ButtonType.editmode].NsViewObject.AlphaValue = States.IsEditMode ? 1 : 0;
                });
        }
        private void _AnimateSelectedEffect()
        {
            NSAnimationContext.RunAnimation(
                (context) =>
                {
                    context.Duration = _AnimationDuration;
                    context.AllowsImplicitAnimation = true;
                    _Buttons[_ButtonType.over].NsViewObject.AlphaValue = 0;
                    _Buttons[_ButtonType.normal].NsViewObject.AlphaValue = States.IsSelected ? 0 : _ButtonAlpha;
                    _Buttons[_ButtonType.selected].NsViewObject.AlphaValue = States.IsSelected ? _ButtonAlpha : 0;
                    _Buttons[_ButtonType.disabled].NsViewObject.AlphaValue = 0;
                    _Buttons[_ButtonType.editmode].NsViewObject.AlphaValue = States.IsEditMode ? 1 : 0;
                });
        }
        private void _AnimateDisableEffect()
        {
            NSAnimationContext.RunAnimation(
                (context) =>
                {
                    context.Duration = _AnimationDuration;
                    context.AllowsImplicitAnimation = true;
                    _Buttons[_ButtonType.normal].NsViewObject.AlphaValue = 0;
                    _Buttons[_ButtonType.over].NsViewObject.AlphaValue = 0;
                    _Buttons[_ButtonType.selected].NsViewObject.AlphaValue = 0;
                    _Buttons[_ButtonType.disabled].NsViewObject.AlphaValue = _DisabledAlpha;
                    _Buttons[_ButtonType.editmode].NsViewObject.AlphaValue = States.IsEditMode ? _ButtonAlpha : 0;
                });
        }
        private void _AnimateEnableEffect()
        {
            NSAnimationContext.RunAnimation(
                (context) =>
                {
                    context.Duration = _AnimationDuration;
                    context.AllowsImplicitAnimation = true;
                    if (States.IsHovering)
                    {
                        _Buttons[_ButtonType.normal].NsViewObject.AlphaValue = 0;
                        _Buttons[_ButtonType.over].NsViewObject.AlphaValue = _ButtonAlpha;
                        _Buttons[_ButtonType.selected].NsViewObject.AlphaValue = 0;
                        _Buttons[_ButtonType.disabled].NsViewObject.AlphaValue = 0;
                        _Buttons[_ButtonType.editmode].NsViewObject.AlphaValue = States.IsEditMode ? 1 : 0;
                    }
                    else
                    {
                        _Buttons[_ButtonType.normal].NsViewObject.AlphaValue = States.IsSelected ? 0 : _ButtonAlpha;
                        _Buttons[_ButtonType.over].NsViewObject.AlphaValue = 0;
                        _Buttons[_ButtonType.selected].NsViewObject.AlphaValue = States.IsSelected ? _ButtonAlpha : 0;
                        _Buttons[_ButtonType.disabled].NsViewObject.AlphaValue = 0;
                        _Buttons[_ButtonType.editmode].NsViewObject.AlphaValue = States.IsEditMode ? 1 : 0;
                    }
                });
        }
        private void _AnimateEditmode()
        {
            NSAnimationContext.RunAnimation(
                (context) =>
                {
                    context.Duration = _AnimationDuration;
                    context.AllowsImplicitAnimation = true;
                    _Buttons[_ButtonType.normal].NsViewObject.AlphaValue = _ButtonAlpha;
                    _Buttons[_ButtonType.over].NsViewObject.AlphaValue = 0;
                    _Buttons[_ButtonType.selected].NsViewObject.AlphaValue = 0;
                    _Buttons[_ButtonType.disabled].NsViewObject.AlphaValue = 0;
                    _Buttons[_ButtonType.editmode].NsViewObject.AlphaValue = States.IsEditMode ? 1 : 0;

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
                _Model.ButtonID = value;// We need to sets model buttonID to retrieve Plugin settings
            }
        }

        public override bool Enabled
        {
            get => Handler.Enabled;
            set
            {
                Handler.Enabled = value;
                if (value == false) _AnimateDisableEffect(); else _AnimateEnableEffect();
            }
        }
        protected enum _ButtonType
        {
            normal = 0,
            over = 1,
            selected = 2,
            disabled = 3,
            icon = 4,
            folderIcon = 5,
            editmode = 6,
            trigger = 7
        }

        protected SectorData _SectorData = new SectorData();
        public SectorData SectorData
        {
            get => _SectorData;
            set
            {
                _UpdateSectorData(value);
                _SectorData = value;
            }
        }
        /// <summary>
        /// Drawables used for animating button UI state changes
        /// </summary>
        protected Dictionary<_ButtonType, ImageButton> _Buttons = new Dictionary<_ButtonType, ImageButton>();

        /// <summary>
        /// Data Model Binding
        /// </summary>
        ButtonModelData _Model = new ButtonModelData();
        public ButtonStates States = new ButtonStates();

        public BindableBinding<MenuButton, ButtonModelData> ButtonModelBinding => new BindableBinding<MenuButton, ButtonModelData>(
            this,
            (MenuButton obj) => obj._Model,
            // Update "model" property with new value and register a property changed event handler of the "model" object
            delegate (MenuButton obj, ButtonModelData value)
            {
                // Remove property changed event handler on current "model" object
                _Model.PropertyChanged -= _ModelChangedHandler;
                _Model.Properties.PropertyChanged -= _ModelChangedHandler;
                // update property
                obj._Model = value;
                // Add property changed handler on "model"
                _Model.PropertyChanged += _ModelChangedHandler;
                _Model.Properties.PropertyChanged += _ModelChangedHandler;
                _UpdateIcon();
                _UpdateTriggerIcon();
            },
            // Add change event handler
            delegate (MenuButton btn, EventHandler<EventArgs> changeEventHandler)
            { },
            // remove change event handler
            delegate (MenuButton btn, EventHandler<EventArgs> changeEventHandler)
            { });

        #endregion
        /// <summary>
        /// Custom and override of ID to generate button ID
        /// </summary>
        // public new string ID { get { return model.buttonID; } set { base.ID = value; } }
        public MenuButton() : base()
        {
            // var w = Stopwatch.StartNew();
            Size = _SectorData.Size;
            _InitButtons();
            _InitEventHandlers();
            _InitStatesChangeHandler();
            // w.Stop();
            // Console.SetOut(RhinoApp.CommandLineOut);
            // Console.WriteLine("SectorArcButton takes:" + w.ElapsedMilliseconds);
        }
        protected void _InitStatesChangeHandler()
        {
            // States property change handler
            States.PropertyChanged += (obj, prop) =>
            {
                switch (prop.PropertyName)
                {
                    case nameof(States.IsSelected): // Animate <selected> property changed
                        _AnimateSelectedEffect();
                        break;
                    case nameof(States.IsEditMode):
                        _UpdateIconEditmode();
                        _AnimateEditmode();
                        break;
                    case nameof(States.IsHovering):
                        _AnimateHoverEffect();
                        break;
                    default: break;
                }
            };
        }
        /// <summary>
        /// Init layout buttons
        /// </summary>
        protected void _InitButtons()
        {
            // Create buttons
            _Buttons[_ButtonType.normal] = new ImageButton();
            _Buttons[_ButtonType.over] = new ImageButton();
            _Buttons[_ButtonType.disabled] = new ImageButton();
            _Buttons[_ButtonType.icon] = new ImageButton();
            _Buttons[_ButtonType.selected] = new ImageButton();
            _Buttons[_ButtonType.trigger] = new ImageButton();
            // edit mode image
            var iconSize = new Size(44, 44);
            var img = Bitmap.FromResource("RadialMenu.Bitmaps.dashed-circle.png").WithSize(iconSize);
            _Buttons[_ButtonType.editmode] = new ImageButton(img, iconSize);
            // folder icon image
            var folderIconSize = new Size(16, 16);
            var folderIconimg = Bitmap.FromResource("RadialMenu.Bitmaps.plus_icon.png").WithSize(folderIconSize);
            _Buttons[_ButtonType.folderIcon] = new ImageButton(folderIconimg, folderIconSize);

            // Add button to layout
            Add(_Buttons[_ButtonType.normal], 0, 0);
            Add(_Buttons[_ButtonType.over], 0, 0);
            Add(_Buttons[_ButtonType.disabled], 0, 0);
            Add(_Buttons[_ButtonType.selected], 0, 0);
            Add(_Buttons[_ButtonType.icon], 0, 0);
            Add(_Buttons[_ButtonType.folderIcon], 0, 0);
            Add(_Buttons[_ButtonType.editmode], 0, 0);
            Add(_Buttons[_ButtonType.trigger], 0, 0);

            _Buttons[_ButtonType.over].NsViewObject.AlphaValue = 0;
            _Buttons[_ButtonType.disabled].NsViewObject.AlphaValue = 0;
            _Buttons[_ButtonType.selected].NsViewObject.AlphaValue = 0;
            _Buttons[_ButtonType.normal].NsViewObject.AlphaValue = _ButtonAlpha;
            _Buttons[_ButtonType.editmode].NsViewObject.AlphaValue = 0;
        }
        /// <summary>
        /// Init event handlers
        /// </summary>
        protected void _InitEventHandlers()
        {
            // Mouse events
            MouseMove += _MouseMoveHandler;
            MouseLeave += _MouseLeaveHandler;
            MouseDown += _MouseDownHandler;

            // DragDrop events for edit mode
            AllowDrop = false;
            _Buttons[_ButtonType.editmode].AllowDrop = true;
            _Buttons[_ButtonType.editmode].DragEnter += _DragEnterHandler;
            _Buttons[_ButtonType.editmode].DragOver += _DragOverHandler;
            _Buttons[_ButtonType.editmode].DragLeave += _DragLeaveHandler;
            _Buttons[_ButtonType.editmode].DragDrop += _DragDropHandler;
            DragEnd += (s, e) =>
            {
                if (e.Effects == DragEffects.None) // No drop target accepted the icon : We should remove the icon
                {
                    _RaiseEvent(OnButtonDragDropEnd, e);
                }
            };
        }
        /// <summary>
        /// Update button display as soon as a new Model is binded to this class. When occurs, the "model" property has already been updated, so we can use it
        /// <para>
        /// REMARK: Don't think we need to optimize props update by checking wich model property is updated. So we systematically update all
        /// </para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void _ModelChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ButtonModelData.Properties):
                case nameof(ButtonProperties.Icon):
                case nameof(ButtonProperties.IsActive):
                case nameof(ButtonProperties.IsFolder):
                    // _Buttons[_ButtonType.icon].SetImage(_Model.Properties.Icon, _SectorData.Size);
                    _UpdateIcon();
                    _UpdateTriggerIcon();
                    break;
                case nameof(ButtonProperties.Trigger):
                    _UpdateTriggerIcon();
                    break;
                default: break;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void _MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (!States.IsHovering) return; // As buttons can overlap, react only to button that has focus
            switch (e.Buttons)
            {
                case MouseButtons.Primary:
                    // We can drag icons with "command" modifier + LMB => Drag is forbidden if button is a "folder"
                    if (e.Modifiers == Keys.Application)
                    {
                        if (_Model.Properties.IsActive && States.IsEditMode)
                        {
                            _RaiseEvent(OnButtonDragDropStart); // Raise event to notify dragging start
                        }
                    }
                    else
                    {
                        if (_Model.Properties.IsActive && !States.IsEditMode) // Command can ONLY be executed when not in edit mode
                        {

                            _RaiseEvent(OnButtonClickEvent, e);
                        }
                    }
                    break;
                case MouseButtons.Alternate:
                    // Shows context menu panel in edit mode when right mouse button click
                    if (States.IsEditMode)
                    {
                        _RaiseEvent(OnButtonContextMenu, e);
                    }
                    else
                    {
                        _RaiseEvent(OnButtonClickEvent, e);
                    }

                    break;
            }
        }
        /// <summary>
        /// Handler for mouse move in control. Note that we check and fire custom mouse "leave", "over" and "enter" events here because
        /// the shape is not a rectangle. So we want to raise events only for the custom (arc) shape
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void _MouseMoveHandler(object sender, MouseEventArgs e)
        {
            // Console.SetOut(RhinoApp.CommandLineOut);
            // Console.WriteLine($"Button mouse move {ID}");
            if (States.IsVisible == true)
            {
                // Ensure main plugin window has focus -> workaround for click event that doesn't work if main window has no focus
                // If main window has no focus, the click event gives focus to main window and no click event on button occurs
                // OnButtonRequestFocusEvent?.Invoke(this,e);
                var oldHovering = _MouseMoveUpdate(e);

                if (States.IsHovering) // Mouse overs the button
                {
                    if (oldHovering) // Mouse was already over the button -> Invoke event mouse over
                    {
                        _RaiseEvent(OnButtonMouseMove, e); // Notify mouse is over the button
                    }
                    else // Mouse enters the button
                    {
                        _RaiseEvent(OnButtonMouseEnter, e);
                    }
                }
                else // Mouse is not over the button
                {
                    if (oldHovering) // mouse was over the button -> Invoke leave event
                    {
                        ToolTip = "";
                        _RaiseEvent(OnButtonMouseLeave, e);
                    }
                }
            }
        }
        /// <summary>
        /// TODO: Check if it is relevant as leave event is handled in "onMouseMove" event 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void _MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            if (States.IsHovering) // Avoid sending "leave" event twice
            {
                States.IsHovering = false;
                _RaiseEvent(OnButtonMouseLeave, e);
            }
        }
        protected void _DragEnterHandler(object sender, DragEventArgs e)
        {
            _RaiseEvent(OnButtonDragEnter, e);
            if (e.Effects != DragEffects.None)
            {
                States.IsHovering = true;
            }
        }
        /// <summary>
        /// Drag leave handler. As soon drag leaves frame rectangle, we are sure drag exit button.
        /// NOTE that we have to check the "leave" state is not already fired by @dragOverHandler method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void _DragLeaveHandler(object sender, DragEventArgs e)
        {
            _RaiseEvent(OnButtonDragLeave, e);
            States.IsHovering = false;
        }
        protected void _DragOverHandler(object sender, DragEventArgs e)
        {
            _RaiseEvent(OnButtonDragOver, e);
            if (e.Effects != DragEffects.None)
            {
                States.IsHovering = true;
            }
        }
        protected void _DragDropHandler(object sender, DragEventArgs e)
        {
            _RaiseEvent(OnButtonDragDrop, e);
            States.IsHovering = false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        protected void _RaiseEvent<T>(AppEventHandler<T> action) where T : MenuButton
        {
            action?.Invoke(this as T);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="e"></param>
        protected void _RaiseEvent<T, E>(AppEventHandler<T, E> action, E e) where T : MenuButton
        {
            switch (typeof(E).Name)
            {
                //
                // Drag/Drop events
                //
                case nameof(DragEventArgs):
                    // Raise event if we are in edit mode AND button is visible
                    // NOTE: Event with nsView alpha=0, the drag event is fired
                    if (States.IsEditMode && States.IsVisible)
                    {
                        if (action != null)
                        {
                            action.Invoke(this as T, e);
                        }
                    }
                    break;
                //
                // Mouse events
                //
                case nameof(MouseEventArgs):
                    action?.Invoke(this as T, e);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Update button Hovering state when mouse move over a button.
        /// Return the old "hovering" state to compare with new state
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected bool _MouseMoveUpdate(MouseEventArgs e)
        {
            var oldHovering = States.IsHovering ? true : false;

            if (States.IsVisible == true)
            {
                var new_isHovering = _SectorData.IsPointInShape(e.Location);

                if (new_isHovering)  // Mouse is over the button
                {
                    if (States.IsHovering) // Mouse was already over the button
                    {
                    }
                    else // Mouse enters the button
                    {
                        States.IsHovering = true;
                        _AnimateHoverEffect();
                    }
                }
                else // Mouse is not over the button
                {
                    if (States.IsHovering) // mouse was over the button
                    {
                        States.IsHovering = false;
                        _AnimateHoverEffect();
                    }
                }
            }
            return oldHovering;
        }
        /// <summary>
        /// Update icon display and position
        /// </summary>
        protected void _UpdateIcon()
        {
            // Update icon image. set to "null" if icon shouldn't be displayed
            if (_Model.Properties.IsActive) _Buttons[_ButtonType.icon].SetImage(_Model.Properties.Icon); else _Buttons[_ButtonType.icon].SetImage(null);

            // if icon exists, update its position
            if (_Model.Properties.Icon != null && _Model.Properties.IsActive)
            {
                // Compute icon position in layout
                var arcCenterWorld = _SectorData.SectorCenter();
                var arcCenterLocal = _SectorData.ConvertWorldToLocal(arcCenterWorld);
                var posX = arcCenterLocal.X - (_Model.Properties.Icon.Size.Width / 2);
                var posY = arcCenterLocal.Y - (_Model.Properties.Icon.Size.Height / 2);
                Move(_Buttons[_ButtonType.icon], (int)posX, (int)posY); // update icon location
                if (_Model.Properties.IsFolder)
                {
                    // posX = posX + _Model.Properties.Icon.Size.Width - 4;
                    // posY = posY - 6;
                    var outerCenterLocationWorld = _SectorData.GetPoint(_SectorData.SweepAngle / 2, _SectorData.Thickness - (_Buttons[_ButtonType.folderIcon].Width / 2) - 2);
                    var outerCenterLocationLocal = _SectorData.ConvertWorldToLocal(outerCenterLocationWorld);
                    posX = outerCenterLocationLocal.X - (_Buttons[_ButtonType.folderIcon].Width / 2);
                    posY = outerCenterLocationLocal.Y - (_Buttons[_ButtonType.folderIcon].Height / 2);
                    _Buttons[_ButtonType.folderIcon].NsViewObject.AlphaValue = (float)0.4;
                    Move(_Buttons[_ButtonType.folderIcon], (int)posX, (int)posY);
                }
                else
                {
                    _Buttons[_ButtonType.folderIcon].NsViewObject.AlphaValue = 0;
                }
            }
            else
            {
                _Buttons[_ButtonType.folderIcon].NsViewObject.AlphaValue = 0;
            }
        }
        /// <summary>
        /// Update button images and size
        /// </summary>
        /// <param name="data"></param>
        protected void _UpdateSectorData(SectorData data)
        {
            // Update self data
            Size = data.Size;
            // Update buttons image
            _Buttons[_ButtonType.normal].SetImage(data.Images.NormalStateImage, data.Size);
            _Buttons[_ButtonType.over].SetImage(data.Images.OverStateImage, data.Size);
            _Buttons[_ButtonType.disabled].SetImage(data.Images.DisabledStateImage, data.Size);
            _Buttons[_ButtonType.selected].SetImage(data.Images.SelectedStateImage, data.Size);
            _Buttons[_ButtonType.icon].SetImage(_Model.Properties.Icon, data.Size);

            // Update position
            _UpdateIcon();
            _UpdateTriggerIcon();
        }
        /// <summary>
        /// 
        /// </summary>
        protected void _UpdateIconEditmode()
        {
            // When in edit mode update edit icon position
            if (States.IsEditMode)
            {
                // Compute icon position in layout
                var arcCenterWorld = _SectorData.SectorCenter();
                var arcCenterLocal = _SectorData.ConvertWorldToLocal(arcCenterWorld);
                var posX = arcCenterLocal.X - (_Buttons[_ButtonType.editmode].Width / 2);
                var posY = arcCenterLocal.Y - (_Buttons[_ButtonType.editmode].Height / 2);
                Move(_Buttons[_ButtonType.editmode], (int)posX, (int)posY); // update icon location
            }
        }
        /// <summary>
        /// Update trigger key icon
        /// </summary>
        protected void _UpdateTriggerIcon()
        {
            Bitmap bm = null;
            if (_Model.Properties.Trigger != "")
            {
                // Create Text image
                var fontSize = 8;
                var bitmapSize = new Size(fontSize + 2, fontSize + 2); // +2 because of underlined font
                bm = new Bitmap(bitmapSize, PixelFormat.Format32bppRgba);
                var g = new Graphics(bm); g.PixelOffsetMode = PixelOffsetMode.Half; g.AntiAlias = true;
                var font = Fonts.Sans(fontSize, FontStyle.None, FontDecoration.Underline);
                g.DrawText(font, Colors.Black, 0, 0, _Model.Properties.Trigger.ToUpper());
                // g.DrawRectangle(Colors.Black, 0, 0, 15, 15);
                g.Dispose();
                // Compute icon position in layout
                var sectorTopLeft = _SectorData.GetPoint(_SectorData.SweepAngle / 2, 10);
                var sectorTopLeftLocal = _SectorData.ConvertWorldToLocal(sectorTopLeft);
                var posX = sectorTopLeftLocal.X - (fontSize / 2);
                var posY = sectorTopLeftLocal.Y - ((fontSize + 2) / 2);
                Move(_Buttons[_ButtonType.trigger], (int)posX, (int)posY);
            }

            // Update icon image and position
            _Buttons[_ButtonType.trigger].SetImage(bm, new Size(16, 16));
        }
    }
}