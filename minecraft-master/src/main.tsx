import { createRoot } from "react-dom/client";
import App from "./App.tsx";
import "@/ui-root/styles/index.less";
import { registerBuiltInLoaders } from "@babylonjs/loaders/dynamic";
// 注册加载器，加载模型需要使用
registerBuiltInLoaders();
createRoot(document.getElementById("root")!).render(<App />);
