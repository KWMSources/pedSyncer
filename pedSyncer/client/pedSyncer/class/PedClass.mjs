import { inDistanceBetweenPos } from "../utils/functions.mjs";
import alt from 'alt';
import native from 'natives';
import { loadModel } from "../utils/functions.mjs";
import { unloadModel } from "../utils/functions.mjs";
import { deletePedEntirely } from "../utils/functions.mjs";
import { setPedWanderingStats } from "../utils/functions.mjs";

var peds = {};
var pedsToScriptID = {};
var pedsProxies = {};
var props = [0,1,2,6,7];
class PedClass {
    //Flag to debug this one ped
    debug = false;

    //Not active currently
    dimension = 0;

    //ID of the Ped
    id = null;
    scriptID = 0;

    /**
     * Just one Player is the netOwner of a Ped. This Player has the task to
     * tell the server the current position of the Ped.
     * 
     * The first netOwner has also the task to creat the ped on the first time.
     */
    netOwner = null;

    /**
     * Always true currently
     * 
     * Will give information about the validity
     * ToDo: When is a ped invalid?
     */
    valid = true;

    /**
     * Tells if the ped was already created on one client
     * 
     * If created is true this object will contain information about the style
     * of this ped
     */
    created = false;
    
    /**
     * Current position, rotation and heading of the ped
     */
    pos = {x: 0, y: 0, z: 0};
    heading = 0;

    /**
     * Ped-Style Properties
     */
    model = null;

    drawable1 = null;
    drawable2 = null;
    drawable3 = null;
    drawable4 = null;
    drawable5 = null;
    drawable6 = null;
    drawable7 = null;
    drawable8 = null;
    drawable9 = null;
    drawable10 = null;
    drawable11 = null;

    texture1 = null;
    texture2 = null;
    texture3 = null;
    texture4 = null;
    texture5 = null;
    texture6 = null;
    texture7 = null;
    texture8 = null;
    texture9 = null;
    texture10 = null;
    texture11 = null;

    prop0 = null;
    prop1 = null;
    prop2 = null;
    prop6 = null;
    prop7 = null;

    proptexture0 = null;
    proptexture1 = null;
    proptexture2 = null;
    proptexture6 = null;
    proptexture7 = null;

    gender = null;

    //Will contain information if the ped is invincible
    invincible = null;

    //The vehicle the ped sits in
    vehicle = null;

    //If the ped is in a vehicle, this tells the current seat of the ped
    seat = null;

    //Currently inactive
    injuries = null;
    hasBlood = null;
    
    //HP-Stats of the ped
    armour = 0;
    health = 200;
    dead = false;

    //Currently inactive - Weapons of the ped
    weapons = [];
    ammo = [];

    //Currently inactive - Aim-Position of the Ped
    weaponAimPos = {x: 0, y: 0, z: 0};

    //Current Task of the Ped with its params
    task = null;
    taskParams = [];

    //Current Scenario the ped is playing
    scenario = null;

    //Will contain information if the ped is never moving
    freeze = null;

    //Tells if the ped is randomly wandering
    //Caution: if the ped is not freezed, it will not wandering
    wandering = false;

    /**
     * If the Ped is Wandering, this tells the intermediate position
     * of his wandering and also the final destination of his wandering.
     * 
     * After the ped reached his final position a new route will be calculated.
     */
    navmashPositions = [];
    nearFinalPosition = false;
    nextNavMeshStation = 200;
    
    pedSpawnTryTime = null;
    pedSpawnTrys = null;

    //The peds flags
    flags = {};

    /**
     * Object Methods
     */
    constructor(ped) {
        //Has to be forbidden on client side
        //Should only be allowed for onServer-Creation

        this.id = ped.id;
        this.flags = {};
        peds[ped.id] = this;

        for (let key of Object.keys(ped)) if (typeof this[key] !== "undefined") this[key] = ped[key];
    }

    createPed() {
        //Create a ped with a random style fitting to the current location
        if (this.vehicle == null) this.scriptID = native.createPed(4, native.getHashKey(this.model), this.pos.x, this.pos.y, this.pos.z);
        else {
            let vehicle = getVehicleById(this.vehicle);

            if (typeof vehicle === "undefined") return;

            this.scriptID = native.createPedInsideVehicle(vehicle.scriptID, 4, native.getHashKey(this.model), this.seat, false, false);

            this.taskParams = ["scriptID", vehicle.scriptID, 10, 786603];
            this.task = "taskVehicleDriveWander";
            this.sendTask();
        }
    }

    /**
     * Spawn the ped, decide if this is the first time this ped will ever be created
     */
    async spawn() {
        //Load the model of this ped
        await loadModel(this.model);

        this.createPed();

        //Store this ped by his scriptID as a key
        pedsToScriptID[this.scriptID] = this;

        native.setEntityInvincible(this.scriptID, this.invincible);
        native.freezeEntityPosition(this.scriptID, this.freeze);

        this.pos = getPedPos(this.scriptID);

        //Set the heading of the ped
        native.setEntityHeading(this.scriptID, this.heading);

        //Don't know why this is important, but it is...
        native.setEntityAsMissionEntity(this.scriptID, true, false);
    
        //Set the peds style
        this.setPedComponentVariation();

        //Set Ped-Attributes
        native.setPedArmour(this.scriptID, this.armour);
        native.setEntityHealth(this.scriptID, this.health, 0);
        if (this.dead) native.setPedToRagdoll(this.scriptID, -1, -1, 0, false, false, false);

        //Start the peds wandering
        if (typeof this.scriptID !== "undefined" && this.scriptID != 0) {
            if (this.wandering) this.startPath();
            if (this.scenario != null) this.startScenario();
        }
    }

    /**
     * This will be executed at the time the server assigns this client as a netowner.
     * The purpose of the netowner is to communicate the current position of the ped
     * to the server, so the server can sync that to all other clients which doesnt
     * have this ped streamed in.
     */
    becomeNetOwner() {
        this.netOwner = alt.Player.local.id;

        if (this.vehicle != null || this.task == "taskVehicleDriveWander") native.taskVehicleDriveWander(this.scriptID, getVehicleById(this.vehicle).scriptID, 10, 786603);
    }

    releaseNetOwner() {
        this.netOwner = null;
    }

    /**
     * This will be executed at the time this client leaves the streaming-range of
     * the ped.
     * 
     * Peds have to be deleted because the "ped-cache" of GTA is limited.
     */
    outOfRange() {
        //Delete ped
        deletePedEntirely(this.scriptID);

        //Delete ped scriptID reference
        delete pedsToScriptID[this.scriptID];

        //Set scriptID of the ped to 0
        this.scriptID = 0;
        
        alt.setTimeout(() => {
            unloadModel(this.model);
        }, 5000);
    }

    /**
     * Method to set the path of the ped
     * 
     * Should only used by events from the server, because if you use it on client-side
     * it will not be synced to the other clients
     * 
     * @param navmashPositionsToAdd An array of positions {x: ..., y: ..., z: ...} 
     */
    setPath(navmashPositionsToAdd) {
        //If there are no navmeshPositions: Stop
        if (!this.wandering) return;

        //Reset the route of the player
        this.navmashPositions = [];

        //Store and parse the navmeshPositions
        for (let pos of navmashPositionsToAdd) this.navmashPositions.push(pos);

        //Reset the nearFinalPosition-variable
        this.nearFinalPosition = false;

        //Start the peds wandering
        this.startPath();
    }

    /**
     * Method to start the peds wandering
     */
    startPath() {
        //If there are no navmeshPositions: Stop
        if (!this.wandering) return;
        
        /**
         * Set nextNavMeshStation
         * 
         * nextNavMeshStation is the index of the next street crossing position in navmashPositions
         */
        this.nextNavMeshStation = this.navmashPositions.length-1;

        for (let navMeshKey in this.navmashPositions) {
            if (this.navmashPositions[navMeshKey].streetCrossing) {
                this.nextNavMeshStation = navMeshKey;
                break;
            }
        }

        //If the ped is spawned: Let it wander
        if (this.scriptID != 0) {
            //If this client is not the netOwner move it to its current position (for sync)
            if (this.netOwner != alt.Player.local.id) native.setEntityCoordsNoOffset(this.scriptID, this.pos.x, this.pos.y, this.pos.z, 0, 0, 0);

            /**
             * Let the ped walk to the end of its path
             * 
             * taskFollowNavMeshToCoord seems not to be the best task-native for this task
             * because it will not follow the gta5-internal navMeshes, it selects the shortest
             * path regardless of the navMeshes. By this, it walks threw streets, alleys, back- & 
             * courtyards which all don't have navMeshes.
             * 
             * So a solution could be to let the ped walk threw all navMeshes in .navmashPositions.
             * This will not be a good solution because the walking of the ped will looks very
             * weird and not smooth. It will walk like zigzag. So this solution is the smoothing-
             * solution.
             * 
             * If there is no other native or gta5-natural solution the navMeshes has to be smoothed,
             * maybe by linear regression.
             */
            native.taskFollowNavMeshToCoord(this.scriptID, this.navmashPositions[this.nextNavMeshStation].x, this.navmashPositions[this.nextNavMeshStation].y, this.navmashPositions[this.nextNavMeshStation].z, 1.0, -1, 0.0, true, 0.0);
            
            this.taskParams = ["scriptID", this.navmashPositions[this.nextNavMeshStation].x, this.navmashPositions[this.nextNavMeshStation].y, this.navmashPositions[this.nextNavMeshStation].z, 1.0, -1, 0.0, true, 0.0];
            this.task = "taskFollowNavMeshToCoord";
            this.sendTask();
        }
    }

    pathPositionReached() {
        //If there are no navmeshPositions: Stop
        if (!this.wandering) return;

        let currenctNextNavMeshStation = this.nextNavMeshStation;
        
        /**
         * Set nextNavMeshStation
         * 
         * nextNavMeshStation is the index of the next street crossing position in navmashPositions
         */
        this.nextNavMeshStation = this.navmashPositions.length-1;

        for (let navMeshKey in this.navmashPositions) {
            if (currenctNextNavMeshStation >= navMeshKey) continue;
            if (this.navmashPositions[navMeshKey].streetCrossing) {
                this.nextNavMeshStation = navMeshKey;
                break;
            }
        }

        /**
         * Let the ped walk to the end of its path
         * 
         * taskFollowNavMeshToCoord seems not to be the best task-native for this task
         * because it will not follow the gta5-internal navMeshes, it selects the shortest
         * path regardless of the navMeshes. By this, it walks threw streets, alleys, back- & 
         * courtyards which all don't have navMeshes.
         * 
         * So a solution could be to let the ped walk threw all navMeshes in .navmashPositions.
         * This will not be a good solution because the walking of the ped will looks very
         * weird and not smooth. It will walk like zigzag. So this solution is the smoothing-
         * solution.
         * 
         * If there is no other native or gta5-natural solution the navMeshes has to be smoothed,
         * maybe by linear regression.
         */
        native.taskFollowNavMeshToCoord(this.scriptID, this.navmashPositions[this.nextNavMeshStation].x, this.navmashPositions[this.nextNavMeshStation].y, this.navmashPositions[this.nextNavMeshStation].z, 1.0, -1, 0.0, true, 0.0);
        
        this.taskParams = ["scriptID", this.navmashPositions[this.nextNavMeshStation].x, this.navmashPositions[this.nextNavMeshStation].y, this.navmashPositions[this.nextNavMeshStation].z, 1.0, -1, 0.0, true, 0.0];
        this.task = "taskFollowNavMeshToCoord";
        this.sendTask();
    }

    /**
     * Method to get the the last navmashPositions
     */
    getPathFinalDestination() {
        if (this.navmashPositions.length > 0) return this.navmashPositions[this.navmashPositions.length-1];

        return null;
    }

    /**
     * Method to start the peds scenario
     */
    startScenario() {
        native.setEntityHeading(this.scriptID, this.heading);

        let that = this;

        alt.setTimeout(() => {
            native.taskStartScenarioInPlace(that.scriptID, that.scenario, 0, false);
            
            that.task = "taskStartScenarioInPlace";
            that.taskParams = [that.scriptID, that.scenario, 0, false];
            this.sendTask();
        }, 1000);
    }
    
    setWandering() {
        if (this.wandering == true) this.startPath();
        else if(this.scriptID != 0) native.clearPedTasks(this.scriptID);
    }

    /**
     * Method to set the peds style, using the setPedComponentVariation-native
     */
    setPedComponentVariation() {
        for (let i = 0; i < 12; i++) native.setPedComponentVariation(this.scriptID, i, this["drawable"+i], this["texture"+i], 0);

        for (let prop in props) native.setPedPropIndex(this.scriptID, prop, this["prop"+prop], this["proptexture"+prop], 0);
    }

    /**
     * Method to update the peds properties
     * 
     * @param ped Ped which has to be updated
     */
    updateProperties(ped) {
        let componentSet = false;

        for (let key of Object.keys(ped)) {
            this[key] = ped[key];

            if (
                key.includes("drawable") ||
                key.includes("texture") ||
                key.includes("prop") ||
                key.includes("proptexture")
            ) componentSet = true;
        }

        if (componentSet) this.setPedComponentVariation();
    }

    /**
     * Update the peds positions
     * 
     * @param ped 
     */
    updatePos(ped) {
        this.pos = ped.pos;
        this.heading = ped.heading;
    }

    sendTask() {
        if (this.netOwner != alt.Player.local.id) return;

        let params = replaceScriptID(this.taskParams, this.scriptID);

        alt.emitServer("pedSyncer:client:task", this.id, this.task, params);
    }

    /**
     * Meta-Data
     */
    metaData = {};
    deleteMeta(key) {
        this.metaData[key] = undefined;
        delete this.metaData[key];
    }

    getMeta(key) {
        return this.metaData[key];
    }

    hasMeta(key) {
        return typeof this.metaData[key] !== "undefined";
    }

    setMeta(key, value) {
        this.metaData[key] = value;
    }

    /**
     * Synced-Meta-Data
     */
    syncedMetaData = {};
    getSyncedMeta(key) {
        return this.syncedMetaData[key];
    }
    
    hasSyncedMeta(key) {
        return typeof this.syncedMetaData[key] !== "undefined";
    }

    pack() {
        return {id: ped.id, pos: JSON.stringify(ped.pos), heading: ped.heading, nearFinalPosition: ped.nearFinalPosition};
    }

    /**
     * Class Methods
     */

    //Method to get the ped by the ID
    static getByID(id) {
        return pedsProxies[id];
    }

    //Method to get the ped by the scriptID
    static getByScriptID(id) {
        return pedsProxies[pedsToScriptID[id].id];
    }

    //Method to get all peds near to the given position and an given radius to this position
    static getNear(pos, distance = 5) {
        return Object.values(peds).filter(p => inDistanceBetweenPos(pos, p.pos, distance));
    }

    //Method to get all peds which are streamed to the player
    static getAllStreamedPeds() {
        return Object.values(pedsToScriptID);
    }

    static emitParse(ped) {
        let newPed = {};

        for (let key in ped) newPed[key] = ped[key]+"";

        return newPed;
    }

    /**
     * Support-Methods
     */

    /**
     * Get the peds which are streamed in and which this client is the netOwner of
     */
    static getAllMyStreamedPeds() {
        return Ped.getAllStreamedPeds().filter(p => p.netOwner == alt.Player.local.id);
    }

    /**
     * Method to set all positions of the peds to which this client is the netOwner and
     * send these new positions to the server.
     */
    static setMyPedPoses() {
        //Iterate over all peds which are streamed in filtered by the netOwner == this client
        for (let ped of Ped.getAllMyStreamedPeds()) {
            try {
                /*if (ped.debug) {
                    
                }*/  
    
                ped = setPedCurrentData(ped);
                
                ped = setPedWanderingStats(ped);
    
                /**
                 * Set Ped flags
                 
                for (let j = 0; j <= 426; j++) {
                    ped.flags[j] = native.getPedConfigFlag(ped.scriptID, j, 1);
                }*/
            } catch (error) {
                alt.log("[PEDSYNCER ERROR] " + error);
            }
        }
        alt.setTimeout(Ped.setMyPedPoses, 500);
    }

    /**
     * Send the positions of the peds of which this client is the netOwner to the server
     */
    static sendMyPedPoses() {
        let pedsToSend = [];

        //Iterate over all peds which are streamed in filtered by the netOwner == this client, add this ped to pedsToSend
        for (let ped of Ped.getAllMyStreamedPeds()) pedsToSend.push(ped.pack());

        //If pedsToSend is not empty: send it to the server
        if(pedsToSend.length > 0) alt.emitServer("pedSyncer:client:positions", pedsToSend);

        alt.setTimeout(Ped.sendMyPedPoses, 500);
    }

    static all;
}

var functions = ['firstSpawn','respawn','spawn','becomeNetOwner','outOfRange','setPath','startPath','getPathFinalDestination','setPedComponentVariation','updateProperties','updatePos','deleteMeta','getMeta','hasMeta','setMeta','getSyncedMeta','hasSyncedMeta'];

//PedClass Proxy
export const Ped = new Proxy(PedClass, {
    construct(target, args) {
        let ped = new Proxy(new target(...args), {
            set(pedTarget, property, value) {
                peds[pedTarget.id][property] = value;

                if (property == "wandering") pedTarget.setWandering();
                else if (property == "task" && value != "" && peds[pedTarget.id]["wandering"] == false) {
                    peds[pedTarget.id]["taskParams"] = insertScriptID(peds[pedTarget.id]["taskParams"], pedTarget.scriptID);

                    if(
                        pedTarget.scriptID != 0 &&
                        (
                            peds[pedTarget.id][property] != "taskVehicleDriveWander" ||
                            pedTarget.netOwner == alt.Player.local.id
                        )
                    ) native[peds[pedTarget.id][property]](...peds[pedTarget.id]["taskParams"]);
                    pedTarget.sendTask();
                }
                
                return true;
            },
            get: (pedTarget, property, receiver) => {
                if (property == "all") return Object.values(pedsProxies);
                
                if (functions.indexOf(property) != -1) {
                    return function() {
                        return peds[pedTarget.id][property].apply(this, arguments);
                    }
                }

                return peds[pedTarget.id][property];
            }
        });

        pedsProxies[ped.id] = ped;

        return ped;
    },
    get: (target, property, receiver) => {
        if (property == "all") return Object.values(pedsProxies);
        return target[property];
    },
    set: (pedTarget, property, value) => {
        return true;
    }
});

alt.setTimeout(Ped.setMyPedPoses, 500);
alt.setTimeout(Ped.sendMyPedPoses, 500);