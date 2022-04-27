#!/bin/bash

$tarFrmwk = $1

echo "-------- Starting MAUI build and deploy for $tarFrmwk --------"

cd ../../
cd src

echo "-------- Restoring Packages From nuget.config-Defined Sources --------"

dotnet restore MvpApi_Maui.sln --configfile nuget.config

cd MvpCompanion.Maui

$iosSubstring = 'ios'

if grep -q "$iosSubstring" <<< "$tarFrmwk";
then
  echo "-------- ...using iOS simulator --------"
  # iPhone 13 Pro Max UUID: 16E44511-85550-49DB-8BE2-A78177912809
  # iPhone 13 UUID: BFFE82B7-299B-4CB9-AA28-DB6627E13F9F
  dotnet build -t:Run -f "$tarFrmwk" -p:_DeviceName=:v2:udid=16E44511-85550-49DB-8BE2-A78177912809 --no-restore
else
  dotnet build -t:Run -f "$tarFrmwk" --no-restore
fi
