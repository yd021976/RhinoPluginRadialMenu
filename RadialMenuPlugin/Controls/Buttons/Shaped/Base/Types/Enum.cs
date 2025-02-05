namespace RadialMenuPlugin.Controls.Buttons.Shaped.Base.Types
{
    public interface IBaseEnumKey
    {
        public string Key { get; }
    }
    /// <summary>
    /// Abstract base class for "enum" classes
    /// </summary>
    public abstract class EnumKey : IBaseEnumKey
    {
        protected string _Key;
        public string Key { get => _Key; }
        protected EnumKey(string key) => _Key = key;
        public bool Equals(EnumKey obj1) => _Key == obj1._Key;
    }
   
}
