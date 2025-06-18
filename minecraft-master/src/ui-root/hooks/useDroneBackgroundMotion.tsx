import { useEffect, useRef, useState } from "react";

interface DroneMotionOptions {
	speed?: number;
	zoomMin?: number;
	zoomMax?: number;
	zoomSpeed?: number;
	minDistance?: number; // 最小移动距离百分比
}

export function useDroneBackgroundMotion(open: boolean, options: DroneMotionOptions = {}) {
	const {
		speed = 0.005,
		zoomMin = 100,
		zoomMax = 200,
		zoomSpeed = 0.2,
		minDistance = 50, // 至少移动10%距离再换目标
	} = options;

	const ref = useRef<HTMLDivElement>(null);
	const [containerSize, setContainerSize] = useState({ width: 0, height: 0 });
	const animationFrameRef = useRef<number>();

	useEffect(() => {
		const updateSize = () => {
			if (ref.current) {
				setContainerSize({
					width: ref.current.offsetWidth,
					height: ref.current.offsetHeight,
				});
			}
		};
		updateSize();
		window.addEventListener("resize", updateSize);
		return () => window.removeEventListener("resize", updateSize);
	}, []);

	useEffect(() => {
		let posX = 0,
			posY = 0;
		let targetX = 0,
			targetY = 0;
		let time = 0;
		let currentZoom = zoomMin;

		const getNewTargetPosition = () => {
			const maxOffsetX = currentZoom - 100;
			const maxOffsetY = currentZoom - 100;

			let newX = targetX;
			let newY = targetY;

			let tryCount = 0;

			while (tryCount < 10) {
				newX = Math.random() * maxOffsetX;
				newY = Math.random() * maxOffsetY;

				const dx = newX - posX;
				const dy = newY - posY;
				const distance = Math.sqrt(dx * dx + dy * dy);

				if (distance >= minDistance) {
					break;
				}
				tryCount++;
			}

			targetX = newX;
			targetY = newY;
		};

		const update = () => {
			if (!open) return;

			time += 0.016;
			// 如果 time 太大了，就重置为等价值，保持周期连续
			if (time > 1e6) {
				const period = (Math.PI * 2) / zoomSpeed; // 正弦波的一个周期
				time = time % period;
			}

			// 更新缩放
			const zoomMid = (zoomMin + zoomMax) / 2;
			const zoomRange = (zoomMax - zoomMin) / 2;
			currentZoom = zoomMid + zoomRange * Math.sin(time * zoomSpeed);

			// 平滑移动
			posX += (targetX - posX) * speed;
			posY += (targetY - posY) * speed;

			// 若已接近目标，换一个新目标点
			const dx = targetX - posX;
			const dy = targetY - posY;
			const distance = Math.sqrt(dx * dx + dy * dy);
			if (distance < 1) {
				getNewTargetPosition();
			}

			// 应用样式
			if (ref.current) {
				ref.current.style.backgroundPosition = `-${posX}% -${posY}%`;
				ref.current.style.backgroundSize = `${currentZoom}%`;
			}

			animationFrameRef.current = requestAnimationFrame(update);
		};

		if (open) {
			getNewTargetPosition();
			animationFrameRef.current = requestAnimationFrame(update);
		} else {
			// 当关闭时，重置到初始状态
			if (ref.current) {
				ref.current.style.backgroundPosition = "0% 0%";
				ref.current.style.backgroundSize = `${zoomMin}%`;
			}
		}

		return () => {
			if (animationFrameRef.current) {
				cancelAnimationFrame(animationFrameRef.current);
			}
		};
	}, [open, speed, zoomMin, zoomMax, zoomSpeed, minDistance]);

	return ref;
}
