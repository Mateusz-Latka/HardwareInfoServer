# HardwareInfoServer

A lightweight REST API server that exposes live hardware metrics from your machine using [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor).

## 📋 Features

✅ CPU usage (%)  
✅ CPU temperature (if supported by platform/BIOS)  
✅ CPU power (if supported)  
✅ GPU load, temperature, fan speed  
✅ RAM usage  
✅ Disk usage  
✅ `/health` endpoint to check server status

The server runs on **.NET 8** and is designed to work with a client application (e.g., [HardwareInfoViewer](https://github.com/your-org/HardwareInfoViewer)).

---

## 🚀 Getting Started

### 🧰 Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Windows OS (since `PerformanceCounter` and `WMI` are used)

### 📦 Installation

Clone the repository:
```bash
git clone https://github.com/your-org/HardwareInfoServer.git
cd HardwareInfoServer
