# pingfixtest

A simple ping test program in a linux container to test the low ttl bug in .NET '7.0.100-preview.6.22352.1'

## Problem

The PING TTL is set in sequence from one to 30 to seek each hop to 8.8.8.8 in order to discover the route to 8.8.8.8

On Windows and Apple Mac running froom the IDE natively, PING works as expected, with most responding with TtlExpired and some with Timdeout. This is as per OS standard ping utilties.

When running inside a Linux container on either development host type. The results are unexpected with EVERY hop leading to the destination returning Timedout.

## Known facts.

Using wireshark on the host and tcpdump on the linux container, it has been established that the ICMP packets exit and return from the container OS via the host, out to and back from the remote node as expected, being recieved within the linux container OS.

Using another experiment it has been established that in the linux container environment the PING partial classes inside System.Net.NetworkInformation.Ping, the run time code is working down through the layer that uses Sockets, and NOT the call out externally to use the OS version of Ping.

Assumption: Something either in the Sockets implemntation or the Ping use of sockets is not 'seeing' the returned ICMP messages for some reason and leads the Ping code to 'assume' a Timedout status in a scenario where a message was recieved by the OS.

One of the .NET team has already identified already that the call to the socket to receive the returning ICMP message returns nothing.

```
 int bytesReceived = socket.ReceiveFrom(receiveBuffer, SocketFlags.None, ref socketConfig.EndPoint);
```

Perhaps this is a sockets issue on linux containers rather than a Ping issue? 

## How to use and reproduce the problem

Clone this repo and use as you desire.

### Clone this repo

```
git clone https://github.com/IainStevenson/pingfixtest.git
cd pingfixtest
```

### Command line

```
dotnet build pingfixtest.csproj
docker build -t pingfixtest .
docker run -t pingfixtest
```

And you will see 

```
Hop: 1 Status: TimedOut Address: 0.0.0.0
Hop: 2 Status: TimedOut Address: 0.0.0.0
Hop: 3 Status: TimedOut Address: 0.0.0.0
Hop: 4 Status: TimedOut Address: 0.0.0.0
Hop: 5 Status: TimedOut Address: 0.0.0.0
Hop: 6 Status: TimedOut Address: 0.0.0.0
Hop: 7 Status: TimedOut Address: 0.0.0.0
Hop: 8 Status: TimedOut Address: 0.0.0.0
Hop: 9 Status: TimedOut Address: 0.0.0.0
Hop: 10 Status: TimedOut Address: 0.0.0.0
Hop: 11 Status: TimedOut Address: 0.0.0.0
Hop: 12 Status: TimedOut Address: 0.0.0.0
Hop: 13 Status: TimedOut Address: 0.0.0.0
Hop: 14 Status: TimedOut Address: 0.0.0.0
Hop: 15 Status: Success Address: 8.8.8.8
```

### IDE

Open 'PingFixTest.sln'  in Visual studio 2022 or any IDE that supports .NET 7 Preview 6

Run the Docker profile for the docker container test. This requries linux containers in docker for your environment. This behaves the same in Windows and Mac hosts.

![][image_ref_fan5wq7e]

You will see this in your Debug view.

```
Hop: 1 Status: TimedOut Address: 0.0.0.0
Hop: 2 Status: TimedOut Address: 0.0.0.0
Hop: 3 Status: TimedOut Address: 0.0.0.0
Hop: 4 Status: TimedOut Address: 0.0.0.0
Hop: 5 Status: TimedOut Address: 0.0.0.0
Hop: 6 Status: TimedOut Address: 0.0.0.0
Hop: 7 Status: TimedOut Address: 0.0.0.0
Hop: 8 Status: TimedOut Address: 0.0.0.0
Hop: 9 Status: TimedOut Address: 0.0.0.0
Hop: 10 Status: TimedOut Address: 0.0.0.0
Hop: 11 Status: TimedOut Address: 0.0.0.0
Hop: 12 Status: TimedOut Address: 0.0.0.0
Hop: 13 Status: TimedOut Address: 0.0.0.0
Hop: 14 Status: TimedOut Address: 0.0.0.0
Hop: 15 Status: Success Address: 8.8.8.8
```

Now: Run the PingFixTest profile to run it natively on your host.

You will see this (similar according to your location) in your console/terminal.

```
Hop: 1 Status: TtlExpired Address: 192.168.0.1
Hop: 2 Status: TtlExpired Address: 192.168.1.1
Hop: 3 Status: TimedOut Address: 8.8.8.8
Hop: 4 Status: TtlExpired Address: 10.248.28.65
Hop: 5 Status: TtlExpired Address: 10.247.87.25
Hop: 6 Status: TimedOut Address: 8.8.8.8
Hop: 7 Status: TtlExpired Address: 10.247.87.9
Hop: 8 Status: TtlExpired Address: 10.247.87.18
Hop: 9 Status: TtlExpired Address: 87.237.20.218
Hop: 10 Status: TtlExpired Address: 87.237.20.67
Hop: 11 Status: TtlExpired Address: 72.14.242.70
Hop: 12 Status: TtlExpired Address: 74.125.242.97
Hop: 13 Status: TtlExpired Address: 172.253.66.101
Hop: 14 Status: Success Address: 8.8.8.8
```


[image_ref_fan5wq7e]: data:image/jpeg;base64,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
