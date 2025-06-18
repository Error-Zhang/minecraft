import React, { useRef, useState } from "react";
import "./index.less";

interface Option {
	value: any;
	label: string;
}

interface GameSelectCustomProps {
	label?: string;
	options: Option[];
	value: any;
	onChange: (value: any) => void;
	disabled?: boolean;
	className?: string;
	flex?: boolean;
}

const GameSelect: React.FC<GameSelectCustomProps> = ({
	label,
	options,
	value,
	onChange,
	disabled = false,
	className = "",
	flex = false,
}) => {
	const [open, setOpen] = useState(false);
	const wrapperRef = useRef<HTMLDivElement>(null);

	const selected = options.find(o => o.value === value);

	return (
		<div
			className={`game-select-custom ${disabled ? "disabled" : ""} ${flex ? "flex" : ""} ${className}`}
			ref={wrapperRef}
		>
			{label && <label className="game-select-label">{label}</label>}
			<div
				className="game-select-box"
				onClick={() => {
					if (!disabled) setOpen(!open);
				}}
			>
				{selected?.label || "请选择"}
			</div>
			{open && !disabled && (
				<ul className="game-select-options">
					{options.map(opt => (
						<li
							key={opt.value}
							className={`option ${opt.value === value ? "selected" : ""}`}
							onClick={() => {
								if (!disabled) {
									onChange(opt.value);
									setOpen(false);
								}
							}}
						>
							{opt.label}
						</li>
					))}
				</ul>
			)}
		</div>
	);
};

export default GameSelect;
