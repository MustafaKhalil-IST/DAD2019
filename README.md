# Meetings Scheduler - DAD 2019

[![N|Solid](https://cldup.com/dTxpPi9lDf.thumb.png)](https://nodesource.com/products/nsolid)

[![Build Status](https://travis-ci.org/joemccann/dillinger.svg?branch=master)](https://travis-ci.org/joemccann/dillinger)

# How to Compile?

  - To compile the Server execute compile_server.bat
  - To compile the Client execute compile_client.bat
  - To compile the CommonTypes Library execute compile_common_types.bat
  - To compile the PuppetMaster execute compile_puppet_master.bat

# How to Run Server?

  - In CMD run: 
```sh
$ Server.exe server_id server_url max_faults min_delay max_delay
```
  - make sure that server_url is in the file servers.txt.

# How to Run Client?
  - Make sure that the client_script is in the same directory
  - To run the client script directly run:
```sh
$ Client.exe client_id client_url server_url client_script
```
  - To run the client script step by step
```sh
$ Client.exe client_id client_url server_url client_script steps
```

# How to Run Puppet master?
  - Click on the icon or run in CMD: PuppetMasterApp.exe
