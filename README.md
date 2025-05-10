# Customer_ThurstonPUD

## Project Lab Pin configuration

Configuration for Project Lab 3.d pins

| terminal | purpose | processor pin | timer | connector |
|-|-|-|-|-|
| 210 | high-speed digital input | PB8 | TIM4_CH3 | mikrobus 1 PWM |
| 211 | high-speed digital input | PH10 | TIM5_CH1 | mikrobus 1 RST |
| 212 | high-speed digital input | PB15 | TIM12_CH2 | mikrobus 1 RX |
| 213 | high-speed digital input | PB14 | TIM12_CH1 | mikrobus 1 TX |

## Backplane Wiring

## Other notes

We will be replacing an existing backplane ([schematic here](./Hardware/22.35.01-Pattison-E-I-Set-(08.31.23)JNS.pdf))

The VFDs will be migrating from 4-20mA to Modbus, but otherwise we will maintain functionality.  So the following signals are needed:

The original schematic show 2 Alarm dialers and 2 panel meters

__4-20mA inputs__

| signal | vendor | units | scale | offset | Terminal | Meadow Signal |
| - | - | - | - | - | - | - |
| Kagy Well Pressure Transducer | 0-xx ft | unknown | unknown | unknown | unknown | unknown |
| Reservoir Pressure Transducer | 0-xx ft | unknown | unknown | unknown | unknown | unknown |
| XMAS Tree Well Flow Meter | unknown | unknown | unknown | unknown | unknown | unknown |
| Discharge Flow meter | unknown | unknown | unknown | unknown | unknown | unknown |
| pH | unknown | unknown | unknown | unknown | unknown | unknown |
| Chlorine | unknown | unknown | unknown | unknown | unknown | unknown |
| Seismic Valve position | unknown | unknown | unknown | unknown | unknown | unknown |

__Modbus RTU inputs__

| signal | vendor | units | address | register | scale | offset | Terminal | Meadow Signal |
| - | - | - | - | - | - | - | - | - |
| Pump1 VFD | unkwown | rpm? | unknown | unknown | unknown | unknown | unknown | unknown |
| Pump2 VFD | unkwown | rpm? | unknown | unknown | unknown | unknown | unknown | unknown |
| Pump3 VFD | unkwown | rpm? | unknown | unknown | unknown | unknown | unknown | unknown |
| Pump4 VFD | unkwown | rpm? | unknown | unknown | unknown | unknown | unknown | unknown |

__Discrete inputs__

| signal | active voltage | source/sink | Terminal | Meadow Signal |
| - | - | - | - | - |
| Low Suction Alarm | unknown (probably 24V?)| unknown | unknown | unknown |
| Duty Pump 1 VFD fault | unknown | unknown | unknown | unknown |
| High Capacity (HC) pump VFD fault | unknown | unknown | unknown | unknown |
| Kagy Well pump VFD fault | unknown | unknown | unknown | unknown |
| Distribution Pressure low alarm | unknown | unknown | unknown | unknown |
| Reservoir hatch intrusion alarm | unknown | unknown | unknown | unknown |
| ATS Generator on | unknown | unknown | unknown | unknown |
| Generator fault | unknown | unknown | unknown | unknown |
