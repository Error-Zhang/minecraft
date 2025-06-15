import { AbstractMesh, Animation, Scene, Vector3 } from "@babylonjs/core";
import { PlayerModel } from "@/game-root/player/PlayerModel.ts";

export class NpcPlayer {
	public model?: AbstractMesh;
	private playerModel?: PlayerModel;

	constructor(
		public scene: Scene,
		private playerId: number
	) {}

	public async loadModel(modelPath: string, texturePath: string) {
		this.playerModel = new PlayerModel(this.scene);
		this.model = await this.playerModel.loadModel(modelPath, texturePath);
		return this.model;
	}

	public setPosition(x: number, y: number, z: number) {
		this.model?.position.set(x, y - 1.5, z);
	}

	public moveTo(x: number, y: number, z: number) {
		if (!this.model) return;

		const targetPosition = new Vector3(x, y - 1.5, z);
		const currentPosition = this.model.position.clone();

		// 如果位置没有变化，直接返回
		if (currentPosition.equals(targetPosition)) return;

		// 创建位置动画
		const frameRate = 60;
		const duration = 0.1; // 动画持续时间（秒）

		const positionAnimation = new Animation(
			"positionAnimation",
			"position",
			frameRate,
			Animation.ANIMATIONTYPE_VECTOR3,
			Animation.ANIMATIONLOOPMODE_CONSTANT
		);

		const keyFrames = [];
		keyFrames.push({
			frame: 0,
			value: currentPosition,
		});
		keyFrames.push({
			frame: frameRate * duration,
			value: targetPosition,
		});

		positionAnimation.setKeys(keyFrames);

		// 停止之前的动画
		this.scene.stopAnimation(this.model);

		// 开始新的动画
		this.scene.beginDirectAnimation(
			this.model,
			[positionAnimation],
			0,
			frameRate * duration,
			false,
			1,
			() => {
				// 停止行走动画
				this.playerModel?.stopWalking();
			}
		);
		this.playerModel?.startWalking();
	}

	public setRotation(yaw: number, pitch: number) {
		this.playerModel?.lookYawPitch(yaw, pitch);
	}

	public dispose() {
		this.model?.dispose();
	}
}
