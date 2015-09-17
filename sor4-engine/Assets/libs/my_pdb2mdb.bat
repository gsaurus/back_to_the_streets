for %%f in (*.pdb) do (
	"C:\Program Files (x86)\Unity\Editor\Data\Mono\lib\mono\2.0\pdb2mdb.exe" "%%~nf.dll"
)