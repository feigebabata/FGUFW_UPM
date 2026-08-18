@echo off

setlocal

if "%1"=="" (
    echo package name missing
    exit /b 1
)

if "%2"=="" (
    echo version missing
    exit /b 1
)


set package=%1
set version=%2
set tag=%package%@%version%


echo Release %package% %version%


git tag | findstr /x "%tag%" >nul
if %errorlevel%==0 (
    echo Tag %tag% already exists
    exit /b 1
)


git add Packages/com.fgufw.%package%

git commit -m "release %tag%"
if errorlevel 1 exit /b 1


git tag %tag%

git push origin main --tags


echo Done: %tag%

endlocal