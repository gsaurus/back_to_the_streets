set projFolder=unity-game\Assets\StreamingAssets
set characterEditorFolder=editor\character-editor\Assets\StreamingAssets
set hudEditorFolder=editor\hud-editor\Assets\StreamingAssets

set packerFolder=editor\assets-packer\AssetBundles

:: remove all game models and data
del /q %projFolder%\*

:: remove all character editor models
del /q %characterEditorFolder%\Characters\Models\*
:: remove all hud editor models
del /q %hudEditorFolder%\HUD\Models\*

:: copy models from packer to character editor
copy /y %packerFolder%\* %characterEditorFolder%\Characters\Models\
:: copy models from packer to HUD editor
copy /y %packerFolder%\* %hudEditorFolder%\HUD\Models\

:: copy models and data from character editor to game
copy /y %characterEditorFolder%\Characters %projFolder%\Characters\
:: copy models and data from HUD editor to game
copy /y %hudEditorFolder%\HUD %projFolder%\HUD\
