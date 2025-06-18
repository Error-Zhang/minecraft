import { Color3, Color4, Material, TransformNode, Vector3, Vector4 } from "@babylonjs/core";
import ModelBlockManager from "@engine/renderer/ModelBlockManager.ts";

export type Color = Color3 | Color4;

// 基础属性接口
export interface MeshProperties {
	isVisible?: boolean;
	isPickable?: boolean;
	checkCollisions?: boolean;
}

export interface BlockProperties {
	isBreakable?: boolean;
	hardness?: number;
	material?: RenderMaterial;
	mass?: number;
}

export enum TransparencyType {
	Opaque = "opaque",
	Cutout = "cutout",
	Transparent = "transparent",
}

// 基础渲染属性接口
interface BaseRender {
	material: RenderMaterial;
	uvs: Vector4[];
}

// 基础颜色获取接口
interface ColorGetter {
	getColor?: (value: number, face?: number, envValue?: number) => Color | undefined;
}

export interface RenderMaterial {
	matKey: string;
	textureKey?: string;
	color?: Color;
	surfaceColor?: Color;
	emissive?: Color3;
	specular?: Color3;
	roughness?: number;
	metallic?: number;
	alpha?: number;
	backFaceCulling?: boolean;
	meshProperties?: MeshProperties;
}

// 立方体渲染
export interface CubeRender extends BaseRender, ColorGetter {
	type: "cube";
	transparencyType: TransparencyType;
	getUv?: (value: number, face: number) => Vector4;
	getRotation?: (value: number, face: number) => 0 | 1 | 2;
}

// 十字渲染
export interface CrossRender extends BaseRender, ColorGetter {
	type: "cross";
	getStage?: (value: number) => number;
}

// 模型渲染
export interface ModelRender {
	type: "model";
	uvs?: Vector4[];
	matKey: string;
	size: Vector3;
	miniBlockScale: number;
	loadModel: (
		modelBlockManager: ModelBlockManager,
		position: Vector3,
		material: Material,
		options?: {
			scale?: number;
			attachCollider?: boolean;
		}
	) => Promise<TransformNode>;
}

export type RenderComponent = CubeRender | CrossRender | ModelRender;

// 基础行为接口
interface BaseBehavior {
	onTick?: () => void;
	onInteract?: () => void;
}

export interface BlockBehavior extends BaseBehavior {
	onPlace?: () => void;
	onDestroy?: () => void;
}

export interface BlockDefinition<TMeta extends Record<string, unknown>> {
	id?: number;
	blockType: string;
	tags: string[];
	properties?: Partial<BlockProperties>;
	render: RenderComponent;
	behavior?: BlockBehavior;
	createEntity?: () => BlockEntity;
	metaData: TMeta;
}

export interface BlockEntity {
	tick?(): void;

	serialize(): any;

	deserialize(data: any): void;
}
