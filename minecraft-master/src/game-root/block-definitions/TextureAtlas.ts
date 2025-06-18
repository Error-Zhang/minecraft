import BlockType from "@/game-root/block-definitions/BlockType.ts";
import UVHelper from "./UVHelper.ts";
import { Vector4 } from "@babylonjs/core";

const uvHelper = new UVHelper();

export const blocksUvTable: Record<
	BlockType,
	{
		faceUvs?: Vector4[];
		stageUvs?: Vector4[];
		regionUv?: Vector4;
		modelUvs?: Vector4[];
	}
> = {
	[BlockType.GrassBlock]: {
		faceUvs: uvHelper.topBottomSide([0, 0], [2, 0], [3, 0]),
	},
	[BlockType.DirtBlock]: {
		faceUvs: uvHelper.uniform([2, 0]),
	},
	[BlockType.OakPlankBlock]: {
		faceUvs: uvHelper.uniform([4, 0]),
	},
	[BlockType.GraniteStoneBlock]: {
		faceUvs: uvHelper.uniform([1, 0]),
	},
	[BlockType.BasaltStoneBlock]: {
		faceUvs: uvHelper.uniform([6, 0]),
	},
	[BlockType.WaterBlock]: {
		regionUv: uvHelper.region([12, 10], [3, 3]),
		faceUvs: uvHelper.uniform([12, 10]),
	},
	[BlockType.MagmaBlock]: {
		regionUv: uvHelper.region([12, 7], [3, 3]),
		faceUvs: uvHelper.uniform([12, 7]),
	},
	[BlockType.ClayBlock]: {
		faceUvs: uvHelper.uniform([8, 0]),
	},
	[BlockType.IceBlock]: {
		faceUvs: uvHelper.uniform([1, 4]),
	},
	[BlockType.TallGrassBlock]: {
		stageUvs: [uvHelper.getUV([4, 5]), uvHelper.getUV([5, 5])],
	},
	[BlockType.RedFlowerBlock]: {
		stageUvs: [uvHelper.getUV([11, 0]), uvHelper.getUV([12, 0])],
	},
	[BlockType.PurpleFlowerBlock]: {
		stageUvs: [uvHelper.getUV([11, 0]), uvHelper.getUV([13, 0])],
	},
	[BlockType.WhiteFlowerBlock]: {
		stageUvs: [uvHelper.getUV([11, 0]), uvHelper.getUV([14, 0])],
	},
	[BlockType.RyeBlock]: {
		stageUvs: Array.from({ length: 8 }).map((_, i) => uvHelper.getUV([i + 8, 5])),
	},
	[BlockType.CottonBlock]: {
		stageUvs: [uvHelper.getUV([12, 1]), uvHelper.getUV([13, 1]), uvHelper.getUV([14, 1])],
	},
	[BlockType.DryBushBlock]: {
		stageUvs: [uvHelper.getUV([7, 3])],
	},
	[BlockType.LargeDryBushBlock]: {
		stageUvs: [uvHelper.getUV([7, 4])],
	},
	[BlockType.CactusBlock]: {
		modelUvs: [uvHelper.getUV([5, 6]), uvHelper.getUV([4, 6])],
	},
	[BlockType.SandBlock]: {
		faceUvs: uvHelper.uniform([2, 1]),
	},
	[BlockType.SandStoneBlock]: {
		faceUvs: uvHelper.uniform([0, 11]),
	},
	[BlockType.GravelStoneBlock]: {
		faceUvs: uvHelper.uniform([3, 1]),
	},
	[BlockType.LimeStoneBlock]: {
		faceUvs: uvHelper.uniform([5, 0]),
	},
	[BlockType.CoalBlock]: {
		faceUvs: uvHelper.uniform([14, 3]),
	},
	[BlockType.CoalOreBlock]: {
		faceUvs: uvHelper.uniform([2, 2]),
	},
	[BlockType.IronOreBlock]: {
		faceUvs: uvHelper.uniform([1, 2]),
	},
	[BlockType.CopperOreBlock]: {
		faceUvs: uvHelper.uniform([0, 2]),
	},
	[BlockType.SaltpeterOreBlock]: {
		faceUvs: uvHelper.uniform([3, 2]),
	},
	[BlockType.SulphurOreBlock]: {
		faceUvs: uvHelper.uniform([4, 2]),
	},
	[BlockType.DiamondOreBlock]: {
		faceUvs: uvHelper.uniform([0, 3]),
	},
	[BlockType.GermaniumOreBlock]: {
		faceUvs: uvHelper.uniform([1, 3]),
	},
	[BlockType.OakWoodBlock]: {
		faceUvs: uvHelper.topBottomSide([5, 1], [5, 1], [4, 1]),
	},
	[BlockType.BirchWoodBlock]: {
		faceUvs: uvHelper.topBottomSide([5, 1], [5, 1], [5, 7]),
	},
	[BlockType.SpruceWoodBlock]: {
		faceUvs: uvHelper.topBottomSide([5, 1], [5, 1], [4, 7]),
	},
	[BlockType.MimosaWoodBlock]: {
		faceUvs: uvHelper.topBottomSide([5, 1], [5, 1], [4, 7]), // ?
	},
	[BlockType.PoplarWoodBlock]: {
		faceUvs: uvHelper.topBottomSide([5, 1], [5, 1], [13, 6]),
	},
	[BlockType.OakLeavesBlock]: {
		faceUvs: uvHelper.uniform([4, 3]),
	},
	[BlockType.BirchLeavesBlock]: {
		faceUvs: uvHelper.uniform([4, 3]),
	},
	[BlockType.SpruceLeavesBlock]: {
		faceUvs: uvHelper.uniform([4, 8]),
	},
	[BlockType.TallSpruceLeavesBlock]: {
		faceUvs: uvHelper.uniform([4, 8]),
	},
	[BlockType.MimosaLeavesBlock]: {
		faceUvs: uvHelper.uniform([4, 8]),
	},
	[BlockType.PoplarLeavesBlock]: {
		faceUvs: uvHelper.uniform([4, 3]),
	},
	[BlockType.CraftTableBlock]: {
		modelUvs: [uvHelper.getUV([11, 2])],
	},
};
