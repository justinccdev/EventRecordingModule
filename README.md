#EventRecordingModule#

##Introduction##

This module will record various events that take place on a simulator (e.g. avatar entering a region) to some other data source for later analysis.

Created in collaboration with AFRL Discovery Lab and Dr.Rob Williams, research director.

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

##References##

[MODULE BUILDING] - http://opensimulator.org/wiki/IRegionModule#From_existing_module_code
