using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
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
                if(urlCheck)
                    result.Add(word);
            }

            return result;
        }

        // TODO: Need to improve the performance and correctness here
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



        private string GetHtmlContentFromLink(HttpWebResponse response)
        {
            try
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;
                if (response.CharacterSet == null)
                    readStream = new StreamReader(receiveStream);
                else
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                string result = readStream.ReadToEnd();
                response.Close();
                readStream.Close();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }



        public bool IsSpam(string content, string[] domains, int redirectionDepth)
        {
            return IsSpamProcess(content, domains, redirectionDepth, 0);
        }


        public String GetRedirectOrHtmlContent(string url)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.AllowAutoRedirect = false; 

            webRequest.Timeout = 10000;
            //webRequest.Method = "GET";
            HttpWebResponse webResponse;
            string result = "";
            try
            {
                using (webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    // TODO: Check more for redirect status
                    if ((int)webResponse.StatusCode == (int)HttpStatusCode.Redirect || (int)webResponse.StatusCode == (int)HttpStatusCode.Moved)
                    {
                        result = webResponse.Headers["Location"];
                        webResponse.Close();
                    }
                    else if (webResponse.StatusCode == HttpStatusCode.OK)
                    {
                        result = GetHtmlContentFromLink(webResponse);
                    }
                }

                return result;
            } catch(Exception ex)
            {
                return "";
            }
        }


        public bool IsSpamProcess(string content, string[] domains, int redirectionDepth, int currentDepth)
        {
            // Avoid going too deep
            if (currentDepth > redirectionDepth)
                return false;

            var potentialLinks = GetAllPotentialLinks(content);
            if (potentialLinks.Count < 1)
                return false;

            var result = false;
            foreach(string link in potentialLinks)
            {
                result = DomainCompare(domains, link);
                if (result)
                    break;

                var newContentString = GetRedirectOrHtmlContent(link);
                if (string.IsNullOrWhiteSpace(newContentString))
                    continue;

                result = IsSpamProcess(newContentString, domains, redirectionDepth, currentDepth + 1);
                if (result)
                    break;
            }

            if (result)
                return true;

            return result;
        }
    }
}
