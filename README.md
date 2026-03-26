# AI 智能体操控自动化实验平台

本项目是一个基于 AI 智能体（AI Agent）控制的自动化材料实验平台。该平台集成了移片机械臂、滴液机械臂、匀胶机和光谱仪，通过 EMQX MQTT 服务器进行通信，实现了从实验参数解析、自动执行到原位光谱数据采集的全流程自动化。

## 📋 项目简介

本平台旨在利用大语言模型（LLM）作为"科学家"，通过自然语言交互或读取文献（PDF），自动规划并执行旋涂（Spin Coating）实验。系统采用 **C# (WinForms)** 构建上位机控制界面，**Python** 构建 AI 智能体后端及光谱数据分析客户端，两者通过 **EMQX MQTT Broker** 进行解耦通信。

### 核心功能

- **🤖 AI 智能体控制**: 支持上传 PDF 文献，AI 自动提取实验参数并下发指令控制平台执行实验。
- **🦾 多设备协同**:
  - **移片机械臂 (Arm)**: 负责基底的抓取、转移和放置。
  - **滴液机械臂 (Dispenser)**: 负责试剂的吸取、定量滴加和枪头更换。
  - **匀胶机 (Coater)**: 负责基底的高速旋涂，支持速度、加速度、时间精确控制。
  - **光谱仪 (Spectrometer)**: 负责实验过程中的原位光谱数据采集与实时传输。
- **📡 消息队列通信**: 基于 EMQX MQTT 服务器，实现控制指令下发和光谱数据上报。
- **📊 可视化交互**: 提供完整的 WinForms 图形界面，可手动调试各模块或监控自动实验流程。

---


---

## 📂 文件结构说明

### 🐍 Python 端 (AI 与 数据处理)

| 文件名 | 描述 |
| :--- | :--- |
| `main.py` | **AI 智能体后端**。基于 FastAPI 和 Pydantic AI，提供 WebSocket 聊天接口和 PDF 上传功能。集成 DeepSeek 模型，具备 `read_pdf` 和 `do_experiment` 工具能力。 |
| `tools.py` | **AI 工具集**。定义 AI 可调用的具体函数，包括解析 PDF 内容和通过 MQTT 向硬件平台发送实验指令 (`do_experiment`)。 |
| `agent_client.py` | **MQTT 客户端封装**。用于 AI 后端连接 EMQX 服务器，发布实验控制指令。 |
| `spec_client.py` | **光谱数据接收端**。订阅 MQTT 主题接收光谱仪上传的波长、光强和时间数据，并进行实时绘图和保存。 |

### 💻 C# 端 (硬件控制上位机)

| 文件名 | 描述 |
| :--- | :--- |
| `MainForm.cs` | **主控制窗体**。系统的核心调度器，负责协调机械臂、滴液臂、匀胶机和光谱仪的联动。包含自动实验流程逻辑 (`round_test`) 和 AI 指令监听 (`AI_Agent_Click`)。 |
| `ArmForm.cs` | **机械臂控制**。基于 Dobot API，控制机械臂的使能、运动 (MovJ/MovL)、夹爪抓取/释放及状态反馈。 |
| `DispenserForm.cs` | **滴液机械臂控制**。通过串口通信控制多轴运动平台和注射泵，实现吸液、排液、换枪头和归零操作。 |
| `CoaterForm.cs` | **匀胶机控制**。通过 Modbus RTU 协议控制电机驱动器，设定转速、加速度、运行时间，并监控电机状态。 |
| `SpecForm.cs` | **光谱仪控制**。连接光谱仪硬件，定时采集光谱数据并通过 MQTT 发布到 `counts`, `wavelength`, `time` 主题。 |


## ⚙️ 环境配置与安装

### 前置要求

- **.NET Framework / .NET 6+**: 用于编译运行 C# WinForms 项目。
- **Python 3.8+**: 用于运行 AI 后端和数据客户端。
- **EMQX Broker**: 需部署 MQTT 服务器（本项目默认配置 IP: `192.168.120.129`, Port: `1883`）。
- **硬件驱动**: 
  - Dobot 机械臂 SDK
  - 光谱仪 SDK 
  - 串口转 USB 驱动 (针对滴液臂和匀胶机)

### Python 环境安装

创建虚拟环境并安装依赖：

```bash
# 创建虚拟环境
python -m venv venv

# Windows 激活
venv\Scripts\activate

# Linux/Mac 激活
source venv/bin/activate

# 安装依赖
pip install -r requirements.txt

```

### 配置环境变量文件（.env）
```.env

DEEPSEEK_API_KEY=your_deepseek_api_key_here

```

### C# 项目编译
- 使用 Visual Studio 打开 .sln 文件（或在命令行使用 dotnet build），确保引用了以下库：
- Newtonsoft.Json
- uPLibrary.Networking.M2Mqtt (MQTT 客户端)
- Modbus.Device
- Dobot SDK

### 🚀 快速开始
- 第一步：启动 MQTT Broker
- 确保 EMQX 服务器正在运行，并且 C# 端和 Python 端配置的 IP/端口/账号密码一致（见代码中的 Client_Conf 和 SpecForm 类）。
- 第二步：启动硬件控制平台 (C#)
- 运行 MainForm.cs 对应的可执行程序。
- 在界面中分别打开 Arm, Dispenser, Coater, Spec 子窗口。
- 依次连接各硬件设备（点击 Connect/Open Serial 等按钮）。
- 点击主界面的 "ai agent on" 按钮，系统将订阅 MQTT 主题等待 AI 指令。
- 第三步：main.py启动 AI 智能体后端 (Python) 
```bash

uvicorn main:app --reload
#访问 http://localhost:8000 打开 Web 交互界面

```
- **第五步**：执行实验
  - Web 界面交互: 在浏览器中上传包含实验参数的 PDF 文件。
  - AI 解析: 提示词指示AI阅读 PDF，提取旋涂速度、试剂名称、体积等信息。
  - 自动执行: AI 调用 do_experiment 工具，通过 MQTT 发送指令。
  - 平台响应: C# 上位机收到指令后，自动执行"取片 → 滴液 → 旋涂&光谱采集"全流程。

### 🔌 通信协议说明

| Topic | 方向 | 内容格式 | 描述 |
| :--- | :--- | :--- | :--- |
| `do_experiment` | AI → 平台 | `p{speed},{acc},{dur},{reagent_pos},{vol}` | 启动单次实验指令 |
| `control` | 平台 ↔ 数据端 | `continue, stop, record, quit` | 控制光谱采集流程 |
| `wavelength` | 平台 → 数据端 | `[int1 int2 ...]` | 波长数据数组 |
| `counts` | 平台 → 数据端 | `[int1 int2 ...]` | 光强计数值数组 |
| `time` | 平台 → 数据端 | `float` | 当前采集时间点 (ms) |


### ⚠️ 注意事项
- **配置文件**: 确保 ArmPoints.json, DispenserPoints.json, reagent_layout.json 中的坐标和试剂位置与实际硬件布局一致。
- **网络配置**: 代码中硬编码了部分 IP 地址（如 192.168.120.129），请根据实际网络环境修改 agent_client.py, spec_client.py, SpecForm.cs 等文件中的配置。
- **平台检查**: 使用前请检查试剂余量、枪头数量和基底供应。
