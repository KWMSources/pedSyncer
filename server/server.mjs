import { startPedControler } from './pedSyncer/control/PedControler.mjs';

/**
 * ToDos:
 * - Variable for Streaming-Range
 * - Variable for ped-count
 * - Ped-Death-Handling
 * - Ped-Dumbling and shocking event reaction
 * - Street-Crossing
 * - Vehicles
 * - Handling for Wandering, Freezing, Inviciblity, Health, Armour, Weapons
 * - Syncing of the peds attributes
 * - Outsourcing Stream-Mechanics to the client?
 * - Ped Path Smoothing
 * - Handling with Peds which starts on a navMesh without a neighbour navMesh (route cannot be calculated)
 */

startPedControler();