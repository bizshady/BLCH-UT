#!/usr/bin/env bash

if [ "$(expr substr $(uname -s) 1 5)" == "Linux" ]; then

    os_distro="unknown"
    os_ver="unknown"

    if [ -f /etc/os-release ]; then
        source /etc/os-release
        os_distro=$ID
        os_ver=$VERSION_ID
    elif [ -f /etc/lsb-release ]; then
        source /etc/lsb-release
        os_distro=$DISTRIB_ID
        os_ver=$DISTRIB_RELEASE
    fi

    echo ${os_distro}:${os_ver}
    exit 0
fi

exit 1