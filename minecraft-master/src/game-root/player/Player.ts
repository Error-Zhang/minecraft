import {
	Color3,
	Color4,
	Mesh,
	MeshBuilder,
	PointerEventTypes,
	Scene,
	StandardMaterial,
	Vector3,
} from "@babylonjs/core";
import { AdvancedDynamicTexture, Control, Rectangle } from "@babylonjs/gui";
import { DebugHelper } from "@/game-root/utils/DebugHelper.ts";
import {
	BasePlayerCamera,
	CreativeCamera,
	SurvivalCamera,
} from "@/game-root/player/PlayerCamera.ts";
import MathUtils from "@/game-root/utils/MathUtils.ts";
import { useGameStore, usePlayerStore, useWorldStore } from "@/store";
import { PlayerInputSystem } from "@/game-root/player/PlayerInputSystem.ts";
import { PlayerModel } from "@/game-root/player/PlayerModel.ts";
import { IBlockActionData } from "@/game-root/client/interface.ts";
import { BlockMaterialManager } from "@engine/renderer/BlockMaterialManager.ts";
import { audios } from "@/ui-root/assets/sounds";

export class Player {
	public playerModel?: PlayerModel;
	public camera: BasePlayerCamera;
	// 场景、相机、世界等
	private scene: Scene;
	private debugHelper: DebugHelper;
	private maxPlaceDistance = 12; // 最大方块放置距离
	private placeBlockCallBacks: ((data: IBlockActionData[]) => void)[] = [];

	constructor(scene: Scene, canvas: HTMLCanvasElement) {
		this.scene = scene;

		const Camera = useGameStore.getState().gameMode ? SurvivalCamera : CreativeCamera;

		this.camera = new Camera(scene, canvas);

		this.addEventListener();
		this.addCrossHair();
		this.showBlockHoverOutline();

		this.debugHelper = new DebugHelper(scene);
	}

	private get worldController() {
		return useWorldStore.getState().worldController!;
	}

	private get playerStore() {
		return usePlayerStore.getState();
	}

	private get worldStore() {
		return useWorldStore.getState();
	}

	public dispose() {
		this.camera.dispose();
	}

	public async loadModel(modelPath: string, texturePath: string) {
		this.playerModel = new PlayerModel(this.scene);
		return await this.playerModel.loadModel(modelPath, texturePath);
	}

	public update(dt: number) {
		this.camera.update(dt);
	}

	public onPlaceBlock(callback: (data: IBlockActionData[]) => void) {
		this.placeBlockCallBacks.push(callback);
	}

	public setPosition(x: number, y: number, z: number): void {
		this.camera.setPosition(x, y, z);
	}

	// 处理键盘输入
	private addEventListener() {
		const inputSystem = PlayerInputSystem.Instance;
		inputSystem.onActionUpdate("break", () => this.destroyBlock());
		inputSystem.onActionStart("interact", () => {
			if (!this.interactWithBlock()) {
				this.placeBlock();
			}
		});
	}

	private getTargetBlockInfo() {
		const pick = this.camera.getPickInfo(this.maxPlaceDistance);
		if (!pick) return null;

		const faceNormal = pick.getNormal(true);
		if (!faceNormal) return null;

		const pickedPos = pick.pickedPoint!;
		const currentBlockPos = this.getCurrentBlockPos(pick?.pickedMesh?.id, pickedPos, faceNormal);

		return { faceNormal, currentBlockPos };
	}

	// 与方块交互
	private interactWithBlock() {
		const info = this.getTargetBlockInfo();
		if (!info) return false;

		const block = this.worldController.getBlock(info.currentBlockPos);
		const interact = block?.behavior?.onInteract;
		interact?.();

		return !!interact;
	}

	// 放置方块
	private placeBlock() {
		const info = this.getTargetBlockInfo();
		if (!info) return;
		let isCross =
			this.worldStore.worldController?.getBlock(info.currentBlockPos)?.render.type === "cross";

		const placePos = isCross ? info.currentBlockPos : info.currentBlockPos.add(info.faceNormal);
		let blockId = this.playerStore.holdBlockId;
		if (blockId === 0) return;
		const { x, y, z } = placePos;
		this.placeBlockCallBacks.forEach(callback => callback([{ x, y, z, blockId }]));
		audios.BlockPlaced.play();
	}

	// 销毁方块
	private destroyBlock() {
		const info = this.getTargetBlockInfo();
		if (!info) return;
		const { x, y, z } = info.currentBlockPos;
		let blocks: IBlockActionData[] = [{ x, y, z, blockId: 0 }];
		let isCrossPos = info.currentBlockPos.add(new Vector3(0, 1, 0));
		let isCross = this.worldStore.worldController?.getBlock(isCrossPos)?.render.type === "cross";
		if (isCross) {
			const { x, y, z } = isCrossPos;
			blocks.push({ x, y, z, blockId: 0 });
		}
		this.placeBlockCallBacks.forEach(callback => callback(blocks));
		audios.BlockPlaced.play();
	}

	private showBlockHoverOutline() {
		// 创建高亮盒子（只创建一次）
		let highlightBox: Mesh = MeshBuilder.CreateBox("hoverBox", { size: 1 }, this.scene);
		highlightBox.renderingGroupId = 1;
		const mat = new StandardMaterial("outlineMat", this.scene);
		mat.emissiveColor = new Color3(1, 1, 1); // 白色发光

		highlightBox.material = mat;
		highlightBox.isPickable = false;
		highlightBox.setEnabled(false); // 初始隐藏
		highlightBox.enableEdgesRendering(); // 开启边框渲染
		highlightBox.edgesWidth = 2;
		highlightBox.edgesColor = new Color4(1, 1, 1, 1); // 白色边框
		highlightBox.material.alpha = 0; // 使本体透明

		this.scene.onPointerObservable.add(pointerInfo => {
			if (pointerInfo.type === PointerEventTypes.POINTERMOVE) {
				const pickResult = this.camera.getPickInfo(this.maxPlaceDistance);
				const pickedPoint = pickResult?.pickedPoint;
				const normal = pickResult?.getNormal();

				if (pickedPoint && normal) {
					// 转换为整数网格坐标（向下取整）
					const currentPos = this.getCurrentBlockPos(
						pickResult?.pickedMesh?.id!,
						pickedPoint,
						normal
					);

					// 将 highlightBox 移动到方块中心位置
					highlightBox.position.set(currentPos.x + 0.5, currentPos.y + 0.5, currentPos.z + 0.5);
					highlightBox.setEnabled(true);
				} else {
					highlightBox.setEnabled(false); // 没有拾取到方块，隐藏高亮框
				}
			}
		});
	}

	// 计算当前方块位置
	private getCurrentBlockPos(
		meshId: string = "",
		pickedPoint: Vector3,
		faceNormal: Vector3
	): Vector3 {
		if (meshId === BlockMaterialManager.PRESET_MATERIALS.CROSS) {
			return new Vector3(
				Math.floor(pickedPoint.x),
				Math.floor(pickedPoint.y),
				Math.floor(pickedPoint.z)
			);
		}
		return new Vector3(
			MathUtils.correct(pickedPoint.x, faceNormal.x),
			MathUtils.correct(pickedPoint.y, faceNormal.y),
			MathUtils.correct(pickedPoint.z, faceNormal.z)
		);
	}

	// 在屏幕中心添加一个十字形准星
	private addCrossHair(size = 8, color = "white") {
		const ui = AdvancedDynamicTexture.CreateFullscreenUI("CrosshairUI", true, this.scene);
		const thickness = 1.5;
		const createLine = (x: number, y: number, width: number, height: number) => {
			const line = new Rectangle();
			line.width = `${width}px`;
			line.height = `${height}px`;
			line.color = color;
			line.thickness = thickness;
			line.background = color;
			line.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_CENTER;
			line.verticalAlignment = Control.VERTICAL_ALIGNMENT_CENTER;
			line.left = `${x}px`;
			line.top = `${y}px`;
			return line;
		};

		const half = size / 2;
		ui.addControl(createLine(0, -half - 2, thickness, size)); // 上
		ui.addControl(createLine(0, half + 2, thickness, size)); // 下
		ui.addControl(createLine(-half - 2, 0, size, thickness)); // 左
		ui.addControl(createLine(half + 2, 0, size, thickness)); // 右
	}
}
