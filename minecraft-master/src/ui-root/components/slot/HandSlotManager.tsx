import React, { useCallback, useEffect, useState } from "react";
import { SlotType } from "@/ui-root/components/slot/index.tsx";
import "./index.less";
import { useGameStore } from "@/store";
import GameWindow from "@/game-root/core/GameWindow.ts";

// 使用更严格的类型定义
type Position = { x: number; y: number };
type HandSlotManagerProps = {
	offset?: Position; // 可选的偏移量配置
};

// 使用模块内的状态管理，避免全局变量污染
let currentHandSlot: SlotType | null = null;
const subscribers: Array<(slot: SlotType | null) => void> = [];

const notifySubscribers = (slot: SlotType | null) => {
	currentHandSlot = slot;
	subscribers.forEach(subscriber => subscriber(slot));
};
export const HandSlotManager: React.FC<HandSlotManagerProps> = ({ offset = { x: 10, y: 10 } }) => {
	const [handSlot, setHandSlot] = useState<SlotType | null>(null);
	const [position, setPosition] = useState<Position>({ x: 0, y: 0 });
	const [isDragging, setIsDragging] = useState(false);

	// 使用useCallback优化事件处理函数
	const handleMouseMove = useCallback(
		(e: MouseEvent) => {
			setPosition({ x: e.clientX + offset.x, y: e.clientY + offset.y });
		},
		[offset.x, offset.y]
	);

	const handleKeyDown = useCallback((e: KeyboardEvent) => {
		if (e.key.toLowerCase() === "q") {
			HandSlotController.clearHandSlot();
		}
	}, []);

	// 添加鼠标按下和释放事件，实现抓取效果
	const handleMouseDown = useCallback(() => {
		if (handSlot) {
			setIsDragging(true);
		}
	}, [handSlot]);

	const handleMouseUp = useCallback(() => {
		setIsDragging(false);
	}, []);

	useEffect(() => {
		const subscription = (slot: SlotType | null) => {
			setHandSlot(slot);
			if (!slot) setIsDragging(false); // 清除时重置拖拽状态
		};
		subscribers.push(subscription);
		return () => {
			const index = subscribers.indexOf(subscription);
			if (index > -1) subscribers.splice(index, 1);
		};
	}, []);

	useEffect(() => {
		window.addEventListener("mousemove", handleMouseMove);
		window.addEventListener("keydown", handleKeyDown);
		window.addEventListener("mousedown", handleMouseDown);
		window.addEventListener("mouseup", handleMouseUp);
		window.addEventListener("contextmenu", e => e.preventDefault());

		GameWindow.Instance.onActiveChange(active => {
			HandSlotController.clearHandSlot();
		});

		return () => {
			window.removeEventListener("mousemove", handleMouseMove);
			window.removeEventListener("keydown", handleKeyDown);
			window.removeEventListener("mousedown", handleMouseDown);
			window.removeEventListener("mouseup", handleMouseUp);
			window.removeEventListener("contextmenu", e => e.preventDefault());
		};
	}, [handleMouseMove, handleKeyDown, handleMouseDown, handleMouseUp]);

	useEffect(() => {
		if (handSlot && handSlot.value <= 0) {
			HandSlotController.clearHandSlot();
		} else if (handSlot) {
			document.body.setAttribute("style", "cursor: grabbing;");
		} else {
			document.body.setAttribute("style", "cursor: auto;");
		}
	}, [handSlot]);

	if (!handSlot) return null;

	return (
		<div
			style={{
				position: "fixed",
				left: position.x,
				top: position.y,
				pointerEvents: "none",
				zIndex: 1000,
				transform: isDragging ? "scale(0.9) rotate(5deg)" : "none",
				transition: "transform 0.1s ease",
			}}
		>
			<div className={`hand-slot`}>
				{handSlot?.icon && <img src={handSlot.icon} alt="item" draggable="false" />}
				{handSlot?.value > 1 && <span className="slot-number">{handSlot.value}</span>}
			</div>
		</div>
	);
};

// 手上物品控制器
export const HandSlotController = {
	setHandSlot: (slot: SlotType) => {
		notifySubscribers(slot);
		useGameStore.setState({ isSplitting: true });
	},
	clearHandSlot: () => {
		notifySubscribers(null);
		useGameStore.setState({ isSplitting: false });
	},
	isHolding: () => currentHandSlot !== null,
	getHandSlot: () => currentHandSlot,
};
