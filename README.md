**Features:**  
• Adds information about sail efficiency and how much of an halyard or sheet is let out of a winch when looking at it; 
• Adds a way of identify winches by coloring them with different color and displaying the name of the sail they're attached to. Can be customised.  
• Displays a bar showing how much the rudder is turned left or right;    
• Adds HUD like information when looking at the rudder, showing:  
	• Apparent wind speed (kts or beaufort scale descriptor words);
	• Apparent wind direction relative to the world. (degrees or cardinal, cardinal by default);  
	• Apparent wind direction relative to the boat (degrees, plain text or colored red/green);  
	• Boat Speed (kts. Disabled by default).
	• Boat Heading (degrees or cardinal. Disabled by default);  
	• Boat velocity made good(VMG) (kts, Disabled by default;  
	• Boat heeling (degrees, Disabled by default, experimental).  
  
**Configuration notes:** You can configure the mod quite a bit in what it shows. In order to do so, install the mod, run the game once, close it and navigate to the *...\Sailwind\BepInEx\config* folder. Find a file called *pr0skynesis.sailinfo.cfg*. Open it with a text editor and change the values of thing you want to disable to false.   
  
**Additional explanation of some features:**  
The wind direction relative to the world and the boat heading can be displayed either in degrees (0°-360°, accurate) or in cardinal direction (e.g N, SW, ENE, etc., less accurate) this is less accurate, but might be considered less cheaty and more immersive.  
The wind direction relative to the boat is expressed in degrees going from 0° to 180° (wind coming from the right side of the boat) and 0° to -180° (wind from the left side of the boat). So, negative values means wind coming from the left, 0° means headwind, 180° means tail wind, etc. This value is colored red (left) or green (right) for additional clarity. Coloration can be disabled in the configuration file.  
Some of the information displayed on the rudder could be considered cheaty, especially the boat heading and speed, since they give you access to information you would need tools to get. This are disabled by default.
Exact wind direction in degrees could also be seen as cheaty or immersion breaking, so by default this value is expressed using cardinal direction instead of exact degrees. (You will see some letters, like NE, meaning north east, instead of 45°).
  
	
**Requirements: Requires BepInEx**  
**Installation:** Download SailInfo.dll and move it into the *...\Sailwind\BepInEx\plugins* folder.  
**Game version:** *0.26* (should work with higher and lower game version as well, this is just the game version from when the mod was made)  
**Mod Version:** *1.1.0*  
**Warning:** Making a backup of your save is advisable before installing new mods, however this mod should not cause any issue with saves.  
