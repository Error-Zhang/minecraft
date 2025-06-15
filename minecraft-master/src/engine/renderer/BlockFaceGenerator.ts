import { CrossRender, CubeRender } from "../types/block.type.ts";
import { MeshBuilderContext, MeshData } from "../types/mesh.type.ts";
import { Color3, Color4 } from "@babylonjs/core";
import {
	AOOffsets,
	FaceDirectionOffset,
	FaceMap,
	FaceVertices,
	getCrossPositions,
} from "./Constant.ts";

export class BlockFaceGenerator {
	public static processBlock(key: string | [number, number, number], context: MeshBuilderContext) {
		const [wx, wy, wz] = typeof key === "string" ? (key as string).split(",").map(Number) : key;
		if (typeof key !== "string") key = `${wx},${wy},${wz}`;
		const [blockValue, blockId, block, envValue] = context.getBlockAt(wx, wy, wz);
		if (!block?.render) return;
		let target: MeshData = this.initData();

		if (block.render.type !== "model") {
			const matKey = block.render.material.matKey;
			if (!context.mergeGroups.has(matKey)) {
				context.mergeGroups.set(matKey, target);
			} else {
				target = context.mergeGroups.get(matKey)!;
			}
		}

		switch (block.render.type) {
			case "cube": {
				let render = block.render as CubeRender;
				let hasFace = false;
				for (let i = 0; i < 6; i++) {
					const normal = FaceDirectionOffset[i];
					const [_, neighborId, neighbor] = context.getBlockAt(
						wx + normal[0],
						wy + normal[1],
						wz + normal[2]
					);
					if (this.shouldRenderFace(blockId, neighborId, render, neighbor?.render)) {
						this.addFace(context, target, [wx, wy, wz], blockValue, envValue, render, i);
						hasFace = true;
					}
				}
				if (hasFace) context.renderedBlocks.add(key);
				break;
			}
			case "cross": {
				this.addCross(target, [wx, wy, wz], blockValue, block.render, envValue);
				context.renderedBlocks.add(key);
				break;
			}
			case "model": {
				context.modelBlocks.set(`${wx},${wy},${wz}`, blockId);
				context.renderedBlocks.add(key);
				break;
			}
		}
	}

	private static shouldRenderFace(
		currentId: number,
		neighborId: number,
		currentRender: CubeRender,
		neighborRender?: any
	): boolean {
		if (neighborId === -1) return false;
		if (neighborId === 0) return true;
		if (neighborId === currentId) return false;
		if (neighborRender?.type !== "cube") return true;
		switch (currentRender.transparencyType) {
			case "opaque":
				return neighborRender.transparencyType !== "opaque";
			case "cutout":
				return neighborRender.transparencyType === "transparent";
			case "transparent":
				return false;
		}
		return true;
	}

	private static initData(): MeshData {
		return { positions: [], indices: [], uvs: [], normals: [], colors: [], indexOffset: 0 };
	}

	private static computeAO(
		context: MeshBuilderContext,
		x: number,
		y: number,
		z: number,
		cornerOffset: [number, number, number],
		side1Offset: [number, number, number],
		side2Offset: [number, number, number]
	): number {
		const [cornerX, cornerY, cornerZ] = [
			x + cornerOffset[0],
			y + cornerOffset[1],
			z + cornerOffset[2],
		];
		const [side1X, side1Y, side1Z] = [x + side1Offset[0], y + side1Offset[1], z + side1Offset[2]];
		const [side2X, side2Y, side2Z] = [x + side2Offset[0], y + side2Offset[1], z + side2Offset[2]];

		const isBlockSolid = (xx: number, yy: number, zz: number): boolean => {
			const [__, neighborId, neighbor] = context.getBlockAt(xx, yy, zz);
			return neighborId !== 0 && neighbor?.render?.type === "cube";
		};

		const side1 = isBlockSolid(side1X, side1Y, side1Z);
		const side2 = isBlockSolid(side2X, side2Y, side2Z);
		const corner = isBlockSolid(cornerX, cornerY, cornerZ);

		if (side1 && side2) return 0.5;
		return 1.0 - (side1 ? 0.2 : 0) - (side2 ? 0.2 : 0) - (corner ? 0.1 : 0);
	}

	private static addFace(
		context: MeshBuilderContext,
		data: MeshData,
		[x, y, z]: [number, number, number],
		blockValue: number,
		envValue: number,
		render: CubeRender,
		index: number
	) {
		const orientation = render.getRotation?.(blockValue, index) ?? 0;
		const mappedIndex = FaceMap[orientation]?.[index] ?? index;
		const uv = render.getUv?.(blockValue, mappedIndex) ?? render.uvs[mappedIndex];
		const surfaceColor = mappedIndex === 4 ? render.material.surfaceColor : undefined;
		let color =
			render.getColor?.(blockValue, mappedIndex, envValue) ??
			render.material.color ??
			new Color3(1, 1, 1);

		const vertices = FaceVertices[index];
		const normal = FaceDirectionOffset[index];
		const baseIndex = data.positions.length / 3;

		for (const [vx, vy, vz] of vertices) {
			data.positions.push(x + vx, y + vy, z + vz);
			data.normals.push(...normal);
		}

		data.indices.push(
			baseIndex,
			baseIndex + 2,
			baseIndex + 1,
			baseIndex,
			baseIndex + 3,
			baseIndex + 2
		);

		let uvOrder = [
			[0, 1],
			[1, 1],
			[1, 0],
			[0, 0],
		];

		if (orientation === 1 && (index === 2 || index === 3)) {
			uvOrder = [
				[1, 1],
				[1, 0],
				[0, 0],
				[0, 1],
			];
		} else if (orientation === 2 && (index === 4 || index === 5 || index === 0 || index === 1)) {
			uvOrder = [
				[0, 0],
				[0, 1],
				[1, 1],
				[1, 0],
			];
		}

		for (const [uRatio, vRatio] of uvOrder) {
			const u = uv.x + (uv.z - uv.x) * uRatio;
			const v = uv.y + (uv.w - uv.y) * vRatio;
			data.uvs.push(u, v);
		}
		const offsets = AOOffsets[index];
		if (surfaceColor && !render.getColor) color = surfaceColor;
		for (let i = 0; i < 4; i++) {
			const ao = this.computeAO(context, x, y, z, ...offsets[i]);
			const finalColor = new Color4(
				color.r * ao,
				color.g * ao,
				color.b * ao,
				(<Color4>color).a ?? 1
			);
			data.colors.push(finalColor.r, finalColor.g, finalColor.b, finalColor.a);
		}
	}

	private static addCross(
		data: MeshData,
		[x, y, z]: [number, number, number],
		blockValue: number,
		render: CrossRender,
		envValue: number
	) {
		const uv = render.uvs[render.getStage?.(blockValue) || 0];
		let color =
			render.getColor?.(blockValue, 0, envValue) ??
			render.material.surfaceColor ??
			render.material.color ??
			new Color3(1, 1, 1);
		const baseIndex = data.positions.length / 3;

		const positions = getCrossPositions(x, y, z);

		data.positions.push(...positions.flat());

		for (let i = 0; i < 8; i++) {
			data.normals.push(0, 1, 0); // 平均法线
		}

		data.indices.push(
			baseIndex,
			baseIndex + 1,
			baseIndex + 2,
			baseIndex,
			baseIndex + 2,
			baseIndex + 3,
			baseIndex + 4,
			baseIndex + 5,
			baseIndex + 6,
			baseIndex + 4,
			baseIndex + 6,
			baseIndex + 7
		);

		const uvOrder = [
			[0, 0],
			[1, 0],
			[1, 1],
			[0, 1],
		];

		for (let i = 0; i < 2; i++) {
			for (const [uRatio, vRatio] of uvOrder) {
				const u = uv.x + (uv.z - uv.x) * uRatio;
				const v = uv.y + (uv.w - uv.y) * vRatio;
				data.uvs.push(u, v);
			}
		}

		for (let i = 0; i < 8; i++) {
			data.colors.push(color.r, color.g, color.b, (<Color4>color).a ?? 1);
		}
	}
}
