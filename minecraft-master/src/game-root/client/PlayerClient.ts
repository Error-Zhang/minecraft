// PlayerClient.ts
import * as signalR from "@microsoft/signalr";
import { ApiResponse, IBlockActionData, IPlayerMoveData } from "@/game-root/client/interface.ts";
import { WorldClient } from "@/game-root/client/WorldClient.ts";

export class PlayerClient {
	private connection: signalR.HubConnection;
	private worldClient!: WorldClient;

	constructor(hubUrl: string) {
		this.connection = new signalR.HubConnectionBuilder()
			.withUrl(hubUrl)
			.withAutomaticReconnect()
			.build();
	}

	bindWorldClient(worldClient: WorldClient) {
		this.worldClient = worldClient;
	}

	async connect() {
		await this.connection.start();
		console.log("[PlayerClient] Connected");
	}

	async disconnect() {
		await this.connection.stop();
		console.log("[PlayerClient] Disconnected");
	}

	async sendPlayerMove(data: IPlayerMoveData) {
		await this.connection.invoke("PlayerMove", data);
	}

	async sendPlaceBlock(data: IBlockActionData[]) {
		await this.worldClient.setBlock(data);
		await this.connection.invoke("PlaceBlock", data);
	}

	onPlayerMove(callback: (data: IPlayerMoveData) => void) {
		this.safeOn("PlayerMove", callback);
	}

	onPlaceBlock(callback: (data: IBlockActionData[]) => void) {
		this.safeOn("PlaceBlock", callback);
	}

	onPlayerJoined(callback: (playerId: number) => void) {
		this.safeOn("PlayerJoined", callback);
	}

	onPlayerLeave(callback: (playerId: number) => void) {
		this.safeOn("PlayerLeave", callback);
	}

	async getPlayerPosition(playerId: number) {
		const res = await this.connection.invoke<ApiResponse<IPlayerMoveData | null>>(
			"GetPlayerPosition",
			playerId
		);
		if (res.code !== 200) {
			throw new Error("获取玩家位置失败:" + res.message);
		}
		return res.data;
	}

	async getPlayersInWorld() {
		return await this.connection.invoke<number[]>("GetPlayersInWorld");
	}

	async joinWorld(worldId: number, playerId: number) {
		const response = await this.connection.invoke<ApiResponse<null>>(
			"JoinWorld",
			worldId,
			playerId
		);
		if (response.code !== 200) {
			throw new Error(`加入世界失败:${response.message}`);
		}
	}

	private safeOn<T>(event: string, callback: (data: T) => void) {
		this.connection.on(event, (data: T) => {
			try {
				callback(data);
			} catch (e) {
				console.error(`[${event}] handler error:`, e);
			}
		});
	}
}
