@ECHO OFF
rem echo "start"
rem Config:
set	dirsToCopy=Loenn\ Graphics\ Dialog\
set filesToCopy=everest.yaml bin\DzhakeHelper.dll bin\DzhakeHelper.pdb
rem End of config!
set buildDir=%~dp0Build\\
rem echo "variables created"
(for %%d in (%dirsToCopy%) do ( 
   xcopy %~dp0%%d %buildDir%%%d /Y /E /H /Q
))
rem echo "folders copied"
(for %%f in (%filesToCopy%) do ( 
   xcopy %~dp0%%f %buildDir%%%f* /Y /H /Q
))
rem echo "files copied"
7z a %~dp0DzhakeHelper.zip %buildDir%/*
rem echo "zipped"
rd /s /q "%builddir%"
rem echo "finished"