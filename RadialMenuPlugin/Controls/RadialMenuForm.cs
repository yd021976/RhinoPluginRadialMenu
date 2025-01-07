using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using Rhino.PlugIns;
using RadialMenuPlugin.Data;
using RadialMenuPlugin.Controls.Buttons;
using RadialMenuPlugin.Controls.Buttons.MenuButton;
using System;

namespace RadialMenuPlugin.Controls
{
    public class RadialMenuForm : TransparentForm
    {
        #region Public properties
        #endregion

        #region Protected/Private properties
        protected static int s_defaultInnerRadius = 50;
        protected static int s_defaultThickness = 60;
        protected static int s_maxLevels = 3;
        protected static Size s_menuSize = new Size(800, 800);
        protected PixelLayout _Layout = new PixelLayout();
        protected List<RadialMenuLevel> _Levels = new List<RadialMenuLevel>() {
                new RadialMenuLevel(1,s_defaultInnerRadius,s_defaultThickness),
                new RadialMenuLevel(2,s_defaultInnerRadius+s_defaultThickness+8,s_defaultThickness,(360/8)/2), // For this one, start angle at 45° to alternate buttons
                new RadialMenuLevel(3,(s_defaultInnerRadius+s_defaultThickness)*2+8,s_defaultThickness),
            };
        /// <summary>
        /// Keep track of drag source Control to register/unregister "dragEnd" event
        /// </summary>
        protected Control _Dragsource;
        /// <summary>
        /// Current radial menu mode : true=>Edit mode, false=>normal mode 
        /// </summary>
        protected bool _EditMode = false;
        /// <summary>
        /// Associate a menu level to a Control of type <SectorArcRadialControl>
        /// </summary>
        protected Dictionary<RadialMenuLevel, RadialMenuControl> _Controls = new Dictionary<RadialMenuLevel, RadialMenuControl>();
        /// <summary>
        /// Ref to Model object when button is dragged
        /// </summary>
        #endregion

        #region Public methods
        public RadialMenuForm(PlugIn plugin) : base(plugin)
        {
            Size = new Size(s_menuSize.Width, s_menuSize.Height);
            _InitLevels(); // Create "blank" radial menu controls (i.e. no button IDs, blank models)

            // Ensure when form is shown that we display only first menu level
            Shown += (o, e) =>
            {
                foreach (var ctrl in _Controls.Values)
                {
                    ctrl.Show(false); // Hide and reset radial control enable and selection
                }
                var radialControl = _Controls.First(obj => obj.Key.Level == 1).Value;
                radialControl.Show(true);
            };

            // Get ressource icon for close button
            var img = Bitmap.FromResource("RadialMenu.Bitmaps.close-icon.png");
            var icon = img.WithSize(16, 16);

            // Create close button
            var closeBtn = new RoundButton();
            closeBtn.Size = new Size(32, 32);
            closeBtn.SetButtonIcon(icon);
            closeBtn.MouseEnter += (o, e) =>
            {
                Focus(); // Get menu focus
            };
            closeBtn.MouseLeave += (o, e) =>
            {
                Rhino.RhinoApp.SetFocusToMainWindow(); // Give Rhino app focus
            };
            closeBtn.OnclickEvent += (sender, args) =>
            {
                // Right click on close button toggles edit mode
                if (args.Buttons == MouseButtons.Alternate)
                {
                    _EditMode = !_EditMode;
                    foreach (var control in _Controls.Values)
                    {
                        if (control.Level.Level != 1) control.Show(false); // Hide current opened sub menu
                        control.SwitchEditMode(_EditMode);
                    }
                }
                else if (args.Buttons == MouseButtons.Primary)
                {
                    _OnCloseClickEvent(sender);
                }
            };
            _Layout.Add(closeBtn, (Size.Width / 2) - closeBtn.Size.Width / 2, (Size.Height / 2) - (closeBtn.Size.Height / 2));

            // Create and add RadialMenu Control for 1st level
            var ctrl = _Controls.First(level => level.Key.Level == 1).Value;
            ctrl.SetMenuForButtonID(null); // Init buttons and model for 1st menu level

            // Add layout to content of form
            Content = _Layout;
        }
        #endregion

        #region Protected/Private methods
        /// <summary>
        /// Init menu level controls
        /// </summary>
        protected void _InitLevels()
        {
            for (int i = 0; i < s_maxLevels; i++)
            {
                var ctrl = new RadialMenuControl(_Levels[i]);

                // Mouse events
                ctrl.MouseEnterButton += _RadialControlMouseEnterButtonHandler;
                ctrl.MouseMoveButton += _RadialControlMouseMoveButtonHandler;
                ctrl.MouseLeaveButton += _RadialControlMouseLeaveButtonHandler;
                ctrl.MouseClickButton += _RadialControlMouseClickHandler;
                ctrl.ButtonContextMenu += _RadialControlContextMenu;

                // Drag Drop events
                ctrl.DragDropEnterButton += _RadialControlDragEnterHandler;
                ctrl.DragDropOverButton += _RadialControlDragOverHandler;
                ctrl.DragDropLeaveButton += _RadialControlDragLeaveHandler;
                ctrl.DragDropButton += _RadialControlDragDropHandler;
                ctrl.RemoveButton += _RadialControlRemoveButton;
                _Controls.Add(_Levels[i], ctrl);
            }
            //
            //REMARK Order is VERY important because arc button are rectangles. So if submenu level 2 is not "top most" (i.e. last position), drag over doesn't work and sometimes
            // drag over don't detect level "2" buttons
            //
            _Layout.Add(_Controls.ElementAt(0).Value, (Size.Width / 2) - (_Controls.ElementAt(0).Value.Size.Width / 2), Size.Height / 2 - (_Controls.ElementAt(0).Value.Size.Height / 2));
            _Layout.Add(_Controls.ElementAt(2).Value, (Size.Width / 2) - (_Controls.ElementAt(2).Value.Size.Width / 2), Size.Height / 2 - (_Controls.ElementAt(2).Value.Size.Height / 2));
            _Layout.Add(_Controls.ElementAt(1).Value, (Size.Width / 2) - (_Controls.ElementAt(1).Value.Size.Width / 2), Size.Height / 2 - (_Controls.ElementAt(1).Value.Size.Height / 2));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        protected bool _AcceptDragDrop(ButtonDragDropEventArgs eventArgs)
        {
            var sourceType = Utilities.DragDropUtilities.dragSourceType(eventArgs.DragEventSourceArgs.Source);
            switch (sourceType)
            {
                case DragSourceType.radialMenuItem:
                    var guid = eventArgs.DragEventSourceArgs.Data.GetString("MODEL_GUID");
                    var sourceDragModel = ModelController.Instance.Find(guid);
                    var ret = false;
                    if (sourceDragModel != null)
                    {
                        if (sourceDragModel.Data.Properties.IsFolder)
                        {
                            if (sourceDragModel == eventArgs.TargetModel) // If source is dragged onto initial place, accept drop
                            {
                                eventArgs.DragEventSourceArgs.Effects = DragEffects.All;
                                ret = true;
                            }
                            else
                            {
                                eventArgs.DragEventSourceArgs.Effects = DragEffects.None;
                            }
                        }
                        else
                        {
                            eventArgs.DragEventSourceArgs.Effects = DragEffects.All;
                            ret = true;
                        }
                    }
                    return ret;
                case DragSourceType.rhinoItem:
                    eventArgs.DragEventSourceArgs.Effects = DragEffects.All;
                    return true;
                case DragSourceType.unknown:
                default:
                    eventArgs.DragEventSourceArgs.Effects = DragEffects.None; // Reject drop if source is unknown
                    return false;
            }
        }
        /// <summary>
        /// Create DragEnd event handler for the source control that initiate a drag/drop. Will remove any existing DragEnd handler if exists
        /// </summary>
        /// <param name="sourceObject"></param>
        protected void _RegisterDragEndHandler(Control sourceObject)
        {
            // if (!(sourceObject is MenuButton)) // Don't register drag end for MenuButton, it will raise its own event <DoDragEnd>
            // {
            if (_Dragsource != null)
            {
                _Dragsource.DragEnd -= _RadialControlDragEndHandler; // Remove event handler for previous Drag source Control
            }
            _Dragsource = sourceObject;
            _Dragsource.DragEnd += _RadialControlDragEndHandler;
            // }
        }
        /// <summary>
        /// Update models after a drag/drop is done
        /// </summary>
        /// <param name="updatedProperties"></param>
        /// <param name="targetModel"></param>
        /// <param name="targetLevel"></param>
        protected void _UpdateDropItem(ButtonProperties updatedProperties, Model targetModel, int targetLevel)
        {
            targetModel.Data.Properties.Icon = updatedProperties.Icon;
            targetModel.Data.Properties.IsActive = true;
            targetModel.Data.Properties.IsFolder = false;
            targetModel.Data.Properties.RhinoScript = updatedProperties.RhinoScript;
            targetModel.Data.Properties.CommandGUID = updatedProperties.CommandGUID;

            if (targetLevel > 1) // If icon is drop on a sub menu -> Update parents menu models
            {
                var parentModel = targetModel.Parent;
                do
                {
                    // Keep parent icon untouched if already set
                    if (parentModel.Data.Properties.Icon == null)
                    {
                        parentModel.Data.Properties.Icon = updatedProperties.Icon;
                    }
                    // Keep parent "isFolder" untouched if already set
                    if (parentModel.Data.Properties.IsFolder == false)
                    {
                        parentModel.Data.Properties.IsFolder = true;
                    }
                    parentModel.Data.Properties.IsActive = true; // Ensure icon is active
                    parentModel.Data.Properties.CommandGUID = updatedProperties.CommandGUID;
                    parentModel = parentModel.Parent;
                } while (parentModel != null);
            }
        }
        /// <summary>
        /// Find and return first control object that matches <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate">function to find object</param>
        /// <returns>Control instance if found, null if not found</returns>
        protected RadialMenuControl _GetControl(Func<RadialMenuControl, bool> predicate)
        {
            RadialMenuControl ctrl = null;
            foreach (RadialMenuControl menuControl in _Controls.Values)
            {
                if (predicate(menuControl))
                {
                    ctrl = menuControl;
                    break;
                }
            }
            return ctrl;
        }
        /// <summary>
        /// Open a submenu from an opened menu control and given a button ID
        /// </summary>
        /// <param name="radialMenuControl">Currently opened submenu. Provided to compute next submenu level</param>
        /// <param name="model">Model contining the current button ID to open submenu for</param>
        /// <returns>the new <see cref="RadialMenuControl"/> opened submenu or null if no new submenu was opened</returns>
        protected RadialMenuControl _ShowSubmenu(RadialMenuControl radialMenuControl, Model model)
        {
            var nextLevel = radialMenuControl.Level.Level + 1; // Compute next level number
            var ctrl = _GetControl(element => element.Level.Level == nextLevel);
            if (ctrl != null)
            {
                ctrl.SetMenuForButtonID(model); // Update radial control buttons
                ctrl.Show(true); // Ensure control is visible
            }
            return ctrl;
        }

        /// <summary>
        /// Hide all submenus from last/max to level of <paramref name="radialMenuControl"/>
        /// <para>If no <paramref name="radialMenuControl"/> id provided, close all submenu except root submenu</para>
        /// </summary>
        /// <param name="radialMenuControl">Menu control to keep</param>
        private void _HideSubmenu(RadialMenuControl radialMenuControl = null)
        {
            var levelToClose = radialMenuControl == null ? 1 : radialMenuControl.Level.Level;
            foreach (var control in _Controls.Values)
            {
                // Hide menu control if higher than the one provided (or higher than "root" control menu)
                if (control.Level.Level > levelToClose)
                {
                    control.Show(false);
                    control.DisableButtonsExceptSelection(); // Enable all buttons
                    control.SelectedButtonID = ""; // Reset selection
                }
            }
        }
        /// <summary>
        /// Do some cleanup of visual control before exit (Hide) the plugin Form object
        /// </summary>
        /// <param name="sender"></param>
        protected override void _OnCloseClickEvent(object sender)
        {
            foreach (var control in _Controls.Values)
            {
                control.Show(false);
                control.DisableButtonsExceptSelection();
            }
            base._OnCloseClickEvent(sender);
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// Each ESC keypress closes sub menu until level 1 submenu is reached => Close radial menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void _OnEscapePressed(object sender, KeyEventArgs e)
        {
            if (Visible) // Ensure radial menu is currently running
            {
                RadialMenuLevel topMostOpenedLevel = null;
                foreach (var control in _Controls)
                {
                    if (control.Value.IsVisible && (control.Key.Level > (topMostOpenedLevel == null ? 0 : topMostOpenedLevel.Level)))
                    {
                        topMostOpenedLevel = control.Key;
                    }
                }
                // Hide top most sub menu if we found any
                if (topMostOpenedLevel != null)
                {
                    if (topMostOpenedLevel.Level == 1)
                    {
                        // Close the radial menu
                        _OnCloseClickEvent(this);
                    }
                    else
                    {
                        _Controls[topMostOpenedLevel].Show(false);
                    }
                }
            }
        }
        //
        //  Mouse enter, over and leave handlers to prevent default radial control default behavior when needed (i.e. Avoid un-selecting button)
        //  Those custom behaviors will influence the event raising of "SelectionChanged", so the logic of displaying sub menu levels is managed in "onSelectionChanged" event
        //
        /// <summary>
        /// Mouse enter a button : Manage show/hide submenus
        /// </summary>
        /// <param name="radialMenuControl"></param>
        protected void _RadialControlMouseEnterButtonHandler(RadialMenuControl radialMenuControl, ButtonMouseEventArgs e)
        {
            if (e.Model.Data.Properties.IsFolder)
            {
                var newSubmenu = _ShowSubmenu(radialMenuControl, e.Model);
                // Hide higher levels from new opened submenu if any
                if (newSubmenu != null)
                {
                    _HideSubmenu(newSubmenu);
                }
            }
            else // If we enter a non folder, hide higher submenus 
            {
                _HideSubmenu(radialMenuControl);
            }
        }

        /// <summary>
        /// Mouse over a button
        /// </summary>
        /// <param name="radialMenuControl"></param>
        protected void _RadialControlMouseMoveButtonHandler(RadialMenuControl radialMenuControl, ButtonMouseEventArgs e)
        {
            Focus(); // Give radial menu focus when mouse overs a button
        }
        protected void _RadialControlMouseClickHandler(RadialMenuControl radialMenuControl, ButtonMouseEventArgs e)
        {
            if (e.Model.Data.Properties.RhinoScript != "")
            {
                _OnCloseClickEvent(this); // close radial menu
                Rhino.RhinoApp.SetFocusToMainWindow();
                Rhino.RhinoApp.RunScript(e.Model.Data.Properties.RhinoScript, false); // Run Rhino command
            }
        }
        /// <summary>
        /// Mouse leave a button
        /// </summary>
        /// <param name="radialMenuControl"></param>
        protected void _RadialControlMouseLeaveButtonHandler(RadialMenuControl radialMenuControl, ButtonMouseEventArgs e)
        {
            Rhino.RhinoApp.SetFocusToMainWindow(); // Give Rhino main window focus when no button is over
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radialMenuControl"></param>
        protected void _RadialControlSelectionChangedHandler(RadialMenuControl radialMenuControl)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radialMenuControl"></param>
        protected void _RadialControlFocusRequestedHandler(RadialMenuControl radialMenuControl) { }
        /// <summary>
        /// Event when button request accept drop target. If drop target menu level is != from level of drag source AND it was a "self" drag, don't accept
        /// </summary>
        /// <param name="radialMenuControl"></param>
        /// <param name="eventArgs"></param>
        protected void _RadialControlAcceptDropHandler(RadialMenuControl radialMenuControl, DropTargetArgs eventArgs)
        { }
        /// <summary>
        /// Button properties were updated, dragging mode ended
        /// </summary>
        /// <param name="radialMenuControl"></param>
        /// <param name="eventArgs"></param>
        protected void _RadialControlDragEnterHandler(RadialMenuControl radialMenuControl, ButtonDragDropEventArgs eventArgs)
        {
            if (_AcceptDragDrop(eventArgs))
            {
                if (radialMenuControl.Level.Level < s_maxLevels) radialMenuControl.SelectedButtonID = eventArgs.TargetModel.Data.ButtonID; // Update selection except for last menu level (useless)
                var newMenuControl = _ShowSubmenu(radialMenuControl, eventArgs.TargetModel); // Open submenu if drop is accepted
                if (newMenuControl != null) _HideSubmenu(newMenuControl); // Hide higher opened submenus
                _RegisterDragEndHandler(eventArgs.DragEventSourceArgs.Source); // Register "DragEnd" event handler
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="radialMenuControl"></param>
        /// <param name="eventArgs"></param>
        protected void _RadialControlDragOverHandler(RadialMenuControl radialMenuControl, ButtonDragDropEventArgs eventArgs)
        {
            if (_AcceptDragDrop(eventArgs))
            {
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="radialMenuControl"></param>
        /// <param name="eventArgs"></param>
        private void _RadialControlDragLeaveHandler(RadialMenuControl radialMenuControl, ButtonDragDropEventArgs eventArgs)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radialMenuControl"></param>
        /// <param name="eventArgs"></param>
        protected void _RadialControlDragDropHandler(RadialMenuControl radialMenuControl, ButtonDragDropEventArgs eventArgs)
        {
            // Update menu button data
            if (_AcceptDragDrop(eventArgs))
            {
                switch (Utilities.DragDropUtilities.dragSourceType(eventArgs.DragEventSourceArgs.Source))
                {
                    case DragSourceType.rhinoItem:
                        ButtonProperties toolbaritem = Utilities.DragDropUtilities.getDroppedToolbarItem(eventArgs.DragEventSourceArgs);
                        _UpdateDropItem(toolbaritem, eventArgs.TargetModel, radialMenuControl.Level.Level);
                        break;
                    case DragSourceType.radialMenuItem:
                        var guid = eventArgs.DragEventSourceArgs.Data.GetString("MODEL_GUID");
                        var sourceDragModel = ModelController.Instance.Find(guid);
                        if (sourceDragModel != null)
                        {
                            if (sourceDragModel == eventArgs.TargetModel) // If source and target are the same, do nothing
                            {
                                return;
                            }
                            else
                            {
                                if (eventArgs.TargetModel.Parent == sourceDragModel.Parent) // Item is moved on same level sub menu
                                {
                                    // Update target model
                                    eventArgs.TargetModel.Data.Properties.Icon = sourceDragModel.Data.Properties.Icon;
                                    eventArgs.TargetModel.Data.Properties.IsFolder = false;
                                    eventArgs.TargetModel.Data.Properties.IsActive = true;
                                    eventArgs.TargetModel.Data.Properties.RhinoScript = sourceDragModel.Data.Properties.RhinoScript;
                                    eventArgs.TargetModel.Data.Properties.CommandGUID = sourceDragModel.Data.Properties.CommandGUID;
                                    // Clear source model data
                                    sourceDragModel.Data.Properties.Icon = null;
                                    sourceDragModel.Data.Properties.IsActive = false;
                                    sourceDragModel.Data.Properties.IsFolder = false;
                                    sourceDragModel.Data.Properties.RhinoScript = "";
                                    sourceDragModel.Data.Properties.CommandGUID = Guid.Empty;
                                }
                                else // Item is moved on another level sub menu
                                {
                                    // Create new model properties from source Model
                                    ButtonProperties data = new ButtonProperties(
                                        new string(sourceDragModel.Data.Properties.RhinoScript),
                                        new Icon(sourceDragModel.Data.Properties.Icon.Frames), true, false,
                                        sourceDragModel.Data.Properties.CommandGUID);

                                    // Clear source model data
                                    sourceDragModel.Data.Properties.Icon = null;
                                    sourceDragModel.Data.Properties.IsActive = false;
                                    sourceDragModel.Data.Properties.IsFolder = false;
                                    sourceDragModel.Data.Properties.RhinoScript = "";
                                    sourceDragModel.Data.Properties.CommandGUID = Guid.Empty;

                                    // (Recursive) Check each source item parent submenus still contains children. If not, clear parent model
                                    var parent = sourceDragModel.Parent;
                                    while (parent != null)
                                    {
                                        var sourceModelChildren = ModelController.Instance.GetChildren(parent); var activeIcon = 0;
                                        foreach (var model in sourceModelChildren)
                                        {
                                            if (model.Data.Properties.Icon != null) activeIcon++;
                                        }
                                        if (activeIcon == 0) // Parent model has no children left -> Remove icon and isFolder flag for parent model
                                        {
                                            parent.Data.Properties.Icon = null;
                                            parent.Data.Properties.IsFolder = false;
                                            parent.Data.Properties.IsActive = false;
                                            parent.Data.Properties.CommandGUID = Guid.Empty;
                                        }
                                        parent = parent.Parent;

                                    }
                                    // Rebuidld/Update sub menu hierarchy for target drop
                                    _UpdateDropItem(data, eventArgs.TargetModel, radialMenuControl.Level.Level);
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// When source control drag ends, clear all opened submenus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        protected void _RadialControlDragEndHandler(object sender, DragEventArgs eventArgs)
        {
            // Close submenus except level 1 menu
            foreach (var radialMenuControl in _Controls.Values)
            {
                switch (radialMenuControl.Level.Level)
                {
                    case > 1:
                        radialMenuControl.Show(false);
                        break;
                    default:
                        radialMenuControl.SelectedButtonID = "";
                        break;
                }
            }
            // Cleanup DragEnd handler
            if (_Dragsource != null)
            {
                _Dragsource.DragEnd -= _RadialControlDragEndHandler;
                _Dragsource = null;
            }
        }
        /// <summary>
        /// Remove button is requested. Clear buttons models and all children if they exists
        /// <para>
        /// Don't really remove models objects, only clear values
        /// </para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        protected void _RadialControlRemoveButton(object sender, ButtonDragDropEventArgs eventArgs)
        {
            _ClearModel(eventArgs.TargetModel);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        protected void _ClearModel(Model model)
        {
            var children = ModelController.Instance.GetChildren(model);
            if (children.Count > 0)
            {
                foreach (var child in children)
                {
                    _ClearModel(child); // Clear children if any
                }
            }
            model.Clear();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        protected void _RadialControlContextMenu(object sender, ButtonMouseEventArgs eventArgs)
        {

        }
        #endregion
    }
}
