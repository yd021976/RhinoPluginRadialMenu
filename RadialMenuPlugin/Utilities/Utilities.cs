using Eto.Drawing;
using Eto.Forms;
using RadialMenuPlugin.Data;
using RadialMenuPlugin.Controls.Buttons.MenuButton;
using System;

namespace RadialMenuPlugin.Utilities
{
    namespace Events
    {
        /// <summary>
        /// DragDrop event handler
        /// </summary>
        /// <typeparam name="TSender"></typeparam>
        /// <typeparam name="TArgs"></typeparam>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public delegate void AppEventHandler<in TSender, in TArgs>(TSender sender, TArgs args);
        public delegate void AppEventHandler<in TSender>(TSender sender);
    }
    public static class DragDropUtilities
    {
        public static readonly Size IconSize = new Size(28, 28);
        /// <summary>
        /// Logger
        /// </summary>
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        ///  <para>When a drop event occurs (dragLeave), check if drop item is a Rhino toolbar item</para>
        ///  <para>If so, try to get the script command macro and the associated icon</para>
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static ButtonProperties getDroppedToolbarItem(DragEventArgs e)
        {
            var obj = e.Source;
            if (obj != null)
            {
                if (obj.GetType().ToString() == "Rhino.UI.Internal.TabPanels.Controls.ToolBarControlItem")
                {
                    try
                    {
                        // Get the macro of the dropped toolbar item
                        var lMacro = obj.GetType().GetProperty("LeftMacro").GetValue(obj, null);
                        var rMacro = obj.GetType().GetProperty("RightMacro").GetValue(obj, null);

                        // Seems that "CreateIcon" is a good condidate to get the Rhino toolbar item icon
                        var iconCreateMethod = lMacro.GetType().GetMethod("CreateIcon");
                        var icon = (Icon)iconCreateMethod?.Invoke(lMacro, new object[] { IconSize, true });
                        if (icon != null)
                        {
                            // Get the LEFT macro "script" and tooltip
                            var leftMacroScript = ""; var leftMacroTooltip = "";
                            if (lMacro != null)
                            {
                                leftMacroScript = (string)lMacro.GetType().GetProperty("Script").GetValue(lMacro, null);
                                leftMacroTooltip = (string)lMacro.GetType().GetProperty("HelpText").GetValue(lMacro, null);
                            }

                            // Get the RIGHT macro "script" and tooltip
                            var rightMacroScript = ""; var rightMacroTooltip = "";
                            if (rMacro != null)
                            {
                                rightMacroScript = (string)rMacro.GetType().GetProperty("Script").GetValue(rMacro, null);
                                rightMacroTooltip = (string)rMacro.GetType().GetProperty("HelpText").GetValue(rMacro, null);

                            }
                            // Get macro GUID
                            var macroGuidProperty = lMacro.GetType().GetProperty("Id");
                            var GUID = macroGuidProperty.GetValue(lMacro, null);

                            return new ButtonProperties(
                                new Macro(leftMacroScript, leftMacroTooltip),
                                new Macro(rightMacroScript, rightMacroTooltip),
                                icon, true, false, (Guid)GUID);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    catch (Exception exception)
                    {
                        Logger.Fatal(exception);
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
        /// Get type of object dragged into this control
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static DragSourceType dragSourceType(Control source)
        {
            if (source.GetType() == typeof(MenuButton))
            {
                return DragSourceType.radialMenuItem;
            }
            else if (source.GetType().ToString() == "Rhino.UI.Internal.TabPanels.Controls.ToolBarControlItem")
            {
                return DragSourceType.rhinoItem;
            }
            else
            {
                return DragSourceType.unknown;
            }
        }
    }
}