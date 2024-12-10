using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Rhino;
using Rhino.PlugIns;

namespace customControls
{
    public class SettingsHelper
    {
        public static SettingsHelper Instance { get; private set; }
        protected PlugIn plugin;
        public SettingsClass settings = new SettingsClass();

        public SettingsHelper(PlugIn plugin)
        {
            this.plugin = plugin;
            Instance = this;
        }

        /// <summary>
        /// Get the persistent settings node for a button. If node doesn't exist, it'll be created.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="parent">Parent node of the button. If none provided, node will be retrieved or created at the root of settings</param>
        /// <returns></returns>
        public PersistentSettings getChildNode(string id, PersistentSettings parent = null)
        {
            if (parent == null)
            {
                getChild(plugin.Settings, "buttons", out parent);
            }
            // Find the node in settings children
            getChild(parent, id, out var node);
            return node;
        }

        /// <summary>
        /// Return properties from Rhino Settings file. If no properties exists, return a new and empty properties object
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public ButtonProperties getProperties(PersistentSettings node)
        {
            ButtonProperties properties = null;
            foreach (var k in node.Keys)
            {
                if (node.TryGetStringDictionary(k, out var props))
                {
                    var propsDic = new Dictionary<string, string>(props);
                    properties = new ButtonProperties();
                    properties.icon = propsDic["iconPath"] != "" ? new Icon(1, new Bitmap(propsDic["iconPath"])) : null;
                    properties.rhinoScript = propsDic["rhinoScript"];
                    properties.isActive = propsDic["isActive"] == "true" ? true : false;
                    properties.isFolder = propsDic["isFolder"] == "true" ? true : false;
                }
            }
            return properties == null ? new ButtonProperties() : properties;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="model"></param>
        public void setProperties(PersistentSettings node, Model model)
        {
            // Build icon file name
            var filename = model.data.buttonID;
            var parentModel = model.Parent;
            while (parentModel != null)
            {
                filename = parentModel.data.buttonID + "_" + filename;
                parentModel = parentModel.Parent;
            }

            // Compute icon path and save file to disk
            var iconPath = "";
            if (model.data.properties.icon != null)
            {
                iconPath = plugin.SettingsDirectoryAllUsers + "/icons/";
                System.IO.Directory.CreateDirectory(iconPath); // Create directory if does not exist
                iconPath += filename + ".png";
                model.data.properties.icon.GetFrame(1).Bitmap.Save(iconPath, ImageFormat.Png);
            }

            // Update Rhino settings
            var settingsProps = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("iconPath",iconPath),
                new KeyValuePair<string, string>("rhinoScript",model.data.properties.rhinoScript),
                new KeyValuePair<string, string>("isActive",model.data.properties.isActive==true?"true":"false"),
                new KeyValuePair<string, string>("isFolder",model.data.properties.isFolder==true?"true":"false"),
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











        public void load()
        {
            // Theme
            getChild(plugin.Settings, "theme", out var theme);
            getThemeSettings(theme, "normal", ref settings.buttonColors.normal.pen, ref settings.buttonColors.normal.fill);
            getThemeSettings(theme, "hover", ref settings.buttonColors.hover.pen, ref settings.buttonColors.hover.fill);
            getThemeSettings(theme, "drag", ref settings.buttonColors.drag.pen, ref settings.buttonColors.drag.fill);
            getThemeSettings(theme, "disable", ref settings.buttonColors.disabled.pen, ref settings.buttonColors.disabled.fill);

            // Button icons path
            getChild(plugin.Settings, "buttons", out var btns);
            getChild(btns, "properties", out var btnProps);
            foreach (var k in btnProps.Keys)
            {
                if (btnProps.TryGetStringDictionary(k, out var props))
                {
                    var propsDic = new Dictionary<string, string>(props);
                    var properties = new ButtonProperties();
                    properties.icon = new Icon(1, new Bitmap(propsDic["iconPath"]));
                    properties.rhinoScript = propsDic["rhinoScript"];
                    properties.isActive = propsDic["isActive"] == "true" ? true : false;
                    settings.buttonProperties.Add(k, properties);
                }
                else
                {
                    settings.buttonProperties.Add(k, new ButtonProperties());
                }
            }
        }
        public void save()
        {
            getChild(plugin.Settings, "theme", out var theme);
            setThemeSettings(theme, "normal", settings.buttonColors.normal.pen, settings.buttonColors.normal.fill);
            setThemeSettings(theme, "hover", settings.buttonColors.hover.pen, settings.buttonColors.hover.fill);
            setThemeSettings(theme, "drag", settings.buttonColors.drag.pen, settings.buttonColors.drag.fill);
            setThemeSettings(theme, "disable", settings.buttonColors.disabled.pen, settings.buttonColors.disabled.fill);
        }
        /// <summary>
        /// Save a button icon to plugin folder, update plugin settings, update settings object
        /// </summary>
        /// <param name="buttonID"></param>
        /// <param name="props"></param>
        public void saveButtonProperties(string buttonID, ButtonProperties props)
        {
            getChild(plugin.Settings, "buttons", out var btns);
            getChild(btns, "properties", out var btnProps);
            var iconPath = plugin.SettingsDirectoryAllUsers + "/icons/";
            System.IO.Directory.CreateDirectory(iconPath); // Create directory if does not exist
            iconPath += buttonID + ".png";
            props.icon.GetFrame(1).Bitmap.Save(iconPath, ImageFormat.Png);

            var settingsProps = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("iconPath",iconPath),
                new KeyValuePair<string, string>("rhinoScript",props.rhinoScript),
                new KeyValuePair<string, string>("isActive",props.isActive==true?"true":"false"),
                new KeyValuePair<string, string>("isFolder",props.isFolder==true?"true":"false"),
            };


            // upate settings dictionnary

            // remove dictionnary if already created
            if (btnProps.TryGetStringDictionary(buttonID, out var fake))
            {
                btnProps.DeleteItem(buttonID);
            }
            // set new dictionary values
            btnProps.SetStringDictionary(buttonID, settingsProps.ToArray());
            if (settings.buttonProperties.ContainsKey(buttonID)) settings.buttonProperties[buttonID] = props; else settings.buttonProperties.Add(buttonID, props);
        }
        private void setThemeSettings(PersistentSettings theme, string key, Color pen, Color fill)
        {
            PersistentSettings childThemeSettings;
            getChild(theme, key, out childThemeSettings);
            childThemeSettings.SetColor("pen", System.Drawing.Color.FromArgb(pen.ToArgb()));
            childThemeSettings.SetColor("fill", System.Drawing.Color.FromArgb(fill.ToArgb()));
        }

        /// <summary>
        /// Get a theme setting by key. If key doesn't exist, will not modify "ref" parameters (they should have default value)
        /// </summary>
        /// <param name="theme">Root PersistentSettings object that contains child</param>
        /// <param name="key">Theme key name</param>
        /// <param name="pen">Ref to a Color object</param>
        /// <param name="fill">Ref to a Color object</param>
        private void getThemeSettings(PersistentSettings theme, string key, ref Color pen, ref Color fill)
        {
            PersistentSettings childThemeSettings;
            System.Drawing.Color penColor;
            System.Drawing.Color fillColor;
            if (theme.TryGetChild(key, out childThemeSettings))
            {
                if (childThemeSettings.TryGetColor("pen", out penColor) && childThemeSettings.TryGetColor("fill", out fillColor))
                {
                    pen = Color.FromArgb(penColor.ToArgb());
                    fill = Color.FromArgb(fillColor.ToArgb());
                }
            }
        }













        /// <summary>
        /// Try to get a settings child node. If child doesn't exist, it will be create
        /// </summary>
        /// <param name="root"></param>
        /// <param name="childName"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        private bool getChild(PersistentSettings root, string childName, out PersistentSettings child)
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