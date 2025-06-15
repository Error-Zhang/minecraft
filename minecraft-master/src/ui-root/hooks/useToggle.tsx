import React, { useEffect } from "react";
import GameWindow from "@/game-root/core/GameWindow.ts";
import { audios } from "@/ui-root/assets/sounds";

type KeyBinding = string | string[];
const escapes: Function[] = [];

/**
 * 监听键盘按键按下，触发回调
 * @param keys 监听的键或键数组（不区分大小写）
 * @param onToggle 触发回调函数
 * @param qkeys
 * @param onEscape
 * @param isToggle
 */
export function useToggle(
	keys: KeyBinding,
	onToggle: () => void,
	qkeys?: KeyBinding,
	onEscape?: () => void,
	isToggle?: () => boolean
) {
	useEffect(() => {
		const game = GameWindow.Instance;
		if (onEscape) {
			escapes.push(onEscape);
			game.onClickCanvas(onEscape);
		}
		if (isToggle && !isToggle()) return;

		const handleKeyDown = (e: KeyboardEvent) => {
			const pressedKey = e.key.toLowerCase();

			const keyArray = Array.isArray(keys) ? keys : [keys];
			const qkeyArray = qkeys ? (Array.isArray(qkeys) ? qkeys : [qkeys]) : [];

			if (game.isInGame && keyArray.some(key => key.toLowerCase() === pressedKey)) {
				onToggle();
				game.togglePointerLock();
				audios.ButtonClick.play();
			} else if (
				qkeys?.length &&
				!game.isInGame &&
				qkeyArray.some(key => key.toLowerCase() === pressedKey)
			) {
				escapes.forEach(escape => escape());
				game.togglePointerLock();
				audios.ButtonClick.play();
			}
		};

		window.addEventListener("keydown", handleKeyDown);
		return () => {
			escapes.length = 0;
			window.removeEventListener("keydown", handleKeyDown);
		};
	}, []);
}

export function useToggleActive(
	setIsActive: React.Dispatch<React.SetStateAction<boolean>>,
	keys: KeyBinding,
	isToggle?: () => boolean
) {
	useToggle(
		keys,
		() => {
			setIsActive(true);
		},
		keys,
		() => {
			setIsActive(false);
		},
		isToggle
	);
}
