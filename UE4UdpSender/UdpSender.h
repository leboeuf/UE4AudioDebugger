#pragma once
// UE4
#include "Net/UnrealNetwork.h"
#include "Sockets.h"
// Generated
#include "UdpSender.generated.h"



UCLASS()
class DBM_API UdpSender : public UObject
{
	GENERATED_BODY()
public:
	void SendString(FString message);

	TSharedPtr<FInternetAddr> RemoteAddr;
	FSocket* SenderSocket;

	bool StartUdpSender(
		const FString& socketName,
		const FString& ip,
		const int32 port
	);
};
