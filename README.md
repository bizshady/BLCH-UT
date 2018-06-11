# nerva-unified-toolkit

Unified toolkit for NERVA, written in C#

## Prerequisites

Mono/.NET

- Ubuntu 17.10

    `sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF`  
    `echo "deb http://download.mono-project.com/repo/ubuntu xenial/snapshots/5.8.0.127 main" | sudo tee /etc/apt/sources.list.d/mono.list`  
    `sudo apt update`  
    `sudo apt install mono-complete`

.NET Core

-Ubuntu 17.10

	`TODO: Add instructions`

## Building

## Building

Linux requires .NET Core mono to build

- Ubuntu 17.10

    `cd ./nerva-unified-toolkit`  
    `dotnet restore`  
    `msbuild /p:configuration=Release`

## Development

Development is done entirely within VS Code using the C# extension. A cross platform GUI is made available via Eto.Forms.  
To set up your environment install the following:

.NET Core SDK.
Eto.Forms .Net Core templates

Within VS Code integrated terminal

- Install the Eto.Forms templates

    `dotnet new -i "Eto.Forms.Templates::*"`

- Fancy creating an Eto.Forms application?

    `dotnet new etoapp -m json -sln -s`
    `dotnet restore`

