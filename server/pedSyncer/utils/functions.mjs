/**
 * Function to get the distance of two given positions
 * @param pos1 
 * @param pos2 
 */
export function getDistanceBetweenPos(pos1, pos2) {
    if (typeof pos1 !== 'undefined' && typeof pos2 !== 'undefined' && typeof pos1.x !== 'undefined' && typeof pos2.x !== 'undefined') {
        return Math.sqrt(Math.pow((pos1.x - pos2.x), 2) + Math.pow((pos1.y - pos2.y), 2) + Math.pow((pos1.z - pos2.z), 2));
    } else return -1;
}

/**
 * Boolean-Function which gives true if the distance of two positions is smaller than an given distance, else false
 * @param pos1 
 * @param pos2 
 * @param distance 
 */
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
        return getDistanceBetweenPos(pos1, pos2) < distance;
    } else return false;
}

/**
 * Auxiliary Method to remove one element of an array and giving the array back
 * @param array 
 * @param elementToRemove 
 */
export function removeElementFromArray(array, elementToRemove) {
    let count = array.length;
    let index = array.indexOf(elementToRemove);
    if (index !== -1) array.splice(index, 1);
    
    if (count != array.length) return removeElementFromArray(array, elementToRemove);
    return array;
}