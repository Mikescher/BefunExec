![](https://raw.githubusercontent.com/Mikescher/BefunUtils/master/README-FILES/icon_BefunExec.png) BefunExec
=================

BefunExec is a fast Befunge-93 interpreter (without any program-size limitations) and a simple tool to execute and debug the generated Befunge-93 files.

While developing BefunGen I encountered the problem that not many interpreters where able to execute my programs.  
Because of the properties of my generated code I needed an interpreter that was

- really fast, it needed to execute really many executions per second
- able to open and display large to *extremely* large programs
- able too zoom in programs (because they can be large)
- able to debug the program (show stack, output, input, breakpoints, single-step ...)

As you can imagine, I didn't find an interpreter that fitted my needs and so I wrote my own.  
The big point that makes BefunExec unique is it's very high speed. On my machine (and its not really a good one) I reach a maximum speed of **6.5 MHz**. This are over **6 million** executions per second, enough for most of my programs :D.  

![](https://raw.githubusercontent.com/Mikescher/BefunUtils/master/README-FILES/BefunExec_Main.png)

Some other features are (as stated above) the ability to set breakpoints, step slowly through the program and zoom into specific parts.  
Also you are able to capture the program execution as a gif animation.  
One other big feature is the integration of BefunHighlight, which will be explained in the BefunHighlight repository.

BefunHighlight:  
![](https://raw.githubusercontent.com/Mikescher/BefunUtils/master/README-FILES/BefunExec_ESH_example.png)

BefunHighlight (Graph display):  
![](https://raw.githubusercontent.com/Mikescher/BefunUtils/master/README-FILES/BefunExec_ESG_example.png)

Download
========

You can download the binaries from my website [www.mikescher.de](http://www.mikescher.de/programs/view/BefunUtils)

Set Up
======

You need the other [BefunUtils](https://github.com/Mikescher/BefunUtils) projects to run this.  
Follow the setup instructions from BefunUtils: [README](https://github.com/Mikescher/BefunUtils/blob/master/README.md)
