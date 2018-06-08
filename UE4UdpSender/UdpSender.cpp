#pragma once
#include "UdpSender.h"
// UE4
#include "ArrayWriter.h"
#include "IPAddress.h"
#include "Runtime/Networking/Public/Common/UdpSocketBuilder.h"
#include "SocketSubsystem.h"
#include "SharedPointer.h"

bool UdpSender::StartUdpSender(
	const FString& socketName,
	const FString& ip,
	const int32 port
)
{
	// https://wiki.unrealengine.com/UDP_Socket_Sender_Receiver_From_One_UE4_Instance_To_Another
	RemoteAddr = ISocketSubsystem::Get(PLATFORM_SOCKETSUBSYSTEM)->CreateInternetAddr();

	bool bIsValid;
	RemoteAddr->SetIp(*ip, bIsValid);
	RemoteAddr->SetPort(port);

	if (!bIsValid)
	{
		UE_LOG(LogTemp, Log, TEXT("UdpSender: IP Address invalid."));
		return false;
	}

	SenderSocket = FUdpSocketBuilder(*socketName)
		.AsReusable()
		.WithBroadcast();
	
	//Set Send Buffer Size
	int32 SendSize = 2 * 1024;
	SenderSocket->SetSendBufferSize(SendSize, SendSize);

	UE_LOG(LogTemp, Log, TEXT("UdpSender: Initialized successfully."));
	return true;
}

void UdpSender::SendString(FString message)
{
	if (!SenderSocket)
	{
		UE_LOG(LogTemp, Log, TEXT("UdpSender: SendString() ignored because no socket open. You must call StartUdpSender() first."));
		return;
	}

	int32 BytesSent = 0;

	FArrayWriter Writer;
	Writer << message;

	SenderSocket->SendTo(Writer.GetData(), Writer.Num(), BytesSent, *RemoteAddr);

	if (BytesSent <= 0)
	{
		const FString Str = "UdpSender: Socket is valid but the receiver received 0 bytes, make sure it is listening properly.";
		UE_LOG(LogTemp, Error, TEXT("%s"), *Str);
		return;
	}

	UE_LOG(LogTemp, Log, TEXT("UdpSender: BytesSent=%u"), BytesSent);
}