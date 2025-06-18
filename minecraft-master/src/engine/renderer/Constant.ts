export const FaceDirectionOffset: [number, number, number][] = [
	[0, 0, 1],
	[0, 0, -1],
	[1, 0, 0],
	[-1, 0, 0],
	[0, 1, 0],
	[0, -1, 0],
];

export const FaceVertices: [number, number, number][][] = [
	[
		[0, 0, 1],
		[1, 0, 1],
		[1, 1, 1],
		[0, 1, 1],
	],
	[
		[1, 0, 0],
		[0, 0, 0],
		[0, 1, 0],
		[1, 1, 0],
	],
	[
		[1, 0, 1],
		[1, 0, 0],
		[1, 1, 0],
		[1, 1, 1],
	],
	[
		[0, 0, 0],
		[0, 0, 1],
		[0, 1, 1],
		[0, 1, 0],
	],
	[
		[0, 1, 1],
		[1, 1, 1],
		[1, 1, 0],
		[0, 1, 0],
	],
	[
		[0, 0, 0],
		[1, 0, 0],
		[1, 0, 1],
		[0, 0, 1],
	],
];
export const EdgeConfigs = [
	{
		edge: 0, // 左边界 (-X)
		dx: -1,
		dz: 0,
		getCoords: (i: number, y: number, size: number) => [0, y, i],
	},
	{
		edge: 1, // 右边界 (+X)
		dx: 1,
		dz: 0,
		getCoords: (i: number, y: number, size: number) => [size - 1, y, i],
	},
	{
		edge: 2, // 下边界 (-Z)
		dx: 0,
		dz: -1,
		getCoords: (i: number, y: number, size: number) => [i, y, 0],
	},
	{
		edge: 3, // 上边界 (+Z)
		dx: 0,
		dz: 1,
		getCoords: (i: number, y: number, size: number) => [i, y, size - 1],
	},
];
export const FaceMap = {
	0: [0, 1, 2, 3, 4, 5],
	1: [4, 5, 2, 3, 0, 1],
	2: [0, 1, 4, 5, 2, 3],
};
// 基础 AO 定义：以 +Z 面（前方向）逆时针定义 4 个顶点的 AO 采样点
const baseAO: [[number, number, number], [number, number, number], [number, number, number]][] = [
	[
		[-1, -1, 1],
		[-1, 0, 1],
		[0, -1, 1],
	], // bottom-left
	[
		[1, -1, 1],
		[1, 0, 1],
		[0, -1, 1],
	], // bottom-right
	[
		[1, 1, 1],
		[1, 0, 1],
		[0, 1, 1],
	], // top-right
	[
		[-1, 1, 1],
		[-1, 0, 1],
		[0, 1, 1],
	], // top-left
];

// 各面旋转偏移函数（将 Z+ 方向的 AO 映射到各方向）
function rotateOffset(
	[x, y, z]: [number, number, number],
	faceIndex: number
): [number, number, number] {
	switch (faceIndex) {
		case 0:
			return [x, y, z]; // +Z
		case 1:
			return [-x, y, -z]; // -Z
		case 2:
			return [z, y, -x]; // +X
		case 3:
			return [-z, y, x]; // -X
		case 4:
			return [x, z, -y]; // +Y (swap y/z)
		case 5:
			return [x, -z, y]; // -Y (swap y/z and flip)
		default:
			throw new Error("Invalid face index");
	}
}

// 构造最终 AOOffsets 映射
export const AOOffsets: Record<
	number,
	[
		[[number, number, number], [number, number, number], [number, number, number]],
		[[number, number, number], [number, number, number], [number, number, number]],
		[[number, number, number], [number, number, number], [number, number, number]],
		[[number, number, number], [number, number, number], [number, number, number]],
	]
> = [0, 1, 2, 3, 4, 5].reduce(
	(acc, faceIndex) => {
		acc[faceIndex] = baseAO.map(
			([corner, side1, side2]) =>
				[
					rotateOffset(corner, faceIndex),
					rotateOffset(side1, faceIndex),
					rotateOffset(side2, faceIndex),
				] as [[number, number, number], [number, number, number], [number, number, number]]
		) as (typeof AOOffsets)[0];
		return acc;
	},
	{} as typeof AOOffsets
);
export const getCrossPositions = (x: number, y: number, z: number) => [
	// plane 1 (/)
	[x, y + 1, z + 1],
	[x + 1, y + 1, z],
	[x + 1, y, z],
	[x, y, z + 1],
	// plane 2 (\)
	[x, y + 1, z],
	[x + 1, y + 1, z + 1],
	[x + 1, y, z + 1],
	[x, y, z],
];
