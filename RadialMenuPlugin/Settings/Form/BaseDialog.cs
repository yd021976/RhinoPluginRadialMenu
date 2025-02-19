
using Eto.Forms;
using RadialMenuPlugin.Controls.Base.ContextMenu;

namespace RadialMenuPlugin.Settings
{
    /// <summary>
    /// Base abstract class to display a dialog base on Eto.Dialog class
    /// <para>
    /// T is the type of contents
    /// </para>
    /// </summary>
    public abstract class BaseDialog<CONTENTS, DATA> : Dialog where CONTENTS : BaseContent<DATA> where DATA : BaseINotifyPropertyChanged, new()
    {
        #region public properties
        public new BaseContent<DATA> Content
        {
            get => _Contents;
            set
            {
                _Contents = value;
                base.Content = _Contents;
            }
        }
        public DATA Model
        {
            get => _Model;
            set
            {
                _Contents.ModelBinding.Unbind();
                _Model = value;
                _Contents.ModelBinding.Bind(this, "Model");
            }
        }
        #endregion

        #region protected/private properties
        protected DATA _Model;
        protected BaseContent<DATA> _Contents;
        #endregion
        #region public methods
        public BaseDialog() : base()
        {
            WindowStyle = WindowStyle.Default;
            AutoSize = true;
            Resizable = true;
            Topmost = true;
        }
        #endregion
        #region protected/private methods
        
        #endregion
    }

}