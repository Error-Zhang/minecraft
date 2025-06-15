import { CrossRender, CubeRender } from "@engine/types/block.type.ts";
import { BlockCoder } from "@/game-root/block-definitions/BlockCoder.ts";
import { getFoliageColor } from "@/game-root/block-definitions/ColorHelper.ts";

type CubeGetters = Pick<CubeRender, "getUv" | "getColor" | "getRotation">;
type CrossGetters = Pick<CrossRender, "getStage" | "getColor">;

// 标签分类
export const TAGS = {
	NATURE: {
		PLANT: "植物",
		LEAVES: "树叶",
		Wood: "木头",
	},
} as const;

// 预定义的标签 getter
export const TAG_GETTERS: {
	[tag: string]: {
		cube?: CubeGetters;
		cross?: CrossGetters;
	};
} = {
	[TAGS.NATURE.PLANT]: {
		cross: {
			getStage: BlockCoder.decodePlantSize.bind(BlockCoder),
		},
	},
	[TAGS.NATURE.LEAVES]: {
		cube: {
			getColor(value) {
				return getFoliageColor(value);
			},
		},
	},
	[TAGS.NATURE.Wood]: {
		cube: {
			getRotation: BlockCoder.decodeWoodDirection.bind(BlockCoder),
		},
	},
};
