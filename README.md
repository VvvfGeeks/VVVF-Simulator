# Summary
  - [Description](#description)
  - [Term of use](#term-of-use)
  - [Functions](#functions)
  - [Setup](#setup)
  - [How to use](#how-to-use)
  - [Support](#support)
  - [Related projects](#related-projects)
  - [Contributor](#contributor)

# Description
`VVVF-Simulator` is the software simulates vvvf inverter sound on a PC.<br>
This program is for the C# wpf app.<br>

# Term of use
You are **free** to use the code in this program.<br>
We are **not responsible** for anything you do with this application.<br>

Please:<br>
- Post the URL of this GitHub page<br>

Don’t:<br>
- Release modified code without referencing this page.<br>

# Functions
## VVVF Audio Generation
This application will export simulated vvvf inverter sound in the `.wav` extension.<br>
The sampling frequency is 192kHz.<br>

## Waveform Video Generation
This application will export video as a `.avi` extension.
![2022-02-14](https://user-images.githubusercontent.com/77259842/153803020-6615bcce-22a6-4839-b919-ea114dc12d03.png)

## Voltage Vector Video Generation
This application will export video as a `.avi` extension.

## Control stat Video Generation
This application can export video of the control stat values.<br>
The file will be the`.avi` extension. <br>
![2022-06-11 (1)](https://user-images.githubusercontent.com/77259842/173188884-72a1290a-6d7b-4354-88e4-cecfa5d0d424.png)

## Realtime Audio Generation
You can generate the audio in real time and control if the sound increases or decreases in frequency as well as the rate that the frequency increases or decreases. <br>

# Setup
You can have code built on your PC or just download exe file.<br>
## Setup with EXE
Go to [Releases](https://github.com/VvvfGeeks/VVVF-Simulator/releases), download `VVVF-SIM.zip`.<br>
Extract zip file. You should endup finding `VVVF-Simulator.exe`<br>

## Setup with source code on Visual Studio
First, download Visual Studio. Then run the installer and make sure you also select the .NET desktop development in the installer. Once it has installed open Visual Studio and click on "Clone a Repository." Now copy the url of the VVVF-Simulator page.
<br>
https://github.com/VvvfGeeks/VVVF-Simulator
<br>
Paste the url and click clone. Now click on "Solution 'VVVF-Simulator'" and then click on the green arrow to compile and run the program. Now you should see a window open.
<br>

# How to Use
To load or save a file click on file and select what you want to do from there.
<br>
<br>
To generate sound in realtime click on the RealTime tab. "VVVF RealTime" is the generated pwm played through the audio where "Train RealTime" simulates the sound on a train.
<br>
<br>
By clicking on the settings for either of these you can change the audio buffer size, show the controlling variables, show a vector hexagon, show the waveform, enable realtime editting or show FFT. Note that the more of these that are enabled the more processing power is required.
<br>
<br>
Now for creating a sound:
<br>
<br>
First click on "Settings". Now click on "PWM Level." Here you select how many pwm levels are used for the sound. Note that most trains use 2 levels. 
<br>
<br>
Now click on "Minimum Frequency." This is where you set the lowest possible frequency that is generated for accelerating and braking. If the control frequency is less than this value the output frequency will remain at this value as long as the control frequency is greater than 0 however amplitude can still change. If you want the minimum output frequency to start from 0 then change the value to 0.
<br>
<br>
Now click on "Jerk Control." These settings change the amount of time it takes for the output to fully turn on and off. "Freq Goto" is the output frequency that the output will go to when turning on or off. "Change Rate" is the number of cycles per second to get to that value. "Mascon On" means going turning on the output and "Mascon Off" means turning off the output. This can be set for both accelerating and braking.
<br>
<br>
Now click on "Accelerate." Here is where you will create the settings for the sound. By clicking the + at the bottom you can add a setting for the sound. You can click on that setting and click - to remove it. 
<br>
<br>
Now click on the sound and where it says "From" that is the output frequency where the particular setting starts. If it is the first one then set it to the minimum output frequency you have set.
<br>
<br>
There is a dropdown menu near the top under "Basic Setting" where you set the type of pwm. Asyc means the carrier frequency is not synchronized with the output. "P_Wide_3" is the wide three pulse mode pwm. Any of the settings that are "P_" are synchronous pwm modes meaning the carrier frequency is a multiple of the output. The modes that say "CHM" are the current harmonic minimum pwm modes and "SHE" are selective harmonic elimination pwm. By clicking on shifted you can invert the carrier signal which changes the shape of the output. By clicking on "Harmonic Setting" you can add harmonics to the modulated wave. The dropbox below can change the modulated wave.
<br>
<br>
If you selected "Async" there are some options for it. You can change the Dipolar bias under "Dipolar Setting." Note that most trains don't use this so it probably can stay at -1. Where it says "Param" is where you set the carrier frequency in hertz. Where it says "Random" and "Range" is the amount the carrier frequency can vary otherwise known as random modulation. For example 700hz carrier frequency with a 50hz range means the carrier frequency can range from 675 to 725. The "Interval" is how frequently the carrier frequency randomly changes.
<br>
<br>
Below you can select if the setting is enabled normally or on freerun turning off or freerun turning on.
<br>
<br>
Now is where you set the amplitude setting. In most cases you would want to use "Linear" but there are other options if necessary. The program calculates the amplitude based in a way like calculating a line between two points. The "Start Freq" is the output frequency where the amplitude setting starts and "Start Amp" is the amplitude at that frequency. "End Freq" is the output frequency where the amplitude setting ends and "End Amp" is the amplitude at that frequency. "Max Amp" is the maximum allowed amplitude and "Cutoff Amp" is the lower limit for amplitude. By using the dropdown menus under "Dipolar Setting" and "Carrier Frequency Setting" you can change if these values are dependent on the output frequency.
<br>
<br>
Note that all of these settings covered are changed in the same way for a brake setting. 
<br>
<br>
I highly suggest looking at the sample VVVF files as messing with those will make it easier to understand how to create you own sounds.
<br>
<br>

# Support
- Join our [discord](https://discord.gg/SQr2tXJgVq)! You can ask frequently about vvvf simulator!
- [Sample Files](https://github.com/VvvfGeeks/VVVF-Simulator/releases/download/v1.9.0.1/yaml_samples.zip)

# Related projects
 - [Raspberry Pi Zero Vvvf](https://github.com/VvvfGeeks/RPi-Zero-VVVF)
 - [Raspberry Pi 3 Vvvf](https://github.com/VvvfGeeks/RPi-3-VVVF)
 - [Youtube](https://www.youtube.com/channel/UCdo7fDodYWO29-Q_0G1S59g)

# Contributor
## Early Contributor
 - [Thunderfeng](https://github.com/Leifengfengfeng)
 - [Geek of the Week](https://github.com/geekotw)
## Language Translation
 - turtle713
 - 02001
