// WorldClient.ts
import * as signalR from "@microsoft/signalr";
import { ApiResponse, IBlockActionData, IChunkSetting } from "@/game-root/client/interface.ts";

export class WorldClient {
	private connection: signalR.HubConnection;

	constructor(hubUrl: string) {
		this.connection = new signalR.HubConnectionBuilder()
			.withUrl(hubUrl)
			.withAutomaticReconnect()
			.build();
	}

	async connect() {
		await this.connection.start();
		console.log("[WorldClient] Connected");
	}

	async disconnect() {
		await this.connection.stop();
		console.log("[WorldClient] Disconnected");
	}

	async setBlock(block: IBlockActionData[]) {
		await this.connection.invoke<ApiResponse<any>>("SetBlock", block);
	}

	async joinWorld(worldId: number, playerId: number) {
		const response = await this.connection.invoke<ApiResponse<IChunkSetting>>(
			"JoinWorld",
			worldId,
			playerId
		);
		if (response.code !== 200) {
			throw new Error(`Can't join world because ${response.message}`);
		}
		return response.data;
	}

	async leaveWorld() {
		const response = await this.connection.invoke<ApiResponse<any>>("LeaveWorld");
		if (response.code !== 200) {
			throw new Error(`Can't leave world because ${response.message}`);
		}
	}
}
