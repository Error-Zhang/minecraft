import { Scene } from "@babylonjs/core";
import { AdvancedDynamicTexture, TextBlock } from "@babylonjs/gui";
import { throttle } from "@/game-root/utils/lodash.ts";
import { VoxelEngine } from "@engine/core/VoxelEngine.ts";
import { Coords } from "@engine/types/chunk.type.ts";
import { Chunk } from "@engine/chunk/Chunk.ts";
import { getCombinedBlocks } from "@/game-root/block-definitions/blocks.ts";
import Assets from "@/game-root/assets";
import { useBlockStore, useGameStore, usePlayerStore, useWorldStore } from "@/store";
import { Player } from "@/game-root/player/Player.ts";
import { blockApi, worldApi } from "@/ui-root/api";
import GameWindow from "@/game-root/core/GameWindow.ts";
import { PlayerInputSystem } from "@/game-root/player/PlayerInputSystem.ts";
import GameClient from "@/game-root/client/GameClient.ts";
import { playerEvents } from "@/game-root/core/events.ts";
import { IChunkSetting } from "@/game-root/client/interface.ts";
import { WorldController } from "@engine/core/WorldController.ts";
import { NpcPlayer } from "@/game-root/player/NpcPlayer.ts";
import { BlockCoder } from "@/game-root/block-definitions/BlockCoder.ts";
import { SurvivalCamera } from "@/game-root/player/PlayerCamera.ts";
import { LoadingScreen } from "./ui/LoadingScreen";
import VertexBuilderWorker from "./worker/VertexBuilder.worker.ts?worker";
import * as Comlink from "comlink";
import { IVertexBuilder, IVertexBuilderConstructor } from "@/game-root/worker/interface.ts";
import MathUtils from "@/game-root/utils/MathUtils.ts";
import { BlockPlacement } from "@/game-root/managers/BlockPlacement.ts";
import { BlockIconGenerator } from "@engine/block-icon/BlockIconGenerator.ts";
import { BlockDefinition } from "@engine/types/block.type.ts";
import { IChunkData } from "@/ui-root/api/interface.ts";
import { audios } from "@/ui-root/assets/sounds";

export class Game {
	public canvas: HTMLCanvasElement;
	private voxelEngine: VoxelEngine;
	private scene!: Scene;
	private player!: Player;
	private gameClient: GameClient = new GameClient();
	private npcPlayers = new Map<number, NpcPlayer>();
	private worker?: Worker;
	private vertexBuilder?: Comlink.Remote<IVertexBuilder>;
	private screen?: LoadingScreen;
	private readonly textures = [{ key: "blocks", path: Assets.blocks.atlas }];
	private unsubscribes: Function[] = [];

	constructor(canvas: HTMLCanvasElement) {
		this.canvas = canvas;
		GameWindow.create(canvas);
		this.voxelEngine = new VoxelEngine(canvas);
		this.registerBlocks().then(blockRegistry => {
			this.generateBlockIcons(blockRegistry.getAllBlocks());
		});
		this.voxelEngine.onUpdate(() => PlayerInputSystem.Instance.update(), false);
	}

	private get playerStore() {
		return usePlayerStore.getState();
	}

	private get worldStore() {
		return useWorldStore.getState();
	}

	private get blockStore() {
		return useBlockStore.getState();
	}

	private get gameStore() {
		return useGameStore.getState();
	}

	public async start() {
		this.scene = await this.voxelEngine.createScene();
		this.attachFPSDisplay();
		this.voxelEngine.registerTexturesAndMaterials(this.scene, this.textures);
		const { chunkSetting, players } = await this.gameClient.joinWorld(
			this.worldStore.worldId,
			this.playerStore.playerId
		);

		this.vertexBuilder = await this.initWorker(chunkSetting);

		const worldController = this.initWorld(chunkSetting);
		worldController.onChunkUnload(async chunk => {
			await this.vertexBuilder?.removeChunk(chunk.Key);
		});

		this.player = new Player(this.scene, this.canvas);
		this.setupEvents(worldController);

		this.voxelEngine.onUpdate(() => {
			const dt = this.voxelEngine.engine.getDeltaTime();
			this.player.update(dt);
		});
		this.showLoadingScreen(worldController);

		await worldController.updateChunk(this.playerStore.origin);
		players.forEach(p => this.spawnNPC(p));
		this.setupPlayerPosition();
	}

	public dispose() {
		this.worker?.terminate();
		this.gameClient.disConnectAll();
		this.player.dispose();
		this.voxelEngine.disposeScene();
		this.worldStore.reset();
		this.playerStore.reset();
		this.npcPlayers.forEach(p => p.dispose());
		this.npcPlayers.clear();
		playerEvents.removeAllListeners();
		this.unsubscribes.forEach(unsubscribe => unsubscribe());
		this.unsubscribes.length = 0;
		useGameStore.setState({ isGaming: false, isLoading: true });
	}

	public destroy() {
		this.dispose();
		GameWindow.Instance.dispose();
	}

	private async generateBlockIcons(blocks: BlockDefinition<any>[]) {
		let count = await BlockIconGenerator.getBlockIconCount();
		if (count === blocks.length) {
			if (!this.gameStore.isInitialized) {
				useGameStore.setState({ isInitialized: true });
			}
			return;
		}
		const blockIconGenerator = new BlockIconGenerator(this.textures);
		for await (const { block, index, total } of blockIconGenerator.generateIconsWithProgress(
			blocks
		)) {
			this.screen?.updateIconText(
				`正在生成图标：${index + 1} / ${total}，当前方块：${block.metaData.displayName}`
			);
			if (index + 1 === total) {
				useGameStore.setState({ isInitialized: true });
			}
		}
	}

	private async initWorker(setting: IChunkSetting) {
		this.worker = new VertexBuilderWorker();
		const VertexBuilderClass = Comlink.wrap<IVertexBuilderConstructor>(this.worker);
		const vertexBuilder = await new VertexBuilderClass(setting.chunkSize, setting.chunkHeight);
		await vertexBuilder.addBlocks(this.blockStore.blockTypes!);
		this.voxelEngine?.registerMeshBuilder(vertexBuilder.buildMesh.bind(vertexBuilder));
		return vertexBuilder;
	}

	private initWorld(setting: IChunkSetting) {
		const controller = this.voxelEngine.registerChunk(this.getWorldGenerator(), setting);
		useWorldStore.setState({ worldController: controller });
		return controller;
	}

	private getWorldGenerator() {
		const processChunkData = async (raw: IChunkData[], isRLE: boolean) => {
			const chunkDatas = raw.map(data => ({
				blocks: isRLE
					? <Uint16Array>MathUtils.decompressRLE(data.cells)
					: Uint16Array.from(data.cells),
				shafts: isRLE
					? <Uint8Array>MathUtils.decompressRLE(data.shafts)
					: Uint8Array.from(data.shafts),
				position: { x: data.x, z: data.z },
			}));
			await this.vertexBuilder?.addChunks(chunkDatas);
			return chunkDatas.map(chunkData => Chunk.fromJSON(chunkData));
		};
		const isFlat = this.worldStore.worldMode === 2;
		const generator = isFlat ? worldApi.generateFlatWorld : worldApi.generateChunks;
		return async (coords: Coords) => {
			const raw = await generator(this.worldStore.worldId, coords);
			return processChunkData(raw, !isFlat);
		};
	}

	private spawnNPC(playerId: number) {
		const npc = new NpcPlayer(this.scene, playerId);
		npc
			.loadModel(Assets.player.models.HumanMale, Assets.player.textures.HumanMaleTexture1)
			.then(model => {
				this.voxelEngine.addMesh(model);
				this.gameClient.playerClient.getPlayerPosition(playerId).then(pos => {
					if (pos) {
						npc.setPosition(pos.x, pos.y, pos.z);
						npc.setRotation(pos.yaw, pos.pitch);
					} else this.setupPlayerPosition(npc.setPosition.bind(npc));
				});
			});
		this.npcPlayers.set(playerId, npc);
		audios.Message.play();
	}

	private setupEvents(world: WorldController) {
		const client = this.gameClient.playerClient;
		const placer = new BlockPlacement(world, this.vertexBuilder);
		this.voxelEngine.onUpdate(() => {
			placer.update();
		});
		client.onPlayerJoined(id => this.spawnNPC(id));
		client.onPlayerMove(({ playerId, x, y, z, pitch, yaw }) => {
			const p = this.npcPlayers.get(playerId);
			p?.moveTo(x, y, z);
			p?.setRotation(yaw, pitch);
		});
		client.onPlayerLeave(id => this.npcPlayers.get(id)?.dispose());

		client.onPlaceBlock(async data => {
			placer.enqueuePlacement(data);
		});

		this.player.onPlaceBlock(client.sendPlaceBlock.bind(client));

		playerEvents.on(
			"playerMoved",
			throttle((pos, rot) => {
				world.updateChunk(pos);
				client.sendPlayerMove({
					playerId: this.playerStore.playerId,
					...pos,
					...rot,
				});
			}, 100)
		);
	}

	private setupPlayerPosition(setPos?: (x: number, y: number, z: number) => void) {
		const [x, y, z] = this.worldStore.worldController!.getChunkCenterTop(
			this.playerStore.origin.x,
			this.playerStore.origin.z
		);
		(setPos ?? this.player.setPosition.bind(this.player))(x, y + 2, z);
		if (this.player.camera instanceof SurvivalCamera) this.player.camera.isGrounded = false;
	}

	private attachFPSDisplay() {
		const gui = AdvancedDynamicTexture.CreateFullscreenUI("FPS-UI", true, this.scene);
		const label = new TextBlock();
		label.color = "white";
		label.fontSize = 18;
		label.textHorizontalAlignment = TextBlock.HORIZONTAL_ALIGNMENT_RIGHT;
		label.textVerticalAlignment = TextBlock.VERTICAL_ALIGNMENT_TOP;
		label.paddingRight = 10;
		label.paddingTop = 10;
		gui.addControl(label);
		const update = throttle(() => {
			label.text = `FPS: ${Math.floor(this.voxelEngine.engine.getFps())}`;
		}, 1000);
		this.scene.onBeforeRenderObservable.add(update);
	}

	private showLoadingScreen(worldController: WorldController) {
		const screen = new LoadingScreen(this.scene);
		const enterToWorld = () => {
			useGameStore.setState({ isLoading: false });
			screen.dispose();
			worldController.offChunkUpdated(id);
			this.voxelEngine.start(this.scene);
		};
		const id = worldController.onChunkUpdated(progress => {
			screen.update({
				worldName: this.worldStore.worldHost,
				seasonId: this.worldStore.season,
				time: "12:00",
				progress,
			});
			this.scene.render();
			if (progress === 1) {
				if (this.gameStore.isInitialized) {
					enterToWorld();
				} else {
					const unsub = useGameStore.subscribe((state, prevState) => {
						if (state.isInitialized && state.isInitialized !== prevState.isInitialized) {
							enterToWorld();
						}
					});
					this.unsubscribes.push(unsub);
				}
			}
		});
		this.screen = screen;
	}

	private async registerBlocks() {
		const types = await blockApi.getBlockTypes();
		const merged = getCombinedBlocks(types);
		const registry = VoxelEngine.registerBlocks(merged, BlockCoder.extractId.bind(BlockCoder));
		useBlockStore.setState({ blockRegistry: registry, blockTypes: types });
		return registry;
	}
}
