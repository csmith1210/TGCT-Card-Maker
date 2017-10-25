# TGCTours Card Maker
### Description:
This program automatically fetches stats from TGCTours.com for a player and creates an image that can be used for forum signatures or to show off stats.
### Libraries Required:
* [HtmlAgilityPack](https://github.com/zzzprojects/html-agility-pack/)
  * Package used to scrape the season and overview player pages for various stats.
* [Costura.Fody](https://github.com/Fody/Costura)
  * Package used to combine the ironwebscraper DLL into the final binary.
### How to use the program:
1. Build or download the binary release file. Only the EXE is required to run the program.
2. Double click the EXE to launch the program and follow the initial set up to save your name, member date, country, and the platform you play TGC2 on.
   * One input that you will need is what I'm calling your player ID. This is the number that is in your URL when you navigate to General Info > Your Tour Profile on TGCTours.com.
   * e.g. Player ID of 1234: http://tgctours.com/Player/OverView/1234
3. The program will fetch your stats and output a PNG file in the same directory as the EXE.
4. When each week's tournament "starts" on the site (Monday morning), run the EXE again (it will remember your information), and the program will fetch the stats for the previous 7 weeks.
### The Template:
![template](https://github.com/csmith1210/TGCT-Card-Maker/raw/master/TGCT%20Card%20Maker/Resources/template.png)
### Acknowledgements:
**HtmlAgilityPack** and **Costura.Fody** use the MIT License which is the same license used by this repository. Please see the included LICENSE.md file.
