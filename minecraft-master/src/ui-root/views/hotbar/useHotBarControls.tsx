import { Dispatch, SetStateAction, useEffect } from "react";
import GameWindow from "@/game-root/core/GameWindow.ts";

export const HOTBAR_SIZE = 9;

export function useHotBarControls(setSelectedIndex: Dispatch<SetStateAction<number>>) {
	// 键盘快捷键（1-9）
	useEffect(() => {
		const handleKey = (e: KeyboardEvent) => {
			const num = parseInt(e.key);
			if (num >= 1 && num <= HOTBAR_SIZE) {
				setSelectedIndex(num - 1);
			}
		};
		GameWindow.Instance.addEventListener("keydown", handleKey);
		return () => GameWindow.Instance.removeEventListener("keydown", handleKey);
	}, [setSelectedIndex]);

	// 鼠标滚轮切换物品栏
	useEffect(() => {
		const handleWheel = (e: WheelEvent) => {
			setSelectedIndex(prev => (prev + (e.deltaY > 0 ? 1 : -1) + HOTBAR_SIZE) % HOTBAR_SIZE);
		};
		GameWindow.Instance.addEventListener("wheel", handleWheel);
		return () => GameWindow.Instance.removeEventListener("wheel", handleWheel);
	}, [setSelectedIndex]);
}
