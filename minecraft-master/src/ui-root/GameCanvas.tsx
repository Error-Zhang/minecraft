import React, { RefObject, useEffect, useRef } from "react";
import { Game } from "../game-root/Game.ts";
import { useGameStore } from "@/store";
import { openGameDialog } from "@/ui-root/components/game-dialog/dialogService.tsx";

const GameCanvas: React.FC<{ canvasRef: RefObject<HTMLCanvasElement> }> = ({ canvasRef }) => {
	const gameRef = useRef<Game | null>(null);

	useEffect(() => {
		gameRef.current = new Game(canvasRef.current!);
		const unsub = useGameStore.subscribe((state, prevState) => {
			if (state.isGaming != prevState.isGaming) {
				state.isGaming ? gameRef.current!.start() : gameRef.current!.dispose();
			}
		});
		return () => {
			gameRef.current!.destroy();
			unsub();
		};
	}, []);

	useEffect(() => {
		const callback = (evt: KeyboardEvent) => {
			if (evt.key.toLowerCase() === "escape") {
				openGameDialog({
					title: "暂停",
					message: "是否要退出世界？",
					onConfirm() {
						useGameStore.setState({ isGaming: false });
					},
				});
			}
		};
		window.addEventListener("keydown", callback);
		return () => {
			window.removeEventListener("keydown", callback);
		};
	}, []);
	return <canvas id="game-canvas" ref={canvasRef} />;
};

export default GameCanvas;
