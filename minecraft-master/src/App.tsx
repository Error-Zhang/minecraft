// src/App.tsx
import React, { useEffect, useRef } from "react";
import GameCanvas from "./ui-root/GameCanvas.tsx";

import GameUI from "@/ui-root/GameUI.tsx";
import { openGameDialog } from "@/ui-root/components/game-dialog/dialogService.tsx";

const App: React.FC = () => {
	const canvasRef = useRef<HTMLCanvasElement>(null);
	useEffect(() => {
		const handeler = (e: any) => {
			openGameDialog({
				title: "错误",
				message: e.reason.message,
			});
		};
		window.addEventListener("unhandledrejection", handeler);
		return () => {
			window.removeEventListener("unhandledrejection", handeler);
		};
	}, []);
	return (
		<div id="game-root">
			<GameCanvas canvasRef={canvasRef} />
			<GameUI canvasRef={canvasRef} />
		</div>
	);
};

export default App;
