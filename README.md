
# Dell iDRAC Orchestrator

Certificate inventory and management for Integrated Dell Remote Access Controller appliances

#### Integration status: Prototype - Demonstration quality. Not for use in customer environments.

## About the Keyfactor Universal Orchestrator Extension

This repository contains a Universal Orchestrator Extension which is a plugin to the Keyfactor Universal Orchestrator. Within the Keyfactor Platform, Orchestrators are used to manage “certificate stores” &mdash; collections of certificates and roots of trust that are found within and used by various applications.

The Universal Orchestrator is part of the Keyfactor software distribution and is available via the Keyfactor customer portal. For general instructions on installing Extensions, see the “Keyfactor Command Orchestrator Installation and Configuration Guide” section of the Keyfactor documentation. For configuration details of this specific Extension see below in this readme.

The Universal Orchestrator is the successor to the Windows Orchestrator. This Orchestrator Extension plugin only works with the Universal Orchestrator and does not work with the Windows Orchestrator.

## Support for Dell iDRAC Orchestrator

Dell iDRAC Orchestrator is supported by Keyfactor for Keyfactor customers. If you have a support issue, please open a support ticket via the Keyfactor Support Portal at https://support.keyfactor.com

###### To report a problem or suggest a new feature, use the **[Issues](../../issues)** tab. If you want to contribute actual bug fixes or proposed enhancements, use the **[Pull requests](../../pulls)** tab.

---


---



## Keyfactor Version Supported

The minimum version of the Keyfactor Universal Orchestrator Framework needed to run this version of the extension is 10.4
## Platform Specific Notes

The Keyfactor Universal Orchestrator may be installed on either Windows or Linux based platforms. The certificate operations supported by a capability may vary based what platform the capability is installed on. The table below indicates what capabilities are supported based on which platform the encompassing Universal Orchestrator is running.
| Operation | Win | Linux |
|-----|-----|------|
|Supports Management Add|&check; |  |
|Supports Management Remove|  |  |
|Supports Create Store|  |  |
|Supports Discovery|  |  |
|Supports Reenrollment|  |  |
|Supports Inventory|&check; |  |


## PAM Integration

This orchestrator extension has the ability to connect to a variety of supported PAM providers to allow for the retrieval of various client hosted secrets right from the orchestrator server itself.  This eliminates the need to set up the PAM integration on Keyfactor Command which may be in an environment that the client does not want to have access to their PAM provider.

The secrets that this orchestrator extension supports for use with a PAM Provider are:

|Name|Description|
|----|-----------|
|ServerUsername|The user id that will be used to authenticate into the Dell Remote Access Controller|
|ServerPassword|The password that will be used to authenticate into the Dell Remote Access Controller|
  

It is not necessary to use a PAM Provider for all of the secrets available above. If a PAM Provider should not be used, simply enter in the actual value to be used, as normal.

If a PAM Provider will be used for one of the fields above, start by referencing the [Keyfactor Integration Catalog](https://keyfactor.github.io/integrations-catalog/content/pam). The GitHub repo for the PAM Provider to be used contains important information such as the format of the `json` needed. What follows is an example but does not reflect the `json` values for all PAM Providers as they have different "instance" and "initialization" parameter names and values.

<details><summary>General PAM Provider Configuration</summary>
<p>



### Example PAM Provider Setup

To use a PAM Provider to resolve a field, in this example the __Server Password__ will be resolved by the `Hashicorp-Vault` provider, first install the PAM Provider extension from the [Keyfactor Integration Catalog](https://keyfactor.github.io/integrations-catalog/content/pam) on the Universal Orchestrator.

Next, complete configuration of the PAM Provider on the UO by editing the `manifest.json` of the __PAM Provider__ (e.g. located at extensions/Hashicorp-Vault/manifest.json). The "initialization" parameters need to be entered here:

~~~ json
  "Keyfactor:PAMProviders:Hashicorp-Vault:InitializationInfo": {
    "Host": "http://127.0.0.1:8200",
    "Path": "v1/secret/data",
    "Token": "xxxxxx"
  }
~~~

After these values are entered, the Orchestrator needs to be restarted to pick up the configuration. Now the PAM Provider can be used on other Orchestrator Extensions.

### Use the PAM Provider
With the PAM Provider configured as an extenion on the UO, a `json` object can be passed instead of an actual value to resolve the field with a PAM Provider. Consult the [Keyfactor Integration Catalog](https://keyfactor.github.io/integrations-catalog/content/pam) for the specific format of the `json` object.

To have the __Server Password__ field resolved by the `Hashicorp-Vault` provider, the corresponding `json` object from the `Hashicorp-Vault` extension needs to be copied and filed in with the correct information:

~~~ json
{"Secret":"my-kv-secret","Key":"myServerPassword"}
~~~

This text would be entered in as the value for the __Server Password__, instead of entering in the actual password. The Orchestrator will attempt to use the PAM Provider to retrieve the __Server Password__. If PAM should not be used, just directly enter in the value for the field.
</p>
</details> 




---


<!-- add integration specific information below -->
## Overview

The Integrated Dell Remote Access Controller (iDRAC) Orchestrator Extension supports the following use cases:

- Inventorying the iDRAC instance's server certificate and importing it into Keyfactor Command for management
- Adding or Replacing an existing or newly enrolled certificate and private key to an existing iDRAC instance.  To replace an existing server certificate, the Ovewrite flag in Keyfactor Command must be selected.

Use cases NOT supported by the F5 Big IQ Orchestrator Extension:

- Removing a server certificate from an iDRAC instance.
- Inventorying or Managing any other certificate type on an iDRAC intance.

Special Note: When adding or replacing the server certificate, there will be a few minute delay as the iDRAC instance will restart.  As a result, it may take a few minutes before the new certificate is reflected in subsequent Inventory jobs.


## Versioning

The version number of a the Integrated Dell Remote Access Controller (iDRAC) Orchestrator Extension can be verified by right clicking on the DellIDRACOrchestrator.dll file in the Extensions/DellIdrac installation folder, selecting Properties, and then clicking on the Details tab.


## Installation Prerequisites

1. The Orchestrator must run on a Windows Server machine with the [Racadm CLI utility](https://www.dell.com/support/home/en-us/drivers/driversdetails?driverid=8gmf6) installed, configured to reach the target iDRAC instance.  The Orchestrator must have read and write access to the folder where racadm.exe is installed.
2. A user id must be set up in iDRAC with minimum privileges of "Configure".  This will be used by the Racadm utility to manage the store.


## iDRAC Orchestrator Extension Installation

1. Stop the Keyfactor Universal Orchestrator Service.
2. In the Keyfactor Orchestrator installation folder (by convention usually C:\Program Files\Keyfactor\Keyfactor Orchestrator), find the "extensions" folder. Underneath that, create a new folder named DellIdrac or another name of your choosing.
3. Download the latest version of the iDRAC Orchestrator Extension from [GitHub](https://github.com/Keyfactor/dell-idrac-orchestrator).
4. Copy the contents of the download installation zip file into the folder created in step 2.
5. Start the Keyfactor Universal Orchestrator Service.


## Create an iDRAC Certificate Store Type

### 1\. In Keyfactor Command, create a new certificate store type by navigating to Settings (the "gear" icon in the top right) => Certificate Store Types, and clicking ADD.  Then enter the following information:

<details>
<summary><b>Basic Tab</b></summary>

- **Name** – Required. The descriptive display name of the new Certificate Store Type.  Suggested => iDRAC
- **Short Name** – Required. This value ***must be*** iDRAC.
- **Custom Capability** - Leave unchecked
- **Supported Job Types** – Select Inventory and Add.
- **General Settings** - Select Needs Server.  Select Blueprint Allowed if you plan to use blueprinting.  Leave Uses PowerShell unchecked.
- **Password Settings** - Leave both options unchecked

</details>

<details>
<summary><b>Advanced Tab</b></summary>

- **Store Path Type** - Select Freeform
- **Supports Custom Alias** - Forbidden
- **Private Key Handling** - Required
- **PFX Password Style** - Default

</details>

<details>
<summary><b>Custom Fields Tab</b></summary>

Not Used

</details>

<details>
<summary><b>Entry Parameters Tab</b></summary>

Not Used

</details>


### 2\. Create an iDRAC Certificate Store

Navigate to Certificate Locations =\> Certificate Stores within Keyfactor Command to add the store. Below are the values that should be entered:

- **Category** – Required.  Select the Name you entered when creating the Certificate Store Type.  Suggested value was iDRAC.

- **Container** – Optional.  Select a container if utilized.

- **Client Machine** – Required.  The IP address of the iDRAC instance being managed.  
  
- **Store Path** – Required.  Enter the full path where the Racadm executable is installed.  See [Installation Prerequisites](#installation-prerequisites) for more details.

- **Orchestrator** – Required.  Select the orchestrator you wish to use to manage this store

- **Server Username/Password** - Required.  The credentials used to log into the iDRAC instance being managed.  These values for server login can be either:
  
  - UserId/Password
  - PAM provider information used to look up the UserId/Password credentials

  Please make sure these credentials have Admin rights on the F5 Big IQ device and can perform SCP functions as described in the F5 Big IQ Prerequisites section above.

- **Use SSL** - N/A.  This value is not referenced in the iDRAC Orchestrator Extension.

- **Inventory Schedule** – Set a schedule for running Inventory jobs or "none", if you choose not to schedule Inventory at this time.

When creating cert store type manually, that store property names and entry parameter names are case sensitive


