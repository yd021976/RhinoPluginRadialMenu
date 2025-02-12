using System;
using System.Collections.Generic;
using System.ComponentModel;
using RadialMenuPlugin.Utilities.Settings;
using Rhino;

namespace RadialMenuPlugin.Data
{
    /// <summary>
    /// Struct to pass as argument for Model constructor
    /// </summary>
    public struct RhinoPersistentSettingsCtor
    {
        /// <summary>
        /// Model GUID string representation
        /// </summary>
        public string Guid;
        /// <summary>
        /// Button ID associated to this model
        /// </summary>
        public string ButtonID;
        public RhinoPersistentSettingsCtor(string guid, string buttonID)
        {
            Guid = guid;
            ButtonID = buttonID;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class ButtonModelData : INotifyPropertyChanged
    {
        #region Notify interface implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void _OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion


        /// <summary>
        /// ID of the button associated to this sector data
        /// </summary>
        string _ButtonID = "";
        public string ButtonID
        {
            get => _ButtonID;
            set
            {
                _ButtonID = value;
            }
        }

        ButtonProperties _Properties = new ButtonProperties();
        public ButtonProperties Properties
        {
            get => _Properties;
            set
            {
                _Properties = value; // Update property object
                _OnPropertyChanged(nameof(Properties)); // Notify property changed
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public ButtonModelData() : base() { }
        /// <summary>
        /// Constructor from Rhino persistent settings
        /// </summary>
        /// <param name="rhinoSettingsData"></param>
        public ButtonModelData(Dictionary<string, string> rhinoSettingsData) : this()
        {
            _Properties = new ButtonProperties(rhinoSettingsData);
        }
    }
    /// <summary>
    /// Model class for "arc buttons"
    /// </summary>
    public class Model : INotifyPropertyChanged
    {
        /// <summary>
        /// Parent model
        /// </summary>
        public Model Parent;
        /// <summary>
        /// Unique identifier of the model
        /// </summary>
        public Guid GUID;
        ButtonModelData _Data = new ButtonModelData();

        public ButtonModelData Data
        {
            get
            {
                return _Data;
            }
            set
            {
                _Data.PropertyChanged -= _OnDataChanged;
                _Data.Properties.PropertyChanged -= _OnPropertiesChanged;
                _Data = value;
                _Data.PropertyChanged += _OnDataChanged;
                _Data.Properties.PropertyChanged += _OnPropertiesChanged;
                _OnDataPropertiesChanged(nameof(Data)); // Notify property changed
            }
        }
        /// <summary>
        /// Get ref to Rhino persistent settings. Can be null if Model GUID is not initialized
        /// </summary>
        protected PersistentSettings _RhinoPersistentSettingsNode
        {
            get
            {
                // Throw an exception if no GUID initialized. This should never happen
                if (GUID == Guid.Empty)
                {
                    return null; //Sliently do nothing and return null
                }

                // Define parent persistent setting object
                PersistentSettings parentSettings;
                if (Parent == null)
                {
                    SettingsHelper.Instance.GetSettingsRoot(SettingsDomain.RadialButtonsConfig, out parentSettings);
                }
                else
                {
                    parentSettings = Parent._RhinoPersistentSettingsNode;
                }

                // Return the persistent settings node or the new created one it if not exist
                var node = SettingsHelper.Instance.GetNode(GUID, parentSettings, true);
                return node;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void _OnDataPropertiesChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Property changed event handler for properties in object "data"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void _OnDataChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                default: break;
            }
        }

        /// <summary>
        /// Property event handler for properties changed in object "data.properties"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void _OnPropertiesChanged(object sender, PropertyChangedEventArgs e)
        {
            SettingsHelper.Instance.SetProperties(_RhinoPersistentSettingsNode, _Data);
        }
        /// <summary>
        /// 
        /// </summary>
        public Model() { }
        
        /// <summary>
        /// Constructor without a GUID. Properties will no be initialized from Rhino Persistent settings
        /// <para>
        /// REMARK: This will not load data from Rhino settings as we have a new GUID
        /// </para>
        /// </summary>
        /// <param name="buttonID"></param>
        /// <param name="parent"></param>
        /// <exception cref="Exception"></exception>
        public Model(string buttonID, Model parent = null) : base()
        {
            if (buttonID == "" || buttonID == null)
            {
                throw new Exception("Button ID cannot by empty or null");
            }

            // Init main object data
            GUID = Guid.NewGuid();
            Parent = parent;
            _Data.ButtonID = buttonID;

            // Property changed event handler
            _Data.PropertyChanged += _OnDataChanged;
            _Data.Properties.PropertyChanged += _OnPropertiesChanged;
        }
        /// <summary>
        /// Create a new model object with a new GUID object
        /// <para>
        /// REMARK: This will not load data from Rhino settings as we have a new GUID
        /// </para>
        /// </summary>
        /// <param name="modelGuid"></param>
        /// <param name="buttonID"></param>
        /// <param name="parent"></param>
        public Model(Guid modelGuid, string buttonID, Model parent = null) : this(buttonID, parent)
        {
            GUID = modelGuid;
        }
        /// <summary>
        /// Constructor with a string GUID to initialize Properties of <see Data/> from Rhino Persistent settings.
        /// <para>
        /// REMARK: This will try to load data from Rhino settings
        /// </para>
        /// </summary>
        /// <param name="modelStringGuid">GUID string formatted</param>
        /// <param name="buttonID"></param>
        /// <param name="parent"></param>
        public Model(RhinoPersistentSettingsCtor args, Model parent = null) : this(args.ButtonID, parent)
        {
            GUID = Guid.Parse(args.Guid);
            // Try to load data from Rhino persistent settings
            var rhinoPS = _RhinoPersistentSettingsNode;
            if (rhinoPS != null)
            {
                _Data = SettingsHelper.Instance.GetData(rhinoPS); // Update properties from Rhino settings
            }
        }
        /// <summary>
        /// Clear data
        /// </summary>
        public void Clear()
        {
            _Data.Properties.CommandGUID = Guid.Empty;
            _Data.Properties.IsActive = false;
            _Data.Properties.IsFolder = false;
            _Data.Properties.Icon = null;
            _Data.Properties.LeftMacro = new Macro();
            _Data.Properties.RightMacro = new Macro();
        }

    }

    public class ModelController
    {
        private static ModelController _Instance;
        public static ModelController Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new ModelController();
                }
                return _Instance;
            }
        }
        List<Model> _Models = new List<Model>();

        /// <summary>
        /// Create main instance and load models from Rhino Persistent settings
        /// </summary>
        private ModelController()
        {
            SettingsHelper.Instance.GetSettingsRoot(SettingsDomain.RadialButtonsConfig, out var rootSettingsNode, true); // Get root node for button config, create it if doesn't exist
            _LoadModels(rootSettingsNode);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parentModel"></param>
        private void _LoadModels(PersistentSettings node, Model parentModel = null)
        {
            foreach (var child in SettingsHelper.Instance.GetChildren(node))
            {
                var buttonData = SettingsHelper.Instance.GetData(child.Value);
                var model = new Model(new RhinoPersistentSettingsCtor(child.Key, buttonData.ButtonID), parentModel); // Will load data from rhino persistent settings
                model.Data = buttonData;
                _Models.Add(model);
                _LoadModels(child.Value, model); // Recursively get children
            }
        }
        /// <summary>
        /// Find a model for a button ID, eventually inside a parent model. Create a new model if option "create" is true
        /// NOTE: We don't use try/catch block, and so no use of <First> method of enumerable because of performance issue with try/catch block
        /// </summary>
        /// <param name="buttonID"></param>
        /// <param name="parent"></param>
        /// <param name="create"></param>
        /// <returns></returns>
        public Model Find(string buttonID, Model parent = null, bool create = false)
        {
            var model = _GetModel(buttonID, parent);
            if (model == null && create) // If model is not found and caller wants to create a new one
            {
                model = new Model(Guid.NewGuid(), buttonID, parent); // As model doesn't exist, create a new one with a new GUID
                _Models.Add(model);
            }
            return model;
        }
        /// <summary>
        /// Find a model by its GUID
        /// </summary>
        /// <param name="modelGUID"></param>
        /// <returns></returns>
        public Model Find(Guid modelGUID)
        {
            Model foundModel = null;
            foreach (var model in _Models)
            {
                if (model.GUID == modelGUID)
                {
                    foundModel = model;
                    break;
                }
            }
            return foundModel;
        }
        /// <summary>
        /// Find a model by its GUID string representation
        /// </summary>
        /// <param name="modelGuidString"></param>
        /// <returns></returns>
        public Model Find(string modelGuidString)
        {
            Model foundModel = null;
            Guid modelGuid = Guid.Empty;
            try
            {
                modelGuid = Guid.Parse(modelGuidString);
                foundModel = Find(modelGuid);
            }
            catch { }
            return foundModel;
        }
        /// <summary>
        /// Add a model in collection
        /// </summary>
        /// <param name="model"></param>
        public void AddModel(Model model)
        {
            _Models.Add(model);
        }
        /// <summary>
        /// Remove a model and all its children (recursive call)
        /// </summary>
        /// <param name="model"></param>
        public void RemoveModel(Model model)
        {
            var foundModel = _ModelExist(model);
            if (foundModel)
            {
                var children = GetChildren(model);
                while (children.Count > 0)
                {
                    foreach (var child in children)
                    {
                        RemoveModel(child);
                    }
                }
                _Models.Remove(model);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<Model> GetChildren(Model model)
        {
            return _Models.FindAll(instance => instance.Parent == model);
        }
        /// <summary>
        /// Get all model at same level
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<Model> GetSiblings(Model model)
        {
            return _Models.FindAll(instance => instance.Parent == model.Parent);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Model> GetRoots()
        {
            return _Models.FindAll(instance => instance.Parent == null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buttonID"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected Model _GetModel(string buttonID, Model parent = null)
        {
            Model foundModel = null;
            var modelList = parent == null ? GetRoots() : GetChildren(parent);
            foreach (var model in modelList)
            {
                if (model.Data.ButtonID == buttonID)
                {
                    foundModel = model;
                    break;
                }
            }
            return foundModel;
        }

        /// <summary>
        /// Check a model instance exists in collection
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected bool _ModelExist(Model model)
        {
            bool foundModel = false;
            foreach (var m in _Models)
            {
                if (m == model)
                {
                    foundModel = true;
                    break;
                }
            }
            return foundModel;
        }
    }
}
