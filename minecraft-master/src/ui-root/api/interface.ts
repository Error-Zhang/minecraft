export interface IUser {
	id?: number;
	userName: string;
	passWord: string;
}

export interface IPlayer {
	id: number;
	playerName: string;
	sex: number;
	createdAt: string;
	user?: IUser;
	userId: number;
	worldId: number;
}

export interface IWorld {
	id?: number;
	userId?: number;
	worldName: string;
	seed: string;
	gameMode: number;
	worldMode: number;
	season: number;
	isPublic: number;
	user?: IUser;
	players?: IPlayer[];
	createdAt?: string;
}

export interface IBlockReflect {
	byId: Record<number, string>;
	byName: Record<string, number>;
}

export interface IChunkData {
	x: number;
	z: number;
	cells: number[];
	shafts: number[];
}
