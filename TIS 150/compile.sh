#!/bin/bash

csc /out:TIS150.exe /appconfig:App.config /win32icon:TIS150_Icon.ico /target:exe /platform:x64 *.cs
mv TIS150.exe ~/Applications/.exe/
