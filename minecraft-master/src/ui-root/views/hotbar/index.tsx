import React, { useEffect, useState } from "react";
import Slot, { createEmptySlots, SlotType } from "../../components/slot";
import "./index.less";
import { useInventorySlots } from "@/ui-root/components/inventory-grid/useInventorySlots.tsx";
import { HOTBAR_SIZE, useHotBarControls } from "@/ui-root/views/hotbar/useHotBarControls.tsx";
import { usePlayerStore } from "@/store";

const HotBar: React.FC = () => {
	const [selectedIndex, setSelectedIndex] = useState(0);
	const [slots, setSlots] = useState<SlotType[]>(createEmptySlots(HOTBAR_SIZE));

	useHotBarControls(setSelectedIndex);
	const { setDroppedIndex, onDrop } = useInventorySlots("HotBar", slots, setSlots);

	useEffect(() => {
		usePlayerStore.setState({ holdBlockId: slots[selectedIndex]?.id || 0 });
	}, [slots, selectedIndex]);

	return (
		<div className="hotbar-container">
			{slots.map((item, index) => (
				<Slot
					key={index}
					className="slot-hotbar"
					selected={index === selectedIndex}
					slot={item}
					draggable
					allowedDropSources={["all"]}
					onDragStart={() => {
						setDroppedIndex(index);
					}}
					onDrop={droppedSlot => onDrop(droppedSlot, index)}
				/>
			))}
		</div>
	);
};

export default HotBar;
