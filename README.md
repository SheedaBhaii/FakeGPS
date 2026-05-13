# FakeGPS

**Note: This driver has been updated to fix the "NaN" location timeout issue and improve compatibility with Windows 10/11.**

FakeGPS is a Windows driver that allows the user to provide geolocation information without a physical GPS device.

**Note: In order to use FakeGPS, please download [the latest released binary](https://github.com/SheedaBhaii/FakeGPS/releases), which includes the fixed build.**

## Why?

Windows includes features like weather, time-zone syncing, and location-based reminders. When there is no GPS device, Windows tries to guess your location via Wi-Fi or IP address, which is often inaccurate or fails entirely on virtual machines and desktops.

This driver provides a manual override, allowing you to set a precise location that the entire Windows operating system will treat as real.

## Requirements

1. **Windows 10/11 x64**
2. **Test Mode Enabled:** Open CMD as Admin and run `bcdedit /set testsigning on`, then restart. *(Note: This may require disabling Secure Boot in BIOS).*
3. **Geolocation Service Active:** The Windows "Geolocation Service" (`lfsvc`) must be set to **Automatic** and **Running** in `services.msc`.
4. **Privacy Settings:** "Location Services" and "Let desktop apps access your location" must be toggled **ON** in Windows Settings.

## Usage

### Driver Installation

* Ensure your system is in **Test Mode** (look for the watermark on your desktop).
* [Download the latest fixed binary](https://github.com/SheedaBhaii/FakeGPS/releases/download/v1.1-fix/FakeGPS_Release.zip) and extract the zip to a folder.
* Open **Device Manager**, select any item, then go to **Action > Add legacy hardware**.
* Choose **Install manually** > **Show All Devices** > **Have Disk**.
* Browse to the extracted folder and select **`FakeGPS.inf`**.
* Confirm you want to install the unsigned driver.

### Command Line Options

The command line tool must be run from an **Administrator Command Prompt**.

```bash
# Get current status
FakeGPS -g

# Set latitude and longitude
FakeGPS -s <lat,long>

```

**Example:**

```powershell
PS> FakeGPS -s 40.7580,-73.9855
The following location has been set in the driver's registry settings:
Lat:    40.7580
Long:   -73.9855

PS> FakeGPS -g
The following location has been got from the Windows location API:
Lat:    40.7580
Long:   -73.9855

```

## Fixed in this version

* **NaN Bug Fix:** The previous version often returned `NaN` because it didn't wait long enough for the Windows API to initialize. The code now implements a `ManualResetEvent` that waits up to 10 seconds for a valid sensor lock.
* **Registry Access:** Added documentation and error handling for Registry Access, ensuring users know to run as Administrator to save coordinates.
* **Service Dependency:** Included instructions for the **Geolocation Service**, which is a mandatory requirement for the driver to communicate with the OS.

**Note:** If your location does not update immediately in apps like Google Maps or Weather, go to **Device Manager**, right-click the **FakeGPS** device, select **Disable**, and then **Enable**. This clears the Windows Geolocation cache.
