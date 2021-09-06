
To make it work you need a Ethereum client so the importer can talk with it. 

I will be using Geth: https://geth.ethereum.org/downloads/
Start the deamon with: `geth.exe` and it will start to synchronize. If you want to change the default storage location, just use `--datadir` parameter. You could also run a `light` mode instead of a `full` mode with `--syncmode light`

This is how I run the local node:
```
geth.exe --datadir . --http --http.port "8545" --http.addr "0.0.0.0"
```


Install the Nethereum.Geth package (this will include the Nethereum.Web package too).
`dotnet add package Nethereum.Geth`

When we start geth we will see something like:
```
Mapped network port                      
proto=udp extport=30303 intport=30303 interface=NAT-PMP(192.168.1.1)

Started P2P networking                   
self=enode://83b8ccd06886d1e6d757161a0ed87e0dcd3b88f37e5e08b6d653274d2e7e2da1415eae5000a651ea98012e709e283772719437539b4ed801547edc808f1a6773@192.168.0.180:30303

IPC endpoint opened                      
url=\\.\pipe\geth.ipc

Mapped network port                      
proto=tcp extport=30303 intport=30303 interface=NAT-PMP(192.168.1.1)
```

