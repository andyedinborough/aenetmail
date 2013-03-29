using AE.Net.Mail;
using AE.Net.Mail.Imap;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Linq;

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
		public void Quoted_Printable() {
			Utilities.DecodeQuotedPrintable("=1");

			var test = "=0D=0A=0D=0A=0D=0A=0D=0A=0D=0A";
			test = Utilities.DecodeQuotedPrintable(test);
			test.ShouldBe("\r\n\r\n\r\n\r\n\r\n");

			test = "H=C3=BAsv=C3=A9ti=20=C3=9Cnnepeket!";
			test = Utilities.DecodeQuotedPrintable(test, System.Text.Encoding.UTF8);
			test.ShouldBe("Húsvéti Ünnepeket!");

			test = Utilities.DecodeWords("coucou =?ISO-8859-1?Q?=E0_tous?=");
			test.ShouldBe("coucou à tous");
			test = Utilities.DecodeWords("=?iso-8859-1?Q?h=E9llo=5Fthere?=");
			test.ShouldBe("héllo_there");

			var invalid = @"=\c";
			test = Utilities.DecodeQuotedPrintable(invalid);
			test.ShouldBe(invalid);

			var msg = GetMessage(quotedPrintable);
			msg.Body.ShouldContain("E-mail Deployment Division");
		}


		void imap_NewMessage(object sender, MessageEventArgs e) {
			var imap = (sender as ImapClient);
			var msg = imap.GetMessage(e.MessageCount - 1);
			Console.WriteLine(msg.Subject);
		}

		[TestMethod]
		public void Parse_Message_From_iPhone() {
			var msg = GetMessage(iphoneMessage);
			msg.Attachments.Count.ShouldBe(1);
			msg.Attachments.All(a => a.GetData().Any().ShouldBe());
			msg.Subject.ShouldBe("Frånvaro: Örebro Golfklubb - Scorecard");
			msg.Body.ShouldContain("Due");

			msg = GetMessage(anotherMessage);
			msg.Body.ShouldContain("Joplin");
		}

		[TestMethod]
		public void Basic_Message() {
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

			msg.From.ShouldBe();
			msg.To.ShouldBe();
			msg.Subject.ShouldBe("DEAR FRIEND");


			msg = GetMessage(bodyNotHeader);
			msg.Body.ShouldNotBeNullOrEmpty();
		}

		[TestMethod]
		public void Basic_Mime_Message() {
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
Content-Disposition: attachment; filename=""test.txt""

this is the attachment text

--XXXXboundary text--");

			msg.From.ShouldBe();
			msg.Attachments.Count.ShouldBe(1);
			msg.Attachments.All(a => a.GetData().Any().ShouldBe());
		}

		[TestMethod]
		public void Nested_Mime_Message() {
			var msg = GetMessage(@"From: John Doe <example@example.com>
MIME-Version: 1.0
Content-Type: multipart/mixed; boundary=""boundary1""

This is a multipart message in MIME format.

--boundary1 
Content-Type: multipart/mixed; boundary=""boundary2""

This is a multipart message in MIME format.

--boundary2
Content-Type: text/plain

this is the body text

--boundary2
Content-Type: text/html

<strong>this is the body text</strong>
--boundary2--

--boundary1
Content-Type: text/html
Content-Disposition: attachment; filename=""test.html""

<strong>this is the body text</strong>

--boundary1--");

			msg.From.ShouldBe();
			msg.Attachments.Count.ShouldBe(1);
			msg.AlternateViews.Count.ShouldBe(2);
			msg.Attachments.All(a => a.GetData().Any().ShouldBe());
		}

		[TestMethod]
		public void Nested_Mime_Message_2() {
			var msg = GetMessage(@"From: John Doe <example@example.com>
MIME-Version: 1.0
Content-Type: multipart/mixed; boundary=""boundary1""

This is a multipart message in MIME format.

--boundary1
Content-Type: text/html
Content-Disposition: attachment; filename=""test.html""

<strong>this is the body text</strong>

--boundary1 
Content-Type: multipart/mixed; boundary=""boundary2""

This is a multipart message in MIME format.

--boundary2
Content-Type: text/plain

this is the body text

--boundary2
Content-Type: text/html

<strong>this is the body text</strong>
--boundary2--

--boundary1--");

			msg.From.ShouldBe();
			msg.Attachments.Count.ShouldBe(1);
			msg.AlternateViews.Count.ShouldBe(2);
			msg.Attachments.All(a => a.GetData().Any().ShouldBe());
		}

		[TestMethod]
		public void Attachment_NameInContentType_ReturnsCorrectFileName() {
			var msg = GetMessage(@"Return-Path: test@domain.com
Delivered-To: test@domain.com
Received: from mail.mailer.domain.com ([194.0.194.158])
	by mail.com
	; Wed, 27 Feb 2013 16:34:12 +0000
Message-ID: <D9CD6D0B-0F5F-42E8-859F-53315F761E49@domain.com>
Received: from TEST11 ([10.2.1.1]) by mailer.domain.com with MailEnable ESMTP; Wed, 27 Feb 2013 16:34:11 +0000
MIME-Version: 1.0
From: ""Digital mail""
 <mail@domain.com>
To: test@test.com,
 test@test.com
Reply-To: test@test.com
Date: 27 Feb 2013 16:34:11 +0000
Subject: Test sbuject
Content-Type: multipart/mixed; boundary=--boundary_0_f0e8cefb-e5b4-4f31-90b9-9d85b3774fc7


----boundary_0_f0e8cefb-e5b4-4f31-90b9-9d85b3774fc7
Content-Type: text/plain; charset=us-ascii
Content-Transfer-Encoding: quoted-printable

<text>
----boundary_0_f0e8cefb-e5b4-4f31-90b9-9d85b3774fc7
Content-Type: application/octet-stream; name=""Filename.pdf""
Content-Transfer-Encoding: base64
Content-Disposition: attachment

<attachment>
----boundary_0_f0e8cefb-e5b4-4f31-90b9-9d85b3774fc7--
");

			msg.Attachments.Count.ShouldBe(1);
			msg.Attachments.First().Filename.ShouldBe("Filename.pdf");
		}

		[TestMethod]
		public void Attachment_FilenameInContentType_ReturnsCorrectFileName() {
			var msg = GetMessage(@"Return-Path: test@domain.com
Delivered-To: test@domain.com
Received: from mail.mailer.domain.com ([194.0.194.158])
	by mail.com
	; Wed, 27 Feb 2013 16:34:12 +0000
Message-ID: <D9CD6D0B-0F5F-42E8-859F-53315F761E49@domain.com>
Received: from TEST11 ([10.2.1.1]) by mailer.domain.com with MailEnable ESMTP; Wed, 27 Feb 2013 16:34:11 +0000
MIME-Version: 1.0
From: ""Digital mail""
 <mail@domain.com>
To: test@test.com,
 test@test.com
Reply-To: test@test.com
Date: 27 Feb 2013 16:34:11 +0000
Subject: Test sbuject
Content-Type: multipart/mixed; boundary=--boundary_0_f0e8cefb-e5b4-4f31-90b9-9d85b3774fc7


----boundary_0_f0e8cefb-e5b4-4f31-90b9-9d85b3774fc7
Content-Type: text/plain; charset=us-ascii
Content-Transfer-Encoding: quoted-printable

<text>
----boundary_0_f0e8cefb-e5b4-4f31-90b9-9d85b3774fc7
Content-Type: application/octet-stream; filename=""Filename.pdf""
Content-Transfer-Encoding: base64
Content-Disposition: attachment

<attachment>
----boundary_0_f0e8cefb-e5b4-4f31-90b9-9d85b3774fc7--
");

			msg.Attachments.Count.ShouldBe(1);
			msg.Attachments.First().Filename.ShouldBe("Filename.pdf");
		}

		private AE.Net.Mail.MailMessage GetMessage(string raw) {
			var msg = new AE.Net.Mail.MailMessage();
			msg.Load(raw, false);

			return msg;
		}

		[TestMethod]
		public void Dont_Die_On_Completely_Invalid_Messages() {
			GetMessage("x");

			GetMessage("\rX\nY");
			GetMessage("\r\rX");
			GetMessage("\n\rX");
			GetMessage("\r\nX");
			GetMessage("\r\n");
			GetMessage("\r\n");
			GetMessage("x\r\ny");
			GetMessage("x");
			GetMessage("");
			GetMessage(null);
		}

		[TestMethod]
		public void Loose_Base64_Encoding() {
			var b64 = "SSBkb24ndCB3YW5uYSB3b3JrLCBJIGp1c3Qgd2Fu\nbmEgYmFuZyBvbiBteSBkcnVtcyBhbGwgZGF5IQ";
			var text = Utilities.DecodeBase64(b64);
			text.ShouldBe("I don't wanna work, I just wanna bang on my drums all day!");
		}

        [TestMethod]
		public void ParsesCorrectly()
        {
            GetMessage(MailWithTextHtmlMultiView);
        }

        [TestMethod]
        public void ParsesCorrectlyWithoutBody()
        {
            var message = GetMessage(DocuMate);

            Assert.AreEqual(1, message.Attachments.Count);

            Assert.AreEqual("file.pdf", message.Attachments.First().Filename);
        }

	    private const string MailWithTextHtmlMultiView = @"Return-Path: testmailaddress+caf_=domaintool=application.com@gmail.com
Delivered-To: user.123@mailtodomain.com
Received: from mxscan0.Mailer.nl ([195.74.65.130])
	by mailtodomain.com
	; Sun, 10 Mar 2013 15:04:29 +0000
Received: from s03.application.com (s03.application.com [195.248.77.90])
	by mxscan0.Mailer.nl (Postfix) with ESMTP id 48CA211735C
	for <user.123@mailtodomain.com>; Sun, 10 Mar 2013 16:04:25 +0100 (CET)
Received: (qmail 6983 invoked by uid 110); 10 Mar 2013 16:04:25 +0100
Delivered-To: 91-domaintool@application.com
Received: (qmail 6978 invoked from network); 10 Mar 2013 16:04:25 +0100
Received: from mail-wi0-f179.google.com (209.85.212.179)
  by s03.application.com with (RC4-SHA encrypted) SMTP; 10 Mar 2013 16:04:25 +0100
Received: by mail-wi0-f179.google.com with SMTP id ez12so477129wid.6
        for <domaintool@application.com>; Sun, 10 Mar 2013 08:04:24 -0700 (PDT)
X-Received: by 10.180.94.135 with SMTP id dc7mr7694160wib.11.1362927864382;
        Sun, 10 Mar 2013 08:04:24 -0700 (PDT)
X-Forwarded-To: domaintool@application.com
X-Forwarded-For: testmailaddress@gmail.com domaintool@application.com
Delivered-To: testmailaddress@gmail.com
Received: by 10.194.152.231 with SMTP id vb7csp16691wjb;
        Sun, 10 Mar 2013 08:04:23 -0700 (PDT)
X-Received: by 10.14.0.135 with SMTP id 7mr26885876eeb.5.1362927863116;
        Sun, 10 Mar 2013 08:04:23 -0700 (PDT)
Received: from mxscan0.Mailer.nl (mxscan0.Mailer.nl. [195.74.65.130])
        by mx.google.com with ESMTP id a1si22070761eez.115.2013.03.10.08.04.22;
        Sun, 10 Mar 2013 08:04:23 -0700 (PDT)
Received-SPF: softfail (google.com: domain of transitioning user@application.com does not designate 185.74.65.130 as permitted sender) client-ip=185.74.65.130;
Authentication-Results: mx.google.com;
       spf=softfail (google.com: domain of transitioning user@application.com does not designate 185.74.65.130 as permitted sender) smtp.mail=user@application.com
Received: from s03.application.com (s03.application.com [195.248.77.90])
	by mxscan0.Mailer.nl (Postfix) with ESMTP id CE2A211735C
	for <testmailaddress@gmail.com>; Sun, 10 Mar 2013 16:04:19 +0100 (CET)
Received: (qmail 6964 invoked by uid 110); 10 Mar 2013 16:04:19 +0100
Delivered-To: 91-testmailaddress@application.com
Received: (qmail 6943 invoked by uid 110); 10 Mar 2013 16:04:19 +0100
Delivered-To: 91-othermailaddress@application.com
Received: (qmail 6919 invoked by uid 48); 10 Mar 2013 16:04:19 +0100
To: othermailaddress@application.com
Subject: Factuur
Date: Sun, 10 Mar 2013 16:04:19 +0100
From: Administratie || user Media <user@application.com>
Reply-to: user@application.com
Message-ID: <c77c8671dbe63ccd55830ce570bf0a32@www.application.com>
X-Priority: 3
X-Mailer: PHPMailer [version 1.72]
MIME-Version: 1.0
Content-Type: multipart/mixed;
	boundary=""b1_c77c8671dbe63ccd55830ce570bf0a32""
X-Mailer-MailScanner: Found to be clean, Found to be clean
X-Mailer-MailScanner-SpamCheck: not spam, SpamAssassin (not cached,
	score=-0.789, required 6, BAYES_00 -1.90, HTML_MESSAGE 0.00,
	HTML_MIME_NO_HTML_TAG 0.38, MIME_HTML_ONLY 0.72,
	T_OBFU_PDF_ATTACH 0.01), not spam, SpamAssassin (cached, score=-0.789,
	required 6, BAYES_00 -1.90, HTML_MESSAGE 0.00,
	HTML_MIME_NO_HTML_TAG 0.38, MIME_HTML_ONLY 0.72,
	T_OBFU_PDF_ATTACH 0.01)
X-Spam-Status: No, No
X-Mailer-MailScanner-Information: Please contact support@Mailer.nl for more information.
X-Mailer-MailScanner-ID: 48CA211735C.AFA71
X-Mailer-MailScanner-From: testmailaddress+caf_=domaintool=application.com@gmail.com


--b1_c77c8671dbe63ccd55830ce570bf0a32
Content-Type: text/html; charset = ""iso-8859-1""; 
Content-Transfer-Encoding: quoted-printable

Body

--b1_c77c8671dbe63ccd55830ce570bf0a32
Content-Type: application/octet-stream; name=""attachment.pdf""
Content-Transfer-Encoding: base64
Content-Disposition: attachment; filename=""attachment.pdf""

Attachment


--b1_c77c8671dbe63ccd55830ce570bf0a32--
";

        private const string DocuMate = @"Return-Path: scanner@domain.com
Delivered-To: user@domain.com
Received: from relay.sub.domain.nl ([64.230.2.11])
	by mailtobasecone.com
	; Wed, 27 Mar 2013 15:21:13 +0000
Received: from [84.233.13.84] (helo=office.domain.com)
	by relay.domain.net with esmtp (Exim 4.80)
	(envelope-from <scanner@domain.com>)
	id 1UKsA1-000843-Ry
	for user@mailtodomain.com; Wed, 27 Mar 2013 16:21:12 +0100
Received: from DocuMate39203F ([192.168.0.40]) by office.domain.com with Microsoft SMTPSVC(6.0.3790.4675);
	 Wed, 27 Mar 2013 16:18:47 +0100
Date: 
From: scanner@domain.nl
To: user@domain.com
Subject: Scan DocuMate
MIME-Version: 1.0
Content-Type: multipart/mixed; boundary=""abcdfwefjsdvsdfg""
Message-ID: <001SERVER1XvpFVjCRG0000002c@office.domain.com>
X-OriginalArrivalTime: 27 Mar 2013 15:18:47.0640 (UTC) FILETIME=[60EB0D80:01CE2AFE]


--abcdfwefjsdvsdfg


--abcdfwefjsdvsdfg
Content-Type: text/plain; charset=us-ascii


=================================================================

--abcdfwefjsdvsdfg
Content-Type: application/octet-stream; name=""file.pdf""
Content-Transfer-Encoding: base64
Content-Disposition: attachment; filename=""file.pdf""

DATA

--abcdfwefjsdvsdfg--


";
	}
}
