<!-- add integration specific information below -->
## Overview

The Integrated Dell Remote Access Controller (iDRAC) Orchestrator Extension supports the following use cases:

- Inventorying the iDRAC instance's server certificate and importing it into Keyfactor Command for management
- Adding or Replacing an existing or newly enrolled certificate and private key to an existing iDRAC instance.  To replace an existing server certificate, the Ovewrite flag in Keyfactor Command must be selected.

Use cases NOT supported by the iDRAC Orchestrator Extension:

- Removing a server certificate from an iDRAC instance.
- Inventorying or Managing any other certificate type on an iDRAC intance.

Special Notes:
* When adding or replacing the server certificate, there will be a few minute delay as the iDRAC instance will restart.  As a result, it may take a few minutes before the new certificate is reflected in subsequent Inventory jobs.
* When replacing an existing server certificate, the Overwrite checkbox must be selected/checked.  When this checkbox is selected, Keyfactor Command may require you to enter an alias.  This alias is not used by the orchestrator extension, so just enter any value.


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
6. In Keyfactor Command, under Orchestrators => Management, approve the orchestrator you just installed the extension on.


## iDRAC Orchestrator Extension Configuration

### Create an iDRAC Certificate Store Type

In Keyfactor Command, create a new certificate store type by navigating to Settings (the "gear" icon in the top right) => Certificate Store Types, and clicking ADD.  Then enter the following information:

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


### Create an iDRAC Certificate Store

Navigate to Certificate Locations =\> Certificate Stores within Keyfactor Command to add the store. Below are the values that should be entered:

- **Category** – Required.  Select the Name you entered when creating the Certificate Store Type.  Suggested value was iDRAC.

- **Container** – Optional.  Select a container if utilized.

- **Client Machine** – Required.  The IP address of the iDRAC instance being managed.  
  
- **Store Path** – Required.  Enter the full path where the Racadm executable is installed on the Orchestrator server.  See [Installation Prerequisites](#installation-prerequisites) for more details.

- **Orchestrator** – Required.  Select the orchestrator you wish to use to manage this store

- **Server Username/Password** - Required.  The credentials used to log into the iDRAC instance being managed.  These values for server login can be either:
  
  - UserId/Password
  - PAM provider information used to look up the UserId/Password credentials

  Please make sure the user id entered has "Configure" privileges on the iDRAC instance.

- **Use SSL** - N/A.  This value is not referenced in the iDRAC Orchestrator Extension.

- **Inventory Schedule** – Set a schedule for running Inventory jobs or "none", if you choose not to schedule Inventory at this time.