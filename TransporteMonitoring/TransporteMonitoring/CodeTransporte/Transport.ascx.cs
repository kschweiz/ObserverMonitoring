using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Web.UI;
using System.Configuration;
using System.Net.Mail;
using System.IO;
using System.Web;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;



// #### Semi
// this is how to log from c# over javascript:
// HttpContext.Current.Response.Write(String.Format("<script type=\"\" language=\"\">console.log('{0}');</script>", "test"));
// ####
public partial class Transport : System.Web.UI.UserControl
{
	private OleDbConnection con;
	private OleDbCommand sql;
	private OleDbDataReader reader;
	private List<FirmaClass> firmen = new List<FirmaClass>();
	private List<FirmaClass> alleFirmen = new List<FirmaClass>();
    private List<Hashtable> runMonitoring;
    int monitorID;

    // Valid Prüfung für Emailaddressen
    bool isValidAbholen = true;
    bool isValidLiefern = true;
    bool isValidCC = true;

	// Eindeutige ID
	Guid guid;

	// Applikations Pfad für Logfile
	string logPath = AppDomain.CurrentDomain.BaseDirectory;



	// Username
	readonly string username = HttpContext.Current.User.Identity.Name;

	// Firmen Klasse
	public class FirmaClass
	{
        public int id { get; set; }
		public string name { get; set; }
		public string strasse { get; set; }
		public int plz { get; set; }
		public string ort { get; set; }
		public string telefon { get; set; }
		public string mail { get; set; }
		public bool status { get; set; }
	}

	// Gebinde Klase 
	public class WarenClass
	{
		public string gebinde { get; set; }
		public int anzahl { get; set; }
		public string dimension { get; set; }
		public bool transport { get; set; }
	}

	// SqlConnection
	public void SqlConnection()
	{
		String connect = ConfigurationManager.ConnectionStrings["mainDB"].ProviderName.ToString() +
		ConfigurationManager.ConnectionStrings["mainDB"].ConnectionString.ToString();
		con = new OleDbConnection(connect);
		con.Open();
		sql = con.CreateCommand();
	}

	//Main
	protected void Page_Init(object sender, EventArgs e)
	{		
		// Erstellt Datei falls sie noch nicht existiert.
		if (!File.Exists(logPath + "/Log.txt"))
		{
			var myFile = File.Create(logPath + "/Log.txt");
			myFile.Close();
		}


        // Liesst Firmen aus
        try
        {
            getAlleFirmen();

        }
        catch(Exception ex)
        {
            WriteLogFile(ConfigurationManager.AppSettings["PathTransportLog"].ToString(), DateTime.Now, ex.Message, "Step GetAlleFirmen", true, true, true, tb_username.Text);
        }

		getFirmen();

		if (!IsPostBack)
		{
			// Setzt Standard
			setStandardAbholen();
			setStandardLiefern();

			// Setzt aktuelle Zeit und Datum
			date_abholen.Date = DateTime.Now.AddHours(1);
			date_liefern.Date = DateTime.Now.AddHours(3);
			date_abholen.MinDate = date_abholen.Date.AddDays(-1);
			date_liefern.MinDate = date_liefern.Date.AddDays(-1);
		}
		
		// Setzt für das Formular den Aussteller
		switch(username){
			case "ferag_einkauf_1":
				tb_username.Text = "Ked";
				tb_cc.Text = "ked@ferag.com";
				break;
			case "ferag_einkauf_2":
				tb_username.Text = "Gldi";
				tb_cc.Text = "gldi@ferag.com";
				break;
			case "ferag_einkauf_3":
				tb_username.Text = "Sive";
				tb_cc.Text = "sive@ferag.com";
				break;
		}
	}


	// Combo box 1
	protected void combo_ab_name_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (username.StartsWith("ferag") || username.StartsWith("denipro"))
		{
			foreach (FirmaClass fc in alleFirmen)
			{
				if (combo_ab_name.Text == fc.name)
				{
					tb_ab_strasse.Text = fc.strasse;
					tb_ab_plz.Text = fc.plz.ToString();
					tb_ab_ort.Text = fc.ort;
					tb_ab_telefon.Text = fc.telefon;
					tb_ab_mail.Text = fc.mail;

					lb_ab_vorschlag1.Text = "";
					lb_ab_vorschlag2.Text = "";
					lb_ab_vorschlag3.Text = "";
					lbl_ab_linkbutton.Text = "";
					return;
				}
			}

		}
		else
		{
			foreach (FirmaClass fc in firmen)
			{
				if (combo_ab_name.Text == fc.name)
				{
					tb_ab_strasse.Text = fc.strasse;
					tb_ab_plz.Text = fc.plz.ToString();
					tb_ab_ort.Text = fc.ort;
					tb_ab_telefon.Text = fc.telefon;
					tb_ab_mail.Text = fc.mail;

					lb_ab_vorschlag1.Text = "";
					lb_ab_vorschlag2.Text = "";
					lb_ab_vorschlag3.Text = "";
					lbl_ab_linkbutton.Text = "";
					return;
				}
			}
		}


		// Macht Vorschläge
		if (combo_ab_name.Text != "")
		{
			checksimilarAbholen();
		}
		
	}

	// Combo box 2
	protected void combo_li_name_SelectedIndexChanged(object sender, EventArgs e)
	{

		if (username.StartsWith("ferag") || username.StartsWith("denipro"))
		{
			foreach (FirmaClass fc in alleFirmen)
			{
				if (combo_li_name.Text == fc.name)
				{
					tb_li_strasse.Text = fc.strasse;
					tb_li_plz.Text = fc.plz.ToString();
					tb_li_ort.Text = fc.ort;
					tb_li_telefon.Text = fc.telefon;
					tb_li_mail.Text = fc.mail;

					lb_li_vorschlag1.Text = "";
					lb_li_vorschlag2.Text = "";
					lb_li_vorschlag3.Text = "";
					lbl_li_linkbutton.Text = "";
					return;
				}
			}

		}
		else
		{
			foreach (FirmaClass fc in firmen)
			{
				if (combo_li_name.Text == fc.name)
				{
					tb_li_strasse.Text = fc.strasse;
					tb_li_plz.Text = fc.plz.ToString();
					tb_li_ort.Text = fc.ort;
					tb_li_telefon.Text = fc.telefon;
					tb_li_mail.Text = fc.mail;


					lb_li_vorschlag1.Text = "";
					lb_li_vorschlag2.Text = "";
					lb_li_vorschlag3.Text = "";
					lbl_li_linkbutton.Text = "";
					return;
				}
			}
		}

		// Macht Vorschläge
		if (combo_li_name.Text != "")
		{
			checksimilarLiefern();
		}
	}


	// Gibt die Gruppen id zurück
	public int getUserID()
	{
		SqlConnection();

		sql.CommandText = "SELECT * FROM tbl_gruppe;";
		reader = sql.ExecuteReader();

		int count = 1;

		while (reader.Read())
		{
			count++;
			if (username == reader.GetValue(1).ToString())
			{
				int user = Convert.ToInt32(reader.GetValue(0));
				reader.Close();
				con.Close();
				return user;
			}
		}
		reader.Close();

		// erstellt neuen Benutzer
		sql.CommandText = "INSERT INTO tbl_gruppe([Name]) VALUES('" + username + "')";
		sql.ExecuteNonQuery();


		con.Close();
		return count;
	}


	// Setzt Standard Abholen
	public void setStandardAbholen()
	{
		int user = getUserID();
		SqlConnection();
		sql.CommandText = "SELECT fi.Name FROM tbl_abholen ab " +
							" INNER JOIN tbl_firma fi ON fi.ID = ab.FS_Firma " +
							" WHERE ab.FS_Gruppe = " + user + " ;";
		reader = sql.ExecuteReader();

		while (reader.Read())
		{
			combo_ab_name.Text = reader.GetValue(0).ToString();
		}

		object sender = null;
		EventArgs e = null;
		combo_ab_name_SelectedIndexChanged(sender, e);

		reader.Close();
		con.Close();
	}

	// Setzt Standard Liefern
	public void setStandardLiefern()
	{
		int user = getUserID();
		SqlConnection();
		sql.CommandText = "SELECT fi.Name FROM tbl_liefern li " +
							" INNER JOIN tbl_firma fi ON fi.ID = li.FS_Firma " +
							" WHERE li.FS_Gruppe = " + user + " ;";
		reader = sql.ExecuteReader();

		while (reader.Read())
		{
			combo_li_name.Text = reader.GetValue(0).ToString();
		}

		object sender = null;
		EventArgs e = null;
		combo_li_name_SelectedIndexChanged(sender, e);

		reader.Close();
		con.Close();
	}


	// Vorschläge
	protected void checksimilarAbholen()
	{
		List<string> similar = new List<string>();

		lb_ab_vorschlag1.Text = "";
		lb_ab_vorschlag2.Text = "";
		lb_ab_vorschlag3.Text = "";
		lbl_ab_linkbutton.Text = "";

		for (int c = 1; c < 4; c++)
		{
			foreach (FirmaClass fc in firmen)
			{
				int i = Compute(fc.name, combo_ab_name.Text);

				if (i == c)
				{
					similar.Add(fc.name + "<pre>" +
										  fc.strasse + "<br>" + fc.plz + " " + fc.ort + "<br>" + fc.telefon + "<br>" + fc.mail + "</pre>");

					if (similar.Count > 3)
					{
						c = 5;
						break;
					}
				}
			}
		}

		try
		{
			lb_ab_vorschlag1.Text = similar[0];
			lbl_ab_linkbutton.Text = "Vorschläge:";
			lb_ab_vorschlag2.Text = similar[1];
			lb_ab_vorschlag3.Text = similar[2];

		}
		catch { }
	}

	// Vorschläge
	protected void checksimilarLiefern()
	{
		List<string> similar = new List<string>();

		lb_li_vorschlag1.Text = "";
		lb_li_vorschlag2.Text = "";
		lb_li_vorschlag3.Text = "";
		lbl_li_linkbutton.Text = "";

		for (int c = 1; c < 4; c++)
		{
			foreach (FirmaClass fc in firmen)
			{
				int i = Compute(fc.name, combo_li_name.Text);

				if (i == c)
				{
					similar.Add(fc.name + "<pre>" +
										  fc.strasse + "<br>" + fc.plz + " " + fc.ort + "<br>" + fc.telefon + "<br>" + fc.mail + "</pre>");

					if (similar.Count > 3)
					{
						c = 5;
						break;
					}
				}
			}
		}

		try
		{
			lb_li_vorschlag1.Text = similar[0];
			lbl_li_linkbutton.Text = "Vorschläge:";
			lb_li_vorschlag2.Text = similar[1];
			lb_li_vorschlag3.Text = similar[2];
		}
		catch { }
	}


	// Vergleicher
	public static int Compute(string s, string t)
	{
		int n = s.Length;
		int m = t.Length;
		int[,] d = new int[n + 1, m + 1];

		// Step 1
		if (n == 0)
		{
			return m;
		}

		if (m == 0)
		{
			return n;
		}

		// Step 2
		for (int i = 0; i <= n; d[i, 0] = i++)
		{
		}

		for (int j = 0; j <= m; d[0, j] = j++)
		{
		}

		// Step 3
		for (int i = 1; i <= n; i++)
		{
			//Step 4
			for (int j = 1; j <= m; j++)
			{
				// Step 5
				int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

				// Step 6
				d[i, j] = Math.Min(
					Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
					d[i - 1, j - 1] + cost);
			}
		}
		// Step 7
		return d[n, m];
	}


	// Gibt alle Firmen Namen zurück
	public void getAlleFirmen()
	{
		// Erstellt eine Liste aller Firmen
		SqlConnection();

		sql.CommandText = "SELECT * FROM tbl_firma ORDER BY ID;";
		reader = sql.ExecuteReader();

		alleFirmen.Clear();


		while (reader.Read())
		{
            alleFirmen.Add(new FirmaClass()
            {
                id = Int32.Parse(reader.GetValue(0).ToString()),
				name = reader.GetValue(1).ToString(),
				strasse = reader.GetValue(2).ToString(),
				plz = Convert.ToInt32(reader.GetValue(3)),
				ort = reader.GetValue(4).ToString(),
				telefon = reader.GetValue(5).ToString(),
				mail = reader.GetValue(6).ToString(),
				status = Convert.ToBoolean(reader.GetValue(7))
			});

		}
		reader.Close();
		con.Close();
	}

	// Firmenliste
	public void getFirmen()
	{
		try
		{
			combo_ab_name.Items.Clear();
			combo_li_name.Items.Clear();
			firmen.Clear();
		}
		catch { }


		if (username.StartsWith("ferag") || username.StartsWith("denipro"))
		{
			foreach (FirmaClass fc in alleFirmen)
			{
				if (fc.status == true)
				{
					firmen.Add(fc);
				}
			}
		}
		else
		{
			int user = getUserID();


			SqlConnection();

			// Überprüft ob die Firma bereits existiert.
			sql.CommandText = "SELECT fi.Name, fi.Strasse, fi.PLZ, fi.Ort, fi.Telefon, fi.Mail, fi.Aktiv FROM tbl_gruppe_hat_firma ghf" +
									" INNER JOIN tbl_firma fi ON fi.ID = ghf.FS_Firma " +
									" WHERE ghf.FS_Gruppe = " + user + " ORDER BY fi.ID;";
			reader = sql.ExecuteReader();

			while (reader.Read())
			{
				if (Convert.ToBoolean(reader.GetValue(6)) == true)
				{
					firmen.Add(new FirmaClass()
					{
						name = reader.GetValue(0).ToString(),
						strasse = reader.GetValue(1).ToString(),
						plz = Convert.ToInt32(reader.GetValue(2)),
						ort = reader.GetValue(3).ToString(),
						telefon = reader.GetValue(4).ToString(),
						mail = reader.GetValue(5).ToString(),
						status = Convert.ToBoolean(reader.GetValue(6))
					});
				}
			}

			reader.Close();
			con.Close();

		}

		// comboboxen abfüllen
		foreach (FirmaClass fc in firmen)
		{
			try
			{
				combo_ab_name.Items.Add(fc.name);
				combo_li_name.Items.Add(fc.name);
			}
			catch { }
		}

	}



	// Speichern der Abholfirma
	protected void btn_ab_speichern_Click(object sender, EventArgs e)
	{
		int user = getUserID();

		if (combo_ab_name.Text != "" && tb_ab_strasse.Text != "" && tb_ab_ort.Text != "" && tb_ab_plz.Text != "" && tb_ab_telefon.Text != "" && tb_ab_mail.Text != "")
		{
			try
			{
				// Mail überprüfung
				var adr = new MailAddress(tb_ab_mail.Text);
			}
			catch
			{
				string myStringVariable = "Bitte geben Sie eine korrekte E-Mail Adresse an!";
				ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);

				return;
			}

			safeFirma(combo_ab_name.Text, tb_ab_strasse.Text, Convert.ToInt32(tb_ab_plz.Value), tb_ab_ort.Text, tb_ab_telefon.Text, tb_ab_mail.Text, user);



			lb_ab_vorschlag1.Text = "";
			lb_ab_vorschlag2.Text = "";
			lb_ab_vorschlag3.Text = "";
			lbl_ab_linkbutton.Text = "";
			
		}
		else
		{
			string myStringVariable = "Bitte füllen Sie alle Felder aus!";
			ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);
		}
	}

	// Speichert Firma
	private void safeFirma(string name, string strasse, int plz, string ort, string telefon, string mail, int user)
	{
		// merkt sie die lsit id
		int i = 0;
		int s = -1;
		int a = -1;


		// prüft nur auf den Firmennamen in eigenen Firmen
		foreach (FirmaClass fc in firmen)
		{
			if (fc.name == name)
			{
				s = i;
				break;
			}
			i++;
		}

		SqlConnection();

		if (s >= 0)
		{
			// Firmen Update
			if (firmen[s].strasse != strasse || firmen[s].plz != plz
				|| firmen[s].ort != ort || firmen[s].telefon != telefon
				|| firmen[s].mail != mail)
			{
				sql.CommandText = "UPDATE tbl_firma SET [Strasse] = '" + strasse + "', [PLZ] = '" + Convert.ToInt32(plz) +
				"', [Ort] = '" + ort + "', [Telefon] = '" + telefon + "', [Mail] = '" + mail +
				"' WHERE [Name] = '" + name + "';";

				sql.ExecuteNonQuery();

				string myStringVariable = "Die Eigenschaften der Firma wurden geändert!";
				ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);


				File.AppendAllText(logPath + "/Log.txt", username + " hat die Firma " + name + " angepasst! " + "   " + DateTime.Now + Environment.NewLine);
			}
			else 
			{ 
				return; 
			}

		}
		else
		{
			i = 0;
			// prüft nur auf den Firmennamen in allen Firmen vorhanden ist
			foreach (FirmaClass fc in alleFirmen)
			{
				if (fc.name == name)
				{
					a = i;
					break;
				}
				i++;
			}

			if (a >= 0)
			{
				// Firma der Gruppe hinzufügen
				if (alleFirmen[a].strasse == strasse && alleFirmen[a].plz == plz
				&& alleFirmen[a].ort == ort && alleFirmen[a].telefon == telefon
				&& alleFirmen[a].mail == mail)
				{
					int firmenID = a + 1;

					sql.CommandText = "INSERT INTO tbl_gruppe_hat_firma([FS_Firma], [FS_Gruppe])" +
					"VALUES('" + firmenID + "', '" + user + "')";
					sql.ExecuteNonQuery();

					string myStringVariable = "Die Firma wurde hinzugefügt!";
					ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);

					// LOG
					File.AppendAllText(logPath + "/Log.txt", "Die Firma " + name + " wurde dem User: " + username + " hinzugefügt! " + "   " + DateTime.Now + Environment.NewLine);

				}
				// Firmen Name existîert aber nicht genau gleich
				else
				{
					int firmenID = a + 1;

					sql.CommandText = "INSERT INTO tbl_gruppe_hat_firma([FS_Firma], [FS_Gruppe])" +
					   "VALUES('" + firmenID + "', '" + user + "')";
					sql.ExecuteNonQuery();

					string myStringVariable = "Diese Firma existiert bereits, mit ähnlichen Eigenschaften und wurde Ihnen hinzugefügt:\\n\\t" 
						+ alleFirmen[a].name + "\\n\\t" + alleFirmen[a].strasse + "\\n\\t" + alleFirmen[a].plz + " " 
						+ alleFirmen[a].ort + "\\n\\t" + alleFirmen[a].telefon + "\\n\\t" + alleFirmen[a].mail;
					
				  

					// liesst Daten aus
					if(combo_ab_name.Text == name)
					{
						tb_ab_strasse.Text = alleFirmen[a].strasse;
						tb_ab_plz.Value = alleFirmen[a].plz;
						tb_ab_ort.Text = alleFirmen[a].ort;
						tb_ab_telefon.Text = alleFirmen[a].telefon;
						tb_ab_mail.Text = alleFirmen[a].mail;
					}
					else
					{
						tb_li_strasse.Text = alleFirmen[a].strasse;
						tb_li_plz.Value = alleFirmen[a].plz;
						tb_li_ort.Text = alleFirmen[a].ort;
						tb_li_telefon.Text = alleFirmen[a].telefon;
						tb_li_mail.Text = alleFirmen[a].mail;

					}

					ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);

					// LOG
					File.AppendAllText(logPath + "/Log.txt", "Die Firma " + name + " wurde dem User: " + username + " hinzugefügt! " + "   " + DateTime.Now + Environment.NewLine);

				}

				// Aktualisiert Firmen
				getFirmen();
			}
			// Komplett neue Firma
			else
			{
				reader.Close();
				bool isInactiv = false;
				int id = 0;

				// Überprüft ob die Firma existiert aber auf inaktiv gesetzt wurde.
				sql.CommandText = "Select * From tbl_firma WHERE [Name] = '" + name + "';";
				reader = sql.ExecuteReader();


				while(reader.Read())
				{
					isInactiv = true;
					id = Convert.ToInt32(reader.GetValue(0));
				}

				reader.Close();

				if(isInactiv)
				{
					sql.CommandText = "UPDATE tbl_firma SET [Aktiv] = " + true + " WHERE [Name] = '" + name + "';";
					sql.ExecuteNonQuery();



					sql.CommandText = "INSERT INTO tbl_gruppe_hat_firma([FS_Firma], [FS_Gruppe])" +
					   "VALUES('" + id + "', '" + user + "')";
					sql.ExecuteNonQuery();

					// Aktualisiert Firmen
					getAlleFirmen();
					getFirmen();

					combo_ab_name.Items.Add(name);
					combo_ab_name.Items.Add(name);


					// Meldung
					string myStringVariable = "Die Firma wurde erstellt:\\n\\t" + name + "\\n\\t" + strasse + "\\n\\t" + plz + " " + ort + "\\n\\t" + telefon + "\\n\\t" + mail;
					ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);

					// LOG
					File.AppendAllText(logPath + "/Log.txt", "Die Firma " + name + " wurde vom User: " + username + " auf Aktiv gesetzt! " + "   " + DateTime.Now + Environment.NewLine);

				}
				else
				{  
					createFirma(name, strasse, plz, ort, telefon, mail, user);
				
					// LOG
					File.AppendAllText(logPath + "/Log.txt", "Die Firma " + name + " wurde vom User: " + username + " erstellt! " + "   " + DateTime.Now + Environment.NewLine);

				}


			  

			}

		}

		con.Close();



	}


	// Standard Einstellungen setzen Abholen
	protected void btn_ab_favorisieren_Click(object sender, EventArgs e)
	{
		int user = getUserID();
		SqlConnection();

		int i = 1;
		int s = 0;


		// prüft nur auf den Firmennamen
		foreach (FirmaClass fc in alleFirmen)
		{
			if (fc.name == combo_ab_name.Text)
			{
				s = i;
			}
			i++;
		}

		if (s != 0)
		{
			sql.CommandText = "Select * From tbl_abholen WHERE [FS_Gruppe] = " + user + ";";
			reader = sql.ExecuteReader();

			bool hasFirma = false;

			// Prüft ob die Firma der Gruppe bereits zugeordnet ist
			try
			{
				while (reader.Read())
				{
					hasFirma = true;
				}
			}
			catch { }
			reader.Close();

			// erstellt oder ändert den Favoriten
			if (hasFirma)
			{
				sql.CommandText = "UPDATE tbl_abholen SET [FS_Firma] = " + s + " WHERE [FS_Gruppe] = " + user + ";";
			}
			else
			{
				sql.CommandText = "INSERT INTO tbl_abholen([FS_Firma], [FS_Gruppe])" +
						  "VALUES('" + s + "', '" + user + "')";
			}
			sql.ExecuteNonQuery();

			string myStringVariable = "Ihr Favorit wurde geändert!";
			ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);
		}
		else
		{
			string myStringVariable = "Diese Firma existiert noch nicht!";
			ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);
		}


		con.Close();

	}

	// Standardeinstellungnen Abholen
	protected void btn_ab_standard_Click(object sender, EventArgs e)
	{
		setStandardAbholen();
	}


	// Speichern der Lieferungsfirma
	protected void btn_li_speichern_Click(object sender, EventArgs e)
	{
		int user = getUserID();

		if (combo_li_name.Text != "" && tb_li_strasse.Text != "" && tb_li_ort.Text != "" && tb_li_plz.Text != "" && tb_li_telefon.Text != "" && tb_li_mail.Text != "")
		{
			try
			{
				// Mail überprüfung
				var adr = new MailAddress(tb_li_mail.Text);
			}
			catch
			{
				string myStringVariable = "Bitte geben Sie eine korrekte E-Mail Adresse an!";
				ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);

				return;
			}

			safeFirma(combo_li_name.Text, tb_li_strasse.Text, Convert.ToInt32(tb_li_plz.Value), tb_li_ort.Text, tb_li_telefon.Text, tb_li_mail.Text, user);



			lb_li_vorschlag1.Text = "";
			lb_li_vorschlag2.Text = "";
			lb_li_vorschlag3.Text = "";
			lbl_li_linkbutton.Text = "";

		}
		else
		{
			string myStringVariable = "Bitte füllen Sie alle Felder aus!";
			ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);
		}
	}

	// Standard Einstellungen setzen Liefern
	protected void btn_li_favorisieren_Click(object sender, EventArgs e)
	{
		int user = getUserID();
		SqlConnection();

		int i = 1;
		int s = 0;


		// prüft nur auf den Firmennamen
		foreach (FirmaClass fc in alleFirmen)
		{
			if (fc.name == combo_li_name.Text)
			{
				s = i;
			}
			i++;
		}

		if (s != 0)
		{
			sql.CommandText = "Select * From tbl_liefern WHERE [FS_Gruppe] = " + user + ";";
			reader = sql.ExecuteReader();

			bool hasFirma = false;

			// Prüft ob die Firma der Gruppe bereits zugeordnet ist
			try
			{
				while (reader.Read())
				{
					hasFirma = true;
				}
			}
			catch { }
			reader.Close();

			// erstellt oder ändert den Favoriten
			if (hasFirma)
			{
				sql.CommandText = "UPDATE tbl_liefern SET [FS_Firma] = " + s + " WHERE [FS_Gruppe] = " + user + ";";
			}
			else
			{
				sql.CommandText = "INSERT INTO tbl_liefern([FS_Firma], [FS_Gruppe])" +
						  "VALUES('" + s + "', '" + user + "')";
			}
			sql.ExecuteNonQuery();

			string myStringVariable = "Ihr Favorit wurde geändert!";
			ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);


		}
		else
		{
			string myStringVariable = "Diese Firma existiert noch nicht!";
			ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);
		}


		con.Close();

	}

	// Standardeinstellungnen Liefern
	protected void btn_li_standard_Click(object sender, EventArgs e)
	{
		setStandardLiefern();
	}


	// Erstellt Firma
	public void createFirma(string name, string strasse, int plz, string ort, string telefon, string mail, int user)
	{

		// Erstellt Firma
		sql.CommandText = "INSERT INTO tbl_firma([Name], [Strasse], [PLZ], [Ort], [Telefon], [Mail], [Aktiv])" +
						  "VALUES('" + name + "', '" + strasse + "', '" + plz + "', '" + ort + "', '" + telefon + "', '" + mail + "', " + true + ")";
		sql.ExecuteNonQuery();


		int af = alleFirmen.Count + 1;

		// Erstell Verbindung zu User
		sql.CommandText = "INSERT INTO tbl_gruppe_hat_firma([FS_Firma], [FS_Gruppe])" +
			"VALUES('" + af + "', '" + user + "')";
		sql.ExecuteNonQuery();

		// Firma der Liste hinzu fügen
		getAlleFirmen();
		getFirmen();

		// Fügt Firmen der Liste hinzu
		combo_ab_name.Items.Add(name);
		combo_li_name.Items.Add(name);

		// Meldung
		string myStringVariable = "Die Firma wurde erstellt:\\n\\t" + name + "\\n\\t" + strasse + "\\n\\t" + plz + " " + ort + "\\n\\t" + telefon + "\\n\\t" + mail;
		ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);

	}


	// Setzt Vorschlag ein - Vorschläge
	protected void lb_ab_vorschlag1_Click(object sender, EventArgs e)
	{
		string[] splitter = new string[] { "<pre>" };
		string[] result;
		result = lb_ab_vorschlag1.Text.Split(splitter, StringSplitOptions.None);
		combo_ab_name.Text = result[0];

		lb_ab_vorschlag1.Text = "";
		lb_ab_vorschlag2.Text = "";
		lb_ab_vorschlag3.Text = "";
		lbl_ab_linkbutton.Text = "";

		combo_ab_name_SelectedIndexChanged(sender, e);
	}

	protected void lb_ab_vorschlag2_Click(object sender, EventArgs e)
	{
		string[] splitter = new string[] { "<pre>" };
		string[] result;
		result = lb_ab_vorschlag2.Text.Split(splitter, StringSplitOptions.None);
		combo_ab_name.Text = result[0];

		lb_ab_vorschlag1.Text = "";
		lb_ab_vorschlag2.Text = "";
		lb_ab_vorschlag3.Text = "";
		lbl_ab_linkbutton.Text = "";

		combo_ab_name_SelectedIndexChanged(sender, e);
	}

	protected void lb_ab_vorschlag3_Click(object sender, EventArgs e)
	{
		string[] splitter = new string[] { "<pre>" };
		string[] result;
		result = lb_ab_vorschlag3.Text.Split(splitter, StringSplitOptions.None);
		combo_ab_name.Text = result[0];

		lb_ab_vorschlag1.Text = "";
		lb_ab_vorschlag2.Text = "";
		lb_ab_vorschlag3.Text = "";
		lbl_ab_linkbutton.Text = "";

		combo_ab_name_SelectedIndexChanged(sender, e);
	}


	protected void lb_li_vorschlag1_Click(object sender, EventArgs e)
	{
		string[] splitter = new string[] { "<pre>" };
		string[] result;
		result = lb_li_vorschlag1.Text.Split(splitter, StringSplitOptions.None);
		combo_li_name.Text = result[0];

		lb_li_vorschlag1.Text = "";
		lb_li_vorschlag2.Text = "";
		lb_li_vorschlag3.Text = "";
		lbl_li_linkbutton.Text = "";

		combo_li_name_SelectedIndexChanged(sender, e);

	}

	protected void lb_li_vorschlag2_Click(object sender, EventArgs e)
	{
		string[] splitter = new string[] { "<pre>" };
		string[] result;
		result = lb_li_vorschlag2.Text.Split(splitter, StringSplitOptions.None);
		combo_li_name.Text = result[0];

		lb_li_vorschlag1.Text = "";
		lb_li_vorschlag2.Text = "";
		lb_li_vorschlag3.Text = "";
		lbl_li_linkbutton.Text = "";

		combo_li_name_SelectedIndexChanged(sender, e);
	}

	protected void lb_li_vorschlag3_Click(object sender, EventArgs e)
	{
		string[] splitter = new string[] { "<pre>" };
		string[] result;
		result = lb_li_vorschlag3.Text.Split(splitter, StringSplitOptions.None);
		combo_li_name.Text = result[0];

		lb_li_vorschlag1.Text = "";
		lb_li_vorschlag2.Text = "";
		lb_li_vorschlag3.Text = "";
		lbl_li_linkbutton.Text = "";

		combo_li_name_SelectedIndexChanged(sender, e);
	}


	// Abschicken
	protected void btn_abschicken_Click(object sender, EventArgs e)
	{
        //Write Log (For Debug)
        if(ConfigurationManager.AppSettings["TransportLog"] == "true")
        {
            WriteLogFile(ConfigurationManager.AppSettings["PathTransportLog"].ToString(), DateTime.Now, "Neuer Transport Eintrag erstellen", "Step 1 (Start)", true, false, false, tb_username.Text);
        }

		// überprüft das Gesamt gewicht
		if(tb_gesamtgewicht.Number == 0)
		{
			// Meldung
			string myStringVariable = "Bitte geben Sie ein Gesamtgewicht an!";
			ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);

            //Write Log (For Debug)
            if (ConfigurationManager.AppSettings["TransportLog"] == "true")
            {
                WriteLogFile(ConfigurationManager.AppSettings["PathTransportLog"].ToString(), DateTime.Now, "Abbruch wegen falschen Angaben im GUI! Gesamtgewicht fehlt!", "Step FAIL", false, true, true, tb_username.Text);
            }

            return;
		}


		// überprüft den Aussteller
		if (tb_username.Text == "")
		{
			// Meldung
			string myStringVariable = "Bitte geben Sie den Aussteller an!";
			ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);

            //Write Log (For Debug)
            if (ConfigurationManager.AppSettings["TransportLog"] == "true")
            {
                WriteLogFile(ConfigurationManager.AppSettings["PathTransportLog"].ToString(), DateTime.Now, "Abbruch wegen falschen Angaben im GUI! Aussteller fehlt!", "Step FAIL", false, true, true, tb_username.Text);
            }

            return;
		}

		// überprüft Datum
		//TimeSpan abholen = TimeSpan.Parse(time_abholen.Value.ToString().Split(' ')[1]);
		//TimeSpan liefern = TimeSpan.Parse(time_liefern.Value.ToString().Split(' ')[1]);

		if (DateTime.Compare(Convert.ToDateTime(date_abholen.Value), Convert.ToDateTime(date_liefern.Value)) < 0)
		{ }
		else
		{
			// Meldung
			string myStringVariable = "Bitte geben Sie ein korrektes Datum an!";
			ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);

            //Write Log (For Debug)
            if (ConfigurationManager.AppSettings["TransportLog"] == "true")
            {
                WriteLogFile(ConfigurationManager.AppSettings["PathTransportLog"].ToString(), DateTime.Now, "Abbruch wegen falschen Angaben im GUI! Falsches Datum!", "Step FAIL", false, true, true, tb_username.Text);
            }

            return;
		}


		// überprüft ob irgedwas bestellt wurde
		bool counter = false;

		// merkt sie die list id
		int i = 1;
		int f1 = 0;
		int f2 = 0;


		// prüft nur auf den Firmennamen
		foreach (FirmaClass fc in alleFirmen)
		{
			if (fc.name == combo_ab_name.Text && fc.strasse == tb_ab_strasse.Text && fc.plz == Convert.ToInt32(tb_ab_plz.Value) && fc.ort == tb_ab_ort.Text && fc.telefon == tb_ab_telefon.Text && fc.mail == tb_ab_mail.Text)
			{
				f1 = fc.id;
			}
			if (fc.name == combo_li_name.Text && fc.strasse == tb_li_strasse.Text && fc.plz == Convert.ToInt32(tb_li_plz.Value) && fc.ort == tb_li_ort.Text && fc.telefon == tb_li_telefon.Text && fc.mail == tb_li_mail.Text)
			{
				f2 = fc.id;
			}
			i++;
		}

		if (f1 != 0 && f2 != 0 && f1 != f2)
		{
			SqlConnection();


			List<WarenClass> daten = new List<WarenClass>();

			// Transport
			daten.Add(new WarenClass() { gebinde = "unverpackt", anzahl = Convert.ToInt32(tb_anzahl_unverpackt.Number), dimension = tb_dimension_unverpackt.Text, transport = true });
			daten.Add(new WarenClass() { gebinde = "Kisten", anzahl = Convert.ToInt32(tb_anzahl_kisten.Number), dimension = tb_dimension_kisten.Text, transport = true });
			daten.Add(new WarenClass() { gebinde = "Schachteln", anzahl = Convert.ToInt32(tb_anzahl_schachteln.Number), dimension = tb_dimension_schachteln.Text, transport = true });
			daten.Add(new WarenClass() { gebinde = "Spez. Palett", anzahl = Convert.ToInt32(tb_anzahl_spezpalett.Number), dimension = tb_dimension_spezpalett.Text, transport = true });
			daten.Add(new WarenClass() { gebinde = "Gestell", anzahl = Convert.ToInt32(tb_anzahl_gestell.Number), dimension = tb_dimension_gestell.Text, transport = true });
			daten.Add(new WarenClass() { gebinde = "Einwegpalett", anzahl = Convert.ToInt32(tb_anzahl_einwegpalett.Number), dimension = tb_dimension_einwegpalett.Text, transport = true });
			daten.Add(new WarenClass() { gebinde = "Boxen", anzahl = Convert.ToInt32(tb_anzahl_boxen.Number), dimension = tb_dimension_boxen.Text, transport = true });
			daten.Add(new WarenClass() { gebinde = "Euro Palett", anzahl = Convert.ToInt32(tb_anzahl_europalett.Number), dimension = "-", transport = true });
			daten.Add(new WarenClass() { gebinde = "Euro Rahmen", anzahl = Convert.ToInt32(tb_anzahl_eurorahmen.Number), dimension = "-", transport = true });
			daten.Add(new WarenClass() { gebinde = "Euro Deckel", anzahl = Convert.ToInt32(tb_anzahl_eurodeckel.Number), dimension = "-", transport = true });
			daten.Add(new WarenClass() { gebinde = "LKW MW", anzahl = Convert.ToInt32(tb_anzahl_lkwmw.Number), dimension = "-", transport = true });
			daten.Add(new WarenClass() { gebinde = "LKW Zug", anzahl = Convert.ToInt32(tb_anzahl_lkwzug.Number), dimension = "-", transport = true });

			// Tausch
			daten.Add(new WarenClass() { gebinde = "Euro Palett", anzahl = Convert.ToInt32(tb_anzahl_tauscheuropalett.Number), dimension = "-", transport = false });
			daten.Add(new WarenClass() { gebinde = "Euro Rahmen", anzahl = Convert.ToInt32(tb_anzahl_tauscheurorahmen.Number), dimension = "-", transport = false });
			daten.Add(new WarenClass() { gebinde = "Euro Deckel", anzahl = Convert.ToInt32(tb_anzahl_tauscheurodeckel.Number), dimension = "-", transport = false });
			daten.Add(new WarenClass() { gebinde = "Spezial Palett", anzahl = Convert.ToInt32(tb_anzahl_tauschspezpalett.Number), dimension = "-", transport = false });

			
			// Durchläuft alle Elemente
			foreach (WarenClass data in daten)
			{
				if (data.anzahl != 0)
				{
					counter = true;
				}
			}

			// Bricht ab wenn nichts ausgewählt wurde
			if (counter == false)
			{
				// Meldung
				string myStringVariable = "Sie müssen mindestens ein Gebinde auswählen";
				ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);

                //Write Log (For Debug)
                if (ConfigurationManager.AppSettings["TransportLog"] == "true")
                {
                    WriteLogFile(ConfigurationManager.AppSettings["PathTransportLog"].ToString(), DateTime.Now, "Einträge sind nicht korrekt --> Abbruch! Es wurde kein Gebinde ausgewählt!", "Step 2", false, true, true, tb_username.Text);
                }

                return;
			}

            //Write Log (For Debug)
            if (ConfigurationManager.AppSettings["TransportLog"] == "true")
            {
                WriteLogFile(ConfigurationManager.AppSettings["PathTransportLog"].ToString(), DateTime.Now, "Einträge wurden erfolgreich geprüft", "Step 2", false, false, false, tb_username.Text);
            }

            string abh = date_abholen.Text;
			string lie = date_liefern.Text;

			con.Close();
            try
            {
                if (!sendMail(daten))
                {
                    string failAdress = "";
                    if (!isValidAbholen)
                    {
                        failAdress = tb_ab_mail.Text;
                    }else if (!isValidLiefern)
                    {
                        failAdress = tb_li_mail.Text;
                    }else if (!isValidCC)
                    {
                        failAdress = tb_cc.Text;
                    }
                    //Write Log (For Debug)
                    if (ConfigurationManager.AppSettings["TransportLog"] == "true")
                    {
                        WriteLogFile(ConfigurationManager.AppSettings["PathTransportLog"].ToString(), DateTime.Now, "Abbruch Mail konnte nicht gesendet werden! Ungültige E-Mail Addresse: " + failAdress, "Step Mail", false, true, true, tb_username.Text);
                    }

                    return;
                }
            }
            catch(Exception ex)
            {
                // Meldung
                string myStringVariable = "FEHLER!\nDer Transport konnte nicht aufgenommen.";
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);

                //Write Log (For Debug)
                if (ConfigurationManager.AppSettings["TransportLog"] == "true")
                {
                    WriteLogFile(ConfigurationManager.AppSettings["PathTransportLog"].ToString(), DateTime.Now, ex.ToString(), "Step Send Mail", false, false, false, tb_username.Text);
                }
            }


			SqlConnection();	
			// Guid
			guid = Guid.NewGuid();

            //Write Log (For Debug)
            if (ConfigurationManager.AppSettings["TransportLog"] == "true")
            {
                WriteLogFile(ConfigurationManager.AppSettings["PathTransportLog"].ToString(), DateTime.Now, "Daten: " + tb_username.Text + "', '" + abh + "', '" + lie + "'," +
                "'" + tb_transportbemerkungen.Text + "', '" + tb_sonstigebemerkungen.Text + "', '" + f1 + "', '" + f2 + "', '" + Convert.ToDouble(tb_gesamtgewicht.Number) + "', '" + combo_warentyp.Text + "', '" + guid.ToString(), "Step 3", false, false, false, tb_username.Text);
            }

            try
            {
                // Erstellt den Transport
                sql.CommandText = "INSERT INTO tbl_transport([Aussteller], [Abholen_Datum], [Liefern_Datum], [Transport_Bemerkungen], [Sonstige_Bemerkungen], [Abholen_Firma], [Liefern_Firma], [Kilogramm], [Warentyp], [Transport_ID])" +
                "VALUES('" + tb_username.Text + "', '" + abh + "', '" + lie + "'," +
                "'" + tb_transportbemerkungen.Text + "', '" + tb_sonstigebemerkungen.Text + "', '" + f1 + "', '" + f2 + "', '" + Convert.ToDouble(tb_gesamtgewicht.Number) + "', '" + combo_warentyp.Text + "', '" + guid.ToString() + "')";
                sql.ExecuteNonQuery();

                //Write Log (For Debug)
                if (ConfigurationManager.AppSettings["TransportLog"] == "true")
                {
                    WriteLogFile(ConfigurationManager.AppSettings["PathTransportLog"].ToString(), DateTime.Now, "DB Eintrag in Transporte wurde erfolgreich erstellt", "Step 4", false, true, false, tb_username.Text);
                }
            }
            catch(Exception ex)
            {
                //Write Log (For Debug)
                if (ConfigurationManager.AppSettings["TransportLog"] == "true")
                {
                    WriteLogFile(ConfigurationManager.AppSettings["PathTransportLog"].ToString(), DateTime.Now, "DB Eintrag in Transporte konnte nicht erstellt werden"+ex.Message, "Step 4", false, true, true, tb_username.Text);
                }
            }

		

			// Erstellt die Waren
			foreach (WarenClass data in daten)
			{
				if (data.anzahl != 0)
				{
					sql.CommandText = "INSERT INTO tbl_ware([Anzahl], [Dimension], [Gebinde], [Transport], [FS_Transport])" +
					 "VALUES('" + data.anzahl + "', '" + data.dimension + "', '" + data.gebinde + "', '" + data.transport + "', '" + guid + "')";
					sql.ExecuteNonQuery();
				}
			}

			con.Close();

			// Maske leeren
			tb_gesamtgewicht.Value = 0;
			combo_warentyp.Text = "Träger / Längsteile";

			tb_anzahl_unverpackt.Value = 0;
			tb_anzahl_kisten.Value = 0;
			tb_anzahl_schachteln.Value = 0;
			tb_anzahl_spezpalett.Value = 0;
			tb_anzahl_gestell.Value = 0;
			tb_anzahl_einwegpalett.Value = 0;
			tb_anzahl_boxen.Value = 0;
			tb_anzahl_europalett.Value = 0;
			tb_anzahl_eurorahmen.Value = 0;
			tb_anzahl_eurodeckel.Value = 0;
			tb_anzahl_lkwmw.Value = 0;
			tb_anzahl_lkwzug.Value = 0;

			tb_dimension_unverpackt.Text = "";
			tb_dimension_kisten.Text = "";
			tb_dimension_schachteln.Text = "";
			tb_dimension_spezpalett.Text = "";
			tb_dimension_gestell.Text = "";
			tb_dimension_einwegpalett.Text = "";
			tb_dimension_boxen.Text = "";

			tb_anzahl_tauscheuropalett.Value = 0;
			tb_anzahl_tauscheurorahmen.Value = 0;
			tb_anzahl_tauscheurodeckel.Value = 0;
			tb_anzahl_tauschspezpalett.Value = 0;

			tb_transportbemerkungen.Text = "";
			tb_sonstigebemerkungen.Text = "";
			tb_cc.Text = "";

		}
		else
		{
			// Meldung
			string myStringVariable = "Bitte geben Sie beide Firmen an und speichern Sie die Firmen ab, falls Sie Änderungen an den Firmen vorgenommen haben!";
			ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);

            //Write Log (For Debug)
            if (ConfigurationManager.AppSettings["TransportLog"] == "true")
            {
                WriteLogFile(ConfigurationManager.AppSettings["PathTransportLog"].ToString(), DateTime.Now, "Abbruch wegen falschen Angaben im GUI! Änderungen wurden nicht gespeichert.", "Step FAIL", false, true, true, tb_username.Text);
            }

            return;
		}
	}

    //LogFile
    public void WriteLogFile(string path, DateTime date, string message, string step, bool start, bool end, bool failbit, string name)
    {
        StringBuilder sb = new StringBuilder();
        string text = "";
        if (start)
        {
            text += ">--------------------------------------------BEGIN LOG---------------------------------------------<\n";
        }
        text += date + ": "  + step + "\n" + message + "\n";
        if (end)
        {
            text += ">----------------------------------------------END LOG----------------------------------------------<\n\n";
        }
        sb.Append(text);

        

        try
        {
            if (start)
            {
                monitorID  = GetNextID();
                runMonitoring = new List<Hashtable>();
            }


            Hashtable monitor = new Hashtable();
            monitor.Add("start", start);
            monitor.Add("end", end);
            monitor.Add("failbit", failbit);
            monitor.Add("date", date);
            monitor.Add("name", name);
            monitor.Add("message", message);
            monitor.Add("step", step);

            runMonitoring.Add(monitor);


            if (end)
            {
                FileStream stream = new FileStream(Path.Combine(ConfigurationManager.AppSettings["Monitoring"].ToString(), DateTime.Now.ToString("MM/dd/yyyy HHmmss") + ConfigurationManager.AppSettings["MonitoringName"].ToString()) + monitorID, FileMode.Create);
                BinaryFormatter formatter = new BinaryFormatter();

                formatter.Serialize(stream, runMonitoring);
                stream.Close();
            }

        }
        catch(Exception ex)
        {
            sb.Append(date + " " + name + " " + failbit + " " + start + " " + end + " " + message + " " + step + "\n" + ex.ToString() + "\n");
        }

        File.AppendAllText(path, sb.ToString()+"\n");
        sb.Clear();
    }

    public int GetNextID()
    {
        string text = System.IO.File.ReadAllText(ConfigurationManager.AppSettings["MonitorCounter"].ToString());
        int id = Int32.Parse(text);
        id++;
        System.IO.File.WriteAllText(ConfigurationManager.AppSettings["MonitorCounter"].ToString(), id.ToString());
        return id;
    }



	// Mail
	public bool sendMail(List<WarenClass> daten)
	{
		string myStringVariable = "";

		string sender = ConfigurationManager.AppSettings["Sender"];
		string receiver = ConfigurationManager.AppSettings["Receiver"];
		string Subject = null;
		string message = null;


/*
		// Transport und Firma einlesen
		sql.CommandText = "SELECT tbl_transport.*, tbl_firma_abholen.*, tbl_firma_liefern.* " +
						   "FROM tbl_firma AS tbl_firma_liefern INNER JOIN (tbl_firma AS tbl_firma_abholen " +
						   "INNER JOIN tbl_transport ON tbl_firma_abholen.ID = tbl_transport.Abholen_Firma) " +
						   "ON tbl_firma_liefern.ID = tbl_transport.Liefern_Firma WHERE (((tbl_transport.Transport_ID)='" + guid + "'));";
		reader = sql.ExecuteReader();
*/

		// Bemerkungen
		string transportBemerkung = null;
		string sonstigeBemerkung = null;


		// Titel
		Subject = "Transportformular, " + combo_ab_name.Text + " (" + tb_ab_ort.Text + ") - " + combo_li_name.Text + " (" + tb_li_ort.Text + ")";

		// Bemerkungen
		transportBemerkung = tb_transportbemerkungen.Text;
		sonstigeBemerkung = tb_sonstigebemerkungen.Text;                


		#region Message
		
		string[] abDates = date_abholen.Text.Split(' ');
		string[] liDates = date_liefern.Text.Split(' ');
		
		message = "<head></head><body>" +
					"<table style=\"padding-right: 30px;width:550px;\">" +
						"<tr>" +
							"<td colspan=\"3\">" +
								"Achtung: Das ist ein automatisch generiertes E-Mail, bitte nicht auf dieses E-Mail antworten. Kontakt E-Mail: beschaffung@ferag.com" +
							"</br></td>" +
						"</tr>" +				
						"<tr>" +
							"<td colspan=\"3\">" +
								"Transportformular vom " + DateTime.Now + " ausgefüllt von " + tb_username.Text +
							"</td>" +
						"</tr>" +


						"<tr>" +
							"<td colspan=\"3\">" +
								"<hr>" +
							"</td>" +
						"</tr>" +


						"<tr>" +
							"<td colspan=\"2\">" +
								"Abholen bei" +
							"</td>" +
						"</tr>" +
						"<tr>" +
							"<td>" +
								"Name:" +
							"</td>" +
							"<td>" +
							   combo_ab_name.Text +
							"</td>" +
						"</tr>" +
						"<tr>" +
							"<td>" +
								"Strasse:" +
							"</td>" +
							"<td>" +
							   tb_ab_strasse.Text +
							"</td>" +
						"</tr>" +
						"<tr>" +
							"<td>" +
								"PLZ/Ort:" +
							"</td>" +
							"<td>" +
							   tb_ab_plz.Text + " " + tb_ab_ort.Text +
							"</td>" +
						"</tr>" +
						"<tr>" +
							"<td>" +
								"TeleNr:" +
							"</td>" +
							"<td>" +
							   tb_ab_telefon.Text +
							"</td>" +
						"</tr>" +
						"<tr>" +
							"<td>" +
								"E-Mail:" +
							"</td>" +
							"<td>" +
							   tb_ab_mail.Text +
							"</td>" +
						"</tr>" +
						"<tr>" +
							"<td>" +
								"Datum:" +
							"</td>" +
							"<td>" +
							   abDates[0] +
							"</td>" +
						"</tr>" +
						"<tr>" +
							"<td>" +
								"Zeit:" +
							"</td>" +
							"<td>" +
							   abDates[1] +
							"</td>" +
						"</tr>" +

						"<tr>" +
							"<td colspan=\"3\">" +
								"<hr>" +
							"</td>" +
						"</tr>" +


						"<tr>" +
							"<td colspan=\"3\">" +
								"Überbringen zu" +
							"</td>" +
						"</tr>" +
						"<tr>" +
							"<td>" +
								"Name:" +
							"</td>" +
							"<td>" +
							   combo_li_name.Text +
							"</td>" +
						"</tr>" +
						"<tr>" +
							"<td>" +
								"Strasse:" +
							"</td>" +
							"<td>" +
							   tb_li_strasse.Text +
							"</td>" +
						"</tr>" +
						"<tr>" +
							"<td>" +
								"PLZ/Ort:" +
							"</td>" +
							"<td>" +
							   tb_li_plz.Text + " " + tb_li_ort.Text +
							"</td>" +
						"</tr>" +
						"<tr>" +
							"<td>" +
								"TeleNr:" +
							"</td>" +
							"<td>" +
							   tb_li_telefon.Text +
							"</td>" +
						"</tr>" +
						"<tr>" +
							"<td>" +
								"E-Mail:" +
							"</td>" +
							"<td>" +
							   tb_li_mail.Text +
							"</td>" +
						"</tr>" +
						"<tr>" +
							"<td>" +
								"Datum:" +
							"</td>" +
							"<td>" +
							   liDates[0] +
							"</td>" +
						"</tr>" +
						"<tr>" +
							"<td>" +
								"Zeit:" +
							"</td>" +
							"<td>" +
							   liDates[1] +
							"</td>" +
						"</tr>" +
						
						"<tr>" + 
							"<td colspan=\"3\">" +
								"<hr>" +
							"</td>" +
						"</tr>" +



						"<tr>" +
							"<td>" +
								"Transportgut" +
							"</td>" +
						"</tr>" +
						 "<tr>" +
							"<td>" +
								"Gesamtgewicht: " +
							"</td>" +
							"<td>" +
							   Convert.ToDouble(tb_gesamtgewicht.Number) + " kg" +
							"</td>" +
						"</tr>" +
						"<tr>" +
							"<td>" +
								"Materialtyp: " +
							"</td>" +
							"<td>" +
							   combo_warentyp.Text +
							"</td>" +
						"</tr>" + 
						"<br>" +
						"<tr>" +
							"<td>" +
								"Anzahl " +
							"</td>" +
							"<td>" +
							   "Gebinde" +
							"</td>" +
							"<td>" +
							   "Dimension (LxBxH)" +
							"</td>" +
						"</tr>";

		#endregion
		
/*
		// Transportgebinde einlesen
		sql.CommandText = "SELECT * FROM tbl_ware WHERE FS_Transport = '" + guid + "' AND [transport] = 'True';";
		reader = sql.ExecuteReader();
*/
		foreach (WarenClass data in daten)
		{
			if (data.anzahl != 0 && data.transport == true)
			{
				message += "<tr>" +
						"<td>" +
							data.anzahl +
						"</td>" +
						"<td>" +
							data.gebinde + 
						"</td>";
		 
				if (data.dimension != "-")
				{
					message += "<td>" +
									data.dimension +
								"</td>";
				}
			}
		}

		// Tauschgebinde
		message +=  "</tr><tr>" +
						"<td colspan=\"3\">" +
							"<hr>" +
						"</td>" +
					 "</tr>" +


					 "<tr>" +
						"<td colspan=\"3\">" +
							"Tauschgebinde" +
						"</td>" +
					"</tr>" +

					"<br>" +
					"<tr>" +
						"<td>" +
							"Anzahl " +
						"</td>" +
						"<td colspan=\"2\">" +
							"Gebinde" +
						"</td>" +
					"</tr>"+
					"<br><br>";
/*
		// Waren einlesen
		sql.CommandText = "SELECT * FROM tbl_ware WHERE FS_Transport = '" + guid + "' AND [transport] = 'False';";
		reader = sql.ExecuteReader();
*/
		foreach (WarenClass data in daten)
		{
			if (data.anzahl != 0 && data.transport == false)
			{
				message += "<tr>" +
								"<td>" +
									data.anzahl +
								"</td>" +
								"<td>" +
									data.gebinde +
								"</td>";
			}
		}

		if (transportBemerkung != "")
		{

			message += "<tr>" +
							"<td>" +
						transportBemerkung +
							"</td>" +
						"</tr>" +

						"<br>";
					}

		message += "<tr>" +
					"<td colspan=\"3\">" +
						"<hr>" +
					"</td>" +
				"</tr>";

		if (sonstigeBemerkung != "")
		{
			message += "<br><br>" +

					"<tr>" +
						"<td>" +
					sonstigeBemerkung +
						"</td>" +
					"</tr>";
		}

		message += "</table></body>";



		MailMessage Mail = new MailMessage(sender, receiver, Subject, message);
		Mail.IsBodyHtml = true;
		SmtpClient Client = new SmtpClient(ConfigurationManager.AppSettings["Mailserver"]);
		Client.Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);

		// Fügt Firmen CC hinzu
		if (tb_ab_mail.Text != "" && CheckVaildMailAddress(tb_ab_mail.Text))
		{
			Mail.CC.Add(tb_ab_mail.Text);
		}
        else
        {
            // Meldung
            myStringVariable = "Die angegebene E-Mail Adresse " + tb_ab_mail.Text + " im Feld E-Mail Abholen bei ist ungültig";
            ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);
            isValidAbholen = false;
            return false;
        }
		if (tb_li_mail.Text != "" && CheckVaildMailAddress(tb_li_mail.Text))
		{
		   Mail.CC.Add(tb_li_mail.Text);
        }
        else
        {
            // Meldung
            myStringVariable = "Die angegebene E-Mail Adresse " + tb_li_mail.Text + " im Feld E-Mal Liefern an ist ungültig";
            ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);
            isValidLiefern = false;
            return false;
        }

		// überprüft CC
		if (tb_cc.Text != "" && CheckVaildMailAddress(tb_cc.Text))
		{
            MailAddress cc = new MailAddress(tb_cc.Text);
            Mail.CC.Add(cc);
        }
        else if(tb_cc.Text == "")
        {

        }
        else
        {
            // Meldung
            myStringVariable = "Die angegebene E-Mail Adresse" + tb_cc.Text + " im Feld CC ist ungültig";
            ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);
            isValidCC = false;
            return false;
        }

		// Sendet das Mail
		try
		{
			Client.Send(Mail);
            Client.Dispose();
            Mail.Dispose();
		}
		catch (SmtpFailedRecipientException exSmtp)
		{
            SmtpStatusCode statusCode = exSmtp.StatusCode;

            if (statusCode == SmtpStatusCode.MailboxBusy || statusCode == SmtpStatusCode.MailboxUnavailable || statusCode == SmtpStatusCode.TransactionFailed)
            {
                Thread.Sleep(5000);
                try
                {
                    Client.Send(Mail);
                    Mail.Dispose();
                    Client.Dispose();
                }
                catch (Exception ex)
                {
                    WriteLogFile(ConfigurationManager.AppSettings["PathMailLog"].ToString(), DateTime.Now, "Fehler Mail:\nDie Email mit den Empfängern An:" + Mail.To.ToString() + " Cc:" + Mail.CC.ToString() + " konnte nicht gesendet werden!\n" +
                        "FehlerCode: " + statusCode + "\nMessage Smtp: " + exSmtp + "\nMessage Exception: " + ex, "Step Send Mail", true, true, true, tb_username.Text);

                    myStringVariable = "Eine der E-Mail Addressen ist nicht erreichbar";
                    ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);

                    return false;
                }
            }

            
        }

		// Meldung
		myStringVariable = "Der Transport wurde aufgenommen.";
		ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('" + myStringVariable + "');", true);

        

		return true;
	}

    //Check vail MailAddress
    public bool CheckVaildMailAddress(string address)
    {
        string pattern = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$";
        Match match = Regex.Match(address.Trim(), pattern, RegexOptions.IgnoreCase);

        if (match.Success)
            return true;
        else
            return false;

    }

    // Setzt Abhol Firma auf Inaktiv
    protected void btn_ab_inaktiv_Click(object sender, EventArgs e)
	{
		bool existFirma = false;

		foreach (var item in combo_ab_name.Items)
		{
			if(item.ToString() == combo_ab_name.Text)
			{
				existFirma = true;
			}
		}

		if(!existFirma)
		{
			return;
		}

		setInactiv(combo_ab_name.Text);

		// entfernt Item
		try
		{
			combo_li_name.Items.RemoveAt(combo_ab_name.SelectedItem.Index);
			combo_ab_name.Items.RemoveAt(combo_ab_name.SelectedItem.Index);
		}
		catch { }

		// leert Felder
		combo_ab_name.Text = "";
		tb_ab_strasse.Text = "";
		tb_ab_plz.Value = 0;
		tb_ab_ort.Text = "";
		tb_ab_telefon.Text = "";
		tb_ab_mail.Text = "";
	}

	// Setzt Liefer Firma auf Inaktiv
	protected void btn_li_inaktiv_Click(object sender, EventArgs e)
	{
		bool existFirma = false;

		foreach (var item in combo_li_name.Items)
		{
			if (item.ToString() == combo_li_name.Text)
			{
				existFirma = true;
			}
		}

		if (!existFirma)
		{
			return;
		}

		setInactiv(combo_li_name.Text);

		// entfernt Item
		try
		{
			combo_ab_name.Items.RemoveAt(combo_li_name.SelectedItem.Index);
			combo_li_name.Items.RemoveAt(combo_li_name.SelectedItem.Index);
		}
		catch { }

		// leert Felder
		combo_li_name.Text = "";
		tb_li_strasse.Text = "";
		tb_li_plz.Value = 0;
		tb_li_ort.Text = "";
		tb_li_telefon.Text = "";
		tb_li_mail.Text = "";
	}

	// Setzt Inaktiv
	public void setInactiv(string name)
	{
		SqlConnection();

		// Setzt Status auf inaktiv
		sql.CommandText = "UPDATE tbl_firma SET [Aktiv] = " + false + " WHERE [Name] = '" + combo_ab_name.Text + "';";
		sql.ExecuteNonQuery();

	// Löscht Firmen aus ghf tbl
		int firmenId = 0;
		int i = 1;

		foreach (FirmaClass fc in alleFirmen)
		{
			if (name == fc.name)
			{
				firmenId = i;
				break;
			}
			i++;
		}

		sql.CommandText = "Delete FROM tbl_gruppe_hat_firma WHERE [FS_Firma] = " + firmenId + ";";
		sql.ExecuteNonQuery();


		getAlleFirmen();
		getFirmen();

		con.Close();
	}

}

[Serializable]
public class Monitoring
{
    private DateTime date;
    private string name;
    private bool failbit;
    private bool start;
    private bool end;
    private string message;
    private string step;


    public Monitoring(DateTime date, string name, bool failbit, bool start, bool end, string message, string step)
    {
        this.date = date;
        this.name = name;
        this.failbit = failbit;
        this.start = start;
        this.end = end;
        this.message = message;
        this.step = step;
    }



}
