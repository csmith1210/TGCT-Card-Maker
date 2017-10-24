using IronWebScraper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;

namespace TGCT_Card_Maker
{
    internal class Program
    {
        //START: Variables

        static public bool firstTime;
        static public int NumEvents = 0;
        static public int wins = 0;
        static public int seconds = 0;
        static public int top10 = 0;
        static public int top25 = 0;
        static public List<int> CutPercents = new List<int>();
        static public double CutPercent = 0;
        static public int money = 0;
        static public int moneyRank = 0;
        static public int wgr = 0;
        static public string playerName;
        static public string memSince;
        static public string tour;
        static public string country;
        static public string platform;
        static public string platUser;
        static public string playerID;
        static public DataTable dt = new DataTable();
        static public DateTime seasonStart = DateTime.Parse("10/16/2017");

        //END: Variables

        private static void Main(string[] args)
        {
            Load();
            if (firstTime)
                Setup();
            Scrape();
            MakeImage();

            //delete the folder ironwebscraper creates
            if (Directory.Exists("Scrape\\"))
                Directory.Delete("Scrape\\");

            Process.Start("tgctcard.png"); //opens the newly created image for viewing
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
            templateGraphic.DrawString("$" + money.ToString(), catFont, Brushes.White, new Rectangle(373, 175, 61, 18), catsFormat);
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
            Console.WriteLine("Scraping tgctours.com...");
            //Create and execute webscraper object to grab info from player overview page
            var mainScraper = new TGCScraper("mains");
            mainScraper.Start();
            //prepare datatable with appropriate columns for next webscraper object
            dt.Columns.Add("Date", typeof(DateTime));
            dt.Columns.Add("League", typeof(string));
            dt.Columns.Add("Place", typeof(string));
            //create and execute webscraper object to grab stats and tournaments from season page
            var catScraper = new TGCScraper("cats");
            catScraper.Start();
            //remove all cut percentages from the seasons pages equal to 0 then average the remaining values
            CutPercents.RemoveAll(i => i == 0);
            CutPercent = Math.Round(CutPercents.Average(), 2);
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
            Console.Clear();
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
    }

    internal class TGCScraper : WebScraper
    {
        public override void Init()
        {
            //set log level to none (but this still results in 1 inital output line to the console)
            this.LoggingLevel = WebScraper.LogLevel.None;
        }

        public TGCScraper(string type)
        {
            if (type.Equals("cats")) //get the stats from seasons page
            {
                this.MaxHttpConnectionLimit = 10; //use more threads for simultaneous results
                //create list of urls for every main tour
                string[] tours = new string[] { "1", "2", "4", "10", "11", "12", "13", "14", "15", "18" };
                List<string> urls = new List<string>();
                string baseUrl = "http://tgctours.com/player/season/" + Program.playerID + "?tourId=";
                foreach (string tour in tours)
                    urls.Add(baseUrl + tour + "&season=");
                //scrape each url in the list using the Parse() method
                this.Request(urls, Parse);
            }
            else if (type.Equals("mains")) //get other info from the overview page
            {
                this.MaxHttpConnectionLimit = 1; //only checking one url here, just need one thread
                string baseUrl = "http://tgctours.com/player/OverView/" + Program.playerID;
                this.Request(baseUrl, GetMains); //scrape url using the GetMains() method
            }
        }

        public void GetMains(Response response)
        {
            //Grab the player's current tour, money rank, and WGR from the Overview page
            //See the ironwebscraper site for how the HTML element parsing works
            Program.tour = response.Css("section.content.clearfix h1")[0].InnerTextClean.Split(' ')[1];
            Program.moneyRank = int.Parse(response.Css("div.box h2")[1].InnerTextClean);
            Program.wgr = int.Parse(response.Css("div.box h2")[3].InnerTextClean.Split(' ')[0]);
        }

        public override void Parse(Response response)
        {
            //This method gets the stats for every major tour
            //get the tour url currently being examined
            string curTour = response.Css("h1")[1].TextContentClean;
            List<string> curTours = curTour.Split(null).ToList();
            curTour = curTours[0];
            List<int> cats = new List<int>();
            //loop through each td element in the summary table (top table) of the seasons page
            //add the elements to the cats list for later use
            foreach (var item in response.GetElementById("summary").Css("td"))
            {
                string strTitle = item.TextContentClean;
                if (strTitle == "--")
                    cats.Add(0);
                else
                {
                    char[] nums = strTitle.Where(Char.IsDigit).ToArray();
                    string number = new string(nums);
                    cats.Add(int.Parse(number));
                }
            }
            //add the scraped element values to accumulate stats among tours (i.e. for when players are promoted/demoted)
            Program.wins += cats[1];
            Program.seconds += cats[2];
            Program.top10 += cats[4];
            Program.top25 += cats[5];
            Program.CutPercents.Add(cats[6]);
            Program.money += cats[7];
            List<string> rs = new List<string>();
            //loop through each td element of the results table (bottom table) of the seasons page
            //add the elements to the rs list (results list) for later use
            foreach (var item in response.GetElementById("results").Css("td"))
            {
                string strTitle = item.TextContentClean;
                rs.Add(strTitle);
            }
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
                Program.dt.Rows.Add(DateTime.Parse(rs[i]), curTour, rs[i + 2]);
        }
    }
}