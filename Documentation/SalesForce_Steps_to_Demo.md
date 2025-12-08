1.Clear the Broswer cache and close the browser 

2.Powershell run as administrator and do the iisreset

3.Open browser in incognito mode

4.Broswe the following url in the following sequence (make sure you finish the Visitor form submission in the specified time 	<add key="session-storage-expiry-time" value="120"/>  (120 is in seconds)

	a. 	https://preview.tridiondemo.com/
	b.  https://preview.tridiondemo.com/products
	c.	https://preview.tridiondemo.com/details/LightSolarCI
	d.	https://preview.tridiondemo.com/products
	e.	At end of the page  Enter details  and  click request now Button.

5. you will be  redirected to Request Information page (thank you page ) 

6. Now log in to sales force Portal https://rws7-dev-ed.my.salesforce.com/  Username : sbhupali@tridionsites.dev

7. Show the new contact is added https://rws7-dev-ed.lightning.force.com/lightning/o/Contact/list?filterName=__Recent also show the 
segment and the tracking details.

8. Now go back to dxa website and click on Home link in the menu bar you will see the selected segment which you have browsed earlier (residentail or commercial) Promotion will be shown accordingly.


	