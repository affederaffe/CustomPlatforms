## Installation
### Easy Method

* Ensure you have the latest verion of ModAssistant https://github.com/Assistant/ModAssistant/releases
* Launch the mod installer
* Select the checkbox for Custom Platforms
* Click Install
### Manual Method

* Ensure your game is patched with BSIPA (ModAssistant does this for you)
* Extract CustomPlatformsX.Y.Z.zip into your Beat Saber directory
* Extract the Platform .zip into your Beat Saber directory - A few are Platforms are included in the CustomPlatformsX.Y.Z.zip

Your Beat Saber folder should then look like this:

```
| Beat Saber
  | Plugins
    | CustomPlatforms.dll             <-- 
  | CustomPlatforms		      <--
    | <.plat files>		      <--
    | Scripts                         <--
      | <.dll Custom Script files>    <--
  | IPA
  | Beat Saber.exe
  | (other files and folders)
```

## Controls

Visit the ModSettings page ingame to access important settings, like:
* Always show feet, to mark the center of the room
* Hide the :heart: that CustomPlatforms uses as a cloneable light source
* Show your selected Platform in the menu
* Load Custom Scripts, only use this option if all Scripts are from a trusted source!
* Use in 360- and 90° Levels, not recommended.
* Use in Multiplayer, not recommended.
* etc.

## Adding More Platforms

Extract the Platforms' .zip file in the Beat Saber directory.
Your installed platforms will be available upon relaunching the game.

## Creating New Platforms

There's a comprehensive guide at https://bsmg.wiki/models/platforms-guide.html written by Emma.
For a guide about how to create Custom Scripts use the guide at https://affederaffe.github.io/CustomPlatformsUnityProject/.
The following are the basic steps:

1. Download the Unity project from https://github.com/affederaffe/CustomPlatformsUnityProject, unzip it.

2. Open the Unity project
The project was created and tested in version 2018.1.6f1, other versions may not be supported.

3. Create an empty GameObject and attach a "Custom Platform" component to it
Fill out the fields for your name and the name of the platform.  You can also toggle the visibility of default environment parts if you need to make room for your platform.
Add an icon for your platform by importing an image, settings it to Sprite/UI in import settings, and dragging it into the icon field of your CustomPlatform

4. Create your custom platform as a child of this root object
You can use most of the built in Unity components, custom shaders and materials, custom meshes, animators, etc.
You can only attach your own custom scripts to these objects that are made like in this guide: https://affederaffe.github.io/CustomPlatformsUnityProject/

5. When you are finished, select the root object you attached the "Custom Platform" component to.
In the inspector, click "Export". Navigate to your CustomPlatforms folder, and press save.

6. Share your custom platform with other players by uploading the Platforms' .zip file

## Hall of Fame (Credits for major rework contributions)
#### AkaRaiden - (The QA Department, Beta Tester, Tome of Wisdom)
  - Without him this would have taken so much more time than it did.

#### Rolo - (The Master Mind, Inventor CustomPlatforms)
  - Went out of her way to help me clean up after six people messed with this.

#### Panics - (Chief Investigator)
  - Helped me get an initial grasp on the damage.

#### Tiruialon - (Top-Cat)
  - Thank you for your contributions!
 
#### Boulders2000 - (Bug Hunter)
  - Stopped counting how many bugreports he sent.
