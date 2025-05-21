# Bench Testing

The Thurston application can be run on a Project Lab for bench testing and validation. 

## Setup

### Pre-requisites

* You should be a member of the `Wilderness Labs` org in Meadow.Cloud for this application. If you're not, ping BK for an invite.

### Instructions

1. [Provision Project Lab](https://developer.wildernesslabs.co/Meadow/Meadow.Cloud/Device_Provisioning/) to the `Wilderness Labs` org. This sets it up up for Meadow.Cloud communications:
    `meadow device provision -o 37fa1d46bd38433e80bd7c19f55bebe5 -n [device name]`
2. Test the app over WiFi. See [WiFi Setup](#wifi-setup).
3. Once WiFi h

## WiFi Setup
1. Edit the credentials in `wifi.config.yaml`.
2. Change the network config in `meadow.config.yaml` to `DefaultInterface: Wifi`.

## Cellular Setup
2. Configure Teal SIM Card.
     a. Go to [aurora.teal.global](https://aurora.teal.global/onechips).
     b. 