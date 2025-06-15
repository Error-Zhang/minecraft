import { MeshBuilderContext } from "../types/mesh.type.ts";
import { BlockFaceGenerator } from "./BlockFaceGenerator.ts";
import { EdgeConfigs } from "./Constant.ts";

export class ChunkBlockProcessor {
	public static processFilteredBlocks(context: MeshBuilderContext) {
		for (const key of context.filter) {
			if (!context.renderedBlocks.has(key)) {
				BlockFaceGenerator.processBlock(key, context);
			}
		}
	}

	public static processChunkEdges(context: MeshBuilderContext) {
		if (!context.edges.size) return;
		const position = context.chunkPos;
		const { width, height } = context;
		for (const { edge, getCoords } of EdgeConfigs) {
			if (!context.edges.has(edge)) continue;
			for (let i = 0; i < width; i++) {
				for (let y = 0; y < height; y++) {
					const [x, yCoord, z] = getCoords(i, y, context.width);
					const [wx, wy, wz] = [position.x * width + x, yCoord, position.z * width + z];
					if (!context.renderedBlocks.has(`${wx},${wy},${wz}`)) {
						BlockFaceGenerator.processBlock([wx, wy, wz], context);
					}
				}
			}
		}
	}

	public static processAllBlocks(context: MeshBuilderContext) {
		const position = context.chunkPos;
		const { width, height } = context;
		for (let z = 0; z < width; z++) {
			for (let x = 0; x < width; x++) {
				for (let y = 0; y < height; y++) {
					const [wx, wy, wz] = [position.x * width + x, y, position.z * width + z];
					BlockFaceGenerator.processBlock([wx, wy, wz], context);
				}
			}
		}
	}
}
