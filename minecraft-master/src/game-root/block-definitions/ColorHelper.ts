import { Color3 } from "@babylonjs/core";
import { useWorldStore } from "@/store";

// 预设草地颜色（春夏秋冬）
const GRASS_COLORS: Readonly<Color3[]> = [
	new Color3(0.7, 0.91, 0.36), // 春：亮绿
	new Color3(0.36, 0.71, 0.19), // 夏：正绿
	new Color3(0.84, 0.61, 0.24), // 秋：土黄
	new Color3(0.85, 0.94, 1.0), // 冬：浅青白
];

// 快速返回草地颜色（无计算）
export function getGrassColor(): Color3 {
	let season: number = useWorldStore.getState().season;
	return GRASS_COLORS[season & 3]; // 等价于 season % 4，性能更好
}

// 原始基础色（春夏秋冬）
const FOLIAGE_BASE_COLORS: Readonly<Color3[]> = [
	new Color3(0.47, 0.78, 0.31), // 春：鲜绿
	new Color3(0.36, 0.71, 0.19), // 夏：深绿
	new Color3(0.8, 0.47, 0.2), // 秋：橙褐
	new Color3(0.94, 0.97, 1.0), // 冬：雪白偏蓝
];

// 缓存扰动结果，每个季节预生成 N 个颜色
const VARIANTS = 6;
const foliageCache: Color3[][] = [];

// 预生成带扰动的颜色
for (let season = 0; season < 4; season++) {
	const base = FOLIAGE_BASE_COLORS[season];
	const list: Color3[] = [];
	for (let i = 0; i < VARIANTS; i++) {
		const offset = 0.9 + (i / (VARIANTS - 1)) * 0.2; // [0.9, 1.1]
		list.push(
			new Color3(
				Math.min(1, base.r * offset),
				Math.min(1, base.g * offset),
				Math.min(1, base.b * offset)
			)
		);
	}
	foliageCache[season] = list;
}

// 快速返回扰动后的颜色（只索引，不计算）
export function getFoliageColor(value: number): Color3 {
	let season: number = useWorldStore.getState().season;
	const s = season & 3;
	const v = value & (VARIANTS - 1); // 快速模 VARIANTS（假设 VARIANTS 为 2 的幂）
	return foliageCache[s][v];
}
