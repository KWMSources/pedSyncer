import { inDistanceBetweenPos } from "../utils/functions.mjs";
import alt from 'alt';
import native from 'natives';
import { loadModel } from "../utils/functions.mjs";

var i = 0;
var peds = {};
var pedsToScriptID = {};
var pedsProxies = {};
class PedClass {
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
    
    pedSpawnTryTime = null;
    pedSpawnTrys = null;

    /**
     * Object Methods
     */
    constructor(ped) {
        //Has to be forbidden on client side
        //Should only be allowed for onServer-Creation
        this.id = ped.id;
        peds[ped.id] = this;

        for (let key of Object.keys(this)) {
            if (typeof this[key] !== "undefined") this[key] = ped[key];
        }
    }

    /**
     * This will be executed by the first client to which the ped is streamed in.
     * This client will decide the model, the drawables and the textures for all
     * users. This client decide the style of this ped and gives it back to the
     * server for sync.
     */
    firstSpawn() {
        //Check if created - if so: stop!
        if (this.created) return;
        this.created = true;
        
        //Create a random ped with a random style fitting to the current location
        this.scriptID = native.createRandomPed(this.pos.x, this.pos.y, this.pos.z);

        //Store this ped by his scriptID as a key
        pedsToScriptID[this.scriptID] = this;

        /**
         * Get all important information to give it back to the server for sync
         */
        this.model = native.getEntityModel(this.scriptID);

        this.pos = JSON.parse(JSON.stringify(native.getEntityCoords(this.scriptID, true)));
        this.rot = native.getEntityRotation(this.scriptID, 0);

        this.gender = native.isPedMale(this.scriptID)?"male":"female";
        this.drawable0 = native.getPedDrawableVariation(this.scriptID, 0);
        this.drawable1 = native.getPedDrawableVariation(this.scriptID, 1);
        this.drawable2 = native.getPedDrawableVariation(this.scriptID, 2);
        this.drawable3 = native.getPedDrawableVariation(this.scriptID, 3);
        this.drawable4 = native.getPedDrawableVariation(this.scriptID, 4);
        this.drawable5 = native.getPedDrawableVariation(this.scriptID, 5);
        this.drawable6 = native.getPedDrawableVariation(this.scriptID, 6);
        this.drawable7 = native.getPedDrawableVariation(this.scriptID, 7);
        this.drawable8 = native.getPedDrawableVariation(this.scriptID, 8);
        this.drawable9 = native.getPedDrawableVariation(this.scriptID, 9);
        this.drawable10 = native.getPedDrawableVariation(this.scriptID, 10);
        this.drawable11 = native.getPedDrawableVariation(this.scriptID, 11);
        this.texture0 = native.getPedTextureVariation(this.scriptID, 0);
        this.texture1 = native.getPedTextureVariation(this.scriptID, 1);
        this.texture2 = native.getPedTextureVariation(this.scriptID, 2);
        this.texture3 = native.getPedTextureVariation(this.scriptID, 3);
        this.texture4 = native.getPedTextureVariation(this.scriptID, 4);
        this.texture5 = native.getPedTextureVariation(this.scriptID, 5);
        this.texture6 = native.getPedTextureVariation(this.scriptID, 6);
        this.texture7 = native.getPedTextureVariation(this.scriptID, 7);
        this.texture8 = native.getPedTextureVariation(this.scriptID, 8);
        this.texture9 = native.getPedTextureVariation(this.scriptID, 9);
        this.texture10 = native.getPedTextureVariation(this.scriptID, 10);
        this.texture11 = native.getPedTextureVariation(this.scriptID, 11);
        this.created = true;

        //Send back to the server for sync with other players
        alt.emitServer("pedSyncer:client:firstSpawn", Ped.emitParse(this));
    }

    /**
     * This will be executed by the clients coming into range of the ped and the ped
     * was already created by an other client which already decided the peds style.
     */
    async respawn() {
        //Load the model of this ped
        await loadModel(this.model);

        //Create the ped with the giving properties of the ped
        this.scriptID = native.createPed(this.gender=="male"?4:5, this.model, this.pos.x, this.pos.y, this.pos.z, this.heading, false, false);

        //Set the heading of the ped
        native.setEntityHeading(this.scriptID, this.heading);

        //Don't know why this is important, but it is...
        native.setEntityAsMissionEntity(this.scriptID, true, false);
    
        //Set the peds style
        this.setPedComponentVariation();

        //Store this ped by his scriptID as a key
        pedsToScriptID[this.scriptID] = this;
    }

    /**
     * Spawn the ped, decide if this is the first time this ped will ever be created
     */
    async spawn() {
        let spawned = false;
        //If already created, respawn it
        if (this.created) await this.respawn();
        //If it wasn't created and this client is the netOwner: spawn it first time
        else if (this.netOwner == alt.Player.local.id) this.firstSpawn();

        //Start the peds wandering
        if (typeof this.scriptID !== "undefined" && this.scriptID != 0) {
            this.startPath();
            spawned = true;
        }

        if (spawned == false && this.pedSpawnTrys < 10) {
            alt.setTimeout(() => {
                Ped.getByID(this.id).spawn();
            }, this.pedSpawnTryTime);
            this.pedSpawnTryTime *= 2;
            this.pedSpawnTrys += 1;
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
    }

    /**
     * This will be executed at the time this client leaves the streaming-range of
     * the ped.
     * 
     * Peds have to be deleted because the "ped-cache" of GTA is limited.
     */
    outOfRange() {
        //Delete ped
        native.deletePed(this.scriptID);

        //Delete ped scriptID reference
        delete pedsToScriptID[this.scriptID];

        //Set scriptID of the ped to 0
        this.scriptID = 0;
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
        if (navmashPositionsToAdd.length < 1) return;

        //Reset the route of the player
        this.navmashPositions = [];

        //Store and parse the navmeshPositions
        for (let pos of navmashPositionsToAdd) this.navmashPositions.push({x: pos.x, y: pos.y, z: pos.z});

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
        if (this.navmashPositions.length == 0) return;

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
            native.taskFollowNavMeshToCoord(this.scriptID, this.navmashPositions[this.navmashPositions.length-1].x, this.navmashPositions[this.navmashPositions.length-1].y, this.navmashPositions[this.navmashPositions.length-1].z, 1.0, -1, 0.0, true, 0.0);
        }
    }

    /**
     * Method to get the the last navmashPositions
     */
    getPathFinalDestination() {
        return this.navmashPositions[this.navmashPositions.length-1];
    }

    /**
     * Method to set the peds style, using the setPedComponentVariation-native
     */
    setPedComponentVariation() {
        native.setPedComponentVariation(this.scriptID, 0, this.drawable0, this.texture0, 0);
        native.setPedComponentVariation(this.scriptID, 1, this.drawable1, this.texture1, 0);
        native.setPedComponentVariation(this.scriptID, 2, this.drawable2, this.texture2, 0);
        native.setPedComponentVariation(this.scriptID, 3, this.drawable3, this.texture3, 0);
        native.setPedComponentVariation(this.scriptID, 4, this.drawable4, this.texture4, 0);
        native.setPedComponentVariation(this.scriptID, 5, this.drawable5, this.texture5, 0);
        native.setPedComponentVariation(this.scriptID, 6, this.drawable6, this.texture6, 0);
        native.setPedComponentVariation(this.scriptID, 7, this.drawable7, this.texture7, 0);
        native.setPedComponentVariation(this.scriptID, 8, this.drawable8, this.texture8, 0);
        native.setPedComponentVariation(this.scriptID, 9, this.drawable9, this.texture9, 0);
        native.setPedComponentVariation(this.scriptID, 10, this.drawable10, this.texture10, 0);
        native.setPedComponentVariation(this.scriptID, 11, this.drawable11, this.texture11, 0);
    }

    /**
     * Method to update the peds properties
     * 
     * @param ped Ped which has to be updated
     */
    updateProperties(ped) {
        let componentSet = false;
        for (let key of Object.keys(ped)) {
            if (typeof this[key] !== "undefined") this[key] = ped[key];
            if (
                key.includes("drawable") ||
                key.includes("texture") ||
                key.includes("prop")
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
        this.rot = ped.rot;
        this.heading = ped.heading;
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
        let pedsReturn = [];
        for (let ped of Object.values(peds).filter(p => inDistanceBetweenPos(pos, p.pos, distance))) {
            pedsReturn.push(ped);
        }
        return pedsReturn;
    }

    //Method to get all peds which are streamed to the player
    static getAllStreamedPeds() {
        return Object.values(pedsToScriptID);
    }

    static emitParse(ped) {
        let newPed = {};
        for (let key in ped) {
            newPed[key] = ped[key]+"";
        }
        return newPed;
    }

    /**
     * Support-Methods
     */

    /**
     * Method to set all positions of the peds to which this client is the netOwner and
     * send these new positions to the server.
     */
    static setMyPedPoses() {
        //Iterate over all peds which are streamed in filtered by the netOwner == this client
        for (let ped of Ped.getAllStreamedPeds().filter(p => p.netOwner == alt.Player.local.id)) {
            //Get the current positions and set them to the ped
            let pos = native.getEntityCoords(ped.scriptID, true);
            let rot = native.getEntityRotation(ped.scriptID, true);
            ped.pos = {x: pos.x, y: pos.y, z: pos.z};
            ped.rot = {x: rot.x, y: rot.y, z: rot.z};
            ped.heading = native.getEntityHeading(ped.scriptID);

            /**
             * If this peds path has a finalDestination, the position is valid and the final position
             * is closer than 5 feet: This ped is near to its finalDestination - it path sould be
             * new calculated
             */
            if (
                ped.getPathFinalDestination() != null && 
                ped.pos != null && 
                native.getDistanceBetweenCoords(ped.getPathFinalDestination().x, ped.getPathFinalDestination().y, ped.getPathFinalDestination().z, ped.pos.x, ped.pos.y, ped.pos.z, false) < 5
            ) {
                ped.nearFinalPosition = true;
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
        for (let ped of Ped.getAllStreamedPeds().filter(p => p.netOwner == alt.Player.local.id && p.scriptID != 0)) pedsToSend.push({id: ped.id, pos: JSON.stringify(ped.pos), heading: ped.heading, nearFinalPosition: ped.nearFinalPosition});

        //If pedsToSend is not empty: send it to the server
        if(pedsToSend.length > 0) alt.emitServer("pedSyncer:client:positions", pedsToSend);

        alt.setTimeout(Ped.sendMyPedPoses, 500);
    }

    static all;
}

//PedClass Proxy
export const Ped = new Proxy(PedClass, {
    construct(target, args) {
        let ped = new Proxy(new target(...args), {
            set(pedTarget, property, value) {
                peds[pedTarget.id][property] = value;
                return true;
            },
            get: (pedTarget, property, receiver) => {
                if (property == "all") return Object.values(pedsProxies);
                switch (property) {
                    case 'firstSpawn':
                    case 'respawn':
                    case 'spawn':
                    case 'becomeNetOwner':
                    case 'outOfRange':
                    case 'setPath':
                    case 'startPath':
                    case 'getPathFinalDestination':
                    case 'setPedComponentVariation':
                    case 'updateProperties':
                    case 'updatePos':
                    case 'deleteMeta':
                    case 'getMeta':
                    case 'hasMeta':
                    case 'setMeta':
                    case 'getSyncedMeta':
                    case 'hasSyncedMeta':
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

alt.setTimeout(Ped.setMyPedPoses, 500);
alt.setTimeout(Ped.sendMyPedPoses, 500);