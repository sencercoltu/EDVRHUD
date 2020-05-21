# EDVRHUD Extensions
Elite Dangerous Virtual Reality Heads Up Display Extension(ish)

<img src="https://github.com/sencercoltu/EDVRHUD/blob/master/images/ScanInfoPanelVR.png?raw=true"/><img src="https://github.com/sencercoltu/EDVRHUD/blob/master/images/JumpPanelVR.png?raw=true"/>

I needed a tool for deep space exploration. Existing ones were not designed for VR, I needed to take off my headset frequently.
Mirroring existing tools windows into the headset was too difficult to read, needed interaction, so I wrote this little extension.

Currently there are only 3 exploration panels added. But panels can easily be added for other roles.

### Jump Panel
<img src="https://github.com/sencercoltu/EDVRHUD/blob/master/images/JumpPanel.png?raw=true"/>
Shows remaining jumps to route, the current star type, and your FSD boost.

### Warning Panel
<img src="https://github.com/sencercoltu/EDVRHUD/blob/master/images/WarningPanel.png?raw=true"/>
Pops up when jumping to a dangerous star like black hole, white dwarf or neutron star.

### Scan Panel
<img src="https://github.com/sencercoltu/EDVRHUD/blob/master/images/ScanInfoPanel.png?raw=true"/>
Top line shows signal count in system, total estimated credits for your discoveries, and discovery percentage. 
Rest lists most valable bodies. List is sorted according to terraformability (ELW count as terraformable), planet values, body resources etc. The system name included in body names are removed, instead you can see the body type.


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
* [ Rn ] Body with rings more than 2 (n = ringcount)

Colour codes:
* Default orange: First discovered, unmapped.
* Faded orange: Previously discovered, unmapped.
* Red: Previously discovered and mapped.
* Yellow: Self mapped.


Voice feedback can be enabled in settings.

All panels can be repositioned in the HUD by opening the corresponding panel from notification icon menu, and dragging the mouse while holding the left button and one of the modifier keys LCTRL, LSHIFT, RCRTL and RSHIFT. 

The tool includes a journal replay function. Once journal replay window is opened, journal listening stops, and you can replay your journal from the selected timestamp. After closing the replay window, all panels continue from where you left off.

Tool can also be used without any VR headset by checking Disable VR option.

Journals are stored in a LiteDB database. On first run, all journal logs will be imported to the database. The DB roughly takes two times the space of ED journal logs.

(The coding is a bit dirty, did it while exploring the edges of the galaxy, may need lots of optimization and commenting.)

### Requires .NET Runtime 4.7.2
