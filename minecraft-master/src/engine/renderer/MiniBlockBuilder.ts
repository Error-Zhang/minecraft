import {
	Color4,
	Mesh,
	PhysicsShape,
	Quaternion,
	Scene,
	TransformNode,
	Vector3,
	Vector4,
	VertexData,
} from "@babylonjs/core";
import { PhysicsBody, PhysicsMotionType, PhysicsShapeBox } from "@babylonjs/core/Physics/v2";

import { FaceDirectionOffset, FaceVertices } from "./Constant.ts";
import { SingleClass } from "@engine/core/Singleton.ts";
import {
	BlockProperties,
	Color,
	CrossRender,
	CubeRender,
	ModelRender,
	RenderComponent,
} from "@engine/types/block.type.ts";
import { BlockMaterialManager } from "@engine/renderer/BlockMaterialManager.ts";
import { BlockRegistry } from "@engine/block/BlockRegistry.ts";
import ModelBlockManager from "@engine/renderer/ModelBlockManager.ts";

export class MiniBlockBuilder extends SingleClass {
	private offset: Vector3 = new Vector3(0.5, 0.5, 0.5);
	private shapes: Map<string, PhysicsShape> = new Map<string, PhysicsShape>();
	private blockMaterialManager: BlockMaterialManager;

	constructor(
		private scene: Scene,
		blockMaterialManager?: BlockMaterialManager,
		public usePhysics: boolean = true
	) {
		super();
		this.blockMaterialManager = blockMaterialManager ?? BlockMaterialManager.Instance;
	}

	public static get Instance(): MiniBlockBuilder {
		return this.getInstance();
	}

	public static buildCubeMeshData(
		blockValue: number,
		uvs: Vector4[],
		getColor: CubeRender["getColor"],
		scale = 0.25
	): VertexData {
		const positions: number[] = [];
		const normals: number[] = [];
		const indices: number[] = [];
		const uvsArray: number[] = [];
		const colors: number[] = [];

		for (let i = 0; i < 6; i++) {
			const face = FaceVertices[i];
			const normal = FaceDirectionOffset[i];
			const baseIndex = positions.length / 3;
			const uv = uvs[i] ?? uvs[0];

			for (const [vx, vy, vz] of face) {
				positions.push((vx - 0.5) * scale, (vy - 0.5) * scale, (vz - 0.5) * scale);
				normals.push(...normal);
			}

			indices.push(
				baseIndex,
				baseIndex + 2,
				baseIndex + 1,
				baseIndex,
				baseIndex + 3,
				baseIndex + 2
			);

			const uvOrder = [
				[0, 1],
				[1, 1],
				[1, 0],
				[0, 0],
			];

			for (const [uRatio, vRatio] of uvOrder) {
				const u = uv.x + (uv.z - uv.x) * uRatio;
				const v = uv.y + (uv.w - uv.y) * vRatio;
				uvsArray.push(u, v);
			}

			const finalColor = getColor?.(blockValue, i) ?? new Color4(1, 1, 1, 1);
			for (let j = 0; j < 4; j++) {
				colors.push(finalColor.r, finalColor.g, finalColor.b, (<Color4>finalColor).a ?? 1);
			}
		}

		const vd = new VertexData();
		vd.positions = positions;
		vd.normals = normals;
		vd.indices = indices;
		vd.uvs = uvsArray;
		vd.colors = colors;
		return vd;
	}

	public static buildPastaMeshData(
		uv: Vector4,
		color?: Color,
		scale = 0.25,
		thin = 0.02 // 厚度
	): VertexData {
		const halfW = 0.5 * scale;
		const halfH = 0.5 * scale;
		const halfT = 0.5 * thin;

		const positions: number[] = [];
		const indices: number[] = [];
		const normals: number[] = [];
		const uvs: number[] = [];
		const colors: number[] = [];

		const c = color ?? new Color4(1, 1, 1, 1);
		let indexOffset = 0;

		// 面数据结构：每个面包含 4 个顶点 + normal + 是否贴图
		const faces = [
			// 正面 (+Z)
			{
				verts: [
					[-halfW, -halfH, halfT],
					[halfW, -halfH, halfT],
					[halfW, halfH, halfT],
					[-halfW, halfH, halfT],
				],
				normal: [0, 0, 1],
				useUV: true,
			},
			// 背面 (-Z)
			{
				verts: [
					[halfW, -halfH, -halfT],
					[-halfW, -halfH, -halfT],
					[-halfW, halfH, -halfT],
					[halfW, halfH, -halfT],
				],
				normal: [0, 0, -1],
				useUV: true,
			},
			// 左侧 (-X)
			{
				verts: [
					[-halfW, -halfH, -halfT],
					[-halfW, -halfH, halfT],
					[-halfW, halfH, halfT],
					[-halfW, halfH, -halfT],
				],
				normal: [-1, 0, 0],
				useUV: false,
			},
			// 右侧 (+X)
			{
				verts: [
					[halfW, -halfH, halfT],
					[halfW, -halfH, -halfT],
					[halfW, halfH, -halfT],
					[halfW, halfH, halfT],
				],
				normal: [1, 0, 0],
				useUV: false,
			},
			// 上面 (+Y)
			{
				verts: [
					[-halfW, halfH, halfT],
					[halfW, halfH, halfT],
					[halfW, halfH, -halfT],
					[-halfW, halfH, -halfT],
				],
				normal: [0, 1, 0],
				useUV: false,
			},
			// 下面 (-Y)
			{
				verts: [
					[-halfW, -halfH, -halfT],
					[halfW, -halfH, -halfT],
					[halfW, -halfH, halfT],
					[-halfW, -halfH, halfT],
				],
				normal: [0, -1, 0],
				useUV: false,
			},
		];

		for (const face of faces) {
			const baseIndex = indexOffset;

			for (const [x, y, z] of face.verts) {
				positions.push(x, y, z);
				normals.push(...face.normal);
				if (face.useUV) {
					colors.push(c.r, c.g, c.b, (<Color4>c).a ?? 1);
				} else {
					colors.push(0, 0, 0, 1);
				}
			}

			if (face.useUV) {
				const uvOrder = [
					[0, 1],
					[1, 1],
					[1, 0],
					[0, 0],
				];
				for (const [uRatio, vRatio] of uvOrder) {
					const u = uv.x + (uv.z - uv.x) * uRatio;
					const v = uv.y + (uv.w - uv.y) * vRatio;
					uvs.push(u, v);
				}
			} else {
				// 给默认 UV，避免 shader 出错
				uvs.push(0, 0, 0, 0, 0, 0, 0, 0);
			}

			// 添加两个三角面
			indices.push(baseIndex, baseIndex + 1, baseIndex + 2);
			indices.push(baseIndex, baseIndex + 2, baseIndex + 3);

			indexOffset += 4;
		}

		const vd = new VertexData();
		vd.positions = positions;
		vd.normals = normals;
		vd.indices = indices;
		vd.uvs = uvs;
		vd.colors = colors;

		return vd;
	}

	public static buildCrossMeshData(uv: Vector4, color?: Color, scale = 0.25): VertexData {
		const halfW = 0.5 * scale;
		const halfH = 0.5 * scale;

		const c = color ?? new Color4(1, 1, 1, 1);

		const positions: number[] = [];
		const indices: number[] = [];
		const normals: number[] = [];
		const uvs: number[] = [];
		const colors: number[] = [];

		const addPlane = (ax: number, az: number, bx: number, bz: number) => {
			const base = positions.length / 3;

			// 四个顶点
			positions.push(ax, -halfH, az, bx, -halfH, bz, bx, halfH, bz, ax, halfH, az);

			// 法线都朝上
			for (let i = 0; i < 4; i++) normals.push(0, 1, 0);

			// UV
			const uvOrder = [
				[0, 1],
				[1, 1],
				[1, 0],
				[0, 0],
			];
			for (const [uRatio, vRatio] of uvOrder) {
				const u = uv.x + (uv.z - uv.x) * uRatio;
				const v = uv.y + (uv.w - uv.y) * vRatio;
				uvs.push(u, v);
			}

			// 颜色
			for (let i = 0; i < 4; i++) colors.push(c.r, c.g, c.b, (<Color4>c).a ?? 1);

			// 三角形索引
			indices.push(base, base + 1, base + 2, base, base + 2, base + 3);
		};

		// 添加两个交叉面
		addPlane(-halfW, 0, halfW, 0); // X轴平面
		addPlane(0, -halfW, 0, halfW); // Z轴平面

		const vd = new VertexData();
		vd.positions = positions;
		vd.normals = normals;
		vd.uvs = uvs;
		vd.indices = indices;
		vd.colors = colors;

		return vd;
	}

	public async createModelMesh(
		position: Vector3,
		blockValue: number,
		render: ModelRender,
		properties?: BlockProperties,
		scale?: number
	) {
		scale = scale ?? render.miniBlockScale;
		const model = await render.loadModel(
			ModelBlockManager.Instance,
			position,
			this.blockMaterialManager.getMaterialByKey(render.matKey),
			{ scale }
		);
		let size = render.size.scale(scale);
		let offset = this.offset.subtract(size.scale(0.5));
		model.position.addInPlace(offset);
		if (!this.usePhysics) return model;
		const body = this.applyHavokPhysics(
			model,
			this.getShape("model", size, render.size.scale(scale / 2))
		);
		this.setMassProperties(body, properties);
		return model;
	}

	public async createMesh(
		position: Vector3,
		blockValue: number,
		render: RenderComponent,
		properties?: BlockProperties,
		scale: number = 0.25
	) {
		switch (render.type) {
			case "cube":
				let cubeMesh = this.createCubeMesh(position, blockValue, render, properties, scale);
				cubeMesh.isPickable = false;
				return cubeMesh;
			case "cross":
				let crossMesh = this.createCrossMesh(position, blockValue, render, properties, scale);
				crossMesh.isPickable = false;
				return crossMesh;
			case "model":
				let model = await this.createModelMesh(position, blockValue, render, properties, scale);
				model.getChildMeshes().forEach(child => {
					child.isPickable = false;
				});
				return model;
		}
	}

	public createCubeMesh(
		position: Vector3,
		blockValue: number,
		render: CubeRender,
		properties?: BlockProperties,
		scale = 0.25
	): Mesh {
		const mesh = new Mesh("miniCube", this.scene);
		const data = MiniBlockBuilder.buildCubeMeshData(blockValue, render.uvs, render.getColor, scale);
		this.applyVertexData(mesh, data);

		mesh.material = this.blockMaterialManager.getMaterialByKey(render.material.matKey);
		mesh.renderingGroupId = 1;
		mesh.position.copyFrom(position.add(this.offset));
		if (!this.usePhysics) return mesh;
		const body = this.applyHavokPhysics(
			mesh,
			this.getShape("cube", new Vector3(scale, scale, scale))
		);
		this.setMassProperties(body, properties);
		return mesh;
	}

	public createCrossMesh(
		position: Vector3,
		blockValue: number,
		render: CrossRender,
		properties?: BlockProperties,
		scale = 0.25
	): Mesh {
		let uv =
			render.uvs[
				BlockRegistry.Instance.isCodeId(blockValue)
					? render.getStage?.(blockValue) || 0
					: render.uvs.length - 1
			];
		const mesh = new Mesh("miniCross", this.scene);
		const data = MiniBlockBuilder.buildCrossMeshData(uv, render.getColor?.(blockValue), scale);
		this.applyVertexData(mesh, data);
		mesh.position.copyFrom(position.add(this.offset));
		mesh.material = this.blockMaterialManager.getMaterialByKey(render.material.matKey);
		mesh.renderingGroupId = 1;
		if (!this.usePhysics) return mesh;
		const body = this.applyHavokPhysics(
			mesh,
			this.getShape("cross", new Vector3(scale, scale, scale * 0.1), new Vector3(0, -0.01, 0))
		);
		this.setMassProperties(body, properties);
		return mesh;
	}

	public dispose() {}

	private setMassProperties(body: PhysicsBody, properties?: BlockProperties) {
		body.setMassProperties({
			mass: properties?.mass || 1,
		});
	}

	private applyVertexData(mesh: Mesh, data: VertexData): void {
		const vertexData = new VertexData();
		Object.assign(vertexData, data);
		vertexData.applyToMesh(mesh);
	}

	private getShape(
		shapeType: "cube" | "cross" | "model",
		size: Vector3,
		center: Vector3 = Vector3.Zero()
	) {
		if (this.shapes.has(shapeType)) return this.shapes.get(shapeType)!;
		let phyShape = new PhysicsShapeBox(center, Quaternion.Identity(), size, this.scene);
		this.shapes.set(shapeType, phyShape);
		return phyShape;
	}

	private applyHavokPhysics(mesh: TransformNode, shapeBox: PhysicsShape) {
		const body = new PhysicsBody(mesh, PhysicsMotionType.DYNAMIC, false, this.scene);
		body.shape = shapeBox;
		// const physicsViewer = new PhysicsViewer();
		// physicsViewer.showBody(body);
		return body;
	}
}
