import { useEffect, useState } from "react";
import { DroppedSlotType, SlotType } from "@/ui-root/components/slot";
import { EventEmitter } from "eventemitter3";

const panelEventBus = new EventEmitter(); // 新建一个panelEventBus专门处理面板之间的通信

export function useInventorySlots(
	source: string,
	slots: SlotType[],
	setSlots: (slots: SlotType[]) => void,
	config?: {
		onDropOver?: () => void;
		overrideOnDrop?: (droppedSlot: DroppedSlotType, current: number) => boolean;
	}
) {
	const [droppedIndex, setDroppedIndex] = useState<number>(-1);

	// 自动清理空格
	useEffect(() => {
		let needUpdate = false;
		const cleaned = slots.map(slot => {
			if (slot && slot.value <= 0) {
				needUpdate = true;
				return null;
			}
			return slot;
		});

		if (needUpdate) {
			setSlots(cleaned);
		}
	}, [slots]);

	// 拖拽逻辑,这里是落下的slot执行
	const onDrop = (droppedSlot: DroppedSlotType, current: number) => {
		const currentSlot = slots[current] || { key: "" };
		if (!config?.overrideOnDrop?.(droppedSlot, current)) {
			// 叠加
			if (currentSlot.key === droppedSlot.key) {
				slots[current]!.value += droppedSlot.dropCount;
			}
			// 交换更新源
			else {
				slots[current] = { ...droppedSlot, value: droppedSlot.dropCount, source }; // 使用拷贝的方式
			}
		} else {
			// 检测一下是否进行了换源，否则会出bug
			if (slots[current]!.source !== source) {
				throw new Error("没有更换源");
			}
		}

		panelEventBus.emit(droppedSlot.source, { droppedSlot, currentSlot }); // 推送到对应的源
		setSlots([...slots]); // 后更新
	};

	// 处理不同源的情况，这里是起始的slot执行
	useEffect(() => {
		const callback = ({
			droppedSlot,
			currentSlot,
		}: {
			droppedSlot: DroppedSlotType;
			currentSlot: any;
		}) => {
			let isSplit = false; // 是否处在分割状态下
			// 更新另一端
			if (currentSlot.key === droppedSlot.key) {
				slots[droppedIndex]!.value -= droppedSlot.dropCount;
				if (slots[droppedIndex]!.value >= 0) isSplit = true;
			} else {
				const count = droppedSlot.value - droppedSlot.dropCount;
				// 分割的逻辑
				if (count > 0) {
					slots[droppedIndex] = {
						...droppedSlot,
						value: droppedSlot.value - droppedSlot.dropCount,
						source: droppedSlot.source,
					};
					isSplit = true;
				}
				// 交换
				else {
					slots[droppedIndex] = currentSlot.key
						? {
								...currentSlot,
								source: droppedSlot.source,
							}
						: null;
				}
			}
			setSlots([...slots]);
			config?.onDropOver?.();
			!isSplit && setDroppedIndex(-1);
		};
		panelEventBus.on(source, callback);
		return () => {
			panelEventBus.off(source, callback); // 注意一定要关闭监听
		};
	}, [slots, setSlots, droppedIndex]);

	return {
		setDroppedIndex,
		onDrop,
	};
}
