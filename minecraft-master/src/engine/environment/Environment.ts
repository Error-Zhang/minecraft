import {
	Animation,
	Color3,
	DirectionalLight,
	HemisphericLight,
	Mesh,
	MeshBuilder,
	Scene,
	ShadowGenerator,
	StandardMaterial,
	Texture,
	Vector3,
} from "@babylonjs/core";
import { SkyMaterial } from "@babylonjs/materials";
import { GameTime } from "../systems/GameTime";
import { SingleClass } from "@engine/core/Singleton.ts";
import cloud from "./assets/Clouds.webp";

export class Environment extends SingleClass {
	public readonly Size: number = 1024;
	// 半径
	public readonly MinUpdateDistance = this.Size * 0.4;

	public openShadow: number = 1;
	public shadowGenerator?: ShadowGenerator;

	/** 每隔多少"游戏分钟"更新一次光照 */
	public updateIntervalMinutes = 60;
	public intensity = 2;
	private readonly scene: Scene;
	private skyBox!: Mesh;
	private readonly gameTime: GameTime;
	private skyMaterial!: SkyMaterial;
	private directionalLight!: DirectionalLight;
	private hemisphericLight!: HemisphericLight;
	private _lastUpdateStep = -1;
	private cloudLayer!: Mesh;

	constructor(scene: Scene) {
		super();
		this.scene = scene;
		this.gameTime = GameTime.getInstance();
		this.createSky();
		this.createCloud();
		this.createLight();
		// scene.fogMode = Scene.FOGMODE_LINEAR;
		// scene.fogColor = new Color3(0.7, 0.8, 1.0); // 建议与天空渐变一致
		// scene.fogStart = (ChunkManager.ViewDistance - 2) * Chunk.Size; // 近处开始无雾，单位为米
		// scene.fogEnd = ChunkManager.ViewDistance * Chunk.Size; // 远处完全雾化
	}

	public static override get Instance(): Environment {
		return this.getInstance();
	}

	public clearShadows(): void {
		let renderList = this.shadowGenerator?.getShadowMap()?.renderList;
		if (renderList?.length) {
			renderList.splice(0);
		}
	}

	public dispose(): void {
		this.clearShadows();
		this.cloudLayer.dispose();
	}

	public updatePosition(x: number, z: number) {
		this.skyBox.position.set(x, 0, z);
		this.directionalLight?.position.set(x, this.Size, z);
		this.cloudLayer?.position.set(x, this.cloudLayer.position.y, z);
	}

	public updateLighting() {
		const progress = this.gameTime.dayProgress; // 0 ~ 1
		const totalGameMinutesPerDay = 24 * 60;
		const updatesPerDay = Math.floor(totalGameMinutesPerDay / this.updateIntervalMinutes);
		const currentStep = Math.floor(progress * updatesPerDay);

		// 减少阴影变化的触发频率
		if (currentStep === this._lastUpdateStep) return;
		this._lastUpdateStep = currentStep;

		this.setSkyConfig("material.inclination", this.skyMaterial.inclination, progress * 2 - 1);

		// 太阳角度，调整偏移让中午在天空正上方
		const sunAngle = progress * Math.PI * 2 - Math.PI / 2;

		const dirY = Math.sin(sunAngle);
		const dirXZ = Math.cos(sunAngle);

		const light = dirY + 1;
		const daylight = Math.max(Math.pow(dirY, 0.5) || 0, 0);

		// 环境光用来补充阴影亮度
		this.hemisphericLight.intensity =
			(daylight * this.intensity) / 2 || Math.max(light * 0.1, 0.005);

		this.directionalLight.direction = new Vector3(dirXZ, dirY, dirXZ).negate().normalize();
		this.directionalLight.intensity = daylight * this.intensity;
	}

	private createSky() {
		// 创建 Sky 材质
		this.skyMaterial = new SkyMaterial("skyMaterial", this.scene);
		this.skyMaterial.backFaceCulling = false;
		this.skyMaterial.azimuth = 0.125; // 东南方向
		// 创建天空盒 Mesh
		this.skyBox = MeshBuilder.CreateBox(
			"skyBox",
			{ width: this.Size, height: this.Size, depth: this.Size },
			this.scene
		);
		this.skyBox.material = this.skyMaterial;
	}

	private createCloud() {
		// 创建云层材质
		const cloudMat = new StandardMaterial("cloudMat", this.scene);
		const cloudTexture = new Texture(cloud, this.scene);
		cloudTexture.hasAlpha = true;
		cloudTexture.uScale = 2;
		cloudTexture.vScale = 2;

		cloudMat.diffuseTexture = cloudTexture;
		cloudMat.opacityTexture = cloudTexture;
		cloudMat.backFaceCulling = false;
		cloudMat.emissiveColor = new Color3(1, 1, 1);
		cloudMat.useEmissiveAsIllumination = true;
		cloudMat.useAlphaFromDiffuseTexture = true;

		// 创建云层半球
		this.cloudLayer = MeshBuilder.CreateGround(
			"cloudLayer",
			{
				width: this.Size,
				height: this.Size,
			},
			this.scene
		);
		this.cloudLayer.position.y = this.Size / 2 - 24;
		this.cloudLayer.material = cloudMat;
		this.cloudLayer.renderingGroupId = 0;
	}

	private createLight() {
		// 创建主光源（太阳/月亮）
		this.directionalLight = new DirectionalLight("Light", new Vector3(-1, -1, -1), this.scene);
		this.directionalLight.intensity = 1;

		if (this.openShadow) {
			this.directionalLight.shadowEnabled = true;
			// 配置阴影
			const shadowGenerator = new ShadowGenerator(1024, this.directionalLight);
			shadowGenerator.bias = 0;
			shadowGenerator.normalBias = 0.01;
			shadowGenerator.usePercentageCloserFiltering = true;
			shadowGenerator.filteringQuality = ShadowGenerator.QUALITY_LOW;
			shadowGenerator.forceBackFacesOnly = true;
			this.shadowGenerator = shadowGenerator;
		}

		// 创建环境光
		this.hemisphericLight = new HemisphericLight("hemiLight", new Vector3(0, 1, 0), this.scene);
		this.hemisphericLight.intensity = 0;
	}

	/**
	 * 设置天空属性的动画变化（用于渐变过渡）
	 * @param property 属性名称
	 * @param from 起始值
	 * @param to 目标值
	 */
	private setSkyConfig(property: string, from: number, to: number) {
		const keys = [
			{ frame: 0, value: from },
			{ frame: 100, value: to },
		];
		const animation = new Animation(
			"skyAnimation",
			property,
			100, // 帧率
			Animation.ANIMATIONTYPE_FLOAT,
			Animation.ANIMATIONLOOPMODE_CONSTANT
		);
		animation.setKeys(keys);

		// 停止现有动画并开始新的属性动画
		this.scene.stopAnimation(this.skyBox);
		this.scene.beginDirectAnimation(this.skyBox, [animation], 0, 100, false, 1);
	}
}
