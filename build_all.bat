:: cleanup libs directory
del /q libs\*.dll
del /q libs\%1\*.dll
del /q libs\%1\*.dll.mdb

:: copy original protocol buffers dll
copy libs\full_protobuf\protobuf-net.dll libs\


:: rebuild all projects
:: low level model
call build_project.bat data-model\engine-model\ %1

:: storage model
call build_project.bat data-model\storage-model\ %1
call build_project.bat data-model\storage-serializer\ %1

:: game specific model
call build_project.bat data-model\game-model\ %1
call build_project.bat data-model\game-serializer\ %1


:: copy libs to unity-game project
call copy_dlls.bat unity-game %1

:: copy libs to character editor project
call copy_dlls.bat editor\character-editor %1
