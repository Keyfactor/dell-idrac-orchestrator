# cpr-orchestrator-template

## Template for new Orchestrator projects

Use this repository to create new integrations for orcehstrator types. 


## Update the following properties in the integration-manifest.json

* "name": "Friendly name for the integration"
* "description": "Brief description of the integration. This will be used in the readme file generation and should be used for the repository description as well"

For each platform (win and linux) define which capabilities are present for this orchestrator extension. You must update the boolean properties for both win and linux platforms.

* "supportsCreateStore"
* "supportsDiscovery"
* "supportsManagementAdd"
* "supportsManagementRemove"
* "supportsReenrollment"
* "supportsInventory"

### Cert Store Definitions

The integration-manifest.json contains cert-store definitions for use with [kfutil](https://github.com/keyfactor/kfutil).
```
Sample definition
{
  "A10vThunder": {
    "Name": "A10vThunder",
    "ShortName": "vThunderU",
    "Capability": "vThunderU",
    "SupportedOperations": {
      "Add": true,
      "Create": false,
      "Discovery": false,
      "Enrollment": false,
      "Remove": true
    },
    "Properties": [
      {
        "StoreTypeId;omitempty": 0,
        "Name": "protocol",
        "DisplayName": "Protocol",
        "Type": "String",
        "DependsOn": "",
        "DefaultValue": null,
        "Required": true
      },
      {
        "StoreTypeId;omitempty": 0,
        "Name": "allowInvalidCert",
        "DisplayName": "Allow Invalid Cert",
        "Type": "Bool",
        "DependsOn": "",
        "DefaultValue": "false",
        "Required": true
      }
    ],
    "EntryParameters": [],
    "PasswordOptions": {
      "EntrySupported": false,
      "StoreRequired": false,
      "Style": "Default"
    },
    "StorePathType": "",
    "StorePathValue": "",
    "PrivateKeyAllowed": "Optional",
    "JobProperties": [],
    "ServerRequired": true,
    "PowerShell": false,
    "BlueprintAllowed": true,
    "CustomAliasAllowed": "Required"
  },
  
```

----

When the repository is ready for SE Demo, change the following property:
* "status": "pilot"

When the integration has been approved by Support and Delivery teams, change the following property:
* "status": "production"

If the repository is ready to be published in the public catalog, the following properties must be updated:
* "update_catalog": true
* "link_github": true
