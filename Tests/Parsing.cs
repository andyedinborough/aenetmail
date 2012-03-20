using System;
using System.Linq;
using AE.Net.Mail;
using AE.Net.Mail.Imap;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Should.Fluent;

namespace Tests {
  /// <summary>
  /// Summary description for UnitTest1
  /// </summary>
  [TestClass]
  public class Parsing {
    public TestContext TestContext { get; set; }

    #region messages
    string bodyNotHeader = @"Delivered-To: yyy@gmail.com
Received: by 10.142.174.3 with SMTP id w3cs50740wfe;
        Mon, 14 Nov 2011 18:10:08 -0800 (PST)
Return-Path: <zzz@gmail.com>
Received-SPF: pass (google.com: domain of zzz@gmail.com designates 10.227.208.71 as permitted sender) client-ip=10.227.208.71;
Authentication-Results: mr.google.com; spf=pass (google.com: domain of zzz@gmail.com designates 10.227.208.71 as permitted sender) smtp.mail=zzz@gmail.com; dkim=pass header.i=zzz@gmail.com
Received: from mr.google.com ([10.227.208.71])
        by 10.227.208.71 with SMTP id gb7mr16473303wbb.7.1321323006082 (num_hops = 1);
        Mon, 14 Nov 2011 18:10:06 -0800 (PST)
DKIM-Signature: v=1; a=rsa-sha256; c=relaxed/relaxed;
        d=gmail.com; s=gamma;
        h=mime-version:from:date:message-id:subject:to:content-type;
        bh=3ajdGhBv88zJknw0EVGu6lJhm0zz+4eRVot/EGmYTOs=;
        b=nxqtHAr0o4/76BCnJVbxXCL0NWiABD9o1ijDXpJaNIJ19+ParWNzEtbTf9xiFMtoDI
        kufMoypwCxokbNJRXxmiuXnWSBvQ2UhNqwnIYvr2YxXpj+nOIEZOXmoj2S3DF0PM7Qif
        MuSMSi3f4Jmcscmi6KNeP4wCcmqF564fccGhw=
Received: by 10.227.208.71 with SMTP id gb7mr16473303wbb.7.1321323006076; Mon, 14 Nov 2011 18:10:06 -0800 (PST)
MIME-Version: 1.0
Received: by 10.227.200.65 with HTTP; Mon, 14 Nov 2011 18:09:45 -0800 (PST)
From: Drew Peterson <zzz@gmail.com>
Date: Mon, 14 Nov 2011 20:09:45 -0600
Message-ID: <redacted>
Subject: test2
To: yyy@gmail.com
Content-Type: text/plain; charset=UTF-8

Test message body";

    string anotherMessage = @"+OK 2536 octets
Received: from ([216.32.180.11]) for <edinboroughs@trac.ky> with MailEnable Catch-All Filter; Tue, 25 Oct 2011 00:19:25 -0700
Received: from VA3EHSOBE008.bigfish.com ([216.32.180.11]) by mail.vortaloptics.com with MailEnable ESMTP; Tue, 25 Oct 2011 00:19:25 -0700
Received: from mail181-va3-R.bigfish.com (10.7.14.244) by
 VA3EHSOBE008.bigfish.com (10.7.40.28) with Microsoft SMTP Server id
 14.1.225.22; Tue, 25 Oct 2011 07:19:20 +0000
Received: from mail181-va3 (localhost.localdomain [127.0.0.1])	by
 mail181-va3-R.bigfish.com (Postfix) with ESMTP id CFE1B19381C5	for
 <edinboroughs@trac.ky>; Tue, 25 Oct 2011 07:19:22 +0000 (UTC)
X-SpamScore: 0
X-BigFish: VS0(zzzz1202hzzz31h87h2a8h668h839h944h61h)
X-Spam-TCS-SCL: 0:0
X-Forefront-Antispam-Report: CIP:65.55.171.153;KIP:(null);UIP:(null);IPVD:NLI;H:VA3DIAHUB054.RED001.local;RD:smtp801.microsoftonline.com;EFVD:NLI
X-FB-SS: 0,
Received-SPF: softfail (mail181-va3: transitioning domain of vortaloptics.com does not designate 65.55.171.153 as permitted sender) client-ip=65.55.171.153; envelope-from=aedinborough@vortaloptics.com; helo=VA3DIAHUB054.RED001.local ;RED001.local ;
X-FB-DOMAIN-IP-MATCH: fail
Received: from mail181-va3 (localhost.localdomain [127.0.0.1]) by mail181-va3
 (MessageSwitch) id 1319527162708761_17303; Tue, 25 Oct 2011 07:19:22 +0000
 (UTC)
Received: from VA3EHSMHS029.bigfish.com (unknown [10.7.14.242])	by
 mail181-va3.bigfish.com (Postfix) with ESMTP id 9DEF66B0053	for
 <edinboroughs@trac.ky>; Tue, 25 Oct 2011 07:19:22 +0000 (UTC)
Received: from VA3DIAHUB054.RED001.local (65.55.171.153) by
 VA3EHSMHS029.bigfish.com (10.7.99.39) with Microsoft SMTP Server (TLS) id
 14.1.225.22; Tue, 25 Oct 2011 07:19:19 +0000
Received: from VA3DIAXVS171.RED001.local ([172.18.2.196]) by
 VA3DIAHUB054.RED001.local ([10.8.230.53]) with mapi; Tue, 25 Oct 2011
 00:19:24 -0700
From: Andy Edinborough <aedinborough@vortaloptics.com>
To: ""edinboroughs@trac.ky"" <edinboroughs@trac.ky>
Date: Tue, 25 Oct 2011 00:19:21 -0700
Subject: Send a card to Lori Bryant
Thread-Topic: Send a card to Lori Bryant
Thread-Index: AcyS5mu9bWF9L2CbQ6KDFH0zybjbqw==
Message-ID: <8F7372BC-5DC9-4939-AFB0-4FD1D041064E@vortaloptics.com>
Accept-Language: en-US
Content-Language: en-US
X-MS-Has-Attach:
X-MS-TNEF-Correlator:
acceptlanguage: en-US
Content-Type: text/plain; charset=""us-ascii""
Content-Transfer-Encoding: base64
MIME-Version: 1.0
Return-Path: <aedinborough@vortaloptics.com>

MTYxOSBOLiBCbGFjayBDYXQgUmQuLCBKb3BsaW4sIE1PIDY0ODAxDQoNCg0K


";

    string iphoneMessage = @"+OK 159354 octets
Received: from ([99.34.8.150]) for <edinboroughs@trac.ky> with MailEnable Catch-All Filter; Wed, 13 Jul 2011 09:25:50 -0700
Received: from main.edinborough.org ([99.34.8.150]) by mail.vortaloptics.com with MailEnable ESMTP; Wed, 13 Jul 2011 09:25:49 -0700
Received: from [192.168.1.140] ([192.168.1.140])
	by main.edinborough.org
	; Wed, 13 Jul 2011 11:29:40 -0500
Subject: =?utf-8?B?RnLDpW52YXJvOiDDlnJlYnJvIEdvbGZrbHViYiAtIFNjb3JlY2FyZA==?=
From: Andy Edinborough <andy@edinborough.org>
Content-Type: multipart/mixed; boundary=Apple-Mail-1--592579169
Message-Id: <AE28825C-F702-438E-8132-AD36D89792C8@edinborough.org>
Date: Wed, 13 Jul 2011 11:29:46 -0500
To: ""edinboroughs@trac.ky"" <edinboroughs@trac.ky>
Content-Transfer-Encoding: 7bit
Mime-Version: 1.0 (iPhone Mail 8J2)
X-Mailer: iPhone Mail (8J2)
Return-Path: <andy@edinborough.org>


--Apple-Mail-1--592579169
Content-Transfer-Encoding: 7bit
Content-Type: text/plain;
	charset=us-ascii

Due: tomorrow


--Apple-Mail-1--592579169
Content-Disposition: inline;
	filename=photo.JPG
Content-Type: image/jpeg;
	name=photo.JPG
Content-Transfer-Encoding: base64

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";

    private static string quotedPrintable = @"Delivered-To: em-ca-bruceg@em.ca
Received: (qmail 3001 invoked from network); 1 Oct 2011 20:25:25 -0000
Received: from [213.144.209.223] (174-144-63-255.pools.spcsdns.net [174.144.63.255])
  by churchill.factcomp.com ([24.89.90.248])
  with SMTP via TCP; 01 Oct 2011 20:25:25 -0000
Received: from [213.144.209.223][127.0.0.1] by [213.144.209.223][127.0.0.1]
  (SMTPD32); Sat, 1 Oct 2011 13:28:09 -0700
Message-ID: <cce9e468d7a9d0b0dd1047da001effd7@email.com>
From: ""Business Division"" <BusinessDivision84@email.com>
To: <bruceg@em.ca>
Subject: October Newsletter
Date: Sat, 1 Oct 2011 13:27:52 -0700
MIME-Version: 1.0
Content-Type: text/plain;
	charset=""windows-1252""
Content-Transfer-Encoding: quoted-printable
Content-Length: 1091

Do you own or manage a business that could use new customers & more revenue?

Using our cutting edge turnkey e-mail marketing programs we can generate =
massive =09
traffic resulting in conversions.=20
=09
We have access to 200 million consumers and 45 million businesses in the =
USA =09
- enabling you to target specific audience segments and send custom =
messages.=09
=09
** Get More Information by Calling 1 (800) 281-8610 **=09

This is What You Will Get:=09

> Our Creative Team Designs your Custom HTML AD=09
> Find Your Best Customers with Our Targeted Database=09
> We Schedule and Send out the E-mails to The Database You Choose=09
> Your Custom AD Will Direct Traffic To Your Web Site and/or Telephones=09
> You Will Be Able to Track Detailed Click-Thru's & Open Rates=09
> Within 72 Hours Customers Will Contact You To Purchase=09
> Email Credits Can Broken Up As You Need=09
> We are ""A"" Rated with the BBB=09
=09
For more information on how to advertise with our company contact us =
directly at 1 (800) 281-8610.=09

Sincerely,

E-mail Deployment Division
1 (800) 281-8610
";
    #endregion

    [TestMethod]
    public void TestQuotedPrintable() {
      var test = "=0D=0A=0D=0A=0D=0A=0D=0A=0D=0A";
      test = Utilities.DecodeQuotedPrintable(test);
      test.Should().Equal("\r\n\r\n\r\n\r\n\r\n");

      test = Utilities.DecodeWords("coucou =?ISO-8859-1?Q?=E0_tous?=");
      test.Should().Equal("coucou à tous");
      test = Utilities.DecodeWords("=?iso-8859-1?Q?h=E9llo=5Fthere?=");
      test.Should().Equal("héllo_there");

      var msg = GetMessage(quotedPrintable);
      msg.Body.Should().Contain("E-mail Deployment Division");
    }


    void imap_NewMessage(object sender, MessageEventArgs e) {
      var imap = (sender as ImapClient);
      var msg = imap.GetMessage(e.MessageCount - 1);
      Console.WriteLine(msg.Subject);
    }

    [TestMethod]
    public void TestParseMessageFromIPhone() {
      var msg = GetMessage(iphoneMessage);
      msg.Attachments.Count.Should().Equal(2);
      msg.Attachments.All(a => a.GetData().Any().Should().Be.True());
      msg.Subject.Should().Equal("Frånvaro: Örebro Golfklubb - Scorecard");
      msg.Body.Should().Contain("Due");

      msg = GetMessage(anotherMessage);
      msg.Body.Should().Contain("Joplin");
    }
     
    [TestMethod]
    public void TestBasicMessage() {
      var msg = GetMessage(@"From: test@localhost
To: root@localhost
Subject: DEAR FRIEND

THROUGH THE COURTESY OF BUSINESS OPPORTUNITY, I TAKE LIBERTY ANCHORED ON A
STRONG DESIRE TO SOLICIT YOUR ASSISTANCE ON THIS MUTUALLY BENEFICIAL AND
RISKFREE TRANSACTION WHICH I HOPE YOU WILL GIVE YOUR URGENT ATTENTION.

I HAVE DEPOSITED THE SUM OF THIRTY MILLION,FIVE HUNDRED THOUSAND UNITED
STATES DOLLARS(US$30,500,000) WITH A SECURITY COMPANY FOR SAFEKEEPING.
THE FUNDS ARE SECURITY CODED TO PREVENT THEM FROM KNOWING THE ACTUAL
CONTENTS.

MAY I AT THIS POINT EMPHASIZE THE HIGH LEVEL OF CONFIDENTIALLITY WHICH THIS
BUSINESS DEMANDS AND HOPE YOU WILL NOT BETRAY THE TRUST AND CONFIDENCE WHICH
WE REPOSE IN YOU.");

      msg.From.Should().Not.Be.Null();
      msg.To.Should().Not.Be.Null();
      msg.Subject.Should().Equal("DEAR FRIEND");


      msg = GetMessage(bodyNotHeader);
      msg.Body.Should().Not.Be.NullOrEmpty();
    }

    [TestMethod]
    public void TestBasicMimeMessage() {
      var msg = GetMessage(@"From: John Doe <example@example.com>
MIME-Version: 1.0
Content-Type: multipart/mixed;
        boundary=""XXXXboundary text""

This is a multipart message in MIME format.

--XXXXboundary text 
Content-Type: text/plain

this is the body text

--XXXXboundary text 
Content-Type: text/plain;
Content-Disposition: attachment;
        filename=""test.txt""

this is the attachment text

--XXXXboundary text--");

      msg.From.Should().Not.Be.Null();
      msg.Attachments.Count.Should().Equal(2);
      msg.Attachments.All(a => a.GetData().Any().Should().Be.True());
    }

    private AE.Net.Mail.MailMessage GetMessage(string raw) {
      var msg = new AE.Net.Mail.MailMessage();
      msg.Load(raw, false);

      return msg;
    }
  }
}
