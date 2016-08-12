# verbose
#set -x

# TODO: duplicate models between character and hud asset packs..
# but that'll do for now..
projFolder=unity-game/Assets/StreamingAssets
characterEditorFolder=editor/character-editor/Assets/StreamingAssets
hudEditorFolder=editor/hud-editor/Assets/StreamingAssets

packerFolder=editor/assets-packer/AssetBundles

# remove all game models and data
rm -d -r $projFolder/*

# remove all character editor models
rm -d $characterEditorFolder/Characters/Models/*
# remove all hud editor models
rm -d $hudEditorFolder/HUD/Models/*

# copy models from packer to character editor
cp $packerFolder/* $characterEditorFolder/Characters/Models/
# copy models from packer to HUD editor
cp $packerFolder/* $hudEditorFolder/HUD/Models/

# copy models and data from character editor to game
cp -rp $characterEditorFolder/Characters/ $projFolder/Characters/
# copy models and data from HUD editor to game
cp -rp $hudEditorFolder/HUD/ $projFolder/HUD/

echo all done
