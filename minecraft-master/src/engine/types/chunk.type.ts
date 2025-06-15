export interface ChunkData {
	blocks: Uint16Array;
	position: { x: number; z: number };
	shafts: Uint8Array;
}

export type Coords = {
	x: number;
	z: number;
}[];

export interface Position {
	x: number;
	y: number;
	z: number;
}
