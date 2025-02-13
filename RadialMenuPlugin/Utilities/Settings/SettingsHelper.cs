using System.Collections.Generic;
using Eto.Drawing;
using Rhino;
using Rhino.PlugIns;
using RadialMenuPlugin.Data;
using System;

namespace RadialMenuPlugin.Utilities.Settings
{
    /**
    Structure of Rhino Settings
    ---------------------------

    <xml>
        <Buttons>
            <String GUID>
                <ButtonID>ID of the button</ButtonID>
                <Properties>
                    <isFolder>true or false</isFolder>
                    ...
                    <isActive>true or false</isActive>
                </Properties>
        </buttons>
        <Theme>TO BE DEFINED</Theme>
        <General>TO BE DEFINED</General>
    **/
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
        protected PersistentSettings rootSettings { get => _Plugin.Settings; }
        protected readonly string c_IconExtension = ".png";
        protected string _IconStoragePath
        {
            get
            {
                var iconPath = _Plugin.SettingsDirectoryAllUsers + "/icons/";
                System.IO.Directory.CreateDirectory(iconPath); // Create directory if does not exist
                return iconPath;
            }
        }
        public SettingsHelper(PlugIn plugin)
        {
            _Plugin = plugin;
            Instance = this;
        }
        /// <summary>
        /// Get a root (level 1) node in persistent settings
        /// </summary>
        /// <param name="settingsDomainName">Persistent setting domain name</param>
        /// <param name="rootSettingsNode">Out value of Persistent settings</param>
        /// <param name="create">Create the node if true</param>
        /// <returns></returns>
        public bool GetSettingsRoot(SettingsDomain settingsDomainName, out PersistentSettings rootSettingsNode, bool create = false)
        {
            var exist = _Plugin.Settings.TryGetChild(settingsDomainName.ToString(), out rootSettingsNode);
            if (!exist && create)
            {
                rootSettingsNode = _Plugin.Settings.AddChild(settingsDomainName.ToString());
                exist = true;
            }
            return exist;
        }
        /// <summary>
        /// Get a persistent settings node by model GUID
        /// </summary>
        /// <param name="modelGuid">GUID to find</param>
        /// <param name="parent">Parent node</param>
        /// <param name="create">True if the node should be create if not exist</param>
        /// <returns></returns>
        public PersistentSettings GetNode(Guid modelGuid, PersistentSettings parent = null, bool create = false)
        {
            _GetChild(out var node, modelGuid, parent); // Get root node

            // Create the node if it is not found and creation is requested
            if (node == null && create)
            {
                if (parent == null) parent = _Plugin.Settings; // If no parent, set node to Rhino Settings root
                node = parent.AddChild(modelGuid.ToString());
            }
            return node;
        }
        /// <summary>
        /// Get children of a node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<KeyValuePair<string, PersistentSettings>> GetChildren(PersistentSettings node)
        {
            List<KeyValuePair<string, PersistentSettings>> children = new List<KeyValuePair<string, PersistentSettings>>();
            foreach (var childKey in node.ChildKeys)
            {
                if (node.TryGetChild(childKey, out var childNode))
                {
                    children.Add(new KeyValuePair<string, PersistentSettings>(childKey, childNode));
                }
            }
            return children;
        }
        /// <summary>
        /// Add a child node to a prent node. If <paramref name="modelGuid"/> already exists, it will be returned and no new node we'll be created
        /// </summary>
        /// <param name="modelGuid"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public PersistentSettings AddNode(Guid modelGuid, PersistentSettings parent = null)
        {
            if (parent == null)
            {
                parent = rootSettings;
            }
            return parent.AddChild(modelGuid.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelGuid"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public void RemoveNode(Guid modelGuid, PersistentSettings parent = null)
        {
            if (parent == null)
            {
                parent = rootSettings;
            }
            parent.DeleteChild(modelGuid.ToString());
        }
        /// <summary>
        /// Return properties from Rhino Settings file. If no properties exists, return a new and empty properties object
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public ButtonModelData GetData(PersistentSettings node)
        {
            ButtonModelData modelData = null;
            var hasButtonID = node.TryGetString("ButtonID", out var buttonID); // Get button ID
            var hasProperties = node.TryGetStringDictionary("Properties", out var properties); // Get model properties
            modelData = new ButtonModelData(new Dictionary<string, string>(properties));
            modelData.ButtonID = buttonID;
            // Create Icon
            if (modelData.Properties.CommandGUID != Guid.Empty)
            {
                if (System.IO.File.Exists(_IconFileFullPath(modelData)))
                    modelData.Properties.Icon = new Icon(_IconFileFullPath(modelData)); // Create icon from file
                else // if icon filename doesn't exist : Set icon to "question mark" icon
                {
                    var img = Bitmap.FromResource("TigrouRadialMenu.Bitmaps.question-mark-circle-outline-icon.png");
                    modelData.Properties.Icon = img.WithSize(16, 16);
                }
            }
            return modelData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="model"></param>
        public void SetProperties(PersistentSettings node, ButtonModelData modelData)
        {
            // Update settings XML file
            node.SetString("ButtonID", modelData.ButtonID);
            var list = modelData.Properties.toList();
            node.SetStringDictionary("Properties", list.ToArray());

            // Save icon file if not already exist
            // REMARK: The <SetProperties> method is called each time a property changed in<ButtonModelData>. So it is possible <CommandGUID> is not already sets
            if (modelData.Properties.Icon != null) // Check button has an icon to save
            {
                if (modelData.Properties.CommandGUID != Guid.Empty) // Check we have a valid GUID
                {
                    if (!System.IO.File.Exists(_IconFileFullPath(modelData)))
                    {
                        new Bitmap(modelData.Properties.Icon).Save(_IconFileFullPath(modelData), ImageFormat.Png);
                    }
                }
            }
        }
        /// <summary>
        /// Find a node by Guid, starting from <paramref name="parent"/> node
        /// </summary>
        /// <param name="parent">Root node to start child search</param>
        /// <param name="guid">GUID of the child</param>
        /// <param name="child">PersistentSettings out value of found child node</param>
        /// <returns></returns>
        private bool _GetChild(out PersistentSettings child, Guid guid, PersistentSettings parent = null)
        {
            // Init root if null
            if (parent == null)
            {
                parent = _Plugin.Settings; // Start from plugin root settings
            }

            // // Loop over children nodes to find GUID node
            foreach (var k in parent.ChildKeys)
            {
                var exist = parent.TryGetChild(k, out child); // Get child node 
                if (k == guid.ToString()) return exist; // We found the child
                return _GetChild(out child, guid, child); // Try to find GUID in children of child node
            }
            // We didn't found the GUID in settings
            child = null;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected string _IconFileFullPath(ButtonModelData data)
        {
            return _IconStoragePath + _IconFileName(data);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected string _IconFileName(ButtonModelData data)
        {
            return data.Properties.CommandGUID.ToString() + c_IconExtension;
        }
    }
}