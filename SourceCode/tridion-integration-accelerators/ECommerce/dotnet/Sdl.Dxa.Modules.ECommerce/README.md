  ECommerce Accelerator
 =============================
    
 ## Introduction
    
 The ECommerce accelerator provides the possibility to do mashup of commerce and Tridion content including:
 
 - Render product information on web pages
 - Localization of product information using Blueprint mappings
 - Semantic mapping of product data on DXA view models (possibility to have models with both Tridion content & Commerce data)
 - Render product categories
 - Perform product search
 - Use product facets to navigate product listers/search results
 
## Semantic Modelling

T.B.D.

## Installation
 
 To install the ECommerce accelerator you need to do the following:
  1. Import CMS packages. Do the following in the 'import' folder using Powershell:
     - .\cms-import.ps1 -moduleZip ECommerce-v1.0.0.zip   
     In addition the script will ask you the CMS URL (e.g. http://mytridion). By default the script will use the Windows authentication to login. 
     You can give the additional arguments to the script:
     - cmsUrl - URL to Tridion Sites CMS
     - cmsUserName - Username to use to login into Tridion Sites
     - cmsPassword - Password to use
     Example: .\cms-import -cmsUrl http://tridionhost -cmsUserName cmsuser1 -cmsPassword secret -moduleZip ECommerce-1.0.0.zip
  2. Copy the following files to your DXA installation:
     - Copy files from the 'Areas'-folder to <DXA install dir>\Areas
     - Copy files from the 'bin'-folder to <DXA install dir>\bin
  3. Install ECommerce connector (like SAP Commerce, Magento etc) via the Add-on Service. Make sure you're enable it both for Site CM and your staging/live DXD environments.
  4. Add the CRM ECL stub schema (e.g. ExternalContentLibraryStubSchema-sapcommerce) to 'Linked Schemas' on the following component templates:
     -  Building Blocks\Modules\ECommerce\Editor\Templates\Product
  5. Republish Settings
  6. Add the following to Web.config (under configuration/appSettings):
  
       <add key="ecommerce-namespaces" value="[ECommerce connector namespaces]" />    
  
  Example of configuration:
  
       <add key="ecommerce-namespaces" value="sapcommerce" />
      
 
 ## Release Notes
  
 ### 1.0 Beta1
  
First version of the ECommerce accelerator including the following core functionality:

- Read product data via Commerce connector
- Render product data on DXA web pages
- Localization of product information using Blueprint mappings

### 1.0 Beta2 (future release)

- Semantic mapping of commerce data
- Product lister widget

  