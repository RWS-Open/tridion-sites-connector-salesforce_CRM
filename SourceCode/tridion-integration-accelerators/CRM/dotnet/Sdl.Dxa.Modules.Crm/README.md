   CRM Accelerator
 =============================
    
 ## Introduction
    
 The CRM accelerator provides the possibility to create personalized customer experiences via:
 - Direct access to CRM fields to create online forms
 - Capture customer and lead information
 - Track visitor behavior
 - Improve customer segmentation
 - Apply personalization using CRM data
 
 
 ## Installation
 
 To install the CRM accelerator you need to do the following:
 1. Import CMS packages. Do the following in the 'import' folder using Powershell:
    - .\cms-import.ps1 -moduleZip IntegrationForm-v1.0.0.zip
    - .\cms-import.ps1 -moduleZip CRM-v1.0.0.zip   
    In addition the script will ask you the CMS URL (e.g. http://mytridion). By default the script will use the Windows authentication to login. 
    You can give the additional arguments to the script:
    - cmsUrl - URL to Tridion Sites CMS
    - cmsUserName - Username to use to login into Tridion Sites
    - cmsPassword - Password to use
    Example: .\cms-import -cmsUrl http://tridionhost -cmsUserName cmsuser1 -cmsPassword secret -moduleZip CRM-1.0.0.zip
 2. Copy the following files to your DXA installation:
    - Copy files from the 'Areas'-folder to <DXA install dir>\Areas
    - Copy files from the 'bin'-folder to <DXA install dir>\bin
 3. Install CRM connector (like Salesforce) via the Add-on Service. Make sure you're enable it both for Site CM and your staging/live DXD environments.
 4. Add the CRM ECL stub schema (e.g. ExternalContentLibraryStubSchema-salesforce) to 'Allowed Schemas' on the following schema fields:
    - 'externalField' in the schema 'Building Blocks\Modules\IntegrationForm\Editor\Schemas\Form Field'
    - 'externalField' in the schema 'Building Blocks\Modules\IntegrationForm\Editor\Schemas\Static Form Field'
 5. Republish HTML design to get the form style & javascript part of the DXA theme
 6. Republish Settings
 7. Publish the categories 'Integration Form Field Validation' & 'CRM Tracking Category'
 8. As optional step: Create a content type to be used in XPM to easy drag & drop new forms onto pages
 9. Add the following to Web.config (under configuration/appSettings):
 
      <add key="form-field-namespaces" value="[CRM connector namespaces]" />    
      <add key="crm-tracking-entity-types" value="[entities to be used to track, format: [namespace]:[entity type], ... ]" />
      <add key="crm-tracking-entity-name" value="[entity type to use for tracking]" />
      <add key="crm-personalization-entity-name" value="[entity type to use for personalization of CRM content]"/>
 
 TODO: Make the configuration more consistent and use namespace prefix everywhere.
 
 Example of configuration:
 
     <add key="form-field-namespaces" value="Salesforce" />    
     <add key="crm-tracking-entity-types" value="salesforce:Contact" />
     <add key="crm-tracking-entity-name" value="VisitorTracking" />
     <add key="crm-personalization-entity-name" value="Contact"/>
     
 
10. Add the following to the Web.Config (under configuration/modelBuilderPipeline):

      <add type="Sdl.Dxa.Modules.Crm.Personalization.CRMEnrichContentModelBuilder, Sdl.Dxa.Modules.Crm" />
      <add type="Sdl.Dxa.Modules.Crm.Tracking.PageTrackingDispatcher, Sdl.Dxa.Modules.Crm" />

 
## Personalization

T.B.D.

TODO: Describe format of variables
 
 ## Tracking
 
 To setup tracking you need to do the following:
 - Add a few values to the category ‘CRM Tracking Category’ (whatever category that make sense like product names etc)
 - Create a new component of type ‘Tracking Widget’ and one or many tracking categories to it
- Add the component to a page which you want to track (using the ‘Tracking Widget’ template)
- Repeat creation of the tracking widget component for each page you want to track
- Republish the pages
- You can easily verify that the tracking widget is active on pages by using XPM on a page. The widget will appear while in XPM displaying the different categories
- When browsing the site (with an active tracking key that is associated with a CRM contact) all tracked pages are automatically tracked by sending data to Salesforce. You can verify the collected tracking data by viewing the contact information in Salesforce.
- The collected tracking data can then be used to segment the visitors and divide them into different segments. Using this info we can then start to personalize the experience on the web site for the current visitor. For example Salesforce formulas can be used
  to calculate segments and write this into a custom field in the contact.

  
 ## Release Notes
 
 ### 1.0 Beta1
 
 First version of the CRM accelerator including the following core functionality:
 - Form support where fields can be associated with CRM fields
 - Form controller that triggers creation/update of a configurable CRM entity (like a Salesforce contact)
 - Tracking of visitors
 - Personalization of content using CRM data
 
 ### 1.0 Beta2 
 
 Second version of the CRM accelerator with the following changes:
  - Support for read-only fields
 
 ### 1.0 Beta3
 
 Upcoming features in  1.0 B3:
 - Expose of CRM data as ADF claims so the data can be used by XO
 - Update profile widgets
 - Example of login/logout where CRM data is fetched at login

 
 
    