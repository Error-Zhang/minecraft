import React, { ReactNode, useEffect, useRef, useState } from "react";
import "./index.less";
import { useDroneBackgroundMotion } from "@/ui-root/hooks/useDroneBackgroundMotion.tsx";
import DroneOverlayGrid from "./DroneOverlayGrid.tsx";
import MainMenu from "./MainMenu.tsx";
import WorldManagerMenu from "./world-manager-menu";
import { useGameStore } from "@/store";

export type ScreenPage = "main" | "worldManager";

// 页面渲染函数
const renderPage = (page: ScreenPage, setPage: (page: ScreenPage) => void): ReactNode => {
	switch (page) {
		case "main":
			return <MainMenu setPage={setPage} />;
		case "worldManager":
			return <WorldManagerMenu setPage={setPage} />;
		default:
			return null;
	}
};

const StartScreen: React.FC<{ hidden: boolean }> = ({ hidden }) => {
	const ref = useDroneBackgroundMotion(!hidden);
	const [page, setPage] = useState<ScreenPage>("main");
	const audioRef = useRef<HTMLAudioElement | null>(null);
	const musicListRef = useRef<string[]>([]);
	const currentIndexRef = useRef(0);

	useEffect(() => {
		let needsUserInteraction = false;

		const loadMusic = async () => {
			const modules = import.meta.glob("/src/ui-root/assets/musics/*", {
				eager: true,
			}) as Record<string, { default: string }>;
			const sorted = Object.entries(modules).map(([_, mod]) => mod.default);
			musicListRef.current = sorted;
		};

		const stop = () => {
			// 如果已有 audio，先停止释放
			if (audioRef.current) {
				audioRef.current.pause();
				audioRef.current.src = "";
				audioRef.current = null;
			}
		};

		const playMusic = (index: number) => {
			const list = musicListRef.current;
			if (!list.length) return;

			const audio = new Audio(list[index]);
			audioRef.current = audio;

			audio.addEventListener("ended", () => {
				const nextIndex = (index + 1) % list.length;
				currentIndexRef.current = nextIndex;
				playMusic(nextIndex);
			});

			audio.volume = 0.5;
			setTimeout(() => {
				audio.play();
			}, 1000 * 60);
		};

		const handleUserInteraction = () => {
			if (needsUserInteraction && musicListRef.current.length > 0) {
				needsUserInteraction = false;
				playMusic(currentIndexRef.current);
				window.removeEventListener("pointerdown", handleUserInteraction);
			}
		};

		loadMusic();
		window.addEventListener("pointerdown", handleUserInteraction);

		const unsub = useGameStore.subscribe((state, prevState) => {
			if (state.isGaming && prevState.isGaming != state.isGaming) {
				playMusic(currentIndexRef.current);
			} else if (!state.isGaming && prevState.isGaming != state.isGaming) {
				stop();
			}
		});

		return () => {
			unsub();
			// 清理事件监听
			window.removeEventListener("pointerdown", handleUserInteraction);
		};
	}, []);

	return (
		<div className="start-screen" style={hidden ? { display: "none" } : {}} ref={ref}>
			{!hidden && <DroneOverlayGrid />}
			{renderPage(page, setPage)}
			<footer className="footer">© 2025 Error.Zhang. All rights reserved.</footer>
		</div>
	);
};

export default StartScreen;
