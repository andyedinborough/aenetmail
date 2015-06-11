using System;
using System.Text.RegularExpressions;

namespace AE.Net.Mail
{
    public class MailAddress
    {
        public MailAddress()
        {
        }

        public MailAddress(string value)
        {
            if(!TryParse(value)) 
            {
                throw new Exception("Unable to parse " + value);
            }
        }

        public MailAddress(string displayName, string address)
        {
            DisplayName = displayName;
            Address = address;
        }
        
        public string DisplayName { get; set; }
        
        public string Address { get; set; }
        
        public static bool TryParse(string value, out MailAddress address) 
        {
            var parsed = new MailAddress();
            var passed = parsed.TryParse(value);
            address =  passed ? parsed : null;
            return passed;
        }
        
        private bool TryParse(string value) 
        {
            if(string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
            
            value = value.Trim();
            if(value.EndsWith(">"))
            {
                var bracket = value.LastIndexOf("<");
                if(bracket == -1) return false;
                Address = value.Substring(bracket + 1, value.Length - 1);
                DisplayName =bracket > 0 ? value.Substring(0, bracket - 1) : string.Empty;
                
                if(DisplayName.Length > 1)
                {
                    var first = DisplayName[0];
                    var last = DisplayName[DisplayName.Length - 1];
                    if(first == last && (first == '"' || first == '\''))
                    {
                        DisplayName = DisplayName.Substring(1, DisplayName.Length - 2)
                            .Replace(@"\" + first.ToString(), first.ToString());
                    }
                }
                
            } else {
                Address = value;
                DisplayName = string.Empty;
            }
         
            return IsValidEmailAddress(Address);   
        }
        
        public static bool IsValidEmailAddress(string address)
        {
            if(string.IsNullOrWhiteSpace(address)) return false;
            
            return Regex.IsMatch(address, @"^(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*
                  |  ""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]
                      |  \\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")
                @ (?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?
                  |  \[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}
                       (?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:
                          (?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]
                          |  \\[\x01-\x09\x0b\x0c\x0e-\x7f])+)
                     \])$", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        }
        
        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(DisplayName) 
                ? Address
                : (DisplayName + " <" + Address + ">");
        }
    }
}