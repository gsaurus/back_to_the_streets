# verbose
#set -x

# cleanup libs directory
rm -d libs/*.dll
rm -d libs/${1}/*.dll
rm -d libs/${1}/*.dll.mdb

# copy original protocol buffers dll
cp libs/full_protobuf/protobuf-net.dll libs/


# rebuild all projects

# low level model
sh build_project.sh data-model/engine-model/ $1

# storage model
sh build_project.sh data-model/storage-model/ $1
sh build_project.sh data-model/storage-serializer/ $1

# game specific model
sh build_project.sh data-model/game-model/ $1
sh build_project.sh data-model/game-serializer/ $1


# copy libs to unity project
sh copy_dlls.sh unity-game $1
# copy libs to character editor project
sh copy_dlls.sh editor/character-editor $1
# copy libs to hud editor project
sh copy_dlls.sh editor/hud-editor $1
