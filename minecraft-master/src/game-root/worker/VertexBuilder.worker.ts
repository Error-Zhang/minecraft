import * as Comlink from "comlink";
import { IBlockReflect } from "@/ui-root/api/interface.ts";
import { IVertexBuilder } from "@/game-root/worker/interface.ts";
import { Chunk } from "@engine/chunk/Chunk.ts";
import { BlockDefinition } from "@engine/types/block.type.ts";
import { BlockCoder } from "@/game-root/block-definitions/BlockCoder.ts";
import { MeshBuilderContext } from "@engine/types/mesh.type.ts";
import { ChunkMeshBuilder } from "@engine/renderer/ChunkMeshBuilder.ts";
import { getCombinedBlocks } from "@/game-root/block-definitions/blocks.ts";
import { VoxelEngine } from "@engine/core/VoxelEngine.ts";
import { ChunkData } from "@engine/types/chunk.type.ts";
import { useWorldStore } from "@/store";
import { WorldStore } from "@/store/interface.ts";

class VertexBuilder implements IVertexBuilder {
	private _chunks: Map<string, Chunk> = new Map();
	private _blocks: BlockDefinition<any>[] = [];

	constructor(
		public ChunkSize: number,
		public ChunkHeight: number
	) {
		Chunk.Height = ChunkHeight;
		Chunk.Size = ChunkSize;
	}

	public addBlocks(blockTypes: IBlockReflect) {
		const blockRegistry = VoxelEngine.registerBlocks(
			getCombinedBlocks(blockTypes),
			BlockCoder.extractId.bind(BlockCoder)
		);
		this._blocks = blockRegistry.blocks;
	}

	public removeChunk(key: string) {
		this._chunks.delete(key);
	}

	public setBlock(x: number, y: number, z: number, blockId: number) {
		const [chunkX, chunkZ, localX, localZ] = this.worldToChunk(x, z);
		const chunk = this._chunks.get(`${chunkX},${chunkZ}`);
		chunk?.setBlock(localX, y, localZ, blockId);
	}

	public addChunks(chunkDatas: ChunkData[]) {
		const chunks = chunkDatas.map(chunkData => Chunk.fromJSON(chunkData));
		chunks.forEach((chunk: Chunk) => {
			this._chunks.set(chunk.Key, chunk);
		});
	}

	public async buildMesh(
		chunkPos: { x: number; z: number },
		edges: Set<number>,
		filter: Set<string>
	) {
		const context: MeshBuilderContext = {
			mergeGroups: new Map(),
			renderedBlocks: new Set(),
			modelBlocks: new Map(),
			chunkPos,
			filter,
			edges,
			width: this.ChunkSize,
			height: this.ChunkHeight,
			getBlockAt: this.getBlockInfo.bind(this),
		};
		const ret = ChunkMeshBuilder.build(context);
		delete (ret as Partial<MeshBuilderContext>).getBlockAt;
		return ret;
	}

	public copyWorldStore(state: WorldStore) {
		useWorldStore.setState(state);
	}

	private worldToChunk(x: number, z: number): [number, number, number, number] {
		const chunkX = Math.floor(x / this.ChunkSize);
		const chunkZ = Math.floor(z / this.ChunkSize);
		const localX = x - chunkX * this.ChunkSize;
		const localZ = z - chunkZ * this.ChunkSize;
		return [chunkX, chunkZ, localX, localZ];
	}

	private getBlockInfo(x: number, y: number, z: number) {
		const [chunkX, chunkZ, localX, localZ] = this.worldToChunk(x, z);
		const chunk = this._chunks.get(`${chunkX},${chunkZ}`);
		const blockValue = chunk?.getBlock(localX, y, localZ) ?? -1;
		const blockId = BlockCoder.extractId(blockValue);
		const blockDef = this._blocks[blockId];
		const envValue = chunk?.getEnvironment(localX, localZ) ?? -1;
		return [blockValue, blockId, blockDef, envValue] as const;
	}
}

Comlink.expose(VertexBuilder);
