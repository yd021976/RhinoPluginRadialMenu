using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AppKit;
using Eto.Drawing;
using Eto.Forms;
using RadialMenuPlugin.Controls.Buttons.MenuButton;
using RadialMenuPlugin.Data;
using RadialMenuPlugin.Utilities;
using RadialMenuPlugin.Utilities.Events;
using Rhino;

namespace RadialMenuPlugin.Controls
{
    #region Event Handler and Args classes
    using DragDropEventHandler = AppEventHandler<RadialMenuControl, ButtonDragDropEventArgs>; // Drag/Drop event delegate type
    using MouseEventHandler = AppEventHandler<RadialMenuControl, ButtonMouseEventArgs>;
    /// <summary>
    /// 
    /// </summary>
    public class ButtonDragDropEventArgs
    {
        /// <summary>
        /// Source object (should be either MenuButton or RhinoToolbarItem)
        /// </summary>
        public DragEventArgs DragEventSourceArgs;
        /// <summary>
        /// Data model of drop target
        /// </summary>
        public Model TargetModel;
        public ButtonDragDropEventArgs(DragEventArgs dragevent, Model target)
        {
            DragEventSourceArgs = dragevent;
            TargetModel = target;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public struct SelectionEventArgs
    {
        public string ButtonID;
        public Model Model;
        public ButtonProperties Properties;
        public SelectionEventArgs(string buttonID, Model model, ButtonProperties properties)
        {
            ButtonID = buttonID;
            Model = model;
            Properties = properties;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public struct ButtonMouseEventArgs
    {
        public MouseEventArgs MouseEventArgs;
        public Model Model;
        public ButtonMouseEventArgs(MouseEventArgs mouseEventArgs, Model model)
        {
            MouseEventArgs = mouseEventArgs;
            Model = model;
        }
    }
    #endregion
    /// <summary>
    /// PixelLayout object that is responsible of displaying sectors button
    /// <para>
    /// WARN: DO NOT USE "VISIBLE" property -> It prevents layout control "NSTrackingArea" to be updated when controls size changes
    /// Instead, we use "AlphaValue" to show/hide this control
    /// </para>
    /// </summary>
    public class RadialMenuControl : PixelLayout
    {
        #region Events declaration
        public event MouseEventHandler MouseEnterButton;
        public event MouseEventHandler MouseMoveButton;
        public event MouseEventHandler MouseLeaveButton;
        public event MouseEventHandler MouseClickButton;
        public event DragDropEventHandler DragDropEnterButton;
        public event DragDropEventHandler DragDropOverButton;
        public event DragDropEventHandler DragDropLeaveButton;
        public event DragDropEventHandler DragDropButton;
        public event DragDropEventHandler RemoveButton;
        public event MouseEventHandler ButtonContextMenu;
        #endregion

        #region Public properties
        public RadialMenuLevel Level;
        public bool IsVisible
        {
            get => NsView.AlphaValue == 0 ? false : true;
        }
        public NSView NsView { get => _GetNSView(); }
        public string SelectedButtonID
        {
            get => _SelectedButtonID;
            set
            {
                MenuButton btn = null;
                var raiseEvent = SelectedButtonID != value;
                _SelectedButtonID = value;

                try
                {
                    btn = _Buttons.First(keyvaluepair => keyvaluepair.Key.ID == _SelectedButtonID).Key;
                }
                catch { }
                finally
                {
                    if (value != "")
                    {
                        if (btn != null)
                        {
                            btn.States.IsSelected = true;
                            _ClearSelection(_SelectedButtonID); // Clear selection of others buttons
                        }
                    }
                    else // if no selection, clear all button selection state
                    {
                        _ClearSelection();
                    }

                    // Raise "selectionChanged" if selection changed
                    if (raiseEvent)
                    {
                        // SelectionArgs args = buildSelectionEventArgs(btn);
                        // onSelectionChanged?.Invoke(this, args);
                    }
                }
            }
        }
        #endregion

        #region Protected/Private properties
        /// <summary>
        /// Associate a button with model
        /// </summary>
        protected Dictionary<MenuButton, Model> _Buttons = new Dictionary<MenuButton, Model>();
        /// <summary>
        /// Associate a button with sector data images
        /// </summary>
        protected Dictionary<MenuButton, SectorData> _ButtonsImages = new Dictionary<MenuButton, SectorData>();
        /// <summary>
        /// The current selected button ID
        /// </summary>
        protected string _SelectedButtonID;
        #endregion

        #region Animations
        protected double _AnimationDuration = 0.3;
        /// <summary>
        /// Animate show and hide effect
        /// </summary>
        /// <param name="show"></param>
        protected void _AnimateShowHideEffect(bool show = true)
        {
            NSAnimationContext.BeginGrouping();
            NSAnimationContext.CurrentContext.AllowsImplicitAnimation = true;
            NSAnimationContext.CurrentContext.Duration = _AnimationDuration;
            NsView.AlphaValue = show ? 1 : 0;
            NSAnimationContext.EndGrouping();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="level"></param>
        public RadialMenuControl(RadialMenuLevel level) : base()
        {
            var w = Stopwatch.StartNew(); //DEBUG Method measure
            Level = level;
            Size = new Size((level.InnerRadius + level.Thickness) * 2, (level.InnerRadius + level.Thickness) * 2);
            var sectors = _BuildSectors();

            // Init empty sectorArcButtons
            for (var i = 0; i < level.SectorsNumber; i++)
            {
                var sector = sectors[i];
                // Initialize button
                var btn = new MenuButton();
                btn.ID = i.ToString();
                btn.SectorData = sector; // Button images

                btn.OnButtonMouseEnter += _OnMouseEnter;
                btn.OnButtonMouseMove += _OnMouseMove;
                btn.OnButtonMouseLeave += _OnMouseLeave;
                btn.OnButtonClickEvent += _OnMouseClick;

                btn.OnButtonDragEnter += _OnDragDropEnter;
                btn.OnButtonDragOver += _OnDragOver;
                btn.OnButtonDragLeave += _onDragLeave;
                btn.OnButtonDragDrop += _OnDragDrop;
                btn.OnButtonDragDropStart += _OnDoDragStart;
                btn.OnButtonDragDropEnd += _OnDoDragEnd;

                btn.OnButtonContextMenu += _OnButtonContextMenu;

                _Buttons.Add(btn, null); // Update button dictionary
                _ButtonsImages.Add(btn, sector); // Store images for this button
                Add(btn, (int)sector.Bounds.X, (int)sector.Bounds.Y); // Add button to layout
            }
            Visible = true;
            //DEBUG Method measure
            w.Stop();
            Console.SetOut(RhinoApp.CommandLineOut);
            Console.WriteLine("SectorArcRadialControl constructor:" + w.ElapsedMilliseconds);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="editMode"></param>
        public void SwitchEditMode(bool editMode)
        {
            _ClearSelection();
            _EnableButtons();
            foreach (var button in _Buttons.Keys)
            {
                button.States.IsSelected = false;
                button.States.IsEditMode = editMode;
            }
        }
        public ButtonProperties GetButtonProperties(string buttonID)
        {
            try
            {
                return _Buttons.First(entry => entry.Key.ID == buttonID).Value.Data.Properties;
            }
            catch
            {
                Console.SetOut(RhinoApp.CommandLineOut);
                Console.WriteLine($"SectorArcRadialControl get button {buttonID} properties error");
                return null;
            }
        }
        public void SetButtonProperties(string buttonID, Data.ButtonProperties properties)
        {
            try
            {
                _Buttons.First(entry => entry.Key.ID == buttonID).Value.Data.Properties = properties;
            }
            catch
            {
                Console.SetOut(RhinoApp.CommandLineOut);
                Console.WriteLine($"SectorArcRadialControl set button {buttonID} properties error");
            }
            finally { }
        }
        public void Show(bool show = true)
        {
            if (!show) // When hidding, reset button state (selected and enabled)
            {
                _ClearSelection(); // when control shows, reset all button selected to false
                _EnableButtons(); // When control is hidden, reset selection and enable children
            }
            _SetChildrenVisibleState(show); // Update children sector button visible state to prevent unwanted "onMouseOver" events
            _AnimateShowHideEffect(show); // Animate effects
        }

        /// <summary>
        /// Activate or deactivate all buttons except the one selected
        /// If no button is selected, enable all buttons
        /// </summary>
        /// <param name="buttonID"></param>
        public void DisableButtonsExceptSelection()
        {
            if (SelectedButtonID == "") // If no selection active, enable all buttons
            {
                _EnableButtons();
            }
            else // If a button is selected, disable all others buttons
            {
                _DisableButtonsExcept(SelectedButtonID);
            }
        }

        /// <summary>
        /// Do the control contains the provided button
        /// </summary>
        /// <param name="btn"></param>
        /// <returns></returns>
        public bool HasButton(MenuButton btn)
        {
            return _Buttons.ContainsKey(btn);
        }

        /// <summary>
        /// Display list of buttons for the specified ID (i.e. ID is the ID of the previous level button that trigger opening the menu)
        /// </summary>
        /// <param name="forButtonID"></param>
        public void SetMenuForButtonID(Model parent = null)
        {
            // var w = Stopwatch.StartNew();   //DEBUG: Method measure
            SelectedButtonID = ""; // As we build new layout, unselect any button

            // Iterate on each sector data to update button
            foreach (MenuButton button in _Buttons.Keys)
            {
                button.Unbind(); // Unbind any bindings
                var model = ModelController.Instance.Find(button.ID, parent, true);
                _Buttons[button] = model; // update model
                button.ButtonModelBinding.Bind(_Buttons, bntCollection => bntCollection[button].Data); // Bind button model
                button.ID = model.Data.ButtonID; // Update button ID

            }
            // w.Stop();
            // Console.SetOut(RhinoApp.CommandLineOut);
            // Console.WriteLine("SetMenuForButtonID takes:" + w.ElapsedMilliseconds);
        }
        #endregion

        #region Protected/Private methods
        /// <summary>
        /// Disable all buttons except the one specified
        /// <para>Use this method when a button opens a folder</para>
        /// </summary>
        /// <param name="buttonID"></param>
        protected void _DisableButtonsExcept(string buttonID)
        {
            foreach (var btn in _Buttons.Keys)
            {
                if (btn.ID == buttonID) btn.Enabled = true; else btn.Enabled = false;
            }
        }

        /// <summary>
        /// Enable all control buttons
        /// </summary>
        protected void _EnableButtons()
        {
            foreach (var btn in _Buttons.Keys)
            {
                btn.Enabled = true;
            }
        }

        /// <summary>
        /// Reset all selected buttons
        /// </summary>
        protected void _ClearSelection(string exceptButtonID = "")
        {
            foreach (var btn in _Buttons.Keys)
            {
                if (exceptButtonID != "") // Check if we have a button to not clear selection
                {
                    if (_Buttons[btn].Data.ButtonID != exceptButtonID) btn.States.IsSelected = false;
                }
                else
                {
                    btn.States.IsSelected = false;
                }

            }
        }
        /// <summary>
        /// Build new sectors data
        /// </summary>
        /// <param name="sectorsNumber"></param>
        /// <param name="startAngle"></param>
        /// <returns></returns>
        protected List<SectorData> _BuildSectors()
        {
            int angleStart;
            List<SectorData> sectors = new List<SectorData>();

            var sweepAngle = 360 / Level.SectorsNumber;

            for (int i = 0; i < Level.SectorsNumber; i++)
            {
                // compute angle. If > 360, reset to 0
                angleStart = Level.StartAngle + (i * sweepAngle);
                if (angleStart > 360) Level.StartAngle -= 360;

                // Draw one sector
                var _graphicsPath = new GraphicsPath();
                var sectorDrawer = new ArcSectorDrawer();
                var sectorData = sectorDrawer.DrawSector(_graphicsPath, Size.Width / 2, Size.Height / 2, Level, angleStart, sweepAngle);

                // add sector infos to list
                sectors.Add(sectorData);

                // Release resources
                _graphicsPath.Dispose();
            }
            return sectors;
        }

        /// <summary>
        /// Hide/Show buttons
        /// </summary>
        /// <param name="show"></param>
        protected void _SetChildrenVisibleState(bool show)
        {
            foreach (var ctrl in _Buttons.Keys)
            {
                ctrl.States.IsVisible = show;
            }
        }
        /// <summary>
        /// Return the native MacOS NSView object
        /// </summary>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        protected NSView _GetNSView()
        {
            var ctrlProp = Handler.GetType().GetProperty("Control");
            return (NSView)ctrlProp.GetValue(Handler, null);
        }
        #endregion

        #region Button event handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void _OnMouseEnter(MenuButton sender, MouseEventArgs e)
        {
            _RaiseEvent(MouseEnterButton, new ButtonMouseEventArgs(e, _Buttons[sender]));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void _OnMouseMove(MenuButton sender, MouseEventArgs e)
        {
            _RaiseEvent(MouseMoveButton, new ButtonMouseEventArgs(e, _Buttons[sender]));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void _OnMouseLeave(MenuButton sender, MouseEventArgs e)
        {
            _RaiseEvent(MouseLeaveButton, new ButtonMouseEventArgs(e, _Buttons[sender]));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void _OnMouseClick(MenuButton sender, MouseEventArgs e)
        {
            _RaiseEvent(MouseClickButton, new ButtonMouseEventArgs(e, _Buttons[sender]));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void _OnDragDropEnter(MenuButton sender, DragEventArgs e)
        {
            _RaiseEvent(DragDropEnterButton, new ButtonDragDropEventArgs(e, _Buttons[sender]));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void _OnDragOver(MenuButton sender, DragEventArgs e)
        {
            _RaiseEvent(DragDropOverButton, new ButtonDragDropEventArgs(e, _Buttons[sender]));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void _onDragLeave(MenuButton sender, DragEventArgs e)
        {
            _RaiseEvent(DragDropLeaveButton, new ButtonDragDropEventArgs(e, _Buttons[sender]));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void _OnDragDrop(MenuButton sender, DragEventArgs e)
        {
            _RaiseEvent(DragDropButton, new ButtonDragDropEventArgs(e, _Buttons[sender]));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        protected void _OnDoDragStart(MenuButton sender)
        {
            var eventObj = new DataObject(); // Empty dataobject
            eventObj.SetString(_Buttons[(MenuButton)sender].GUID.ToString(), "MODEL_GUID");
            sender.DoDragDrop(eventObj, DragEffects.All, _Buttons[(MenuButton)sender].Data.Properties.Icon, new PointF(10, 10));
        }
        /// <summary>
        /// Handler when button drag ended and no drop target has accepted icon
        /// <para>This means dragged icon was drop anywhere an so should be removed</para>
        /// </summary>
        /// <param name="sender"></param>
        protected void _OnDoDragEnd(MenuButton sender, DragEventArgs eventArgs)
        {
            _RaiseEvent(RemoveButton, new ButtonDragDropEventArgs(eventArgs, _Buttons[(sender)]));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void _OnButtonContextMenu(MenuButton sender, MouseEventArgs e)
        {
            var model = _Buttons[sender];
            _RaiseEvent(ButtonContextMenu, new ButtonMouseEventArgs(e, model));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="EVENT"></typeparam>
        /// <param name="action"></param>
        /// <param name="e"></param>
        protected void _RaiseEvent<T, EVENT>(AppEventHandler<T, EVENT> action, EVENT e) where T : RadialMenuControl
        {
            action?.Invoke(this as T, e);
        }
        #endregion
    }
}