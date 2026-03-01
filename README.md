# Project_Digital_Twin_Aerospace - Setup & Run Guide

This guide covers how to set up and run the full system: the **Flask backend server** and the **Unity HoloLens 2 application**.

---

## Prerequisites

### Server-side
- Python 3.8+
- pip packages: `flask`, `flask-restful`

### Unity / Development machine
- Unity 2020.3 LTS or later (with Universal Windows Platform build support installed)
- Mixed Reality Toolkit (MRTK) for Unity
- Vuforia Engine SDK (Unity package)
- Unity Addressables package (`com.unity.addressables`)
- Windows 10/11 machine with Visual Studio 2019 or 2022 (with UWP and C++ workloads)
- HoloLens 2 device (Developer Mode enabled)

---

## Part 1 — Flask Server Setup

### 1. Install dependencies

```bash
pip install flask flask-restful
```

### 2. Prepare Addressable bundles

Place your Unity-built Addressable asset bundles (the `WSAPlayer` folder output) in:

```
/home/<your-user>/mysite/WSAPlayer/
```

> This path is configured in `flask_app.py`. Edit the `filepath` variable in `get_addressable()` if your path is different.

### 3. Run the server

```bash
python flask_app.py
```

The server will start on `http://0.0.0.0:5000` by default.

> **Note:** Make sure your server machine and HoloLens 2 are on the **same local network** (same Wi-Fi or subnet).

---

## Part 2 — Unity Project Setup

### 1. Configure the server IP address

In **`ContinuityTest.cs`**, update the URL to match your server's local IP address:

```csharp
private string URL = "http://<YOUR_SERVER_IP>:5000/data";
```

### 2. Configure Unity Addressables

1. Open **Window → Asset Management → Addressables → Groups**.
2. Set the **Remote Load Path** to:
   ```
   http://<YOUR_SERVER_IP>:5000/WSAPlayer/[BuildTarget]
   ```
3. Assign your AR target prefabs to the `targetsAddressables` array in the `VuforiaTargetsHandler` component (Inspector).
4. Build Addressables: **Build → Build Content** (choose the WSAPlayer target).
5. Copy the output `ServerData/WSAPlayer/` folder to your Flask server's `/home/<your-user>/mysite/WSAPlayer/` directory.

### 3. Configure scene navigation

`Change_Scene.cs` uses Unity's scene index to navigate between scenes. Make sure your scenes are added to the **Build Settings** (File → Build Settings → Add Open Scenes) in the correct order, and call `MoveToScene(sceneIndex)` from your UI buttons accordingly.

### 4. Build for UWP (HoloLens 2)

1. Go to **File → Build Settings**.
2. Select **Universal Windows Platform**.
3. Set the following options:
   - **Target Device:** HoloLens
   - **Architecture:** ARM64
   - **Build Type:** D3D Project
   - **Minimum Platform Version:** 10.0.18362.0
4. Click **Build**, choose an output folder (e.g., `Build/`).

### 5. Deploy with Visual Studio

1. Open the generated `.sln` file in Visual Studio.
2. Set the build target to **Release / ARM64**.
3. Select **Remote Machine** as the deployment target.
4. Enter your HoloLens 2 IP address (found in HoloLens Settings → Network).
5. Click **Deploy** (or press F5 to deploy and debug).

> On first deploy, you may need to pair the device: go to HoloLens Settings → Update → For developers → Device Portal, and pair using the PIN shown during Visual Studio deployment.

---

## Part 3 — Running the Full System

1. **Start the Flask server** on your host machine:
   ```bash
   python flask_app.py
   ```
2. **Ensure HoloLens 2 and server are on the same network.**
3. **Launch the app on HoloLens 2** from the Start menu.
4. The app will automatically attempt to download the first AR target from the server on startup.
5. Use the **Next / Previous** buttons in the Instructions Menu to navigate through AR instruction steps.
6. The continuity test panel (if used) will poll `/data` every ~0.3 seconds and display the result.

---

## Troubleshooting

**App cannot connect to server**
- Confirm both devices are on the same Wi-Fi/subnet.
- Check Windows Firewall rules — allow inbound connections on port `5000`.
- Ping the server IP from the HoloLens Device Portal shell.

**Addressables fail to download**
- Verify the Remote Load Path in Addressables settings matches the server URL exactly.
- Confirm the bundle files exist under `/WSAPlayer/` on the server.
- Check the Unity console / HoloLens Device Portal for `Addressables: Connection error` messages.

**"Open circuit" always shown in continuity test**
- The `/data` endpoint must return a valid string. Confirm the sensor is connected and the Flask `/data` route is implemented and returning data.

**Keyboard not appearing (MRTK)**
- `MixedRealityKeyboard.cs` requires the UWP `InputPane` API. Make sure the build target is UWP (not editor/standalone).