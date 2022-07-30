# Please mention: This project is no longer maintained. It will also not be maintained for money. No matter how much money you offer: I will not help you maintain this project. So please: Don't contact me. Thanks.

# pedSyncer

This AltV-resource is an experiment for synchronize peds. I publish this resource to give an idea how this can work. Furthermore, I also published it to allow contribution by other developers to improve this experiment.

So, contribution is welcome!

# Usecases

This experiment can be used to:

- create ambient citizens to make the city more lively
- create, sync and stream peds with functionalities

# Features

- ped creation and development like AltV players or vehicles
- streaming and sync of peds
  - using of the c# entity sync
  - sync of ped components
  - sync of models
  - sync of tasks
  - sync of scenarios
  - sync of invincible
  - sync of freeze
  - sync of ped death
- ped path walking
- ped ambient scenarios
- ped wandering
- serverside ped movement calculation if the ped is not streamed to anybody
- ped meta & synced meta
- random ped spawning
- ped spawning according to the location (soldiers will not spawn in the city but on the military base)
- ped vehicle wander traffic

# Disclaimer - IMPORTANT!

This experiment is free to use. If you find any possibilities for improvements, please tell me or contribute by pull requests. It would improve this experiment and give a better idea about how this can work. If you find any issues please post them here.

**As mentioned, this is just an experiment.** I do not recommend to use it on live-projects, because it is not tested to a big amount of players. If you want to test it, do it and tell me your feedback.

# Contribution

Contribution is very welcome! I commented the code and wrote all my thoughts into it. This experiment definitely needs improvements in performance and synchronization. If you find any possibilites please tell me or contribute by pull requests.

You can also add new features into it, but please let me know to avoid double work.

# Idea

Before you use or analyze this project I want to tell you the idea of the pedSyncer.

## Development usage

The idea of using this resource for your own development is that you can manage peds like players or vehicles in AltV. You can find a documentation in this readme on the chapter documentation.

## Serverside

The idea of this experiment is to manage peds mostly on the serverside, less on the clientside. The server stores all peds, syncronize and stream them to the clients by the entity sync. All changes on the ped will be transfered to all clients. Furthermore, it handles the spawn of citizens and the random wandering of peds by the calculation of the peds wandering path.

Also the ped vehicle wander traffic will mostly handled by the server and the altv-vehicle-sync. Only one client let the ped vehicle wander, which will be synced to all clients by altv itself.

## Clientside

The client gets the information from the server and execute them on the client-side (calling natives and so on). Furthermore, the client does have two important responsibilities. These responsibilities comes with the `netOwner`-status of a ped. A ped will be owned by one player - this player is the `netOwner` of this ped.

The tasks of the `netOwner` are:

- If a ped wasn't streamed to anybody, the first client which get this ped streamed in will decide the peds "style". After the creation of the ped the client sends information about drawables, textures, gender and so on to the server to sync the style of the ped.
- The `netOwner` of a ped sends its current position to the server very often for new players coming into the streaming range
- The `netOwner` of a ped driving a vehicle will send the current vehicle location to the altv engine

## Streaming

The pedSyncer use the c# entity sync.

# Installation & integration

1. Download the pedSyncer directory and put it into your resource folder
2. Load the c#-module https://altv.mp/#/downloads and store it in the modules folder
3. Add `csharp-module` to `server.cfg` in the `modules` section
4. Add `'pedSyncer'` to the resource section
5. Please mention https://github.com/DurtyFree/gta-v-data-dumps/blob/master/navigationmesh.md#systemnotsupportedexception-a-non-collectible-assembly-may-not-reference-a-collectible-assembly if you get the `A non-collectible assembly may not reference a collectible assembly.` exception

You can also compile the script by yourself with your own configurations (like spawning of citizens or npc traffic).

## Configurations

**Activate / Deactivate Citizen Spawning**

See https://github.com/KWMSources/pedSyncer/blob/697752a7278d26459fd156e2f81866df0d5f2328/src/PedSyncer.cs#L72, Comment it out for deactivating citizen spawning

**Activate / Deactivate NPC Vehicle Spawning**

See https://github.com/KWMSources/pedSyncer/blob/697752a7278d26459fd156e2f81866df0d5f2328/src/PedSyncer.cs#L69, Comment it out for deactivating citizen spawning

**Activate Debug-Mode**

See https://github.com/KWMSources/pedSyncer/blob/697752a7278d26459fd156e2f81866df0d5f2328/src/PedSyncer.cs#L21

**Change Streaming Range of Peds**

See https://github.com/KWMSources/pedSyncer/blob/697752a7278d26459fd156e2f81866df0d5f2328/src/model/Ped.cs#L24

## NodeJS Wrapper

There is a node js wrapper for the pedSyncer which can be used to access the pedSyncer functionalities in node js. The pedSyncer package has to be installed on the server. The pedSyncerJS resource will wrap the functionalities of the original pedSyncer to node js. Currently, the wrapper is not tested and maybe full of issues, so there is a need of development.

1. Download the pedSyncerJS directory and put it into your resource folder
2. Add `'pedSyncerJS'` to the resource section of your `server.cfg`

# Documentation

The idea of developing with this resource is that you can use the peds like you use players and vehicles in AltV. The properties and functions are oriented to the use of players or vehicles.

## Serverside

### Ped

**Object-Properties**

`Id: ulong` 
(get) Unique ID of the ped

`Dimension: int`
(get/set) Dimension of the ped, default 0

`Position: Vector3`
(get/set) Position of the ped

`Range: uint`
(get/set) Range of the ped

`NetOwner: IPlayer`
(get) netOwner of the ped

`Valid: bool`
(get) Inactive, will contain information about the validity of the ped

`Created: bool`
(get) true if the ped was already created at one client

`Heading: double`
(get, set) heading of the ped

`Model: string`
(get, set) ped-model of the ped

`Drawble[1-11]: int`
(get, set) index of the drawable[1-11]

`Texture[1-11]: int`
(get, set) index of the texture[1-11]

`Prop[0,1,2,6,7]: int`
(get, set) index of the properties[0,1,2,6,7]

`Gender: string`
Inactive

`Flags: bool[]`
Inactive, sync of the peds flags

`Invincible: bool`
(get, set) true if the ped is invincible

`Vehicle: IVehicle?`
(get, set) the vehicle of the ped, null if it is not a passenger of any vehicle

`Seat: int`
(get, set) seat in the vehicle, -1 as driver

`HasBlood: bool`
(get, set) true if the ped is blood smeared - can be setted to false but set to true will not work

`Armour: int`
(get, set) current armour of the ped (0 - 100)

`Health: int`
(get, set) current health of the ped (100 - 200)

`Dead: bool`
(get, set) true if the ped is dead

`Weapons: List<string>`
Inactive

`Ammo: Dictionary<int, int>`
Inactive

`WeaponAimPos: Vector3`
Inactive

`Task: string`
(get, set) current task of the ped (note: taskParams should be setted before the taks)

`TaskParams: List<string>`
(get, set) params of the peds task

`Scenario: string`
(get, set) the scenario the ped is playing

`Freeze: bool`
(get, set) true if the ped is freezed

`Wandering: bool`
(get, set) true if the ped is wandering

`PathPositions: List<IPathElement>`
(get, set) needs more documentation, but also more development because of the peds wandering smoothing - list of positions the ped is wandering through. There are two types of IPathElements: navMeshes and streetCrossings. The navMeshes will be used to precalculate the movement of the ped if it is not streamed to anybody. If it is streamed to anyone, the ped will walk to the next streetcrossing or to the end of the list.

`NearFinalPosition: bool`
(get, set) true if it is near to the position of the end of the PathPositions-list, can be set to true to calculate a new path of the ped

`CurrentNavmashPositionsIndex: int`
(get) Index of the element in the PathPositions list the ped is currently at. Only active if the ped is not streamed to anyone.

**Object-Methods**

`constructor(x: float, y: float, z: float, model: number): void`
Constuctor of the ped. `x`, `y` and `z` are the Vector3 coordinates. Usage like `new Ped(0,0,72);`

`Destroy(): void`
Not active currently

`StartWandering(IPathElement? StartNavMesh = null): void`
Let the ped wander starting at the given start.

`ContinueWandering(): void`
Method to let the ped further wander at the moment the ped reaches the final destination

`SetRandomModel(): void`
Set a random model to the ped according to its current position.

`SetData(string key, object value): void`
Entity Sync function to store data to the ped

`TryGetData<T>(string key, out T value)`
Entity Sync function to get data of the ped

**Static methods and properties**

`GetByID(id: ulong): Ped`
Returns the ped object given by his ID. `undefined` if it found nothing

`GetNear(pos: Vector3, distance: float): Array<Ped>`
Returns peds which are in the distance of a given position.

`All`
Returns all peds.

`createCitizenPeds(): void`
Method to create citizen peds which spawns randomly and wander.

## Clientside

### Ped

`id: ulong` 
(get) Unique ID of the ped

`dimension: int`
(get) Dimension of the ped

`position: Vector3`
(get) Position of the ped

`netOwner: int`
(get) netOwners ID, only not null if this client is the netOwner

`valid: bool`
(get) Inactive, will contain information about the validity of the ped

`created: bool`
(get) true if the ped was already created at one client

heading: double`
(get) heading of the ped

`model: string`
(get) ped-model of the ped

`drawble[1-11]: int`
(get, set) index of the drawable[1-11]

`texture[1-11]: int`
(get, set) index of the texture[1-11]

`prop[0,1,2,6,7]: int`
(get, set) index of the properties[0,1,2,6,7]

`gender: string`
Inactive

`flags: bool[]`
Inactive, sync of the peds flags

`invincible: bool`
(get) true if the ped is invincible

`vehicle: int`
(get) id of the vehicle of the ped, null if it is not a passenger of any vehicle

`seat: int`
(get) seat in the vehicle, -1 as driver

`hasBlood: bool`
(get) true if the ped is blood smeared - can be setted to false but set to true will not work

`armour: int`
(get) current armour of the ped (0 - 100)

`health: int`
(get) current health of the ped (100 - 200)

`dead: bool`
(get) true if the ped is dead

`weapons: List<string>`
Inactive

`Ammo: Dictionary<int, int>`
Inactive

`WeaponAimPos: Vector3`
Inactive

`task: string`
(get, set) current task of the ped (note: taskParams should be setted before the taks)

`taskParams: string[]`
(get, set) params of the peds task

`scenario: string`
(get) the scenario the ped is playing

`freeze: bool`
(get) true if the ped is freezed

`wandering: bool`
(get) true if the ped is wandering

**Static methods and properties**

`getByID(id: number): Ped`
Method to get the ped by the ID

`getByScriptID(scriptID: number): Ped`
Method to get the ped by the script ID

`getNear(position: position, distance: number): Ped[]`
Method to get the ped to a position in the given distance

`getAllStreamedPeds(): Ped[]`
Method to get all the peds which are streamed in into the client

# ToDos
- [ ] Ped shocking event reaction
- [ ] Ped in vehicles
- [ ] Handling for weapons
- [ ] Syncing of the peds attributes
- [ ] Ped path smoothing
- [ ] Handling with Peds which starts on a navMesh without a neighbour navMesh (route cannot be calculated)
- [ ] More comments
- [ ] A better possibility to import the pedSyncer functionalities into other resources
- [ ] Improvements and Development of the nodeJS-wrapper
- [ ] Destroy of peds
- [ ] clean up, refactoring, parallelization
- [ ] Flag sync

# Known Issues
- [ ] Scenario z-offset is broke of some peds
- [ ] Set peds into other vehicles doesn't work currently (first attemp works), + seat without functionalities

If you want to contribute and to do some of these todos please tell me to avoid double work. Write me on discord: Saltmueller#0001

# FAQ

**Can I use this for my project?**

Yes, but I don't recommend it to use it for "production", because it is not tested for a large amount of players. Bugs and issues can also appear.

**Can I change the experiments code?**

Yes. If you find some possibilities for improvements please tell me about it. If you use it to develop your idea or for new exciting stuff, I would be happy if you show me.

**Why do you this or that?**

Ask me on a discord channel on the AltV-discord or contact me on pm.

**Can I buy this for an exclusive use?**

No, it is an open source experiment.

# Special Thanks

- Goes to Durty for his navMesh-Dump and navMesh-Framework. Visit https://github.com/DurtyFree/gta-v-data-dumps
- Goes to WhishN for his idea of syncing NPC Traffic. Visit https://github.com/WhishN/altV--NPCVehicleSync
- Goes to Heron for answering all my questions and developing the c# stuff and entitySync. See https://fabianterhorst.github.io/coreclr-module/
- Goes to Dirty-Gaming - I've learned a lot there and its sad that I leaved this project but the show must go on! <3
