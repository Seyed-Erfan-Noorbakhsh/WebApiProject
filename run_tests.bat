@echo off
echo Running Integration Tests...
echo.
dotnet test Shop_ProjForWeb.Tests --verbosity normal
echo.
echo Tests completed!
pause