# Rhino Radial menu (MacOS Only)
<br/><br/>

## How to install
   
For now, there is no package installer (coming later)

Just move the *.rhp file from the folder /bin/Release/net7.0-macos12.0/RadialMenu.rhp to your Rhino 8.0.x MacPlugins folder on your mac.

The Rhino 8 path on MacOS should be at : ~/Library/Application Support/McNeel/Rhinoceros/8.0/MacPlugIns
<br/><br/>

## Launch the menu
In Rhino command, type command "TigrouRadialMenu" to launch the radial menu.
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
   4. Remove icons (<ins>**Work in progress not available in this version**</ins>)
      * Left mouse click (maintain) + Press Command key (maintain) on an icon to initiate Drag
<br/><br/>

## Radial menu usages
   1. Press ESC key to close any opened sub menu. If ESC is pressed when radial menu shows the 1st menu level, it will close the radial menu
   2. Click close button at the center to close the menu
   3. Click a button to run the command (WIP)
<br/><br/>

## TODO (not priority ordered)
   * [x] Hide menu and give rhino window focus when a button is clicked (Run a command)
   * [x] Make settings persistent to keep menu configuration between Rhino launches
   * [ ] Add hability to remove icons from menu
   * [ ] Improve UI visual changes when changing sub menu (i.e. Add soft fade in/out)
   * [ ] Context menu to configure manually a button : Choose other icons, type in the Rhino command
     * This one will take long time and should be splitted in several sub tasks
   * [ ] Create Setting panel to choose shortcuts to launch radial menu
