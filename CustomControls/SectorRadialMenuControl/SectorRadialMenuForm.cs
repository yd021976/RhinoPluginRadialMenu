using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using AppKit;
using Eto.Drawing;
using Eto.Forms;
using RadialMenu;
using Rhino;
using Rhino.PlugIns;
using Rhino.UI;

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
                foreach (var ctrl in radialMenuCtrl)
                {
                    if (ctrl.Value.level.level > 1)
                    {
                        ctrl.Value.Visible = false;
                    }
                }
                var radialControl = radialMenuCtrl.First(obj => obj.Key.level == 1).Value;
                radialControl.Visible = true;
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
            ctrl.setMenuForButtonID("default", 0);

            // Add layout to content of form
            Content = layout;
        }

        private void initLevels()
        {
            RadialMenuLevel mLevel = null;
            for (int i = 0; i < 3; i++)
            {
                int? innerRadius = mLevel != null ? mLevel.innerRadius + mLevel.thickness + 24 : defaultInnerRadius;
                mLevel = new RadialMenuLevel(i + 1, (int)innerRadius, defaultThickness);
                var ctrl = new SectorArcRadialControl(mLevel, 0);
                ctrl.Visible = false;

                // Event handlers
                ctrl.onMouseLeaveButton += onRadialControlMouseLeave;
                ctrl.onMouseOverButton += onRadialControlMouseOver;
                ctrl.onFocusRequested += onRadialControlFocusRequested;
                ctrl.onNewIconAdded += onRadialControlNewIconAdded;

                radialMenuCtrl.Add(mLevel, ctrl);
                layout.Add(ctrl, (Size.Width / 2) - (ctrl.Size.Width / 2), Size.Height / 2 - (ctrl.Size.Height / 2));
            }
        }
        private void onRadialControlMouseOver(SectorArcRadialControl radialControl, SectorArcRadialControlEventArgs args)
        {
            if (!args.isDragging)
            {
                showNextLevel(radialControl, args.button, args.model); // Show next menu level if applicable
            }
            // We are in dragging mode
            else { }
        }
        private void onRadialControlMouseLeave(SectorArcRadialControl sender, SectorArcRadialControlEventArgs args) { }
        private void onRadialControlFocusRequested(SectorArcRadialControl sender, SectorArcRadialControlEventArgs args) { }
        private void onRadialControlNewIconAdded(SectorArcRadialControl sender, SectorArcRadialControlEventArgs args) { }
        private void showNextLevel(SectorArcRadialControl radialControl, SectorArcButton button, ButtonModel model)
        {
            // if (model.properties.isActive && model.properties.isFolder)
            if (true)
            {
                if (radialControl.level.level < maxLevels) // check we didn't reach the max submenu levels
                {
                    if (button.ID == radialControl.selectedButtonID && radialControl.selectedButtonID != "")
                    { // We hover over the selected button ID -> Hide sub menu and un select button
                        for (var i = maxLevels - 1; i > radialControl.level.level - 1; i--) // Hide each higher opened sub menu
                        {
                            var control = radialMenuCtrl.First(obj => obj.Value.level.level == i + 1).Value;
                            control.selectButtonID(); // Unselect button
                            control.Visible = false; // hide sub menu
                        }
                        radialControl.selectButtonID(""); // Unselect any button ID
                    }
                    else
                    {
                        var nextLevel = radialControl.level.level + 1; // Compute next level number
                        var ctrl = radialMenuCtrl.First(obj => obj.Key.level == nextLevel).Value; //TODO: Do we need to check Radial control exists for the next level ?

                        ctrl.setMenuForButtonID(button.ID, model.sectorData.startAngle + ((model.sectorData.endAngle - model.sectorData.startAngle) / 2)); // Update radial control buttons
                        layout.Move(ctrl, (Size.Width / 2) - (ctrl.Size.Width / 2), Size.Height / 2 - (ctrl.Size.Height / 2)); // Update control position
                        
                        // disable previous menu (i.e. arc sectors buttons control)
                        radialControl.selectButtonID(button.ID);
                        ctrl.Visible = true; // Ensure control is visible
                    }
                }
            }
        }

        /// <summary>
        /// Do some cleanup of visual control before exit (Hide) the plugin Form object
        /// </summary>
        /// <param name="sender"></param>
        protected override void onCloseClickEvent(object sender)
        {
            foreach (var control in radialMenuCtrl.Values)
            {
                if (control.level.level == 1)
                {
                    control.Visible = true;
                }
                else
                {
                    control.Visible = false;
                }
                control.selectButtonID();
            }
            base.onCloseClickEvent(sender);
        }
    }
}
