using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;

namespace CC.Web
{
	public class SmtpClientWrapper : IDisposable
	{
		private System.Net.Mail.SmtpClient smtpClient;
		private log4net.ILog log = log4net.LogManager.GetLogger(typeof(SmtpClientWrapper));
		public SmtpClientWrapper()
		{
			smtpClient = new System.Net.Mail.SmtpClient();
		}

		//
		// Summary:
		//     Sends the specified message to an SMTP server for delivery.
		//
		// Parameters:
		//   message:
		//     A System.Net.Mail.MailMessage that contains the message to send.
		//
		// Exceptions:
		//   System.ArgumentNullException:
		//     message is null.
		//
		//   System.InvalidOperationException:
		//     This System.Net.Mail.SmtpClient has a Overload:System.Net.Mail.SmtpClient.SendAsync
		//     call in progress.-or- System.Net.Mail.MailMessage.From is null.-or- There
		//     are no recipients specified in System.Net.Mail.MailMessage.To, System.Net.Mail.MailMessage.CC,
		//     and System.Net.Mail.MailMessage.Bcc properties.-or- System.Net.Mail.SmtpClient.DeliveryMethod
		//     property is set to System.Net.Mail.SmtpDeliveryMethod.Network and System.Net.Mail.SmtpClient.Host
		//     is null.-or-System.Net.Mail.SmtpClient.DeliveryMethod property is set to
		//     System.Net.Mail.SmtpDeliveryMethod.Network and System.Net.Mail.SmtpClient.Host
		//     is equal to the empty string ("").-or- System.Net.Mail.SmtpClient.DeliveryMethod
		//     property is set to System.Net.Mail.SmtpDeliveryMethod.Network and System.Net.Mail.SmtpClient.Port
		//     is zero, a negative number, or greater than 65,535.
		//
		//   System.ObjectDisposedException:
		//     This object has been disposed.
		//
		//   System.Net.Mail.SmtpException:
		//     The connection to the SMTP server failed.-or-Authentication failed.-or-The
		//     operation timed out.-or-System.Net.Mail.SmtpClient.EnableSsl is set to true
		//     but the System.Net.Mail.SmtpClient.DeliveryMethod property is set to System.Net.Mail.SmtpDeliveryMethod.SpecifiedPickupDirectory
		//     or System.Net.Mail.SmtpDeliveryMethod.PickupDirectoryFromIis.-or-System.Net.Mail.SmtpClient.EnableSsl
		//     is set to true, but the SMTP mail server did not advertise STARTTLS in the
		//     response to the EHLO command.
		//
		//   System.Net.Mail.SmtpFailedRecipientsException:
		//     The message could not be delivered to one or more of the recipients in System.Net.Mail.MailMessage.To,
		//     System.Net.Mail.MailMessage.CC, or System.Net.Mail.MailMessage.Bcc.
		public void Send(MailMessage message)
		{
			try
			{
				smtpClient.Send(message);
				log.Info(ConvertToString(message));
			}
			catch (Exception ex)
			{
				log.Error(ConvertToString(message), ex);
				throw;
			}
		}
		private static string ConvertToString(MailMessage message)
		{
			return string.Format("To:{0}\nCC:{1}\nBCC:{2}\nSubject:{3}\nBody:{4}",
				message.To,
				message.CC,
				message.Bcc,
				message.Subject,
				message.Body);
		}

		public void Dispose()
		{
			if (smtpClient != null) smtpClient.Dispose();
		}

		internal void SendAsync(MailMessage msg, object userToken)
		{
			smtpClient.SendAsync(msg, userToken);
		}

		
	}
}