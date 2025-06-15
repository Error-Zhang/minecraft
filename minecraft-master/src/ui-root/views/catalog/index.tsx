import React, { useEffect, useState } from "react";
import "./index.less";
import Slot, { SlotType } from "@/ui-root/components/slot";
import { useToggleActive } from "@/ui-root/hooks/useToggle.tsx";
import { useBlockStore, useGameStore } from "@/store";
import { BlockMetaData } from "@/game-root/block-definitions/BlockBuilder.ts";

const Catalog: React.FC = () => {
	const [slots, setSlots] = useState<SlotType[]>([]);
	const [isActive, setIsActive] = useState(false);
	const initBlocks = async () => {
		const blockRegistry = useBlockStore.getState().blockRegistry!;
		if (!useBlockStore.getState().blockIcons) {
			let icons = await blockRegistry.getBlockIcons();
			useBlockStore.setState({ blockIcons: icons });
		}
		const items: SlotType[] = blockRegistry.getAllBlocks<BlockMetaData>().map(block => ({
			id: block.id!,
			key: block.blockType,
			displayName: block.metaData.displayName,
			value: block.metaData.maxStackCount,
			icon: useBlockStore.getState().blockIcons![block.blockType],
			source: "Catalog",
		}));
		setSlots(items);
	};
	useEffect(() => {
		initBlocks();
	}, []);

	useToggleActive(setIsActive, "e", () => useGameStore.getState().gameMode === 0);

	return (
		<div
			style={
				isActive
					? {}
					: {
							display: "none",
						}
			}
			className="catalog-panel"
		>
			{slots.map((slot, index) => (
				<Slot key={index} slot={slot} draggable showCount={false} />
			))}
		</div>
	);
};

export default Catalog;
