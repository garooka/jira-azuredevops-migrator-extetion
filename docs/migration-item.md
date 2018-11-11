# Migration item file

This document describes the structure of the migration item file.

## Structure

## Example item

```json
***REMOVED***
    "Type": "Bug",
    "OriginId": "SCRUM-17",
    "WiId": -1,
    "Revisions": [
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  "Author": "some.user@azuredevops.domain",
***REMOVED******REMOVED***  "Time": "2018-01-29T03:46:18.5+01:00",
***REMOVED******REMOVED***  "Index": 0,
***REMOVED******REMOVED***  "Fields": [
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***"ReferenceName": "System.Title",
***REMOVED******REMOVED******REMOVED******REMOVED***"Value": "[SCRUM-17] Instructions for deleting this sample board and project are in the description for this issue >> Click the \"SCRUM-17\" link and read the description tab of the detail view for more"
***REMOVED******REMOVED******REMOVED*** ***REMOVED***,
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***"ReferenceName": "System.AssignedTo",
***REMOVED******REMOVED******REMOVED******REMOVED***"Value": "some.user@azuredevops.domain"
***REMOVED******REMOVED******REMOVED*** ***REMOVED***,
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***"ReferenceName": "System.Description",
***REMOVED******REMOVED******REMOVED******REMOVED***"Value": "*To delete this Sample Project _(must be performed by a user with Administration rights)_* \n- Open the administration interface to the projects page by using the keyboard shortcut 'g' then 'g' and typing 'Projects' in to the search dialog\n- Select the \"Delete\" link for the \"Scrum-Demo\" project\n\n*To delete the Sample Project workflow and workflow scheme _(must be performed by a user with Administration rights)_* \n- Open the administration interface to the workflow schemes page by using the keyboard shortcut 'g' then 'g' and typing 'Workflow Schemes' in to the search dialog\n- Select the \"Delete\" link for the \"SCRUM: Software Simplified Workflow Scheme\" workflow scheme\n- Go to the workflows page by using the keyboard shortcut 'g' then 'g' and typing 'Workflows' in to the search dialog(_OnDemand users should select the second match for Workflows_)\n- Expand the \"Inactive\" section\n- Select the \"Delete\" link for the \"Software Simplified Workflow  for Project SCRUM\" workflow\n\n*To delete this Board _(must be performed by the owner of this Board or an Administrator)_*\n- Click the \"Tools\" cog at the top right of this board\n- Select \"Delete\""
***REMOVED******REMOVED******REMOVED*** ***REMOVED***,
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***"ReferenceName": "Microsoft.VSTS.Common.Priority",
***REMOVED******REMOVED******REMOVED******REMOVED***"Value": 3
***REMOVED******REMOVED******REMOVED*** ***REMOVED***,
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***"ReferenceName": "System.State",
***REMOVED******REMOVED******REMOVED******REMOVED***"Value": "New"
***REMOVED******REMOVED******REMOVED*** ***REMOVED***,
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***"ReferenceName": "Microsoft.VSTS.TCM.ReproSteps",
***REMOVED******REMOVED******REMOVED******REMOVED***"Value": "<style>div ***REMOVED***\r\n    display: block;\r\n***REMOVED***\r\n\r\ntable.confluenceTable ***REMOVED***\r\n    border-collapse: collapse;\r\n    margin: 5px 0 5px 2px;\r\n    width: auto;\r\n***REMOVED***\r\n\r\ntable ***REMOVED***\r\n    display: table;\r\n    border-collapse: separate;\r\n    border-spacing: 2px;\r\n    border-color: grey;\r\n***REMOVED***\r\n\r\ntbody ***REMOVED***\r\n    display: table-row-group;\r\n    vertical-align: middle;\r\n    border-color: inherit;\r\n***REMOVED***\r\n\r\ntr ***REMOVED***\r\n    display: table-row;\r\n    vertical-align: inherit;\r\n    border-color: inherit;\r\n***REMOVED***\r\n\r\nth.confluenceTh ***REMOVED***\r\n    border: 1px solid #ccc;\r\n    background: #f5f5f5;\r\n    padding: 3px 4px;\r\n    text-align: center;\r\n***REMOVED***\r\n\r\nth ***REMOVED***\r\n    font-weight: bold;\r\n    text-align: -internal-center;\r\n***REMOVED***\r\n\r\ntd, th ***REMOVED***\r\n    display: table-cell;\r\n    vertical-align: inherit;\r\n***REMOVED***\r\n\r\n    td.confluenceTd ***REMOVED***\r\n***REMOVED***   border: 1px solid #ccc;\r\n***REMOVED***   padding: 3px 4px;\r\n***REMOVED***\r\n\r\ndfn, cite ***REMOVED***\r\n    font-style: italic;\r\n***REMOVED***\r\n\r\n    cite:before ***REMOVED***\r\n***REMOVED***   content: \"\\2014 \\2009\";\r\n***REMOVED***\r\n</style><p><b>To delete this Sample Project <em>(must be performed by a user with Administration rights)</em></b> </p>\n<ul class=\"alternate\" type=\"square\">\n\t<li>Open the administration interface to the projects page by using the keyboard shortcut 'g' then 'g' and typing 'Projects' in to the search dialog</li>\n\t<li>Select the \"Delete\" link for the \"Scrum-Demo\" project</li>\n</ul>\n\n\n<p><b>To delete the Sample Project workflow and workflow scheme <em>(must be performed by a user with Administration rights)</em></b> </p>\n<ul class=\"alternate\" type=\"square\">\n\t<li>Open the administration interface to the workflow schemes page by using the keyboard shortcut 'g' then 'g' and typing 'Workflow Schemes' in to the search dialog</li>\n\t<li>Select the \"Delete\" link for the \"SCRUM: Software Simplified Workflow Scheme\" workflow scheme</li>\n\t<li>Go to the workflows page by using the keyboard shortcut 'g' then 'g' and typing 'Workflows' in to the search dialog(<em>OnDemand users should select the second match for Workflows</em>)</li>\n\t<li>Expand the \"Inactive\" section</li>\n\t<li>Select the \"Delete\" link for the \"Software Simplified Workflow  for Project SCRUM\" workflow</li>\n</ul>\n\n\n<p><b>To delete this Board <em>(must be performed by the owner of this Board or an Administrator)</em></b></p>\n<ul class=\"alternate\" type=\"square\">\n\t<li>Click the \"Tools\" cog at the top right of this board</li>\n\t<li>Select \"Delete\"</li>\n</ul>\n"
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ],
***REMOVED******REMOVED***  "Links": [],
***REMOVED******REMOVED***  "Attachments": [],
***REMOVED******REMOVED***  "AttachmentReferences": true
***REMOVED***   ***REMOVED***,
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  "Author": "some.user@azuredevops.domain",
***REMOVED******REMOVED***  "Time": "2018-01-29T14:30:18.5+01:00",
***REMOVED******REMOVED***  "Index": 1,
***REMOVED******REMOVED***  "Fields": [
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***"ReferenceName": "System.State",
***REMOVED******REMOVED******REMOVED******REMOVED***"Value": "Committed"
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ],
***REMOVED******REMOVED***  "Links": [],
***REMOVED******REMOVED***  "Attachments": [],
***REMOVED******REMOVED***  "AttachmentReferences": false
***REMOVED***   ***REMOVED***,
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  "Author": "some.user@azuredevops.domain",
***REMOVED******REMOVED***  "Time": "2018-02-01T20:22:18.5+01:00",
***REMOVED******REMOVED***  "Index": 2,
***REMOVED******REMOVED***  "Fields": [
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***"ReferenceName": "System.State",
***REMOVED******REMOVED******REMOVED******REMOVED***"Value": "Done"
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ],
***REMOVED******REMOVED***  "Links": [],
***REMOVED******REMOVED***  "Attachments": [],
***REMOVED******REMOVED***  "AttachmentReferences": false
***REMOVED***   ***REMOVED***,
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  "Author": "some.user@azuredevops.domain",
***REMOVED******REMOVED***  "Time": "2018-02-01T20:22:18.5+01:00",
***REMOVED******REMOVED***  "Index": 3,
***REMOVED******REMOVED***  "Fields": [
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***"ReferenceName": "System.History",
***REMOVED******REMOVED******REMOVED******REMOVED***"Value": "Joined Sample Sprint 2 7 days 9 hours 10 minutes ago"
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ],
***REMOVED******REMOVED***  "Links": [],
***REMOVED******REMOVED***  "Attachments": [],
***REMOVED******REMOVED***  "AttachmentReferences": false
***REMOVED***   ***REMOVED***,
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  "Author": "some.user@azuredevops.domain",
***REMOVED******REMOVED***  "Time": "2018-02-01T20:22:18.5+01:00",
***REMOVED******REMOVED***  "Index": 4,
***REMOVED******REMOVED***  "Fields": [
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***"ReferenceName": "System.History",
***REMOVED******REMOVED******REMOVED******REMOVED***"Value": "To Do to In Progress 6 days 22 hours 26 minutes ago\r\nIn Progress to Done 3 days 16 hours 34 minutes ago"
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ],
***REMOVED******REMOVED***  "Links": [],
***REMOVED******REMOVED***  "Attachments": [],
***REMOVED******REMOVED***  "AttachmentReferences": false
***REMOVED***   ***REMOVED***
    ]
***REMOVED***
```