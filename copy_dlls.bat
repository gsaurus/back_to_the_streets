set projLibs=%1\Assets\libs

:: remove all existing libs
del /q %projLibs%\*

:: copy all libs
copy /y libs\%2\*.dll %projLibs%

:: copy original protocol buffers dll
copy /y libs\full_protobuf\protobuf-net.dll %projLibs%

:: if debug, copy mdb files
if %2==Debug (
	copy /y libs\%2\*.dll.mdb %projLibs%
)
