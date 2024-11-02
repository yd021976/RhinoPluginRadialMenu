namespace customControls
{
    public class ButtonProperties
    {
        public string rhinoScript { get; set; }
        public Eto.Drawing.Icon icon { get; set; }
        public bool isActive = true;

        public ButtonProperties() : base()
        {
            icon = null;
            isActive = false;
            rhinoScript = "";
        }
        public ButtonProperties(string script, Eto.Drawing.Icon icon) : this()
        {
            this.rhinoScript = script;
            this.icon = icon;
        }
        public ButtonProperties(string script, Eto.Drawing.Icon icon, bool isActive) : this(script, icon)
        {
            this.isActive = isActive;
        }
    }
}