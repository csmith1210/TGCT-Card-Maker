using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;

namespace TGCT_Card_Maker
{
    internal class Program
    {
        //START: Variables

        static private bool firstTime;
        static private int NumEvents = 0;
        static private int wins = 0;
        static private int seconds = 0;
        static private int top10 = 0;
        static private int top25 = 0;
        static private List<int> CutPercents = new List<int>();
        static private double CutPercent = 0;
        static private int money = 0;
        static private int moneyRank = 0;
        static private int wgr = 0;
        static private string playerName;
        static private string memSince;
        static private string tour;
        static private string country;
        static private string platform;
        static private string platUser;
        static private string playerID;
        static private DataTable dt = new DataTable();
        static private DateTime seasonStart = new DateTime(2017, 10, 16);
        static private HtmlWeb web = new HtmlWeb();
        static private HtmlDocument doc = new HtmlDocument();

        //END: Variables

        private static void Main(string[] args)
        {
            Load();
            if (firstTime)
                Setup();
            Console.WriteLine("Scraping tgctours.com...");
            Scrape();
            Console.WriteLine("Making image...");
            MakeImage();

            Process.Start("tgctcard.png"); //opens the newly created image for viewing
        }

        private static void Load()
        {
            //Load string variables from the settings file for use in MakeImage()
            firstTime = TGCT_Card_Maker.Properties.Settings.Default.firstTime;
            playerName = TGCT_Card_Maker.Properties.Settings.Default.playerName;
            playerID = TGCT_Card_Maker.Properties.Settings.Default.playerID;
            country = TGCT_Card_Maker.Properties.Settings.Default.playerCountry;
            memSince = TGCT_Card_Maker.Properties.Settings.Default.memSince;
            platform = TGCT_Card_Maker.Properties.Settings.Default.plaform;
        }

        private static void Save()
        {
            //Save the variables in the settings file for subsequent program executions
            TGCT_Card_Maker.Properties.Settings.Default.playerName = playerName;
            TGCT_Card_Maker.Properties.Settings.Default.playerID = playerID;
            TGCT_Card_Maker.Properties.Settings.Default.playerCountry = country;
            TGCT_Card_Maker.Properties.Settings.Default.memSince = memSince;
            TGCT_Card_Maker.Properties.Settings.Default.plaform = platform;
            TGCT_Card_Maker.Properties.Settings.Default.firstTime = false;
            TGCT_Card_Maker.Properties.Settings.Default.Save();
        }

        private static void Setup()
        {
            bool valid = false;
            while (!valid) //while loop to let user re-enter data if mistake was made
            {
                Console.Clear();
                Console.WriteLine("Initial Setup");
                Console.Write("Enter player's tour name (e.g. John Smith): ");
                playerName = Console.ReadLine();
                Console.Write("Enter player's country (e.g. United States): ");
                country = Console.ReadLine();
                Console.Write("Enter the player's join date (e.g. May 2017): ");
                memSince = Console.ReadLine();
                Console.WriteLine("Choose your platform:\n\t1. Steam\n\t2. Xbox one\n\t3. PS4");
                Console.Write("Enter the number for your platform (e.g. 1): ");
                platform = Console.ReadLine();
                Console.Write("Enter you username on the above platform: ");
                platUser = Console.ReadLine();
                Console.Write("Finally, enter the TGCTours player id: ");
                playerID = Console.ReadLine();
                Console.Clear();

                if (platform.Equals("1"))
                    platform = "STEAM:" + platUser;
                else if (platform.Equals("2"))
                    platform = "XBOX ONE:" + platUser;
                else if (platform.Equals("3"))
                    platform = "PS4:" + platUser;
                else
                    platform = "Invalid choice";

                Console.WriteLine("Player Name: " + playerName);
                Console.WriteLine("Country: " + country);
                Console.WriteLine("Member since: " + memSince);
                Console.WriteLine(platform);
                Console.WriteLine("TGCTours Player ID: " + playerID);
                Console.Write("Is this information correct? (y/n): ");
                string corr = Console.ReadLine().ToLower();
                if (corr.Equals("y"))
                    valid = true; //if the information is correct then exit while loop
            }
            Console.Clear();
            Save(); //save variables to the settings so this doesn't have to be done again
        }

        private static void Scrape()
        {
            //prepare datatable with appropriate columns for next webscraper object
            dt.Columns.Add("Date", typeof(DateTime));
            dt.Columns.Add("League", typeof(string));
            dt.Columns.Add("Place", typeof(string));
            Console.WriteLine("Grabbing the overview page info...");
            GetMains(); //grab info from player overview page
            Console.WriteLine("Grabbing the season pages info...");
            GetStats(); //grab stats and tournaments from season page
        }

        private static void GetMains()
        {
            //Grab the player's current tour, money rank, and WGR from the Overview page
            //HTML element parsing works using XPaths
            string url = "http://tgctours.com/player/OverView/" + playerID;
            doc = web.Load(url);
            var overviews = doc.DocumentNode.SelectNodes("//div[@class='box']/h2");
            moneyRank = int.Parse(overviews[1].InnerText);
            wgr = int.Parse(overviews[3].InnerText.Split(' ')[0]);
            tour = doc.DocumentNode.SelectNodes("//section[@class='content clearfix']/h1")[0].InnerText.Split(' ')[1];
        }

        private static void GetStats()
        {
            //create list of urls for every main tour
            string[] tours = new string[] { "1", "2", "4", "10", "11", "12", "13", "14", "15", "18" };
            List<string> urls = new List<string>();
            string baseUrl = "http://tgctours.com/player/season/" + playerID + "?tourId=";
            foreach (string tour in tours)
                urls.Add(baseUrl + tour + "&season=");
            int cutTracker = 0; //int used to keep track of all cut percents that are actually real (i.e. not "--")
            foreach (string url in urls) //scrape each url in the list
            {
                doc = web.Load(url); //load the webpage
                string curTour = doc.DocumentNode.SelectNodes("//h1")[1].InnerText.Split(null).ToList()[0]; //get the tour url currently being examined
                List<int> cats = new List<int>();
                //loop through each td element in the summary table (top table) of the seasons page and add the elements to the cats list for later use
                var nodes = doc.DocumentNode.SelectNodes("//table[@id='summary']/tbody/tr/td");
                for (int i = 0; i < nodes.Count; i++)
                {
                    string strTitle = nodes[i].InnerText;
                    if (strTitle.Equals("--"))
                        cats.Add(0);
                    else
                    {
                        if (i == 6) //cut percent is always 6th position
                            cutTracker++; //increase the tracker if the cut percent is actually a number
                        char[] nums = strTitle.Where(Char.IsDigit).ToArray(); //save only the characters that are numbers
                        string number = new string(nums); //make the character array back into a string
                        cats.Add(int.Parse(number)); //parse the new string of only numbers into an int
                    }
                }
                //add the scraped element values to accumulate stats among tours (i.e. for when players are promoted/demoted)
                wins += cats[1];
                seconds += cats[2];
                top10 += cats[4];
                top25 += cats[5];
                CutPercents.Add(cats[6]);
                money += cats[7];
                List<string> rs = new List<string>();
                //loop through each td element of the results table (bottom table) of the seasons page and add the elements to the rs list for later use
                var items = doc.DocumentNode.SelectNodes("//table[@id='results']/tbody/tr/td");
                if (items == null) //make sure results table isn't empty, if it is then skip to next tour
                    continue;
                foreach (var item in items)
                    rs.Add(item.InnerText);
                //get the amount of tournaments in the rs list where the player did not play/finish
                var indexes = Enumerable.Range(0, rs.Count)
                    .Where(i => rs[i] == "Did Not Play")
                    .ToList().Count;
                //loop through the rs list and remove the elements associated with the did not play rounds
                for (int i = 0; i < indexes; i++)
                {
                    int index = rs.IndexOf("Did Not Play");
                    rs.RemoveAt(index);
                    rs.RemoveAt(index - 1);
                    rs.RemoveAt(index - 2);
                }
                //for the remaining tournaments, add their respective info to their own rows in the datatable
                for (int i = 0; i < rs.Count; i += 9)
                    dt.Rows.Add(DateTime.ParseExact(rs[i], "MM/dd/yyyy", CultureInfo.InvariantCulture), curTour, rs[i + 2]);
            }
            CutPercent = Math.Round((double)CutPercents.Sum() / (double)cutTracker, 2);
            //sort the datatable by descending date
            dt.DefaultView.Sort = "Date desc";
            dt = dt.DefaultView.ToTable();
            //add a column to hold the week number and add a value for each row
            dt.Columns.Add("Week");
            foreach (DataRow row in dt.Rows)
            {
                DateTime date = row.Field<DateTime>("Date");
                var span = date.Subtract(seasonStart);
                int week = 1 + (span.Days / 7); //Week one has span of 0 + 1 = 1
                row["Week"] = "WK " + week.ToString(); //add calculated week number to the current row
            }
            NumEvents = dt.Rows.Count; //set the number of events equal to the amount of tournaments found for the current year
        }

        private static void MakeImage()
        {
            //Check the user's tour that was scraped and choose correct logo from resources
            Bitmap logoBitmap = new Bitmap(1, 1);
            if (tour.Equals("World"))
                logoBitmap = new Bitmap(TGCT_Card_Maker.Properties.Resources.World_logo);
            else if (tour.Equals("PGA"))
                logoBitmap = new Bitmap(TGCT_Card_Maker.Properties.Resources.PGA_logo);
            else if (tour.Equals("European"))
                logoBitmap = new Bitmap(TGCT_Card_Maker.Properties.Resources.European_logo);
            else if (tour.Contains("Web"))
                logoBitmap = new Bitmap(TGCT_Card_Maker.Properties.Resources.Web_logo);
            else if (tour.Contains("CC"))
                logoBitmap = new Bitmap(TGCT_Card_Maker.Properties.Resources.CC_logo);

            //create bitmap image holder
            Bitmap templateBitmap = new Bitmap(TGCT_Card_Maker.Properties.Resources.template);
            //create graphic object to make to changes to
            Graphics templateGraphic = Graphics.FromImage(templateBitmap);

            //START: Formatting to make text clear and set the font and alignment.
            templateGraphic.SmoothingMode = SmoothingMode.HighQuality;
            templateGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            templateGraphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
            templateGraphic.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            StringFormat mainFormat = new StringFormat()
            {
                //Near, Near = Top Left
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near
            };
            StringFormat catsFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            Font font = new Font("Consolas", 10, FontStyle.Bold);
            Font catFont = new Font("Consolas", 8, FontStyle.Bold);
            //END: Formatting

            //Add all stats to the image. For the main info that is not scraped, the text is added at specific pixel locations.
            //For the stats, a rectangle is created with the same dimensions as the boxes in the image, and the text is centered in those boxes.
            templateGraphic.DrawString(playerName, font, Brushes.White, new Point(45, 50), mainFormat);
            templateGraphic.DrawString(country, font, Brushes.White, new Point(71, 68), mainFormat);
            templateGraphic.DrawString(tour, font, Brushes.White, new Point(50, 86), mainFormat);
            templateGraphic.DrawString(memSince, font, Brushes.White, new Point(110, 104), mainFormat);
            templateGraphic.DrawString(platform, font, Brushes.White, new Point(9, 123), mainFormat);
            templateGraphic.DrawImage(logoBitmap, new Rectangle(390, 50, 80, 80));
            templateGraphic.DrawString(NumEvents.ToString(), catFont, Brushes.White, new Rectangle(0, 175, 61, 18), catsFormat);
            templateGraphic.DrawString(wins.ToString(), catFont, Brushes.White, new Rectangle(63, 175, 61, 18), catsFormat);
            templateGraphic.DrawString(seconds.ToString(), catFont, Brushes.White, new Rectangle(125, 175, 61, 18), catsFormat);
            templateGraphic.DrawString(top10.ToString(), catFont, Brushes.White, new Rectangle(187, 175, 61, 18), catsFormat);
            templateGraphic.DrawString(top25.ToString(), catFont, Brushes.White, new Rectangle(249, 175, 61, 18), catsFormat);
            templateGraphic.DrawString(CutPercent.ToString() + "%", catFont, Brushes.White, new Rectangle(311, 175, 61, 18), catsFormat);
            if (money <= 999999)
            {
                string moneyComma = string.Format(CultureInfo.InvariantCulture, "{0:N0}", money); //force commans in thousands place regardless of globalization settings
                templateGraphic.DrawString("$" + moneyComma, catFont, Brushes.White, new Rectangle(373, 175, 61, 18), catsFormat);
            }
            else
            {
                double mil = (double)money / 1000000;
                string moneyDec = string.Format(CultureInfo.InvariantCulture, "{0:0.000}", Math.Truncate(mil * 1000) / 1000); //force decimal place regardless of globalization
                templateGraphic.DrawString("$" + moneyDec + "M", catFont, Brushes.White, new Rectangle(373, 175, 61, 18), catsFormat);
            }
            templateGraphic.DrawString(moneyRank.ToString(), catFont, Brushes.White, new Rectangle(435, 175, 61, 18), catsFormat);
            templateGraphic.DrawString(wgr.ToString(), catFont, Brushes.White, new Rectangle(0, 232, 61, 18), catsFormat);

            //This loop add the last 7 tournament results to the remaining blank boxes.
            //Rectangle locations are calculated using the fact that each box with its white line is 62 pixels wide.
            for (int i = 1; i <= dt.Rows.Count && i < 8; i++)
            {
                string header = dt.Rows[i - 1]["Week"].ToString() + "\n" + dt.Rows[i - 1]["League"].ToString();
                templateGraphic.DrawString(header, catFont, Brushes.White, new Rectangle(62 * i + 1, 203, 61, 28), catsFormat);
                templateGraphic.DrawString(dt.Rows[i - 1]["Place"].ToString(), catFont, Brushes.White, new Rectangle(62 * i + 1, 232, 61, 18), catsFormat);
            }

            templateGraphic.Flush(); //Flush the changes to the graphic object to the bitmap object
            templateBitmap.Save("tgctcard.png"); //Save the newly changed bitmap object to a file
            //Dispose of the image objects to clear memory
            templateGraphic.Dispose();
            templateBitmap.Dispose();
        }
    }
}