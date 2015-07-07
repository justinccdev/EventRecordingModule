#EventRecordingModule#

##Introduction##

This module can record various user related events that take place on a simulator for later analysis.

Events that can currently be recorded are

1. User login.
2. User logout.
3. User entrance to a specific region.
4. User chat.
5. User to user instant messages.
6. User to group instant messages.

##Requirements##

Like OpenSimulator 0.8 onwards, this module requires the .net 4.0 SDK.  With Mono, this means that you need a version
higher than 2.8 to run it.

##Building##

1.  Copy the support libraries in lib/*.dll to $OPENSIM_BASE/bin.  These are referenced in the build process.

2.  For the rest, follow the standard instructions for building modules from source [MODULE BUILDING].

##Installation##

The build process will copy the EventRecorder.dll file to $OPENSIM_BASE/bin automatically on each build.  However, you
will also need to perform the following steps.

1.  If you are using a prebuilt-binary and so have not done this in the building step, copy the support libraries in 
lib/*.dll to $OPENSIM_BASE/bin.  

2.  Copy the [EventRecorder] section of config/EventRecorder.ini to OpenSim.ini OR copy the config/EventRecorder.ini to
$OPENSIM_BASE/bin/addon-modules/EventRecordingModule/config/EventRecorder.ini 

3.  Edit the [EventRecorder] section as required.  If you use the MySQL recorder, then you will also need to create the
database that you specify in the ConnectionString.  You will always need to set a GridId in this section.

##Console Commands##

* evr info - This will print out current information about the event recorder (e.g. number of events queued for writing.

## Web Interface ##

Analyzing the generated data can be done in a number of ways.  Some very basic examples are given at [WEB INTERFACE].

##Limitations##

Because this module works at the simulator level, on a multi-simulator installation of OpenSimulator it needs to be
enabled on every simulator if you want to be sure to capture all user login, logout and region entrance events.

##Issues##

* At the moment, this module will not work properly if the number of regions on a simulator drops to zero without
the simulator being shutdown (this is possible if one dynamically removes regions).  It will be possible to handle this
case when the next version of OpenSimulator is released

##Repository##

This module is hosted at [EVENT RECORDER].  Please report any issues with the bug tracker there.

##Credits##

* Justin Clark-Casey (http://justincc.org, http://twitter.com/justincc) 
* Created in collaboration with AFRL Discovery Lab and Dr.Rob Williams, research director.

##Third Party Libraries##

* FluidMigrator (1.2.0) - https://github.com/schambers/fluentmigrator

##References##

[EVENT RECORDER] - https://github.com/justincc/EventRecordingModule

[MODULE BUILDING] - http://opensimulator.org/wiki/IRegionModule#From_existing_module_code

[WEB INTERFACE] - https://github.com/justincc/EventRecorderWebInterface

vim: ts=4:sw=4:et:tw=120
