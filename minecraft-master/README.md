# Minecraft-like Game

一个基于 Babylon.js 的 Minecraft 风格游戏项目。

## 项目简介

这是一个使用现代Web技术栈开发的Minecraft风格游戏。项目结合了3D游戏引擎和桌面应用开发技术，提供了一个完整的游戏开发框架。

## 技术栈

### 核心框架

- **Babylon.js**: 强大的3D游戏引擎
- **React**: 用户界面开发
- **TypeScript**: 类型安全的JavaScript超集
- **Electron**: 跨平台桌面应用开发
- **Vite**: 现代前端构建工具

### 主要依赖

- **@babylonjs/core**: Babylon.js核心功能
- **@babylonjs/gui**: 游戏UI组件
- **@babylonjs/havok**: 物理引擎
- **@babylonjs/materials**: 材质系统
- **@microsoft/signalr**: 实时通信

## 项目结构

```
├── src/                          # 源代码目录
│   ├── game-root/               # 游戏核心逻辑
│   │   ├── managers/           # 游戏管理器（如世界、实体、网络等）
│   │   ├── worker/            # Web Worker相关代码
│   │   ├── ui/                # 游戏内UI组件
│   │   ├── core/              # 核心游戏逻辑
│   │   ├── block-definitions/ # 方块定义
│   │   ├── player/            # 玩家相关逻辑
│   │   ├── client/            # 客户端逻辑
│   │   ├── utils/             # 工具函数
│   │   └── assets/            # 游戏资源
│   │
│   ├── ui-root/               # 用户界面
│   │   ├── hooks/            # React自定义钩子
│   │   ├── assets/           # UI资源文件
│   │   ├── styles/           # 样式文件
│   │   ├── api/              # API接口
│   │   ├── components/       # 可复用UI组件
│   │   ├── views/            # 页面视图组件
│   │   ├── GameCanvas.tsx    # 游戏画布组件
│   │   └── GameUI.tsx        # 游戏UI主组件
│   │
│   ├── engine/               # 游戏引擎
│   │   ├── core/            # 引擎核心功能
│   │   ├── chunk/           # 区块系统
│   │   ├── renderer/        # 渲染系统
│   │   ├── environment/     # 环境系统
│   │   ├── block/           # 方块系统
│   │   ├── block-icon/      # 方块图标
│   │   ├── types/           # 类型定义
│   │   ├── systems/         # 游戏系统
│   │   └── shaders/         # 着色器
│   │
│   ├── store/               # 状态管理
│   ├── main.tsx             # 应用入口
│   └── App.tsx              # 主应用组件
│
├── electron/                # Electron相关配置
├── public/                 # 静态资源
├── dist/                  # 构建输出目录
└── package.json           # 项目配置和依赖
```

### 核心模块说明

#### game-root

- **managers/**: 包含游戏各个系统的管理器，如世界管理、实体管理、网络管理等
- **worker/**: 使用Web Worker处理计算密集型任务，如地形生成
- **core/**: 包含游戏的核心逻辑，如游戏循环、事件系统等
- **block-definitions/**: 定义游戏中所有方块的属性和行为
- **player/**: 处理玩家相关的逻辑，如移动、交互等

#### ui-root

- **components/**: 可复用的UI组件，如按钮、菜单等
- **views/**: 完整的页面视图，如主菜单、设置页面等
- **hooks/**: 自定义React钩子，用于共享UI逻辑
- **styles/**: 全局样式和主题定义

#### engine

- **core/**: 引擎的基础功能，如场景管理、相机控制等
- **chunk/**: 区块系统，处理世界分块和加载
- **renderer/**: 渲染系统，处理图形渲染和优化
- **environment/**: 环境系统，如天空、天气等
- **block/**: 方块渲染和交互系统
- **shaders/**: 自定义着色器，用于特殊视觉效果

## 开发指南

### 环境要求

- Node.js
- Yarn 或 npm

### 安装依赖

```bash
yarn install
# 或
npm install
```

### 开发命令

- `yarn dev`: 启动开发服务器
- `yarn build`: 构建项目
- `yarn electron:dev`: 启动Electron开发环境
- `yarn electron:build`: 构建Electron应用
- `yarn lint`: 运行代码检查

### 构建目标

- Windows: NSIS安装包
- macOS: DMG镜像
- Linux: AppImage

## 特性

- 3D游戏引擎集成
- 跨平台支持
- 实时多人游戏支持
- 模块化架构
- 类型安全
- 现代化UI/UX

## 作者

Error.Zhang

## 许可证

© 2025 Error.Zhang. All rights reserved.
