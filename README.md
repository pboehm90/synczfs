SyncZFS — The managed ZFS syncing solution
=====================

General information
-------------------
SyncZFS is an in .NET Core implemented syncing solution for the ZFS file system.
Easy to use and reliabilty are the main targets of this project.

Build
-----------------------
You need simply an the .NET Core SDK installed. For Code editing and debugging i would sugggest VS Code with the C# Plugin.
```sh
# git clone https://github.com/pboehm90/synczfs.git
$ cd synczfs
$ dotnet build
```

Installing
-----------------------
1. You have to install the .NET Core runtime.
- [manual install Instructions](https://docs.microsoft.com/en-us/dotnet/core/install/)
2. Get the binaries from release or compile yourself. Put the binaries in an folder you want.

Run
-----------------------
In this example the local dataset tank/virtual_machines would replicated to the remote machine 'backup-server'.
Important: you need password-less ssh access to the destination machine!

To setup password-less ssh access
```sh
$ ssh-keygen
  --> follow the instructions
$ ssh-copy-id -i ~/.ssh/id_rsa.pub root@backup-server
$ ssh backup-server
  --> check if login works
```

(Example) Now you can sync the dataset over ssh!
```sh
$ dotnet /opt/synczfs-bin/synczfs.dll <job-name> <source> <destination> -r
$ dotnet /opt/synczfs-bin/synczfs.dll backup_vms tank/virtual_machines root@backup-server://tank/virtual_machines -r
```

Command line options
-----------------------
+ Parameter 1 - job-name
  Set the job name, please set different names for different targets to archieve snapshot independence
  
+ Parameter 2 - source
  Here you have to specify the source. There are few possible arguments.
  ```sh
  <poolname>/<dataset>...
  ```
  If you want to use a ssh source you have to use the prefix like this:
  ```sh
  <user>@<ip/hostname>://
  ```
  
+ Parameter 3 - destination
  like Parameter 2
  
+ -r
 	the root-source dataset and the childs will be recursively replicated

Contributing
-----------------------
Please feel free to request feature requests or bug reports. And do not forget to send the log-output if an problem occurs! Pull requests are welcome, too.
