using System;
using System.Text.RegularExpressions;

namespace AE.Net.Mail
{
    public class MailAddress
    {
        #region Constructors

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

        #endregion

        #region Properties

        public string Address { get; private set; }

        public string DisplayName { get; private set; }
        public string Host { get; private set; }
        public string User { get; private set; }

        #endregion

        #region Methods

        internal static Match ParseEmailAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) return null;

            const string RX = @"^                 #beginning of the string
    (?<user>
       [a-z0-9+=_\-]+    #one or more of these characters (this list is what I trimmed down)
       (?:               #group, but don’t capture 
          \.             #must have a . here
          [a-z0-9+=_\-]+ #same list as above
       )*                #this grouping can occur zero or more times
    )
    @                 #must have a @ here
    (?<host>
       (?:               #group, don’t capture
          [a-z0-9]       #any character in this list, once
          (?:            #group don’t capture
             [a-z0-9-]*  #any of these, zero or more
             [a-z0-9]    #any of these, once
          )?             #this grouping can happen 0 or 1 times
          \.             #must have a .
       )*                #this grouping zero or more times
       [a-z0-9]          #any of these characters, once
       (?:               #group don’t capture
          [a-z0-9-]*     #any of these characters, one or more times
          [a-z0-9]       #any of these characters, once
       )+                #this grouping, 1 or more times
    )
   $                 #end of the string
";

            var match = Regex.Match(address, RX, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            return match;
        }

        public static bool TryParse(string value, out MailAddress address)
        {
            var parsed = new MailAddress();
            var passed = parsed.TryParse(value);
            address =  passed ? parsed : null;
            return passed;
        }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(DisplayName)
                ? Address
                : (DisplayName + " <" + Address + ">");
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
                Address = value.Substring(bracket + 1, value.Length - bracket - 2);
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

            var match = ParseEmailAddress(Address);
            if (match == null || !match.Success) return false;

            Host = match.Groups["host"].Value;
            User = match.Groups["user"].Value;
            return !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(User);
        }

        #endregion
    }
}