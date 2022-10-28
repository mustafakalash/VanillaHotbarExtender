# <img src="https://user-images.githubusercontent.com/313146/198734717-e875818b-9c08-4c8e-80cd-d53441ba339d.png" alt="icon" width="25em" /> Vanilla Hotbar Extender
![Preview](https://user-images.githubusercontent.com/313146/197404328-d0d39ef0-dc9f-4256-b0db-be39786a6968.gif)

## Description
Vanilla Hotbar Extender allows you to save hotbar set ups to the plugin configuration and load them into any hotbar on demand. The intended use is to imitate the vanilla nested hotbar trick that utilizes the `/hotbar copy` command (as featured in the gif above), but without the need take up hotbar storage from which to copy and makes the process of setting up much easier. This adds the ability to have functionally unlimited hotbar space.

Right now, the plugin is in an early testing phase with extremely basic implementation. Just enough to accomplish the above objective. If you find a bug or would like to suggest a feature not listed below, please [create an issue](https://github.com/mustafakalash/VanillaHotbarExtender/issues/new).

### Known issues
- The plugin does not write its changes to the hotbar to the character's hotbar configuration file, so hotbars will revert to their previous state upon job change, log out, or after anything else that would refresh the hotbars. It appears that the functionality to accomplish this may not yet be reverse engineered. Any help on this issue would be appreciated.
- Saving and loading from cross hotbars has not been thoroughly tested.

### Tentative thoughts on features to potentionally add
- More fleshed out plugin menu.
  - Saving and loading hotbars.
  - Preview of the actions saved to hotbars.
  - Editing saved actions.
- Referring to hotbars by name rather than index.
- Grouping hotbars or the ability to load multiple hotbars with one command.
- Triggers for automatically loading hotbars.

## Usage
Start by setting up one of your hotbars with the actions you would like to save. Both filled and unfilled slots will be loaded as you have them, overwriting anything already there. Once your hotbar is how you would like it, run the command `/vhe save [hotbar number] [save name]`. Spaces are allowed in the name, and names are case sensitive. You should see confirmation in the chatbox that the hotbar is saved. After saving, you can remove the actions from that hotbar. Now you can manually load the hotbar into any hotbar of your choice (same or otherwise) with the command `/vhe load [hotbar number] [save name]`, or create a macro to do so.

In my preview above, I use multiple macros persistently placed in hotbar 10 to load submenus into hotbars 9-5. Some of the actions loaded are additional macros that load further submenus into hotbars 8-5. The following is one of my macros that are loaded into hotbar 9 after clicking on my "emotes" macro in hotbar 10. It loads two hotbars, "dance emotes 1" and "dance emotes 2" into hotbars 8 and 7, respectively. Then, it clears hotbars 6 and 5, since they are unused by this submenu.

<img src="https://user-images.githubusercontent.com/313146/197404346-4602444e-4bd6-44b6-b783-34d47e2089b7.png" alt="macro example" width="250px" />

This screenshot shows the plugin's menu, opened by typing `/vhe` without any argument, or by clicking on the configuration button in the Dalamud plugin list. Currently, it lists the name of each saved hotbar and allows you to edit the name of (saved as you type) or delete them.

<img src="https://user-images.githubusercontent.com/313146/197404347-f9126a20-006a-459e-8fb4-61cfba59f364.png" alt="menu preview" width="250px" />

## Changelog
**1**
- First release.
