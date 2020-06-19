# pedSyncer

This AltV-resource is an experiment for synchronize peds. I publish this resource to give an idea how this can work. Furthermore, I also published it to allow contribution by other developers to improve this experiment. Maybe the developers of AltV themself can get an idea about how to implement this nice feature for the multiplayer-mod.

So, contribution is welcome!

# Features

- ped creation and development like AltV players or vehicles
- streaming and sync of peds
- ped path walking
- ped wandering
- serverside ped movement calculation if the ped is not streamed to anybody
- ped meta & synced meta
- random ped spawning

# Disclaimer - IMPORTANT!

This experiment is free to use. If you find any possibilities for improvements, please tell me or contribute by pull requests. It would improve this experiment and give a better idea about how this can work - so it could also an idea for the AltV-Devs themself. If you find any issues please post them here.

**As mentioned, this is just an experiment. For now I give no guarantee that it works.** I do not recommend to use it on live-projects, because it is not tested to a big amount of players. If you want to test it, do it and tell me your feedback.

# Idea

Before you use or analyze this project I want to tell you the idea of the pedSyncer.

## Development usage

The idea of using this resource for your own development is that you can manage peds like players or vehicles in AltV. You can find a documentation in this readme on the chapter documentation.

## Serverside

The idea of this experiment is to manage peds mostly on the serverside, less on the clientside. The server stores all peds, syncronize and stream them to the clients. All changes on the ped will be transfered to all clients. Furthermore, it handles the spawn of citizens and the random wandering of peds by the calculation of the peds wandering path.

## navMeshes & external path calculation service

The calculation of the paths and the wandering is based on navMeshes. The GTA5 natural navMeshes will be used to calculate paths and wandering paths of a ped. There are 98,000 footpath navMeshes which have to be managed. Because of the size of the footpath navMeshes and the complexity to load them and calculating paths, I decide to outsource these tasks to an external service. This external service calculates random spawns of peds and their wandering paths based on navMeshes.

For more information about the external service, click here: https://github.com/KWMSources/pedSyncer-Service

## Clientside

The client gets the information from the server and execute them on the client-side (calling natives and so on). Furthermore, the client does have two important responsibilities. These responsibilities comes with the `netOwner`-status of a ped. A ped will be owned by one player - this player is the `netOwner` of this ped.

The tasks of the `netOwner` are:

- If a ped wasn't streamed to anybody, the first client which get this ped streamed in will decide the peds "style" (`native.createRandomPed`). After the creation of the ped the client sends information about drawables, textures, gender and so on to the server to sync the style of the ped.
- The `netOwner` of a ped sends its current position to the server very often for new players coming into the streaming range

## Streaming

If a player comes into the streaming-range of the ped or the ped comes into the streaming-range to the player, the ped will be streamed (displayed) to the player. The streaming range is currently static at 140 - 150. If the ped gets out of the streaming range the players client will delete the ped out of its ped cache and tells it to the server, so the server can handle the "out of range" event by selecting other netOwners or starting the serverside path walk calculation.

The peds are stored in zones in an array-map. Divide the map into zones. Zones are building up by dividing the x- and y-position by 100, so each zone is 100x100 big.

To Check if an ped is in distance to the player, the streamer calculates the players current zone and its neighbour-zones. Putting all the peds of these zones together and check the distance of these peds to the player.

This will largly decrease the number of peds which the distance to the player has to be checked.

Ped-Map has to be recalculated very often, because:
- its important for the sync
- netowner-players are changing the peds very often
- automatic-movement of the players changes the position of the player

Maybe its more performant not to recalculate the whole map every 100ms but change the ped-zone position at the time the position of the ped changed. Has to be rewritten.

The streaming process is time-critical. With delays in the streaming process, the peds will not be synced on the other players. Currently, sync-problems are possible. The streaming process have to be improved but maybe its a better idea to integrate the ped-sync more into the AltV code itself. So for this reason, this is just an experiment to give an idea about how this could work.

# Installation & integration

# Documentation

# ToDos

- [ ] Variable for Streaming-Range
- [ ] Variable for citizen count
- [ ] Ped-Death-Handling
- [ ] Ped-Dumbling
- [ ] Ped shocking event reaction
- [ ] Street-Crossing
- [ ] Ped in vehicles
- [ ] Handling for wandering, freezing, inviciblity, health, armour, weapons
- [ ] Syncing of the peds attributes
- [ ] Outsourcing Stream-Mechanics to the client?
- [ ] Ped path smoothing
- [ ] Handling with Peds which starts on a navMesh without a neighbour navMesh (route cannot be calculated)

If you want to contribute and to do some of these todos please tell me to avoid double work. Write me on discord: Saltmueller#0001

# FAQ

# Special Thanks

- Goes to Durty for his navMesh-Dump and navMesh-Framework. Visit https://github.com/DurtyFree/gta-v-data-dumps
- Goes to Dirty-Gaming - I've learned a lot their and its sad that I leaved this project but the show must go on! <3
