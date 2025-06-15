import "./index.less";

const DroneOverlayGrid = () => {
	const totalCells = 10 * 6;

	return (
		<div className="drone-overlay">
			{Array.from({ length: totalCells }).map((_, i) => {
				const duration = (Math.random() * 2 + 2).toFixed(2);
				const delay = (Math.random() * 2 + 2).toFixed(2);
				const opacity = (Math.random() * 0.4).toFixed(2);

				return (
					<div
						key={i}
						className="drone-cell"
						style={{
							animationDuration: `${duration}s`,
							animationDelay: `${delay}s`,
							opacity: opacity,
						}}
					/>
				);
			})}
		</div>
	);
};
export default DroneOverlayGrid;
