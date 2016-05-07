# verbose
#set -x

original_dir=$(pwd)

# extract project name from directory (last component of the path name is also the project name)
new_dir=$1
proj_name=$(basename $new_dir)

# get mono & mdtool locations
mdtool_cmd=/Applications/Unity/MonoDevelop.app/Contents/MacOS/mdtool
mono_app=/Applications/Unity/MonoDevelop.app/Contents/Frameworks/Mono.framework/Versions/Current/bin/mono
mdtool_exe=/Applications/Unity/MonoDevelop.app/Contents/MacOS/lib/monodevelop/bin/mdtool.exe

if [ ! -f ${mdtool_cmd} ] ; then
	mdtool_cmd=${mono_app}" "${mdtool_exe}
fi

pushd $new_dir

	# clean and build the project
	rm bin/$2 -d -f
	
	${mdtool_cmd} build ${proj_name}.csproj -c:$2 -t:Clean
	${mdtool_cmd} -v build ${proj_name}.csproj -c:$2 -t:Build
	
	pushd bin/$2
	
		# the serializer generates an executable
		if [ -f ${proj_name}.exe ] ; then
			#run serializer project to generate the dll
			${mono_app} ${proj_name}.exe
		fi
		
		# copy artefacts
		cp ${proj_name}.dll ${original_dir}/libs/${2}/
		if [ $2 == Debug ] ; then
			# convert pdb to mdb (debug symbols needed for unity), and copy it to our build folder
			# "C:/Program Files (x86)/Unity/Editor/Data/Mono/lib/mono/2.0/pdb2mdb.exe" ${proj_name}.dll
			cp ${proj_name}.dll.mdb ${original_dir}/libs/${2}/
		fi
		# also make a copy in the general folder for dependencies on other projects..
		cp ${proj_name}.dll ${original_dir}/libs/
	popd
	
popd