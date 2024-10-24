<p align="center">
	<img src="Assets/icons/logo.ico" height="75">
	<img src="/Assets/Presentation/CopyFlyouts.png" height="75">
</p>

<p align="center">
	<img alt="GitHub Downloads (all assets, all releases)" src="https://img.shields.io/github/downloads/krznck/CopyFlyouts/total">
	<img alt="GitHub Release" src="https://img.shields.io/github/v/release/krznck/CopyFlyouts">
</p>

CopyFlyouts is a configurable Windows program meant to give visual and optionally audible feedback to copy operations, with the aim of avoiding situations in which a copy attempt has been made, and yet it is unclear whether the target was correctly copied.

## Disclaimer

While the main purpose and goal of CopyFlyouts is to be a genuinely useful program to improve the experience of copying things on Windows, it is still the summer project of a CS student with little real-life experience, and hence should be considered with the bad code that that can imply in mind.

## Getting started

Install the program via the [setup executable](https://github.com/krznck/CopyFlyouts/releases/latest/download/Setup_CopyFlyouts.exe), or download the [portable version](https://github.com/krznck/CopyFlyouts/releases/latest/download/CopyFlyouts_Portable.zip).

## Showcase

When the user presses Ctrl+C, a flyout appears, showing the contents currently attached to the clipboard.

![Gif showing successful copy attempt](/Assets/Presentation/KeyboardSuccessfulCopy.gif)

If the item inside the clipboard is the same as before the copy attempt, an indicator will show that the clipboard was not updated.
By default, a failure sound will play when this happens.

![Gif showing duplicated copy attempt](/Assets/Presentation/KeyboardDuplicateCopy.gif)

There is also a special warning when the user copies whitespace, or there is nothing in the clipboard.

![Gif showing empty copy attempt](/Assets/Presentation/KeyboardEmptyCopy.gif)

Flyouts can be displayed from copies not done through the keyboard, including copies not initiated by the user (like another program forcefully putting something into the clipboard).

![Gif showing successful mouse copy attempt](Assets/Presentation/NonKeyboardSuccessfulCopy.gif)

CopyFlyouts works with files and images!

![Gif showing image and files copy attempts](/Assets/Presentation/ImageAndFilesCopy.gif)

Both the appearance and behavior of CopyFlyouts is customizable via the settings.

![Image showing a the Behavior tab of the settings window](/Assets/Presentation/SettingsExample.png)

Other useful features of CopyFlyouts are:

- light and dark color schemes
- ability to start minimized on system startup
- ability to minimize to the system tray
- portability to removable drives while retaining settings
- ability to show which process triggered a copy
