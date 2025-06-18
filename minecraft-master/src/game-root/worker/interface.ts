import { IBlockReflect } from "@/ui-root/api/interface.ts";
import { MeshBuilderFun } from "@engine/renderer/WorldRenderer.ts";
import { ChunkData } from "@engine/types/chunk.type.ts";
import { WorldStore } from "@/store/interface.ts";

export interface IVertexBuilderConstructor {
	new (...args: any[]): IVertexBuilder;
}

export interface IVertexBuilder {
	addChunks: (chunksDatas: ChunkData[]) => void;
	addBlocks: (blockTypes: IBlockReflect) => void;
	buildMesh: MeshBuilderFun;
	removeChunk: (key: string) => void;
	setBlock: (x: number, y: number, z: number, blockId: number) => void;

	copyWorldStore(state: WorldStore): void;
}
