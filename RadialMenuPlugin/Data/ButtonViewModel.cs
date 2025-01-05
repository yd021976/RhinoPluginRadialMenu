using System;
using System.Collections.Generic;
using System.ComponentModel;
using Rhino;

namespace RadialMenuPlugin.Data
{
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
    }
    /// <summary>
    /// Model class for "arc buttons"
    /// </summary>
    public class Model : INotifyPropertyChanged
    {
        public Model Parent;
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
        protected PersistentSettings _RhinoPersistentSettingsNode;

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
            //TODO: Manage on property changed
        }

        public Model(string buttonID, Model parent = null) : base()
        {
            if (buttonID == "" || buttonID == null)
            {
                throw new Exception("Button ID cannot by empty or null");
            }
            // Init main object data
            Parent = parent;
            _Data.ButtonID = buttonID;

            // TODO:Load properties from rhino settings

            // Property changed event handler
            _Data.PropertyChanged += _OnDataChanged;
            _Data.Properties.PropertyChanged += _OnPropertiesChanged;
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

        private ModelController()
        { }

        /// <summary>
        /// Find a model for a button ID, eventually inside a parent model. Create a new model if option "create" is true
        /// NOTE: We don't use try/catch block, and so no use of <First> method of enumerable because of performance issue with try/catch block
        /// </summary>
        /// <param name="buttonID"></param>
        /// <param name="parent"></param>
        /// <param name="create"></param>
        /// <returns></returns>
        public Model FindModel(string buttonID, Model parent = null, bool create = false)
        {
            var model = _GetModel(buttonID, parent);
            if (model == null && create) // If model is not found and caller wants to create a new one
            {
                model = new Model(buttonID, parent);
                _Models.Add(model);
            }
            return model;
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
    }
}
