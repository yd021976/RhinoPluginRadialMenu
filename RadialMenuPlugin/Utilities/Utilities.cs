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
                    // Get the macro of the dropped toolbar item
                    var lMacro = obj.GetType().GetProperty("LeftMacro").GetValue(obj, null);

                    // Seems that "CreateIcon" is a good condidate to get the Rhino toolbar item icon
                    var iconCreateMethod = lMacro.GetType().GetMethod("CreateIcon");
                    var icon = (Icon)iconCreateMethod?.Invoke(lMacro, new object[] { new Size(28, 28), true });
                    if (icon != null)
                    {
                        // Get the macro "script" command
                        var macroScript = lMacro.GetType().GetProperty("Script").GetValue(lMacro, null);
                        // Get macro GUID
                        var macroGuidProperty = lMacro.GetType().GetProperty("Id");
                        var GUID = macroGuidProperty.GetValue(lMacro, null);
                        return new ButtonProperties((string)macroScript, icon, true, false, (Guid)GUID);
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