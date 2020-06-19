import { inDistanceBetweenPos } from "../utils/functions.mjs";
import alt from 'alt';
import got from 'got';

var i = 0;
var peds = {};
var pedsProxies = {};
class PedClass {
    //Not active currently
    dimension = 0;

    //ID of the Ped
    id = null;

    /**
     * Just one Player is the netOwner of a Ped. This Player has the task to
     * tell the server the current position of the Ped.
     * 
     * The first netOwner has also the task to creat the ped on the first time.
     */
    netOwner = null;

    //Player-IDs to which this ped is streamed
    playerHaveStreamed = [];

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
     * 
     * ToDo: Rotation still needed???
     */
    pos = {x: 0, y: 0, z: 0};
    rot = {x: 0, y: 0, z: 0};
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

    gender = null;

    //Currently inactive - will contain information if the ped is invincible
    invincible = null;

    //Currently inactive - The vehicle the ped sits in
    vehicle = null;

    //Currently inactive - If the ped is in a vehicle, this tells the current seat of the ped
    seat = null;

    //Currently inactive - HP-Stats of the ped
    injuries = null;
    hasBlood = null;
    armour = 0;
    health = 200;

    //Currently inactive - Weapons of the ped
    weapons = [];
    ammo = [];

    //Currently inactive - Aim-Position of the Ped
    weaponAimPos = {x: 0, y: 0, z: 0};

    //Currently inactive - Current Task of the Ped with its params
    currentTask = null;
    currentTaskParams = [];

    //Currently inactive - will contain information if the ped is never moving
    freeze = null;

    //Currently inactive - Tells if the ped is randomly wandering
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
    currentNavmashPositionsIndex = null;

    /**
     * Object Methods
     */
    constructor(x, y, z, model = null) {
        //Set ID by a counter
        this.id = i++;

        //Store ID to its id in an ped-object
        peds[this.id] = this;

        //Set model of the ped
        this.model = model;

        //Set current position
        this.pos = {x: x, y: y, z: z};
        this.rot = {x: 0, y: 0, z: 0};

        //Emit new ped to the players
        alt.emitClient(null, "pedSyncer:server:create", this);
    }

    //Method to delete the ped
    destroy() {
        //ToDo: All server-side stuff for ped deletion
        alt.emitClient(null, "pedSyncer:server:delete", this.id);
    }

    /**
     * Method to set the path of the ped.
     * 
     * @param navmashPositionsToAdd An array of positions {x: ..., y: ..., z: ...} 
     */
    setPath(navmashPositionsToAdd) {
        //Set the current path of the ped empty
        this.navmashPositions = [];

        //If no positions was given: Stop
        if (navmashPositionsToAdd.length == 0) return;

        //Set the path of the ped by the given positions
        for (let pos of navmashPositionsToAdd) {
            /**
             * The path calculcation system is written in c#. c# seems not to allow object properties
             * starting with a lowercase. So we have to check if this given position is with uppercase
             * and parse it to lowercase.
             */
            if (
                typeof pos.X !== "undefined" &&
                typeof pos.Y !== "undefined" &&
                typeof pos.Z !== "undefined" 
            ) {
                this.navmashPositions.push({x: pos.X, y: pos.Y, z: pos.Z});
            } else {
                this.navmashPositions.push(pos);
            }
        }
        
        //Determine the fact if the ped is near its final destination
        this.nearFinalPosition = false;

        //Send the path of the ped to all clients for syncing
        alt.emitClient(null, "pedSyncer:server:path", this.id, this.navmashPositions);
    }

    //Method to get the final destination of the ped
    getPathFinalDestination() {
        return {x: this.navmashPositions[this.navmashPositions.length-1].x, y: this.navmashPositions[this.navmashPositions.length-1].y, z: this.navmashPositions[this.navmashPositions.length-1].z};
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
    deleteSyncedMeta(key) {
        this.syncedMetaData[key] = undefined;
        delete this.syncedMetaData[key];
        alt.emitClient(null, "pedSyncer:server:metaDelete", this.id, key);
    }
    
    getSyncedMeta(key) {
        return this.syncedMetaData[key];
    }
    
    hasSyncedMeta(key) {
        return typeof this.syncedMetaData[key] !== "undefined";
    }
    
    setSyncedMeta(key, value) {
        this.syncedMetaData[key] = value;
        alt.emitClient(null, "pedSyncer:server:metaSet", this.id, key, value);
    }

    /**
     * Class Methods
     */

    //Method to get the ped by the ID
    static getByID(id) {
        return pedsProxies[id];
    }

    //Method to get all peds near to the given position and an given radius to this position
    static getNear(pos, distance = 5) {
        let pedsReturn = [];
        for (let ped of Object.values(peds)) {
            if (inDistanceBetweenPos(pos, ped.pos, distance)) pedsReturn.push(ped);
        }
        return pedsReturn;
    }

    //Property which will be catched by the proxy to get all peds (like alt.Player.all or alt.Vehicle.all)
    static all;

    /**
     * Method to create citizen peds which wander on the map
     * 
     * Call the pedSyncer-Service to get random ped-spawns and random wandering paths
     */
    static async createCitizenPeds() {
        //Call the pedSyncer-Service
        let spawnsRequest = await got(
            "http://localhost:5000/getSpawns", {
                https: {
                    rejectUnauthorized: false
                }
            }
        );

        //Parse the answer of the service
        let spawns = JSON.parse(spawnsRequest.body);
        for (let spawn of spawns) {
            //Create ped by random spawn position
            let ped = new Ped(spawn.X, spawn.Y, spawn.Z);
            //Set the peds path
            ped.setPath(spawn.path);
        }
    }
}

//PedClass Proxy
export const Ped = new Proxy(PedClass, {
    construct: (target, args, newTarget) => {
        let ped = new Proxy(new target(...args), {
            set: (pedTarget, property, value) => {
                if (property == "pos" && typeof value === "undefined") {
                    console.error("pos set to undefined");
                    console.trace();
                    throw "pos set to undefined";
                }
                peds[pedTarget.id][property] = value;
                return true;
            },
            get: (pedTarget, property, receiver) => {
                if (property == "all") return Object.values(pedsProxies);
                switch (property) {
                    case "destroy":
                    case "setPath":
                    case "getPathFinalDestination":
                    case "deleteMeta":
                    case "getMeta":
                    case "hasMeta":
                    case "setMeta":
                    case "deleteSyncedMeta":
                    case "getSyncedMeta":
                    case "hasSyncedMeta":
                    case "setSyncedMeta":
                        return function() {
                            return peds[pedTarget.id][property].apply(this, arguments);
                        }
                    default:
                        return peds[pedTarget.id][property];
                }
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
