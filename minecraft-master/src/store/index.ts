import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";
import { BlockStore, GameStore, PlayerStore, UserStore, WorldStore } from "./interface.ts";

export const useUserStore = create<UserStore>()(
	persist(
		set => ({
			userId: 0,
			username: "",
			reset: () => set({ userId: 0, username: "" }),
		}),
		{
			name: "user-storage",
			storage: createJSONStorage(() => sessionStorage),
		}
	)
);

export const useGameStore = create<GameStore>(set => ({
	gameMode: 0,
	isGaming: false,
	isSplitting: false,
	isLoading: true,
	isInitialized: false,
}));

export const usePlayerStore = create<PlayerStore>((set, get) => ({
	playerId: 0,
	origin: { x: -112, z: -112 },
	holdBlockId: 0,
	setOrigin: (x: number, z: number) => set({ origin: { x, z } }),
	reset: () => set({ playerId: 0, holdBlockId: 0, origin: { x: -112, z: -112 } }),
}));

export const useWorldStore = create<WorldStore>(set => ({
	worldId: 0,
	worldHost: "",
	worldMode: 0,
	season: 0,
	worldController: null,
	reset: () => set({ worldId: 0, worldController: null }),
}));

export const useBlockStore = create<BlockStore>((set, get) => ({
	blockRegistry: null,
	blockTypes: null,
	blockRecipes: null,
	blockIcons: null,
	reset: () => set({ blockRegistry: null, blockTypes: null, blockRecipes: null, blockIcons: null }),
}));
