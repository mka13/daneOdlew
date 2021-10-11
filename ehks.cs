using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using mshtml;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
public class ehks {
public static List<Mecz> listaMeczow;
	public static List<Zawodnik> listaDoWyszukiwania;
	public static bool err;
[DllImport("kernel32.dll")]
static extern IntPtr GetConsoleWindow();

[DllImport("user32.dll")]
static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

const int SW_HIDE = 0;
const int SW_SHOW = 5;
	public static void Main(string[] args)
	{
		var handle = GetConsoleWindow();
		ShowWindow(handle, SW_HIDE);



		Form form = new Form();
		form.Text = "Konwerter statystyk Odlew Poznań";
		form.Icon = new Icon("herb.ico");
		form.BackColor = Color.Gray;
		Button button1 = new Button();
		button1.Size = new Size(100, 50);
		button1.Location = new System.Drawing.Point(410, 10);
		button1.Text = "Odśwież";
		ListBox listBox1 = new ListBox();
		button1.BackColor = Color.Black;
		button1.ForeColor = Color.White;
		TextBox box = new TextBox();
		box.ScrollBars = ScrollBars.Vertical;
		Button button2 = new Button();
		button2.Click += ((a, b) =>
		{
						if (listBox1.SelectedItem == null)
			{
				MessageBox.Show("Musisz wybrać mecz z listy", "Niespójne dane", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			string curItem = listBox1.SelectedItem.ToString();
			string adres = (string)listaMeczow.Where(x => (x.przeciwnik + " " + x.data) == curItem).Select(x => x.link).FirstOrDefault();
			
			adres = adres.Substring(0, adres.IndexOf("?"));
			adres = adres.Substring(7);

			box.Text = "";
			Task task = tworzenieRaportu(adres).ContinueWith(x =>
			{
				box.Text = (string)x.Result;
			});
		});

		box.Size = new Size(780, 450);
		box.Multiline = true;
		box.Location = new Point(10, 310);
		form.Controls.Add(box);
		TextBox box2 = new TextBox();
		box2.Width = 200;
		box2.Multiline = false;
		box2.Location = new Point(550, 30);
		form.Controls.Add(box2);
		TextBox box3 = new TextBox();
		Label label = new Label();
		label.Text = "Wyszukaj zawodnika";
		label.Location = new Point(550, 10);
		label.Size = new Size(200, 20);
		form.Controls.Add(label);
		box3.Multiline = true;
		box3.Location = new Point(550, 70);
		box3.Size = new Size(200, 200);
		form.Controls.Add(box3);
		box2.TextChanged += ((a, b) =>
		{
			//wysylanieStatystyk();
			box3.Clear();
			if (box2.Text != "")
			{
				if (listaDoWyszukiwania == null)
				{
					pobieranieListyZawodnikow(box3).ContinueWith(y =>
					{
						listaDoWyszukiwania = (List<Zawodnik>)y.Result; StringBuilder builder = new StringBuilder();
						List<string> bufor = listaDoWyszukiwania.Where(x => x.ksywka.IndexOf(box2.Text) != -1 || x.pelnaNazwa.IndexOf(box2.Text) != -1).Select(x => x.nazwisko).ToList();
						foreach (string x in bufor)
						{
							builder.AppendLine(x);
						}
						box3.Text = builder.ToString();
					});

				}
				else
				{
					StringBuilder builder = new StringBuilder();
					List<string> bufor = listaDoWyszukiwania.Where(x => x.ksywka.IndexOf(box2.Text) != -1 || x.pelnaNazwa.IndexOf(box2.Text) != -1).Select(x => x.nazwisko).ToList();
					foreach (string x in bufor)
					{
						builder.AppendLine(x);
					}
					box3.Text = builder.ToString();
				}
			}
		});
		button2.Size = new Size(100, 50);
		button2.Location = new System.Drawing.Point(410, 70);
		button2.Text = "Generuj tekst";
		button2.BackColor = Color.Black;
		button2.ForeColor = Color.White;






		form.Controls.Add(button2);
		form.Size = new System.Drawing.Size(850, 850);


		listBox1.BackColor = Color.Black;
		listBox1.ForeColor = Color.White;
		listBox1.Size = new System.Drawing.Size(300, 300);
		listBox1.Location = new System.Drawing.Point(10, 10);
		listBox1.MultiColumn = true;
		form.Controls.Add(listBox1);


		button1.Click += ((a, b) =>
		{
			Task task = pobieranieMeczow().ContinueWith(x =>
{


List<Mecz> lista = (List<Mecz>)x.Result;
listaMeczow = new List<Mecz>(lista);
listBox1.Items.Clear();
foreach (Mecz mecz in lista)
{
listBox1.Items.Add(mecz.przeciwnik + " " + mecz.data);
}

});
		});

		form.Controls.Add(button1);

		Button button3 = new Button();
		button3.Size = new Size(100, 50);
		button3.Location = new System.Drawing.Point(410, 130);
		button3.Text = "Prześlij dane";
		button3.BackColor = Color.Black;
		button3.ForeColor = Color.White;
		form.Controls.Add(button3);





		button3.Click += ((a, b) =>
		{

			if (listaMeczow==null ||  listaMeczow.Count == 0)
			{

				MessageBox.Show("Musisz najpierw pobrać listę meczów używając przycisku 'Odśwież'", "Niespójne dane", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;

			}
			else if (listBox1.SelectedItem == null)
			{
				MessageBox.Show("Musisz wybrać mecz z listy", "Niespójne dane", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			DialogResult result = MessageBox.Show("Nastąpi wysłanie danych dla meczu Odlewu z " + listBox1.SelectedItem + ". Czy na pewno?", "Potwierdzenie", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
			if (result != DialogResult.Yes) { 
			MessageBox.Show("Nastąpiło przerwanie wprowadzania danych", "Potwierdzenie", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;


			}
				
			List<Statystyka> listaStatystyk = new List<Statystyka>();
			List<Zawodnik> kadra = new List<Zawodnik>();
	
			Task<List<Zawodnik>> task = pobieranieListyZawodnikow(null);
			task.ContinueWith(z =>
			{
				StatystykaZespolowa statsOdlew = new StatystykaZespolowa();
				StatystykaZespolowa statsPrzeciwnik = new StatystykaZespolowa();
				statsOdlew.nazwaZespolu = "HKS Odlew Poznań";
				List<Zawodnik> listaZawodnikow = (List<Zawodnik>)z.Result;
				string curItem = listBox1.SelectedItem.ToString();
				string adres = (string)listaMeczow.Where(x => (x.przeciwnik + " " + x.data) == curItem).Select(x => x.link).FirstOrDefault();
				string id = adres.Substring(adres.IndexOf("stats") + 6, adres.IndexOf("?") - adres.IndexOf("stats") - 6);
				string przeciwnik = (string)listaMeczow.Where(x => (x.przeciwnik + " " + x.data) == curItem).Select(x => x.przeciwnik).FirstOrDefault();
				statsPrzeciwnik.nazwaZespolu = przeciwnik;
				string tekstZeStatystykami = box.Text;
				string wynik = tekstZeStatystykami.Substring(0, tekstZeStatystykami.IndexOf("Skład Odlewu:"));
				string wynikMeczu = "";

	
				if (wynik.IndexOf("HKS Odlew Poznań") != -1)
				{


					if (wynik.LastIndexOf("(") != -1 && wynik.LastIndexOf(")") != -1)
					{

						wynikMeczu = wynik.Substring(wynik.LastIndexOf("(") + 1, wynik.LastIndexOf(")") - wynik.LastIndexOf("(") - 1);
					}


					List<string> listaZWynikami = wynikMeczu.Split(':').ToList();

					try
					{
						if (wynik.IndexOf("HKS Odlew Poznań") > wynik.IndexOf("-"))
						{

							statsOdlew.goleDoPrzerwy = Convert.ToInt32(listaZWynikami[1]);
							statsPrzeciwnik.goleDoPrzerwy = Convert.ToInt32(listaZWynikami[0]);

						}
						else
						{
							
							statsOdlew.goleDoPrzerwy = Convert.ToInt32(listaZWynikami[0]);
							statsPrzeciwnik.goleDoPrzerwy = Convert.ToInt32(listaZWynikami[1]);

						}
					}
					catch (Exception e)
					{


					}


				}

				string skladTekst = tekstZeStatystykami.Substring(tekstZeStatystykami.IndexOf("Skład Odlewu:"), tekstZeStatystykami.IndexOf("Strzały") - tekstZeStatystykami.IndexOf("Skład Odlewu:"));
				
				ekstrakcjaSkladu(skladTekst, statsOdlew,listaZawodnikow);
				
				string strzalyTekst = tekstZeStatystykami.Substring(tekstZeStatystykami.IndexOf("Strzały"), tekstZeStatystykami.IndexOf("Strzały celne") - tekstZeStatystykami.IndexOf("Strzały"));
				//if(strzalyOdlew.IndexOf("(")!=-1){
				//strzalyOdlew = strzalyOdlew.Substring(0,strzalyOdlew.IndexOf("("));
				//}
				//StatystykaZespolowa statOdlew = new StatystykaZespolowa();
				//StatystykaZespolowa statPrzeciwnik = new StatystykaZespolowa();

				//statPrzeciwnik.strzaly=Convert.ToInt32(strzalyPrzeciwnik);
				//statOdlew.strzaly=Convert.ToInt32(strzalyOdlew);
				//Console.WriteLine("Strzaly przeciwnik " + strzalyPrzeciwnik);
				//statOdlew.strzaly=Convert.ToInt32(strzalyOdlew);
				//strzalyOdlew = strzalyTekst.Substring(strzalyTekst.IndexOf("(")+1);
				//strzalyOdlew = strzalyOdlew.Substring(0,strzalyOdlew.IndexOf(")"));
				//List <string> lista = strzalyOdlew.Split(',').ToList();
				//lista.ForEach(x=>{

				//listaZawodnikow.ForEach(y=>{
				//if(x.Trim().IndexOf(y.nazwisko)==0){
				//try{
				//y.strzaly=Convert.ToInt32(x.Trim().Substring(y.nazwisko.Length));
				//}catch(Exception e)
				//{
				//MessageBox.Show("Dla wartości " + x + " nie możliwa identyfikacja zawodnika", "Niespójne dane", MessageBoxButtons.OK,MessageBoxIcon.Error);
				//err=true;
				//}
				//}
				//});
				//});
				if (err) {
					err = false;
					return; }
				//string strzalyCelneTekst = box.Text.Substring(box.Text.IndexOf("Strzały celne"),box.Text.IndexOf("Faule")-box.Text.IndexOf("Strzały celne"));
				//Console.WriteLine("Tekst strzaly celne: " + strzalyCelneTekst);
				//string fauleTekst = box.Text.Substring(box.Text.IndexOf("Faule"),box.Text.IndexOf("Rogi")-box.Text.IndexOf("Faule"));
				//Console.WriteLine("Tekst faule: " + fauleTekst);
	
				ekstrakcja(statsOdlew, statsPrzeciwnik, strzalyTekst, "Strzały", listaZawodnikow);
				string strzalyCelneTekst = tekstZeStatystykami.Substring(tekstZeStatystykami.IndexOf("Strzały celne"), tekstZeStatystykami.IndexOf("Faule") - tekstZeStatystykami.IndexOf("Strzały celne"));
				ekstrakcja(statsOdlew, statsPrzeciwnik, strzalyCelneTekst, "Strzały celne", listaZawodnikow);
				string fauleTekst = tekstZeStatystykami.Substring(tekstZeStatystykami.IndexOf("Faule"), tekstZeStatystykami.IndexOf("Rogi") - tekstZeStatystykami.IndexOf("Faule"));
				ekstrakcja(statsOdlew, statsPrzeciwnik, fauleTekst, "Faule", listaZawodnikow);
				string rogiTekst = tekstZeStatystykami.Substring(tekstZeStatystykami.IndexOf("Rogi"), tekstZeStatystykami.IndexOf("Spalone") - tekstZeStatystykami.IndexOf("Rogi"));
				ekstrakcja(statsOdlew, statsPrzeciwnik, rogiTekst, "Rogi", listaZawodnikow);
				string spaloneTekst = tekstZeStatystykami.Substring(tekstZeStatystykami.IndexOf("Spalone"), tekstZeStatystykami.IndexOf("Słupki/poprzeczki") - tekstZeStatystykami.IndexOf("Spalone"));
				ekstrakcja(statsOdlew, statsPrzeciwnik, spaloneTekst, "Spalone", listaZawodnikow);
				string aluminumTekst = tekstZeStatystykami.Substring(tekstZeStatystykami.IndexOf("Słupki/poprzeczki"), tekstZeStatystykami.IndexOf("Kartki żółte") - tekstZeStatystykami.IndexOf("Słupki/poprzeczki"));
				ekstrakcja(statsOdlew, statsPrzeciwnik, aluminumTekst, "Słupki/poprzeczki", listaZawodnikow);
				string zolteTekst = tekstZeStatystykami.Substring(tekstZeStatystykami.IndexOf("Kartki żółte"), tekstZeStatystykami.IndexOf("Kartki czerwone") - tekstZeStatystykami.IndexOf("Kartki żółte"));
				ekstrakcja(statsOdlew, statsPrzeciwnik, zolteTekst, "Kartki żółte", listaZawodnikow);
				string czerwoneTekst = tekstZeStatystykami.Substring(tekstZeStatystykami.IndexOf("Kartki czerwone"), tekstZeStatystykami.IndexOf("Asysty") - tekstZeStatystykami.IndexOf("Kartki czerwone"));
				ekstrakcja(statsOdlew, statsPrzeciwnik, czerwoneTekst, "Kartki czerwone", listaZawodnikow);
				
				string asysty= tekstZeStatystykami.Substring(tekstZeStatystykami.IndexOf("Asysty"), tekstZeStatystykami.IndexOf("Gole") - tekstZeStatystykami.IndexOf("Asysty"));
				ekstrakcja(statsOdlew, statsPrzeciwnik, asysty, "Asysty", listaZawodnikow);
				
				string gole = tekstZeStatystykami.Substring(tekstZeStatystykami.IndexOf("Asysty"), tekstZeStatystykami.Length - tekstZeStatystykami.IndexOf("Asysty"));
				ekstrakcja(statsOdlew, statsPrzeciwnik, gole, "Gole", listaZawodnikow);

				List<Zawodnik> listaGrajacychZawodnikow = listaZawodnikow.Where(x => x.czyGral).ToList();
				//listaGrajacychZawodnikow.ForEach(x => { Console.WriteLine(x.nazwisko + " aluminium: " + x.aluminium + " asysty " + x.asysty + " gole " + x.gol); });
				//Console.WriteLine("Odlew gole: " + statsOdlew.gole);

				
				string item = listBox1.SelectedItem.ToString();
					wysylanieStatystyk(listaMeczow.Where(x => (x.przeciwnik + " " + x.data) == item).Select(x => x.id).FirstOrDefault().ToString(), listaGrajacychZawodnikow, statsOdlew, statsPrzeciwnik);
				
			});
		});





		form.ShowDialog();





	



















}
	public async static Task wysylanieStatystyk(string idMeczu, List<Zawodnik> listaZawodnikow,StatystykaZespolowa statsOdlew,StatystykaZespolowa statsPrzeciwnik) {
		

		await Task.Run(() => {

			
			string text = File.ReadAllText("Konfig.txt");
			string user = text.Split(';')[0];
			string haslo = text.Split(';')[1];

			Sesja sesja = new Sesja();
			CookieContainer cookies = new CookieContainer();
		HttpClientHandler handler = new HttpClientHandler();
		handler.CookieContainer = cookies;
		IWebProxy proxy = WebRequest.GetSystemWebProxy();
		proxy.Credentials = CredentialCache.DefaultCredentials;
		var pairs = new List<KeyValuePair<string, string>>();
		handler.Proxy = proxy;
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		
			
			HttpClient client = new HttpClient(handler);
			client.BaseAddress = new Uri("http://www.odlew.poznan.pl/system/login");
			pairs = new List<KeyValuePair<string, string>>
{
new KeyValuePair<string,string>("data[User][username]",user),
new KeyValuePair<string,string>("data[User][password]",haslo)


};
			var content = new FormUrlEncodedContent(pairs);
			var result = client.PostAsync("", content).Result;
			string data = result.Content.ReadAsStringAsync().Result;
			IEnumerable<Cookie> responseCookies = cookies.GetCookies(new Uri("http://www.odlew.poznan.pl")).Cast<Cookie>();
				
				sesja = new Sesja();
				foreach (Cookie cookie in responseCookies)
				{
					sesja.numer = cookie.Value;
				}
				
				
				cookies.Add(new Uri("http://www.odlew.poznan.pl"), new Cookie("PHPSESSID", sesja.numer));		
		
			

			HttpClient client2 = new HttpClient(handler);
			client2.BaseAddress = new Uri("http://www.odlew.poznan.pl/system/matches/set_stats/" + idMeczu);

			pairs = new List<KeyValuePair<string, string>>();

			var result3 = client2.GetAsync("").Result;
			 data = result3.Content.ReadAsStringAsync().Result;
			IHTMLDocument2 doc = new HTMLDocumentClass();
			doc.write(new object[] { data });
			IHTMLElement element = ((IHTMLDocument3)doc).getElementById("MatchSeasonId");
			string MatchSeasonId = (string)element.getAttribute("value");
			element = ((IHTMLDocument3)doc).getElementById("MatchHomeAway");
			string MatchHomeAway = (string)element.getAttribute("value");
			element = ((IHTMLDocument3)doc).getElementById("MatchOpponentId");
			string MatchOpponentId = (string)element.getAttribute("value");
			element = ((IHTMLDocument3)doc).getElementById("MatchDate");
			string MatchDate = (string)element.getAttribute("value");
			pairs.Add(new KeyValuePair<string, string>("data[Match][id]", idMeczu));
			pairs.Add(new KeyValuePair<string, string>("data[Match][home_away]", MatchHomeAway));
			pairs.Add(new KeyValuePair<string, string>("data[Match][opponent_id]", MatchOpponentId));
			pairs.Add(new KeyValuePair<string, string>("data[Match][date]", MatchDate));
			pairs.Add(new KeyValuePair<string, string>("data[Match][season_id]", MatchSeasonId));
			
			int counterField = 1;
			int counterGoal = 1;
			listaZawodnikow.ForEach(x =>
			{
				if (x.bramkarz)
				{

					
					pairs.Add(new KeyValuePair<string, string>("data[GoalkeeperStats][" + counterGoal + "][player_id]", x.identyfikator.ToString()));
					pairs.Add(new KeyValuePair<string, string>("data[GoalkeeperStats][" + counterGoal + "][min_played]", x.czasGry.ToString()));
					if (x.pierwszySklad) {
						pairs.Add(new KeyValuePair<string, string>("data[GoalkeeperStats][" + counterGoal + "][first_team]", "1"));
					}
					else {
						pairs.Add(new KeyValuePair<string, string>("data[GoalkeeperStats][" + counterGoal + "][first_team]", "0"));

					}
					
					counterGoal++;
				}
				else {

					pairs.Add(new KeyValuePair<string, string>("data[FieldStats][" + counterField + "][player_id]", x.identyfikator.ToString()));
					
				pairs.Add(new KeyValuePair<string, string>("data[FieldStats][" + counterField + "][min_played]", x.czasGry.ToString()));
					if (x.pierwszySklad)
					{
						pairs.Add(new KeyValuePair<string, string>("data[FieldStats][" + counterField + "][first_team]", "1"));
					}
					else
					{
						pairs.Add(new KeyValuePair<string, string>("data[FieldStats][" + counterField + "][first_team]", "0"));

					}
					pairs.Add(new KeyValuePair<string, string>("data[FieldStats][" + counterField + "][goals_scored]", x.gol.ToString()));
					pairs.Add(new KeyValuePair<string, string>("data[FieldStats][" + counterField + "][assists]", x.asysty.ToString()));
					pairs.Add(new KeyValuePair<string, string>("data[FieldStats][" + counterField + "][shots_on_target]", x.strzalyCelne.ToString()));
					pairs.Add(new KeyValuePair<string, string>("data[FieldStats][" + counterField + "][shots_missed]", (x.strzaly-x.strzalyCelne).ToString()));
					pairs.Add(new KeyValuePair<string, string>("data[FieldStats][" + counterField + "][posts_hitted]", x.aluminium.ToString()));
					pairs.Add(new KeyValuePair<string, string>("data[FieldStats][" + counterField + "][fouls]", x.faule.ToString()));
					pairs.Add(new KeyValuePair<string, string>("data[FieldStats][" + counterField + "][fouled]", x.faulowany.ToString()));
					pairs.Add(new KeyValuePair<string, string>("data[FieldStats][" + counterField + "][yellow_cards]", x.zolteKartki.ToString()));
					pairs.Add(new KeyValuePair<string, string>("data[FieldStats][" + counterField + "][red_cards]", x.czerwoneKartki.ToString()));
					pairs.Add(new KeyValuePair<string, string>("data[FieldStats][" + counterField + "][offsides]", x.spalone.ToString()));

					counterField++;
				}

				pairs.Add(new KeyValuePair<string, string>("data[Match][our_goals]", statsOdlew.gole.ToString()));
				pairs.Add(new KeyValuePair<string, string>("data[Match][our_shots_on_target]", statsOdlew.strzalyCelne.ToString()));
				pairs.Add(new KeyValuePair<string, string>("data[Match][our_shots_missed]", (statsOdlew.strzaly-statsOdlew.strzalyCelne).ToString()));
				pairs.Add(new KeyValuePair<string, string>("data[Match][our_posts]", statsOdlew.Aluminium.ToString()));
				pairs.Add(new KeyValuePair<string, string>("data[Match][our_corners]", statsOdlew.Rozne.ToString()));
				pairs.Add(new KeyValuePair<string, string>("data[Match][our_offsides]", statsOdlew.spalone.ToString()));
				pairs.Add(new KeyValuePair<string, string>("data[Match][our_fouls]", statsOdlew.Faule.ToString()));
				pairs.Add(new KeyValuePair<string, string>("data[Match][our_yellow_cards]", statsOdlew.zolteKartki.ToString()));
				pairs.Add(new KeyValuePair<string, string>("data[Match][our_red_cards]", statsOdlew.czerwoneKartki.ToString()));
				pairs.Add(new KeyValuePair<string, string>("data[Match][our_half_time_score]", statsOdlew.goleDoPrzerwy.ToString()));
				
				
				pairs.Add(new KeyValuePair<string, string>("data[Match][their_goals]", statsPrzeciwnik.gole.ToString()));
				pairs.Add(new KeyValuePair<string, string>("data[Match][their_shots_on_target]", statsPrzeciwnik.strzalyCelne.ToString()));
				pairs.Add(new KeyValuePair<string, string>("data[Match][their_shots_missed]", (statsPrzeciwnik.strzaly - statsPrzeciwnik.strzalyCelne).ToString()));
				pairs.Add(new KeyValuePair<string, string>("data[Match][their_posts]", statsPrzeciwnik.Aluminium.ToString()));
				pairs.Add(new KeyValuePair<string, string>("data[Match][their_corners]", statsPrzeciwnik.Rozne.ToString()));
				pairs.Add(new KeyValuePair<string, string>("data[Match][their_offsides]", statsPrzeciwnik.spalone.ToString()));
				pairs.Add(new KeyValuePair<string, string>("data[Match][their_fouls]", statsPrzeciwnik.Faule.ToString()));
				pairs.Add(new KeyValuePair<string, string>("data[Match][their_yellow_cards]", statsPrzeciwnik.zolteKartki.ToString()));
				pairs.Add(new KeyValuePair<string, string>("data[Match][their_red_cards]", statsPrzeciwnik.czerwoneKartki.ToString()));
				pairs.Add(new KeyValuePair<string, string>("data[Match][their_half_time_score]", statsPrzeciwnik.goleDoPrzerwy.ToString()));


				//pairs.Add(new KeyValuePair<string, string>("data[Match][our_half_time_score]", statsOdlew.gole.ToString());





			});
			//pairs.ForEach(x => { Console.WriteLine(x.Key + " | " + x.Value); });
			var content2 = new FormUrlEncodedContent(pairs);
			var result2 = client2.PostAsync("", content2).Result;
			MessageBox.Show("Wysyłanie danych zakończone powodzeniem!","Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
		


		});
	
	} 

	
	public static void ekstrakcjaSkladu(string tekst, StatystykaZespolowa statsOdlew, List<Zawodnik> listaZawodnikow)
	{
		int counter = 0;
		tekst = tekst.Substring(13);

		string [] formacje = tekst.Split('-');
		foreach (string x in formacje)
		{

			string [] zawodnicy = x.Split(',');
			foreach (string y in zawodnicy)
			{

				if (y.IndexOf("(") != -1)
				{
					
					int czasGry =  Convert.ToInt32(y.Substring(y.IndexOf("(") + 1, y.IndexOf("'") - y.IndexOf("(") - 1));
					//Console.WriteLine(y.Substring(0, y.IndexOf("(")).Trim());
					Zawodnik zawodnik = listaZawodnikow.Where(z => z.nazwisko == y.Substring(0, y.IndexOf("(")).Trim()).FirstOrDefault();
					if (zawodnik != null)
					{
                        if (counter == 0) { zawodnik.bramkarz = true; }
						zawodnik.czasGry = czasGry;
						zawodnik.czyGral = true;
						zawodnik.pierwszySklad = true;
					}
					else {

						MessageBox.Show("Dla wartości " + y.Substring(0, y.IndexOf("(")).Trim() + " nie możliwa identyfikacja zawodnika", "Niespójne dane", MessageBoxButtons.OK, MessageBoxIcon.Error);
						err = true;

					}
					
					zawodnik = listaZawodnikow.Where(z => z.nazwisko== y.Substring(y.IndexOf("'") + 1, y.IndexOf(")") - y.IndexOf("'") - 1).Trim()).FirstOrDefault();
					if (zawodnik != null)
					{
						if (counter == 0) { zawodnik.bramkarz = true; }
						zawodnik.czasGry = 90 - czasGry;
						zawodnik.czyGral = true;
					}
					else {
						MessageBox.Show("Dla wartości " + y.Substring(y.IndexOf("'") + 1, y.IndexOf(")") - y.IndexOf("'") - 1).Trim() + " nie możliwa identyfikacja zawodnika", "Niespójne dane", MessageBoxButtons.OK, MessageBoxIcon.Error);
						err = true;
					}
				}
				else {

					Zawodnik zawodnik = listaZawodnikow.Where(z => z.nazwisko == y.Trim()).FirstOrDefault();
					if (zawodnik != null)
					{
						if (counter == 0) { zawodnik.bramkarz = true; }
						zawodnik.czasGry = 90;
						zawodnik.czyGral = true;
						zawodnik.pierwszySklad = true;

					}
					else {
						MessageBox.Show("Dla wartości " + y.Trim() + " nie możliwa identyfikacja zawodnika", "Niespójne dane", MessageBoxButtons.OK, MessageBoxIcon.Error);
						err = true;
					}

				}
				counter++;
			}

		}

	}
	public static void ekstrakcja (StatystykaZespolowa statsOdlew, StatystykaZespolowa statsPrzeciwnik,string tekst,string typ, List<Zawodnik> listaZawodnikow)
	{
       // Console.WriteLine("Typ: " + typ);
        //Console.WriteLine("Tekst: " + tekst);
		string wartosciOdlew = "";
		if (typ != "Asysty")
		{
			 wartosciOdlew = tekst.Substring(tekst.IndexOf(statsOdlew.nazwaZespolu) + statsOdlew.nazwaZespolu.Length, tekst.IndexOf(statsPrzeciwnik.nazwaZespolu) - tekst.IndexOf(statsOdlew.nazwaZespolu) - statsOdlew.nazwaZespolu.Length);
		}
		else {
			wartosciOdlew = tekst.Substring(tekst.IndexOf(statsOdlew.nazwaZespolu) + statsOdlew.nazwaZespolu.Length);
		}
			string wartosciPrzeciwnik = "";
		string wartosciFaulowani = "";
		if (tekst.IndexOf(statsPrzeciwnik.nazwaZespolu) != -1)
		{
			if (typ != "Faule")
			{

				wartosciPrzeciwnik = tekst.Substring(tekst.IndexOf(statsPrzeciwnik.nazwaZespolu) + statsPrzeciwnik.nazwaZespolu.Length);


			}
			else
			{
				
				wartosciPrzeciwnik = tekst.Substring(tekst.IndexOf(statsPrzeciwnik.nazwaZespolu) + statsPrzeciwnik.nazwaZespolu.Length);
				if (wartosciPrzeciwnik.IndexOf("(") != -1)
				{
					
			
					wartosciFaulowani = wartosciPrzeciwnik.Substring(wartosciPrzeciwnik.IndexOf("(") + 1);
					wartosciFaulowani = wartosciFaulowani.Substring(0, wartosciFaulowani.IndexOf(")"));
					
					wartosciPrzeciwnik = wartosciPrzeciwnik.Substring(0, wartosciOdlew.IndexOf("("));
					
				}

			}
		}
string wartosci="";
		
		

	
		if (wartosciOdlew.IndexOf("(")!=-1){
wartosci = wartosciOdlew.Substring(0,wartosciOdlew.IndexOf("("));
}else{
wartosci = wartosciOdlew;	
}

		
		if (typ == "Asysty")
		{
			
			statsOdlew.asysty = Convert.ToInt32(wartosci);
			if (wartosciPrzeciwnik != "") {
				statsPrzeciwnik.strzaly = Convert.ToInt32(wartosciPrzeciwnik);
			}
		}

		if (typ == "Gole")
		{
			statsOdlew.gole = Convert.ToInt32(wartosci);
			statsPrzeciwnik.gole = Convert.ToInt32(wartosciPrzeciwnik);
		}
		if (typ=="Strzały"){
	statsOdlew.strzaly= Convert.ToInt32(wartosci);
	statsPrzeciwnik.strzaly = Convert.ToInt32(wartosciPrzeciwnik);
}
if(typ=="Strzały celne"){
	statsOdlew.strzalyCelne= Convert.ToInt32(wartosci);
	statsPrzeciwnik.strzalyCelne = Convert.ToInt32(wartosciPrzeciwnik);
}
if(typ=="Faule"){
	statsOdlew.Faule= Convert.ToInt32(wartosci);
	statsPrzeciwnik.Faule = Convert.ToInt32(wartosciPrzeciwnik);
			statsOdlew.Faulowani = Convert.ToInt32(wartosciPrzeciwnik);
			statsPrzeciwnik.Faulowani = Convert.ToInt32(wartosci);
		}
if(typ=="Rogi"){
	statsOdlew.Rozne= Convert.ToInt32(wartosci);
	statsPrzeciwnik.Rozne = Convert.ToInt32(wartosciPrzeciwnik);
}
if(typ=="Spalone"){
	statsOdlew.spalone= Convert.ToInt32(wartosci);
	statsPrzeciwnik.spalone = Convert.ToInt32(wartosciPrzeciwnik);
}
if(typ=="Słupki/poprzeczki"){
	statsOdlew.Aluminium= Convert.ToInt32(wartosci);
	statsPrzeciwnik.Aluminium = Convert.ToInt32(wartosciPrzeciwnik);
}
if(typ=="Kartki żółte"){
	statsOdlew.zolteKartki= Convert.ToInt32(wartosci);
	statsPrzeciwnik.zolteKartki = Convert.ToInt32(wartosciPrzeciwnik);
}
if(typ=="Kartki czerwone"){
	statsOdlew.czerwoneKartki= Convert.ToInt32(wartosci);
	statsPrzeciwnik.czerwoneKartki = Convert.ToInt32(wartosciPrzeciwnik);
}



	
			
		if (typ == "Faule")
		{
			List<string> lista2 = new List<string>();
			
			lista2 = wartosciFaulowani.Split(',').ToList();
			lista2.ForEach(x =>
			{
				listaZawodnikow.ForEach(y => {
					if (x.Trim().IndexOf(y.nazwisko) == 0)
					{
						try
						{
							y.faulowany = Convert.ToInt32(x.Trim().Substring(y.nazwisko.Length));
						}
						catch (Exception e)
						{
							MessageBox.Show("Dla wartości " + x + " nie możliwa identyfikacja zawodnika", "Niespójne dane", MessageBoxButtons.OK, MessageBoxIcon.Error);
							err = true;
						}
					}


				});

			});
		}




		if (wartosciOdlew.IndexOf("(") != -1)
		{
			wartosciOdlew = wartosciOdlew.Substring(wartosciOdlew.IndexOf("(") + 1);
			wartosciOdlew = wartosciOdlew.Substring(0, wartosciOdlew.IndexOf(")"));
			List<string> lista = wartosciOdlew.Split(',').ToList();




			lista.ForEach(x =>
			{
				listaZawodnikow.ForEach(y =>
				{
					if (x.Trim().IndexOf(y.nazwisko) == 0)
					{
						try
						{
						
							if (typ == "Strzały")
							{
								y.strzaly = Convert.ToInt32(x.Trim().Substring(y.nazwisko.Length));
							}
							if (typ == "Strzały celne")
							{
								y.strzalyCelne = Convert.ToInt32(x.Trim().Substring(y.nazwisko.Length));
							}
							if (typ == "Faule")
							{
								y.faule = Convert.ToInt32(x.Trim().Substring(y.nazwisko.Length));
							
							}
							if (typ == "Rogi")
							{
								y.rogi = Convert.ToInt32(x.Trim().Substring(y.nazwisko.Length));
							}
							if (typ == "Spalone")
							{
								y.spalone = Convert.ToInt32(x.Trim().Substring(y.nazwisko.Length));
							}
							if (typ == "Słupki/poprzeczki")
							{
								y.aluminium = Convert.ToInt32(x.Trim().Substring(y.nazwisko.Length));
							}
							if (typ == "Kartki żółte")
							{
								y.zolteKartki = Convert.ToInt32(x.Trim().Substring(y.nazwisko.Length));
							}
							if (typ == "Kartki czerwone")
							{
								y.czerwoneKartki = Convert.ToInt32(x.Trim().Substring(y.nazwisko.Length));
							}
							if (typ == "Asysty")
							{
								y.asysty = Convert.ToInt32(x.Trim().Substring(y.nazwisko.Length));
							}
							if (typ == "Gole")
							{
								y.gol = Convert.ToInt32(x.Trim().Substring(y.nazwisko.Length));
							}

						}
						catch (Exception e)
						{
							MessageBox.Show("Dla wartości " + x + " nie możliwa identyfikacja zawodnika", "Niespójne dane", MessageBoxButtons.OK, MessageBoxIcon.Error);
							err = true;
						}
					}
				});
			});
		}

		//strzalyOdlew = strzalyOdlew.Substring(0,strzalyOdlew.IndexOf(")"));
		//List <string> lista = strzalyOdlew.Split(',').ToList();
		//listaZawodnikow.ForEach(y=>{
		//if(x.Trim().IndexOf(y.nazwisko)==0){
		//try{
		//y.strzaly=Convert.ToInt32(x.Trim().Substring(y.nazwisko.Length));
		//}catch(Exception e)
		//{
		//MessageBox.Show("Dla wartości " + x + " nie możliwa identyfikacja zawodnika", "Niespójne dane", MessageBoxButtons.OK,MessageBoxIcon.Error);
		//err=true;
		//}
		//}
		//});
		//});
				}


		public async static Task<List<Zawodnik>> pobieranieListyZawodnikow(TextBox box){
return await Task.Run(()=>{
	if (box != null)
	{
		box.Text = "Pobieranie danych...";
	}
List<Zawodnik>lista = new List<Zawodnik>();
HttpClientHandler handler = new HttpClientHandler();
IWebProxy proxy = WebRequest.GetSystemWebProxy();
proxy.Credentials = CredentialCache.DefaultCredentials;
handler.Proxy=proxy;
ServicePointManager.SecurityProtocol=SecurityProtocolType.Tls12;
HttpClient client = new HttpClient(handler);
client.BaseAddress= new Uri("http://www.odlew.poznan.pl/system/players/index/page:1");
var result = client.GetAsync("").Result;
int counter=0;
string data = result.Content.ReadAsStringAsync().Result;
IHTMLDocument2 doc = new HTMLDocumentClass();
doc.write(new object[] { data });

Mecz mecz =null;
IHTMLElementCollection elements = ((IHTMLDocument3) doc).getElementsByTagName("b");
string iloscStron=((IHTMLElement) elements.item(0)).innerText;
iloscStron =  iloscStron.Substring(23,iloscStron.IndexOf("Wyświetlanie rekordów")-23);
iloscStron=iloscStron.Replace(".","").Trim();


for(int i=1;i<=Convert.ToInt32(iloscStron);i++)
{
counter=0;
if(i>1){
client = new HttpClient(handler);
client.BaseAddress= new Uri("http://www.odlew.poznan.pl/system/players/index/page:" + i);
result = client.GetAsync("").Result;
data = result.Content.ReadAsStringAsync().Result;
doc = new HTMLDocumentClass();
doc.write(new object[] { data });
}
elements = ((IHTMLDocument3) doc).getElementsByTagName("table");
IHTMLElement element = (IHTMLElement) ((IHTMLElementCollection) elements).item(1);
	
	elements = ((IHTMLElement2)element).getElementsByTagName("tr");
	foreach(IHTMLElement x in elements){
	if(counter>0){

string nazwa ="";
string id="";
				string ksywka = "";
				string pelnaNazwa = "";
				pelnaNazwa= nazwa = (string)((IHTMLElement)((IHTMLElementCollection)((IHTMLElement2)x).getElementsByTagName("td")).item(1)).innerText + " " + (string)((IHTMLElement)((IHTMLElementCollection)((IHTMLElement2)x).getElementsByTagName("td")).item(2)).innerText;
				if (((IHTMLElement) ((IHTMLElementCollection)((IHTMLElement2)x).getElementsByTagName("td")).item(4)).innerText==null){
	nazwa = (string)((IHTMLElement) ((IHTMLElementCollection)((IHTMLElement2)x).getElementsByTagName("td")).item(1)).innerText + " " + (string)((IHTMLElement) ((IHTMLElementCollection)((IHTMLElement2)x).getElementsByTagName("td")).item(2)).innerText;

}else{
	nazwa= (string)((IHTMLElement) ((IHTMLElementCollection)((IHTMLElement2)x).getElementsByTagName("td")).item(4)).innerText;
					ksywka = nazwa;
}

		IHTMLElement link = (IHTMLElement) ((IHTMLElementCollection)((IHTMLElement2)x).getElementsByTagName("a")).item(0);
		if(link !=null){
		
		id=(string)	((IHTMLElement)link).getAttribute("href");


		id=id.Substring(id.IndexOf("stats/")+6);
if(id.IndexOf("?")>0){
		id=id.Substring(0,id.IndexOf("?"));
}

		}
Zawodnik zawodnik = new Zawodnik ();
zawodnik.nazwisko = nazwa;
				zawodnik.ksywka = ksywka;
				zawodnik.pelnaNazwa = pelnaNazwa;
zawodnik.identyfikator=Convert.ToInt32(id);
lista.Add(zawodnik);

	}	
	counter++;
	}

}


return lista;
});


}
public async static Task<List<Mecz>> pobieranieMeczow(){

return await Task.Run(()=>{
HttpClientHandler handler = new HttpClientHandler();
IWebProxy proxy = WebRequest.GetSystemWebProxy();
proxy.Credentials = CredentialCache.DefaultCredentials;
handler.Proxy=proxy;
ServicePointManager.SecurityProtocol=SecurityProtocolType.Tls12;
HttpClient client = new HttpClient(handler);
client.BaseAddress= new Uri("http://www.odlew.poznan.pl/system/matches/prev");
var result = client.GetAsync("").Result;
string data = result.Content.ReadAsStringAsync().Result;
IHTMLDocument2 doc = new HTMLDocumentClass();
doc.write(new object[] { data });
List<Mecz>lista = new List<Mecz>();
int counter =0;
Mecz mecz =null;
IHTMLElementCollection elements = ((IHTMLDocument3) doc).getElementsByTagName("table");
IHTMLElement element = (IHTMLElement) ((IHTMLElementCollection) elements).item(1);
	
	elements = ((IHTMLElement2)element).getElementsByTagName("tr");
	foreach(IHTMLElement x in elements){
	if(counter>0){
		 mecz = new Mecz();
		mecz.data = (string)((IHTMLElement) ((IHTMLElementCollection)((IHTMLElement2)x).getElementsByTagName("td")).item(0)).innerText;
		mecz.przeciwnik = (string)((IHTMLElement) ((IHTMLElementCollection)((IHTMLElement2)x).getElementsByTagName("td")).item(1)).innerText;
		mecz.sezon = (string)((IHTMLElement) ((IHTMLElementCollection)((IHTMLElement2)x).getElementsByTagName("td")).item(3)).innerText;
		IHTMLElement link = (IHTMLElement) ((IHTMLElementCollection)((IHTMLElement2)x).getElementsByTagName("a")).item(0);
			if ((string)((IHTMLElement)((IHTMLElementCollection)((IHTMLElement2)x).getElementsByTagName("td")).item(1)).innerText == "U siebie")
			{
				mecz.uSiebie =  "1";

			}
			else {
				mecz.uSiebie = "0";
			}
		if(link !=null){
		mecz.link=	(string)	((IHTMLElement)link).getAttribute("href");
			mecz.id= Convert.ToInt64(mecz.link.Substring(mecz.link.IndexOf("stats/")+6,mecz.link.IndexOf("?")- mecz.link.IndexOf("stats/") -6));
		}
	lista.Add(mecz);
	}	
	counter++;
	}
return lista;
});
}
public async static Task<string> tworzenieRaportu(string adres){

return await Task.Run(()=>{
	bool gospodarzOldew = false;
HttpClientHandler handler = new HttpClientHandler();
IWebProxy proxy = WebRequest.GetSystemWebProxy();
proxy.Credentials = CredentialCache.DefaultCredentials;
handler.Proxy=proxy;
ServicePointManager.SecurityProtocol=SecurityProtocolType.Tls12;
HttpClient client = new HttpClient(handler);
client.BaseAddress= new Uri("http://www.odlew.poznan.pl/" + adres);
var result = client.GetAsync("").Result;
string data = result.Content.ReadAsStringAsync().Result;
IHTMLDocument2 doc = new HTMLDocumentClass();
doc.write(new object[] { data });
IHTMLElementCollection elements = ((IHTMLDocument7) doc).getElementsByClassName("table-text-header");
	IHTMLElement wynikDoPrzerwy = (IHTMLElement)((IHTMLElementCollection)((IHTMLDocument7)doc).getElementsByClassName("entry-content")).item(0);
	
	wynikDoPrzerwy= (IHTMLElement)((IHTMLElementCollection)((IHTMLElement2) wynikDoPrzerwy).getElementsByTagName("center")).item(1);
	//wynikDoPrzerwy = (IHTMLElement)((IHTMLElementCollection)((IHTMLElement2)wynikDoPrzerwy).getElementsByTagName("h2")).item(0);
	string tekstZWynikiemDoPrzerwy = wynikDoPrzerwy.innerText;
	int wynikDoPrzerwyOdlew = 0;
	int wynikDoPrzerwyPrzeciwnik = 0;
	if (tekstZWynikiemDoPrzerwy.IndexOf("HKS Odlew Poznań") != -1)
	{
	

		if (tekstZWynikiemDoPrzerwy.LastIndexOf("(") != -1 && tekstZWynikiemDoPrzerwy.LastIndexOf(")") != -1)
		{

			tekstZWynikiemDoPrzerwy = tekstZWynikiemDoPrzerwy.Substring(tekstZWynikiemDoPrzerwy.LastIndexOf("(") + 1, tekstZWynikiemDoPrzerwy.LastIndexOf(")") - tekstZWynikiemDoPrzerwy.LastIndexOf("(") - 1);
		}
		

		List<string> listaZWynikami = tekstZWynikiemDoPrzerwy.Split(':').ToList();

		try
		{
			if (wynikDoPrzerwy.innerText.IndexOf("HKS Odlew Poznań") > wynikDoPrzerwy.innerText.IndexOf("-"))
			{

				wynikDoPrzerwyOdlew = Convert.ToInt32(listaZWynikami[1]);
				wynikDoPrzerwyPrzeciwnik = Convert.ToInt32(listaZWynikami[0]);

			}
			else {
				gospodarzOldew = true;
				wynikDoPrzerwyOdlew = Convert.ToInt32(listaZWynikami[0]);
				wynikDoPrzerwyPrzeciwnik = Convert.ToInt32(listaZWynikami[1]);

			}
		}
		catch (Exception e) { 
		
		
		}
		

	}


	IHTMLElement element = null;
foreach(IHTMLElement x in elements){

	if(x.innerText=="Statystyki szczegółowe"){element = x;}
}
element = (IHTMLElement) ((IHTMLDOMNode)element).nextSibling;
IHTMLElementCollection naglowki = ((IHTMLElement2) element).getElementsByTagName("th");

IHTMLElementCollection wiersze =  ((IHTMLElement2) element).getElementsByTagName("td");
int counter =0;
int counter_=0;
List<Statystyka> lista = new List<Statystyka>();
foreach(IHTMLElement x in naglowki)
{
	Statystyka statystyka = new Statystyka ();
	statystyka.nazwa = x.innerText;
	lista.Add(statystyka);
}
List<string> wartosci = new List<string>();
foreach(IHTMLElement x in wiersze){

if (x.innerText!=null){
wartosci =   x.innerHTML.Split(new string[] {"</B>"},StringSplitOptions.None).ToList();


IHTMLElementCollection wierszeZDanymi =  (IHTMLElementCollection) ((IHTMLElement2) x).getElementsByTagName("a");
foreach(IHTMLElement y in wierszeZDanymi){
counter_++;

Zawodnik zawodnik = new Zawodnik ();
zawodnik.id = (string) y.getAttribute("href");
zawodnik.nazwisko = y.innerText;
string wartosc = "";  

if (wartosci[counter_].IndexOf("<BR>")>0){
	if(wartosci[counter_].IndexOf("title=Bramka")==-1 && wartosci[counter_].IndexOf("title=\"Żółta kartka\"")==-1  && wartosci[counter_].IndexOf("title=\"Czerwona kartka\"")==-1){
wartosc=wartosci[counter_].Substring(1,wartosci[counter_].IndexOf("<BR>")-1).Replace("-","");
zawodnik.wartosc = Convert.ToInt32(wartosc);
	}
if(wartosci[counter_].IndexOf("title=Bramka")!=-1 && wartosci[counter_].IndexOf("title=\"Żółta kartka\"")==-1  && wartosci[counter_].IndexOf("title=\"Czerwona kartka\"")==-1){
wartosc= wartosci[counter_].Substring(1,wartosci[counter_].IndexOf("<IMG")-1).Replace("-","");
zawodnik.wartosc =Convert.ToInt32( wartosc);
int iloscGoli = wartosci[counter_].Split(new [] {"<IMG"},StringSplitOptions.None).Length-1;
zawodnik.gol = iloscGoli;
	}
if(wartosci[counter_].IndexOf("title=Bramka")==-1 && wartosci[counter_].IndexOf("title=\"Żółta kartka\"")!=-1){

int iloscKartekZoltych = wartosci[counter_].Split(new [] {"IMG title=\"Żółta kartka\""},StringSplitOptions.None).Length-1;
zawodnik.wartosc = iloscKartekZoltych;


}
					if (wartosci[counter_].IndexOf("title=\"Czerwona kartka\"") != -1)
					{

						zawodnik.czerwonaKartka = 1;
					}
				}
lista[counter].listaStatystyk.Add(zawodnik);

}
counter_=0;

}

counter++;
}


foreach(IHTMLElement x in elements){
	
	if(x.innerText=="Statystyki"){element = x;}
	
}
element = (IHTMLElement) ((IHTMLDOMNode)element).nextSibling;
IHTMLElement tabela = (IHTMLElement) ((IHTMLElementCollection) ((IHTMLElement2) element).getElementsByTagName("table")).item(0);
StatystykaZespolowa gospodarze = new StatystykaZespolowa();
gospodarze.nazwaZespolu = ((IHTMLElement) ((IHTMLElementCollection) ((IHTMLElement2) element).getElementsByTagName("center")).item(0)).innerText;
StatystykaZespolowa goscie = new StatystykaZespolowa();
goscie.nazwaZespolu = ((IHTMLElement) ((IHTMLElementCollection) ((IHTMLElement2) element).getElementsByTagName("center")).item(1)).innerText;
IHTMLElementCollection wierszeTabeli =  (IHTMLElementCollection) ((IHTMLElement2) element).getElementsByTagName("tr");
foreach(IHTMLElement x in wierszeTabeli)
{
	
	IHTMLElementCollection dane = (IHTMLElementCollection) ((IHTMLElement2) x).getElementsByTagName("td");
	if(dane.length==3){
	string kategoria = (string) ((IHTMLElement) ((IHTMLElementCollection) ((IHTMLElement) dane.item(1)).children).item(0)).getAttribute("title");
	string daneGospodarze =  ((IHTMLElement) ((IHTMLElementCollection) dane).item(0)).innerText;
	string daneGoscie =  ((IHTMLElement) ((IHTMLElementCollection) dane).item(2)).innerText;
	if(kategoria=="Bramki"){goscie.gole=Convert.ToInt32(daneGoscie);gospodarze.gole=Convert.ToInt32(daneGospodarze);}
	if(kategoria=="Strzały (celne)"){

goscie.strzaly = Convert.ToInt32(daneGoscie.Substring(0,daneGoscie.IndexOf("(")-1));
gospodarze.strzaly = Convert.ToInt32(daneGospodarze.Substring(0,daneGospodarze.IndexOf("(")-1));
goscie.strzalyCelne = Convert.ToInt32(daneGoscie.Substring(daneGoscie.IndexOf("(")+1,1));
gospodarze.strzalyCelne =Convert.ToInt32(daneGospodarze.Substring(daneGospodarze.IndexOf("(")+1,1));
					}
	if(kategoria=="Słupki/poprzeczki"){goscie.Aluminium=Convert.ToInt32(daneGoscie);gospodarze.Aluminium=Convert.ToInt32(daneGospodarze);}
	if(kategoria=="Faule"){goscie.Faule=Convert.ToInt32(daneGoscie);gospodarze.Faule=Convert.ToInt32(daneGospodarze);}
			if (kategoria == "Faulowani") { gospodarze.Faulowani = Convert.ToInt32(daneGospodarze); }
			if (kategoria=="Rożne"){goscie.Rozne=Convert.ToInt32(daneGoscie);gospodarze.Rozne=Convert.ToInt32(daneGospodarze);}
	if(kategoria=="Kartki żółte/czerwone"){

goscie.zolteKartki = Convert.ToInt32(daneGoscie.Substring(0,daneGoscie.IndexOf(@"/")));
gospodarze.zolteKartki = Convert.ToInt32(daneGospodarze.Substring(0,daneGospodarze.IndexOf(@"/")));

goscie.czerwoneKartki = Convert.ToInt32(daneGoscie.Substring(daneGoscie.IndexOf(@"/")+1,1));
gospodarze.czerwoneKartki =Convert.ToInt32(daneGospodarze.Substring(daneGoscie.IndexOf(@"/")+1,1));
					}
	if(kategoria=="Spalone"){goscie.spalone=Convert.ToInt32(daneGoscie);gospodarze.spalone=Convert.ToInt32(daneGospodarze);}

		}
	

}

String raport="";
StatystykaZespolowa odlew = new StatystykaZespolowa();
StatystykaZespolowa przeciwnik = new StatystykaZespolowa();
if(gospodarze.nazwaZespolu.Contains("Odlew")){

	 odlew  = gospodarze;
	 przeciwnik = goscie;
}else{

	 odlew  = goscie;
	 przeciwnik = gospodarze;	

}
	odlew.goleDoPrzerwy = wynikDoPrzerwyOdlew;
	przeciwnik.goleDoPrzerwy = wynikDoPrzerwyPrzeciwnik;
	string sklad = "";

	elements = ((IHTMLDocument3)doc).getElementsByTagName("center");
    foreach (IHTMLElement x in elements)
    {

		if (x.innerText != null)
		{
			if (x.innerText.IndexOf("Skład Odlewu:") != -1)
			{
				sklad = x.innerText.Substring(x.innerText.IndexOf("Skład Odlewu:") + 13);
	

			}
		}
    }
	if (gospodarzOldew) {
		gospodarze = odlew;
		goscie = przeciwnik;
	}
	else {

		gospodarze = przeciwnik;
		goscie = odlew;
	}
    //lista.ForEach(x=>{Console.WriteLine(x.nazwa);x.listaStatystyk.ForEach(y=>{Console.WriteLine(y.nazwisko + " " + y.wartosc);});});
    StringBuilder sb = new StringBuilder();
	sb.AppendLine(gospodarze.nazwaZespolu + " - " + goscie.nazwaZespolu + "  " + gospodarze.gole + ":" + goscie.gole + " (" + gospodarze.goleDoPrzerwy + ":" + goscie.goleDoPrzerwy + ")"  );
	sb.AppendLine("");
	sb.AppendLine("Skład Odlewu:");
	sb.AppendLine("");
	sb.AppendLine(sklad);
	sb.AppendLine("");
	sb.AppendLine("Strzały");
sb.AppendLine("");
if (odlew.strzaly>0){
sb.Append(odlew.nazwaZespolu + " "  + odlew.strzaly);
raport=" (" + lista.Aggregate("",(acc,x)=>{if(x.nazwa=="Strzały"){x.listaStatystyk.ForEach(y=>acc+=y.nazwisko + " " + y.wartosc + " , ");};return acc;})+ " )";
raport=raport.Substring(0,raport.Length-5) + ")";
sb.AppendLine(raport);
}else{

sb.AppendLine(odlew.nazwaZespolu + " "  + odlew.strzaly);
}
sb.AppendLine(przeciwnik.nazwaZespolu + " " + przeciwnik.strzaly);
sb.AppendLine("");
sb.AppendLine("Strzały celne");
sb.AppendLine("");
if (odlew.strzalyCelne>0){

raport=" (" + lista.Aggregate("",(acc,x)=>{if(x.nazwa=="Strzały celne"){x.listaStatystyk.ForEach(y=>acc+=y.nazwisko + " " + y.wartosc + " , ");};return acc;})+ " )";
if(raport.Length>5){
sb.Append(odlew.nazwaZespolu + " " + odlew.strzalyCelne);
raport=raport.Substring(0,raport.Length-5) + ")";
}
sb.AppendLine(raport);
}else{

sb.AppendLine(odlew.nazwaZespolu + " "  + odlew.strzalyCelne);
}

sb.AppendLine(przeciwnik.nazwaZespolu + " " + przeciwnik.strzalyCelne);
sb.AppendLine("");
sb.AppendLine("Faule");
sb.AppendLine("");
if (odlew.Faule>0){
sb.Append(odlew.nazwaZespolu + " " + odlew.Faule);
raport=" (" + lista.Aggregate("",(acc,x)=>{if(x.nazwa=="Faule"){x.listaStatystyk.ForEach(y=>acc+=y.nazwisko + " " + y.wartosc + " , ");};return acc;})+ " )";
if(raport.Length>5){
raport=raport.Substring(0,raport.Length-5) + ")";
}
sb.AppendLine(raport);
}else
{
sb.AppendLine(odlew.nazwaZespolu + " " + odlew.Faule);
}
	if (przeciwnik.Faule > 0)
	{
		sb.Append(przeciwnik.nazwaZespolu + " " + przeciwnik.Faule);
		raport = " (" + lista.Aggregate("", (acc, x) => { if (x.nazwa == "Faulowani") { x.listaStatystyk.ForEach(y => acc += y.nazwisko + " " + y.wartosc + " , "); }; return acc; }) + " )";
		if (raport.Length > 5)
		{
			raport = raport.Substring(0, raport.Length - 5) + ")";
		}
		sb.AppendLine(raport);
	}
	else {
		sb.AppendLine(przeciwnik.nazwaZespolu + " " + przeciwnik.Faule);
	}

		sb.AppendLine("");
sb.AppendLine("Rogi");
sb.AppendLine("");
sb.AppendLine(odlew.nazwaZespolu + " " + odlew.Rozne);
sb.AppendLine(przeciwnik.nazwaZespolu + " " + przeciwnik.Rozne);
sb.AppendLine("");
sb.AppendLine("Spalone");
sb.AppendLine("");
sb.AppendLine(odlew.nazwaZespolu + " " + odlew.spalone);
sb.AppendLine(przeciwnik.nazwaZespolu + " " + przeciwnik.spalone);
sb.AppendLine("");
sb.AppendLine("Słupki/poprzeczki");
sb.AppendLine("");

if (odlew.Aluminium>0){
sb.Append(odlew.nazwaZespolu + " " + odlew.Aluminium);
raport=" (" + lista.Aggregate("",(acc,x)=>{if(x.nazwa=="Słupki/poprzeczki"){x.listaStatystyk.ForEach(y=>acc+=y.nazwisko + " " + y.wartosc + " , ");};return acc;})+ " )";
if(raport.Length>5){
raport=raport.Substring(0,raport.Length-5) + ")";
}
sb.AppendLine(raport);
}else{
sb.AppendLine(odlew.nazwaZespolu + " " + odlew.Aluminium);
}
sb.AppendLine(przeciwnik.nazwaZespolu + " " + przeciwnik.Aluminium);
sb.AppendLine("");
sb.AppendLine("Kartki żółte");
sb.AppendLine("");
if (odlew.zolteKartki>0){
sb.Append(odlew.nazwaZespolu + " " + odlew.zolteKartki);
raport=" (" + lista.Aggregate("",(acc,x)=>{ if (x.nazwa == "Kartki żółte/czerwone") { x.listaStatystyk.ForEach(y =>{

	if (y.wartosc > 0)
	{

		acc += y.nazwisko + " " + y.wartosc + " , ";
	}
}


);};return acc;})+ " )";
if(raport.Length>5){
raport=raport.Substring(0,raport.Length-5) + ")";
}
sb.AppendLine(raport);
}else{
sb.AppendLine(odlew.nazwaZespolu + " " + odlew.zolteKartki);
}
sb.AppendLine(przeciwnik.nazwaZespolu + " " + przeciwnik.zolteKartki);
sb.AppendLine("");
sb.AppendLine("Kartki czerwone");
sb.AppendLine("");
if (odlew.czerwoneKartki>0){
sb.Append(odlew.nazwaZespolu + " " + odlew.czerwoneKartki);
raport=" (" + lista.Aggregate("",(acc,x)=>{if(x.nazwa=="Kartki żółte/czerwone"){x.listaStatystyk.ForEach(y=> {if(y.czerwonaKartka>0){acc+=y.nazwisko + " " + y.czerwonaKartka + " , ";}}
);};return acc;})+ " )" ;
if(raport.Length>5){
raport=raport.Substring(0,raport.Length-5) + ")";
}
sb.AppendLine(raport);
}
else{
sb.AppendLine(odlew.nazwaZespolu + " " + odlew.czerwoneKartki);
}
sb.AppendLine(przeciwnik.nazwaZespolu + " " + przeciwnik.czerwoneKartki);


		sb.AppendLine("");
		sb.AppendLine("Asysty");
		sb.AppendLine("");
	sb.Append(odlew.nazwaZespolu + " " + lista.Where(x => x.nazwa == "Asysty").Select(x => x.listaStatystyk.Sum(y => y.wartosc) ).FirstOrDefault());
	if (lista.Where(x => x.nazwa == "Asysty").Select(x => x.listaStatystyk.Sum(y => y.wartosc)).FirstOrDefault() > 0)
	{

		raport = " (" + lista.Aggregate("", (acc, x) =>
		{
			if (x.nazwa == "Asysty")
			{
				x.listaStatystyk.ForEach(y => { acc += y.nazwisko + " " + y.wartosc + " , "; }
);
			}; return acc;
		}) + " )";
		if (raport.Length > 5)
		{
			raport = raport.Substring(0, raport.Length - 5) + ")";
		}
		sb.AppendLine(raport);
	}
	sb.AppendLine("");
	sb.AppendLine("Gole");
	sb.AppendLine("");
	sb.Append(odlew.nazwaZespolu + " " + odlew.gole);

	if (lista.Where(x => x.nazwa == "Strzały").Select(x => x.listaStatystyk.Sum(y => y.gol)).FirstOrDefault() > 0)
	{

		raport = " (" + lista.Aggregate("", (acc, x) =>
		{
			if (x.nazwa == "Strzały")
			{
				x.listaStatystyk.ForEach(y => { if (y.gol != 0) { acc += y.nazwisko + " " + y.gol + " , "; } }
);
			}; return acc;
		}) + " )";
		if (raport.Length > 5)
		{
			raport = raport.Substring(0, raport.Length - 5) + ")";
		}
		sb.Append(raport);
	}
	sb.AppendLine("");
	sb.Append(przeciwnik.nazwaZespolu + " " + przeciwnik.gole);
	//lista.Where(x => x.nazwa == "Asysty").ToList().ForEach(x => Console.WriteLine(x.listaStatystyk.Count));




	doc.close();
return sb.ToString();});

}

}

public class Statystyka {
public string nazwa;
public List<Zawodnik> listaStatystyk;
public Statystyka (){listaStatystyk=new List<Zawodnik>();}
}
public class StatystykaZespolowa {
	public string nazwaZespolu;
	public int gole;
	public int strzalyCelne;
	public int strzaly;
	public int Aluminium;
	public int Faule;
	public int Rozne;
	public int czerwoneKartki;
	public int zolteKartki;
	public int spalone;
	public int asysty;
	public int Faulowani;
	public int goleDoPrzerwy;
	public bool czyGospodarze;

}
public class Zawodnik{
	public string pelnaNazwa;
	public int czasGry;
public string nazwisko;
public string id;
public int identyfikator;
public string ksywka;
public int wartosc;
public int czerwonaKartka;
public int gol;
public int gole;
public int strzalyCelne;
public int zolteKartki;
public int czerwoneKartki;
public int strzaly;
public int faule;
public int faulowany;
public int spalone;
	public int rogi;
	public int aluminium;
	public bool czyGral;
	public bool pierwszySklad;
	public bool bramkarz;
	public int asysty;
}
public class Mecz{
public string przeciwnik;
public string data;
public long id;
public string link;
public string sezon;
	public string uSiebie;
}
[Serializable]
public class Sesja
{
	public string numer;
}