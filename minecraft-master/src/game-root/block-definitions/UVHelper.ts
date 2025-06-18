import { Vector4 } from "@babylonjs/core";

class UVHelper {
	constructor(
		private readonly tilesX = 16,
		private readonly tilesY = 16,
		private readonly padding = 0.0001
	) {}

	/**
	 * 获取指定贴图块的 UV 向量
	 * @param uv
	 */
	getUV(uv: [number, number]): Vector4 {
		const [x, y] = uv;
		const tileSizeX = 1 / this.tilesX;
		const tileSizeY = 1 / this.tilesY;

		return new Vector4(
			x * tileSizeX + this.padding,
			y * tileSizeY + this.padding,
			(x + 1) * tileSizeX - this.padding,
			(y + 1) * tileSizeY - this.padding
		);
	}

	/**
	 * 快速生成 6 个面的 faceUV 向量数组
	 * @param all 所有面统一使用的贴图位置
	 */
	uniform(all: [number, number]): Vector4[] {
		const uv = this.getUV(all);
		return [uv, uv, uv, uv, uv, uv];
	}

	/**
	 * 分别为顶面、底面、侧面指定贴图位置
	 * @param top 顶面贴图坐标 [x, y]
	 * @param bottom 底面贴图坐标 [x, y]
	 * @param side 侧面贴图坐标 [x, y]
	 */
	topBottomSide(
		top: [number, number],
		bottom: [number, number],
		side: [number, number]
	): Vector4[] {
		const topUV = this.getUV(top);
		const bottomUV = this.getUV(bottom);
		const sideUV = this.getUV(side);

		return [sideUV, sideUV, sideUV, sideUV, topUV, bottomUV];
	}

	/**
	 * 获取从 (x, y) 开始、宽高为 (w, h) 的区域的 UV 向量
	 * @param start 起始坐标 [x, y]（左上角为 (0, 0)）
	 * @param size 区域尺寸 [w, h]
	 */
	region(start: [number, number], size: [number, number]): Vector4 {
		const [x, y] = start;
		const [w, h] = size;
		const tileSizeX = 1 / this.tilesX;
		const tileSizeY = 1 / this.tilesY;

		return new Vector4(x * tileSizeX, y * tileSizeY, (x + w) * tileSizeX, (y + h) * tileSizeY);
	}
}

export default UVHelper;
