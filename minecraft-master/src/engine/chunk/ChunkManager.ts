import { WorldRenderer } from "../renderer/WorldRenderer";
import { Chunk } from "./Chunk";
import { BlockEntity } from "../types/block.type.ts";
import { SingleClass } from "../core/Singleton.ts";
import { Coords } from "../types/chunk.type.ts";
import { EdgeConfigs } from "../renderer/Constant.ts";
import { BlockRegistry } from "../block/BlockRegistry.ts";
import { MiniBlockBuilder } from "@engine/renderer/MiniBlockBuilder.ts";
import { Vector3 } from "@babylonjs/core";

// requestIdleCallback 兼容封装
const runIdle =
	typeof requestIdleCallback === "function"
		? (cb: IdleRequestCallback) => requestIdleCallback(cb)
		: (cb: IdleRequestCallback) =>
				setTimeout(() => cb({ timeRemaining: () => 16, didTimeout: false } as any), 1);

export class ChunkManager extends SingleClass {
	// 半径(单位区块)
	public static ViewDistance = 4;
	private _use: boolean = false;
	// 实例部分
	private chunks = new Map<string, Chunk>();
	private readonly generator: (coords: Coords) => Promise<Chunk[]>;
	private readonly worldRenderer: WorldRenderer;
	private unloadCallbacks: Array<(chunk: Chunk) => void> = [];
	private loadCallbacks: Array<(chunk: Chunk) => void> = [];
	private updatedCallbacks: Array<(progress: number) => void> = [];
	private isUpdating: boolean = false;
	private unloadTimer: NodeJS.Timeout | null = null;
	private chunksToUnload: Map<string, number> = new Map(); // 存储区块key和标记时间戳
	private readonly UNLOAD_DELAY = 1000 * 30; // 60秒后卸载
	private readonly POLLING_TIME = 1000 * 30; // 每分检查一次

	constructor(generator: (coords: Coords) => Promise<Chunk[]>) {
		super();
		this._use = true;
		this.generator = generator;
		this.worldRenderer = WorldRenderer.getInstance();
	}

	public static get LoadDistance() {
		return this.ViewDistance;
	}

	public static get UnloadDistance() {
		return this.LoadDistance + 1;
	}

	public static override get Instance(): ChunkManager {
		return this.getInstance();
	}

	// 半径(单位方块)
	public static get MinUpdateDistance() {
		return Chunk.Size;
	}

	// 静态方法：全局访问区块方块数据
	public static getBlockAt(x: number, y: number, z: number): number {
		return this.Instance.getBlock(x, y, z);
	}

	public static async setBlockAt(x: number, y: number, z: number, blockId: number) {
		this.Instance.setBlock(x, y, z, blockId);
	}

	public static getBlockInfo(x: number, y: number, z: number) {
		let blockValue = ChunkManager.getBlockAt(x, y, z);
		let envValue = ChunkManager.Instance.getEnvironment(x, z);
		let id = BlockRegistry.Instance.decodeId?.(blockValue) ?? blockValue;
		let def = BlockRegistry.Instance.getById(id)!;
		return [blockValue, id, def, envValue] as const;
	}

	public getEnvironment(x: number, z: number) {
		const [chunkX, chunkZ, localX, localZ] = this.worldToChunk(x, z);
		const chunk = this.getChunk(chunkX, chunkZ);
		return chunk?.getEnvironment(x, z) || -1;
	}

	public onChunkLoad(callback: (chunk: Chunk) => void) {
		this.loadCallbacks.push(callback);
	}

	public onChunkUnload(callback: (chunk: Chunk) => void) {
		this.unloadCallbacks.push(callback);
	}

	public async updateChunksAround(x: number, z: number) {
		await this.withUpdateLock(async () => {
			const start = performance.now();
			try {
				console.log("[VoxelEngine] chunk updating");
				const [chunkX, chunkZ] = this.worldToChunk(x, z);

				const minX = chunkX - ChunkManager.LoadDistance;
				const maxX = chunkX + ChunkManager.LoadDistance;
				const minZ = chunkZ - ChunkManager.LoadDistance;
				const maxZ = chunkZ + ChunkManager.LoadDistance;

				// 卸载超出范围的区块
				this.getSortedChunks(Array.from(this.chunks.values()), chunkX, chunkZ)
					.reverse()
					.forEach(chunk => {
						const { x, z } = chunk.position;
						const dist = this.getChunkApproxDistance(x, z, chunkX, chunkZ);
						chunk.isVisible = dist <= ChunkManager.ViewDistance;
						this.worldRenderer.setChunkEnabled(chunk);
						if (dist > ChunkManager.UnloadDistance * 1.5) {
							this.unloadChunk(chunk);
						} else if (dist > ChunkManager.UnloadDistance) {
							this.scheduleUnloadChunk(chunk);
						} else {
							this.cancelScheduledUnload(chunk);
						}
					});

				// 收集需要加载的区块坐标
				const coordsToLoad: Coords = [];
				for (let cz = minZ; cz <= maxZ; cz++) {
					for (let cx = minX; cx <= maxX; cx++) {
						const dist = this.getChunkApproxDistance(cx, cz, chunkX, chunkZ);
						const key = this.chunkKey(cx, cz);
						if (dist <= ChunkManager.ViewDistance && !this.chunks.has(key)) {
							coordsToLoad.push({ x: cx, z: cz });
						}
					}
				}

				// 批量生成区块
				if (coordsToLoad.length) {
					const chunks = await this.generator(coordsToLoad);
					for (const chunk of chunks) {
						this.chunks.set(chunk.Key, chunk);
					}
				}

				// 按距离排序
				const chunks = this.getSortedChunks(Array.from(this.chunks.values()), chunkX, chunkZ);

				await new Promise<void>(resolve => {
					let currentIndex = 0;
					const maxPerFrame = 1;

					const processChunksFrame = async () => {
						let count = 0;

						while (currentIndex < chunks.length && count < maxPerFrame) {
							if (!this._use) return;
							const chunk = chunks[currentIndex++];
							await this.worldRenderer.buildChunk(chunk);
							this.execUpdated(currentIndex / (chunks.length - 1));
							count++;
						}

						if (currentIndex < chunks.length) {
							if (!this._use) return;
							requestAnimationFrame(processChunksFrame);
						} else {
							resolve();
						}
					};

					requestAnimationFrame(processChunksFrame);
				});
			} catch (error) {
				console.error("[VoxelEngine] Error updating chunks:", error);
			} finally {
				this.updateChunkEdges();
				const end = performance.now();
				console.log(`[VoxelEngine] 区块更新完成，处理耗时: ${(end - start).toFixed(2)} ms`);
			}
		});
	}

	public onUpdated(callback: (progress: number) => void) {
		this.updatedCallbacks.push(callback);
		return this.updatedCallbacks.length - 1;
	}

	public offUpdated(id: number) {
		this.updatedCallbacks.splice(id, 1);
	}

	public getChunk(chunkX: number, chunkZ: number) {
		return this.chunks.get(this.chunkKey(chunkX, chunkZ));
	}

	public worldToChunk(x: number, z: number): [number, number, number, number] {
		const chunkX = Math.floor(x / Chunk.Size);
		const chunkZ = Math.floor(z / Chunk.Size);
		const localX = x - chunkX * Chunk.Size;
		const localZ = z - chunkZ * Chunk.Size;
		return [chunkX, chunkZ, localX, localZ];
	}

	public getBlock(x: number, y: number, z: number): number {
		const [chunkX, chunkZ, localX, localZ] = this.worldToChunk(x, z);
		const chunk = this.getChunk(chunkX, chunkZ);
		return chunk?.getBlock(localX, y, localZ) ?? -1;
	}

	public setBlocks(blocks: { x: number; y: number; z: number; blockId: number }[]) {
		let flag = false;
		blocks.forEach(block => {
			if (this.setBlock(block.x, block.y, block.z, block.blockId)) {
				flag = true;
			}
		});
		if (flag) this.worldRenderer.updateChunks(blocks);
	}

	public setBlock(x: number, y: number, z: number, blockValue: number) {
		const [chunkX, chunkZ, localX, localZ] = this.worldToChunk(x, z);
		const chunk = this.getChunk(chunkX, chunkZ)!;
		const [oldValue, oldId, oldDef] = ChunkManager.getBlockInfo(x, y, z);
		if (oldValue === 0 && blockValue === 0) return false;
		const newBlockDef = BlockRegistry.Instance.decodeGetById(blockValue)!;
		const position = new Vector3(x, y, z);

		let needRender = false;
		let isRemove = false;

		if (oldDef?.render.type === "model" || newBlockDef?.render.type === "model") {
			const renderer = this.worldRenderer.getRenderer(chunk.Key)!;
			let key = `${x},${y},${z}`;
			if (blockValue !== 0) {
				renderer.addModelBlock(key, blockValue);
			} else {
				renderer.removeModelBlock(key);
				isRemove = true;
			}
		} else {
			if (blockValue === 0) isRemove = true;
			needRender = true;
		}
		if (isRemove) {
			MiniBlockBuilder.Instance.createMesh(position, oldValue, oldDef.render, oldDef.properties);
		}

		chunk.setBlock(localX, y, localZ, blockValue);
		return needRender;
	}

	public loadChunk(chunkX: number, chunkZ: number) {
		const key = this.chunkKey(chunkX, chunkZ);
		const chunk = this.chunks.get(key)!;
		this.execCallback(chunk, this.loadCallbacks);
	}

	public cancelScheduledUnload(chunk: Chunk) {
		this.chunksToUnload.delete(chunk.Key);
	}

	public scheduleUnloadChunk(chunk: Chunk) {
		this.chunksToUnload.set(chunk.Key, Date.now());

		// 如果已经有定时器在运行，就不需要创建新的
		if (this.unloadTimer !== null) return;

		this.unloadTimer = setInterval(() => {
			this.withUpdateLock(async () => {
				const now = Date.now();
				let count = 0;
				// 卸载所有标记时间超过阈值的区块
				for (const [key, timestamp] of this.chunksToUnload) {
					if (now - timestamp >= this.UNLOAD_DELAY) {
						const chunk = this.chunks.get(key);
						if (chunk) {
							count++;
							this.unloadChunk(chunk);
						}
						this.chunksToUnload.delete(key);
					}
				}
				if (count) console.log(`[VoxelEngine] 区块卸载:${count}`);
			});
		}, this.POLLING_TIME);
	}

	public unloadChunk(chunk: Chunk) {
		this.worldRenderer.unloadChunk(chunk.Key);
		this.chunks.delete(chunk.Key);
		this.execCallback(chunk, this.unloadCallbacks);
	}

	public forEachChunk(callback: (chunk: Chunk) => void) {
		for (const chunk of this.chunks.values()) {
			callback(chunk);
		}
	}

	public forEachBlockEntity(callback: (entity: BlockEntity, chunk: Chunk) => void) {
		for (const chunk of this.chunks.values()) {
			for (const key in chunk.blockEntities) {
				callback(chunk.blockEntities[key], chunk);
			}
		}
	}

	public getChunkByKey(key: string) {
		return this.chunks.get(key);
	}

	public getColumnHeight(x: number, z: number): number {
		const [chunkX, chunkZ, localX, localZ] = this.worldToChunk(x, z);
		const chunk = this.getChunk(chunkX, chunkZ);
		if (!chunk) return 0;

		for (let y = Chunk.Height - 1; y > 0; y--) {
			const blockId = chunk.getBlock(localX, y, localZ);
			if (blockId !== 0) {
				return y + 1;
			}
		}
		return 0;
	}

	dispose(): void {
		this._use = false;
		clearInterval(this.unloadTimer!);
		this.chunks.clear();
	}

	/**
	 * 计算区块中心点的世界坐标
	 */
	public getChunkCenter(chunkX: number, chunkZ: number): [number, number] {
		const centerX = chunkX * Chunk.Size + Chunk.Size / 2;
		const centerZ = chunkZ * Chunk.Size + Chunk.Size / 2;
		return [centerX, centerZ];
	}

	/**
	 * 计算区块到指定区块位置中心点的距离(横向)
	 */
	public getChunkDistance(
		chunkX: number,
		chunkZ: number,
		worldChunkX: number,
		worldChunkZ: number
	): number {
		const [chunkCenterX, chunkCenterZ] = this.getChunkCenter(chunkX, chunkZ);
		const [worldCenterX, worldCenterZ] = this.getChunkCenter(worldChunkX, worldChunkZ);

		const dx = chunkCenterX - worldCenterX;
		const dz = chunkCenterZ - worldCenterZ;
		return Math.max(Math.abs(dx), Math.abs(dz)) / Chunk.Size;
	}

	public getChunkCenterDistance(
		chunkX: number,
		chunkZ: number,
		worldChunkX: number,
		worldChunkZ: number
	): number {
		const [chunkCenterX, chunkCenterZ] = this.getChunkCenter(chunkX, chunkZ);
		const [worldCenterX, worldCenterZ] = this.getChunkCenter(worldChunkX, worldChunkZ);

		const dx = chunkCenterX - worldCenterX;
		const dz = chunkCenterZ - worldCenterZ;
		return Math.sqrt(dx * dx + dz * dz) / Chunk.Size;
	}

	public getChunkApproxDistance(
		chunkX: number,
		chunkZ: number,
		worldChunkX: number,
		worldChunkZ: number
	): number {
		// 中心点计算
		const [chunkCenterX, chunkCenterZ] = this.getChunkCenter(chunkX, chunkZ);
		const [worldCenterX, worldCenterZ] = this.getChunkCenter(worldChunkX, worldChunkZ);

		// dx, dz 是“中心差”
		const dx = chunkCenterX - worldCenterX;
		const dz = chunkCenterZ - worldCenterZ;

		// 减去半边长（以 Chunk 为半径）
		const half = Chunk.Size / 2;

		// 计算实际离玩家最短距离
		const nx = Math.max(0, Math.abs(dx) - half);
		const nz = Math.max(0, Math.abs(dz) - half);
		return Math.sqrt(nx * nx + nz * nz) / Chunk.Size;
	}

	private getSortedChunks(chunks: Chunk[], chunkX: number, chunkZ: number): Chunk[] {
		return chunks.sort((a, b) => {
			const distA = this.getChunkApproxDistance(a.position.x, a.position.z, chunkX, chunkZ);
			const distB = this.getChunkApproxDistance(b.position.x, b.position.z, chunkX, chunkZ);
			return distA - distB;
		});
	}

	private execUpdated(progress: number) {
		this.updatedCallbacks.forEach(callback => callback(progress));
	}

	// 异步锁
	private async withUpdateLock<T>(callback: () => Promise<T>): Promise<T | undefined> {
		if (this.isUpdating) return;
		this.isUpdating = true;
		try {
			return await callback();
		} finally {
			this.isUpdating = false;
		}
	}

	private updateChunkEdges() {
		Array.from(this.chunks.values()).forEach((chunk: Chunk) => {
			const { x, z } = chunk.position;
			chunk.edges.clear();

			for (const { dx, dz, edge } of EdgeConfigs) {
				const neighbor = this.getChunk(x + dx, z + dz);
				if (!neighbor) chunk.edges.add(edge);
			}
		});
	}

	private execCallback(chunk: Chunk, callbacks: Function[]) {
		for (const callback of callbacks) {
			callback(chunk);
		}
	}

	private chunkKey(x: number, z: number): string {
		return `${x},${z}`;
	}
}
