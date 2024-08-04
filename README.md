# VHD Attach

This is small tool that adds Attach and Detach option to contextual (aka
right-click) menu of the Virtual disk (vhd) files. That enables those
operations to be done without trip to Disk Management console and allows for
automatic disk attachment during system startup.

## Shortcut Keys

- <kbd>F5</kbd>                      Refresh.
- <kbd>F6</kbd>                      Attach.
- <kbd>Ctrl</kbd>+<kbd>A</kbd>        Select all.
- <kbd>Ctrl</kbd>+<kbd>C</kbd>        Copy.
- <kbd>Ctrl</kbd>+<kbd>N</kbd>        New file.
- <kbd>Ctrl</kbd>+<kbd>O</kbd>        Open file.
- <kbd>Alt</kbd>+<kbd>A</kbd>         Show attach menu.
- <kbd>Alt</kbd>+<kbd>D</kbd>         Detach.
- <kbd>Alt</kbd>+<kbd>M</kbd>         Auto-mount.
- <kbd>Alt</kbd>+<kbd>O</kbd>         Show open menu (recent files).

## Command Line Parameters

    [/attach|/detach] "disk.vhd"

Attaches (or detaches) virtual disk using file disk.vhd.

    [/detachdrive] "X:"

Detaches virtual disk attached to drive letter.
