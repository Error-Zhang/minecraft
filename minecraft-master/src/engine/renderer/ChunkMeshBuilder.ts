import { VertexData } from "@babylonjs/core";
import { MeshBuilderContext, MeshData } from "../types/mesh.type.ts";
import { ChunkBlockProcessor } from "./ChunkBlockProcessor.ts";

export class ChunkMeshBuilder {
	public static build(context: MeshBuilderContext): MeshBuilderContext {
		if (context.filter.size) {
			ChunkBlockProcessor.processChunkEdges(context);
			ChunkBlockProcessor.processFilteredBlocks(context);
		} else {
			ChunkBlockProcessor.processAllBlocks(context);
		}

		return context;
	}

	public static createMeshGroups(mergeGroups: Map<string, MeshData>): Record<string, VertexData> {
		const meshGroups: Record<string, VertexData> = {};
		for (const [matKey, data] of mergeGroups.entries()) {
			if (data.positions.length) {
				const vd = new VertexData();
				vd.positions = data.positions;
				vd.indices = data.indices;
				vd.uvs = data.uvs;
				vd.normals = data.normals;
				vd.colors = data.colors;
				meshGroups[matKey] = vd;
			}
		}
		return meshGroups;
	}
}
