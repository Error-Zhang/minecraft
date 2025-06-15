import React, { useEffect, useState } from "react";
import CraftingPanel from "@/ui-root/views/craft-table/CraftingPanel.tsx";
import {
	BlockRecipe,
	blockRecipes,
	getBlockRecipes,
} from "@/game-root/block-definitions/BlockRecipes.ts";
import { useBlockStore } from "@/store";

const CraftTable: React.FC<{ guid: string }> = ({ guid }) => {
	const [recipes, setRecipes] = useState<Record<string, BlockRecipe[]>>({} as any);

	useEffect(() => {
		if (!useBlockStore.getState().blockRecipes) {
			const namespace = useBlockStore.getState().blockRegistry!.DEFAULT_NAMESPACE;
			const runtimeRecipes = getBlockRecipes(namespace, blockRecipes);
			useBlockStore.setState({ blockRecipes: runtimeRecipes });
		}
		setRecipes(useBlockStore.getState().blockRecipes!);
	}, []);
	return <CraftingPanel title="工作台" guid={guid} rows={3} columns={3} recipes={recipes} />;
};

export default CraftTable;
