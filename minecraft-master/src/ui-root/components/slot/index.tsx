import React, { useState } from "react";
import "./index.less";
import { Nullable } from "@babylonjs/core";
import { HandSlotController } from "@/ui-root/components/slot/HandSlotManager.tsx";
import { audios } from "@/ui-root/assets/sounds";

export type SlotType = Nullable<{
	id: number;
	key: string;
	displayName: string;
	value: number;
	icon?: string;
	source: string; // 如 "catalog" | "hotbar"
}>;

export type DroppedSlotType = Exclude<SlotType, null> & { dropCount: number };

interface SlotProps {
	selected?: boolean;
	splitting?: boolean;
	slot: SlotType;
	onDragStart?: (slot: SlotType) => void;
	onDrop?: (slot: DroppedSlotType) => void;
	allowedDropSources?: string[]; // 允许哪些来源的 slot 拖入
	draggable?: boolean; // 是否允许拖拽（控制是否可以点击拿起）
	className?: string;
	showCount?: boolean;
}

export const createEmptySlots = (count: number): SlotType[] => {
	return Array.from({ length: count }).map(_ => null);
};

const Slot: React.FC<SlotProps> = ({
	selected = false,
	className = "",
	slot,
	onDragStart,
	onDrop,
	allowedDropSources = [],
	draggable = false,
	showCount = true,
}) => {
	const [isActive, setIsActive] = useState(false);
	const [imgError, setImgError] = useState(false);

	const handleClick = (e: any) => {
		if (!draggable) return;

		if (!HandSlotController.isHolding()) {
			if (!slot) return;
			HandSlotController.setHandSlot({ ...slot });
			onDragStart?.(slot);
			audios.ItemMoved.play();
		} else {
			const handSlot = HandSlotController.getHandSlot();
			if (allowedDropSources.includes(handSlot!.source) || allowedDropSources.includes("all")) {
				onDrop?.({ ...handSlot!, dropCount: handSlot!.value });
				HandSlotController.clearHandSlot();
				audios.ItemMoved.play();
			}
		}
	};

	const handleRightClick = (e: React.MouseEvent<HTMLDivElement>) => {
		e.preventDefault(); // 阻止默认右键菜单
		// 处理分割
		if (HandSlotController.isHolding() && !isActive) {
			const handSlot = HandSlotController.getHandSlot()!;
			if (allowedDropSources.includes(handSlot!.source) || allowedDropSources.includes("all")) {
				onDrop?.({ ...handSlot!, dropCount: 1 });
				HandSlotController.setHandSlot({ ...handSlot, value: handSlot.value - 1 });
				audios.ItemMoved.play();
			}
			if (HandSlotController.getHandSlot()!.value < 1) {
				HandSlotController.clearHandSlot();
				audios.ItemMoved.play();
			}
		}
	};
	return (
		<div
			title={slot?.displayName || ""}
			style={{ cursor: slot ? "grab" : "auto" }}
			className={`slot ${selected ? "selected" : ""} ${isActive ? "splitting" : ""} ${className}`}
			onClick={handleClick}
			onContextMenu={handleRightClick} // 加上右键处理
		>
			{slot &&
				(!imgError ? (
					<img
						className="slot-icon"
						src={slot.icon || ""}
						onError={() => setImgError(true)}
						alt=""
					/>
				) : (
					<span className="slot-fallback-text">{slot.displayName}</span>
				))}
			{showCount && slot && slot.value > 1 && <span className="slot-number">{slot.value}</span>}
		</div>
	);
};

export default Slot;
