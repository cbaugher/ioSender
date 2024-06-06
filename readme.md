## ioSender - a gcode sender for grblHAL and Grbl controllers
---
THIS SOFTWARE IS NOT FOR PRODUCTION USE!!!

It is barely more than a proof of concept for porting the Windows based IoSender to Linux via the Avalonia toolkit.

It will compile on Windows using MS Visual Studio 2022 and also JetBrains Rider, and should at least connect to a GRBLHal controller and run a very basic program.
But there are many things still broken so expect random bugs and crashes.

The hope is to get more people involved to continue the porting effort so that the wonderful IOSender can run on Linux.
For anyone wanting to contribute to this effort... the main thing needed right now is programmers familiar with WPF/Avalonia.
Porting IOSender to Linux is a huge effort.  It's taken me months to get it this far and there is still a ways to go.
I'll never be able to do it myself but with some help I think we can get there.


---

Please check out the [Wiki](https://github.com/terjeio/Grbl-GCode-Sender/wiki) for further details.

8-bit Arduino controllers needs _Toggle DTR_ selected in order to reset the controller on connect. Behaviour may be erratic if not set.

![Toggle DTR](Media/Sender8.png)

---

Latest release is 2.0.43, see the [changelog](changelog.md) for details. 

---

A complete rewrite of my [Grbl CNC Controls library](https://github.com/terjeio/Grbl_CNC_Controls) including a sender application on top of these. It supports new features in [grblHAL](https://github.com/grblHAL) such as manual tool change and [external MPG](https://github.com/terjeio/GRBL_MPG_DRO_BoosterPack) control - and is one of the reasons for writing this library and app. Other senders I have tried does not play nice when a MPG pendant is connected directly to the Grbl processor card...

---

Some UI examples:

![Sender](Media/Sender.png)

Main screen.
<br><br>

![3D view](Media/Sender2.png)

3D view of program, with live update of tool marker.
<br><br>

![3D view](Media/Sender2_XL.png)

XL version, German translation.
<br><br>

![Jog flyout](Media/Sender7.png)

Jogging flyout, supports up to 9 axes. The sender also supports keyboard jogging with \<Shift\> \(speed\) and \<Ctrl\> \(distance\) modifiers.
<br><br>

![Easy configuration](Media/Sender3.png)

Advanced grbl configuration with on-screen documentation. UI is dynamically generated from data in a file and/or from the controller.
<br><br>

![Probing options](Media/Sender4.png)

Probing options.
<br><br>

![Easy configuration](Media/Sender5.png)

Lathe mode.
<br><br>

![Easy configuration](Media/Sender6.png)

Conversational programming for Lathe Mode. Threading requires [grblHAL](https://github.com/grblHAL) controller with driver that has spindle sync support.

---
2023-07-30
