<%@ WebService Language="C#" Class="omae" %>

using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Web.Mail;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;

[WebService(Namespace = "http://www.dndjunkie.com/")]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]
public class omae : System.Web.Services.WebService
{
	public omae()
	{
	}
	
	public const string CONNECTION_STRING = "Server=10.10.10.3;Database=DNDJunkie;User ID=dndjunkie;Password=hxcr5k2;Trusted_Connection=False;";

	// User Account Methods.
	[WebMethod(Description = "Register a new Omae account.")]
	public int RegisterUser(string strUserName, string strPassword)
	{
		SqlConnection objConnection = new SqlConnection(CONNECTION_STRING);
		objConnection.Open();

		// See if someone with that username already exists.
		SqlCommand objCommand = new SqlCommand();
		objCommand.Connection = objConnection;
		objCommand.CommandText = "SELECT * FROM tblOmaeUser WHERE strUserName = '" + SQLSafe(strUserName) + "'";

		SqlDataReader objReader = objCommand.ExecuteReader();
		if (objReader.Read())
		{
			// Someone with that username already exists.
			objReader.Close();
			objConnection.Close();
			return -1;
		}
		else
		{
			// That username does not yet exist, so create it.
			objReader.Close();
			objCommand.CommandText = "INSERT INTO tblOmaeUser (strUserName, strPassword) VALUES('" + SQLSafe(strUserName) + "', '" + SQLSafe(strPassword) + "')";
			objCommand.ExecuteNonQuery();
			objConnection.Close();
			return 0;
		}
	}

	[WebMethod(Description = "Login to an existing Omae account.")]
	public bool Login(string strUserName, string strPassword)
	{
		SqlConnection objConnection = new SqlConnection(CONNECTION_STRING);
		objConnection.Open();
		
		// Attempt to login using the information provided.
		SqlCommand objCommand = new SqlCommand();
		objCommand.Connection = objConnection;
		objCommand.CommandText = "SELECT * FROM tblOmaeUser WHERE strUserName = '" + SQLSafe(strUserName) + "' AND strPassword = '" + SQLSafe(strPassword) + "'";

		SqlDataReader objReader = objCommand.ExecuteReader();
		if (objReader.Read())
		{
			// Username and password match, so return true;
			objReader.Close();
			objConnection.Close();
			return true;
		}
		else
		{
			// Username and password don't match, so return false;
			objReader.Close();
			objConnection.Close();
			return false;
		}
	}

	[WebMethod(Description = "Set the email address for the user's account.")]
	public bool SetEmailAddress(string strUserName, string strEmail)
	{
		SqlConnection objConnection = new SqlConnection(CONNECTION_STRING);
		objConnection.Open();

		// Look up the user's information.
		SqlCommand objCommand = new SqlCommand();
		objCommand.Connection = objConnection;
		objCommand.CommandText = "SELECT * FROM tblOmaeUser WHERE strUserName = '" + SQLSafe(strUserName) + "'";

		SqlDataReader objReader = objCommand.ExecuteReader();
		if (objReader.Read())
		{
			objReader.Close();

			// Update the user's email address.
			objCommand.CommandText = "UPDATE tblOmaeUser SET strEmail = '" + SQLSafe(strEmail) + "' WHERE strUserName = '" + SQLSafe(strUserName) + "'";
			objCommand.ExecuteNonQuery();

			objConnection.Close();
			return true;
		}
		else
		{
			// The user account doesn't exist, so don't do anything.
			objReader.Close();
			objConnection.Close();
			return false;
		}
	}

	[WebMethod(Description = "Get the email address for the user's account.")]
	public string GetEmailAddress(string strUserName)
	{
		SqlConnection objConnection = new SqlConnection(CONNECTION_STRING);
		objConnection.Open();

		// Look up the user's information.
		SqlCommand objCommand = new SqlCommand();
		objCommand.Connection = objConnection;
		objCommand.CommandText = "SELECT * FROM tblOmaeUser WHERE strUserName = '" + SQLSafe(strUserName) + "'";

		SqlDataReader objReader = objCommand.ExecuteReader();
		if (objReader.Read())
		{
			string strEmail = "";
			if (objReader["strEmail"] == DBNull.Value)
				strEmail = "";
			else
			{
				strEmail = objReader["strEmail"].ToString();
			}
			objReader.Close();
			objConnection.Close();
			return strEmail;
		}
		else
		{
			// The user account doesn't exist, so don't do anything.
			objReader.Close();
			objConnection.Close();
			return "";
		}
	}

	[WebMethod(Description = "Reset the user's password.")]
	public bool ResetPassword(string strUserName)
	{
		string strNewPassword = "";
		string strEmail = "";

		SqlConnection objConnection = new SqlConnection(CONNECTION_STRING);
		objConnection.Open();

		// Look up the user's information.
		SqlCommand objCommand = new SqlCommand();
		objCommand.Connection = objConnection;
		objCommand.CommandText = "SELECT * FROM tblOmaeUser WHERE strUserName = '" + SQLSafe(strUserName) + "'";

		SqlDataReader objReader = objCommand.ExecuteReader();
		if (objReader.Read())
		{
			// Make sure the user has provided an email address.
			if (objReader["strEmail"] == DBNull.Value)
			{
				objReader.Close();
				objConnection.Close();
				return false;
			}
			if (!objReader["strEmail"].ToString().Contains("@"))
			{
				objReader.Close();
				objConnection.Close();
				return false;
			}

			strEmail = objReader["strEmail"].ToString();
			Random objRandom = new Random();
			for (int i = 0; i <= 8; i++)
			{
				strNewPassword += objRandom.Next(1, 11).ToString();
			}

			objReader.Close();

			// Update the user's password.
			objCommand.CommandText = "UPDATE tblOmaeUser SET strPassword = '" + SQLSafe(Base64Encode(strNewPassword)) + "' WHERE strUserName = '" + SQLSafe(strUserName) + "'";
			objCommand.ExecuteNonQuery();


			objConnection.Close();

			MailMessage objMessage = new MailMessage();
			objMessage.To = strEmail;
			objMessage.From = "omae@dndjunkie.com";
			objMessage.Subject = "Omae Password Reset";
			objMessage.Body = "Your Omae password has been reset to " + strNewPassword;

			SmtpMail.SmtpServer = "smtp.dndjunkie.com";
			SmtpMail.Send(objMessage);
			return true;
		}
		else
		{
			// No information exists, so don't attempt to do anything.
			objReader.Close();
			objConnection.Close();
			return false;
		}
	}

	// Character Methods.
	[WebMethod(Description = "Get the list of Character Types.")]
	public XmlDocument GetCharacterTypes()
	{
		SqlConnection objConnection = new SqlConnection(CONNECTION_STRING);
		objConnection.Open();

		SqlCommand objCommand = new SqlCommand();
		objCommand.Connection = objConnection;
		objCommand.CommandText = "SELECT * FROM tblOmaeCharacterType";

		MemoryStream objStream = new MemoryStream();
		XmlTextWriter objWriter = new XmlTextWriter(objStream, System.Text.Encoding.UTF8);

		objWriter.WriteStartDocument();
		// <characters>
		objWriter.WriteStartElement("types");

		SqlDataReader objReader = objCommand.ExecuteReader();
		while (objReader.Read())
		{
			// <type>
			objWriter.WriteStartElement("type");
			objWriter.WriteElementString("id", objReader["intTypeID"].ToString());
			objWriter.WriteElementString("name", objReader["strTypeName"].ToString());
			// </type>
			objWriter.WriteEndElement();
		}

		// </character>
		objWriter.WriteEndElement();
		objWriter.WriteEndDocument();

		// Flush the output.
		objWriter.Flush();
		objStream.Flush();

		// Load the XmlDocument from the stream.
		XmlDocument objXmlDocument = XmlDocumentFromStream(objStream);

		// Close everything now that we're done.
		objWriter.Close();
		objStream.Close();

		return objXmlDocument;
	}

	[WebMethod(Description = "Upload a Character file.")]
	public bool UploadCharacter(string strUserName, int intCharacterID, string strCharacterName, string strDescription, string strMetatype, string strMetavariant, string strQualities, int intTypeID, byte[] bytFile)
	{
		SqlConnection objConnection = new SqlConnection(CONNECTION_STRING);
		objConnection.Open();
		
		SqlCommand objCommand = new SqlCommand();
		objCommand.Connection = objConnection;
		
		if (intCharacterID == 0)
		{
			// This is a new character, so create a record for it.
			objCommand.CommandText = "INSERT INTO tblOmaeCharacter(strCharacterName, strUserName, strDescription, intTypeID, strMetatype, strMetavariant, strQualities) VALUES(N'" + SQLSafe(strCharacterName) + "', '" + SQLSafe(strUserName) + "', '" + SQLSafe(strDescription) + "', " + intTypeID.ToString() + ", '" + SQLSafe(strMetatype) + "', '" + SQLSafe(strMetavariant) + "', '" + SQLSafe(strQualities) + "'); SELECT @@IDENTITY AS NewID";

			SqlDataReader objReader = objCommand.ExecuteReader();
			objReader.Read();
			intCharacterID = Convert.ToInt32(objReader["NewID"]);
			objReader.Close();
		}
		else
		{
			// This is an existing character, so update the information.
			objCommand.CommandText = "UPDATE tblOmaeCharacter SET strCharacterName = '" + SQLSafe(strCharacterName) + "', intTypeID = " + intTypeID.ToString() + ", datUpdated = GETDATE(), strDescription = '" + SQLSafe(strDescription) + "', strMetatype = '" + SQLSafe(strMetatype) + "', strMetavariant = '" + SQLSafe(strMetavariant) + "', strQualities = '" + SQLSafe(strQualities) + "' WHERE intCharacterID = " + intCharacterID.ToString();
			objCommand.ExecuteNonQuery();
		}
		objConnection.Close();
		
		// If the file already exists on the server, delete it.
		string strFileName = Path.Combine(Path.Combine(Server.MapPath("."), "data"), FileSafe(strCharacterName) + "_" + intCharacterID.ToString() + ".gz");
		if (File.Exists(strFileName))
		{
			try
			{
				File.Delete(strFileName);
			}
			catch
			{
			}
		}
		// Save the new file.
		File.WriteAllBytes(strFileName, bytFile);
		return true;
	}

	[WebMethod(Description = "Upload a Character file.")]
	public bool UploadCharacter153(string strUserName, int intCharacterID, string strCharacterName, string strDescription, string strMetatype, string strMetavariant, string strQualities, int intTypeID, int intCreated, byte[] bytFile)
	{
		SqlConnection objConnection = new SqlConnection(CONNECTION_STRING);
		objConnection.Open();

		SqlCommand objCommand = new SqlCommand();
		objCommand.Connection = objConnection;

		if (intCharacterID == 0)
		{
			// This is a new character, so create a record for it.
			objCommand.CommandText = "INSERT INTO tblOmaeCharacter(strCharacterName, strUserName, strDescription, intTypeID, strMetatype, strMetavariant, strQualities, blnCreated) VALUES(N'" + SQLSafe(strCharacterName) + "', '" + SQLSafe(strUserName) + "', '" + SQLSafe(strDescription) + "', " + intTypeID.ToString() + ", '" + SQLSafe(strMetatype) + "', '" + SQLSafe(strMetavariant) + "', '" + SQLSafe(strQualities) + "', " + intCreated.ToString() + "); SELECT @@IDENTITY AS NewID";

			SqlDataReader objReader = objCommand.ExecuteReader();
			objReader.Read();
			intCharacterID = Convert.ToInt32(objReader["NewID"]);
			objReader.Close();
		}
		else
		{
			// This is an existing character, so update the information.
			objCommand.CommandText = "UPDATE tblOmaeCharacter SET strCharacterName = '" + SQLSafe(strCharacterName) + "', intTypeID = " + intTypeID.ToString() + ", datUpdated = GETDATE(), strDescription = '" + SQLSafe(strDescription) + "', strMetatype = '" + SQLSafe(strMetatype) + "', strMetavariant = '" + SQLSafe(strMetavariant) + "', strQualities = '" + SQLSafe(strQualities) + "', blnCreated = " + intCreated.ToString() + " WHERE intCharacterID = " + intCharacterID.ToString();
			objCommand.ExecuteNonQuery();
		}
		objConnection.Close();

		// If the file already exists on the server, delete it.
		string strFileName = Path.Combine(Path.Combine(Server.MapPath("."), "data"), FileSafe(strCharacterName) + "_" + intCharacterID.ToString() + ".gz");
		if (File.Exists(strFileName))
		{
			try
			{
				File.Delete(strFileName);
			}
			catch
			{
			}
		}
		// Save the new file.
		File.WriteAllBytes(strFileName, bytFile);
		return true;
	}

	[WebMethod(Description = "Download a Character file.")]
	public byte[] DownloadCharacter(int intCharacterID)
	{
		SqlConnection objConnection = new SqlConnection(CONNECTION_STRING);
		objConnection.Open();
		
		SqlCommand objCommand = new SqlCommand();
		objCommand.Connection = objConnection;
		objCommand.CommandText = "SELECT * FROM tblOmaeCharacter WHERE intCharacterID = " + intCharacterID.ToString();

		SqlDataReader objReader = objCommand.ExecuteReader();
		if (objReader.Read())
		{
			if (objReader["intTypeID"].ToString() == "4")
			{
				byte[] bytFile = File.ReadAllBytes(Path.Combine(Path.Combine(Server.MapPath("."), "data"), "npcpack_" + objReader["intCharacterID"].ToString() + ".zip"));
				objReader.Close();

				// Update the download count for the character.
				objCommand.CommandText = "UPDATE tblOmaeCharacter SET intDownloadCount = intDownloadCount + 1 WHERE intCharacterID = " + intCharacterID.ToString();
				objCommand.ExecuteNonQuery();
				
				objConnection.Close();
				
				return bytFile;
			}
			else
			{
				if (File.Exists(Path.Combine(Path.Combine(Server.MapPath("."), "data"), FileSafe(objReader["strCharacterName"].ToString()) + "_" + objReader["intCharacterID"].ToString() + ".gz")))
				{
					byte[] bytFile = File.ReadAllBytes(Path.Combine(Path.Combine(Server.MapPath("."), "data"), FileSafe(objReader["strCharacterName"].ToString()) + "_" + objReader["intCharacterID"].ToString() + ".gz"));
					objReader.Close();

					// Update the download count for the character.
					objCommand.CommandText = "UPDATE tblOmaeCharacter SET intDownloadCount = intDownloadCount + 1 WHERE intCharacterID = " + intCharacterID.ToString();
					objCommand.ExecuteNonQuery();
					
					objConnection.Close();
					
					return bytFile;
				}
				else
				{
					objReader.Close();
					objConnection.Close();
					byte[] bytFile = new byte[0];
					return bytFile;
				}
			}
		}
		else
		{
			objReader.Close();
			objConnection.Close();
			byte[] bytFile = new byte[0];
			return bytFile;
		}
	}

	[WebMethod(Description = "Delete a Character.")]
	public bool DeleteCharacter(int intCharacterID)
	{
		SqlConnection objConnection = new SqlConnection(CONNECTION_STRING);
		objConnection.Open();

		SqlCommand objCommand = new SqlCommand();
		objCommand.Connection = objConnection;
		objCommand.CommandText = "SELECT * FROM tblOmaeCharacter WHERE intCharacterID = " + intCharacterID.ToString();
		
		SqlDataReader objReader = objCommand.ExecuteReader();
		if (objReader.Read())
		{
			try
			{
				// Delete the file.
				File.Delete(Path.Combine(Path.Combine(Server.MapPath("."), "data"), FileSafe(objReader["strCharacterName"].ToString()) + "_" + objReader["intCharacterID"].ToString() + ".gz"));
			}
			catch
			{
			}
			objReader.Close();

			// Delete the Tags for the Character.
			objCommand.CommandText = "DELETE FROM tblOmaeCharacterTag WHERE intCharacterID = " + intCharacterID.ToString();
			objCommand.ExecuteNonQuery();

			// Delete the Character.
			objCommand.CommandText = "DELETE FROM tblOmaeCharacter WHERE intCharacterID = " + intCharacterID.ToString();
			objCommand.ExecuteNonQuery();

			objConnection.Close();
			return true;
		}
		else
		{
			objReader.Close();
			objConnection.Close();
			return false;
		}
	}
	
	[WebMethod(Description = "List all of the characters for a given category.")]
	public XmlDocument FetchCharacters(int intTypeID, int intSortOrder, string strMetatype, string strMetavariant, string strUser, string strQuality1, string strQuality2, string strQuality3)
	{
		SqlConnection objConnection = new SqlConnection(CONNECTION_STRING);
		objConnection.Open();
		
		SqlCommand objCommand = new SqlCommand();
		objCommand.Connection = objConnection;
		if (intTypeID == 0)
			objCommand.CommandText = "SELECT * FROM tblOmaeCharacter WHERE 1 = 1";
		else
			objCommand.CommandText = "SELECT * FROM tblOmaeCharacter WHERE intTypeID = " + intTypeID.ToString();

		// Determie the additional parameters to include.	
		string strWhere = "";
		if (strMetatype != "")
			strWhere += " AND strMetatype = '" + SQLSafe(strMetatype) + "'";
		if (strMetavariant != "")
			strWhere += " AND strMetavariant = '" + SQLSafe(strMetavariant) + "'";
		if (strUser != "")
			strWhere += " AND strUserName LIKE '%" + SQLSafe(strUser) + "%'";
		if (strQuality1 != "" || strQuality2 != "" || strQuality3 != "")
		{
			strWhere += " AND (";
			string strQualityFilter = "";
			if (strQuality1 != "")
				strQualityFilter += "strQualities LIKE '%" + SQLSafe(strQuality1) + "%' OR ";
			if (strQuality2 != "")
				strQualityFilter += "strQualities LIKE '%" + SQLSafe(strQuality2) + "%' OR ";
			if (strQuality3 != "")
				strQualityFilter += "strQualities LIKE '%" + SQLSafe(strQuality3) + "%' OR";
			
			// Remove the trailing "OR".
			strQualityFilter = strQualityFilter.Substring(0, strQualityFilter.Length - 3);
			strWhere += strQualityFilter + ")";
		}

		if (strWhere != "")
			objCommand.CommandText += strWhere;

		switch (intSortOrder)
		{
			case 0:
				objCommand.CommandText += " ORDER BY strCharacterName";
				break;
			case 1:
				objCommand.CommandText += " ORDER BY datUpdated DESC";
				break;
			case 2:
				objCommand.CommandText += " ORDER BY intDownloadCount DESC, strCharacterName ASC";
				break;
		}
		
		MemoryStream objStream = new MemoryStream();
		XmlTextWriter objWriter = new XmlTextWriter(objStream, System.Text.Encoding.UTF8);
		
		objWriter.WriteStartDocument();
		// <characters>
		objWriter.WriteStartElement("characters");
		
		SqlDataReader objReader = objCommand.ExecuteReader();
		while (objReader.Read())
		{
			// <character>
			objWriter.WriteStartElement("character");
			objWriter.WriteElementString("id", objReader["intCharacterID"].ToString());
			objWriter.WriteElementString("name", objReader["strCharacterName"].ToString());
			objWriter.WriteElementString("metatype", objReader["strMetatype"].ToString());
			objWriter.WriteElementString("metavariant", objReader["strMetavariant"].ToString());
			objWriter.WriteElementString("user", objReader["strUserName"].ToString());
			objWriter.WriteElementString("date", objReader["datUpdated"].ToString());
			objWriter.WriteElementString("created", objReader["blnCreated"].ToString());
			objWriter.WriteElementString("count", objReader["intDownloadCount"].ToString());
			if (objReader["strDescription"] == DBNull.Value)
				objWriter.WriteElementString("description", "");
			else
				objWriter.WriteElementString("description", objReader["strDescription"].ToString());
			objWriter.WriteElementString("dl", objReader["intDownloadCount"].ToString());
			// </character>
			objWriter.WriteEndElement();
		}
		
		// </character>
		objWriter.WriteEndElement();
		objWriter.WriteEndDocument();
		
		// Flush the output.
		objWriter.Flush();
		objStream.Flush();
		
		// Load the XmlDocument from the stream.
		XmlDocument objXmlDocument = XmlDocumentFromStream(objStream);
		
		// Close everything now that we're done.
		objWriter.Close();
		objStream.Close();
		
		return objXmlDocument;
	}

	[WebMethod(Description = "List all of the characters for a given category.")]
	public XmlDocument FetchCharacters153(int intTypeID, int intSortOrder, string strMetatype, string strMetavariant, int intCreated, string strUser, string strQuality1, string strQuality2, string strQuality3)
	{
		SqlConnection objConnection = new SqlConnection(CONNECTION_STRING);
		objConnection.Open();

		SqlCommand objCommand = new SqlCommand();
		objCommand.Connection = objConnection;
		if (intTypeID == 0)
			objCommand.CommandText = "SELECT * FROM tblOmaeCharacter WHERE 1 = 1";
		else
			objCommand.CommandText = "SELECT * FROM tblOmaeCharacter WHERE intTypeID = " + intTypeID.ToString();

		// Determie the additional parameters to include.	
		string strWhere = "";
		if (strMetatype != "")
			strWhere += " AND strMetatype = '" + SQLSafe(strMetatype) + "'";
		if (strMetavariant != "")
			strWhere += " AND strMetavariant = '" + SQLSafe(strMetavariant) + "'";
		if (strUser != "")
			strWhere += " AND strUserName LIKE '%" + SQLSafe(strUser) + "%'";
		switch (intCreated)
		{
			case 0:
				strWhere += " AND blnCreated = 0";
				break;
			case 1:
				strWhere += " AND blnCreated = 1";
				break;
			default:
				break;
		}
		if (strQuality1 != "" || strQuality2 != "" || strQuality3 != "")
		{
			strWhere += " AND (";
			string strQualityFilter = "";
			if (strQuality1 != "")
				strQualityFilter += "strQualities LIKE '%" + SQLSafe(strQuality1) + "%' OR ";
			if (strQuality2 != "")
				strQualityFilter += "strQualities LIKE '%" + SQLSafe(strQuality2) + "%' OR ";
			if (strQuality3 != "")
				strQualityFilter += "strQualities LIKE '%" + SQLSafe(strQuality3) + "%' OR";

			// Remove the trailing "OR".
			strQualityFilter = strQualityFilter.Substring(0, strQualityFilter.Length - 3);
			strWhere += strQualityFilter + ")";
		}

		if (strWhere != "")
			objCommand.CommandText += strWhere;

		switch (intSortOrder)
		{
			case 0:
				objCommand.CommandText += " ORDER BY strCharacterName";
				break;
			case 1:
				objCommand.CommandText += " ORDER BY datUpdated DESC";
				break;
			case 2:
				objCommand.CommandText += " ORDER BY intDownloadCount DESC, strCharacterName ASC";
				break;
		}

		MemoryStream objStream = new MemoryStream();
		XmlTextWriter objWriter = new XmlTextWriter(objStream, System.Text.Encoding.UTF8);

		objWriter.WriteStartDocument();
		// <characters>
		objWriter.WriteStartElement("characters");

		SqlDataReader objReader = objCommand.ExecuteReader();
		while (objReader.Read())
		{
			// <character>
			objWriter.WriteStartElement("character");
			objWriter.WriteElementString("id", objReader["intCharacterID"].ToString());
			objWriter.WriteElementString("name", objReader["strCharacterName"].ToString());
			objWriter.WriteElementString("metatype", objReader["strMetatype"].ToString());
			objWriter.WriteElementString("metavariant", objReader["strMetavariant"].ToString());
			objWriter.WriteElementString("user", objReader["strUserName"].ToString());
			objWriter.WriteElementString("date", objReader["datUpdated"].ToString());
			objWriter.WriteElementString("created", objReader["blnCreated"].ToString());
			objWriter.WriteElementString("count", objReader["intDownloadCount"].ToString());
			if (objReader["strDescription"] == DBNull.Value)
				objWriter.WriteElementString("description", "");
			else
				objWriter.WriteElementString("description", objReader["strDescription"].ToString());
			objWriter.WriteElementString("dl", objReader["intDownloadCount"].ToString());
			// </character>
			objWriter.WriteEndElement();
		}

		// </character>
		objWriter.WriteEndElement();
		objWriter.WriteEndDocument();

		// Flush the output.
		objWriter.Flush();
		objStream.Flush();

		// Load the XmlDocument from the stream.
		XmlDocument objXmlDocument = XmlDocumentFromStream(objStream);

		// Close everything now that we're done.
		objWriter.Close();
		objStream.Close();

		return objXmlDocument;
	}
	
	// Data File Methods.
	[WebMethod(Description = "Upload a data file.")]
	public bool UploadDataFile(string strUserName, int intDataID, string strDataName, string strDescription, string strFilesIncluded, byte[] bytFile)
	{
		SqlConnection objConnection = new SqlConnection(CONNECTION_STRING);
		objConnection.Open();

		SqlCommand objCommand = new SqlCommand();
		objCommand.Connection = objConnection;

		if (intDataID == 0)
		{
			// This is a new file, so create a record for it.
			objCommand.CommandText = "INSERT INTO tblOmaeData(strDataName, strUserName, strDescription, strFilesIncluded) VALUES(N'" + SQLSafe(strDataName) + "', '" + SQLSafe(strUserName) + "', '" + SQLSafe(strDescription) + "', '" + SQLSafe(strFilesIncluded) + "'); SELECT @@IDENTITY AS NewID";

			SqlDataReader objReader = objCommand.ExecuteReader();
			objReader.Read();
			intDataID = Convert.ToInt32(objReader["NewID"]);
			objReader.Close();
		}
		else
		{
			// This is an existing data file, so update the information.
			objCommand.CommandText = "UPDATE tblOmaeData SET strDataName = '" + SQLSafe(strDataName) + "', datUpdated = GETDATE(), strDescription = '" + SQLSafe(strDescription) + "', strFilesIncluded = '" + SQLSafe(strFilesIncluded) + "' WHERE intDataID = " + intDataID.ToString();
			objCommand.ExecuteNonQuery();
		}
		objConnection.Close();

		// If the file already exists on the server, delete it.
		string strFileName = Path.Combine(Path.Combine(Server.MapPath("."), "data"), "data_" + FileSafe(strDataName) + "_" + intDataID.ToString() + ".gz");
		if (File.Exists(strFileName))
		{
			try
			{
				File.Delete(strFileName);
			}
			catch
			{
			}
		}
		// Save the new file.
		File.WriteAllBytes(strFileName, bytFile);
		return true;
	}

	[WebMethod(Description = "Download a data file.")]
	public byte[] DownloadDataFile(int intDataID)
	{
		SqlConnection objConnection = new SqlConnection(CONNECTION_STRING);
		objConnection.Open();

		SqlCommand objCommand = new SqlCommand();
		objCommand.Connection = objConnection;
		objCommand.CommandText = "SELECT * FROM tblOmaeData WHERE intDataID = " + intDataID.ToString();

		SqlDataReader objReader = objCommand.ExecuteReader();
		if (objReader.Read())
		{
			if (File.Exists(Path.Combine(Path.Combine(Server.MapPath("."), "data"), "data_" + FileSafe(objReader["strDataName"].ToString()) + "_" + objReader["intDataID"].ToString() + ".gz")))
			{
				byte[] bytFile = File.ReadAllBytes(Path.Combine(Path.Combine(Server.MapPath("."), "data"), "data_" + FileSafe(objReader["strDataName"].ToString()) + "_" + objReader["intDataID"].ToString() + ".gz"));
				objReader.Close();

				// Update the download count for the data file.
				objCommand.CommandText = "UPDATE tblOmaeData SET intDownloadCount = intDownloadCount + 1 WHERE intDataID = " + intDataID.ToString();
				objCommand.ExecuteNonQuery();

				objConnection.Close();

				return bytFile;
			}
			else
			{
				objReader.Close();
				objConnection.Close();
				byte[] bytFile = new byte[0];
				return bytFile;
			}
		}
		else
		{
			objReader.Close();
			objConnection.Close();
			byte[] bytFile = new byte[0];
			return bytFile;
		}
	}

	[WebMethod(Description = "Delete a data file.")]
	public bool DeleteDataFile(int intDataID)
	{
		SqlConnection objConnection = new SqlConnection(CONNECTION_STRING);
		objConnection.Open();

		SqlCommand objCommand = new SqlCommand();
		objCommand.Connection = objConnection;
		objCommand.CommandText = "SELECT * FROM tblOmaeData WHERE intDataID = " + intDataID.ToString();

		SqlDataReader objReader = objCommand.ExecuteReader();
		if (objReader.Read())
		{
			try
			{
				// Delete the file.
				File.Delete(Path.Combine(Path.Combine(Server.MapPath("."), "data"), "data_" + FileSafe(objReader["strDataName"].ToString()) + "_" + objReader["intDataID"].ToString() + ".gz"));
			}
			catch
			{
			}
			objReader.Close();

			// Delete the Character.
			objCommand.CommandText = "DELETE FROM tblOmaeData WHERE intDataID = " + intDataID.ToString();
			objCommand.ExecuteNonQuery();

			objConnection.Close();
			return true;
		}
		else
		{
			objReader.Close();
			objConnection.Close();
			return false;
		}
	}

	[WebMethod(Description = "List all of the data files.")]
	public XmlDocument FetchDataFiles(int intSortOrder, string strFilesIncluded, string strUser)
	{
		SqlConnection objConnection = new SqlConnection(CONNECTION_STRING);
		objConnection.Open();

		SqlCommand objCommand = new SqlCommand();
		objCommand.Connection = objConnection;
		objCommand.CommandText = "SELECT * FROM tblOmaeData WHERE 1 = 1";

		// Determie the additional parameters to include.	
		string strWhere = "";
		if (strFilesIncluded != "")
			strWhere += " AND strFilesInluded LIKE '%" + SQLSafe(strFilesIncluded) + "%'";
		if (strUser != "")
			strWhere += " AND strUserName LIKE '%" + SQLSafe(strUser) + "%'";

		if (strWhere != "")
			objCommand.CommandText += strWhere;

		switch (intSortOrder)
		{
			case 0:
				objCommand.CommandText += " ORDER BY strDataName";
				break;
			case 1:
				objCommand.CommandText += " ORDER BY datUpdated DESC";
				break;
			case 2:
				objCommand.CommandText += " ORDER BY intDownloadCount DESC, strDataName ASC";
				break;
		}

		MemoryStream objStream = new MemoryStream();
		XmlTextWriter objWriter = new XmlTextWriter(objStream, System.Text.Encoding.UTF8);

		objWriter.WriteStartDocument();
		// <datas>
		objWriter.WriteStartElement("datas");

		SqlDataReader objReader = objCommand.ExecuteReader();
		while (objReader.Read())
		{
			// <data>
			objWriter.WriteStartElement("data");
			objWriter.WriteElementString("id", objReader["intDataID"].ToString());
			objWriter.WriteElementString("name", objReader["strDataName"].ToString());
			objWriter.WriteElementString("filesincluded", objReader["strFilesIncluded"].ToString());
			objWriter.WriteElementString("user", objReader["strUserName"].ToString());
			objWriter.WriteElementString("date", objReader["datUpdated"].ToString());
			objWriter.WriteElementString("count", objReader["intDownloadCount"].ToString());
			if (objReader["strDescription"] == DBNull.Value)
				objWriter.WriteElementString("description", "");
			else
				objWriter.WriteElementString("description", objReader["strDescription"].ToString());
			objWriter.WriteElementString("dl", objReader["intDownloadCount"].ToString());
			// </data>
			objWriter.WriteEndElement();
		}

		// </datas>
		objWriter.WriteEndElement();
		objWriter.WriteEndDocument();

		// Flush the output.
		objWriter.Flush();
		objStream.Flush();

		// Load the XmlDocument from the stream.
		XmlDocument objXmlDocument = XmlDocumentFromStream(objStream);

		// Close everything now that we're done.
		objWriter.Close();
		objStream.Close();

		return objXmlDocument;
	}

	// Character Sheet Methods.
	[WebMethod(Description = "Upload a character sheet.")]
	public bool UploadSheet(string strUserName, int intSheetID, string strSheetName, string strDescription, byte[] bytFile)
	{
		SqlConnection objConnection = new SqlConnection(CONNECTION_STRING);
		objConnection.Open();

		SqlCommand objCommand = new SqlCommand();
		objCommand.Connection = objConnection;

		if (intSheetID == 0)
		{
			// This is a new file, so create a record for it.
			objCommand.CommandText = "INSERT INTO tblOmaeSheets(strSheetName, strUserName, strDescription) VALUES(N'" + SQLSafe(strSheetName) + "', '" + SQLSafe(strUserName) + "', '" + SQLSafe(strDescription) + "'); SELECT @@IDENTITY AS NewID";

			SqlDataReader objReader = objCommand.ExecuteReader();
			objReader.Read();
			intSheetID = Convert.ToInt32(objReader["NewID"]);
			objReader.Close();
		}
		else
		{
			// This is an existing character sheet, so update the information.
			objCommand.CommandText = "UPDATE tblOmaeSheets SET strSheetName = '" + SQLSafe(strSheetName) + "', datUpdated = GETDATE(), strDescription = '" + SQLSafe(strDescription) + "' WHERE intSheetID = " + intSheetID.ToString();
			objCommand.ExecuteNonQuery();
		}
		objConnection.Close();

		// If the file already exists on the server, delete it.
		string strFileName = Path.Combine(Path.Combine(Server.MapPath("."), "data"), "sheet_" + FileSafe(strSheetName) + "_" + intSheetID.ToString() + ".gz");
		if (File.Exists(strFileName))
		{
			try
			{
				File.Delete(strFileName);
			}
			catch
			{
			}
		}
		// Save the new file.
		File.WriteAllBytes(strFileName, bytFile);
		return true;
	}

	[WebMethod(Description = "Download a character sheet.")]
	public byte[] DownloadSheet(int intSheetID)
	{
		SqlConnection objConnection = new SqlConnection(CONNECTION_STRING);
		objConnection.Open();

		SqlCommand objCommand = new SqlCommand();
		objCommand.Connection = objConnection;
		objCommand.CommandText = "SELECT * FROM tblOmaeSheets WHERE intSheetID = " + intSheetID.ToString();

		SqlDataReader objReader = objCommand.ExecuteReader();
		if (objReader.Read())
		{
			if (File.Exists(Path.Combine(Path.Combine(Server.MapPath("."), "data"), "sheet_" + FileSafe(objReader["strSheetName"].ToString()) + "_" + objReader["intSheetID"].ToString() + ".gz")))
			{
				byte[] bytFile = File.ReadAllBytes(Path.Combine(Path.Combine(Server.MapPath("."), "data"), "sheet_" + FileSafe(objReader["strSheetName"].ToString()) + "_" + objReader["intSheetID"].ToString() + ".gz"));
				objReader.Close();

				// Update the download count for the character sheet.
				objCommand.CommandText = "UPDATE tblOmaeSheets SET intDownloadCount = intDownloadCount + 1 WHERE intSheetID = " + intSheetID.ToString();
				objCommand.ExecuteNonQuery();

				objConnection.Close();

				return bytFile;
			}
			else
			{
				objReader.Close();
				objConnection.Close();
				byte[] bytFile = new byte[0];
				return bytFile;
			}
		}
		else
		{
			objReader.Close();
			objConnection.Close();
			byte[] bytFile = new byte[0];
			return bytFile;
		}
	}

	[WebMethod(Description = "Delete a character sheet.")]
	public bool DeleteSheet(int intSheetID)
	{
		SqlConnection objConnection = new SqlConnection(CONNECTION_STRING);
		objConnection.Open();

		SqlCommand objCommand = new SqlCommand();
		objCommand.Connection = objConnection;
		objCommand.CommandText = "SELECT * FROM tblOmaeSheets WHERE intSheetID = " + intSheetID.ToString();

		SqlDataReader objReader = objCommand.ExecuteReader();
		if (objReader.Read())
		{
			try
			{
				// Delete the file.
				File.Delete(Path.Combine(Path.Combine(Server.MapPath("."), "data"), "sheet_" + FileSafe(objReader["strSheetName"].ToString()) + "_" + objReader["intSheetID"].ToString() + ".gz"));
			}
			catch
			{
			}
			objReader.Close();

			// Delete the character sheet.
			objCommand.CommandText = "DELETE FROM tblOmaeSheets WHERE intSheetID = " + intSheetID.ToString();
			objCommand.ExecuteNonQuery();

			objConnection.Close();
			return true;
		}
		else
		{
			objReader.Close();
			objConnection.Close();
			return false;
		}
	}

	[WebMethod(Description = "List all of the character sheets.")]
	public XmlDocument FetchSheets(int intSortOrder, string strUser)
	{
		SqlConnection objConnection = new SqlConnection(CONNECTION_STRING);
		objConnection.Open();

		SqlCommand objCommand = new SqlCommand();
		objCommand.Connection = objConnection;
		objCommand.CommandText = "SELECT * FROM tblOmaeSheets WHERE 1 = 1";

		// Determie the additional parameters to include.	
		string strWhere = "";
		if (strUser != "")
			strWhere += " AND strUserName LIKE '%" + SQLSafe(strUser) + "%'";

		if (strWhere != "")
			objCommand.CommandText += strWhere;

		switch (intSortOrder)
		{
			case 0:
				objCommand.CommandText += " ORDER BY strSheetName";
				break;
			case 1:
				objCommand.CommandText += " ORDER BY datUpdated DESC";
				break;
			case 2:
				objCommand.CommandText += " ORDER BY intDownloadCount DESC, strSheetName ASC";
				break;
		}

		MemoryStream objStream = new MemoryStream();
		XmlTextWriter objWriter = new XmlTextWriter(objStream, System.Text.Encoding.UTF8);

		objWriter.WriteStartDocument();
		// <datas>
		objWriter.WriteStartElement("sheets");

		SqlDataReader objReader = objCommand.ExecuteReader();
		while (objReader.Read())
		{
			// <data>
			objWriter.WriteStartElement("sheet");
			objWriter.WriteElementString("id", objReader["intSheetID"].ToString());
			objWriter.WriteElementString("name", objReader["strSheetName"].ToString());
			objWriter.WriteElementString("user", objReader["strUserName"].ToString());
			objWriter.WriteElementString("date", objReader["datUpdated"].ToString());
			objWriter.WriteElementString("count", objReader["intDownloadCount"].ToString());
			if (objReader["strDescription"] == DBNull.Value)
				objWriter.WriteElementString("description", "");
			else
				objWriter.WriteElementString("description", objReader["strDescription"].ToString());
			objWriter.WriteElementString("dl", objReader["intDownloadCount"].ToString());
			// </data>
			objWriter.WriteEndElement();
		}

		// </datas>
		objWriter.WriteEndElement();
		objWriter.WriteEndDocument();

		// Flush the output.
		objWriter.Flush();
		objStream.Flush();

		// Load the XmlDocument from the stream.
		XmlDocument objXmlDocument = XmlDocumentFromStream(objStream);

		// Close everything now that we're done.
		objWriter.Close();
		objStream.Close();

		return objXmlDocument;
	}

	// Compression and Encoding.
	private string Base64Encode(string data)
	{
		try
		{
			byte[] encData_byte = new byte[data.Length];
			encData_byte = System.Text.Encoding.UTF8.GetBytes(data);
			string encodedData = Convert.ToBase64String(encData_byte);
			return encodedData;
		}
		catch (Exception e)
		{
			throw new Exception("Error in Base64Encode" + e.Message);
		}
	}

	private string Base64Decode(string data)
	{
		try
		{
			System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
			System.Text.Decoder utf8Decode = encoder.GetDecoder();

			byte[] todecode_byte = Convert.FromBase64String(data);
			int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
			char[] decoded_char = new char[charCount];
			utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
			string result = new String(decoded_char);
			return result;
		}
		catch (Exception e)
		{
			throw new Exception("Error in Base64Decode" + e.Message);
		}
	}

	/// <summary>
	/// Compresses byte array to new byte array.
	/// </summary>
	public byte[] Compress(byte[] raw)
	{
		using (MemoryStream memory = new MemoryStream())
		{
			using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
			{
				gzip.Write(raw, 0, raw.Length);
			}
			return memory.ToArray();
		}
	}

	/// <summary>
	/// Decompress byte array to a new byte array.
	/// </summary>
	public byte[] Decompress(byte[] gzip)
	{
		// Create a GZIP stream with decompression mode.
		// ... Then create a buffer and write into while reading from the GZIP stream.
		using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
		{
			const int size = 4096;
			byte[] buffer = new byte[size];
			using (MemoryStream memory = new MemoryStream())
			{
				int count = 0;
				do
				{
					count = stream.Read(buffer, 0, size);
					if (count > 0)
					{
						memory.Write(buffer, 0, count);
					}
				}
				while (count > 0);
				return memory.ToArray();
			}
		}
	}

	// Helper Methods.
	private string SQLSafe(string strValue)
	{
		string strReturn = strValue;
		strReturn = strReturn.Replace("'", "''");
		return strReturn;
	}
	
	private string FileSafe(string strValue)
	{
		string strReturn = strValue;
		strReturn = strReturn.Replace(" ", "_");
		strReturn = strReturn.Replace("\\", "");
		strReturn = strReturn.Replace("/", "");
		strReturn = strReturn.Replace(":", "");
		strReturn = strReturn.Replace("*", "");
		strReturn = strReturn.Replace("?", "");
		strReturn = strReturn.Replace("<", "");
		strReturn = strReturn.Replace(">", "");
		strReturn = strReturn.Replace("|", "");
		return strReturn;
	}
	
	/// <summary>
	/// Write the contents of a MemoryStream to an XmlDocument.
	/// </summary>
	/// <param name="objStream">MemoryStream to read.</param>
	private XmlDocument XmlDocumentFromStream(MemoryStream objStream)
	{
		string strXml = "";
		objStream.Position = 0;
		StreamReader objReader = new StreamReader(objStream);
		strXml = objReader.ReadToEnd();

		XmlDocument objXmlDocument = new XmlDocument();
		objXmlDocument.LoadXml(strXml);

		return objXmlDocument;
	}
} 