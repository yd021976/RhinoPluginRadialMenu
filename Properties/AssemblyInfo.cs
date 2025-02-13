﻿using System.Runtime.InteropServices;
using Rhino.PlugIns;

// Plug-in Description Attributes - all of these are optional.
// These will show in Rhino's option dialog, in the tab Plug-ins.
[assembly: PlugInDescription(DescriptionType.Address, "")]
[assembly: PlugInDescription(DescriptionType.Country, "")]
[assembly: PlugInDescription(DescriptionType.Email, "")]
[assembly: PlugInDescription(DescriptionType.Phone, "")]
[assembly: PlugInDescription(DescriptionType.Fax, "")]
[assembly: PlugInDescription(DescriptionType.Organization, "Tigrou")]
[assembly: PlugInDescription(DescriptionType.UpdateUrl, "")]
[assembly: PlugInDescription(DescriptionType.WebSite, "https://github.com/yd021976/RhinoPluginRadialMenu")]

// Icons should be Windows .ico files and contain 32-bit images in the following sizes: 16, 24, 32, 48, and 256.
[assembly: PlugInDescription(DescriptionType.Icon, "RhinoPluginRadialMenu.Bitmaps.icon.png")]

// The following GUID is for the ID of the typelib if this project is exposed to COM
// This will also be the Guid of the Rhino plug-in
[assembly: Guid("f251fa18-1961-4025-b074-d2c9b9df1771")] 
