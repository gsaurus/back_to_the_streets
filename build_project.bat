set original_dir=%cd%

:: extract project name from directory (last component of the path name is also the project name)
set new_dir=%1
set tmp_dir=%new_dir:~0,-1%
for %%f in (%tmp_dir%) do (
	set proj_name=%%~nxf
)

pushd %new_dir%

	:: clean and build the project
	rmdir bin\%2 /s /q
	C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %proj_name%.csproj /property:Configuration=%2 /target:Clean;Build
	
	pushd bin\%2
	
		:: the serializer generates an .exe
		if exist %proj_name%.exe (
			:: run serializer project to generate the dll
			%proj_name%.exe
		)
		
		:: copy artefacts
		copy /y %proj_name%.dll %original_dir%\libs\%2\
		if %2==Debug (
			:: convert pdb to mdb (debug symbols needed for unity), and copy it to our build folder
			"C:\Program Files (x86)\Unity\Editor\Data\Mono\lib\mono\2.0\pdb2mdb.exe" %proj_name%.dll
			copy /y %proj_name%.dll.mdb %original_dir%\libs\%2\
		)
		:: also make a copy in the general folder for dependencies on other projects..
		copy /Y %proj_name%.dll %original_dir%\libs\
	popd
	
popd