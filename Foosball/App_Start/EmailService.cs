using Microsoft.AspNet.Identity;
using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Foosball
{
	// email service for sending through SendGrid
	public class SendGridEmailService : IIdentityMessageService
	{
		public async Task SendAsync(IdentityMessage message)
		{
			await Task.FromResult(0);

			//// Create the email object first, then add the properties.
			//var myMessage = new SendGridMessage();

			//// this defines email and name of the sender
			//myMessage.From = new MailAddress("no-reply@tech.trailmax.info", "My Awesome Admin");

			//// set where we are sending the email
			//myMessage.AddTo(message.Destination);

			//myMessage.Subject = message.Subject;

			//// make sure all your messages are formatted as HTML
			//myMessage.Html = message.Body;

			//// Create credentials, specifying your SendGrid username and password.
			//var credentials = new NetworkCredential(ConfigurationManager.AppSettings["mailAccount"], ConfigurationManager.AppSettings["mailPassword"]);

			//// Create an Web transport for sending email.
			//var transportWeb = new Web(credentials);

			//// Send the email.
			//await transportWeb.DeliverAsync(myMessage);
		}
	}

	public class GmailEmailService : IIdentityMessageService
	{
		public async Task SendAsync(IdentityMessage message)
		{
			var mailAccount = ConfigurationManager.AppSettings["mailAccount"];
			if (string.IsNullOrEmpty(mailAccount))
			{
				return;
			}

            var msg = new MailMessage
			{
				From = new MailAddress(mailAccount, "NFL Pool", System.Text.Encoding.UTF8),
				Subject = message.Subject,
				Body = message.Body,
				IsBodyHtml = true
			};
			msg.To.Add(new MailAddress(message.Destination));

			var smtpClient = new SmtpClient("smtp.gmail.com", 587)
			{
				Credentials = new NetworkCredential(mailAccount, ConfigurationManager.AppSettings["mailPassword"]),
				EnableSsl = true
			};

			smtpClient.Send(msg);
		}
	}
}