# cleanup libs directory
rm -d libs/*.dll
rm -d libs/${1}/*.dll
rm -d libs/${1}/*.dll.mdb

# copy original protocol buffers dll
cp libs/full_protobuf/protobuf-net.dll libs/

# rebuild all projects
sh build_project.sh mono-solution/engine-model/ $1
sh build_project.sh shooter-demo/shooter-model/ $1
sh build_project.sh shooter-demo/shooter-serializer/ $1

# copy libs to unity project
rm -d sor4-engine/Assets/libs/*
cp libs/${1}/*.dll sor4-engine/Assets/libs/
# copy original protocol buffers dll
cp libs/full_protobuf/protobuf-net.dll sor4-engine/Assets/libs/
# if debug, copy mdb files
if [ $1 == Debug ]; then
	cp libs/${1}/*.dll.mdb sor4-engine/Assets/libs/
fi
