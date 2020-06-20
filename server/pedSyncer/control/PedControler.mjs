import alt from 'alt';
import { Ped } from '../class/PedClass.mjs';
import { startPedStreamer } from '../streamer/PedStreamer.mjs';
import { removeElementFromArray } from '../utils/functions.mjs';
import { setPedNewPath } from './PedMovementControler.mjs';
import { automaticMovementForPed } from './PedMovementControler.mjs';
import { startPedAutomaticMovements } from './PedMovementControler.mjs';

export function startPedControler() {
    /**
     * Event which is fired on the time the first netOwner of a ped created the ped and decided
     * the 'look' of the ped.
     */
    alt.onClient('pedSyncer:client:firstSpawn', (player, pedClient) => {
        //Get all 'style'-properties of the ped and store it to the server-side object of this ped
        let ped = Ped.getByID(pedClient.id);
        for (let key of Object.keys(pedClient)) {
            if (
                typeof ped[key] !== "undefined" && (
                    key.includes("drawable") ||
                    key.includes("texture") ||
                    key.includes("prop") ||
                    key.includes("model") ||
                    key.includes("gender")
                )
            ) ped[key] = pedClient[key];
        }

        //Ped is now created and can now be streamed to other players
        ped.created = true;

        //Update the client-side ped-objects
        alt.emitClient(null, "pedSyncer:server:update", Ped.getByID(pedClient.id));
    });

    //Event which is fired on the time that the ped is out of the streaming range of the ped-netowner-player
    alt.onClient('pedSyncer:client:outOfRange', playerNoLongerNetOwner);

    //Event which is fired on the players disconnect
    //The netOwners of the players netowning peds as the be reassigned
    alt.on("playerDisconnect", (player) => {
        for (let ped of Ped.all.filter(p => ped.netOwner == player.id)) {
            playerNoLongerNetOwner(player, ped.id);
        }
    });

    //Function to handle the event that an given player is no longer the netOwner
    function playerNoLongerNetOwner(player, pedClientId) {
        let ped = Ped.getByID(pedClientId);

        //Remove the ped out of the array of the peds which are streamed for the player
        removeElementFromArray(player.pedsStreamedIn, pedClientId);

        //Rempve the player out of the array of the players to which this ped is streamed in
        removeElementFromArray(ped.playerHaveStreamed, player.id);

        if (ped.playerHaveStreamed.length == 0) {
            //If the ped is now not streamed to any player: set netOwner null and start the server-calculcated movement of the ped
            ped.netOwner = null;
            automaticMovementForPed(ped);
        } else {
            /**
             * Determine the new netOwner of the ped
             * Check if these players are still connected - if not, remove them from playerHaveStreamed
             * newNetOwnerSet: check if a new netOwner was set - if not, proceed with the server-calculation of the peds position
             */
            let newNetOwnerSet = false;

            //Proceed for every player the ped is streamed to
            for (let playerId of ped.playerHaveStreamed) {
                //Check if the player is still connected
                if (alt.Player.getByID(playerId) == null || alt.Player.getByID(playerId).valid != true) {
                    removeElementFromArray(ped.playerHaveStreamed, playerId);
                } else {
                    //If so, assign this player as the new netOwner
                    ped.netOwner = ped.playerHaveStreamed[0];
                    alt.emitClient(alt.Player.getByID(ped.netOwner), 'pedSyncer:server:newNetOwner', pedClientId);
                    newNetOwnerSet = true;
                    break;
                }
            }

            //The ped is now not streamed to any player: set netOwner null and start the server-calculcated movement of the ped
            if (newNetOwnerSet == false) {
                ped.netOwner = null;
                automaticMovementForPed(ped);
            }
        }
    }

    /**
     * Event which is fired on the sending of the current positions of the peds by their netOwner
     * This event will be fired very, very often.
     */
    alt.onClient("pedSyncer:client:positions", (player, peds) => {
        if (peds.length == 0) return;
        for (let ped of peds) {
            if (typeof ped.pos === "undefined" || typeof ped.pos.x === "undefined" || typeof ped.pos.y === "undefined" || typeof ped.pos.z === "undefined") continue;
            let pedToUpdate = Ped.getByID(ped.id);
            pedToUpdate.pos = ped.pos;
            pedToUpdate.rot = ped.rot;
            pedToUpdate.heading = ped.heading;
            
            if (ped.nearFinalPosition && !pedToUpdate.nearFinalPosition) setPedNewPath(ped);
        }
    });

    /**
     * If the ressource was restarted set all now connected player to ready for the ped streamer
     */
    alt.on("resourceStart", () => {
        for (let player of alt.Player.all) {
            player.pedsStreamedIn = [];
            player.pedSyncerReady = true;
        }
    });

    /**
     * On connection of a player:
     * 1. wait 5 seconds for the start of the peds client
     * 2. send all peds to the player
     * 3. wait 5 seconds for the client to handle all peds
     * 4. set the player ready for ped-streaming
     */
    alt.on("playerConnect", (player) => {
        alt.setTimeout(() => {
            for (let ped of Ped.all) alt.emitClient(player, "pedSyncer:server:create", ped);
            alt.setTimeout(() => {
                player.pedsStreamedIn = [];
                player.pedSyncerReady = true;
            }, 5000);
        }, 5000);
    });

    //Start the server-side ped-movement calculcation
    startPedAutomaticMovements();

    alt.setTimeout(() => {
        startPedStreamer();
        Ped.createCitizenPeds();
    }, 5000);
}