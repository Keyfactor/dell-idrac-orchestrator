### Prereqs
Orchestrator must run on a Windows Server machine with racadm.exe CLI utility, configured to reach the target IDRAC instance.
Orchestrator must have read and write access to folder with racadm.exe

### Store type
Create a store type with shortname/capability "IDRAC". Select "Add" as supported job type, and check "Needs Server". On advanced, set Private Key Handling: Required. All other defaults are fine, and no parameters are needed.
Default properties have the following meaning:
Store path - Path to folder containing racadm.exe
Client machine - Address of target IDRAC instance
Server username - Username to authenticate racadm.exe to IDRAC
Server password - Password to authenticate racadm.exe to IDRAC

### Supported operations
IDRAC Orchestrator performs inventory operations by executing "racadm.exe sslcertdownload -t ___" with types 1-10.
IDRAC Orchestrator performs management: add jobs by executing "racadm.exe sslkeyupload" and "racadm.exe sslcertupload -t 1". A private key is required to perform the job.

### License
[Apache](https://apache.org/licenses/LICENSE-2.0)
