using System;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using Eto.Drawing;
using Eto.Forms;
using Rhino;
using Rhino.Display;
using Rhino.PlugIns;

namespace customControls
{

    public class SectorRadialMenuForm : TransparentForm
    {
        protected static int defaultInnerRadius = 70;
        protected static int defaultThickness = 40;
        protected static int maxLevels = 3;
        private PixelLayout layout = new PixelLayout();
        protected bool dragMode = false;
        /// <summary>
        /// Associate a menu level to a Control of type <SectorArcRadialControl>
        /// </summary>
        private Dictionary<RadialMenuLevel, SectorArcRadialControl> radialMenuCtrl = new Dictionary<RadialMenuLevel, SectorArcRadialControl>();
        public SectorRadialMenuForm(PlugIn plugin) : base(plugin)
        {
            Size = new Size(500, 500);
            initLevels(); // Create "blank" radial menu controls (i.e. no button IDs, blank models)

            // Allow drop just to remove drop animation when icon is dragged outside button
            AllowDrop = true;
            DragEnter += (o, e) =>
            {
                Console.SetOut(RhinoApp.CommandLineOut);
                Console.WriteLine($"Drag item in form");
                dragMode = true;
                e.Effects = DragEffects.Move;
                var prop = e.Source.GetType().GetProperty("DeleteDraggedItem");
                var value = prop.GetValue(e.Source, null);
            };
            DragDrop += (o, e) =>
            {
                // We not really drop something on the form. But used to set dragmode to false and handle drop item outside of buttons
                dragMode = false;
                Console.SetOut(RhinoApp.CommandLineOut);
                Console.WriteLine($"Drop item in form");
            };
            DragLeave += (o, e) =>
            {
                // Do nothing as form can "loose" drag as soon as a button get drag events
            };

            // Ensure when form is shown that we display only first menu level
            Shown += (o, e) =>
            {
                foreach (var ctrl in radialMenuCtrl.Values)
                {
                    ctrl.show(false); // Hide and reset radial control enable and selection
                }
                var radialControl = radialMenuCtrl.First(obj => obj.Key.level == 1).Value;
                radialControl.show(true);
            };

            // Get ressource icon for close button
            var img = Bitmap.FromResource("RadialMenu.Bitmaps.close-icon-13612.png");
            var icon = img.WithSize(16, 16);

            // Create close button
            var closeBtn = new RoundButton();
            closeBtn.Size = new Size(32, 32);
            closeBtn.setButtonIcon(icon);
            closeBtn.onclickEvent += onCloseClickEvent;
            layout.Add(closeBtn, (Size.Width / 2) - closeBtn.Size.Width / 2, (Size.Height / 2) - (closeBtn.Size.Height / 2));

            // Create and add RadialMenu Control for 1st level
            var ctrl = radialMenuCtrl.First(level => level.Key.level == 1).Value;
            ctrl.setMenuForButtonID(null, 0); // Init buttons and model for 1st menu level

            // Add layout to content of form
            Content = layout;
        }

        /// <summary>
        /// Init menu level controls
        /// </summary>
        private void initLevels()
        {
            RadialMenuLevel mLevel = null;
            for (int i = 0; i < 3; i++)
            {
                int? innerRadius = mLevel != null ? mLevel.innerRadius + mLevel.thickness + 24 : defaultInnerRadius;
                mLevel = new RadialMenuLevel(i + 1, (int)innerRadius, defaultThickness);
                var ctrl = new SectorArcRadialControl(mLevel, 0);

                // Mouse events
                ctrl.onMouseEnterButton += radialControlMouseEnterButtonHandler;
                ctrl.onMouseOverButton += radialControlMouseOverButtonHandler;
                ctrl.onMouseLeaveButton += radialControlMouseLeaveButtonHandler;
                ctrl.onSelectionChanged += radialControlSelectionChangedHandler;
                ctrl.onFocusRequested += radialControlFocusRequestedHandler;
                // Drag Drop events
                ctrl.onButtonDragDrop += radialControlDragDropHandler;
                ctrl.onButtonDragEnter += radialControlDragEnterHandler;
                ctrl.onButtonDragOver += radialControlDragOverHandler;
                ctrl.onButtonDragLeave += radialControlDragLeaveHandler;
                ctrl.onDropTargetAccepted += radialControlAcceptDropHandler;
                radialMenuCtrl.Add(mLevel, ctrl);
            }
            //
            //REMARK Order is VERY important because arc button are rectangles. So if submenu level 2 is not "top most" (i.e. last position), drag over doesn't work and sometimes
            // drag over don't detect level "2" buttons
            //
            layout.Add(radialMenuCtrl.ElementAt(0).Value, (Size.Width / 2) - (radialMenuCtrl.ElementAt(0).Value.Size.Width / 2), Size.Height / 2 - (radialMenuCtrl.ElementAt(0).Value.Size.Height / 2));
            layout.Add(radialMenuCtrl.ElementAt(2).Value, (Size.Width / 2) - (radialMenuCtrl.ElementAt(2).Value.Size.Width / 2), Size.Height / 2 - (radialMenuCtrl.ElementAt(2).Value.Size.Height / 2));
            layout.Add(radialMenuCtrl.ElementAt(1).Value, (Size.Width / 2) - (radialMenuCtrl.ElementAt(1).Value.Size.Width / 2), Size.Height / 2 - (radialMenuCtrl.ElementAt(1).Value.Size.Height / 2));
        }

        #region Custom mouse over and leave behaviors
        //
        //  Mouse enter, over and leave handlers to prevent default radial control default behavior when needed (i.e. Avoid un-selecting button)
        //  Those custom behaviors will influence the event raising of "SelectionChanged", so the logic of displaying sub menu levels is managed in "onSelectionChanged" event
        //


        /// <summary>
        /// Mouse enter a button : Manage show/hide submenus
        /// </summary>
        /// <param name="radialControl"></param>
        /// <param name="args"></param>
        private void radialControlMouseEnterButtonHandler(SectorArcRadialControl radialControl, SectorArcRadialControl.SelectionArgs args)
        {
            Console.SetOut(RhinoApp.CommandLineOut);
            Console.WriteLine($"Mouse over button (form handler)");
            args.shouldUpdateSelection = false; // Don't apply radial control default behavior for mouse enter event
            if (args.buttonProps != null) // Ensure we have a selected button (should never happens)
            {

                var isOpenableButton = args.buttonProps?.model.data.properties.isActive & args.buttonProps?.model.data.properties.isFolder ?? false; // Ensure button contains submenu items

                // isOpenableButton = true; // DEBUG & TEST

                if (radialControl.level.level < maxLevels && isOpenableButton) // Nothing to do for last sub menu level (no show/hide submenus)
                {
                    // Do we have to show or hide submenu ?
                    var doHide = radialControl.selectedButtonID == args.buttonProps?.model.data.buttonID;

                    // Update radial control selected button
                    radialControl.selectedButtonID = radialControl.selectedButtonID == args.buttonProps?.model.data.buttonID ? "" : args.buttonProps?.model.data.buttonID; // Will trigger event <selectionchanged>

                    // Open or hide submenu
                    if (doHide)
                    {
                        hideSubmenu(radialControl);
                        radialControl.disableButtonsExceptSelection(); // Enable all buttons (because no ButtonID is selected)
                    }
                    else
                    {
                        showSubmenu(radialControl, args.buttonProps?.model);
                        radialControl.disableButtonsExceptSelection(); // Disable all buttons but the one selected
                    }
                }
            }
        }

        /// <summary>
        /// Mouse over a button
        /// </summary>
        /// <param name="radialControl"></param>
        /// <param name="args"></param>
        private void radialControlMouseOverButtonHandler(SectorArcRadialControl radialControl, SectorArcRadialControl.SelectionArgs args)
        {
        }

        /// <summary>
        /// Mouse leave a button
        /// </summary>
        /// <param name="radialControl"></param>
        /// <param name="args"></param>
        private void radialControlMouseLeaveButtonHandler(SectorArcRadialControl radialControl, SectorArcRadialControl.SelectionArgs args)
        {
            args.shouldUpdateSelection = false;
            // Dragging mode : Close sub menu when mouse leaves button
            // if (args != null && (!args.buttonProps?.buttonState.isDraggingIcon ?? false))
            // {
            //     hideSubmenu(radialControl);
            // }
        }
        #endregion

        /// <summary>
        /// TODO: Unused, to be removed
        /// </summary>
        /// <param name="radialControl"></param>
        /// <param name="args"></param>
        private void radialControlSelectionChangedHandler(SectorArcRadialControl radialControl, SectorArcRadialControl.SelectionArgs args)
        {
            // Console.SetOut(RhinoApp.CommandLineOut);
            // Console.WriteLine("Radial control selection changed");
        }
        private void radialControlFocusRequestedHandler(SectorArcRadialControl sender, SectorArcRadialControl.SelectionArgs args) { }
        /// <summary>
        /// Event when button request accept drop target. If drop target menu level is != from level of drag source AND it was a "self" drag, don't accept
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void radialControlAcceptDropHandler(SectorArcRadialControl sender, SectorArcButton.DropTargetArgs args)
        {
            if (args.sourceType == DragSourceTypes.self)
            {
                // if the level of the button source != target, don't accept drop
                var btn = (SectorArcButton)args.dragEventArgs.Source;
                try
                {
                    var keyValuePair = radialMenuCtrl.First((control) =>
                    {
                        if (control.Value.hasButton(btn))
                        {
                            return true;
                        }
                        return false;
                    });
                    if (keyValuePair.Value.level.level != sender.level.level)
                    {
                        args.acceptTarget = false;
                    }
                }
                catch { }
            }
        }
        /// <summary>
        /// Button properties were updated, dragging mode ended
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void radialControlDragDropHandler(SectorArcRadialControl sender, SectorArcRadialControl.SelectionArgs args)
        {
            // If icon is added to a menu higher than 1, update lower submenus so that they becomes "folder" and add an icon
            if (sender.level.level > 1)
            {
                for (var i = sender.level.level - 1; i >= 1; i--)
                {
                    var ctrl = radialMenuCtrl.First(entry => entry.Value.level.level == i).Value;
                    if (ctrl.selectedButtonID != "")// As we drag dropped in a submenu level, we should have a selected button ID
                    {
                        var btnProps = ctrl.getButtonProperties(ctrl.selectedButtonID);
                        btnProps.icon = new Icon(args.buttonProps?.model.data.properties.icon.Frames); // Duplicate dropped icon
                        btnProps.isFolder = true;
                        btnProps.isActive = true;
                        btnProps.rhinoScript = args.buttonProps?.model.data.properties.rhinoScript; // Keep rhino command in case this button is moved
                        ctrl.setButtonProperties(ctrl.selectedButtonID, btnProps);
                        ctrl.selectedButtonID = ""; // Unselect button
                    }
                }
            }
            // Hide all sub menus
            hideSubmenu(radialMenuCtrl.First(ctrl => ctrl.Key.level == 1).Value);
            sender.selectedButtonID = ""; // Unselect button
            sender.disableButtonsExceptSelection(); // Enable all buttons
            dragMode = false; // Ensure dragMode is reset
        }
        private void radialControlDragEnterHandler(SectorArcRadialControl radialControl, SectorArcRadialControl.SelectionArgs args)
        {
            // Update control selection + Will trigger event <selectionchanged>
            radialControl.selectedButtonID = args.buttonProps?.model.data.buttonID;

            // If entered a button menu level that can show higher level -> Show next level
            if (radialControl.level.level < maxLevels)
            {
                // Check next level is not already visible
                var nextLevel = radialControl.level.level + 1;
                // Update radial control selected button
                radialControl.selectedButtonID = args.buttonProps?.model.data.buttonID; // Update control selection + Will trigger event <selectionchanged>
                showSubmenu(radialControl, args.buttonProps?.model);

                //
                // Console.SetOut(RhinoApp.CommandLineOut);
                // Console.WriteLine($"show drag next level ${nextLevel}");
                //
                // Ensure no upper level is shown
                for (var i = maxLevels; i > nextLevel; i--)
                {
                    SectorArcRadialControl ctrl = radialMenuCtrl.First(control => control.Value.level.level == i).Value;
                    ctrl.selectedButtonID = ""; // clear selection
                    ctrl.show(false);
                }
            }
        }
        private void radialControlDragOverHandler(SectorArcRadialControl sender, SectorArcRadialControl.SelectionArgs args)
        {

        }
        private void radialControlDragLeaveHandler(SectorArcRadialControl sender, SectorArcRadialControl.SelectionArgs args)
        {

        }

        private void hideSubmenu(SectorArcRadialControl radialControl)
        {
            for (var i = maxLevels - 1; i > radialControl.level.level - 1; i--) // Hide each higher opened sub menu
            {
                var control = radialMenuCtrl.First(obj => obj.Value.level.level == i + 1).Value;
                control.disableButtonsExceptSelection(); // Enable all buttons
                control.show(false); // hide sub menu
            }
        }
        private void showSubmenu(SectorArcRadialControl radialControl, Model model)
        {
            var nextLevel = radialControl.level.level + 1; // Compute next level number
            var ctrl = radialMenuCtrl.First(obj => obj.Key.level == nextLevel).Value; //TODO: Do we need to check Radial control exists for the next level ?
            ctrl.setMenuForButtonID(model, model.data.sectorData.startAngle + ((model.data.sectorData.endAngle - model.data.sectorData.startAngle) / 2)); // Update radial control buttons
            ctrl.show(true); // Ensure control is visible
        }

        /// <summary>
        /// Do some cleanup of visual control before exit (Hide) the plugin Form object
        /// </summary>
        /// <param name="sender"></param>
        protected override void onCloseClickEvent(object sender)
        {
            foreach (var control in radialMenuCtrl.Values)
            {
                control.show(false);
                control.disableButtonsExceptSelection();
            }
            base.onCloseClickEvent(sender);
        }
    }
}
