# UE4 Audio Debugger

A C# tool to visualize audio events in real-time in Unreal Engine via UDP messages.

## Instructions

In your UE4 C++ project:

* Copy the `UE4UdpSender` folder to your project.

* Right-click your `.uproject` and select "Generate Visual Studio project files".

* In YourProject.Build.cs, in PublicDependencyModuleNames add:

```
"Networking",
"Sockets",
```

Use it like so:

```
auto udpSender = NewObject<UDPSender>();
udpSender->StartUDPSender(
	FString(TEXT("audio-debug-socket")), // Socket name
	FString(TEXT("127.0.0.1")), // IP address of the C# server
	8089 // Port
);

udpSender->SendString(FString(TEXT("Hello world!")));
```
