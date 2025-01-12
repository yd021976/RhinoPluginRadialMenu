# Rhino Radial menu (MacOS Only)
<br/><br/>

## How to install
   
The easiest way to install the plugin is to use Rhino package installer :

* You can find the package installer file in this repository in folder `/bin/Release/net7.0-macos12.0/`
* The file is named `radialmenu-1.0.0-rh8_0-any.yak`

Alternatively you can also copy the plugin files manually. Follow the steps below :
* The files to install are in the folder : `/bin/Release/net7.0-macos12.0/`
* Copy/Paste *.rhp and *.dll files to your local ~/Library/Application Support/McNeel/Rhinoceros/8.0/MacPlugIns folder
<br/><br/>

## Launch the menu
- In Rhino command, type command "TigrouRadialMenu" to launch the radial menu.
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
   * [x] Hide menu and give rhino window focus when a button is clicked (Run a command)
   * [x] Make settings persistent to keep menu configuration between Rhino launches
   * [x] Add hability to remove icons from menu
   * [ ] Improve UI visual changes when changing sub menu (i.e. Add soft fade in/out)
   * [x] Context menu to configure manually a button : Choose other icons, type in the Rhino command
     * [x] Hability to set a trigger key for each button
     * [ ] Hability to choose menu colors
     * Other improvements will come later
   * [ ] Create Setting panel to choose shortcuts to launch radial menu
   * [ ] Create plugin package installer
