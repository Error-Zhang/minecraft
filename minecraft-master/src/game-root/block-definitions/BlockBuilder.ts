import { AbstractMesh, Vector3, Vector4 } from "@babylonjs/core";
import BlockType from "./BlockType.ts";
import {
	BlockBehavior,
	BlockDefinition,
	BlockProperties,
	CrossRender,
	CubeRender,
	MeshProperties,
	ModelRender,
	RenderComponent,
	RenderMaterial,
	TransparencyType,
} from "@engine/types/block.type.ts";
import ModelBlockManager from "@engine/block/ModelBlockManager.ts";
import { blocksUvTable } from "@/game-root/block-definitions/TextureAtlas.ts";
import { BlockMaterialManager } from "@engine/renderer/BlockMaterialManager.ts";
import { TAG_GETTERS } from "./BlockTags.ts";

type CubeGetters = Pick<CubeRender, "getUv" | "getColor" | "getRotation">;
type CrossGetters = Pick<CrossRender, "getStage" | "getColor">;

// 工具函数
function createCubeRender(
	uvs: Vector4[],
	transparencyType: CubeRender["transparencyType"],
	material: RenderMaterial,
	getters?: CubeGetters
): CubeRender {
	return {
		type: "cube",
		uvs,
		transparencyType,
		material,
		...getters,
	};
}

function createCrossRender(
	uvs: Vector4[],
	material: RenderMaterial,
	getters?: CrossGetters
): CrossRender {
	return {
		type: "cross",
		uvs,
		material,
		...getters,
	};
}

function calculateModelOffset(
	currentCenter: Vector3,
	scale: number = 1,
	targetCenter: Vector3 = Vector3.Zero()
): Vector3 {
	// 计算缩放后的当前中心点
	const scaledCurrentCenter = new Vector3(
		currentCenter.x * scale,
		currentCenter.y * scale,
		currentCenter.z * scale
	);

	// 计算需要的偏移量
	return new Vector3(
		scaledCurrentCenter.x - targetCenter.x,
		scaledCurrentCenter.y - targetCenter.y,
		scaledCurrentCenter.z - targetCenter.z
	);
}

function createModelRender(
	path: string,
	matKey: string,
	uvs?: Vector4[],
	setMesh?: (mesh: AbstractMesh) => void,
	offset: Vector3 = Vector3.Zero(),
	size: Vector3 = Vector3.One(),
	miniBlockScale: number = 0.25
): ModelRender {
	return {
		type: "model",
		uvs,
		matKey,
		size,
		miniBlockScale,
		loadModel: async (scene, position, material, options) => {
			const setMesh2 = (mesh: AbstractMesh) => {
				mesh.position.addInPlace(offset);
				mesh.material = material;
				mesh.isPickable = false;
				setMesh?.(mesh);
			};
			const model = await ModelBlockManager.loadModel(path, scene, setMesh2, options);
			model.position = position;
			model.scaling.scaleInPlace(options?.scale || 1);
			options?.attachCollider && ModelBlockManager.attachCollider(scene, model, size);
			return model;
		},
	};
}

export interface BlockMetaData {
	displayName: string;
	maxStackCount: number;

	[key: string]: any; // 允许任意属性
}

type RunTimeBlockDefine = Omit<BlockDefinition<BlockMetaData>, "blockType"> & {
	blockType: BlockType;
	options: {
		properties?: Partial<BlockProperties>;
		materialOptions?: RenderMaterial;
	};
	cubeGetters?: Pick<CubeRender, "getUv" | "getColor" | "getRotation">;
	crossGetters?: Pick<CrossRender, "getStage" | "getColor">;
};

// 通用方块构造器
export class BlockBuilder {
	private block: RunTimeBlockDefine;

	private defaultModelOffset = new Vector3(0.5, 0, 0.5);

	constructor(blockType: BlockType, displayName: string, id?: number) {
		this.block = {
			id,
			blockType,
			metaData: { displayName, maxStackCount: 40 },
			options: {},
			tags: [],
			render: {} as RenderComponent,
		};
	}

	withBehavior(behavior: BlockBehavior): BlockBuilder {
		this.block.behavior = behavior;
		return this;
	}

	withMetaData(
		metaData: Partial<{ displayName: string; maxStackCount: number } & Record<string, any>>
	) {
		this.block.metaData = { ...this.block.metaData, ...metaData };
		return this;
	}

	withTags(...tags: string[]) {
		this.block.tags = [...new Set([...this.block.tags, ...tags])];

		// 应用标签对应的 getter
		for (const tag of tags) {
			const getters = TAG_GETTERS[tag];
			if (getters) {
				if (getters.cube) {
					this.block.cubeGetters = {
						...this.block.cubeGetters,
						...getters.cube,
					};
				}
				if (getters.cross) {
					this.block.crossGetters = {
						...this.block.crossGetters,
						...getters.cross,
					};
				}
			}
		}
		return this;
	}

	withProperties(properties: Partial<BlockProperties>) {
		this.block.options.properties = properties;
		return this;
	}

	withMeshOptions(meshOptions: MeshProperties) {
		let materialOptions = this.block.options.materialOptions;
		if (materialOptions) {
			materialOptions.meshProperties = meshOptions;
		} else {
			this.block.options.materialOptions = {
				matKey: "",
				meshProperties: meshOptions,
			};
		}
		return this;
	}

	withMaterialOptions(materialOptions: Partial<RenderMaterial>, presetMatKey?: string) {
		if (presetMatKey) {
			materialOptions = Object.assign(
				BlockMaterialManager.getMaterialPreset(presetMatKey) || {},
				materialOptions
			);
		}
		if (materialOptions.matKey) {
			BlockMaterialManager.registerCustomMaterial(materialOptions.matKey, materialOptions);
		}
		this.block.options.materialOptions = {
			...materialOptions,
			matKey: materialOptions.matKey || presetMatKey || "",
		};
		return this;
	}

	withCubeGetters(getters: CubeGetters) {
		this.block.cubeGetters = getters;
		return this;
	}

	withCrossGetters(getters: CrossGetters) {
		this.block.crossGetters = getters;
		return this;
	}

	asCube(transparencyType: TransparencyType, materialKey: string = "") {
		if (!materialKey && !this.block.options.materialOptions?.matKey) {
			throw new Error("material key missing");
		}
		this.block.render = createCubeRender(
			blocksUvTable[this.block.blockType].faceUvs!,
			transparencyType,
			{
				...this.block.options.materialOptions,
				matKey: this.block.options.materialOptions?.matKey || materialKey,
			},
			this.block.cubeGetters
		);
		return this;
	}

	asCross() {
		if (!blocksUvTable[this.block.blockType].stageUvs) {
			throw new Error("stageUvs missing");
		}
		this.block.render = createCrossRender(
			blocksUvTable[this.block.blockType].stageUvs!,
			{
				...this.block.options.materialOptions,
				matKey:
					this.block.options.materialOptions?.matKey || BlockMaterialManager.PRESET_MATERIALS.CROSS,
			},
			this.block.crossGetters
		);
		return this;
	}

	asModel(
		path: string,
		setMesh?: (mesh: AbstractMesh) => void,
		offset?: Vector3,
		size?: Vector3,
		miniBlockScale?: number
	) {
		this.block.render = createModelRender(
			path,
			this.block.options.materialOptions?.matKey || BlockMaterialManager.PRESET_MATERIALS.MODEL,
			blocksUvTable[this.block.blockType].modelUvs,
			setMesh,
			offset ?? this.defaultModelOffset,
			size,
			miniBlockScale
		);
		return this;
	}

	build() {
		return {
			...this.block,
			blockType: BlockType[this.block.blockType],
		};
	}
}
