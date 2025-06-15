import { BlockDefinition, RenderMaterial } from "../types/block.type.ts";
import { VertexData } from "@babylonjs/core";

export interface MeshData {
	positions: number[];
	indices: number[];
	uvs: number[];
	normals: number[];
	colors: number[];
	indexOffset: number;
	material?: RenderMaterial;
}

export interface MeshBuilderContext {
	mergeGroups: Map<string, MeshData>;
	renderedBlocks: Set<string>;
	modelBlocks: Map<string, number>;
	chunkPos: { x: number; z: number };
	edges: Set<number>;
	filter: Set<string>;
	width: number;
	height: number;
	getBlockAt: (
		x: number,
		y: number,
		z: number
	) => readonly [number, number, BlockDefinition<any>, number];
}

export interface MeshBuilderResult {
	renderedBlocks: Set<string>;
	modelBlocks: Map<string, number>;
	meshGroups: Record<string, VertexData>;
}
