mkdir release\obs-plugins\64bit
mkdir release\data\obs-plugins\ObsCSharpExample\locale
move /Y publish\* release\obs-plugins\64bit\
copy /Y locale\* release\data\obs-plugins\ObsCSharpExample\locale
