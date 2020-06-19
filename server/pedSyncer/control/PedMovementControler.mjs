import alt from 'alt';
import { Ped } from '../class/PedClass.mjs';
import { inDistanceBetweenPos } from '../utils/functions.mjs';
import { getDistanceBetweenPos } from '../utils/functions.mjs';
import got from 'got';

/**
 * Method to set a new wandering path from the pedSyncer-service
 * 
 * @param ped ped which path has to be set
 */
export async function setPedNewPath(ped) {
    let pedToSet = Ped.getByID(ped.id);

    //Set ped is near to its final position
    pedToSet.nearFinalPosition = true;

    //Get new path from the pedSyncer-service
    let newPedPath = await got(
        "https://localhost:44386/getRoute/" 
            + pedToSet.getPathFinalDestination().x.toString().replace(".", ",") + "/"
            + pedToSet.getPathFinalDestination().y.toString().replace(".", ",") + "/"
            + pedToSet.getPathFinalDestination().z.toString().replace(".", ",") + "/"
            + ped.pos.x.toString().replace(".", ",") + "/"
            + ped.pos.y.toString().replace(".", ",") + "/"
            + ped.pos.z.toString().replace(".", ","), {
            https: {
                rejectUnauthorized: false
            }
        }
    );

    //Parse the service answer
    let path = JSON.parse(newPedPath.body);
    alt.setTimeout(() => {
        //Set new path to the ped
        pedToSet.setPath(path.path);
    }, 2000);
}

/**
 * Determine the current nearest navMesh-Position for the start of the server-side ped-movement calculation
 * 
 * @param ped 
 */
function getNearestPathPosition(ped) {
    let minimumPos = null;
    let minimumValue = 1000;

    //Pre-filter the navmashPositions to these positions which are near to the current position
    for (let pathPos of ped.navmashPositions.filter(p => (inDistanceBetweenPos(ped.pos, p, 30)))) {
        let distance = getDistanceBetweenPos(pathPos, ped.pos);
        //if found a new minimum distance store it
        if (minimumValue > distance) {
            minimumPos = pathPos;
            minimumValue = distance;
        }
    }
    //Return the specific navmeshPosition and the index in the navmeshPositions-Array
    return [minimumPos, ped.navmashPositions.indexOf(minimumPos)];
}

/**
 * The idea of the service-side calculation of the ped-movement is to move the peds to its next
 * navmeshPosition.
 * 
 * So the assumption is, that the ped moves one feet (one unit in the distance of two positions)
 * every second. If the next navmeshPosition is in a distance of around 4 feet, the server will
 * move this ped to the next navmeshPosition after 4 seconds.
 * 
 * The variable pedMovements stores the peds to move by the seconds till the peds have to move 
 * in seconds.
 * 
 * Example: If ped A has a distance of 2 feet to the next navmeshPosition, it will stored in
 * pedMovements[2]. After one second, it will be in pedMovements[1]. After the next second, it
 * will be in pedMovements[0] and then will be moved to the next navmeshPositions - then start
 * again.
 * 
 * It will now be called as pedMovments-queue
 */
let pedMovements = {};

//Method to add the ped to the pedMovments-queue
function addPedMovement(i, pedId) {
    if (typeof pedMovements[i] === "undefined") pedMovements[i] = [];
    pedMovements[i].push(pedId);
}

/**
 * Return the peds which have to be moved now (index 0) and decrease all keys by one
 */
function popPedMovements() {
    //Store peds to return
    let toReturn;
    if (typeof pedMovements[0] === "undefined") toReturn = [];
    else toReturn = pedMovements[0];

    //Decrease all other keys than 0 by 1
    let newPedMovements = {};
    for (let i of Object.keys(pedMovements)) {
        if (i == 0) continue;
        newPedMovements[i-1] = pedMovements[i];
    }
    pedMovements = newPedMovements;

    return toReturn;
}

/**
 * Activate the server-side path calculation for a given ped
 * 
 * @param ped Ped which path should be calculated by the server
 */
export async function automaticMovementForPed(ped) {
    //Determine the peds path progress
    let nearestPathPosition = getNearestPathPosition(ped);

    //If the ped has no path or the ped is at the end of its path: calculate new path
    if (nearestPathPosition[0] == null || nearestPathPosition[1] == ped.navmashPositions.length) {
        await setPedNewPath(ped);
        automaticMovementForPed(ped);
    } else {
        //Set peds path to the current path navmeshPosition
        let pedToMove = Ped.getByID(ped.id);
        pedToMove.pos = nearestPathPosition[0];
        pedToMove.currentNavmashPositionsIndex = nearestPathPosition[1];

        //Get the distance to the next navmeshPosition to determine the next ped movement in seconds
        let distanceToNextNavMesh = Math.ceil(getDistanceBetweenPos(pedToMove.pos, pedToMove.navmashPositions[nearestPathPosition[1]+1]));

        //Put ped to the pedMovement-queue
        addPedMovement(distanceToNextNavMesh, ped.id);
    }
}

/**
 * Move peds in pedMovement-queue at key 0 to their next navmeshPosition
 */
function movePeds() {
    //Pop the peds at key 0 and decrease all other keys by 1
    let pedsToMove = popPedMovements();

    for (let pedId of pedsToMove) {
        let ped = Ped.getByID(pedId);

        //If the ped now has a netOwner: Continue with the next ped, current position is now calculated by the netOwner
        if (ped.netOwner != null) continue;

        //If the ped is at the end of its path: get a new path
        if (
            ped.currentNavmashPositionsIndex == ped.navmashPositions.length || 
            typeof ped.navmashPositions[ped.currentNavmashPositionsIndex+1] === "undefined"
        ) setPedNewPath(ped);
        else {
            //Set ped to the next navmeshPosition
            ped.pos = ped.navmashPositions[ped.currentNavmashPositionsIndex+1];
            ped.currentNavmashPositionsIndex = ped.currentNavmashPositionsIndex + 1;

            //Determine the time in seconds for the next change of his position
            let distanceToNextNavMesh = Math.ceil(getDistanceBetweenPos(ped.pos, ped.navmashPositions[ped.currentNavmashPositionsIndex + 1]));

            //Add ped to the pedMovement-queue
            addPedMovement(distanceToNextNavMesh, pedId);
        }
    }

    alt.setTimeout(movePeds, 1000);
}

export function startPedAutomaticMovements() {
    alt.setTimeout(movePeds, 1000);
}