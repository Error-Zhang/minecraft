import { WorldController } from "@engine/core/WorldController.ts";
import { BlockRegistry } from "@engine/block/BlockRegistry.ts";
import { IBlockReflect } from "@/ui-root/api/interface.ts";
import { BlockRecipe } from "@/game-root/block-definitions/BlockRecipes.ts";

export interface UserStore {
	userId: number;
	username: string;
	reset: () => void;
}

export interface GameStore {
	isGaming: boolean;
	gameMode: number;
	isSplitting: boolean;
	isLoading: boolean;
	isInitialized: boolean;
}

export interface PlayerStore {
	origin: { x: number; z: number };
	playerId: number;
	holdBlockId: number;
	setOrigin: (x: number, z: number) => void;
	reset: () => void;
}

export interface WorldStore {
	worldId: number;
	worldHost: string;
	worldMode: number;
	season: number;
	worldController: WorldController | null;
	reset: () => void;
}

export interface BlockStore {
	blockRegistry: BlockRegistry | null;
	blockTypes: IBlockReflect | null;
	blockRecipes: Record<string, BlockRecipe[]> | null;
	blockIcons: Record<string, string> | null;
}
