# HoloServer

This package contains a TCP/IP server for remote access to a digital holographic microscope from LyncéeTec written in C# and a matching client using Python 3. It also contains Python scripts to access the client via external commands of 3DPoliCompiler used to control the Laser Nanofactory from Femtika.


## Installation

The server is compiled using Visual Studio from Microsoft. It requires the commercial LyncéeTec-SDK 9.1 delivered with the digital holographic microscope.

To build and install the client package, go to `DhmClient` and run the command

```
python -m pip install .
```

To clean all intermediate files and directories, run
```
python clean.py
```


## Usage examples

See Python scripts in the directory `DhmClient/test`.