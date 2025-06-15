import { IBlockActionData } from "@/game-root/client/interface.ts";
import { WorldController } from "@engine/core/WorldController.ts";
import { IVertexBuilder } from "@/game-root/worker/interface.ts";
import * as Comlink from "comlink";

export class BlockPlacement {
	private queueMap = new Map<string, IBlockActionData>();
	private flushing = false;
	private lastFlushTime = 0;
	private readonly flushInterval = 100;

	constructor(
		private world: WorldController,
		private vertexBuilder?: Comlink.Remote<IVertexBuilder>
	) {}

	public enqueuePlacement(data: IBlockActionData[]) {
		for (const block of data) {
			const key = this.getKey(block);
			this.queueMap.set(key, block); // 以坐标去重
		}
	}

	public async update() {
		if (
			this.queueMap.size &&
			performance.now() - this.lastFlushTime >= this.flushInterval &&
			!this.flushing
		) {
			this.flushing = true;
			await this.flushQueue();
			this.flushing = false;
		}
	}

	public async flushImmediately() {
		if (this.flushing) return;
		this.flushing = true;
		await this.flushQueue();
		this.flushing = false;
	}

	private async flushQueue() {
		if (!this.queueMap.size) return;

		const blocks = Array.from(this.queueMap.values());

		// 提前设置 vertexBuilder
		if (this.vertexBuilder) {
			for (const block of blocks) {
				await this.vertexBuilder.setBlock(block.x, block.y, block.z, block.blockId);
			}
		}

		// 通知 world 进行正式更新
		this.world.setBlocks(blocks);

		this.queueMap.clear();
		this.lastFlushTime = performance.now();
	}

	private getKey(data: IBlockActionData): string {
		return `${data.x},${data.y},${data.z}`;
	}
}
