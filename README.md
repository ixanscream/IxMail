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
		
          using (IxMailMessage iM = new IxMailMessage())
            {
                iM.mailBody = new messageClass
                {
                    display = true,
                    message = "this is message",
                    type = "this is type"
                };

                iM.To.Add("ix.habibi@gmail.com");
                iM.Subject = "test";

                EmailSender.SendMailAsync(iM);
            }
            
