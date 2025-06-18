import { app, BrowserWindow } from "electron";
import path from "path";
import { fileURLToPath } from "url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

function createWindow() {
	const win = new BrowserWindow({
		width: 1280,
		height: 720,
		webPreferences: {
			contextIsolation: true,
		},
	});

	// 加载本地 HTML（Vite 构建后的 index.html）
	win.loadFile(path.join(__dirname, "../dist/index.html"));

	// win.webContents.openDevTools(); // 开发时开启控制台
}

app.whenReady().then(createWindow);

app.on("window-all-closed", () => {
	if (process.platform !== "darwin") app.quit();
});
