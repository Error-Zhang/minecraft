import { ChunkData } from "../types/chunk.type.ts";
import { ChunkManager } from "../chunk/ChunkManager.ts";
import { BlockRegistry } from "@engine/block/BlockRegistry.ts";
import { BlockEntity } from "@engine/types/block.type.ts";

export class Chunk {
	public position!: { x: number; z: number };
	public blocks!: Uint16Array;
	public edges: Set<number> = new Set();
	public blockEntities: Record<string, BlockEntity> = {};
	public shafts!: ChunkData["shafts"];
	public isVisible: boolean = true;

	constructor() {}

	public static get Height() {
		return this._height;
	}

	public static set Height(value: number) {
		this._height = value;
	}

	public static get Size() {
		return this._size;
	}

	public static set Size(value: number) {
		this._size = value;
	}

	private static _height: number = 128;

	public get height(): number {
		return Chunk.Height;
	}

	private static _size: number = 16;

	public get size(): number {
		return Chunk.Size;
	}

	public get Key() {
		return `${this.position.x},${this.position.z}`;
	}

	public static fromJSON(data: ChunkData): Chunk {
		const chunk = new Chunk();
		chunk.position = data.position;
		chunk.blocks = data.blocks;
		chunk.shafts = data.shafts;
		return chunk;
	}

	public static getIndex(x: number, y: number, z: number) {
		return y + x * Chunk.Height + z * Chunk.Height * Chunk.Size;
	}

	public static getCoordinates(index: number): { x: number; y: number; z: number } {
		const y = index % Chunk.Height;
		const x = Math.floor(index / Chunk.Height) % Chunk.Size;
		const z = Math.floor(index / (Chunk.Height * Chunk.Size));
		return { x, y, z };
	}

	public static getBlock(chunkData: ChunkData, x: number, y: number, z: number) {
		let i = this.getIndex(x, y, z);
		return chunkData.blocks[i];
	}

	public static getEnvironment(chunkData: ChunkData, x: number, z: number) {
		return chunkData.shafts[x + z * Chunk.Size] || -1;
	}

	/**
	 * 支持跨区块边界的 Block 获取
	 */
	public static getBlockWithEdge(chunk: Chunk, x: number, y: number, z: number): number {
		const edges: Record<string, Uint16Array> = (chunk as any).aroundEdges;
		const size = Chunk.Size;
		const height = Chunk.Height;

		if (x >= 0 && x < size && z >= 0 && z < size && y >= 0 && y < height) {
			return Chunk.getBlock(chunk, x, y, z);
		}

		if (y < 0 || y >= height) return -1;

		// 处理四个方向
		if (x === -1 && edges.west) {
			if (z >= 0 && z < size) {
				const idx = y + z * height;
				return edges.west[idx];
			}
		} else if (x === size && edges.east) {
			if (z >= 0 && z < size) {
				const idx = y + z * height;
				return edges.east[idx];
			}
		} else if (z === -1 && edges.south) {
			if (x >= 0 && x < size) {
				const idx = y + x * height;
				return edges.south[idx];
			}
		} else if (z === size && edges.north) {
			if (x >= 0 && x < size) {
				const idx = y + x * height;
				return edges.north[idx];
			}
		}
		// 超出支持边界
		return -1;
	}

	public static isInBounds(chunk: Chunk, x: number, y: number, z: number): boolean {
		const chunkX = chunk.position.x * Chunk.Size;
		const chunkZ = chunk.position.z * Chunk.Size;
		return (
			x >= chunkX &&
			x < chunkX + Chunk.Size &&
			z >= chunkZ &&
			z < chunkZ + Chunk.Size &&
			y >= 0 &&
			y < Chunk.Height
		);
	}

	public getBlock(x: number, y: number, z: number): number {
		if (!this.isInBounds(x, y, z)) return -1;
		return this.blocks[this.index(x, y, z)] ?? 0;
	}

	public getEnvironment(x: number, z: number): number {
		return this.shafts[x + z * Chunk.Size] || -1;
	}

	public setBlock(x: number, y: number, z: number, blockId: number) {
		if (!this.isInBounds(x, y, z)) return;
		this.blocks[this.index(x, y, z)] = blockId;
		const def = BlockRegistry.Instance.getById(blockId);
		if (def?.createEntity) {
			this.setBlockEntity(x, y, z, def.createEntity());
		}
	}

	public getBlockEntity(x: number, y: number, z: number) {
		return this.blockEntities[`${x},${y},${z}`];
	}

	public setBlockEntity(x: number, y: number, z: number, entity: BlockEntity) {
		this.blockEntities[`${x},${y},${z}`] = entity;
	}

	public removeBlockEntity(x: number, y: number, z: number) {
		delete this.blockEntities[`${x},${y},${z}`];
	}

	public isInBounds(x: number, y: number, z: number): boolean {
		return x >= 0 && x < Chunk.Size && z >= 0 && z < Chunk.Size && y >= 0 && y < Chunk.Height;
	}

	public getAroundEdges() {
		return {
			north: this.getNeighborSlice("north"),
			south: this.getNeighborSlice("south"),
			east: this.getNeighborSlice("east"),
			west: this.getNeighborSlice("west"),
		};
	}

	public getNeighborSlice(direction: string): Uint16Array | null {
		const neighbor = this.getNeighbor(direction);
		if (!neighbor) return null;

		const size = Chunk.Size;
		const height = Chunk.Height;
		const slice = new Uint16Array(size * height); // Z * Y（或 X * Y，取决于方向）

		for (let i = 0; i < size; i++) {
			for (let y = 0; y < height; y++) {
				let blockId = 0;

				switch (direction) {
					case "north":
						blockId = neighbor.getBlock(i, y, 0); // neighbor 的最南边
						break;
					case "south":
						blockId = neighbor.getBlock(i, y, size - 1); // neighbor 的最北边
						break;
					case "east":
						blockId = neighbor.getBlock(0, y, i); // neighbor 的最西边
						break;
					case "west":
						blockId = neighbor.getBlock(size - 1, y, i); // neighbor 的最东边
						break;
				}

				slice[i * height + y] = BlockRegistry.Instance.decodeId?.(blockId) ?? blockId;
			}
		}

		return slice;
	}

	private index(x: number, y: number, z: number): number {
		return y + x * Chunk.Height + z * Chunk.Height * Chunk.Size;
	}

	private getNeighbor(direction: string): Chunk | undefined {
		const { x, z } = this.position;
		const manager = ChunkManager.Instance;
		switch (direction) {
			case "north":
				return manager.getChunk(x, z + 1);
			case "south":
				return manager.getChunk(x, z - 1);
			case "east":
				return manager.getChunk(x + 1, z);
			case "west":
				return manager.getChunk(x - 1, z);
		}
	}
}
