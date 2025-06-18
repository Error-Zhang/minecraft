import BlockType from "@/game-root/block-definitions/BlockType.ts";

export type BlockRecipeItem = string | number;

export type BlockRecipe = {
	pattern: (BlockRecipeItem | null)[][]; // 排列
	output: {
		item: BlockRecipeItem;
		count: number;
	}; // 主产物
	byproducts?: {
		item: BlockRecipeItem;
		count: number;
	}[]; // 副产物
	allowPlayerLevel?: number;
	allowCrafts?: BlockRecipeItem[]; // 允许使用的工作台
	allowMirrored?: boolean; // 是否允许左右镜像合成（默认 false）
};

export const blockRecipes: BlockRecipe[] = [
	// 木板：1 个原木 → 4 块木板
	{
		pattern: [[BlockType.OakWoodBlock]],
		output: { item: BlockType.OakPlankBlock, count: 4 },
	},

	// 工作台：4 个木板 → 1 个工作台
	{
		pattern: [
			[BlockType.OakPlankBlock, BlockType.OakPlankBlock],
			[BlockType.OakPlankBlock, BlockType.OakPlankBlock],
		],
		output: { item: BlockType.CraftTableBlock, count: 1 },
	},
];

function itemToNamespaced(item: BlockRecipeItem, namespace: string): string {
	if (typeof item === "number") {
		const enumName = BlockType[item];
		if (!enumName) throw new Error(`Invalid BlockType enum value: ${item}`);
		return `${namespace}:${enumName}`;
	}
	return `${namespace}:${item}`;
}

export function getBlockRecipes(
	namespace: string,
	recipes: BlockRecipe[]
): Record<string, BlockRecipe[]> {
	return recipes.reduce(
		(acc, recipe) => {
			// 转换 output.item 为 key
			const key = itemToNamespaced(recipe.output.item, namespace);

			// 深拷贝整个 recipe 并转换其中所有 BlockRecipeItem
			const converted: BlockRecipe = {
				pattern: recipe.pattern.map(row =>
					row.map(item => (item != null ? itemToNamespaced(item, namespace) : null))
				),
				output: {
					item: key,
					count: recipe.output.count,
				},
				byproducts: recipe.byproducts?.map(b => ({
					item: itemToNamespaced(b.item, namespace),
					count: b.count,
				})),
				allowPlayerLevel: recipe.allowPlayerLevel,
				allowCrafts: recipe.allowCrafts?.map(item => itemToNamespaced(item, namespace)),
				allowMirrored: recipe.allowMirrored ?? false,
			};

			// 填入结果
			if (!acc[key]) acc[key] = [];
			acc[key].push(converted);
			return acc;
		},
		{} as Record<string, BlockRecipe[]>
	);
}

export const getBlockName = (key: BlockRecipeItem) => {
	const [namespace, blockType, props] = (key as string).split(":");
	return `${namespace}:${blockType}`;
};
