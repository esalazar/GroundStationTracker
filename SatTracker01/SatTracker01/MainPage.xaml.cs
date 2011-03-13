using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SatTracker01.SatelliteSituationCenterService;
using Microsoft.Maps.MapControl;
using System.Windows.Media.Imaging;
using SatTracker01.GeocodeService;
using System.IO;
using System.Globalization;

namespace SatTracker01
{
    public partial class MainPage : UserControl
    {
        // Because the NASA service doesn't accept Silverlight clients we need to statically retrieve the ground stations
        private bool NASAService = false;

        // Creates an object that will allow us to gather satellite data
        private SatelliteSituationCenterInterfaceClient ssc;

        // Because the Bing Maps geocoding service doesn't return ISO codes to retrieve the proper image
        // we must use a dictionary to statically store a mapping of Country name to ISO code
        private Dictionary<string, string> countryMap = new Dictionary<string, string>();

        // Bing Maps Key
        private string BingMapsKey = "AozlO9eznK42ZibXSSwYG5xlu8FAs2IdG80kGSqvPP2sqNuUzn0HRQ1OiFrs_Atw";

        /// <summary>
        /// Main Method needed for initialization
        /// </summary>
        public MainPage()
        {

            // Required for silverlight applications
            this.InitializeComponent();

            // Initialize satellite application's global variables
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        /// <summary>
        /// Function that calls all methods needed for initialization. This should be called after
        /// the main page has loaded.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e events</param>
        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Create an instance of the Web service class
            this.ssc = new SatelliteSituationCenterInterfaceClient();

            // Makes the hash from standard name to iso country code from the images list
            this.getFileDictionary();

            // Retrieve all of the ground stations from the service
            this.getAllGroundStations();
        }

        /// <summary>
        /// Will create the Map of country names to ISO codes
        /// </summary>
        private void getFileDictionary()
        {
            // Statically placed all country with iso codes using a simple python script i developed
            string totalCountry = "AFGHANISTAN;AF|ÅLAND ISLANDS;AX|ALBANIA;AL|ALGERIA;DZ|AMERICAN SAMOA;AS|ANDORRA;AD|ANGOLA;AO|ANGUILLA;AI|ANTARCTICA;AQ|ANTIGUA AND BARBUDA;AG|ARGENTINA;AR|ARMENIA;AM|ARUBA;AW|AUSTRALIA;AU|AUSTRIA;AT|AZERBAIJAN;AZ|BAHAMAS;BS|BAHRAIN;BH|BANGLADESH;BD|BARBADOS;BB|BELARUS;BY|BELGIUM;BE|BELIZE;BZ|BENIN;BJ|BERMUDA;BM|BHUTAN;BT|BOLIVIA, PLURINATIONAL STATE OF;BO|BONAIRE, SAINT EUSTATIUS AND SABA;BQ|BOSNIA AND HERZEGOVINA;BA|BOTSWANA;BW|BOUVET ISLAND;BV|BRAZIL;BR|BRITISH INDIAN OCEAN TERRITORY;IO|BRUNEI DARUSSALAM;BN|BULGARIA;BG|BURKINA FASO;BF|BURUNDI;BI|CAMBODIA;KH|CAMEROON;CM|CANADA;CA|CAPE VERDE;CV|CAYMAN ISLANDS;KY|CENTRAL AFRICAN REPUBLIC;CF|CHAD;TD|CHILE;CL|CHINA;CN|CHRISTMAS ISLAND;CX|COCOS (KEELING) ISLANDS;CC|COLOMBIA;CO|COMOROS;KM|CONGO;CG|CONGO, THE DEMOCRATIC REPUBLIC OF THE;CD|COOK ISLANDS;CK|COSTA RICA;CR|CÔTE D'IVOIRE;CI|CROATIA;HR|CUBA;CU|CURAÇAO;CW|CYPRUS;CY|CZECH REPUBLIC;CZ|DENMARK;DK|DJIBOUTI;DJ|DOMINICA;DM|DOMINICAN REPUBLIC;DO|ECUADOR;EC|EGYPT;EG|EL SALVADOR;SV|EQUATORIAL GUINEA;GQ|ERITREA;ER|ESTONIA;EE|ETHIOPIA;ET|FALKLAND ISLANDS (MALVINAS);FK|FAROE ISLANDS;FO|FIJI;FJ|FINLAND;FI|FRANCE;FR|FRENCH GUIANA;GF|FRENCH POLYNESIA;PF|FRENCH SOUTHERN TERRITORIES;TF|GABON;GA|GAMBIA;GM|GEORGIA;GE|GERMANY;DE|GHANA;GH|GIBRALTAR;GI|GREECE;GR|GREENLAND;GL|GRENADA;GD|GUADELOUPE;GP|GUAM;GU|GUATEMALA;GT|GUERNSEY;GG|GUINEA;GN|GUINEA-BISSAU;GW|GUYANA;GY|HAITI;HT|HEARD ISLAND AND MCDONALD ISLANDS;HM|HOLY SEE (VATICAN CITY STATE);VA|HONDURAS;HN|HONG KONG;HK|HUNGARY;HU|ICELAND;IS|INDIA;IN|INDONESIA;ID|IRAN, ISLAMIC REPUBLIC OF;IR|IRAQ;IQ|IRELAND;IE|ISLE OF MAN;IM|ISRAEL;IL|ITALY;IT|JAMAICA;JM|JAPAN;JP|JERSEY;JE|JORDAN;JO|KAZAKHSTAN;KZ|KENYA;KE|KIRIBATI;KI|KOREA, DEMOCRATIC PEOPLE'S REPUBLIC OF;KP|KOREA, REPUBLIC OF;KR|KUWAIT;KW|KYRGYZSTAN;KG|LAO PEOPLE'S DEMOCRATIC REPUBLIC;LA|LATVIA;LV|LEBANON;LB|LESOTHO;LS|LIBERIA;LR|LIBYAN ARAB JAMAHIRIYA;LY|LIECHTENSTEIN;LI|LITHUANIA;LT|LUXEMBOURG;LU|MACAO;MO|MACEDONIA, THE FORMER YUGOSLAV REPUBLIC OF;MK|MADAGASCAR;MG|MALAWI;MW|MALAYSIA;MY|MALDIVES;MV|MALI;ML|MALTA;MT|MARSHALL ISLANDS;MH|MARTINIQUE;MQ|MAURITANIA;MR|MAURITIUS;MU|MAYOTTE;YT|MEXICO;MX|MICRONESIA, FEDERATED STATES OF;FM|MOLDOVA, REPUBLIC OF;MD|MONACO;MC|MONGOLIA;MN|MONTENEGRO;ME|MONTSERRAT;MS|MOROCCO;MA|MOZAMBIQUE;MZ|MYANMAR;MM|NAMIBIA;NA|NAURU;NR|NEPAL;NP|NETHERLANDS;NL|NEW CALEDONIA;NC|NEW ZEALAND;NZ|NICARAGUA;NI|NIGER;NE|NIGERIA;NG|NIUE;NU|NORFOLK ISLAND;NF|NORTHERN MARIANA ISLANDS;MP|NORWAY;NO|OMAN;OM|PAKISTAN;PK|PALAU;PW|PALESTINIAN TERRITORY, OCCUPIED;PS|PANAMA;PA|PAPUA NEW GUINEA;PG|PARAGUAY;PY|PERU;PE|PHILIPPINES;PH|PITCAIRN;PN|POLAND;PL|PORTUGAL;PT|PUERTO RICO;PR|QATAR;QA|RÉUNION;RE|ROMANIA;RO|RUSSIAN FEDERATION;RU|RWANDA;RW|SAINT BARTHÉLEMY;BL|SAINT HELENA, ASCENSION AND TRISTAN DA CUNHA;SH|SAINT KITTS AND NEVIS;KN|SAINT LUCIA;LC|SAINT MARTIN (FRENCH PART);MF|SAINT PIERRE AND MIQUELON;PM|SAINT VINCENT AND THE GRENADINES;VC|SAMOA;WS|SAN MARINO;SM|SAO TOME AND PRINCIPE;ST|SAUDI ARABIA;SA|SENEGAL;SN|SERBIA;RS|SEYCHELLES;SC|SIERRA LEONE;SL|SINGAPORE;SG|SINT MAARTEN (DUTCH PART);SX|SLOVAKIA;SK|SLOVENIA;SI|SOLOMON ISLANDS;SB|SOMALIA;SO|SOUTH AFRICA;ZA|SOUTH GEORGIA AND THE SOUTH SANDWICH ISLANDS;GS|SPAIN;ES|SRI LANKA;LK|SUDAN;SD|SURINAME;SR|SVALBARD AND JAN MAYEN;SJ|SWAZILAND;SZ|SWEDEN;SE|SWITZERLAND;CH|SYRIAN ARAB REPUBLIC;SY|TAIWAN, PROVINCE OF CHINA;TW|TAJIKISTAN;TJ|TANZANIA, UNITED REPUBLIC OF;TZ|THAILAND;TH|TIMOR-LESTE;TL|TOGO;TG|TOKELAU;TK|TONGA;TO|TRINIDAD AND TOBAGO;TT|TUNISIA;TN|TURKEY;TR|TURKMENISTAN;TM|TURKS AND CAICOS ISLANDS;TC|TUVALU;TV|UGANDA;UG|UKRAINE;UA|UNITED ARAB EMIRATES;AE|UNITED KINGDOM;GB|UNITED STATES;US|UNITED STATES MINOR OUTLYING ISLANDS;UM|URUGUAY;UY|UZBEKISTAN;UZ|VANUATU;VU|VENEZUELA, BOLIVARIAN REPUBLIC OF;VE|VIET NAM;VN|VIRGIN ISLANDS, BRITISH;VG|VIRGIN ISLANDS, U.S.;VI|WALLIS AND FUTUNA;WF|WESTERN SAHARA;EH|YEMEN;YE|ZAMBIA;ZM|ZIMBABWE;ZW".ToLower();
            string[] countryList = totalCountry.Split('|');

            for (int x = 0; x < countryList.Length; x++)
            {
                string[] countryBoth = countryList[x].Split(';');
                countryMap.Add(countryBoth[0], countryBoth[1]);
            }
        }

        /// <summary>
        /// Public method that will obtain all ground stations. There are two ways to do 
        /// this, online and offline. Either way will work and will be independent of the
        /// rest of the code because I carefully modulated the methods to be carefree of
        /// whether the data was picked up from online or offline.
        /// <online>
        /// Will call the NASA services, assuming that they accept silverlight clients.
        /// </online>
        /// <offline>
        /// Will use the strings that I have created statically using some python scripts.
        /// </offline>
        /// </summary>
        public void getAllGroundStations()
        {
            if (this.NASAService)
            {
                this.getAllGroundStationsOnline();
            }
            else
            {
                this.getAllGroundStationsStatic();
            }
        }

        /// <summary>
        /// Retrieves the GroundStation data from a string that I have 
        /// statically set. (Taken from the NASA service). This will
        /// get all the data and send it to be reverse geocoded to find
        /// out what country it is in.
        /// </summary>
        private void getAllGroundStationsStatic()
        {
            string data = "SPA|-89.99|-13.32|South Pole\nSBF|-77.85|166.75|Scott Base\nMCM|-77.85|166.7|McMurdo\nSPL|-76|-84|Siple\nHBA|-75.52|-26.6|Halley Bay\nMZH|-70.43|40.2|Mizuho\nSYO|-69.01|39.59|Syowa\nMAF|-67.62|62.89|Mawson\nFRZ|-62.08|-58.39|Ferraz\nCCF|-43.83|172.68|Christchurch\nADF|-34.56|138.48|Adelaide\nCTO|-33.95|18.47|Cape Town\nAQF|-16.5|-71.5|Arequipa\nHUA|-12.05|-75.34|Huancayo\nJRO|-11.9|-76|Jicamarca\nCIA|1.95|-157.3|Christmas Island\nKGO|9.3|-5.4|Korhogo\nGUA|13.58|144.87|Guam\nARO|18.3|-66.75|Arecibo\nSJG|18.38|-66.12|San Juan\nTEO|19.74|-260.81|Teoloyucan\nHON|21.32|-158.06|Honolulu\nFIT|28.07|-80.95|Florida Tech.\nJAX|30.35|-81.6|Jacksonville Univ.\nSLU|30.44|-269.27|SE Louisiana Univ.\nTUC|32.25|-110.83|Tucson\nUSC|33.34|-81.46|Univ. S. Carolina\nMUI|34.8|136.1|Shigaraki\nLAL|35.88|-253.33|Los Alamos\nKAK|36.23|140.19|Kakioka\nDSO|36.25|-81.4|Dark Sky Obs.\nBERK|37.881|-122.244|THM_Berkeley\nASH|37.95|58.11|Ashkhabad\nFRD|38.2|-77.4|USGS Fredericksburg\nONW|38.43|141.47|Onagawa\nAFA|39.01|-255.12|USAFA Colorado\nAPL|39.17|-76.88|Applied Physics Lab.\nGDN|39.75|-105.16|Golden\nUIL|40.1|-88.1|Urbana\nPLA|40.13|-104.5|Platteville\nBLD|40.13|-254.76|Boulder\nBOU|40.14|-105.24|Boulder\nCSL|40.59|-105.14|Ft Collins\nTKT|41.33|69.62|Tashkent\nUSI|41.4|-111.5|Bear Lake\nTFS|42.09|44.71|Tbilisi\nWES|42.38|-71.32|Weston\nPFP|42.4|-86.93|Peach Mountain\nMLH|42.6|-71.48|Millstone Hill\nDUM|43.12|-70.94|Durham\nAAA|43.2|76.9|Alma-Ata\nGTF|43.62|-71.95|Grafton Test Facility\nCLK|44.7|-75|Clarkson University\nOTT|45.4|-75.5|Ottawa\nNKK|45.8|62.1|Novokazalinsk\nSTJ|47.59|-52.68|St. Johns\nNEW|48.26|-117.12|Newport\nVIC|48.52|-123.42|Victoria\nKAPU|49.4|-82.4|THM_Kapuskasing\nGLL|49.6|-97.1|Glenlea\nKGD|49.82|73.08|Karaganda\nDOU|50.1|4.6|Dourbes\nPINA|50.3|-96|THM_Pinawa\nKIV|50.72|30.3|Kiev\nRDL|50.9|-93.5|Red Lake\nGANG|51.9|-68.2|THM_Gangon\nCOF|52|15|Collm\nSHF|52.16|-106.53|Saskatoon\nIRT|52.46|104.04|Irkutsk\nGBAY|53.3|-60.4|THM_Goose Bay\nNIP|53.5|-103.7|Nipawin\nPGEO|53.815|-122.828|THM_Prince George\nISL|53.88|-94.68|Island Lake\nTPAS|53.994|-100.941|THM_Flin Flon/The Pas\nMEA|54.62|-112.33|Meanook\nKNG|54.7|20.62|Kaliningrad\nATHA|54.714|-113.314|THM_Athabasca\nNVS|55.03|82.9|Novosibirsk\nPBQ|55.3|-77.75|THM_Poste Baleine\nESK|55.32|-3.2|Eskdalemuir\nMOS|55.48|37.31|Moscow\nGILL|56.4|-94.6|THM_Gillam\nTMK|56.47|84.93|Tomsk\nNAIN|56.5|-61.7|THM_Nain\nFMM|56.73|-111.38|Fort McMurray\nSVD|56.73|61.07|Sverdlovsk\nLYN|56.85|-101.07|Lynn Lake\nSIT|57.05|-135.33|Sitka\nBKC|57.68|-94.23|Back\nRBL|58.22|-103.67|Rabbit Lake\nFCC|58.8|-94.1|Fort Churchill\nSTM|59.5|18.2|Stockholm\nUPP|59.8|17.6|Uppsala\nLNN|59.95|30.71|Leningrad\nFSMI|59.984|-111.842|THM_Fort Smith\nWFP|60.06|-128.58|Watson Lake\nMGD|60.12|151.02|Magadan\nNUR|60.51|24.66|Nurmijarvi\nWHOR|61.01|-135.223|THM_White Horse\nEKP|61.1|-94.07|Eskimo Point\nNAQ|61.1|-45.2|Narssarssuaq\nAMU|61.24|-149.87|Anchorage\nPOD|61.6|90|Podk. Tunguska\nFSIM|61.8|-121.2|THM_Fort Simpson\nYAK|62.02|129.72|Yakutsk\nTLK|62.3|-150.1|Talkeetna\nFHF|62.32|26.61|Hankasalmi\nGAK|62.4|-145.2|THM_HAARP/Gakona\nYKC|62.4|-114.5|Yellowknife\nRANK|62.828|-92.113|THM_Rankin Inlet\nMCGR|62.953|-155.596|THM_Mcgrath\nIQA|63.8|-68.6|Iqaluit\nWHF|63.86|-22.02|Stokkseyri\nEHF|63.86|-19.2|Pykkvibaerehf\nDWC|64.07|-139.42|Dawson City\nBLC|64.33|-96.03|Baker Lake\nARK|64.6|40.5|Arkhangelsk\nLYS|64.62|18.77|Lycksele\nHSF|64.67|-21.03|Husafell\nEKAT|64.733|-110.67|THM_Lac de Gras/Ekati\nCMO|64.85|-147.83|College\nNOW|64.9|-125.5|Norman Wells\nOUL|65.1|25.85|Oulu\nCOW|65.73|-111.25|Contwoyto Lake\nIFJ|66.08|-23.13|Isafjordur\nTJN|66.2|-17.12|Tjornes\nFYU|66.56|-145.214|THM_Fort Yukon\nPEL|66.9|24.08|Pello\nKIA|66.9|-160.4|THM_Kiana\nSTF|67.02|-50.72|Sondrestrom\nSOD|67.37|26.65|ECAT-Sodankyla\nKIR|67.83|20.42|ECAT-Kiruna\nMUO|68.02|23.53|Muonio\nAVI|68.13|-145.57|Arctic Village\nINUV|68.413|-133.77|THM_Inuvik\nIVA|68.6|27.48|Ivalo\nALT|68.9|22.96|Alta\nCPS|68.92|-179.48|Cape Schmidt\nMMK|68.95|33.05|Murmansk\nKAU|69.02|23.05|Kautokeino\nKIL|69.05|20.7|Kilpisjarvi\nCBB|69.2|-105|Cambridge Bay\nGDH|69.24|-53.52|Godhavn\nNOK|69.4|88.1|Norilsk\nAMD|69.47|61.42|Amderma\nTRO|69.66|18.95|ECAT-Tromso\nKEV|69.76|27.01|Kevo\nCPY|70.17|-124.72|Cape Parry\nSOE|70.54|22.22|Soroya\nBRW|71.3|-156.75|Barrow\nTIK|71.58|129|Tixie Bay\nSKG|71.9|82.1|Sopoch. Karga\nSAH|72|-125|Sachs Harbor\nDIK|73.54|80.56|Dixon Island\nRES|74.7|-94.9|Resolute Bay\nIZV|75.9|82.7|Izvestia\nKOT|76|137.9|Kotelny\nMBC|76.2|-119.4|Mould Bay\nHRN|77|15.55|Hornsund\nTFP|76.53|-68.44|Thule\nTHL|77.48|-69.17|Thule/Qaanaaq\nUDN|77.5|82.3|Uedinenie\nESR|78.15|16.03|ECAT-Svalbard\nVIZ|79.5|78.1|Vize\nLNC|78.9|12|Ny Alesund\nHIS|80.62|58.05|Heiss Island\nNOC|81.6|-16.6|Nord\nALE|82.5|-62.5|Alert";
            string[] dataLines = data.Split('\n');

            for (int x = 0; x < dataLines.Length; x++)
            {
                string[] result = dataLines[x].Split('|');
                this.reverseGeocode(result[3], result[0], new Location(float.Parse(result[1]), float.Parse(result[2])));
            }
        }

        /// <summary>
        /// Retrieves the data from the online services provided by NASA.
        /// Will gather the data asynchronously and call the callback method
        /// on completion.
        /// </summary>
        private void getAllGroundStationsOnline()
        {
            // Create the completed handler for when the asynchronous data download is finished
            this.ssc.getAllGroundStationsCompleted += new EventHandler<getAllGroundStationsCompletedEventArgs>(this.getAllGroundStationsOnlineCompleted);

            // Retrive the data asynchronously
            this.ssc.getAllGroundStationsAsync();
        }

        /// <summary>
        /// After gathering the ground station data asynchronously, it will attempt to 
        /// send the data to be reverse geocoded to find out what country the staion is in.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">Contains the results from the NASA web service</param>
        private void getAllGroundStationsOnlineCompleted(object sender, getAllGroundStationsCompletedEventArgs e)
        {
            // Assign the retrieved data into variable result
            groundStationDescription[] result = e.Result;

            // For each ground station reverse geocode it
            for (int x = 0; x < result.Length; x++)
            {
                this.reverseGeocode(result[x].name, result[x].id, new Location(result[x].latitude, result[x].longitude));
            }
        }

        /// <summary>
        /// After obtaining the data for the ground station, this method will create the marker
        /// on the map by placing its country flag on the map.
        /// </summary>
        /// <param name="name">Name of the ground station</param>
        /// <param name="id">id of the station</param>
        /// <param name="location">Location of the ground station</param>
        /// <param name="country">The ISO country code</param>
        private void mapGroundStation(string name, string id, Location location, string country)
        {
            // Country flag as the marker
            Image countryFlag = new Image();
            countryFlag.Source = new BitmapImage(new Uri("/Images/" + country + ".png", UriKind.Relative));
            countryFlag.Height = 11;
            countryFlag.Width = 16;

            this.mapLayer.AddChild(countryFlag, location); 
        }

        /// <summary>
        /// This method will reverse geocode the ground station based on its location and
        /// call the callback method.
        /// </summary>
        /// <param name="name">Name of the ground station</param>
        /// <param name="id">id of the station</param>
        /// <param name="location">Location of the ground station</param>
        private void reverseGeocode(string name, string id, Location location)
        {
            ReverseGeocodeRequest reverseGeocodeRequest = new ReverseGeocodeRequest();

            // Set the credentials using a valid Bing Maps key
            reverseGeocodeRequest.Credentials = new Credentials();
            reverseGeocodeRequest.Credentials.ApplicationId = this.BingMapsKey;

            // Set the point to use to find a matching address
            GeocodeService.GeocodeLocation point = new GeocodeService.GeocodeLocation();
            point.Latitude = location.Latitude;
            point.Longitude = location.Longitude;
            reverseGeocodeRequest.Location = point;

            // Make the reverse geocode request
            GeocodeServiceClient geocodeService = new GeocodeServiceClient("BasicHttpBinding_IGeocodeService");
            geocodeService.ReverseGeocodeCompleted += new EventHandler<ReverseGeocodeCompletedEventArgs>(
                delegate(object sender, ReverseGeocodeCompletedEventArgs e)
                {
                    this.reverseGeocodeCompleted(sender, e, name, id, location);
                });
            geocodeService.ReverseGeocodeAsync(reverseGeocodeRequest);
        }

        /// <summary>
        /// This method will be called upon completion of the reverse geocoding. On completion
        /// this ground station will be sent to be mapped by using the correct ISO country code.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">Results of the reverse geocoding</param>
        /// <param name="name">Name of the ground station</param>
        /// <param name="id">id of the station</param>
        /// <param name="location">Location of the ground station</param>
        private void reverseGeocodeCompleted(object sender, ReverseGeocodeCompletedEventArgs e, string name, string id, Location location)
        {
            string country = "";
            try
            {
                // Get the country code from the results
                country = e.Result.Results[0].Address.CountryRegion.ToLower();

                // Add the ground station locations to the map
                this.mapGroundStation(name, id, location, countryMap[country]);
            }
            catch
            {
                // Error was found, due to lack of time just place it to be european union
                this.mapGroundStation(name, id, location, "europeanunion");
            }
        }
    }
}
