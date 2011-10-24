
using System;
using System.Collections.Generic;
namespace AE.Net.Mail {

    public class SearchCondition {
        public static SearchCondition Text(string text) { return new SearchCondition { Location = Locations.Text, Value = text }; }
        public static SearchCondition BCC(string text) { return new SearchCondition { Location = Locations.BCC, Value = text }; }
        public static SearchCondition Before(DateTime date) { return new SearchCondition { Location = Locations.Before, Value = date }; }
        public static SearchCondition Body(string text) { return new SearchCondition { Location = Locations.Body, Value = text }; }
        public static SearchCondition Cc(string text) { return new SearchCondition { Location = Locations.Cc, Value = text }; }
        public static SearchCondition From(string text) { return new SearchCondition { Location = Locations.From, Value = text }; }
        //public static SearchCondition Header(string name, string text) { return new SearchCondition { Location = Locations.From, Value = text }; }
        public static SearchCondition Keyword(string name, string text) { return new SearchCondition { Location = Locations.Keyword, Value = text }; }
        public static SearchCondition Larger(long size) { return new SearchCondition { Location = Locations.Larger, Value = size }; }
        public static SearchCondition Smaller(long size) { return new SearchCondition { Location = Locations.Smaller, Value = size }; }
        public static SearchCondition SentBefore(DateTime date) { return new SearchCondition { Location = Locations.SentBefore, Value = date }; }
        public static SearchCondition SentOn(DateTime date) { return new SearchCondition { Location = Locations.SentOn, Value = date }; }
        public static SearchCondition SentSince(DateTime date) { return new SearchCondition { Location = Locations.SentSince, Value = date }; }
        public static SearchCondition Subject(string text) { return new SearchCondition { Location = Locations.Subject, Value = text }; }
        public static SearchCondition To(string text) { return new SearchCondition { Location = Locations.To, Value = text }; }
        public static SearchCondition UID(string ids) { return new SearchCondition { Location = Locations.UID, Value = ids }; }
        public static SearchCondition Unkeyword(string text) { return new SearchCondition { Location = Locations.Unkeyword, Value = text }; }
        public static SearchCondition Answered() { return new SearchCondition { Location = Locations.Answered }; }
        public static SearchCondition Deleted() { return new SearchCondition { Location = Locations.Deleted }; }
        public static SearchCondition Draft() { return new SearchCondition { Location = Locations.Draft }; }
        public static SearchCondition Flagged() { return new SearchCondition { Location = Locations.Flagged }; }
        public static SearchCondition New() { return new SearchCondition { Location = Locations.New }; }
        public static SearchCondition Old() { return new SearchCondition { Location = Locations.Old }; }
        public static SearchCondition Recent() { return new SearchCondition { Location = Locations.Recent }; }
        public static SearchCondition Seen() { return new SearchCondition { Location = Locations.Seen }; }
        public static SearchCondition Unanswered() { return new SearchCondition { Location = Locations.Unanswered }; }
        public static SearchCondition Undeleted() { return new SearchCondition { Location = Locations.Undeleted }; }
        public static SearchCondition Undraft() { return new SearchCondition { Location = Locations.Undraft }; }
        public static SearchCondition Unflagged() { return new SearchCondition { Location = Locations.Unflagged }; }
        public static SearchCondition Unseen() { return new SearchCondition { Location = Locations.Unseen }; }

        public object Value { get; set; }
        public Locations? Location { get; set; }

        public enum Locations {
            BCC, Before, Body, Cc, From, Header, Keyword,
            Larger, On, SentBefore, SentOn, SentSince, Since, Smaller, Subject,
            Text, To, UID, Unkeyword, All, Answered, Deleted, Draft, Flagged,
            New, Old, Recent, Seen, Unanswered, Undeleted, Undraft, Unflagged, Unseen
        }

        internal List<SearchCondition> Conditions { get; set; }
        internal string Operator { get; set; }
        private static SearchCondition Join(string condition, SearchCondition left, params SearchCondition[] right) {
            condition = condition.ToUpper();

            if (left.Operator != condition) {
                left = new SearchCondition { Operator = condition, Conditions = new List<SearchCondition> { left } };
            }

            left.Conditions.AddRange(right);
            return left;
        }

        public SearchCondition And(params SearchCondition[] other) {
            return Join(string.Empty, this, other);
        }

        public SearchCondition Not(params SearchCondition[] other) {
            return Join("NOT", this, other);
        }

        public SearchCondition Or(params SearchCondition[] other) {
            return Join("OR", this, other);
        }

        public override string ToString() {
            if (Conditions != null && Conditions.Count > 0 && Operator != null) {
                return (Operator.ToUpper() + " (" + string.Join(") (", Conditions) + ")").Trim();
            }

            var builder = new System.Text.StringBuilder();
            if (Location != null) builder.Append(Location.ToString().ToUpper());

            if (Value != null) {
                var value = Value;
                switch (Location) {
                    case Locations.BCC:
                    case Locations.Body:
                    case Locations.From:
                    case Locations.Subject:
                    case Locations.Text:
                    case Locations.To:
                        value = "\"" + Convert.ToString(Value)
                            .Replace("\\", "\\\\")
                            .Replace("\r", "\\r")
                            .Replace("\n", "\\n")
                            .Replace("\"", "\\\"") + "\"";
                        break;
                }

                if (value is DateTime) {
                    value = "\"" + GetRFC2060Date((DateTime)value) + "\"";
                }

                if (Location != null) builder.Append(" ");
                builder.Append(value);
            }

            return builder.ToString();
        }

        public static string GetRFC2060Date(DateTime date) {
            return date.ToString("dd-MMM-yyyy hh:mm:ss zz");
        }
    }
}
