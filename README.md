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

# Contribution

Contribution is very welcome! I commented the code and wrote all my thoughts into it. This experiment definitely needs improvements in performance and synchronization. If you find any possibilites please tell me or contribute by pull requests.

You can also add new features into it, but please let me know to avoid double work.

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

If you want to run this experiment with citizen-spawning and wandering please run the pedSyncer-service on the same localhost or change the localhost references. For more information about the pedSyncer-service see https://github.com/KWMSources/pedSyncer-Service.

You can use this experiment as a separate resource or you can integrate this resource into your resource for your development.

**Use as separate resource**
1. `npm install got` (usage of the REST-API of the pedSyncer-service)
2. create a folder and name it "pedSyncer"
3. unpack this repository into this folder
4. add 'pedSyncer' into your server.cfg

**Integrate this resource into your resource**
1. `npm install got` (usage of the REST-API of the pedSyncer-service)
2. copy the content of the server-folder into your server-folder of your resource
3. copy this code into your main mjs:
```
import alt from 'alt';
import { Ped } from './pedSyncer/class/PedClass.mjs';
import { startPedStreamer } from './pedSyncer/streamer/PedStreamer.mjs';
import { startPedControler } from './pedSyncer/control/PedControler.mjs';

...
startPedControler();
```
4. copy the content of the client-folder into your client-folder of your resource
5. copy this code into your client-main js:
```
import * as alt from 'alt';
import { startPedControler } from './pedSyncer/control/PedControler.mjs';

startPedControler();
```

**How to stop the citizen creation?**
Delete in `PedControler.mjs` the `Ped.createCitizenPeds();` line.

# Documentation

The idea of developing with this resource is that you can use the peds like you use players and vehicles in AltV. The properties and functions are oriented to the use of players or vehicles.

## Serverside

### Ped

**Object-Properties**

`dimension: number`

Not active currently

`id: number`

Unique ID of the ped

`netOwner: number`

The netOwner of this ped given by the ID of the player

`playerHaveStreamed: Array<number>`

The players given by their IDs which have this ped streamed in

`valid: boolean`

Not active currently

`created: boolean`

True if the peds style was set - otherwise false

`pos: WorldVector3`

Current position of the ped

`heading: float`

Current heading of the ped

`model: number`

Model of the ped

`drawable{1-11}: number`

Drawable Index of the ped

`texture{1-11}: number`

Texture Index of the ped

`prop{0,1,2,6,7}: number`

Not active currently

`invincible: boolean`

Not active currently

`vehicle: number`

Not active currently

`seat: number`

Not active currently

`injuries: ?`

Not active currently

`hasBlood: boolean`

Not active currently

`armour: number`

Not active currently

`health: number`

Not active currently

`weapons: ?`

Not active currently

`ammo: ?`

Not active currently

`currentTask: string`

Not active currently

`currentTaskParams: array<string>`

Not active currently

`freeze: boolean`

Not active currently

`wandering: boolean`

Not active currently - active by default for citizen peds

`navmashPositions: array<WorldVector3>`

Positions of the wandering of the ped

`nearFinalPosition: boolean`

Is the ped near his final destination of his wandering? If yes, calculate new route.

`currentNavmashPositionsIndex: number`

Only active if the ped is not streamed to anyone. Contains the index of `navmashPositions` on which the ped is

**Object-Methods**

`constructor(x: float, y: float, z: float, model: number): void`

Constuctor of the ped. `x`, `y` and `z` are the WorldVector3 coordinates. Usage like `let ped = new Ped(0,0,72);`

`destroy(): void`

Not active currently

`setPath(positionsToFollow: array<WorldVector3>): void`

Method to set the path which the ped will follow and start the peds walking to these positions. Please mention: The ped will only follow to the last position because of smoothing. The positions till the end are intermediate positions to its final destination. These are important for the server-side ped-movement calculation.

`getPathFinalDestination(): WorldVector3`

Returns the final destination of the ped

`deleteMeta(key: string): void`

Delete meta information of the ped

`getMeta(key: string): object`

Returns meta information of the ped

`hasMeta(key: string): boolean`

Returns true if the meta object is set, otherwise false

`setMeta(key: string, value: object): void`

Sets the peds meta information

`deleteSyncedMeta(key: string): void`

Delete synchronized meta information of the ped

`getSyncedMeta(key: string): object`

Returns synchronized meta information of the ped

`hasSyncedMeta(key: string): boolean`

Returns true if the synchronized meta object is set, otherwise false

`setSyncedMeta(key: string, value: object): void`

Sets the peds synchronized meta information

**Static methods and properties**

`getByID(id: number): Ped`

Returns the ped object given by his ID. `undefined` if it found nothing

`getNear(pos: WorldVector3, distance: float): Array<Ped>`

Returns peds which are in the distance of a given position.

`all`

Returns all peds.

`createCitizenPeds(): void`

Method to create citizen peds which spawns randomly and wander.

### PedControler

`setPedNewPath(ped: Ped): void`

Generate a new path for the ped to wander.

## Clientside

### Ped

**Object-Properties**

`dimension: number`

Not active currently

`id: number`

Unique ID of the ped

`scriptID: number`

The scriptID of the ped

`netOwner: number`

The netOwner of this ped given by the ID of the player

`valid: boolean`

Not active currently

`created: boolean`

True if the peds style was set - otherwise false

`pos: WorldVector3`

Current position of the ped

`heading: float`

Current heading of the ped

`model: number`

Model of the ped

`drawable{1-11}: number`

Drawable Index of the ped

`texture{1-11}: number`

Texture Index of the ped

`prop{0,1,2,6,7}: number`

Not active currently

`invincible: boolean`

Not active currently

`vehicle: number`

Not active currently

`seat: number`

Not active currently

`injuries: ?`

Not active currently

`hasBlood: boolean`

Not active currently

`armour: number`

Not active currently

`health: number`

Not active currently

`weapons: ?`

Not active currently

`ammo: ?`

Not active currently

`currentTask: string`

Not active currently

`currentTaskParams: array<string>`

Not active currently

`freeze: boolean`

Not active currently

`wandering: boolean`

Not active currently - active by default for citizen peds

`navmashPositions: array<WorldVector3>`

Positions of the wandering of the ped

`nearFinalPosition: boolean`

Is the ped near his final destination of his wandering? If yes, calculate new route.

**Object-Methods**

`constructor(x: float, y: float, z: float, model: number): void`

Constuctor of the ped. `x`, `y` and `z` are the WorldVector3 coordinates. Usage like `let ped = new Ped(0,0,72);`

`destroy(): void`

Not active currently

`setPath(positionsToFollow: array<WorldVector3>): void`

Method to set the path which the ped will follow and start the peds walking to these positions. Please mention: The ped will only follow to the last position because of smoothing. The positions till the end are intermediate positions to its final destination. These are important for the server-side ped-movement calculation.

`getPathFinalDestination(): WorldVector3`

Returns the final destination of the ped

`deleteMeta(key: string): void`

Delete meta information of the ped

`getMeta(key: string): object`

Returns meta information of the ped

`hasMeta(key: string): boolean`

Returns true if the meta object is set, otherwise false

`setMeta(key: string, value: object): void`

Sets the peds meta information

`getSyncedMeta(key: string): object`

Returns synchronized meta information of the ped

`hasSyncedMeta(key: string): boolean`

Returns true if the synchronized meta object is set, otherwise false

**Static methods and properties**

`getByID(id: number): Ped`

Returns the ped object given by his ID. `undefined` if it found nothing

`getByScriptID(id: number): Ped`

Returns the ped object given by this scriptID. Is 0 if it is not streamed to the player.

`getNear(pos: WorldVector3, distance: float): Array<Ped>`

Returns peds which are in the distance of a given position.

`all`

Returns all peds.

`getAllStreamedPeds(): Array<Ped>`

Returns all peds which are streamed to this client.

`createCitizenPeds(): void`

Method to create citizen peds which spawns randomly and wander.

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

**Can I use this for my project?**

Yes, but I don't recommend it to use it for "production", because it is not tested for a large amount of players. Bugs and issues can also appear.

**Can I change the experiments code?**

Yes. If you find some possibilities for improvements please tell me about it. If you use it to develop your idea or for new exciting stuff, I would be happy if you show me.

**Can you upload the pedSyncer-Service as a central service for all pedSyncer-user?**

I don't plan to do this. I think a central service would be a big risk for the stability. DDoS-Attacks, bottle necks, ... I think it would be better for you to run it by yourself.

**Why do you this or that?**

Ask me on a discord channel on the AltV-discord or contact me on pm.

**Can I buy this for an exclusive use?**

No, it is an open source experiment.

**I use c# - can you develop it for c#?**

Currently I don't plan to transfer this experiment to c#.

# Special Thanks

- Goes to Durty for his navMesh-Dump and navMesh-Framework. Visit https://github.com/DurtyFree/gta-v-data-dumps
- Goes to Dirty-Gaming - I've learned a lot there and its sad that I leaved this project but the show must go on! <3
