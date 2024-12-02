using System;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using Eto.Drawing;
using Eto.Forms;
using Rhino;
using Rhino.PlugIns;

namespace customControls
{

    public class SectorRadialMenuForm : TransparentForm
    {
        protected static int defaultInnerRadius = 70;
        protected static int defaultThickness = 40;
        protected static int maxLevels = 3;
        private PixelLayout layout = new PixelLayout();

        /// <summary>
        /// Associate a menu level to a Control of type <SectorArcRadialControl>
        /// </summary>
        private Dictionary<RadialMenuLevel, SectorArcRadialControl> radialMenuCtrl = new Dictionary<RadialMenuLevel, SectorArcRadialControl>();
        public SectorRadialMenuForm(PlugIn plugin) : base(plugin)
        {
            Size = new Size(500, 500);
            initLevels();

            // Allow drop just to remove drop animation when icon is dragged outside button
            AllowDrop = true;
            DragEnter += (o, e) =>
            {
                e.Effects = DragEffects.Move;
            };
            // Fake drop to remove animation
            DragDrop += (o, e) =>
            {
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
            ctrl.setMenuForButtonID(null, 0);

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

                // Event handlers
                ctrl.onMouseEnterButton += onRadialControlMouseEnterButton;
                ctrl.onMouseOverButton += onRadialControlMouseOverButton;
                ctrl.onMouseLeaveButton += onRadialControlMouseLeaveButton;
                ctrl.onSelectionChanged += onRadialControlSelectionChanged;
                ctrl.onFocusRequested += onRadialControlFocusRequested;
                ctrl.onNewIconAdded += onRadialControlNewIconAdded;
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
        private void onRadialControlMouseEnterButton(SectorArcRadialControl radialControl, SectorArcRadialControl.SelectionArgs args)
        {
            args.shouldUpdateSelection = false; // Don't apply radial control default behavior for mouse enter event
            if (args.buttonProps != null) // Ensure we have a selected button (should never happens)
            {
                if (args.buttonProps?.buttonState.isDraggingIcon == false) // We are not in dragging mode -> show submenu if button has submenu to display
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
                else // Dragging mode -> Show submenu 
                {
                    if (radialControl.level.level < maxLevels)
                    {
                        // Check next level is not already visible
                        var nextLevel = radialControl.level.level + 1;
                        // Update radial control selected button
                        radialControl.selectedButtonID = args.buttonProps?.model.data.buttonID; // Update control selection + Will trigger event <selectionchanged>
                        showSubmenu(radialControl, args.buttonProps?.model);

                        Console.SetOut(RhinoApp.CommandLineOut);
                        Console.WriteLine($"show drag next level ${nextLevel}");
                        // Ensure no upper level is shown
                        for (var i = maxLevels ; i > nextLevel ; i--)
                        {
                            SectorArcRadialControl ctrl = radialMenuCtrl.First(control => control.Value.level.level == i).Value;
                            ctrl.selectedButtonID = ""; // clear selection
                            ctrl.show(false);
                            // hideSubmenu(ctrl);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Mouse over a button
        /// </summary>
        /// <param name="radialControl"></param>
        /// <param name="args"></param>
        private void onRadialControlMouseOverButton(SectorArcRadialControl radialControl, SectorArcRadialControl.SelectionArgs args)
        {
        }

        /// <summary>
        /// Mouse leave a button
        /// </summary>
        /// <param name="radialControl"></param>
        /// <param name="args"></param>
        private void onRadialControlMouseLeaveButton(SectorArcRadialControl radialControl, SectorArcRadialControl.SelectionArgs args)
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
        private void onRadialControlSelectionChanged(SectorArcRadialControl radialControl, SectorArcRadialControl.SelectionArgs args)
        {
            // Console.SetOut(RhinoApp.CommandLineOut);
            // Console.WriteLine("Radial control selection changed");
        }
        private void onRadialControlFocusRequested(SectorArcRadialControl sender, SectorArcRadialControl.SelectionArgs args) { }
        private void onRadialControlNewIconAdded(SectorArcRadialControl sender, SectorArcRadialControl.SelectionArgs args) { }

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
