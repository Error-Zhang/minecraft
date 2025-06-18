import React, { RefObject, useEffect, useState } from "react";
import GameUIWrapper from "./components/GameUIWrapper.tsx";
import StartScreen from "@/ui-root/views/start-screen";
import { useGameStore } from "@/store";
import Hotbar from "@/ui-root/views/hotbar";
import Catalog from "@/ui-root/views/catalog";
import Bag from "@/ui-root/views/bag";
import { HandSlotManager } from "@/ui-root/components/slot/HandSlotManager.tsx";

const GameUI: React.FC<{ canvasRef: RefObject<HTMLCanvasElement> }> = ({ canvasRef }) => {
	const [startGame, setStartGame] = useState(false);
	const [loading, setLoading] = useState(true);
	useEffect(() => {
		const unsub = useGameStore.subscribe(state => {
			setStartGame(state.isGaming);
			setLoading(state.isLoading);
		});
		return () => {
			unsub();
		};
	}, [setStartGame, setLoading]);
	return (
		<GameUIWrapper canvasRef={canvasRef}>
			{startGame && !loading && (
				<div>
					<Hotbar />
					<Catalog />
					<Bag rows={4} columns={4} />
				</div>
			)}
			<StartScreen hidden={startGame} />
			<HandSlotManager />
		</GameUIWrapper>
	);
};

export default GameUI;
