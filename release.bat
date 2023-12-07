@ECHO OFF
rem echo "start"
rem Config:
set	dirsToCopy=bin\ Loenn\ Graphics\ Dialog\
set filesToCopy=everest.yaml
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
7zip a %~dp0DzhakeHelper.zip %buildDir%/*
rem echo "zipped"
rem echo "finished"