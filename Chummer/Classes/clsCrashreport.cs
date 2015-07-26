using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Chummer
{
	class CrashReport
	{
		public static void BuildFromException(object sender, UnhandledExceptionEventArgs e)
		{
			if (
				MessageBox.Show("Chummer5a crashed.\nDo you want to send a crash report to the developer?", "Crash!",
					MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				CrashReport report = new CrashReport(Guid.NewGuid())
					.AddDefaultData()
					.AddData("exception.txt", e.ExceptionObject.ToString());

				Log.Kill(); //Make sure log object is not used

				try
				{
					string strFile = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "chummerlog.txt";
					report.AddData("chummerlog.txt", new StreamReader(strFile).BaseStream);
				}
				catch(Exception ex)
				{
					report.AddData("chummerlog.txt", ex.ToString());
				}


				//Considering doing some magic with 
				//Application.OpenForms
				//And reflection to all savefiles
				//here

				//try to include default settings file
				try
				{
					string strFilePath = Path.Combine(Environment.CurrentDirectory, "settings");
					strFilePath = Path.Combine(strFilePath, "default.xml");

					report.AddData("default.xml", new StreamReader(strFilePath).BaseStream);
				}
				catch (Exception ex)
				{
					report.AddData("default.xml", ex.ToString());
				}


				report.Send();

			}
		}

		private List<KeyValuePair<String, Stream>> values; 

		/// <summary>
		/// Unique ID for the crash report, makes a user able to refer to a specific report
		/// </summary>
		public Guid Id { get; private set; }

		public CrashReport(Guid repordGuid)
		{
			Id = repordGuid;
			values = new List<KeyValuePair<String, Stream>>();
		}

		public CrashReport AddDefaultData()
		{
			return AddData("info.txt", DefaultInfo());
		}

		private String DefaultInfo()
		{
			StringBuilder report = new StringBuilder();

			try
			{
				//Keep this multiple places for good measure
				report.AppendFormat("Crash ID = {0:B}", Id);
				report.AppendLine();
				//We want to know what crash happened on
#if LEGACY
				report.AppendFormat("Legacy Build");
#elif DEBUG
				report.AppendFormat("Debug Build");
#else
				report.AppendFormat("Release Build");
#endif
				report.AppendLine();
				//Seconadary id for linux systems?
				try
				{
					report.AppendFormat("Machine ID Primary= {0}",
						Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\ CurrentVersion", "ProductId", "Missing"));
					report.AppendLine();
				}
				finally
				{
				}

				report.AppendFormat("CommandLine={0}", Environment.CommandLine);
				report.AppendLine();

				report.AppendFormat("Version={0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
			}
			finally
			{
			}
			return report.ToString();
		}

		public CrashReport AddData(String title, String contents)
		{
			//Convert string to stream
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(contents);
			writer.Flush();
			stream.Position = 0;

			
			return AddData(title, stream);
		}

		public CrashReport AddData(String title, Stream contents)
		{
			values.Add(new KeyValuePair<string, Stream>(title, contents));
			return this;
		}

		public bool Send()
		{
			try
			{
				//Not worried about password, but don't want to place it in clear. Not that this is going to stop anybody
				//But hopefully this barrier keeps it above the lowest hanging fruits
				String password = Encoding.ASCII.GetString(Convert.FromBase64String("Y3Jhc2hkdW1wd29yZHBhc3M="));

				MailAddress address = new MailAddress("chummercrashdumps@gmail.com");
				SmtpClient client = new SmtpClient
				{
					Host = "smtp.gmail.com",
					Port = 587,
					EnableSsl = true,
					DeliveryMethod = SmtpDeliveryMethod.Network,
					UseDefaultCredentials = false,
					Credentials = new NetworkCredential(address.Address, password)
				};

				MailMessage message = new MailMessage(address, address);
				//Forwarding rule used instead?
				//message.CC.Add("chummer5isalive+chummerdump@gmail.com");

				message.Subject = Id.ToString("D");
				message.Body = DefaultInfo();

				//Compression?
				foreach (KeyValuePair<string, Stream> pair in values)
				{
					message.Attachments.Add(new Attachment(pair.Value, pair.Key));
				}
#if !DEBUG


				client.Send(message);
#else
				MessageBox.Show("Not sending mail while debugging, email limit and spaming other devs :P\n" + message);
#endif
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
