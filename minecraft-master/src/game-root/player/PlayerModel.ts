import {
	AbstractMesh,
	ImportMeshAsync,
	PBRMaterial,
	Quaternion,
	Scene,
	Texture,
	Vector3,
} from "@babylonjs/core";

export class PlayerModel {
	public root!: AbstractMesh;
	public parts: Record<
		"body" | "head" | "leftLeg" | "rightLeg" | "leftArm" | "rightArm",
		AbstractMesh
	> = {} as any;
	private walkTime = 0;
	private bodyYaw: number = 0;
	private isWalking = false;
	private targetAngles: Record<string, number> = {
		leftArm: 0,
		rightArm: 0,
		leftLeg: 0,
		rightLeg: 0,
	};
	private readonly TRANSITION_SPEED = 0.1; // 过渡速度

	constructor(private scene: Scene) {}

	public async loadModel(modelPath: string, skinUrl: string) {
		const { meshes } = await ImportMeshAsync(modelPath, this.scene);
		const root = meshes[0];

		meshes.slice(1).forEach(mesh => {
			mesh.setParent(root);
			mesh.isPickable = false;
			mesh.checkCollisions = true;
			mesh.renderingGroupId = 1;
			const texture = new Texture(skinUrl, this.scene);
			texture.vScale = -1;
			(<PBRMaterial>mesh.material).albedoTexture = texture;

			// 收集子部件（用名称识别）
			const name = mesh.name.toLowerCase();
			switch (name) {
				case "node2":
					this.parts.body = mesh;
					break;
				case "node6":
					this.parts.head = mesh;
					mesh.setPivotPoint(new Vector3(0, 1.5, 0));
					break;
				case "node10":
					this.parts.leftLeg = mesh;
					mesh.setPivotPoint(new Vector3(0, 0.7, 0));
					break;
				case "node8":
					this.parts.rightLeg = mesh;
					mesh.setPivotPoint(new Vector3(0, 0.7, 0));
					break;
				case "node4":
					this.parts.leftArm = mesh;
					mesh.setPivotPoint(new Vector3(0, 1.3, 0));
					break;
				case "node12":
					this.parts.rightArm = mesh;
					mesh.setPivotPoint(new Vector3(0, 1.3, 0));
					break;
			}
		});
		this.root = root;
		this.registerWalkingAnimation();
		return root;
	}

	public lookYawPitch(yaw: number, pitch: number) {
		// 头部旋转
		this.parts.head.rotationQuaternion = Quaternion.FromEulerAngles(-pitch, -yaw + this.bodyYaw, 0);

		// 如果头部与身体偏差过大（比如超过 45°），身体慢慢转过去
		const yawDiff = yaw - this.bodyYaw;
		if (Math.abs(yawDiff) > Math.PI / 4) {
			// 插值或直接调整身体方向
			this.bodyYaw += yawDiff * 0.1; // 0.1 是平滑系数
		}

		// 应用身体旋转
		this.root.rotationQuaternion = Quaternion.FromEulerAngles(0, this.bodyYaw, 0);
	}

	public registerWalkingAnimation() {
		this.scene.onBeforeRenderObservable.add(() => {
			this.onAnimationFrame();
		});
	}

	public startWalking() {
		this.isWalking = true;
	}

	public stopWalking() {
		this.isWalking = false;
		// 设置目标角度为0，让动画系统平滑过渡
		this.targetAngles = {
			leftArm: 0,
			rightArm: 0,
			leftLeg: 0,
			rightLeg: 0,
		};
	}

	// 动画回调函数
	public onAnimationFrame() {
		if (this.isWalking) {
			const targetAngle = Math.sin(this.walkTime) * 0.8;
			this.walkTime += 0.1;

			this.targetAngles = {
				leftArm: targetAngle,
				rightArm: -targetAngle,
				leftLeg: targetAngle,
				rightLeg: -targetAngle,
			};
		}

		// 平滑过渡到目标角度
		const updateLimb = (mesh: AbstractMesh, targetAngle: number) => {
			const currentAngle = mesh.rotationQuaternion ? mesh.rotationQuaternion.toEulerAngles().x : 0;
			const newAngle = currentAngle + (targetAngle - currentAngle) * this.TRANSITION_SPEED;
			mesh.rotationQuaternion = Quaternion.FromEulerAngles(newAngle, 0, 0);
		};

		updateLimb(this.parts.leftArm, this.targetAngles.leftArm);
		updateLimb(this.parts.rightArm, this.targetAngles.rightArm);
		updateLimb(this.parts.leftLeg, this.targetAngles.leftLeg);
		updateLimb(this.parts.rightLeg, this.targetAngles.rightLeg);
	}
}
