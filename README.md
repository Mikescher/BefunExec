![](https://raw.githubusercontent.com/Mikescher/BefunUtils/master/README-FILES/icon_BefunExec.png) BefunExec [![Build status](https://ci.appveyor.com/api/projects/status/u10tua2nyn5pyr6x?svg=true)](https://ci.appveyor.com/project/Mikescher/befunexec)
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

You can download the binaries from my website **[www.mikescher.com](http://www.mikescher.com/programs/view/BefunUtils)**

Or you can get the latest [Github release](https://github.com/Mikescher/BefunExec/releases/latest) (In case AppVeyor is down)

Or you can download the latest (nightly) version from the **[AppVeyor build server](https://ci.appveyor.com/project/Mikescher/BefunExec/build/artifacts)**

Set Up
======

*This program was developed under Windows with Visual Studio.*

You need the other [BefunUtils](https://github.com/Mikescher/BefunUtils) projects *(especially BefunHighlight)* to run this.  
Follow the setup instructions from BefunUtils: [README](https://github.com/Mikescher/BefunUtils/blob/master/README.md)


Feature overview / Manual
=========================

 - Click on a field to create a breakpoint
 - Middle click on a field to add the field to the watch list, middle click again to change the display mode
 - Use preprocessor statements in you code to automatically watch specific fields / break on specific fields etc (see chapter below)
 - Press ctrl+R to *smart reload* the file. (If the overall size hasn't changed the current program state ist not lost, kinda like *Edit and compile*)
 - Press left in paused mode to undo the last program step (needs the option UndoLog enabled)
 - Use the key `1` to `5` to change the execution speed
 - Draw a box to zoom in
 - Shift+Drag to move the current view
 - Press TAB to view execution speed, program info etc
 - Press TAB with extended Syntaxhighlighting to see a graph of the possible program flow (uses *BefunHighlight* library)
 - **Read the commandline output for all the options etc**


Preprocessor
============

You can insert a few special preprocessor lines in you befunge code:

~~~
#$watch[1,2]:int = my_var_name
~~~
This line adds the field `[1,2]` to the list of watched fields (with description `my_var_name`).
Supported display types are `int`, `long`, `char`, `hex`, `hex8` and `binary`

~~~
#$break[10,5]
~~~
This line adds a breakpoint at `[10,5]`

~~~
#$replace {V01} -> 55+3g
~~~
This line replaces all occurrences of `{V01}` with `55+3g` (! Please note that left and right side must be of equal size) 


GIF Capturing
=============

Under the menu tools you can use the `Capture GIF` option to make a small (possible looping) animation of you program.


Contributions
=============

Yes, please
