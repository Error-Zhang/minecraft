import {
	Animation,
	Material,
	Mesh,
	PBRMaterial,
	PhysicsShapeMesh,
	Quaternion,
	Scene,
	StandardMaterial,
	TransformNode,
	Vector3,
	VertexData,
} from "@babylonjs/core";
import { Chunk } from "../chunk/Chunk";
import { BlockRegistry } from "../block/BlockRegistry";
import { BlockMaterialManager } from "../renderer/BlockMaterialManager.ts";
import { ModelRender } from "../types/block.type.ts";
import { Position } from "../types/chunk.type.ts";
import { Environment } from "../environment/Environment.ts";
import { FaceDirectionOffset } from "./Constant.ts";
import { WorldRenderer } from "@engine/renderer/WorldRenderer.ts";
import { ChunkMeshBuilder } from "@engine/renderer/ChunkMeshBuilder.ts";
import { PhysicsBody, PhysicsMotionType, PhysicsShapeBox } from "@babylonjs/core/Physics/v2";
import ModelBlockManager from "@engine/renderer/ModelBlockManager.ts";

export class ChunkRenderer {
	private root: TransformNode;
	private modelInstances: Map<string, TransformNode> = new Map();
	private renderedBlocks: Set<string> = new Set();
	private _isEnabled: boolean = true;

	constructor(
		public scene: Scene,
		public chunk: Chunk
	) {
		this.root = this.createRoot();
	}

	// 传入绝对坐标
	public async update(positions: Position[]) {
		const toUpdate = this.renderedBlocks;

		for (const pos of positions) {
			toUpdate.add(`${pos.x},${pos.y},${pos.z}`);
			// 这里必须验证是否在当前区块内，否则会出现多出渲染的面在边界处
			for (const [dx, dy, dz] of FaceDirectionOffset) {
				if (Chunk.isInBounds(this.chunk, pos.x + dx, pos.y + dy, pos.z + dz)) {
					const nk = `${pos.x + dx},${pos.y + dy},${pos.z + dz}`;
					toUpdate.add(nk);
				}
			}
		}
		await this.buildChunk(toUpdate);
	}

	public async buildChunk(filter: Set<string> = this.renderedBlocks) {
		const { mergeGroups, modelBlocks, renderedBlocks } = await WorldRenderer.Instance.buildMesh(
			this.chunk.position,
			this.chunk.edges,
			filter
		);
		this.renderedBlocks = renderedBlocks;

		const meshGroups = ChunkMeshBuilder.createMeshGroups(mergeGroups);

		// 双缓冲处理：新 root 替代旧 root，避免空白闪烁
		const oldRoot = this.root;
		const newRoot = this.createRoot();
		this.root = newRoot;

		for (const matKey in meshGroups) {
			const vertexData = meshGroups[matKey];
			const material = this.cloneMaterialForMesh(
				BlockMaterialManager.Instance.getMaterialByKey(matKey)
			);
			const mesh = this.createMesh(matKey, vertexData, material, newRoot);
			if (!filter.size) {
				this.fadeAlpha(material, true);
			}
		}

		await this.buildModelBlocks(modelBlocks);

		oldRoot.getChildren().forEach(child => {
			Environment.Instance.shadowGenerator?.removeShadowCaster(child as Mesh);
		});
		oldRoot.getChildMeshes().forEach(child => {
			if (child.material?.alpha !== 1) {
				child.dispose();
			}
		});
		requestAnimationFrame(() => {
			oldRoot.dispose();
		});
	}

	// bugfix:如果你在某天发现本应该被销毁的网格回到了世界原点，那一定是dispose出了问题
	public dispose() {
		this.modelInstances.forEach(modelInstance => {
			modelInstance.dispose();
		});
		this.modelInstances.clear();
		this.root.getChildren().forEach(child => {
			Environment.Instance.shadowGenerator?.removeShadowCaster(child as Mesh);
		});
		this.root.dispose();
	}

	public setEnabled(enabled: boolean) {
		if (this._isEnabled === enabled) return;
		// root 下所有普通块 mesh 淡入淡出
		this.root.getChildMeshes().forEach(mesh => {
			let alpha = mesh.material!.alpha;
			mesh.setEnabled(true);
			this.fadeAlpha(mesh.material!, enabled, () => {
				mesh.setEnabled(enabled);
				mesh.material!.alpha = alpha;
			});
		});

		// 模型块淡入淡出
		this.modelInstances.forEach(modelInstance => {
			modelInstance.getChildMeshes().forEach(child => {
				let alpha = child.material!.alpha;
				child.setEnabled(true);
				this.fadeAlpha(child.material!, enabled, () => {
					child.setEnabled(enabled);
					child.material!.alpha = alpha;
				});
			});
		});
		this._isEnabled = enabled;
	}

	public async addModelBlock(posKey: string, blockId: number) {
		const [x, y, z] = posKey.split(",").map(Number);
		const def = BlockRegistry.Instance.getById(blockId)!;
		const render = <ModelRender>def.render;
		const mat = BlockMaterialManager.Instance.getMaterialByKey(render.matKey);
		const model = await render.loadModel(
			ModelBlockManager.Instance,
			new Vector3(x, y, z),
			mat.clone(mat.name)!,
			{
				attachCollider: true,
			}
		);
		model.metadata = { blockId };
		model.getChildMeshes().forEach(child => {
			Environment.Instance.shadowGenerator?.addShadowCaster(child);
		});
		const body = new PhysicsBody(model, PhysicsMotionType.STATIC, false, this.scene);
		const shape = new PhysicsShapeBox(
			render.size.scale(0.5),
			Quaternion.Identity(),
			render.size,
			this.scene
		);
		body.shape = shape;

		this.modelInstances.set(posKey, model);
		this.renderedBlocks.add(posKey);
	}

	public removeModelBlock(posKey: string) {
		this.modelInstances.get(posKey)?.dispose();
		this.modelInstances.delete(posKey);
	}

	private fadeAlpha(
		material: Material,
		fadeIn: boolean,
		onAnimationEnd?: () => void,
		duration: number = 500
	) {
		if ((material as StandardMaterial).alphaCutOff) {
			onAnimationEnd?.();
			return;
		}
		const targetAlpha = fadeIn ? material.alpha : 0;
		const currentAlpha = fadeIn ? 0 : material.alpha;
		material.alpha = currentAlpha;

		const anim = new Animation(
			"fadeAlpha",
			"alpha",
			60,
			Animation.ANIMATIONTYPE_FLOAT,
			Animation.ANIMATIONLOOPMODE_CONSTANT
		);

		const keys = [
			{ frame: 0, value: currentAlpha },
			{ frame: (60 * duration) / 1000, value: targetAlpha },
		];
		anim.setKeys(keys);
		material.animations = [anim];

		this.scene.beginAnimation(material, 0, (60 * duration) / 1000, false, 1, onAnimationEnd);
	}

	private createRoot() {
		const newRoot = new TransformNode(
			`chunk-${this.chunk.position.x},${this.chunk.position.z}`,
			this.scene
		);
		newRoot.position.set(this.chunk.position.x * Chunk.Size, 0, this.chunk.position.z * Chunk.Size);
		return newRoot;
	}

	private cloneMaterialForMesh(baseMaterial: Material): Material {
		if (baseMaterial instanceof StandardMaterial || baseMaterial instanceof PBRMaterial) {
			return baseMaterial.clone(baseMaterial.name + "_clone")!;
		}
		return baseMaterial;
	}

	private createMesh(
		name: string,
		vertexData: VertexData,
		material: Material,
		root: TransformNode
	): Mesh {
		const mesh = new Mesh(name, this.scene);
		vertexData.applyToMesh(mesh);
		mesh.material = material;

		// 获取材质中的网格属性配置
		const meshProperties = (material as any).metadata?.meshProperties ?? {};

		// 设置默认值
		mesh.isPickable = meshProperties.isPickable ?? true;
		mesh.checkCollisions = meshProperties.checkCollisions ?? true;
		mesh.receiveShadows = meshProperties.receiveShadows ?? true;
		mesh.alphaIndex = meshProperties.alphaIndex ?? 0;

		// 添加物理系统
		if (mesh.checkCollisions) {
			const body = new PhysicsBody(mesh, PhysicsMotionType.STATIC, false, this.scene);
			body.shape = new PhysicsShapeMesh(mesh, this.scene);
		}

		// 分租渲染确保在环境渲染之后，否则会被影响
		mesh.renderingGroupId = 1;
		if (material.alpha === 1) {
			Environment.Instance.shadowGenerator?.addShadowCaster(mesh);
		}

		mesh.setParent(root);
		return mesh;
	}

	private async buildModelBlocks(modelBlocks: Map<string, number>) {
		// 1. 清理旧模型（位置不存在或 ID 已变）
		for (const [key, instance] of this.modelInstances.entries()) {
			const newId = modelBlocks.get(key);
			const oldId = instance.metadata.blockId as number;

			if (!newId || oldId !== newId) {
				instance.dispose();
				this.modelInstances.delete(key);
			}
		}

		// 2. 添加或更新模型
		for (const [posKey, blockId] of modelBlocks.entries()) {
			if (!this.modelInstances.has(posKey)) {
				this.addModelBlock(posKey, blockId);
			}
		}
	}
}
