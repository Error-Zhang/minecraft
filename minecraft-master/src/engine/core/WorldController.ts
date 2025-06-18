import { ChunkManager } from "../chunk/ChunkManager.ts";
import { BlockRegistry } from "../block/BlockRegistry.ts";
import { Position } from "../types/chunk.type.ts";
import { Environment } from "@engine/environment/Environment.ts";
import { Chunk } from "@engine/chunk/Chunk.ts";

export class WorldController {
	private lastUpdatePos?: { x: number; z: number };
	private lastSkyPos?: { x: number; z: number };

	constructor(
		public chunkManager: ChunkManager,
		public sky: Environment
	) {}

	/**
	 * 根据玩家位置更新区块和天空
	 */
	async updateChunk(position: { x: number; z: number }) {
		const { x, z } = position;
		const shouldUpdateSky =
			!this.lastSkyPos ||
			Math.max(Math.abs(x - this.lastSkyPos.x), Math.abs(z - this.lastSkyPos.z)) >
				this.sky.MinUpdateDistance;

		if (shouldUpdateSky) {
			this.sky.updatePosition(x, z);
			this.lastSkyPos = { x, z };
		}

		const shouldUpdateChunk =
			!this.lastUpdatePos ||
			Math.max(Math.abs(x - this.lastUpdatePos.x), Math.abs(z - this.lastUpdatePos.z)) >=
				ChunkManager.MinUpdateDistance;

		if (shouldUpdateChunk) {
			await this.chunkManager.updateChunksAround(x, z);
			this.lastUpdatePos = { x, z };
		}
	}

	getChunkCenterTop(x: number, z: number): [number, number, number] {
		const [chunkX, chunkZ] = this.chunkManager.worldToChunk(x, z);
		const [centerX, centerZ] = this.chunkManager.getChunkCenter(chunkX, chunkZ);
		return [centerX, this.chunkManager.getColumnHeight(x, z), centerZ];
	}

	onChunkUnload(callback: (chunk: Chunk) => void) {
		this.chunkManager.onChunkUnload(callback);
	}

	onChunkUpdated(callback: (progress: number) => void) {
		return this.chunkManager.onUpdated(callback);
	}

	offChunkUpdated(id: number) {
		this.chunkManager.offUpdated(id);
	}

	/**
	 * 获取指定位置的方块信息
	 */
	getBlock(position: Position) {
		const blockId = ChunkManager.getBlockAt(position.x, position.y, position.z);
		return BlockRegistry.Instance.decodeGetById(blockId);
	}

	/**
	 * 设置指定位置的方块ID
	 */
	setBlock(position: Position, blockId: number) {
		this.chunkManager.setBlock(position.x, position.y, position.z, blockId);
	}

	setBlocks(blocks: { x: number; y: number; z: number; blockId: number }[]) {
		this.chunkManager.setBlocks(blocks);
	}
}
