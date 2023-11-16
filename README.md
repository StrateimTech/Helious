# Helious
Complete-_ish_ BattleBit recoil compensation

![Gif Example](recoil.gif)
Uses the same concepts for hardware mouse injection as [Cezium](https://github.com/StrateimTech/Cezium)

## Features
- Supports all types of weapon configurations along with scopes. (- Semi/burst guns)

## Cons
- Doesn't support horizontal recoil compensation
- Doesn't support burst & single fire modes

### Requirements
- RPi 4b or newer (Must have USB OTG)
- [HID-API](https://github.com/StrateimTech/HID-API) library [gadget](https://github.com/StrateimTech/hid-api-rs/blob/master/example_gadget.sh) installed on rpi and working. You can follow the first two steps in this [repo](https://github.com/StrateimTech/hid-api-rs#setting-up) if you're confused
- .NET 6 installed on local machine (**NOT on the RPI**), this is for building.

## Installation
```
git clone https://github.com/StrateimTech/Helious
cd ./hid-api-rs
dotnet publish -o build -r linux-arm
```
Transfer all files within the build directory to the RPi.
Don't forget to chmod :)
```
chmod +x ./Helious
sudo ./Helious
```
Requires elevated permission to access ``/dev/`` directory

## Increasing performance
For the most accurate recoil compensation you'll want a weapon setup with the least horizontal recoil even if vertical recoil is high.