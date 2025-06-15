import {
	Color3,
	Color4,
	Material,
	PBRMaterial,
	Scene,
	ShaderMaterial,
	StandardMaterial,
	Texture,
} from "@babylonjs/core";
import { Color } from "../types/block.type.ts";
import { SingleClass } from "../core/Singleton.ts";

/** 材质文档
 * https://doc.babylonjs.com/features/featuresDeepDive/materials/using/pbrMaterials
 */

/** Shader配置接口 */
interface ShaderConfig {
	/** 顶点着色器名称或代码 */
	vertex: string;
	/** 片段着色器名称或代码 */
	fragment: string;
	/** 着色器属性列表 */
	attributes?: string[];
	/** 着色器uniform变量列表 */
	uniforms?: string[];
	/** 自定义uniform变量值 */
	customUniforms?: Record<string, any>;
	/** 是否需要动画更新 */
	needAnimation?: boolean;
	/** 动画更新回调函数 */
	onAnimationUpdate?: (material: ShaderMaterial, deltaTime: number) => void;
}

/** 材质创建配置接口 */
interface MaterialCreationConfig {
	/** 纹理键，用于从纹理缓存中获取对应的纹理 */
	textureKey?: string;
	/** 基础颜色，影响材质的整体色调 */
	color: Color;
	/** 自发光颜色，使材质在暗处也能发光 */
	emissive: Color3;
	/** 金属度，影响材质的金属感，0-1之间，值越大金属感越强（控制镜面反射，必须与环境贴图搭配使用，否则值越大反而越暗） */
	metallic: number;
	/** 粗糙度，影响材质的反光程度，0-1之间，值越大越粗糙 */
	roughness: number;
	/** 透明度，0完全透明，1完全不透明 */
	alpha: number;
	/** 透明度阈值，低于此值的像素会被完全剔除 */
	alphaCutOff: number;
	/** 背面剔除，true时背面不可见，false时双面可见 */
	backFaceCulling: boolean;
	/** 高光颜色，影响材质反光时的颜色 */
	specular: Color3;
	/** 环境光强度，影响环境光对材质的影响程度，值越大环境光影响越强 */
	environmentIntensity: number;
	/** 是否使用物理光照衰减，开启后光照会随距离衰减 */
	usePhysicalLightFalloff: boolean;
}

export interface MaterialConfig {
	materialType?: "standard" | "pbr";
	textureKey?: string;
	color?: Color3 | Color4;
	emissive?: Color3;
	metallic?: number;
	roughness?: number;
	alpha?: number;
	alphaCutOff?: number;
	backFaceCulling?: boolean;
	environmentIntensity?: number;
	usePhysicalLightFalloff?: boolean;
	specular?: Color3;
	/** 自定义着色器配置 */
	shader?: ShaderConfig;
	meshProperties?: {
		isPickable?: boolean;
		checkCollisions?: boolean;
		receiveShadows?: boolean;
		alphaIndex?: number; // 控制渲染顺序
	};
	shadowProperties?: any;
}

export class BlockTextureManager extends SingleClass {
	private textureCache: Map<string, Texture> = new Map();

	constructor(public scene: Scene) {
		super();
	}

	registerTextures(textures: { key: string; path: string }[]) {
		for (const { path, key } of textures) {
			if (!this.textureCache.has(path)) {
				let texture = new Texture(
					path,
					this.scene,
					false,
					false,
					Texture.NEAREST_NEAREST_MIPNEAREST
				);
				texture.hasAlpha = true;
				this.textureCache.set(key, texture);
			}
		}
	}

	getTexture(key?: string): Texture {
		if (!key) key = Array.from(this.textureCache.keys())[0];
		let texture = this.textureCache.get(key);
		if (!texture) throw new Error(`Texture not found: ${key}`);
		return texture;
	}

	public override dispose() {
		this.textureCache.forEach(texture => texture.dispose());
		this.textureCache.clear();
	}
}

export class BlockMaterialManager extends SingleClass {
	// 预设材质类型
	static readonly PRESET_MATERIALS = {
		SOLID: "solid",
		WATER: "water",
		LAVA: "lava",
		LEAVES: "leaves",
		GLASS: "glass",
		CROSS: "cross",
		MODEL: "model",
		MODEL_COLLIDER: "model_collider",
		METAL: "metal",
	} as const;
	private static materialPresets: Map<string, MaterialConfig> = new Map();
	private materialCache: Map<string, Material> = new Map();
	private blockTextureManager: BlockTextureManager;

	constructor(
		public scene: Scene,
		blockTextureManager?: BlockTextureManager
	) {
		super();
		if (!BlockMaterialManager.materialPresets.size) {
			BlockMaterialManager.initializePresetMaterials();
		}
		this.blockTextureManager = blockTextureManager ?? BlockTextureManager.getInstance();
	}

	public static get Instance(): BlockMaterialManager {
		return this.getInstance();
	}

	// 初始化预设材质
	static initializePresetMaterials() {
		// 固体方块材质
		this.registerMaterialPreset(this.PRESET_MATERIALS.SOLID, {
			alpha: 1,
			backFaceCulling: true,
			roughness: 1,
			metallic: 0,
		});

		// 金属材质
		this.registerMaterialPreset(this.PRESET_MATERIALS.METAL, {
			alpha: 1,
			backFaceCulling: true,
			roughness: 0.2,
			metallic: 1.0,
			emissive: new Color3(0.01, 0.01, 0.01),
		});

		// 模型方块材质
		this.registerMaterialPreset(this.PRESET_MATERIALS.MODEL, {
			alpha: 1,
			backFaceCulling: true,
			roughness: 1,
			alphaCutOff: 0.5,
		});

		// 模型碰撞体材质
		this.registerMaterialPreset(this.PRESET_MATERIALS.MODEL_COLLIDER, {
			alpha: 0,
		});

		// 水材质 - 使用PBR材质模拟
		this.registerMaterialPreset(this.PRESET_MATERIALS.WATER, {
			alpha: 0.6,
			backFaceCulling: true,
			emissive: new Color3(0.4, 0.8, 0.8),
			meshProperties: {
				alphaIndex: 1,
				isPickable: false,
				checkCollisions: false,
			},
		});

		// 岩浆材质
		this.registerMaterialPreset(this.PRESET_MATERIALS.LAVA, {
			backFaceCulling: true,
			shader: {
				vertex: "lava",
				fragment: "lava",
				uniforms: ["flowSpeed", "glowIntensity", "time"],
				customUniforms: {
					flowSpeed: 0.5,
					glowIntensity: 0.8,
					time: 0,
				},
				needAnimation: true,
				onAnimationUpdate: (() => {
					let materialTime = 0;
					return (material: ShaderMaterial, deltaTime: number) => {
						materialTime += deltaTime;
						material.setFloat("time", materialTime);
					};
				})(),
			},
			meshProperties: {
				alphaIndex: 2,
				isPickable: false,
				checkCollisions: false,
			},
		});

		// 树叶材质
		this.registerMaterialPreset(this.PRESET_MATERIALS.LEAVES, {
			alphaCutOff: 0.5,
			backFaceCulling: false,
			roughness: 0.8,
			meshProperties: {
				checkCollisions: false,
			},
		});

		// 玻璃材质
		this.registerMaterialPreset(this.PRESET_MATERIALS.GLASS, {
			alphaCutOff: 0.5,
			backFaceCulling: false,
			roughness: 0.1,
			metallic: 0.9,
			meshProperties: {
				checkCollisions: true,
			},
		});

		// 十字材质（用于植物等）
		this.registerMaterialPreset(this.PRESET_MATERIALS.CROSS, {
			alpha: 1,
			backFaceCulling: false,
			roughness: 0.8,
			alphaCutOff: 0.5,
			meshProperties: {
				isPickable: true,
				checkCollisions: false,
			},
		});
	}

	// 注册预设材质
	static registerMaterialPreset(key: string, config: MaterialConfig) {
		this.materialPresets.set(key, config);
	}

	// 获取预设材质
	static getMaterialPreset(key: string): MaterialConfig | undefined {
		return this.materialPresets.get(key);
	}

	// 注册自定义材质
	static registerCustomMaterial(key: string, config: MaterialConfig) {
		if (this.materialPresets.has(key)) {
			throw new Error(`material key ${key} already exists`);
		}
		this.registerMaterialPreset(key, config);
	}

	static registerCustomMaterials(materials: { key: string; material: MaterialConfig }[]) {
		materials.forEach(({ key, material }) => {
			this.registerCustomMaterial(key, material);
		});
	}

	createMaterial(config: MaterialConfig): Material {
		const {
			materialType = "standard",
			color = new Color3(1, 1, 1),
			emissive = new Color3(0, 0, 0),
			metallic = 0,
			roughness = 1,
			textureKey,
			alpha = 1,
			backFaceCulling = true,
			shader,
			alphaCutOff = 0,
			specular = new Color3(0, 0, 0),
			environmentIntensity = 1,
			usePhysicalLightFalloff = false,
			meshProperties,
		} = config;

		const createMaterial =
			materialType === "standard"
				? this.createStandardMaterial.bind(this)
				: this.createPBRMaterial.bind(this);

		const mat: Material = shader
			? this.createShaderMaterial({
					textureKey,
					backFaceCulling,
					shader,
				})
			: createMaterial({
					textureKey,
					color,
					emissive,
					metallic,
					roughness,
					alpha,
					alphaCutOff,
					backFaceCulling,
					specular,
					environmentIntensity,
					usePhysicalLightFalloff,
				});
		mat.metadata = { meshProperties };

		return mat;
	}

	// 获取材质（带缓存）
	getMaterial(key: string, config: MaterialConfig): Material {
		if (this.materialCache.has(key)) {
			return this.materialCache.get(key)!;
		}

		const material = this.createMaterial(config);
		this.materialCache.set(key, material);
		return material;
	}

	// 根据材质键获取材质
	getMaterialByKey(matKey: string): Material {
		const presetConfig = BlockMaterialManager.getMaterialPreset(matKey);

		if (!presetConfig) {
			throw new Error(`No material preset found for key: ${matKey}`);
		}

		return this.getMaterial(matKey, presetConfig);
	}

	// 清除材质缓存
	public override dispose() {
		this.materialCache.clear();
	}

	// 创建Shader材质
	private createShaderMaterial(config: {
		textureKey?: string;
		backFaceCulling: boolean;
		shader: ShaderConfig;
	}): Material {
		const shaderMaterial = new ShaderMaterial(
			"customShaderMaterial",
			this.scene,
			{
				vertex: config.shader.vertex,
				fragment: config.shader.fragment,
			},
			{
				attributes: config.shader.attributes || ["position", "normal", "uv"],
				uniforms: [
					"world",
					"worldView",
					"worldViewProjection",
					"time",
					...(config.shader.uniforms || []),
				],
			}
		);

		// 设置基础属性
		shaderMaterial.backFaceCulling = config.backFaceCulling;
		shaderMaterial.needAlphaBlending = () => true;
		shaderMaterial.disableDepthWrite = false;

		// 设置纹理
		if (config.textureKey) {
			const texture = this.blockTextureManager.getTexture(config.textureKey);
			shaderMaterial.setTexture("diffuseSampler", texture);
		}

		// 设置自定义uniform变量
		if (config.shader.customUniforms) {
			for (const [key, value] of Object.entries(config.shader.customUniforms)) {
				if (Array.isArray(value) && value.length === 3) {
					// 如果是颜色数组，转换为Color3
					shaderMaterial.setColor3(key, new Color3(value[0], value[1], value[2]));
				} else if (typeof value === "number") {
					shaderMaterial.setFloat(key, value);
				} else if (value instanceof Color3) {
					shaderMaterial.setColor3(key, value);
				}
			}
		}

		// 如果需要动画更新
		if (config.shader.needAnimation) {
			// 为每个材质创建独立的时间变量
			let materialTime = 0;

			const observer = this.scene.onBeforeRenderObservable.add(() => {
				const deltaTime = this.scene.getEngine().getDeltaTime() / 1000;
				materialTime += deltaTime;

				// 设置材质特定的时间uniform
				shaderMaterial.setFloat("time", materialTime);

				// 调用自定义动画更新函数
				if (config.shader.onAnimationUpdate) {
					config.shader.onAnimationUpdate(shaderMaterial, deltaTime);
				}
			});

			// 在材质销毁时移除观察者
			shaderMaterial.onDisposeObservable.add(() => {
				this.scene.onBeforeRenderObservable.remove(observer);
			});
		}

		return shaderMaterial;
	}

	// 创建标准材质
	private createStandardMaterial(config: MaterialConfig): Material {
		const standardMaterial = new StandardMaterial("standardMaterial", this.scene);
		const texture = this.blockTextureManager.getTexture(config.textureKey);

		standardMaterial.diffuseTexture = texture;
		standardMaterial.alpha = config.alpha ?? 1;
		standardMaterial.alphaCutOff = config.alphaCutOff ?? 0;
		standardMaterial.separateCullingPass = true;
		standardMaterial.emissiveColor = config.emissive ?? new Color3(0, 0, 0);
		standardMaterial.specularColor = config.specular ?? new Color3(0, 0, 0);
		standardMaterial.roughness = config.roughness ?? 1;
		standardMaterial.backFaceCulling = config.backFaceCulling ?? true;

		return standardMaterial;
	}

	// 创建PBR材质
	private createPBRMaterial(config: MaterialConfig): Material {
		const pbrMaterial = new PBRMaterial("pbrMaterial", this.scene);
		const texture = this.blockTextureManager.getTexture(config.textureKey);

		pbrMaterial.albedoTexture = texture;
		pbrMaterial.alpha = config.alpha ?? 1;
		pbrMaterial.alphaCutOff = config.alphaCutOff ?? 0;
		pbrMaterial.emissiveColor = config.emissive ?? new Color3(0, 0, 0);
		pbrMaterial.roughness = config.roughness ?? 1;
		pbrMaterial.metallic = config.metallic ?? 0;
		pbrMaterial.backFaceCulling = config.backFaceCulling ?? true;
		pbrMaterial.environmentIntensity = config.environmentIntensity ?? 1;
		pbrMaterial.usePhysicalLightFalloff = config.usePhysicalLightFalloff ?? false;

		return pbrMaterial;
	}
}
