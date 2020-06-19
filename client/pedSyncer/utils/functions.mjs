import native from 'natives';
import alt from 'alt';

export function getDistanceBetweenPos(pos1, pos2) {
    if (typeof pos1 !== 'undefined' && typeof pos2 !== 'undefined' && typeof pos1.x !== 'undefined' && typeof pos2.x !== 'undefined') {
        return Math.sqrt(Math.pow((pos1.x - pos2.x), 2) + Math.pow((pos1.y - pos2.y), 2) + Math.pow((pos1.z - pos2.z), 2));
    } else return -1;
}

export function inDistanceBetweenPos(pos1, pos2, distance) {
    if (
        pos1.x-pos2.x > distance ||
        pos1.y-pos2.y > distance ||
        pos1.z-pos2.z > distance ||
        pos1.x-pos2.x < (-1)*distance ||
        pos1.y-pos2.y < (-1)*distance ||
        pos1.z-pos2.z < (-1)*distance 
    ) return false;
    if (typeof pos1 !== 'undefined' && typeof pos2 !== 'undefined' && typeof pos1.x !== 'undefined' && typeof pos2.x !== 'undefined') {
        return Math.sqrt(Math.pow((pos1.x - pos2.x), 2) + Math.pow((pos1.y - pos2.y), 2) + Math.pow((pos1.z - pos2.z), 2)) < distance;
    } else return false;
}

export function loadModel(classname) {
    return new Promise((resolve, reject) => {
        if (typeof classname === 'string' && classname.substr(0, 2) === '0x') {
            classname = parseInt(classname);
        } else if (typeof classname === 'string') {
            classname = native.getHashKey(classname);
        }

        if (!native.isModelValid(classname)) return resolve(false);

        if (native.hasModelLoaded(classname)) return resolve(classname);

        native.requestModel(classname);

        let interval = alt.setInterval(() => {
            if (native.hasModelLoaded(classname)) {
                alt.clearInterval(interval);

                return resolve(classname);
            }
        }, 10);
    });
}