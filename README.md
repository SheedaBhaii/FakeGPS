# FakeGPS

**Note: This driver has been updated to fix the "NaN" location timeout issue and implement automated driver refreshing for Windows 10/11.**

FakeGPS is a Windows driver that allows the user to provide geolocation information without a physical GPS device.

**Note: To use FakeGPS, please download [the latest released binary](https://github.com/SheedaBhaii/FakeGPS/releases), which includes the fixed build.**

## Why?

Windows includes features like weather, time-zone syncing, and location-based reminders. When there is no GPS device, Windows tries to guess your location via Wi-Fi or IP address, which is often inaccurate.

This driver provides a manual override, allowing you to set a precise location that the entire Windows operating system will treat as real.

## Requirements

1. **Windows 10/11 x64**
2. **Test Mode Enabled:** Open CMD as Admin and run `bcdedit /set testsigning on`, then restart. *(Note: This may require disabling Secure Boot in BIOS).*
3. **Geolocation Service Active:** The Windows "Geolocation Service" (`lfsvc`) must be set to **Automatic** and **Running** in `services.msc`.
4. **Privacy Settings:** "Location Services" and "Let desktop apps access your location" must be toggled **ON** in Windows Settings.

## Usage

### Driver Installation

* Ensure your system is in **Test Mode** (look for the watermark on your desktop).
* [Download the latest binary](https://github.com/SheedaBhaii/FakeGPS/releases/latest) and extract the folder.
* Open **Device Manager**, select any item, then go to **Action > Add legacy hardware**.
* Choose **Install manually** > **Show All Devices** > **Have Disk**.
* Browse to the extracted folder and select **`FakeGPS.inf`**.
* Confirm you want to install the unsigned driver.

### Command Line Options

The command line tool must be run from an **Administrator Command Prompt** to allow the automated driver refresh to function.

```bash
# Get current status
FakeGPS -g

# Set latitude and longitude
FakeGPS -s <lat,long>

```

**Example:**

```cmd
FakeGPS -s 40.7580,-73.9855
The following location has been set in the driver's registry settings:
Lat:    40.7580
Long:   -73.9855
Refreshing driver to clear Windows location cache...
Driver refresh complete!

```

## New in v1.2

* **Automated Driver Refresh:** When setting a location (`-s`), the tool now automatically toggles the hardware state. This clears the Windows location cache instantly, so you don't have to manually disable/enable in Device Manager.
* **NaN Bug Fix:** Implemented a 10-second wait for a valid sensor lock, preventing the "NaN" coordinates error found in older versions.
* **Localization Support:** Added `InvariantCulture` support to ensure coordinates parse correctly regardless of your Windows regional/decimal settings.
* **Stability:** Compiled for x64 with a stable ILMerge configuration for a portable, single-executable experience.

**Note:** If your location does not update in a web browser, try a new Incognito window or clear the browser cache to force a new check against the refreshed driver.
