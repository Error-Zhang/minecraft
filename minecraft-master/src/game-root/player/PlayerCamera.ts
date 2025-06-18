import { FreeCamera, Ray, Scene, Vector3 } from "@babylonjs/core";
import { PlayerInputSystem } from "@/game-root/player/PlayerInputSystem.ts";
import { ParabolaMotion } from "@/game-root/utils/ParabolaMotion.ts";
import { playerEvents } from "@/game-root/core/events.ts";

// 基础相机类
export abstract class BasePlayerCamera {
	protected readonly scene: Scene;
	protected readonly camera: FreeCamera;
	protected readonly inputSystem: PlayerInputSystem;
	protected moveValue = { x: 0, y: 0, z: 0 };

	protected moveSpeed: number = 0.05;
	private cameraState = {
		lastPosition: new Vector3(),
		lastYaw: 0,
		lastPitch: 0,
	};

	constructor(scene: Scene, canvas: HTMLCanvasElement) {
		this.scene = scene;
		this.camera = new FreeCamera("Camera", new Vector3(0, 0, 0), this.scene);
		this.camera.minZ = 0.1;
		this.camera.attachControl(canvas, true);
		this.inputSystem = PlayerInputSystem.Instance;

		scene.gravity = new Vector3(0, -9.81 / 60, 0);
		scene.collisionsEnabled = true;

		this.camera.ellipsoid = new Vector3(0.4, 0.9, 0.2); // 碰撞半径
		this.camera.ellipsoidOffset = new Vector3(0, 0, 0); // 默认在顶部，向下偏移

		this.initCamera();
		this.bindInput();
	}

	public get root() {
		return this.camera;
	}

	public get position(): Vector3 {
		return this.camera.position;
	}

	public dispose() {
		this.camera.detachControl();
		this.camera.dispose();
	}

	public getYawPitch() {
		const yaw = this.camera.rotation.y;
		const pitch = this.camera.rotation.x;

		return [yaw, pitch] as const;
	}

	public detectCameraChanges() {
		const currentPosition = this.camera.position;
		const [yaw, pitch] = this.getYawPitch();
		let value = 0.01;
		const moved = !currentPosition.equalsWithEpsilon(this.cameraState.lastPosition, value);
		const turned =
			Math.abs(yaw - this.cameraState.lastYaw) > value ||
			Math.abs(pitch - this.cameraState.lastPitch) > value;

		if (moved) this.cameraState.lastPosition.copyFrom(currentPosition);
		if (turned) {
			this.cameraState.lastYaw = yaw;
			this.cameraState.lastPitch = pitch;
		}

		if (moved || turned) {
			playerEvents.emit(
				"playerMoved",
				{ x: currentPosition.x, y: currentPosition.y, z: currentPosition.z },
				{
					yaw,
					pitch,
				}
			);
		}
	}

	public setPosition(x: number, y: number, z: number): void {
		this.camera.position.set(x, y, z);
		this.camera.setTarget(new Vector3(0, y, 0));
	}

	// 射线拾取选中的方块信息
	public getPickInfo(maxPlaceDistance: number) {
		const ray = this.camera!.getForwardRay();
		const pick = this.scene.pickWithRay(ray, mesh => {
			return mesh.isPickable;
		});

		if (pick?.hit && pick.pickedPoint && pick.distance <= maxPlaceDistance) {
			return pick;
		}

		return null;
	}

	public update(dt: number): void {
		this.camera.cameraDirection.addInPlace(
			new Vector3(this.moveValue.x, this.moveValue.y, this.moveValue.z)
		);
		// 重置移动值
		this.moveValue = { x: 0, y: 0, z: 0 };
		this.detectCameraChanges();
	}

	protected initCamera(): void {
		this.camera.inertia = 0.6;
		this.camera.speed = 0.6;
	}

	protected bindInput(): void {
		// 移动控制
		this.inputSystem.onActionUpdate("moveForward", () => this.moveFront());
		this.inputSystem.onActionUpdate("moveBackward", () => this.moveBack());
		this.inputSystem.onActionUpdate("moveLeft", () => this.moveLeft());
		this.inputSystem.onActionUpdate("moveRight", () => this.moveRight());
	}

	protected abstract getMoveSpeed(): number;

	protected moveFront(): void {
		this.moveByDirection(this.camera.getDirection(Vector3.Forward()));
	}

	protected moveBack(): void {
		this.moveByDirection(this.camera.getDirection(Vector3.Forward().scale(-1)));
	}

	protected moveLeft(): void {
		this.moveByDirection(this.camera.getDirection(Vector3.Right().scale(-1)));
	}

	protected moveRight(): void {
		this.moveByDirection(this.camera.getDirection(Vector3.Right()));
	}

	protected moveByDirection(direction: Vector3): void {
		const speedMultiplier = this.inputSystem.isActionActive("sprint") ? 1.25 : 1;
		const move = this.getMoveSpeed() * speedMultiplier;
		// 只保留水平方向的移动，忽略垂直分量
		direction.y = 0;
		const dir = direction.normalize().scale(move);
		this.moveValue.x += dir.x;
		this.moveValue.z += dir.z;
	}
}

// 生存模式相机
export class SurvivalCamera extends BasePlayerCamera {
	public isGrounded = true;
	private isJumping = false;
	private velocityY = 0;
	private motion: ParabolaMotion;
	private speed: number = 1;

	constructor(scene: Scene, canvas: HTMLCanvasElement) {
		super(scene, canvas);
		this.motion = new ParabolaMotion(0.7, "up", 1);
	}

	public override update(dt: number): void {
		this.checkGrounded();
		// 判断是否着地
		if (this.isJumping) {
			this.speed = 0.5;
			this.velocityY = this.motion.update(dt);

			if (this.velocityY <= 0 && this.isGrounded) {
				this.velocityY = 0;
				this.motion.reset();
				this.isJumping = false;
				this.speed = 1;
			}
			this.camera.cameraDirection.y = this.velocityY;
		}

		if (!this.isGrounded && !this.isJumping) {
			this.camera.cameraDirection.y = -0.02;
		}

		super.update(dt);
	}

	protected initCamera(): void {
		super.initCamera();
		this.camera.checkCollisions = true;
		this.camera.applyGravity = true;
	}

	protected bindInput(): void {
		super.bindInput();

		// 跳跃
		this.inputSystem.onActionStart("jump", () => {
			if (this.isGrounded) {
				this.isJumping = true;
				this.isGrounded = false;
			}
		});
	}

	protected getMoveSpeed(): number {
		return this.moveSpeed * this.speed; // 生存模式移动速度
	}

	private checkGrounded() {
		// 从碰撞体底部发射一条短射线向下
		const origin = this.camera.position;
		const ray = new Ray(origin, Vector3.Down(), 2);
		const pick = this.scene.pickWithRay(ray, mesh => mesh.isPickable && mesh.checkCollisions);
		this.isGrounded = !!(pick?.hit && pick.distance <= 2);
		return this.isGrounded;
	}
}

// 创造模式相机
export class CreativeCamera extends BasePlayerCamera {
	protected initCamera(): void {
		super.initCamera();
		this.camera.checkCollisions = false;
		this.camera.applyGravity = false;
	}

	protected bindInput(): void {
		super.bindInput();

		// 垂直移动
		this.inputSystem.onActionUpdate("fly", () => {
			this.moveValue.y += this.getMoveSpeed();
		});

		this.inputSystem.onActionUpdate("sneak", () => {
			this.moveValue.y -= this.getMoveSpeed();
		});
	}

	protected getMoveSpeed(): number {
		return 0.1; // 创造模式移动速度更快
	}
}
