# SA-MP Launcher (C# / WinForms)

A simple launcher for GTA San Andreas Multiplayer.

## Features
- Nickname, server IP/port, optional password
- Auto-detects the GTA SA folder from the SA-MP registry key
- Favorite servers (saved to settings.json)
- Live server query over UDP (name, players, ping, gamemode, language)
- Dark UI with generated background / logo / icon (Assets folder)

## Requirements
- Windows
- .NET 8 SDK  https://dotnet.microsoft.com/download
- GTA San Andreas + SA-MP client installed (samp.exe in the GTA folder)

## Build & run
    dotnet run
    dotnet publish -c Release -r win-x86 --self-contained false

(SA-MP is a 32-bit game; the launcher itself can be any arch.)

## Structure
    Program.cs              entry point
    MainForm.cs             whole UI + logic
    Models/Settings.cs      settings + favorites (JSON)
    Services/SampService.cs finds GTA path, launches samp.exe
    Services/ServerQuery.cs SA-MP UDP query protocol
    Assets/                 bg.png, logo.png, icon.ico
