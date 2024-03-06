# Helious
Complete-_ish_ BattleBit recoil compensation

![Gif Example](recoil.gif)
Uses the same concepts for hardware mouse injection as [Cezium](https://github.com/StrateimTech/Cezium) <br>
_Has since improved exponentially as of 3/6/2024_

## Features
- Supports **all** types of weapon configurations along with scopes
- Undetectable by EAC (I've been using HID-API for many months, no ban (I cheat on main lol), Rust + BattleBit no ban +2 years.)

## Cons
- Doesn't support horizontal recoil compensation. (Impossible? to predict)
- Doesn't support burst & single fire modes

### Requirements
- RPi 4b or newer (Must have USB OTG)
- [HID-API](https://github.com/StrateimTech/HID-API) library [gadget](https://github.com/StrateimTech/hid-api-rs/blob/master/example_gadget.sh) installed on rpi and working. You can follow the first two steps in this [repo](https://github.com/StrateimTech/hid-api-rs#setting-up) if you're confused
- .NET 8 installed on local machine (**NOT on the RPI**), this is for compiling the binary.

## Installation
```
git clone https://github.com/StrateimTech/Helious
cd ./Helious
dotnet publish -c Release -o publish -r linux-arm --self-contained true -p:PublishReadyToRun=true
```
Transfer all files within the build directory to the RPi via your choice of method.
Don't forget to chmod :)
```
chmod +x ./Helious
sudo ./Helious
```
Requires elevated permission to access ``/dev/`` directory

## Input Tutorial
> Inputs, (Vertical, Initial, Rpm, Magazine Size, Fov) <br>
> Optional, (Smoothness, Scope (2x, 4x), Global Overflow, Local Overflow) <br>

### Required Inputs<br>
Stock AK74 for example<br>
``1.40, 1.0, 670, 30, 110``
* 1.40 = Vertical Recoil
* 1.0 = First Shot Kick
* 670 = Firerate
* 30 = Magazine capacity
* 110 = FOV <br>

Most of these can be found on the bottom right when choosing/modifying a weapon.
![gun-stats-example.png](gun-stats-example.png)

### Optional Inputs<br>
``1.40, 1.0, 670, 30, 110, 8, 2x, true, true``
* 8 = Smoothing (Divides the calculated recoil 8 times over a period smoothing it visually) (Use **0** if you want to leave it at default) (default: 0)
* 2x = Scope zoom (1.4x, 3.0x) (default: 1.0x)
* true = Attempts to compensate for loss in accuracy after each bullet. (default: true)
* true = ^, instead locally compensates for accuracy loss during smoothing process. (default: true)

## Tips
For the most accurate recoil compensation you'll want a weapon setup with the least horizontal recoil even if vertical recoil is high. (e.g. using "B C M- Gun Fighter" grip as it provides the most horizontal recoil reduction atm)