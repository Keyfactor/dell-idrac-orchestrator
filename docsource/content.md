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
 

## Requirements

1. The Orchestrator must run on a Windows Server machine with the [Racadm CLI utility](https://www.dell.com/support/home/en-us/drivers/driversdetails?driverid=8gmf6) installed, configured to reach the target iDRAC instance.  The Orchestrator must have read and write access to the folder where racadm.exe is installed.
2. A user id must be set up in iDRAC with minimum privileges of "Configure".  This will be used by the Racadm utility to manage the store.