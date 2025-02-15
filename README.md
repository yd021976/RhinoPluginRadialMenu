# Rhino Radial menu (MacOS Only)
<br/><br/>
## What it does
Radial Menu for Rhinoceros V8.0 on macOS.
The aim of this plugin is to provide a radial menu to quick access your most used command within a click or shortcut (trigger keys)
<br>
The menu can be configured by just drag & drop items, either from Rhino toolbars, either inside the radial menu itself to re-arrange items.

The radial menu support 3 submenu levels.

You can have a demo video on youtube https://www.youtube.com/watch?v=viOj8lNkz2s

## How to install
   
There are 3 ways to get the plugin and install it.

1. From github releases
* Go on "releases", download the binary .yak file
* Move or copy/paste the .yak file into your Rhino MacPlugins folder

2. Get the plugin on food4rhino
* Just visit the page https://www.food4rhino.com/en/app/tigrouradialmenu?lang=fr

3. Get it with Rhino package manager

<br/><br/>

## Launch the menu
- In Rhino command, type command "TigrouRadialMenu" to launch the radial menu.
- You can also create a Rhino shortcut to this command
- You can set this command to your middle mouse click in Rhino settings
<br/><br/>

## Manage menu icons

   1. Enter edit mode
      * Mouse right click on "close" button (the one in the center of radial menu)
      
   2. Drag Rhino toolbar icons to radial menu
      * Right mouse click on "close" button in the center to enter "edit mode"
      * Command+Left mouse click on a rhino icon to drag it
      * Drag Rhino item onto radial menu buttons
      * Release mouse and Command key
      * The item is added
      
   3. Move (Drag) radial menu items
      * Command+Left mouse click on a radial menu item to drag it (NOTE: You can't move "folder items" for the moment)
      * Move item on the radial menu
      * Release mouse button and Command key to confirm item drop
   4. Remove icons
      * Drag icon outside a "slot" : Left mouse click (maintain) + Press Command key (maintain) on an icon to initiate Drag
      * **NOTE:** You can cancel removing if you set the icon back to its initial location in the menu
   5. Set a trigger key
      * You can set a trigger key for each menu. When the menu is open, if you type the trigger letter (shows in the button menu), the menu will run the rhino command of the button or open the sub menu if the button is a "folder"
      * Right click on a button, a context menu opens
      * Type a letter in the "Trigger" field. This letter will be the trigger key for the menu button
<br/><br/>

## Radial menu usages
   1. Press ESC key to close any opened sub menu. If ESC is pressed when radial menu shows the 1st menu level, it will close the radial menu
   2. Click close button at the center to close the menu
   3. Click a button to run the command
   4. Press a trigger key to
     * Run the Rhino (left click macro) if the button is a Rhino command
     * Open the next submenu if the button is a "folder"
<br/><br/>

## TODO (not priority ordered)
   * [ ] Give choice to choose a "full circle" sub menu or "segmented circle" sub menu
   * [x] Hide menu and give rhino window focus when a button is clicked (Run a command)
   * [x] Make settings persistent to keep menu configuration between Rhino launches
   * [x] Add hability to remove icons from menu
   * [x] Improve UI visual changes when changing sub menu (i.e. Add soft fade in/out)
   * [x] Improved tooltip. They now are displayed in radial menu center, with mouse button indicator (left / right) 
   * [x] Context menu to configure manually a button : Choose other icons, type in the Rhino command
     * [x] Hability to set a trigger key for each button
     * [x] Hability to manually set Rhino command to execute for left and right click
     * [x] Hability to manually set Rhino tooltip text for left and right "command"
     * [x] Hability to manually set the button icon
     * [ ] Hability to choose menu colors
     * Other improvements will come later
   * [ ] Create Setting panel to choose shortcuts to launch radial menu
   * [x] Create plugin package installer
