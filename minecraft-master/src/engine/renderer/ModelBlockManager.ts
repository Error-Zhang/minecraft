import { ImportMeshAsync, MeshBuilder, Scene, TransformNode, Vector3 } from "@babylonjs/core";
import { BlockMaterialManager } from "@engine/renderer/BlockMaterialManager.ts";
import { SingleClass } from "@engine/core/Singleton.ts";

class ModelBlockManager extends SingleClass {
	meshes: Map<string, TransformNode> = new Map();

	constructor(public scene: Scene) {
		super();
	}

	static get Instance(): ModelBlockManager {
		return this.getInstance();
	}

	dispose(): void {
		this.meshes.clear();
	}

	async loadModel(modelPath: string, setMesh: Function, options: any) {
		let key = `${modelPath}-${JSON.stringify(options)}`;
		if (this.meshes.has(key)) return this.meshes.get(key)!.clone(key, null)!;

		const { meshes } = await ImportMeshAsync(modelPath, this.scene);
		const root = new TransformNode(`${key}_root`, this.scene);

		for (const mesh of meshes.filter(m => m.name !== "__root__")) {
			mesh.setParent(root);
			setMesh(mesh);
			mesh.renderingGroupId = 1;
		}
		// 防止坐标原点出现模型实体
		root.setEnabled(false);
		this.meshes.set(key, root);
		// 返回克隆体防止本体被摧毁后所有克隆体全部消失
		return root.clone(key, null)!;
	}

	attachCollider(node: TransformNode, size: Vector3) {
		const collider = MeshBuilder.CreateBox(
			"collider",
			{ width: size.x, height: size.y, depth: size.z },
			this.scene
		);

		const mat = BlockMaterialManager.Instance.getMaterialByKey(
			BlockMaterialManager.PRESET_MATERIALS.MODEL_COLLIDER
		);

		collider.material = mat;
		collider.checkCollisions = true;
		collider.isPickable = true;
		collider.visibility = 0;

		collider.setParent(node);
		collider.position = new Vector3(0.5, 0.5, 0.5);
	}
}

export default ModelBlockManager;
