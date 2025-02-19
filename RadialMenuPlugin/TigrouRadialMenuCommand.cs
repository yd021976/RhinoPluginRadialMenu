using Rhino;
using Rhino.Commands;
using RadialMenuPlugin.Controls;
using Eto.Forms;
using Rhino.UI;
using System.Collections.Generic;
using NLog;
using System;

namespace RadialMenuPlugin
{
    public class RadialMenuCommand : Rhino.Commands.Command
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Maintain list of plugin form instance associated to each opened Rhino doc (Window)
        /// </summary>
        protected Dictionary<RhinoDoc, TransparentForm> PluginForms = new Dictionary<RhinoDoc, TransparentForm>();
        public RadialMenuCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;

            // When ESC is pressed in rhino AND command is running -> Send keyUp to radial menu
            RhinoApp.EscapeKeyPressed += (s, e) =>
            {
                TransparentForm form = GetFormInstance(RhinoDoc.ActiveDoc);
                if (form != null)
                {
                    form = PluginForms[RhinoDoc.ActiveDoc];
                    HandleKeyPress(form, e);
                }
            };

            /**
            Remove radial menu form instance when document is closed
            **/
            RhinoDoc.CloseDocument += (s, e) =>
            {
                if (e.Document != null)
                {
                    if (PluginForms.ContainsKey(e.Document))
                    {
                        PluginForms[e.Document].Close();
                        PluginForms[e.Document].Dispose();
                        PluginForms.Remove(e.Document);
                    }
                }
            };
        }

        ///<summary>The only instance of this command.</summary>
        public static RadialMenuCommand Instance { get; private set; }

        /// <summary>
        /// Name of the command in Rhino
        /// </summary>
        /// <returns>
        /// The command name as it appears on the Rhino command line.
        /// </returns>
        public override string EnglishName => "TigrouRadialMenu";

        /// <summary>
        /// Shows radial menu at mouse cursor position
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            TransparentForm form = GetFormInstance(doc);
            if (form != null)
            {
                var m = Mouse.Position;
                var formSize = form.Size;
                form.Location = new Eto.Drawing.Point((int)m.X - (formSize.Width / 2), (int)m.Y - (formSize.Height / 2));
                form.Show();
            }
            else
            {
                Logger.Error($"No radial menu form instance found");
            }
            return Result.Nothing;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="form"></param>
        /// <param name="e"></param>
        protected void HandleKeyPress(TransparentForm form, EventArgs e)
        {
            if (form != null && form.Visible) // If radial menu exist and is showing
            {
                form.KeyPress(new KeyEventArgs(Keys.Escape, KeyEventType.KeyDown)); // Send ESC key up event to form
            }
        }

        /// <summary>
        /// Get radial menu form instance for the provided document. If no form instance exists, create a new one
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        protected TransparentForm GetFormInstance(RhinoDoc document)
        {
            TransparentForm form = null;

            if (document != null)
            {
                if (PluginForms.ContainsKey(document))
                {
                    form = PluginForms[document];
                    Logger.Info($"Found radial menu form instance for document {document.RuntimeSerialNumber}");
                }
                else
                {
                    var window = RhinoEtoApp.MainWindowForDocument(document);
                    form = new RadialMenuForm(PlugIn, window);
                    PluginForms.Add(document, form);
                    Logger.Info($"Create new radial menu form instance for document {document.RuntimeSerialNumber}");
                }
            }
            else
            {
                Logger.Error("Document is null, no radial menu form instance found or created");
            }
            return form;
        }
    }
}