using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareMarket_Enhanced
{
    public static class Utility
    {
        public static List<string> urls = new List<string>();
        public static Dictionary<string, string> stockUrls = new Dictionary<string, string>
        {
            { "Airtel", "https://www.indiainfoline.com/company/bharti-airtel-ltd-share-price/15542" },
            { "Maruti", "https://www.indiainfoline.com/company/maruti-suzuki-india-ltd-share-price/5496" },
            { "Nestle", "https://www.indiainfoline.com/company/nestle-india-ltd-share-price/175" }
        };
        public static Dictionary<string, string> stockData = new Dictionary<string, string>()
        {
            {"Airtel", "" },
            {"Maruti", "" },
            {"Nestle", "" }
        };

        public static void LoadUrls()
        {
            string []urlCompany = { "MARUTI", "BHARTIARTL", "NESTLEIND", "EICHERMOT"};
            string baseUrl = "https://www1.nseindia.com/live_market/dynaContent/live_watch/get_quote/GetQuote.jsp?symbol=";

            foreach (string company in urlCompany)
            {
                urls.Add(baseUrl + company + "&illiquid=0&smeFlag=0&itpFlag=0");
            }
        }
    }
}
