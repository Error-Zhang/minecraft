import { ChunkRenderer } from "@engine/renderer/ChunkRenderer.ts";
import { ChunkManager } from "../chunk/ChunkManager";
import { Scene } from "@babylonjs/core";
import { Position } from "@engine/types/chunk.type.ts";
import { Chunk } from "../chunk/Chunk";
import { SingleClass } from "@engine/core/Singleton.ts";
import { MeshBuilderContext } from "@engine/types/mesh.type.ts";
import { ChunkMeshBuilder } from "@engine/renderer/ChunkMeshBuilder.ts";

export type MeshBuilderFun = (
	chunkPos: { x: number; z: number },
	edges: Set<number>,
	filter: Set<string>
) => Promise<MeshBuilderContext>;

const buildMeshByMainThread = async (
	chunkPos: { x: number; z: number },
	edges: Set<number>,
	filter: Set<string>
) => {
	const context: MeshBuilderContext = {
		mergeGroups: new Map(),
		renderedBlocks: new Set(),
		modelBlocks: new Map(),
		chunkPos,
		filter,
		edges,
		width: Chunk.Size,
		height: Chunk.Height,
		getBlockAt: ChunkManager.getBlockInfo.bind(ChunkManager),
	};
	return ChunkMeshBuilder.build(context);
};

export class WorldRenderer extends SingleClass {
	public buildMesh: MeshBuilderFun;
	private renderers: Map<string, ChunkRenderer> = new Map();

	constructor(private scene: Scene) {
		super();
		this.buildMesh = buildMeshByMainThread;
	}

	public static get Instance(): WorldRenderer {
		return this.getInstance();
	}

	public registerBuildMeshFun(buildMesh: MeshBuilderFun) {
		this.buildMesh = buildMesh;
	}

	/**
	 * 更新区块中某一部分时调用
	 * @param positions
	 */
	public async updateChunks(positions: Position[] | Position) {
		const allAffectedChunks = new Map<string, Position[]>();
		const chunkSize = Chunk.Size;

		for (const pos of Array.isArray(positions) ? positions : [positions]) {
			const cx = Math.floor(pos.x / chunkSize);
			const cz = Math.floor(pos.z / chunkSize);
			const key = `${cx},${cz}`;

			if (!allAffectedChunks.has(key)) allAffectedChunks.set(key, []);
			allAffectedChunks.get(key)!.push(pos);
			// 只有破坏地形才会影响其他区块
			if (ChunkManager.getBlockAt(pos.x, pos.y, pos.z) !== 0) continue;

			const localX = ((pos.x % chunkSize) + chunkSize) % chunkSize;
			const localZ = ((pos.z % chunkSize) + chunkSize) % chunkSize;

			const isAtMinX = localX === 0;
			const isAtMaxX = localX === chunkSize - 1;
			const isAtMinZ = localZ === 0;
			const isAtMaxZ = localZ === chunkSize - 1;

			// 向相邻区块添加影响点
			if (isAtMinX) {
				const k = `${cx - 1},${cz}`;
				if (!allAffectedChunks.has(k)) allAffectedChunks.set(k, []);
				allAffectedChunks.get(k)!.push(pos);
			}
			if (isAtMaxX) {
				const k = `${cx + 1},${cz}`;
				if (!allAffectedChunks.has(k)) allAffectedChunks.set(k, []);
				allAffectedChunks.get(k)!.push(pos);
			}
			if (isAtMinZ) {
				const k = `${cx},${cz - 1}`;
				if (!allAffectedChunks.has(k)) allAffectedChunks.set(k, []);
				allAffectedChunks.get(k)!.push(pos);
			}
			if (isAtMaxZ) {
				const k = `${cx},${cz + 1}`;
				if (!allAffectedChunks.has(k)) allAffectedChunks.set(k, []);
				allAffectedChunks.get(k)!.push(pos);
			}
		}

		// 遍历所有受影响区块并触发更新
		for (const [key, worldPositions] of allAffectedChunks.entries()) {
			const chunk = ChunkManager.Instance.getChunkByKey(key);
			if (!chunk || !chunk.isVisible) continue;

			const renderer = this.renderers.get(key);
			if (!renderer) continue;
			await renderer.update(worldPositions);
		}
	}

	public getRenderer(key: string) {
		return this.renderers.get(key);
	}

	public setChunkEnabled(chunk: Chunk) {
		const renderer = this.getRenderer(chunk.Key);
		renderer?.setEnabled(chunk.isVisible);
	}

	public async rebuildChunk(chunk: Chunk) {
		const renderer = this.renderers.get(chunk.Key)!;
		await renderer.buildChunk();
	}

	public async buildChunk(chunk: Chunk) {
		if (chunk.isVisible) {
			if (!this.renderers.has(chunk.Key)) {
				await this.createChunkRenderer(chunk);
			} else if (chunk.edges.size) {
				await this.rebuildChunk(chunk);
			}
		}
		this.setChunkEnabled(chunk);
	}

	public unloadChunk(key: string) {
		const renderer = this.renderers.get(key);
		renderer?.dispose();
		this.renderers.delete(key);
	}

	public dispose() {
		for (const renderer of this.renderers.values()) {
			renderer.dispose();
		}
		this.renderers.clear();
	}

	private async createChunkRenderer(chunk: Chunk) {
		const renderer = new ChunkRenderer(this.scene, chunk);
		this.renderers.set(chunk.Key, renderer);
		await renderer.buildChunk();
		return renderer;
	}
}
