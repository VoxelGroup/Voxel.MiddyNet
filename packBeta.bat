@echo off

set versionmajorminor="1.1"
set solution="Voxel.MiddyNet.sln"
set beta="beta"



for /f %%a in ('powershell -Command "Get-Date -format yy"') do set "datetimeyear=%%a"

for /f %%a in ('powershell -Command "(Get-Date).DayofYear"') do set "datetimedays=%%a"

for /f %%a in ('powershell -Command "[int](((Get-Date) - (Get-Date -Hour 0 -Minute 00 -Second 00)).TotalSeconds/2)"') do set "timeinseconds=%%a"


set version="%versionmajorminor%.%datetimeyear%%datetimedays%.%timeinseconds%-%beta%"

dotnet build %solution% /p:Version=%version%
del *%beta%.symbols.nupkg
dotnet nuget push *%beta%.nupkg -s http://nuget.voxelgroup.net:8181/nuget -k tGzdZyRveK4aV7fy
del *%beta%.nupkg

pause