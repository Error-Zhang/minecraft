import { ImportMeshAsync, MeshBuilder, Scene, TransformNode, Vector3 } from "@babylonjs/core";
import { BlockMaterialManager } from "@engine/renderer/BlockMaterialManager.ts";

class ModelBlockManager {
	static meshes: Map<string, TransformNode> = new Map();

	static async loadModel(modelPath: string, scene: Scene, setMesh: Function, options: any) {
		let key = `${modelPath}-${JSON.stringify(options)}`;
		if (this.meshes.has(key)) return this.meshes.get(key)!.clone(key, null)!;

		const { meshes } = await ImportMeshAsync(modelPath, scene);
		const root = new TransformNode(`${key}_root`, scene);

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

	static attachCollider(scene: Scene, node: TransformNode, size: Vector3) {
		const collider = MeshBuilder.CreateBox(
			"collider",
			{ width: size.x, height: size.y, depth: size.z },
			scene
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
