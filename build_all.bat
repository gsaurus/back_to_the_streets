:: cleanup libs directory
del /q libs\*.dll
del /q libs\%1\*.dll
del /q libs\%1\*.dll.mdb

:: copy original protocol buffers dll
copy libs\full_protobuf\protobuf-net.dll libs\

:: rebuild all projects
call build_project.bat mono-solution\engine-model\ %1
call build_project.bat shooter-demo\shooter-model\ %1
call build_project.bat shooter-demo\shooter-serializer\ %1

:: copy libs to unity project
del /q sor4-engine\Assets\libs\*
copy libs\%1\*.dll sor4-engine\Assets\libs\
:: copy original protocol buffers dll
copy libs\full_protobuf\protobuf-net.dll sor4-engine\Assets\libs\
:: if debug, copy mdb files
if %1==Debug (
	copy libs\%1\*.dll.mdb sor4-engine\Assets\libs\
)
