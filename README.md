# Language
- English (Current language)
- [日本語](README_JPN.md)
- [简体中文](README_CHS.md)
- [한국어](README_KOR.md)

# Summary
  - [Description](#description)
  - [Term of use](#term-of-use)
  - [Features](#features)
  - [Setup](#setup)
  - [How to use](#how-to-use)
  - [Support](#support)
  - [Related projects](#related-projects)
  - [Contributor](#contributor)

# Description
`VVVF Simulator` is the software simulates vvvf inverter sound on a PC.<br>
This program is for the C# wpf app.<br>

# Term of use
You are free to use any of the code in this program for noncommerical use.<br>
If you make a video or a modified program using this program then please leave a link to the program in the description.<br>
We are not responsible for anything you do with this application.<br>

# Features
## VVVF Sound Simulation
This application can simulate VVVF sound.<br>

## Motor Sound Simulation
You can simulate the actual sound from a motor and gearbox.<br>
Currently this is a work in progress as it is not very accurate.<br>

## Realtime Audio Generation
You can generate the audio in real time and control.<br>
Key assignment in default is W key to deaccelerate and S key to accelerate.<br>
![2024-11-04](https://github.com/user-attachments/assets/954d67ab-65c4-4298-9514-031b1221be6a)

## Realtime Waveform Output
You can passthrough waveform data through USB serial port.<br>

## File Export
This application can export a `.wav` audio file, a `.avi` video or a `.png` image file in next contents.<br>

|Content|Type(s)|Example|
| --- | --- | --- |
|Line/Phase Voltage|`.wav`<br>`.avi`|Example : Line Voltage waveform export in `.avi`![2022-02-14](https://user-images.githubusercontent.com/77259842/153803020-6615bcce-22a6-4839-b919-ea114dc12d03.png)|
|Motor Sound|`.wav`|
|Voltage Vector|`.avi` <br> `.png`|
|Control Status|`.avi` | Example : Control Status Design 2 ![2022-06-11 (1)](https://user-images.githubusercontent.com/77259842/173188884-72a1290a-6d7b-4354-88e4-cecfa5d0d424.png)|
|Frequency Distribution|`.avi` <br> `.png`|

# Setup
You can have code built on your PC or just download `.exe` file.<br>
## Method1 : Setup with `.exe`
Go to [Releases](https://github.com/VvvfGeeks/VVVF-Simulator/releases), download `VVVF-SIM.zip`.<br>
Extract zip file. You should endup finding `VVVF-Simulator.exe`<br>

## Method2 : Build source code
First, download Visual Studio. Then run the installer and make sure you also select the .NET desktop development in the installer. Once it has installed open Visual Studio and click on "Clone a Repository." Now copy the url of the VVVF-Simulator page.
<br>
https://github.com/VvvfGeeks/VVVF-Simulator
<br>
Paste the url and click clone. Now click on "Solution 'VVVF-Simulator'" and then click on the green arrow to compile and run the program. Now you should see a window open.
<br>

# How to use
## Basic
To load or save a file click on file and select what you want to do from there. Note that if you load a file and click save the program will save to whatever file you loaded last unless you use save as.
## Realtime Generation
To generate sound in realtime click on the RealTime tab. "VVVF RealTime" is the generated pwm played through the audio where "Train RealTime" simulates the sound on a train.
<br>
<br>
By clicking on the settings for either of these you can change the audio buffer size, show the controlling variables, show a vector hexagon, show the waveform, enable realtime editting or show FFT. Note that the more of these that are enabled the more processing power is required.
## Sound Recreation
### PWM Level Setting
First click on "Settings". Now click on "PWM Level." Here you select how many pwm levels are used for the sound. Note that most trains use 2 levels. 
### Minimum Frequency Setting
Now click on "Minimum Frequency." This is where you set the lowest possible frequency that is generated for accelerating and braking. If the control frequency is less than this value the output frequency will remain at this value as long as the control frequency is greater than 0 however amplitude can still change. If you want the minimum output frequency to start from 0 then change the value to 0.
### Jerk Setting
Now click on "Jerk Setting." These settings change the amount of time it takes for the output to fully turn on and off. "Max Voltage Frequency" is the output frequency that the output will go to when turning on or off. "Frequency Change Rate" is the number of cycles per second to get to that value. "Power On" means going turning on the output and "Power Off" means turning off the output. This can be set for both accelerating and braking.
### Acceleration Pattern Creation
#### Add Pattern
Now click on "Accelerate." Here is where you will create the settings for the sound. By clicking the + at the bottom you can add a setting for the sound. You can right click on that setting and click copy or remove if needed. 
#### Set Condition
Now click on the sound and where it says "From" that is the output frequency where the particular setting starts. If it is the first setting then set it to the minimum output frequency you have set or zero.
<br>
<br>
If you want the setting to be active over a specific frequency range you can use "Rotate From" and "Rotate Below." "Rotate from" is the starting frequency for when the setting will have an output and "Rotate Below" is the end frequency for setting which has no output.
<br>
<br>
"Keep" determines if the setting is held on to for accelerating to neutral "Power Off" or neutral to accelerating "Power On." If either of these are off then when there is a transition in modes then if the control frequency is low enough then the next setting will be active. "Enable" determines if the setting has an output for "Normal State," "Power On," or "Power Off."
#### Pulse Mode Setting
There is a dropdown menu near the top under "Pulse Mode Type" where you set the type of pwm.<br>
Available Pulse Modes are shown on next table.<br>

| Mode | Description |
| --- | --- |
|Async|Mode to modulate base waveform by carrier frequency|
|Sync|Mode to modulate base waveform by multiple of base frequency|
|CHM|Meaning of Current Harmonic Minimum|
|SHE|Meaning of Selective Harmonic Elimination|
|HO|Mode to modulate base waveform by special carrier wave|

If you selected "Async" or "Sync" then the following information applies. Clicking "shifted" inverts the carrier wave. By clicking on "Harmonic Setting" you can add harmonics to the modulated wave. The most commonly used if any are space vector PWM or "SVM" and third harmonic pwm or "THIPWM." The default wave as the reference for each phase is a sinewave which is the setting "sinusoidal." By clicking on "sinusoidal" you can change the wave being modulated if needed. The discrete setting changes how many steps are used for generating the reference wave being modulated.
<br>
<br>
If you selected "Async" then the following information applies. Under "Carrier Frequency" there are various settings you can set. "Parameter" is where you set the carrier frequency in hertz. Where it says "Random Modulation" you will see "Range" which the amount the carrier frequency can vary from it's default setting. For example, a 700hz carrier frequency with a 50hz range means the carrier frequency can range from 675 to 725. "Interval" specifies how often the carrier frequency can change in seconds. If you want to have a variable carrier frequency over a range then change "Carrier Frequency" from constant to variable. "Start" and "End" mark the starting and ending frequency for the setting and "Start Value" and "End Value" are the endpoint for the actual carrier frequency you want to use. If you are using the "Three Level Setting" you can change the Dipolar bias under "Dipolar Setting."
<br>
<br>
If you selected "Sync," "SHE," "CHM," or "HO" then the following information applies. "Count" determines the number of PWM pulses. Note that some setting may have an alternate option. 
<br>
<br>
Note that this setting only applies to "Sync." By clicking "Square" the reference wave is changed to a square wave which may be used for some pwm modes. 

#### Modulation Index Setting
"Modulation Setting" is where you set the amplitude settings. In most cases you would want to use "Proportional" but there are other options if necessary. The program calculates the amplitude based in a way like calculating a line between two points. The "Start Frequency" is the output frequency where the amplitude setting starts and "Start Amplitude" is the amplitude at that frequency. "End Frequency" is the output frequency where the amplitude setting ends and "End Amplitude" is the amplitude at that frequency. "Max Amplitude" is the maximum allowed amplitude and "Cutoff Amplitude" is the lower limit for amplitude. There are default amplitude range limits in place but you can disable them if needed. This information also applies to "Power On" and "Power Off"
### Brake Pattern Creation
Note that all of these settings covered for "accelerate" can be used in the same way for a brake setting. 
## Note
I highly suggest looking at the sample VVVF files as messing with those will make it easier to understand how to create you own sounds.

# Support
- Join our [discord](https://discord.gg/SQr2tXJgVq)! You can ask frequently about vvvf simulator!
- [Sample Files](https://github.com/VvvfGeeks/YAML-Files)

# Related projects
 - [Raspberry Pi Pico Drive](https://github.com/VvvfGeeks/RPi-Pico-Drive)
 - [Raspberry Pi Zero Vvvf](https://github.com/VvvfGeeks/RPi-Zero-VVVF)
 - [Raspberry Pi 3 Vvvf](https://github.com/VvvfGeeks/RPi-3-VVVF)
 - [Youtube](https://www.youtube.com/channel/UCdo7fDodYWO29-Q_0G1S59g)

# Contributor
## Early Contributor
 - [Thunderfeng](https://github.com/Leifengfengfeng)
 - [Geek of the Week](https://github.com/geekotw)
## Language Translation
 - Korean
    - [turtle713](https://github.com/intel713)
 - Simplified Chinese
    - [02001](https://github.com/Jerethon)
    - [YAJ-54S12G-1](https://github.com/YJ-305-A2)
