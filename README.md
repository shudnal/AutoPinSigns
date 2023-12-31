# Auto Pin Signs
Create map pins based on sign's text you placed. Useful for nomap + Compass playthrough. Kinda useless otherwise. Don't break immersion while saving pin functionality to nomap playthroughs.

The mod was not intended as a fully functional replacement for pins in nomap walkthrough. Just to pin most important POI like main base and remote camps or to be some beacon for sailing home.

## Installation (manual)
extract AutoPinSigns.dll file to your BepInEx\Plugins\ folder

## Features
* Creates a pin on the minimap when you set text on sign that fits list
* Configurable strings filter for 5 map pins (fire,base,hammer,dot,portal)
* Deletes a pin on text change or sign destroying
* Automatically adds new pin on close proximity with the sign (when it is loaded)
* Support of html flavored signs like "<color="red">pin"
* Console command "autopinsigns clear 5" will erase all pins from map in that radius around player

## Known issues
* if someone destroyed pinned sign when you're not there your pin will stay. Still you can build and destroy sign in that place to remove your pin from map.

## Configurating
The best way to handle configs is configuration manager. Choose one that works for you:

https://www.nexusmods.com/site/mods/529

https://valheim.thunderstore.io/package/Azumatt/Official_BepInEx_ConfigurationManager/

## Changelog

v 1.0.5
* option to less strict strings comparison

v 1.0.4
* patch 0.217.22

v 1.0.3
* improved stability
* support of signs with html tags
* console command to clear nearest pins

v 1.0.2
* Added some protection from potential issues

v 1.0.1
* Added config update on sign interaction

v 1.0.0
* Initial release