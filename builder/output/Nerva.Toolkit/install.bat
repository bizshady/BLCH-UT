@echo off
md "C:/Program Files/Nerva/GUI"
copy "bin" *.* "C:/Program Files/Nerva/GUI"
mklink "C:/Program Files/Nerva/GUI/Nerva.Toolkit/Frontend.exe" "%userprofile%\desktop"
