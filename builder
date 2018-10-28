#!/bin/bash

dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
installdir=/usr/local/bin
builddir=${dir}/BuildOutput/Bin

function install()
{
	sudo cp -r ${dir}/BuildOutput ${installdir}/nerva-unified-toolkit

	# write linux run script
	echo '#!/bin/bash' > ${dir}/BuildOutput/nerva-gui
	echo 'dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"' >> ${dir}/BuildOutput/nerva-gui
	echo 'mono "${dir}/nerva-unified-toolkit/Bin/Nerva.Toolkit.Frontend.exe" --config-file "~/.nerva/app.config" --log-file "~/.nerva/app.log"' >> ${dir}/BuildOutput/nerva-gui
	chmod +x ${dir}/BuildOutput/nerva-gui

	sudo mv ${dir}/BuildOutput/nerva-gui ${installdir}/nerva-gui

	exit 0
}

function uninstall()
{
	sudo rm -rf ${installdir}/nerva-unified-toolkit
	exit 0
}

function build()
{
	mkdir -p ${builddir}
	dotnet restore
	msbuild /property:GenerateFullPaths=true /property:Configuration=Release /property:TrimUnusedDependencies=true /t:build /p:OutputPath=${dir}/BuildOutput/Bin

	# write windows run script
	echo '@echo off' > ${dir}/BuildOutput/run.bat
	echo 'start %~dp0/Bin/Nerva.Toolkit.Frontend.exe --config-file %~dp0/app.config --log-file %~dp0/app.log' >> ${dir}/BuildOutput/run.bat

	# write linux run script
	echo '#!/bin/bash' > ${dir}/BuildOutput/run.sh
	echo 'dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"' >> ${dir}/BuildOutput/run.sh
	echo 'mono "${dir}/Bin/Nerva.Toolkit.Frontend.exe" --config-file "${dir}/app.config" --log-file "${dir}/app.log"' >> ${dir}/BuildOutput/run.sh
	chmod +x ${dir}/BuildOutput/run.sh

	exit 0
}

function package()
{
	cd ${dir}/BuildOutput
	zip -r "./nerva-gui-v0.0.2.0.zip" *

	exit 0
}

$1

