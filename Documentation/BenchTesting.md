# Bench Testing

The Thurston application can be run on a Project Lab for bench testing and validation. 

## Setup

### Pre-requisites

* You should be a member of the `Wilderness Labs` org in Meadow.Cloud for this application. If you're not, ping BK for an invite.

### Instructions for Setting up the Physical (Project Lab) Bench Test

1. [Provision your Project Lab](https://developer.wildernesslabs.co/Meadow/Meadow.Cloud/Device_Provisioning/) to the `Wilderness Labs` org. This sets it up up for Meadow.Cloud communications:
    `meadow device provision -o 37fa1d46bd38433e80bd7c19f55bebe5 -n [device name]`
2. Set your device name in `meadow.config.yaml`
     `Name: BryanThurstonPL`
3. Test the app over WiFi. See [WiFi Setup](#wifi-setup).
4. Once project has been verified connecting, change over to the [cell config](#cellular-setup).

## WiFi Setup
1. Edit the credentials in `wifi.config.yaml`.
2. Change the network config in `meadow.config.yaml` to `DefaultInterface: Wifi`.

## Cellular Setup

1. Activate and assign Teal SIM Card.
  * Go to [aurora.teal.global](https://aurora.teal.global/onechips).
  * Search for the last few SIM card ID numbers to pull up the detail page, and click the **Activate** button.
  * Make sure that it's in the `internal_AquaMira` sub account. Email [Teal Support](mailto:support@tealcom.freshdesk.com) with the SIM card ID with a request to move to that group.
2. In `meadow.config.yaml`, change the default network interface to cell:
    `DefaultInterface: Cell`