@echo off
setlocal EnableExtensions

if "%~1"=="" (
    echo Error: package name missing.
    echo Usage: release.bat package-name version
    exit /b 1
)

if "%~2"=="" (
    echo Error: version missing.
    echo Usage: release.bat package-name version
    exit /b 1
)

set "package=%~1"
set "version=%~2"
set "tag=%package%@%version%"
set "packagePath=Packages/com.fgufw.%package%"

echo Release %package% %version%

git rev-parse --is-inside-work-tree >nul 2>&1
if errorlevel 1 (
    echo Error: current directory is not a Git repository.
    exit /b 1
)

if not exist "%packagePath%\package.json" (
    echo Error: package not found: %packagePath%
    exit /b 1
)

for /f "delims=" %%B in ('git branch --show-current') do set "branch=%%B"
if /I not "%branch%"=="main" (
    echo Error: releases must be created from main. Current branch: %branch%
    exit /b 1
)

git show-ref --verify --quiet "refs/tags/%tag%"
if not errorlevel 1 (
    echo Error: tag %tag% already exists.
    exit /b 1
)

git add -- "%packagePath%"
if errorlevel 1 (
    echo Error: failed to stage %packagePath%.
    exit /b 1
)

git diff --cached --quiet -- "%packagePath%"
if errorlevel 2 (
    echo Error: failed to inspect staged package changes.
    exit /b 1
)
if not errorlevel 1 (
    echo Error: no package changes to release.
    exit /b 1
)

git commit -m "release %tag%" -- "%packagePath%"
if errorlevel 1 (
    echo Error: failed to create release commit.
    exit /b 1
)

git tag "%tag%"
if errorlevel 1 (
    echo Error: failed to create tag %tag%.
    exit /b 1
)

git push --atomic origin "main:main" "refs/tags/%tag%:refs/tags/%tag%"
if errorlevel 1 (
    echo Error: push failed. The commit and tag exist locally.
    echo Retry after fixing the connection:
    echo   git push --atomic origin main "%tag%"
    exit /b 1
)

echo Done: %tag%
exit /b 0
