import { PlayerClient } from "./PlayerClient.ts";
import { WorldClient } from "./WorldClient.ts";
import { BASE_URL } from "@/ui-root/api/request.ts";

class GameClient {
	public playerClient: PlayerClient;
	public worldClient: WorldClient;

	constructor(baseUrl: string = BASE_URL) {
		this.playerClient = new PlayerClient(`${baseUrl}/playerhub`);
		this.worldClient = new WorldClient(`${baseUrl}/worldhub`);
		this.playerClient.bindWorldClient(this.worldClient);
	}

	public async disConnectAll() {
		await Promise.all([this.playerClient.disconnect(), this.worldClient.disconnect()]);
	}

	public async joinWorld(worldId: number, playerId: number) {
		await this.connectAll();
		await this.playerClient.joinWorld(worldId, playerId);
		const players = await this.playerClient.getPlayersInWorld();
		const chunkSetting = await this.worldClient.joinWorld(worldId, playerId);
		return { players, chunkSetting };
	}

	private async connectAll() {
		await Promise.all([this.playerClient.connect(), this.worldClient.connect()]);
	}
}

export default GameClient;
