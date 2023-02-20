set ProjectName=ObsCSharpExample
mkdir release\obs-plugins\64bit
mkdir release\data\obs-plugins\%ProjectName%\locale
del /F /S /Q release\obs-plugins\64bit\*
del /F /S /Q release\data\obs-plugins\%ProjectName%\locale\*
copy /Y publish\* release\obs-plugins\64bit\
copy /Y locale\* release\data\obs-plugins\%ProjectName%\locale
