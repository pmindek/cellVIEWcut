# cellVIEW

Prerequisite:

In order to be able to load a scene it is necessary to add/modify the TdrDelay key from the windows registery.
This value correspond to the timeout value after which the GPU driver will restart when the GPU is busy.
By default this value is set to 2 seconds on Windows, and should be changed to order to allow more computing time for the loading, a value of 20/30 seconds is enough on most decents graphics hardware.
