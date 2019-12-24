using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace SpamFilter
{
    public class Filter
    {
        // www.google.com
        // http://aaa

        private readonly char[] _wordDelimiters = {
            '{', '}', '|', ' ', '~', '@', '$', '\'', '"', '\t' };


        private List<string> GetAllPotentialLinks(string content)
        {
            var result = new List<string>();

            if (String.IsNullOrWhiteSpace(content))
                return result;

            var words = content.ToLower().Split(_wordDelimiters, StringSplitOptions.RemoveEmptyEntries);
            foreach(string word in words)
            {
                var urlCheck = IsLink(word);
                result.Add(word);
            }

            return result;
        }

        // TODO: Need to improve the performance here
        private bool IsLink(string word)
        {
            var regex = new Regex(@"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?$");
            return regex.IsMatch(word);
        }


        private bool DomainCompare(string[] domains, string url)
        {
            try
            {
                Uri myUri = new Uri(url);
                string urlHost = myUri.Host.ToLower().Replace("www.", "");
                bool result = false;
                foreach(string domain in domains)
                {
                    var comparingDomain = domain.ToLower().Replace("www.", "");
                    result = comparingDomain.Equals(urlHost);
                    if (result)
                        break;
                }

                return result;
            }
            catch
            {
                return false;
            }
        }



        private string GetHtmlContentFromLink(string url)
        {
            try
            {
                string result;
                using (WebClient client = new WebClient())
                {
                    result = client.DownloadString(url);
                }

                return result;
            }
            catch
            {
                return string.Empty;
            }
        }



        public bool IsSpam(string content, string[] domains, int redirectionDepth)
        {
            return IsSpamProcess(content, domains, redirectionDepth, 1);
        }


        public bool IsSpamProcess(string content, string[] domains, int redirectionDepth, int currentDepth)
        {
            var potentialLinks = GetAllPotentialLinks(content);
            if (potentialLinks.Count < 1)
                return false;

            foreach(string link in potentialLinks)
            {
                var newContentString = GetHtmlContentFromLink(link);
                if (string.IsNullOrWhiteSpace(newContentString))
                    continue;

            }

            // Avoid going too deep
            if (currentDepth >= redirectionDepth)
                return false;

            return false;
        }
    }
}
