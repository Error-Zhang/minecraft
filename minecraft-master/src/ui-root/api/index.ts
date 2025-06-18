import { del, get, post, put } from "./request.ts";
import { IBlockReflect, IChunkData, IPlayer, IUser, IWorld } from "./interface.ts";
import { useUserStore } from "@/store";
import { Chunk } from "@engine/chunk/Chunk.ts";

const getUserId = () => useUserStore.getState().userId;

export const userApi = {
	// 登录
	login(user: IUser) {
		return get<IUser>("/user", user);
	},

	// 注册
	register(user: IUser) {
		return post<IUser>("/user", user);
	},
};

export const playerApi = {
	addPlayerToWorld(player: IPlayer) {
		return post("/player", player);
	},
	getPlayer(worldId: number) {
		return get<IPlayer>(`/player?worldId=${worldId}&userId=${getUserId()}`);
	},
	getPlayerList(worldId: number) {
		return get<IPlayer[]>(`/player/list/${worldId}`);
	},
};

export const worldApi = {
	// 获取某用户的所有世界
	getWorldList() {
		return get<IWorld[]>(`/world/${getUserId()}`);
	},

	// 创建一个新世界
	createWorld<IWorld>(world: IWorld) {
		return post("/world", { ...world, userId: getUserId() });
	},

	updateWorld<IWorld>(world: IWorld) {
		return put("/world", { ...world, userId: getUserId() });
	},

	// 删除一个世界
	deleteWorld(worldId: number) {
		return del(`/world?worldId=${worldId}&userId=${getUserId()}`);
	},

	generateChunks(worldId: number, coords: { x: number; z: number }[]) {
		return post<IChunkData[]>(`/chunk/generate/${worldId}`, coords);
	},

	async generateFlatWorld(worldId: number, coords: { x: number; z: number }[]) {
		const generator = (chunkX: number, chunkZ: number) => {
			const chunkData: IChunkData = {
				cells: [],
				shafts: [],
				x: chunkX,
				z: chunkZ,
			};

			// 生成基础地形
			for (let y = 0; y < 128; y++) {
				const id = y == 4 ? 4 : y < 4 ? 3 : 0;
				for (let z = 0; z < 16; z++) {
					for (let x = 0; x < 16; x++) {
						const index = Chunk.getIndex(x, y, z);
						chunkData.cells[index] = id;
					}
				}
			}
			return chunkData;
		};
		return coords.map(coord => generator(coord.x, coord.z));
	},
};

export const blockApi = {
	getBlockTypes() {
		return get<IBlockReflect>("/block/types");
	},
};
