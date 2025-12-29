This directory contains *map patch files* used to add or remove entities from a map without adjusting the mapfile itself. It is suitable for small changes like introducing or removing certain objects.

Each map patch file is a list of actions to perform on the map after load.

## Creating new patches.
### Spawn entity patches
In order to create a patch that spawns a new entity within a map, first open the map in mapping mode (if you don't know how to do this, [this guide](https://docs.spacestation14.com/en/space-station-14/mapping/guides/general-guide.html) contains information).
Place the entity you want to patch in, and right click it. In the verb menu for that entity, under Debug, click `Add to patchset`, which will add it to your change list.

Once you have the changes you want written out, you can run either `cd_map_patch:print` or `cd_map_patch:write` to print, or write to a file, a copy of the patch set.
`cd_map_patch:write` will write the file to the server's data directory.

Note if you are combining patches (which can be done simply by putting the new patch at the end of the existing one), remove any trailing `...` the game may have generated, as it marks the end of the document.

## Testing patches.
There are two options:
- Add the patch to an existing map directly, setting the `patchfile` field to point to the patch in the VFS.
- Load the patch using `cd_map_patch:load`, providing a mapid and path to load.
