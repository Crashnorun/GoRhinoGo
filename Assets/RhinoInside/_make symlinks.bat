@echo off
::------------------------------------------------------
:: right-click and run this file as a Administrator
:: or just copy the Rhino WIP into this folder
::
:: the symbolic link may be preferable if building 
:: for incompatible platforms, like Android
::------------------------------------------------------

:: this folder's location
SET linkLocation=C:\Users\aswartzell\Pickard Chilton Dropbox\Pickard Chilton Gadgets\Unity\GoRhinoGo\Assets\RhinoInside

:: a copy of the Rhino WIP outside of Program Files with full read/write access
SET linkDestination=C:\Rhino WIP

:: make symbolic links
MKLink "%linkLocation%\Rhino WIP" "%linkDestination%" /D  
MKLink "%linkLocation%\Microsoft.VisualBasic.dll" "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Microsoft.VisualBasic.dll"
MKLink "%linkLocation%\System.Windows.Forms.dll" "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Windows.Forms.dll"

echo.

:: pause to show output
pause