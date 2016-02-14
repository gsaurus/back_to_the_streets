projLibs=${1}/Assets/libs

# remove all existing libs
rm -d $projLibs/*

# copy all libs
cp libs/${2}/*.dll $projLibs

# copy original protocol buffers dll
cp libs/full_protobuf/protobuf-net.dll $projLibs
# if debug, copy mdb files
if [ $2 == Debug ]; then
	cp libs/${2}/*.dll.mdb $projLibs
fi
