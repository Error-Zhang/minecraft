import { Color3, MeshBuilder, Scene, StandardMaterial, Vector3 } from "@babylonjs/core";

export class DebugHelper {
	private scene: Scene;

	constructor(scene: Scene) {
		this.scene = scene;
	}

	// 可视化射线
	drawRay(origin: Vector3, direction: Vector3, length = 5, color = Color3.Red()) {
		const rayEnd = origin.add(direction.normalize().scale(length));
		const lines = MeshBuilder.CreateLines(
			"rayDebug",
			{
				points: [origin, rayEnd],
			},
			this.scene
		);
		lines.color = color;

		// 一定时间后销毁
		//setTimeout(() => lines.dispose(), 1000);
	}

	// 标记一个坐标点
	markPoint(pos: Vector3, color = Color3.Green(), size = 0.1) {
		const sphere = MeshBuilder.CreateSphere("marker", { diameter: size }, this.scene);
		sphere.position = pos.clone();

		const mat = new StandardMaterial("markerMat", this.scene);
		mat.emissiveColor = color;
		sphere.material = mat;

		//setTimeout(() => sphere.dispose(), 1000);
	}

	/**
	 * 生成xyz轴线
	 * @param size
	 * @param height
	 */
	createAxisHelper(size = 128, height: number = 3): void {
		const scene = this.scene;

		// X Axis - Red (双向)
		const axisX = MeshBuilder.CreateLines(
			"axisX",
			{
				points: [new Vector3(-size, height, 0), new Vector3(size, 0, 0)],
			},
			scene
		);
		axisX.color = Color3.Red();

		// Y Axis - Green (双向)
		const axisY = MeshBuilder.CreateLines(
			"axisY",
			{
				points: [new Vector3(0, -size, 0), new Vector3(0, size, 0)],
			},
			scene
		);
		axisY.color = Color3.Green();

		// Z Axis - Blue (双向)
		const axisZ = MeshBuilder.CreateLines(
			"axisZ",
			{
				points: [new Vector3(0, height, -size), new Vector3(0, 0, size)],
			},
			scene
		);
		axisZ.color = Color3.Blue();
	}
}
