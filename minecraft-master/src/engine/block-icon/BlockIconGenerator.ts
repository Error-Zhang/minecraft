import {
	ArcRotateCamera,
	Camera,
	Color4,
	Engine,
	HemisphericLight,
	Scene,
	Vector3,
} from "@babylonjs/core";
import { BlockIconStore } from "@engine/block-icon/BlockIconStore.ts";
import { MiniBlockBuilder } from "@engine/renderer/MiniBlockBuilder.ts";
import { BlockDefinition } from "@engine/types/block.type.ts";
import {
	BlockMaterialManager,
	BlockTextureManager,
} from "@engine/renderer/BlockMaterialManager.ts";
import { BlockRegistry } from "@engine/block/BlockRegistry.ts";

export class BlockIconGenerator {
	public scene: Scene;
	private engine: Engine;
	private canvas: HTMLCanvasElement;
	private camera: ArcRotateCamera;
	private hemisphericLight: HemisphericLight;
	private blockTextureManager: BlockTextureManager;
	private blockMaterialManager: BlockMaterialManager;
	private miniBlockBuilder: MiniBlockBuilder;

	constructor(textures: { key: string; path: string }[]) {
		this.canvas = document.createElement("canvas");
		this.canvas.width = 128;
		this.canvas.height = 128;
		this.engine = new Engine(this.canvas, false);

		this.scene = new Scene(this.engine);
		this.scene.clearColor = new Color4(0, 0, 0, 0);
		this.hemisphericLight = new HemisphericLight("hemiLight", new Vector3(0, 1, 0), this.scene);
		this.camera = new ArcRotateCamera(
			"Camera",
			Math.PI / 4,
			Math.atan(Math.sqrt(2)),
			3,
			Vector3.Zero(),
			this.scene
		);
		this.camera.mode = Camera.ORTHOGRAPHIC_CAMERA;

		const halfSize = 1;
		this.camera.orthoLeft = -halfSize;
		this.camera.orthoRight = halfSize;
		this.camera.orthoTop = halfSize;
		this.camera.orthoBottom = -halfSize;

		this.blockTextureManager = new BlockTextureManager(this.scene);
		this.blockTextureManager.registerTextures(textures);
		this.blockMaterialManager = new BlockMaterialManager(this.scene, this.blockTextureManager);
		this.miniBlockBuilder = new MiniBlockBuilder(this.scene, this.blockMaterialManager, false);
	}

	public static async getBlockIconCount() {
		return await BlockIconStore.count();
	}

	public async *generateIconsWithProgress(blocks: BlockDefinition<any>[]): AsyncGenerator<{
		block: BlockDefinition<any>;
		index: number;
		total: number;
	}> {
		const total = blocks.length;

		for (let index = 0; index < total; index++) {
			const block = blocks[index];
			let fullName = BlockRegistry.Instance.getFullName(block.blockType);
			let blob = await BlockIconStore.get(fullName);
			if (!blob) {
				blob = await this.generateBlockIcon(block);
				await BlockIconStore.set(fullName, blob);
			}

			yield { block, index, total };
		}
		console.log("[BlockIconGenerator] 方块图标生成完成");
	}

	dispose() {
		this.blockTextureManager.dispose();
		this.blockMaterialManager.dispose();
		this.scene.dispose();
		this.engine.dispose();
	}

	private async generateBlockIcon(block: BlockDefinition<any>): Promise<Blob> {
		const mesh = await this.miniBlockBuilder.createMesh(
			Vector3.Zero(),
			block.id!,
			block.render,
			block.properties,
			1
		);

		await new Promise<void>(resolve => {
			this.scene.executeWhenReady(resolve);
		});

		this.scene.render();

		return await new Promise<Blob>(resolve => {
			this.canvas.toBlob(blob => resolve(blob!), "image/webp");
			mesh.dispose();
		});
	}
}
