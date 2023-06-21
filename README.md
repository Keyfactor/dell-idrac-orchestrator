# Dell IDRAC Orchestrator

Certificate inventory and management for Integrated Dell Remote Access Controller appliances

#### Integration status: Prototype - Demonstration quality. Not for use in customer environments.


## About the Keyfactor Universal Orchestrator Extension

This repository contains a Universal Orchestrator Extension which is a plugin to the Keyfactor Universal Orchestrator. Within the Keyfactor Platform, Orchestrators are used to manage “certificate stores” &mdash; collections of certificates and roots of trust that are found within and used by various applications.

The Universal Orchestrator is part of the Keyfactor software distribution and is available via the Keyfactor customer portal. For general instructions on installing Extensions, see the “Keyfactor Command Orchestrator Installation and Configuration Guide” section of the Keyfactor documentation. For configuration details of this specific Extension see below in this readme.

The Universal Orchestrator is the successor to the Windows Orchestrator. This Orchestrator Extension plugin only works with the Universal Orchestrator and does not work with the Windows Orchestrator.




## Support for Dell IDRAC Orchestrator

Dell IDRAC Orchestrator is open source and community supported, meaning that there is **no SLA** applicable for these tools.

###### To report a problem or suggest a new feature, use the **[Issues](../../issues)** tab. If you want to contribute actual bug fixes or proposed enhancements, use the **[Pull requests](../../pulls)** tab.



---




## Keyfactor Version Supported

The minimum version of the Keyfactor Universal Orchestrator Framework needed to run this version of the extension is 10.1

## Platform Specific Notes

The Keyfactor Universal Orchestrator may be installed on either Windows or Linux based platforms. The certificate operations supported by a capability may vary based what platform the capability is installed on. The table below indicates what capabilities are supported based on which platform the encompassing Universal Orchestrator is running.
| Operation | Win | Linux |
|-----|-----|------|
|Supports Management Add|&check; |  |
|Supports Management Remove|  |  |
|Supports Create Store|  |  |
|Supports Discovery|  |  |
|Supports Renrollment|  |  |
|Supports Inventory|&check; |  |





---


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

