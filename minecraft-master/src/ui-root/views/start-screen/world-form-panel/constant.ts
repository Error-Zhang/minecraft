export const gameModeOptions = [
	{ label: "创造模式", value: 0 },
	{ label: "生存模式", value: 1 },
];
export const worldModeOptions = [
	{ label: "大陆", value: 0 },
	{ label: "岛屿", value: 1 },
	{ label: "平坦", value: 2 },
];
export const seasonOptions = [
	{ label: "春季", value: 0 },
	{ label: "夏季", value: 1 },
	{ label: "秋季", value: 2 },
	{ label: "冬季", value: 3 },
];
export const publicOptions = [
	{ label: "私人", value: 0 },
	{ label: "公开", value: 1 },
];
export const worldViewOptions = [
	{ label: "私人", value: 0 },
	{ label: "公开", value: 1 },
	{ label: "全部", value: 2 },
];

export function getLabelByValue(options: { label: string; value: any }[], value: any): string {
	const found = options.find(opt => opt.value === value);
	return found ? found.label : "";
}

export function generateRandomWorldName(): string {
	const adjectives = [
		"神秘",
		"远古",
		"荒芜",
		"翠绿",
		"蔚蓝",
		"燃烧",
		"迷雾",
		"幽暗",
		"静谧",
		"狂野",
	];
	const nouns = ["平原", "岛屿", "森林", "山脉", "大陆", "峡谷", "荒原", "冰川", "沙漠"];
	const suffix = Math.floor(Math.random() * 1000);
	return `${adjectives[Math.floor(Math.random() * adjectives.length)]}${nouns[Math.floor(Math.random() * nouns.length)]}-${suffix}`;
}

export function generateRandomSeed(): string {
	return (Math.floor(Math.random() * 2_000_000_000) - 1_000_000_000).toString(16);
}
