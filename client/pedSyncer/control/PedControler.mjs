import native from 'natives';
import * as alt from 'alt';
import { Ped } from '../class/PedClass.mjs';
import { inDistanceBetweenPos } from '../utils/functions.mjs';

var pedType = 1654;

export function startPedControler() {
    //Event which fires if syncedMeta of a ped has been deleted
    alt.onServer('pedSyncer:server:metaDelete', (pedId, key) => {
        Ped.getByID(pedId).syncedMetaData[key] = undefined;
        delete Ped.getByID(pedId).syncedMetaData[key];
    });

    //Event which fires if syncedMeta of a ped has been changed
    alt.onServer('pedSyncer:server:metaSet', (pedId, key, value) => {
        Ped.getByID(pedId).syncedMetaData[key] = value;
    });

    //Event which fires if a ped was created on server-side or on connect of this player
    alt.onServer('pedSyncer:server:create', (newPed) => {
        if (typeof Ped.getByID(newPed.id) !== "undefined") return;
        let ped = new Ped(newPed);
    });

    //Event which fires if a ped was deleted
    //Currently inactive
    alt.onServer('pedSyncer:server:delete', (ped) => {

    });

    //Event which fires if the properties of a ped has been changed
    alt.onServer('pedSyncer:server:update', (ped) => {
        Ped.getByID(ped.id).updateProperties(ped);
    });

    //Event which fires if the position was changed
    alt.onServer("entitySync:updatePosition", (entityId, entityType, position) => {
        if (entityType != pedType) return;

        let ped = Ped.getByID(entityId);

        if (typeof ped !== "undefined") {
            ped.pos = position;
            if (ped.scriptID != 0 && !inDistanceBetweenPos(ped.pos, position, 2)) {
                native.setEntityCoords2(ped.scriptID, position.x, position.y, position.z, 1, 0, 0, 1);
            }
        }
    });

    alt.onServer("entitySync:create", (entityId, entityType, position, newEntityData) => {
        if (entityType != pedType) return;
        trySpawn(entityId, position, 0, 100);
    });

    function trySpawn(entityId, position, spawnTrys, spawnTrysTime) {
        if (spawnTrys >= 10) return;
        let ped = Ped.getByID(entityId);

        if (typeof ped === "undefined" || typeof ped.id === "undefined" || ped.id != entityId) {
            alt.setTimeout(() => {
                trySpawn(entityId, position, spawnTrys+1, spawnTrysTime*2);
            }, spawnTrysTime);
        } else {
            ped.pedSpawnTrys = 0;
            ped.pedSpawnTryTime = 100;
            ped.pos = position;
            ped.spawn();
        }
    }

    alt.onServer("entitySync:remove", (entityId, entityType) => {
        if (entityType != pedType) return;
        Ped.getByID(entityId).outOfRange();
    });

    alt.onServer("entitySync:updateData", (entityId, entityType, newEntityData) => {
        if (entityType != pedType) return;
        if (newEntityData["netOwner"] == alt.Player.local.id) setNetOwner(entityId, 0, 100);
    });

    alt.onServer("entitySync:netOwner", (entityId, entityType, isNetOwner) => {
        if (entityType != pedType) return;
        if (isNetOwner) {
            alt.log("Become NetOwner: " + entityId);
            setNetOwner(entityId, 0, 100);
        }
    });

    function setNetOwner(entityId, setTrys, setTrysTime) {
        if (setTrys >= 10) return;
        let ped = Ped.getByID(entityId);

        if (typeof ped === "undefined") {
            alt.setTimeout(() => {
                setNetOwner(entityId, setTrys+1, setTrysTime*2);
            }, setTrysTime);
        } else {
            ped.becomeNetOwner();
        }
    }
    
    //Event which fires if a ped has a new path
    alt.onServer('pedSyncer:server:path', (pedId, path) => {
        Ped.getByID(pedId).setPath(path);
    });

    //Delete all current peds if the ressource stop (important for development)
    alt.on("resourceStop", () => {
        for (let ped of Ped.getAllStreamedPeds()) native.deletePed(ped.scriptID);
    });
}