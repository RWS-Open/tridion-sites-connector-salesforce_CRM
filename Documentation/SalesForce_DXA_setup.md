## SalesForce CRM Configuragtion Steps

Download the [SalesForce_CRM_Deployables.zip](https://github.com/RWS-Open/tridion-sites-connector-salesforce_CRM/releases)

1)	Import the cms content from “..\SalesForce_CRM_Deployables\DXA_Files\CRM-Accelerator-1.0-Beta2-OD2025_R3”
	 
	Powershell run as administrator : 

	Navigate to below path using powershell 
	PS ..\SalesForce_CRM_Deployables\DXA_Files\CRM-Accelerator-1.0-Beta2-OD2025_R3\import> 

	Run the command

	.\cms-import -cmsUrl https://sites.tridiondemo.com -cmsUserName "sam" -cmsUserPassword "rws" -moduleZip "IntegrationForm-v1.0.0.zip"

	.\cms-import -cmsUrl https://sites.tridiondemo.com -cmsUserName "sam" -cmsUserPassword "rws" -moduleZip "CRM-v1.0.0.zip"

	Set-XoSettings -OpenSearchUrl "https://os.tridiondemo.com:9200" -Credential (Get-Credential) 
	New-XoTriggerType -Id int_segment -Name "SalesForce CRM segment" -BaseType Text -UrlParam "int_segment" -Values "Residential","Commercial"


2)	Upload the SalesForce CRM  connector  and udp-content-connector-framework-extension-assembly-12.2.0-1061-core to addon Service 
	a.(..\SalesForce_CRM_Deployables\Salesforce-CRM-Addon\)  
	b. connector-framework-extension  (C:\softwares\Tridion_Site\Tridion+Sites+10.1.1-RC1\Content Delivery\roles\content\add-ons\udp-content-connector-framework-extension-assembly-12.2.0-1061-core.zip)
	c.Restart the service content and deployer


3)	Update the Schemas in 100 publicaiton Add the CRM ECL stub schema (e.g. ExternalContentLibraryStubSchema-salesforce) to 'Allowed Schemas' on the following schema fields:
    - 'externalField' in the schema 'Building Blocks\Modules\IntegrationForm\Editor\Schemas\Form Field'
    - 'externalField' in the schema 'Building Blocks\Modules\IntegrationForm\Editor\Schemas\Static Form Field'


4)	At 300 publicaiton Create Residential and Commercial keyword in CRM Tracking category


5)	At 300 Publication Create 2 componets in 300 Publication -> Building Block-> Content-> SalesForce CRM 
														Tracking Widget - Commercial Solar Panels
														Tracking Widget - Residential Solar Panels
			
			
6)	updated schema [Onedemo:DetailsBanner] Region  to accept the tracking widgt.
schema url for easy access https://sites.tridiondemo.com/WebUI/item.aspx?tcm=8#id=tcm:7-667-8
 
 
 
7)	Added the tracking component to
a.LightSolar RI Page Details https://sites.tridiondemo.com/WebUI/item.aspx?tcm=64#id=tcm:7-804-64
b.LightSolar CI Page Details https://sites.tridiondemo.com/WebUI/item.aspx?tcm=64#id=tcm:7-769-64
			


8)	updated schema [Onedemo:RelatedContent] Region to accept integration form compoent https://sites.tridiondemo.com/WebUI/item.aspx?tcm=8#id=tcm:7-652-8



9)	updated schema  Integration Form  to accept media image    https://sites.tridiondemo.com/WebUI/item.aspx?tcm=8#id=tcm:2-10480-8
field details for media 
	a. xml name as image
	b. type : Multimedia Link
	
		
10)	Created a new component of integration form schema  (300->Building Block->Content->Salesforce crm-> Visitor Profile form 
	Create a text fields that connects to salesforce crm fields


11)	Added the Visitor Profile form  to 500-> home->001 Products -> solar products page  under RelatedContent Region


12)	created a Requested Info Banner  in 300-> Building Block -> content -> SalesForce crm -> Requested Info Banner


13)	Under 500 publicaion Create a request information page - thank you  and added the banner Requested Info Banner and releated content to it.


14)	Publish 500 publication 

	
15)	Update the web.config file C:\tridion\Websites\DXA_Preview\Web.config under <appSettings>
		<add key="form-field-types" value="Salesforce" />
		<add key="crm-tracking-entity-types" value="salesforce:Contact" />
		<add key="crm-tracking-entity-name" value="VisitorTracking" />
		<add key="crm-personalization-entity-name" value="Contact"/>
		<add key="session-adf-claims" value="Contact.Id, Contact.Name, Contact.Email, Contact.Segment" />
		<add key="session-storage-expiry-time" value="120"/>         <!-- Ensure that lead generation(Visitor Profile form)(preview.tridiondemo.com/products) form should be submitted within specified time of session-storage-expiry-time -->    
		<add key="crm-tracking-exclude-paths" value="/error-404"/>
		<add key="crm-track-anonymous-visitors" value="true"/>
		
		<add key="form-field-namespaces" value="Salesforce" />
		<add key="form-field-policies" value="JourneyEventTrigger=NameValueList, salesforce:Contact=EntityFields" />
		<add key="form-objectkey-fieldnames" value="JourneyEventTrigger=eventDefinitionKey" />
		
16)	Update the web.config file bindingRedirect in C:\tridion\Websites\DXA_Preview\Web.config under </assemblyBinding>

		  <dependentAssembly>
			<assemblyIdentity name="Tridion.ConnectorFramework.Contracts" publicKeyToken="DDFC895746E5EE6B" culture="neutral" />
			<bindingRedirect oldVersion="0.0.0.0-42.1.0.0" newVersion="42.2.1.0" />
		  </dependentAssembly>
		  <dependentAssembly>
			<assemblyIdentity name="Tridion.ConnectorFramework.Connector.SDK" publicKeyToken="ddfc895746e5ee6b" culture="neutral" />
			<bindingRedirect oldVersion="0.0.0.0-42.0.0.0" newVersion="42.2.1.0" />
		  </dependentAssembly>
		  <dependentAssembly>
			<assemblyIdentity name="Tridion.Remoting.Contracts" publicKeyToken="ddfc895746e5ee6b" culture="neutral" />
			<bindingRedirect oldVersion="0.0.0.0-42.0.0.0" newVersion="42.2.1.0" />
		  </dependentAssembly>
		  <dependentAssembly>
			<assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral" />
			<bindingRedirect oldVersion="0.0.0.0-12.0.0.0" newVersion="13.0.0.0" />
		  </dependentAssembly>
		  
		  
17)	Update the web.config file modelBuilderPipeline in C:\tridion\Websites\DXA_Preview\Web.config under </modelBuilderPipeline>
			<add type="Sdl.Dxa.Modules.Crm.Personalization.CRMEnrichContentModelBuilder, Sdl.Dxa.Modules.Crm" />
			<add type="Sdl.Dxa.Modules.Crm.Tracking.PageTrackingDispatcher, Sdl.Dxa.Modules.Crm" />


18)	Add the Promotion 
			Salesforce CRM - Residential audience
			Where : 500 www en-US
			Pages : 000 Home
			Page Regions : Banner and Hero
			When Triggers : SalesForce CRM segment is Residential 
			What Content Items : Residential Solar Pannels Banner ( Component Template : Banner [Smarttarget:Hero] )
			
		
19)	pdated the C:\tridion\Websites\DXA_Preview\bin\config\cd_ambient_conf.xml Under ForwardedClaims and GloballyAcceptedClaims (in both the tags)
		<Claim Uri="taf:claim:integration:contact:id"/>
			  <Claim Uri="taf:claim:integration:contact:email"/>
			<Claim Uri="taf:claim:integration:contact:name"/>
   			<Claim Uri="taf:claim:integration:contact:segment"/>



20)	Updated the C:\tridion\Websites\DXA_Preview\bin\config\smarttarget_conf.xml under AmbientData->Prefixes
			<taf_claim_integration_contact>int</taf_claim_integration_contact>
						<taf_claim_integration_contact>contact</taf_claim_integration_contact>
			Note : i have removed <taf_claim_visitor>vis</taf_claim_visitor>
			  

21)	copy the bin folder files (..\SalesForce_CRM_Deployables\DXA_Files\CRM-Accelerator-1.0-Beta2-OD2025_R3\bin)  to C:\tridion\Websites\DXA_Preview\Bin\ folder  
			Sdl.Tridion.Api.Client.dll
			Sdl.Dxa.Modules.Crm
			Sdl.Dxa.Integration.Form
			Sdl.Dxa.Integration.Personalization
			Sdl.Dxa.Integration.Client
			Newtonsoft.Json
			Tridion.ConnectorFramework.Connector.SDK
			Tridion.ConnectorFramework.Contracts

	
22)	copy the CRM and IntegrationForm folder from (..\SalesForce_CRM_Deployables\DXA_Files\CRM-Accelerator-1.0-Beta2-OD2025_R3\Areas\)  to C:\tridion\Websites\DXA_Preview\Areas\ folder 
		

	  
23)	update the application.properties (C:\tridion\ContentDelivery\content\config\application.properties   graphql.mutations.enabled to true 
			graphql.mutations.enabled=${mutationsenabled:true} 
		Restart the content service 

