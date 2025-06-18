import React from "react";
import Slot, { DroppedSlotType, SlotType } from "../slot";
import "./index.less";
import { useInventorySlots } from "@/ui-root/components/inventory-grid/useInventorySlots.tsx";

interface InventoryGridProps {
	source: string; // 来源的组件名称
	slots: SlotType[];
	setSlots: (slots: SlotType[]) => void;
	onDrop?: (droppedSlot: DroppedSlotType, current: number) => boolean;
	allowedDropSources?: string[];
	columns: number;
	rows: number;
}

const InventoryGrid: React.FC<InventoryGridProps> = ({
	source,
	slots,
	setSlots,
	onDrop,
	columns,
	rows,
	allowedDropSources = ["all"],
}) => {
	const { setDroppedIndex, onDrop: onDropDefault } = useInventorySlots(source, slots, setSlots, {
		overrideOnDrop: onDrop,
	});

	return (
		<div
			className="inventory-grid"
			style={{
				gridTemplateColumns: `repeat(${columns},auto)`, // 不能缺少宽度，auto使用子元素的宽度
				gridTemplateRows: `repeat(${rows},auto)`,
			}}
		>
			{Array.from({ length: columns * rows }).map((_, i) => (
				<Slot
					key={i}
					slot={slots[i]}
					draggable
					allowedDropSources={allowedDropSources}
					onDragStart={() => {
						setDroppedIndex(i);
					}}
					onDrop={droppedSlot => {
						onDropDefault(droppedSlot, i);
					}}
				/>
			))}
		</div>
	);
};

export default InventoryGrid;
