# IxMail
Mail ASP.NET Helper tamplating

install this package using nuget package visual studio : 

PM> Install-Package IxMail 

after installation you'll see your web.config file change like this :


		<mailSettings>
			<!-- Method#1: Configure smtp server credentials -->
			<smtp from="user@gmail.com">
				<network enableSsl="true" host="smtp.gmail.com" port="587" userName="user@gmail.com" password="password" />
			</smtp>
			
			<!-- Method#2: Dump emails to a local directory -->
			<!--
			<smtp from="some-email@gmail.com" deliveryMethod="SpecifiedPickupDirectory">
				<network host="localhost"/>
				<specifiedPickupDirectory pickupDirectoryLocation="c:\temp\"/>
			</smtp>
			-->
		</mailSettings>
		
		just run the code below :
		
		
    after you run this code (below), you'll see hidden folder ixMail, you can add your custom tamplate there.
    you can add attachment or whatever, into code written above.
		
            IxMailMessage iM = new IxMailMessage();
            iM.mailBody = new SelectListItem { Value = "Value", Text = "Text", Selected = true };
            iM.To.Add("emailto");
            iM.Subject = "this is subject";
            EmailSender.SendMailAsync(iM);
            
    how to create your tamplate?
    
    
    you create txtfile or html, with object example :
    
    object to send : new SelectListItem { Value = "Value", Text = "Text", Selected = true };
    
    you create your html file just like this
    
    <html>
    <head>
    </head>
    <body>
        Value : {{Value}}
        Text : {{Text}}
        Selected : {{Selected}}
    </body>
    </html>
    
    save it, place it into IxMail folder
    the separator {{ }} will replace your value.
    
    
            
