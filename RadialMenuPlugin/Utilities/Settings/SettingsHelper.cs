using System.Collections.Generic;
using Eto.Drawing;
using Rhino;
using Rhino.PlugIns;
using RadialMenuPlugin.Data;

namespace RadialMenuPlugin.Utilities.Settings
{
    public class SettingsHelper
    {
        /// <summary>
        /// 
        /// </summary>
        public static SettingsHelper Instance { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        public SettingsClass Settings = new SettingsClass();
        
        /// <summary>
        /// 
        /// </summary>
        protected PlugIn _Plugin;

        public SettingsHelper(PlugIn plugin)
        {
            _Plugin = plugin;
            Instance = this;
        }

        /// <summary>
        /// Get the persistent settings node for a button. If node doesn't exist, it'll be created.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="parent">Parent node of the button. If none provided, node will be retrieved or created at the root of settings</param>
        /// <returns></returns>
        public PersistentSettings GetChildNode(string id, PersistentSettings parent = null)
        {
            if (parent == null)
            {
                GetChild(_Plugin.Settings, "buttons", out parent);
            }
            // Find the node in settings children
            GetChild(parent, id, out var node);
            return node;
        }

        /// <summary>
        /// Return properties from Rhino Settings file. If no properties exists, return a new and empty properties object
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public ButtonProperties GetProperties(PersistentSettings node)
        {
            ButtonProperties properties = null;
            foreach (var k in node.Keys)
            {
                if (node.TryGetStringDictionary(k, out var props))
                {
                    var propsDic = new Dictionary<string, string>(props);
                    properties = new ButtonProperties();
                    properties.Icon = propsDic["iconPath"] != "" ? new Icon(1, new Bitmap(propsDic["iconPath"])) : null;
                    properties.RhinoScript = propsDic["rhinoScript"];
                    properties.IsActive = propsDic["isActive"] == "true" ? true : false;
                    properties.IsFolder = propsDic["isFolder"] == "true" ? true : false;
                }
            }
            return properties == null ? new ButtonProperties() : properties;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="model"></param>
        public void SetProperties(PersistentSettings node, Model model)
        {
            // Build icon file name
            var filename = model.Data.ButtonID;
            var parentModel = model.Parent;
            while (parentModel != null)
            {
                filename = parentModel.Data.ButtonID + "_" + filename;
                parentModel = parentModel.Parent;
            }

            // Compute icon path and save file to disk
            var iconPath = "";
            if (model.Data.Properties.Icon != null)
            {
                iconPath = _Plugin.SettingsDirectoryAllUsers + "/icons/";
                System.IO.Directory.CreateDirectory(iconPath); // Create directory if does not exist
                iconPath += filename + ".png";
                model.Data.Properties.Icon.GetFrame(1).Bitmap.Save(iconPath, ImageFormat.Png);
            }

            // Update Rhino settings
            var settingsProps = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("iconPath",iconPath),
                new KeyValuePair<string, string>("rhinoScript",model.Data.Properties.RhinoScript),
                new KeyValuePair<string, string>("isActive",model.Data.Properties.IsActive==true?"true":"false"),
                new KeyValuePair<string, string>("isFolder",model.Data.Properties.IsFolder==true?"true":"false"),
            };
            // upate settings dictionnary
            // remove dictionnary if already created
            if (node.TryGetStringDictionary("properties", out var fake))
            {
                node.DeleteItem("properties");
            }
            // set new dictionary values
            node.SetStringDictionary("properties", settingsProps.ToArray());
        }

        /// <summary>
        /// Try to get a settings child node. If child doesn't exist, it will be create
        /// </summary>
        /// <param name="root"></param>
        /// <param name="childName"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        private bool GetChild(PersistentSettings root, string childName, out PersistentSettings child)
        {
            var exist = root.TryGetChild(childName, out child);
            if (exist == false)
            {
                child = root.AddChild(childName);
            }
            return exist;
        }
    }
}