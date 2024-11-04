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
        public void load()
        {
            getChild(plugin.Settings, "theme", out var theme);
            getThemeSettings(theme, "normal", ref settings.buttonColors.normal.pen, ref settings.buttonColors.normal.fill);
            getThemeSettings(theme, "hover", ref settings.buttonColors.hover.pen, ref settings.buttonColors.hover.fill);
            getThemeSettings(theme, "drag", ref settings.buttonColors.drag.pen, ref settings.buttonColors.drag.fill);
        }
        public void save()
        {
            getChild(plugin.Settings, "theme", out var theme);
            setThemeSettings(theme, "normal", settings.buttonColors.normal.pen, settings.buttonColors.normal.fill);
            setThemeSettings(theme, "hover", settings.buttonColors.hover.pen, settings.buttonColors.hover.fill);
            setThemeSettings(theme, "drag", settings.buttonColors.drag.pen, settings.buttonColors.drag.fill);
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
        private void getThemeSettings(PersistentSettings theme, string key, ref Eto.Drawing.Color pen, ref Eto.Drawing.Color fill)
        {
            PersistentSettings childThemeSettings;
            System.Drawing.Color penColor;
            System.Drawing.Color fillColor;
            if (theme.TryGetChild(key, out childThemeSettings))
            {
                if (childThemeSettings.TryGetColor("pen", out penColor) && childThemeSettings.TryGetColor("fill", out fillColor))
                {
                    pen = Eto.Drawing.Color.FromArgb(penColor.ToArgb());
                    fill = Eto.Drawing.Color.FromArgb(fillColor.ToArgb());
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