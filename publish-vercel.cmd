@echo off
REM Publish local WebGL build (Test\WebGLBuild) to 'vercel' branch
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\publish-vercel.ps1" %*
if %ERRORLEVEL% NEQ 0 (
  echo Publish failed with error %ERRORLEVEL%
  exit /b %ERRORLEVEL%
)
echo Publish succeeded.
