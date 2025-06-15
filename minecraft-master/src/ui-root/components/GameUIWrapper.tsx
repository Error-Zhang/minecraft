import React, {RefObject, useEffect, useRef} from "react";

interface GameUIWrapperProps {
    children: React.ReactNode;
    canvasRef:RefObject<HTMLCanvasElement>;
}
function setResponsiveFontWithCanvas(
    target:HTMLDivElement,
    canvas: HTMLCanvasElement,
    baseFontSize = 16,
    baseWidth = 1080
) {
    const resize = () => {
        const width = canvas.clientWidth;
        const scale = width / baseWidth;
        const fontSize = baseFontSize * scale;
        target.style.fontSize = `${fontSize}px`;
    };

    // 初始设置
    resize();

    const observer = new ResizeObserver(() => {
        resize();
    });

    observer.observe(canvas);

    return () => {
        observer.disconnect();
    };
}
const GameUIWrapper: React.FC<GameUIWrapperProps> = ({ children,canvasRef }) => {
    const wrapperRef=useRef(null);
    useEffect(() => {
        setResponsiveFontWithCanvas(wrapperRef.current!,canvasRef.current!);
    }, []);
    return (
        <div id="game-ui-wrapper" ref={wrapperRef}>
            {children}
        </div>
    );
};

export default GameUIWrapper;
