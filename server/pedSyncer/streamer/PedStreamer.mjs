import * as alt from 'alt';
import { Ped } from '../class/PedClass.mjs';

/**
 * Maybe all of this should be outsourced to the client so the client decides if it shows / streams the ped or not. This
 * is possible because peds positions could be send to the clients very often. But this would mean more network-work.
 * It's an assessment between server-power and network-work + client-power.
 * 
 * Have to think about this. What's your opinion? Tell me!
 */

/**
 * Map peds to an area map to find them faster
 * 
 * Divide the map into zones. Zones are building up by dividing the x- and y-position by 100, so each zone is 100x100 big.
 * 
 * To Check if an ped is in distance to the player, you have to calculate the players current zone and its neighbour-zones.
 * Putting all the peds of these zones together and check the distance of these peds to the player.
 * 
 * This will largly decrease the number of peds which the distance to the player has to be checked. If the player is not on
 * one of these zones, the ped is to far away from the player - it can be excluded from the distance-check.
 * 
 * Ped-Map has to be recalculated very often, because:
 * - its important for the sync
 * - netowner-players are changing the peds very often
 * - automatic-movement of the players changes the position of the player
 * 
 * Maybe its more performant not to recalculate the whole map every 100ms but change the ped-zone position at the time the
 * position of the ped changed. Has to be rewritten.
 */
let pedsMapped = {};
function mapPeds() {
    pedsMapped = {};
    for (let ped of Ped.all) {
        let zoneX = Math.ceil(ped.pos.x/100);
        let zoneY = Math.ceil(ped.pos.y/100);

        if (typeof pedsMapped[zoneX] === "undefined") pedsMapped[zoneX] = {};
        if (typeof pedsMapped[zoneX][zoneY] === "undefined") pedsMapped[zoneX][zoneY] = [];

        pedsMapped[zoneX][zoneY].push(ped);
    }
    alt.setTimeout(mapPeds, 100);
}

/**
 * Get the peds which have to be streamed to the player. According to its position, calculate the players current zone and
 * the neighbours of this zone. Merge the peds of these zones together and give them back as potential new peds to be 
 * streamed to the player.
 */
function getPedsToStream(playerPos) {
    //Calculate the players current zone
    let zoneX = Math.ceil(playerPos.x/100), zoneY = Math.ceil(playerPos.y/100);

    //Merge the peds of this zone and the neighbour zones together. Check the existence of every zone
    let selectedPeds = [];
    if (typeof pedsMapped[zoneX-1] !== "undefined" && typeof pedsMapped[zoneX-1][zoneY-1] !== "undefined") selectedPeds = [...selectedPeds, ...pedsMapped[zoneX-1][zoneY-1]];
    if (typeof pedsMapped[zoneX-1] !== "undefined" && typeof pedsMapped[zoneX-1][zoneY] !== "undefined") selectedPeds = [...selectedPeds, ...pedsMapped[zoneX-1][zoneY]];
    if (typeof pedsMapped[zoneX-1] !== "undefined" && typeof pedsMapped[zoneX-1][zoneY+1] !== "undefined") selectedPeds = [...selectedPeds, ...pedsMapped[zoneX-1][zoneY+1]];
    if (typeof pedsMapped[zoneX] !== "undefined" && typeof pedsMapped[zoneX][zoneY-1] !== "undefined") selectedPeds = [...selectedPeds, ...pedsMapped[zoneX][zoneY-1]];
    if (typeof pedsMapped[zoneX] !== "undefined" && typeof pedsMapped[zoneX][zoneY] !== "undefined") selectedPeds = [...selectedPeds, ...pedsMapped[zoneX][zoneY]];
    if (typeof pedsMapped[zoneX] !== "undefined" && typeof pedsMapped[zoneX][zoneY+1] !== "undefined") selectedPeds = [...selectedPeds, ...pedsMapped[zoneX][zoneY+1]];
    if (typeof pedsMapped[zoneX+1] !== "undefined" && typeof pedsMapped[zoneX+1][zoneY-1] !== "undefined") selectedPeds = [...selectedPeds, ...pedsMapped[zoneX+1][zoneY-1]];
    if (typeof pedsMapped[zoneX+1] !== "undefined" && typeof pedsMapped[zoneX+1][zoneY] !== "undefined") selectedPeds = [...selectedPeds, ...pedsMapped[zoneX+1][zoneY]];
    if (typeof pedsMapped[zoneX+1] !== "undefined" && typeof pedsMapped[zoneX+1][zoneY+1] !== "undefined") selectedPeds = [...selectedPeds, ...pedsMapped[zoneX+1][zoneY+1]];

    //Give back the selected peds of the zones, filtered by the streaming-range
    return selectedPeds.filter(ped => ped.pos.x - playerPos.x < 140 && ped.pos.x - playerPos.x > -140 && ped.pos.y - playerPos.y < 140 && ped.pos.y - playerPos.y > -140);
}

/**
 * Stream peds to the players
 */
function syncPedsToPlayer() {
    for (let player of alt.Player.all) {
        /**
         * player.pedSyncerReady gives the information if the player is ready to get peds synced. Important for
         * restarting the ressource and the event of the player-connect. Players shouldn't get the peds streamed
         * the one ms after it connected - it wouldn't work
         */
        if (typeof player.pedSyncerReady === "undefined") continue;

        //Get the peds which have to be streamed to the player
        let pedsToStreamTemp = getPedsToStream(player.pos);
        let pedsToStream = [];

        //Check all peds
        for (let ped of pedsToStreamTemp) {
            //Check if the Ped is already streamed into the player - if so: continue with the next ped
            if (player.pedsStreamedIn.indexOf(ped.id) != -1) continue;

            //If this ped isn't streamted to anyone make this player the peds netOwner
            if (ped.playerHaveStreamed.length == 0 && ped.netOwner == null) ped.netOwner = player.id;
            //If the ped is streamed into a player, check if it already created - if not, wait for creation of the ped
            else if (ped.created != true && ped.netOwner != player.id) continue;

            pedsToStream.push(ped);

            //Store Ped and Player in support-arrays
            player.pedsStreamedIn.push(ped.id);
            ped.playerHaveStreamed.push(player.id)
        }

        if (pedsToStream.length > 0) {
            alt.emitClient(player, 'pedSyncer:server:stream', pedsToStream);
        }
    }
    alt.setTimeout(syncPedsToPlayer, 1000);
}

export function startPedStreamer() {
    syncPedsToPlayer();
    mapPeds();
}