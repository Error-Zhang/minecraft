import { BlockDefinition, TransparencyType } from "@/engine/types/block.type";
import { BlockBuilder } from "./BlockBuilder";
import BlockType from "./BlockType";
import { BlockMaterialManager } from "@/engine/renderer/BlockMaterialManager";
import Assets from "@/game-root/assets";
import { TAGS } from "@/game-root/block-definitions/BlockTags.ts";
import { BlockCoder } from "@/game-root/block-definitions/BlockCoder.ts";
import { getGrassColor } from "@/game-root/block-definitions/ColorHelper.ts";
import { Color3 } from "@babylonjs/core";
import { IBlockReflect } from "@/ui-root/api/interface.ts";
import { interactEvents } from "@/game-root/core/events.ts";

// 方块定义
export const blocks: BlockDefinition<Record<string, any>>[] = [
	// 基础方块
	new BlockBuilder(BlockType.GrassBlock, "草方块", 1)

		.withMetaData({
			displayName: "草方块",
			maxStackCount: 64,
			hardness: 0.6,
			toolType: "shovel",
		})
		.withCubeGetters({
			getColor(value, face, envValue) {
				if (face === 4) {
					return getGrassColor();
				}
			},
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.SOLID)
		.build(),

	new BlockBuilder(BlockType.DirtBlock, "土方块", 2)

		.withMetaData({
			displayName: "土方块",
			maxStackCount: 64,
			hardness: 0.5,
			toolType: "shovel",
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.SOLID)
		.build(),

	new BlockBuilder(BlockType.ClayBlock, "粘土块")

		.withMetaData({
			displayName: "粘土块",
			maxStackCount: 64,
			hardness: 0.6,
			toolType: "shovel",
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.SOLID)
		.build(),

	// 树叶方块
	new BlockBuilder(BlockType.OakLeavesBlock, "橡树叶")
		.withTags(TAGS.NATURE.LEAVES)
		.withMetaData({
			displayName: "橡树叶",
			maxStackCount: 64,
			hardness: 0.2,
			toolType: "shears",
			flammable: true,
		})
		.asCube(TransparencyType.Cutout, BlockMaterialManager.PRESET_MATERIALS.LEAVES)
		.build(),

	new BlockBuilder(BlockType.BirchLeavesBlock, "桦树叶")
		.withTags(TAGS.NATURE.LEAVES)
		.withMetaData({
			displayName: "桦树叶",
			maxStackCount: 64,
			hardness: 0.2,
			toolType: "shears",
			flammable: true,
		})
		.asCube(TransparencyType.Cutout, BlockMaterialManager.PRESET_MATERIALS.LEAVES)
		.build(),

	// 透明方块
	new BlockBuilder(BlockType.WaterBlock, "水")
		.withMetaData({
			displayName: "水",
			maxStackCount: 1,
			flowable: true,
			viscosity: 0.8,
		})
		.asCube(TransparencyType.Transparent, BlockMaterialManager.PRESET_MATERIALS.WATER)
		.build(),

	new BlockBuilder(BlockType.MagmaBlock, "岩浆")
		.withMetaData({
			displayName: "岩浆",
			maxStackCount: 1,
			flowable: true,
			viscosity: 0.3,
			damage: 4,
		})
		.asCube(TransparencyType.Transparent, BlockMaterialManager.PRESET_MATERIALS.LAVA)
		.build(),

	// 十字形方块
	new BlockBuilder(BlockType.TallGrassBlock, "高草")
		.withTags(TAGS.NATURE.PLANT)
		.withMetaData({
			displayName: "高草",
			maxStackCount: 64,
			hardness: 0,
			toolType: "shears",
			flammable: true,
		})
		.withCrossGetters({
			getColor(_, __, envValue) {
				return getGrassColor();
			},
		})
		.asCross()
		.build(),

	new BlockBuilder(BlockType.RedFlowerBlock, "红花")
		.withTags(TAGS.NATURE.PLANT)
		.withMetaData({
			displayName: "红花",
			maxStackCount: 64,
			hardness: 0,
			flammable: true,
		})
		.asCross()
		.build(),

	// 模型方块
	new BlockBuilder(BlockType.CactusBlock, "仙人掌")
		.withMetaData({
			displayName: "仙人掌",
			maxStackCount: 64,
			hardness: 0.4,
			damage: 1,
			flammable: true,
		})
		.asModel(Assets.blocks.models.Cactus)
		.build(),

	new BlockBuilder(BlockType.CraftTableBlock, "工作台")
		.withMetaData({
			displayName: "工作台",
			maxStackCount: 1,
			hardness: 2.5,
			toolType: "axe",
			interactable: true,
		})
		.withBehavior({
			onInteract() {
				interactEvents.emit("CraftTableBlock");
			},
		})
		.asModel(Assets.blocks.models.CraftTable)
		.build(),

	// 矿石类方块
	new BlockBuilder(BlockType.CoalOreBlock, "煤矿")

		.withMetaData({
			displayName: "煤矿",
			maxStackCount: 64,
			hardness: 3.0,
			toolType: "pickaxe",
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.SOLID)
		.build(),

	new BlockBuilder(BlockType.IronOreBlock, "铁矿")

		.withMetaData({
			displayName: "铁矿",
			maxStackCount: 64,
			hardness: 3.0,
			toolType: "pickaxe",
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.METAL)
		.build(),

	new BlockBuilder(BlockType.CopperOreBlock, "铜矿")

		.withMetaData({
			displayName: "铜矿",
			maxStackCount: 64,
			hardness: 3.0,
			toolType: "pickaxe",
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.METAL)
		.build(),

	new BlockBuilder(BlockType.SaltpeterOreBlock, "硝石矿")

		.withMetaData({
			displayName: "硝石矿",
			maxStackCount: 64,
			hardness: 2.5,
			toolType: "pickaxe",
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.SOLID)
		.build(),

	new BlockBuilder(BlockType.SulphurOreBlock, "硫磺矿")

		.withMetaData({
			displayName: "硫磺矿",
			maxStackCount: 64,
			hardness: 2.5,
			toolType: "pickaxe",
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.SOLID)
		.build(),

	new BlockBuilder(BlockType.DiamondOreBlock, "钻石矿")

		.withMetaData({
			displayName: "钻石矿",
			maxStackCount: 64,
			hardness: 3.0,
			toolType: "pickaxe",
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.METAL)
		.build(),

	new BlockBuilder(BlockType.GermaniumOreBlock, "锗矿")

		.withMetaData({
			displayName: "锗矿",
			maxStackCount: 64,
			hardness: 3.0,
			toolType: "pickaxe",
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.METAL)
		.build(),

	// 石头类方块
	new BlockBuilder(BlockType.SandBlock, "沙子")

		.withMetaData({
			displayName: "沙子",
			maxStackCount: 64,
			hardness: 0.5,
			toolType: "shovel",
			gravity: true,
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.SOLID)
		.build(),

	new BlockBuilder(BlockType.SandStoneBlock, "砂岩")

		.withMetaData({
			displayName: "砂岩",
			maxStackCount: 64,
			hardness: 0.8,
			toolType: "pickaxe",
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.SOLID)
		.build(),

	new BlockBuilder(BlockType.GraniteStoneBlock, "花岗岩")

		.withMetaData({
			displayName: "花岗岩",
			maxStackCount: 64,
			hardness: 1.5,
			toolType: "pickaxe",
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.SOLID)
		.build(),

	new BlockBuilder(BlockType.BasaltStoneBlock, "玄武岩")

		.withMetaData({
			displayName: "玄武岩",
			maxStackCount: 64,
			hardness: 1.5,
			toolType: "pickaxe",
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.SOLID)
		.build(),

	new BlockBuilder(BlockType.GravelStoneBlock, "沙砾")

		.withMetaData({
			displayName: "沙砾",
			maxStackCount: 64,
			hardness: 0.6,
			toolType: "shovel",
			gravity: true,
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.SOLID)
		.build(),

	new BlockBuilder(BlockType.LimeStoneBlock, "石灰石")

		.withMetaData({
			displayName: "石灰石",
			maxStackCount: 64,
			hardness: 1.5,
			toolType: "pickaxe",
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.SOLID)
		.build(),

	// 树木类方块
	new BlockBuilder(BlockType.SpruceWoodBlock, "云杉木")
		.withTags(TAGS.NATURE.Wood)
		.withMetaData({
			displayName: "云杉木",
			maxStackCount: 64,
			hardness: 2.0,
			toolType: "axe",
			flammable: true,
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.SOLID)
		.build(),

	new BlockBuilder(BlockType.MimosaWoodBlock, "金合欢木")
		.withTags(TAGS.NATURE.Wood)
		.withMetaData({
			displayName: "金合欢木",
			maxStackCount: 64,
			hardness: 2.0,
			toolType: "axe",
			flammable: true,
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.SOLID)
		.build(),

	new BlockBuilder(BlockType.PoplarWoodBlock, "白杨木")
		.withTags(TAGS.NATURE.Wood)
		.withMetaData({
			displayName: "白杨木",
			maxStackCount: 64,
			hardness: 2.0,
			toolType: "axe",
			flammable: true,
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.SOLID)
		.build(),

	new BlockBuilder(BlockType.SpruceLeavesBlock, "云杉树叶")
		.withTags(TAGS.NATURE.LEAVES)
		.withMetaData({
			displayName: "云杉树叶",
			maxStackCount: 64,
			hardness: 0.2,
			toolType: "shears",
			flammable: true,
		})
		.asCube(TransparencyType.Cutout, BlockMaterialManager.PRESET_MATERIALS.LEAVES)
		.build(),

	new BlockBuilder(BlockType.TallSpruceLeavesBlock, "高云杉树叶")
		.withTags(TAGS.NATURE.LEAVES)
		.withMetaData({
			displayName: "高云杉树叶",
			maxStackCount: 64,
			hardness: 0.2,
			toolType: "shears",
			flammable: true,
		})
		.asCube(TransparencyType.Cutout, BlockMaterialManager.PRESET_MATERIALS.LEAVES)
		.build(),

	new BlockBuilder(BlockType.MimosaLeavesBlock, "金合欢树叶")
		.withTags(TAGS.NATURE.LEAVES)
		.withMetaData({
			displayName: "金合欢树叶",
			maxStackCount: 64,
			hardness: 0.2,
			toolType: "shears",
			flammable: true,
		})
		.asCube(TransparencyType.Cutout, BlockMaterialManager.PRESET_MATERIALS.LEAVES)
		.build(),

	new BlockBuilder(BlockType.PoplarLeavesBlock, "白杨树叶")
		.withTags(TAGS.NATURE.LEAVES)
		.withMetaData({
			displayName: "白杨树叶",
			maxStackCount: 64,
			hardness: 0.2,
			toolType: "shears",
			flammable: true,
		})
		.asCube(TransparencyType.Cutout, BlockMaterialManager.PRESET_MATERIALS.LEAVES)
		.build(),

	// 其他植物方块
	new BlockBuilder(BlockType.PurpleFlowerBlock, "紫花")
		.withTags(TAGS.NATURE.PLANT)
		.withTags(TAGS.NATURE.LEAVES)
		.withMetaData({
			displayName: "紫花",
			maxStackCount: 64,
			hardness: 0,
			flammable: true,
		})
		.asCross()
		.build(),

	new BlockBuilder(BlockType.WhiteFlowerBlock, "白花")
		.withTags(TAGS.NATURE.PLANT)
		.withMetaData({
			displayName: "白花",
			maxStackCount: 64,
			hardness: 0,
			flammable: true,
		})
		.asCross()
		.build(),

	new BlockBuilder(BlockType.RyeBlock, "黑麦")
		.withTags(TAGS.NATURE.PLANT)
		.withMetaData({
			displayName: "黑麦",
			maxStackCount: 64,
			hardness: 0,
			toolType: "shears",
			flammable: true,
		})
		.withCrossGetters({
			getColor(value) {
				if (BlockCoder.decodePlantIsWild(value)) {
					return Color3.Green();
				}
			},
			getStage(value) {
				return BlockCoder.decodePlantSize(value);
			},
		})
		.asCross()
		.build(),

	new BlockBuilder(BlockType.CottonBlock, "棉花")
		.withTags(TAGS.NATURE.PLANT)
		.withMetaData({
			displayName: "棉花",
			maxStackCount: 64,
			hardness: 0,
			toolType: "shears",
			flammable: true,
		})
		.asCross()
		.build(),

	new BlockBuilder(BlockType.DryBushBlock, "干枯灌木")
		.withMetaData({
			displayName: "干枯灌木",
			maxStackCount: 64,
			hardness: 0,
			toolType: "shears",
			flammable: true,
		})
		.asCross()
		.build(),

	new BlockBuilder(BlockType.LargeDryBushBlock, "大型干枯灌木")
		.withMetaData({
			displayName: "大型干枯灌木",
			maxStackCount: 64,
			hardness: 0,
			toolType: "shears",
			flammable: true,
		})
		.asCross()
		.build(),

	// 其他方块
	new BlockBuilder(BlockType.IceBlock, "冰块")
		.withMetaData({
			displayName: "冰块",
			maxStackCount: 64,
			hardness: 0.5,
			toolType: "pickaxe",
			slippery: true,
		})
		.asCube(TransparencyType.Transparent, BlockMaterialManager.PRESET_MATERIALS.SOLID)
		.build(),

	new BlockBuilder(BlockType.CoalBlock, "纯煤矿")
		.withMetaData({
			displayName: "纯煤矿",
			maxStackCount: 64,
			hardness: 5.0,
			toolType: "pickaxe",
			flammable: true,
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.SOLID)
		.build(),

	new BlockBuilder(BlockType.OakPlankBlock, "橡木木板")
		.withMetaData({
			displayName: "橡木木板",
			maxStackCount: 64,
			hardness: 2.0,
			toolType: "axe",
			flammable: true,
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.SOLID)
		.build(),

	new BlockBuilder(BlockType.OakWoodBlock, "橡树木块")
		.withTags(TAGS.NATURE.Wood)
		.withMetaData({
			displayName: "橡树木块",
			maxStackCount: 64,
			hardness: 2.0,
			toolType: "axe",
			flammable: true,
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.SOLID)
		.build(),

	new BlockBuilder(BlockType.BirchWoodBlock, "桦树木块")
		.withTags(TAGS.NATURE.Wood)
		.withMetaData({
			displayName: "桦树木块",
			maxStackCount: 64,
			hardness: 2.0,
			toolType: "axe",
			flammable: true,
		})
		.asCube(TransparencyType.Opaque, BlockMaterialManager.PRESET_MATERIALS.SOLID)
		.build(),
];

const getBlocksMap = () =>
	blocks.reduce(
		(acc, cur) => {
			acc[cur.blockType] = cur;
			return acc;
		},
		{} as Record<string, BlockDefinition<any>>
	);
export const getCombinedBlocks = (blockTypes: IBlockReflect) => {
	const blocksMap = getBlocksMap();
	Object.entries(blockTypes.byName).forEach(([blockType, blockId]) => {
		if (blockId === 0) return;
		if (!blocksMap[blockType]) {
			throw new Error(`Block ${blockType} was not defined in website.`);
		}
	});
	return blocks.map(block => ({
		...block,
		id: blockTypes.byName[block.blockType] ?? block.id,
	}));
};
