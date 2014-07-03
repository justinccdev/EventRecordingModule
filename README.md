#EventRecordingModule#

##Introduction##

This module will record various events that take place on a simulator (e.g. avatar entering a region) to some other data source for later analysis.

Created in collaboration with AFRL Discovery Lab and Dr.Rob Williams, research director.

##Requirements##

Like OpenSimulator 0.8 onwards, this module requires the .net 4.0 SDK.  With Mono, this means that you need a version
higher than 2.8 to run it.

##Building##

Follow the standard instructions for building modules from source [MODULE BUILDING].

## Installation

The build process will copy the EventRecorder.dll file to $OPENSIM_BASE/bin automatically on each build.  However, you will
also need to perform the following steps if you haven't before.

1.  Copy the support libraries in lib/*.dll to $OPENSIM_BASE/bin.
2.  Copy the [EventRecorder] section of config/EventRecorder.ini to OpenSim.ini
      OR copy the config/EventRecorder.ini to $OPENSIM_BASE/bin/addon-modules/EventRecordingModule/config/EventRecorder.ini
3.  Edit the [EventRecorder] section as required.  If you use the MySQL recorder, then you will also need to create the
      database that you specify in the ConnectionString.

##Issues##

* At the moment, this module will not work properly if the number of regions on a simulator drops to zero without
the simulator being shutdown (this is possible if one dynamically removes regions).  It will be possible to 
handle this case when the next version of OpenSimulator is released

##Repository##

This module is hosted at [EVENT RECORDER].  Please report any issues with the bug tracker there.

##References##

[EVENT RECORDER] - https://github.com/justincc/EventRecordingModule

[MODULE BUILDING] - http://opensimulator.org/wiki/IRegionModule#From_existing_module_code
