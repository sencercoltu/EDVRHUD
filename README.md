# EDVRHUD Extensions
Elite Dangerous Virtual Reality Heads Up Display Extensions for SteamVR

<img src="https://github.com/sencercoltu/EDVRHUD/blob/master/images/ScanInfoPanelVR.png?raw=true"/><img src="https://github.com/sencercoltu/EDVRHUD/blob/master/images/JumpPanelVR.png?raw=true"/>

Tool for deep space exploration. Existing ones were not designed for VR, I needed to take off my headset frequently.
Mirroring windows into the headset was too difficult to read, needed interaction.
I wrote this extension primarily for my own needs, so I don't need to remove the headset frequently, and my hands are always on throttle & stick (=HOTAS).


Currently there are only 4 exploration panels added. But panels can easily be added for other roles.

### Jump Panel
<img src="https://github.com/sencercoltu/EDVRHUD/blob/master/images/JumpPanel.png?raw=true"/>
Shows remaining jumps to destination, the current star type, and your FSD boost.

### Warning Panel
<img src="https://github.com/sencercoltu/EDVRHUD/blob/master/images/WarningPanel.png?raw=true"/>
Pops up when jumping to a dangerous star like black hole, white dwarf or neutron star.

### Travel History Panel
<img src="https://github.com/sencercoltu/EDVRHUD/blob/master/images/TravelHistoryPanel.png?raw=true"/>
History of last 100 jumps in 2D.

### Scan Panel
<img src="https://github.com/sencercoltu/EDVRHUD/blob/master/images/ScanInfoPanel.png?raw=true"/><img src="https://github.com/sencercoltu/EDVRHUD/blob/master/images/ScanInfoPanel2.png?raw=true"/>
Top line shows signal count in system, total estimated credits for your discoveries, and discovery percentage. 
Rest lists most valuable bodies. List is sorted according to planet values and body features. The repeating system name included in body names are removed, instead I used the body type.

Letters in brackets show extra properties of the body or system:
* [ T ] Terraformable or EarthLike body
* [ B ] Basic jumponium materials
* [ S ] Standard jumponium materials
* [ P ] Premium jumponium materials
* [ A ] All surface materials
* [ LA ] Landable with atmosphere
* [ LT ] Landable terraformable
* [ HG ] High gravity (hard coded > 4 G)
* [ LG ] Low gravity (hard coded < 0.01 G)
* [ FR ] Fast rotation (hard coded < 1 H)
* [ FO ] Fast orbital period (hard coded < 1 H)
* [ SR ] Small radius (hard coded < 150 km)
* [ Rn ] Body with 3 or more rings (n = ringcount)

Colour codes:
* Default orange: First discovered, unmapped.
* Faded orange: Previously discovered, unmapped.
* Red: Previously discovered and mapped.
* Yellow: Self mapped.

### Settings
Voice feedback can be enabled.
Tool can also be used without any VR headset by checking Disable VR option.
Auto discovery scan is currently hardcoded to right mouse button.


All panels can be repositioned/rotated/scaled in the HUD by opening the corresponding panel from notification icon menu, and dragging the mouse while holding the left, middle or right button and one of the modifier keys LCTRL, LSHIFT. 

Load HUD reloads the last saved layout.
Save HUD saves the layout (after repositioning, rotating panels).
Panels alpha and scale properties can be directly edited in Panels.json file.


Journals are stored in a LiteDB database. On first run, all journal logs will be imported to the database. The DB roughly takes two times the space of ED journal logs.

The tool includes a journal replay function. Once journal replay window is opened, journal listening stops, and you can replay your journal from the selected timestamp. After closing the replay window, all panels continue from where you left off.


(The coding is a bit dirty, did it while exploring the edges of the galaxy, may need lots of optimization and commenting.)

### Requires .NET Runtime 4.7.2
