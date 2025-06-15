import React, { useEffect, useState } from "react";
import "./index.less";
import InventoryGrid from "@/ui-root/components/inventory-grid";
import { createEmptySlots } from "@/ui-root/components/slot";
import { useToggleActive } from "@/ui-root/hooks/useToggle.tsx";
import { useGameStore } from "@/store";
import { interactEvents } from "@/game-root/core/events.ts";
import BlockType from "@/game-root/block-definitions/BlockType.ts";
import CraftTable from "@/ui-root/views/craft-table";
import GameWindow from "@/game-root/core/GameWindow.ts";
import { audios } from "@/ui-root/assets/sounds";

const PanelMap: Partial<Record<string | number, React.FC<{ guid: string }>>> = {
	[BlockType.CraftTableBlock]: ({ guid }) => <CraftTable guid={guid} />,
};

const getPanel = (blockInfo?: { blockType: string | number; guid: string }) => {
	if (!blockInfo) return null;
	const PanelComponent = PanelMap[blockInfo.blockType];
	return PanelComponent && <PanelComponent guid={blockInfo.guid} />;
};

const Bag: React.FC<{ rows: number; columns: number }> = ({ rows, columns }) => {
	const [slots, setSlots] = useState(createEmptySlots(rows * columns));
	const [isActive, setIsActive] = useState(false);

	const [activeBlock, setActiveBlock] = useState<{ blockType: string | number; guid: string }>();

	useEffect(() => {
		interactEvents.on("CraftTableBlock", () => {
			setActiveBlock({
				blockType: BlockType.CraftTableBlock,
				guid: "",
			});
			setIsActive(true);
			GameWindow.Instance.togglePointerLock();
			audios.ButtonClick.play();
		});
	}, []);

	useToggleActive(setIsActive, "e", () => useGameStore.getState().gameMode !== 0);

	return (
		isActive && (
			<div className="bag-panel absolute-center">
				{getPanel(activeBlock)}
				<div className="bag">
					<div className="panel-title">背包</div>
					<InventoryGrid
						rows={rows}
						columns={columns}
						source="Bag"
						slots={slots}
						setSlots={setSlots}
					/>
				</div>
			</div>
		)
	);
};
export default Bag;
