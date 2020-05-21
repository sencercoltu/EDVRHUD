# EDVRHUD
Elite Dangerous Virtual Reality Heads Up Display Extension(ish)

I needed a tool for deep space exploration. Existing ones were not designed for VR, I needed to take off my headset frequently.
Existin tools bringing mirroring windows into the headset were poorly coded and too difficult to read, so I wrote this little extension.

Currently there are only 3 panels.

### Jump Panel
<img src="https://github.com/sencercoltu/EDVRHUD/blob/master/images/JumpPanel.png?raw=true"/>
Shows remaining jumps to route, the current star type, and your FSD boost.

### Warning Panel
<img src="https://github.com/sencercoltu/EDVRHUD/blob/master/images/WarningPanel.png?raw=true"/>
Pops up when jumping to a dangerous star like black hole, white dwarf or neutron star.

### Scan Panel
<img src="https://github.com/sencercoltu/EDVRHUD/blob/master/images/ScanInfoPanel.png?raw=true"/>
Shows signal count in system, total estimated credits for your discoveries, and discovery percent. Also lists most valable bodies.
List is sorted according to terraformability (ELW count as terraformable), planet values, body resources etc.  
The system name included in body names are removed, instead you can see the body type.

All panels can be repositioned in the HUD by opening the corresponding panel from notification icon menu, and dragging the mouse while holding the left button and modifier keys LCTRL, LSHIFT, RCRTL and RSHIFT.

Also includes a journal replay function. Once journal replay window is opened, journal listening stops, and you can replay your journal from the selected timestamp. Once exiting the replay window, all panels continue from where you left off.

Tool can also be used without any VR headset by checking Disable VR option.





