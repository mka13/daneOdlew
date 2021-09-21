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
public class ehks {
public static List<Mecz> listaMeczow;
[DllImport("kernel32.dll")]
static extern IntPtr GetConsoleWindow();

[DllImport("user32.dll")]
static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

const int SW_HIDE = 0;
const int SW_SHOW = 5;
public static void Main (string [] args){
var handle = GetConsoleWindow();
ShowWindow(handle, SW_HIDE);



Form form = new Form();
form.BackColor=Color.Gray;
Button button1 =new Button();
button1.Size = new Size(100,50);
button1.Location = new System.Drawing.Point(410,10);
button1.Text ="Odśwież";
ListBox listBox1 = new ListBox();
button1.BackColor=Color.Black;
button1.ForeColor=Color.White;
TextBox box = new TextBox();
box.ScrollBars = ScrollBars.Vertical;
Button button2 =new Button();
button2.Click += ((a,b)=>{
string curItem = listBox1.SelectedItem.ToString();
string adres = (string) listaMeczow.Where(x=>(x.przeciwnik + " " + x.data) == curItem).Select(x=>x.link).FirstOrDefault();
adres = adres.Substring(0,adres.IndexOf("?"));
adres=adres.Substring(7);
box.Text="";
Task task = tworzenieRaportu(adres).ContinueWith(x=>{
box.Text= (string) x.Result;});
});

box.Size = new Size(780,450);
box.Multiline=true;
box.Location = new Point(10,310);
form.Controls.Add(box);


button2.Size = new Size(100,50);
button2.Location = new System.Drawing.Point(410,70);
button2.Text ="Generuj tekst";
button2.BackColor=Color.Black;
button2.ForeColor=Color.White;






form.Controls.Add(button2);
form.Size = new System.Drawing.Size(850,850);


 listBox1.BackColor=Color.Black;
listBox1.ForeColor=Color.White;
   listBox1.Size = new System.Drawing.Size(300, 300);
   listBox1.Location = new System.Drawing.Point(10,10);
   listBox1.MultiColumn = true;
	form.Controls.Add(listBox1);


button1.Click += ((a,b)=>{Task task = pobieranieMeczow().ContinueWith(x=>
{


List<Mecz>lista = (List<Mecz>) x.Result;
listaMeczow= new List<Mecz>(lista);
	listBox1.Items.Clear();
	foreach(Mecz mecz in lista){
		listBox1.Items.Add(mecz.przeciwnik + " " + mecz.data);
	}

});});

form.Controls.Add(button1);


	










form.ShowDialog();

























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
		if(link !=null){
		mecz.link=	(string)	((IHTMLElement)link).getAttribute("href");
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
if(wartosci[counter_].IndexOf("title=\"Czerwona kartka\"")!=-1){

zawodnik.czerwonaKartka = 1;
	}

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
	if(kategoria=="Rożne"){goscie.Rozne=Convert.ToInt32(daneGoscie);gospodarze.Rozne=Convert.ToInt32(daneGospodarze);}
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
//lista.ForEach(x=>{Console.WriteLine(x.nazwa);x.listaStatystyk.ForEach(y=>{Console.WriteLine(y.nazwisko + " " + y.wartosc);});});
StringBuilder sb = new StringBuilder();
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

sb.AppendLine(przeciwnik.nazwaZespolu + " " + przeciwnik.Faule);
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
raport=" (" + lista.Aggregate("",(acc,x)=>{if(x.nazwa=="Kartki żółte/czerwone"){x.listaStatystyk.ForEach(y=>acc+=y.nazwisko + " " + y.wartosc + " , ");};return acc;})+ " )";
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
	

}
public class Zawodnik{
public string nazwisko;
public string id;
public int wartosc;
public int czerwonaKartka;
public int gol;
}
public class Mecz{
public string przeciwnik;
public string data;
public long id;
public string link;
public string sezon;
}