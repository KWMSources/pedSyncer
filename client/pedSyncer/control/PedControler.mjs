import native from 'natives';
import * as alt from 'alt';
import { Ped } from '../class/PedClass.mjs';

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

    //Event which fires if this player gets a ped to stream
    alt.onServer('pedSyncer:server:stream', (peds) => {
        for (let ped of peds) {
            Ped.getByID(ped.id).updatePos(ped);
            if (ped.netOwner == alt.Player.local.id) Ped.getByID(ped.id).netOwner = alt.Player.local.id;
            Ped.getByID(ped.id).spawn();
        }
    });

    //Event which fires if this player becomes a netOwner of a ped, so this client now knows his task for this ped
    alt.onServer('pedSyncer:server:newNetOwner', (pedId) => {
        Ped.getByID(pedId).becomeNetOwner();
    });

    //Event which fires if a ped has a new path
    alt.onServer('pedSyncer:server:path', (pedId, path) => {
        Ped.getByID(pedId).setPath(path);
    });

    //Delete all current peds if the ressource stop (important for development)
    alt.on("resourceStop", () => {
        for (let ped of Ped.getAllStreamedPeds()) native.deletePed(ped.scriptID);
    });
}