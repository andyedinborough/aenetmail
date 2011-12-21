using System;
using System.Collections.Generic;
namespace AE.Net.Mail {

  public class SearchCondition {
    public static SearchCondition Text(string text) { return new SearchCondition { Field = Fields.Text, Value = text }; }
    public static SearchCondition BCC(string text) { return new SearchCondition { Field = Fields.BCC, Value = text }; }
    public static SearchCondition Before(DateTime date) { return new SearchCondition { Field = Fields.Before, Value = date }; }
    public static SearchCondition Body(string text) { return new SearchCondition { Field = Fields.Body, Value = text }; }
    public static SearchCondition Cc(string text) { return new SearchCondition { Field = Fields.Cc, Value = text }; }
    public static SearchCondition From(string text) { return new SearchCondition { Field = Fields.From, Value = text }; }
    public static SearchCondition Header(string name, string text) { return new SearchCondition { Field = Fields.Header, Value = name + " " + text.QuoteString() }; }
    public static SearchCondition Keyword(string name, string text) { return new SearchCondition { Field = Fields.Keyword, Value = text }; }
    public static SearchCondition Larger(long size) { return new SearchCondition { Field = Fields.Larger, Value = size }; }
    public static SearchCondition Smaller(long size) { return new SearchCondition { Field = Fields.Smaller, Value = size }; }
    public static SearchCondition SentBefore(DateTime date) { return new SearchCondition { Field = Fields.SentBefore, Value = date }; }
    public static SearchCondition SentOn(DateTime date) { return new SearchCondition { Field = Fields.SentOn, Value = date }; }
    public static SearchCondition SentSince(DateTime date) { return new SearchCondition { Field = Fields.SentSince, Value = date }; }
    public static SearchCondition Subject(string text) { return new SearchCondition { Field = Fields.Subject, Value = text }; }
    public static SearchCondition To(string text) { return new SearchCondition { Field = Fields.To, Value = text }; }
    public static SearchCondition UID(string ids) { return new SearchCondition { Field = Fields.UID, Value = ids }; }
    public static SearchCondition Unkeyword(string text) { return new SearchCondition { Field = Fields.Unkeyword, Value = text }; }
    public static SearchCondition Answered() { return new SearchCondition { Field = Fields.Answered }; }
    public static SearchCondition Deleted() { return new SearchCondition { Field = Fields.Deleted }; }
    public static SearchCondition Draft() { return new SearchCondition { Field = Fields.Draft }; }
    public static SearchCondition Flagged() { return new SearchCondition { Field = Fields.Flagged }; }
    public static SearchCondition New() { return new SearchCondition { Field = Fields.New }; }
    public static SearchCondition Old() { return new SearchCondition { Field = Fields.Old }; }
    public static SearchCondition Recent() { return new SearchCondition { Field = Fields.Recent }; }
    public static SearchCondition Seen() { return new SearchCondition { Field = Fields.Seen }; }
    public static SearchCondition Unanswered() { return new SearchCondition { Field = Fields.Unanswered }; }
    public static SearchCondition Undeleted() { return new SearchCondition { Field = Fields.Undeleted }; }
    public static SearchCondition Undraft() { return new SearchCondition { Field = Fields.Undraft }; }
    public static SearchCondition Unflagged() { return new SearchCondition { Field = Fields.Unflagged }; }
    public static SearchCondition Unseen() { return new SearchCondition { Field = Fields.Unseen }; }

    public object Value { get; set; }
    public Fields? Field { get; set; }

    public enum Fields {
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
      if (Field != null) builder.Append(Field.ToString().ToUpper());

      if (Value != null) {
        var value = Value;
        switch (Field) {
          case Fields.BCC:
          case Fields.Body:
          case Fields.From:
          case Fields.Subject:
          case Fields.Text:
          case Fields.To:
            value = Convert.ToString(value).QuoteString();
            break;
        }

        if (value is DateTime) {
          value = ((DateTime)value).GetRFC2060Date().QuoteString();
        }

        if (Field != null) builder.Append(" ");
        builder.Append(value);
      }

      return builder.ToString();
    }
  }
}
