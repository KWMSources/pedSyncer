import * as PedSyncer from 'pedSyncer';

class PedClass {
    //ID of the ped
    id = null;

    //Dimension of the ped
    dimension = 0;

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
     * Current position and heading of the ped
     * 
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
    dead = false;

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

    /**
     * Object Methods
     */
    constructor(id) {
        this.id = id;
    }

    destroy() {
        PedSyncer.destroyPed(this.id);
    }

    startWandering() {
        PedSyncer.startWandering(this.id);
    }
    
    continueWandering() {
        PedSyncer.continueWandering(this.id);
    }
    
    /**
     * Class Methods
     */

   //Method to get the ped by the ID
   static getByID(id) {
       return pedsProxies[id];
   }

}

var peds = {};
var pedsProxies = {};

//PedClass Proxy
export const Ped = new Proxy(PedClass, {
    construct(target, args) {
        let id;

        if (args[0] != "id") id = PedSyncer.createPed(args[0], args[1], args[2], typeof args[3] !== "undefined"?args[3]:null);
        else id = args[1];

        let ped = new Proxy(new target(id), {
            set(pedTarget, property, value) {
                PedSyncer.setPedData(pedTarget.id, property, value);
                pedTarget[property] = value;
                return true;
            },
            get: (pedTarget, property, receiver) => {
                if (property == "all") {
                    let ids = PedSyncer.getAllPedIds();

                    for (let idNeu of ids) {
                        if (typeof pedsProxies[idNeu] === "undefined") {
                            let ped = new Ped("id", idNeu);
                        }
                    }

                    return Object.values(pedsProxies);
                }
                switch (property) {
                    case 'destroy':
                    case 'startWandering':
                    case 'continueWandering':
                        return function() {
                            return peds[pedTarget.id][property].apply(this, arguments);
                        }
                    case 'createCitizenPeds':
                        PedSyncer.createCitizenPeds();
                        return;
                    default:
                        let value = PedSyncer.getPedData(pedTarget.id, property);
                        pedTarget[property] = value;
                        return value;
                }
            }
        });

        pedsProxies[ped.id] = ped;

        return ped;
    },
    get: (target, property, receiver) => {
        if (property == "all") {
            let ids = PedSyncer.getAllPedIds();

            for (let id of ids) {
                if (typeof pedsProxies[id] === "undefined") {
                    let ped = new Ped("id", id);
                }
            }

            return Object.values(pedsProxies);
        } else if (property == "getNear") {
            return function() {
                let ids = PedSyncer.getNear(arguments[0], arguments[1]);
                let pedsReturn = [];

                for (let id of ids) {
                    if (typeof pedsProxies[id] === "undefined") {
                        let ped = new Ped("id", id);
                    }
                    pedsReturn.push(pedsProxies[id]);
                }

                return pedsReturn;
            }
        } else if (property == "createCitizenPeds") {
            return function() {
                PedSyncer.createCitizenPeds();
            }
        }
        return target[property];
    },
    set: (pedTarget, property, value) => {
        return true;
    }
});