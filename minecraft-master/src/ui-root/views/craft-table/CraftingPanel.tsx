import React, { useEffect, useRef, useState } from "react";
import Slot, { createEmptySlots, DroppedSlotType, SlotType } from "../../components/slot";
import InventoryGrid from "@/ui-root/components/inventory-grid";
import { matchesPattern } from "@/ui-root/views/craft-table/match.ts";
import { useInventorySlots } from "@/ui-root/components/inventory-grid/useInventorySlots.tsx";
import "./index.less";
import { BlockRecipe, getBlockName } from "@/game-root/block-definitions/BlockRecipes.ts";
import { useBlockStore } from "@/store";

interface CraftingPanelProps {
	title?: string;
	guid: string;
	rows: number;
	columns: number;
	recipes: Record<string, BlockRecipe[]>;
}

const CraftingPanel: React.FC<CraftingPanelProps> = ({ title, guid, rows, columns, recipes }) => {
	const [gridSlots, setGridSlots] = useState<SlotType[]>(createEmptySlots(rows * columns));
	const [resultSlot, setResultSlot] = useState<SlotType[]>(createEmptySlots(1));
	const dropFinish = useRef(false);

	function reshape<T>(array: T[]): T[][] {
		const result: T[][] = [];
		for (let i = 0; i < array.length; i += columns) {
			result.push(array.slice(i, i + columns));
		}
		return result;
	}

	const updateResultSlot = () => {
		const result = Object.entries(recipes)
			.flatMap(([block, recipeList]) => {
				return recipeList.map(recipe => ({
					block: block,
					recipe,
				}));
			})
			.find(({ recipe }) =>
				matchesPattern(reshape(gridSlots.map(item => (item ? item.key : null))), recipe)
			);
		if (result) {
			const output = result.recipe.output;
			const blockRegistry = useBlockStore.getState().blockRegistry!;
			const outputName = getBlockName(output.item);
			const def = blockRegistry.getByName(outputName)!;

			setResultSlot([
				{
					id: def.id || 0,
					key: outputName,
					displayName: def.metaData.displayName as string,
					value: output.count,
					icon: useBlockStore.getState().blockIcons![outputName],
					source: "CraftTableResult",
				},
			]);
		} else {
			setResultSlot([null]);
		}
	};

	const updateTable = () => {
		if (dropFinish.current) {
			dropFinish.current = false;
			return;
		}

		gridSlots.forEach((slot: SlotType) => {
			slot && slot.value--;
		});
		setGridSlots([...gridSlots]);
	};

	useEffect(() => {
		// 让其在最后执行，防止逻辑碰撞导致的不确定性
		setTimeout(() => {
			updateResultSlot();
		}, 0);
	}, [gridSlots]);

	const onDropTable = (droppedSlot: DroppedSlotType, current: number) => {
		if (droppedSlot?.source === "CraftTableResult") {
			if (gridSlots[current] && gridSlots[current].value - 1 > 0) return true;
			gridSlots.forEach((slot: SlotType) => {
				slot && slot.value--;
			});
			gridSlots[current] = { ...droppedSlot, source: "CraftTable" }; // 最好不要忘记换源
			dropFinish.current = true;
			return true;
		}
		return false;
	};

	const { setDroppedIndex, onDrop: onDropResult } = useInventorySlots(
		"CraftTableResult",
		resultSlot,
		setResultSlot,
		{ onDropOver: updateTable }
	);

	return (
		<div className="crafting-panel">
			<span className="panel-title">{title}</span>
			<div className="crafting">
				<InventoryGrid
					source="CraftTable"
					slots={gridSlots}
					setSlots={setGridSlots}
					onDrop={onDropTable}
					columns={columns}
					rows={rows}
				/>
				<div className="crafting-arrow">→</div>
				<Slot
					slot={resultSlot[0]}
					draggable
					onDragStart={() => setDroppedIndex(0)}
					onDrop={droppedSlot => onDropResult(droppedSlot, 0)}
				/>
			</div>
		</div>
	);
};

export default CraftingPanel;
