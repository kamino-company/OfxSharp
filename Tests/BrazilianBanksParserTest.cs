using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OfxSharpLib;

namespace OFXSharp.Tests
{
    [TestFixture]
    public class BrazilianBanksParserTest
    {
        [Test]
        public void CanParseItau()
        {
            var parser = new OfxDocumentParser();
            var ofxDocument = parser.Import(new FileStream(@"itau.ofx", FileMode.Open));

            Assert.AreEqual(ofxDocument.Account.AccountId, "9999 99999-9");
            Assert.AreEqual(ofxDocument.Account.BankId, "0341");

            Assert.AreEqual(3, ofxDocument.Transactions.Count());
            CollectionAssert.AreEqual(ofxDocument.Transactions.Select(x => x.Memo.Trim()).ToList(), new[] { "RSHOP", "REND PAGO APLIC AUT MAIS", "SISDEB" });
        }

        [Test]
        public void CanParseSantander()
        {
            var parser = new OfxDocumentParser();
            var ofxDocument = parser.Import(new FileStream(@"santander.ofx", FileMode.Open));

            Assert.IsNotNull(ofxDocument);
        }

        // Regression: Santander serializes amounts >= R$ 1,000,000 using '.' as thousand separator
        // and ',' as decimal (e.g. "6000.000,00"). The original parser did a naive Replace(",", ".")
        // which produced three dots and threw FormatException, breaking OFX import for affected clients.
        // See N3-5888.
        [Test]
        public void CanParseSantanderWithThousandsSeparator()
        {
            var parser = new OfxDocumentParser();
            var ofxDocument = parser.Import(new FileStream(@"santander-thousands-separator.ofx", FileMode.Open));

            Assert.IsNotNull(ofxDocument);
            Assert.AreEqual(4, ofxDocument.Transactions.Count());

            var transactions = ofxDocument.Transactions.ToList();
            Assert.AreEqual(-2.48m, transactions[0].Amount);
            Assert.AreEqual(6000000m, transactions[1].Amount);
            Assert.AreEqual(-5800000m, transactions[2].Amount);
            Assert.AreEqual(-152541.14m, transactions[3].Amount);
        }

        [Test]
        public void CanParseOfxWithSignOnRequestVariant()
        {
            var parser = new OfxDocumentParser();
            using var stream = new FileStream(@"sulcredi-signon-request.ofx", FileMode.Open);
            var ofxDocument = parser.Import(stream);

            Assert.IsNotNull(ofxDocument);
            Assert.IsNotNull(ofxDocument.SignOn);
            Assert.AreEqual(0, ofxDocument.SignOn.StatusCode);
            Assert.AreEqual("POR", ofxDocument.SignOn.Language);
            Assert.AreEqual(new System.DateTime(2026, 5, 1), ofxDocument.SignOn.DtServer);
            Assert.AreEqual("273", ofxDocument.Account.BankId);
            Assert.AreEqual("12345-6", ofxDocument.Account.AccountId);
            Assert.AreEqual(2, ofxDocument.Transactions.Count());
            Assert.AreEqual(1500m, ofxDocument.Transactions.First().Amount);
            Assert.AreEqual(-250m, ofxDocument.Transactions.Last().Amount);
        }

        [Test]
        public void SonrsTakesPrecedenceOverSonrq()
        {
            // When both SONRS and SONRQ are present, SONRS (response) wins via ?? operator
            const string ofxXml = @"<OFX>
  <SIGNONMSGSRSV1><SONRS>
    <STATUS><CODE>1</CODE><SEVERITY>INFO</SEVERITY></STATUS>
    <DTSERVER>20260501120000</DTSERVER><LANGUAGE>ENG</LANGUAGE>
  </SONRS></SIGNONMSGSRSV1>
  <SIGNONMSGSRQV1><SONRQ>
    <STATUS><CODE>0</CODE><SEVERITY>INFO</SEVERITY></STATUS>
    <DTSERVER>20260501120000</DTSERVER><LANGUAGE>POR</LANGUAGE>
  </SONRQ></SIGNONMSGSRQV1>
  <BANKMSGSRSV1><STMTTRNRS><TRNUID>1</TRNUID>
    <STATUS><CODE>0</CODE><SEVERITY>INFO</SEVERITY></STATUS>
    <STMTRS><CURDEF>BRL</CURDEF>
      <BANKACCTFROM><BANKID>999</BANKID><ACCTID>12345</ACCTID><ACCTTYPE>CHECKING</ACCTTYPE></BANKACCTFROM>
      <BANKTRANLIST><DTSTART>20260401</DTSTART><DTEND>20260430</DTEND></BANKTRANLIST>
      <LEDGERBAL><BALAMT>1000.00</BALAMT><DTASOF>20260430</DTASOF></LEDGERBAL>
    </STMTRS></STMTTRNRS></BANKMSGSRSV1>
</OFX>";
            var parser = new OfxDocumentParser();
            var ofxDocument = parser.Import(ofxXml);

            Assert.AreEqual(1, ofxDocument.SignOn.StatusCode);
            Assert.AreEqual("ENG", ofxDocument.SignOn.Language);
        }

        [Test]
        public void ThrowsWhenSignOnNotPresent()
        {
            const string ofxXml = @"<OFX>
  <BANKMSGSRSV1><STMTTRNRS><TRNUID>1</TRNUID>
    <STATUS><CODE>0</CODE><SEVERITY>INFO</SEVERITY></STATUS>
    <STMTRS><CURDEF>BRL</CURDEF>
      <BANKACCTFROM><BANKID>999</BANKID><ACCTID>12345</ACCTID><ACCTTYPE>CHECKING</ACCTTYPE></BANKACCTFROM>
      <BANKTRANLIST><DTSTART>20260401</DTSTART><DTEND>20260430</DTEND></BANKTRANLIST>
      <LEDGERBAL><BALAMT>1000.00</BALAMT><DTASOF>20260430</DTASOF></LEDGERBAL>
    </STMTRS></STMTTRNRS></BANKMSGSRSV1>
</OFX>";
            var parser = new OfxDocumentParser();
            Assert.Throws<OfxParseException>(() => parser.Import(ofxXml));
        }

        [Test]
        public void CanParseBancoDoBrasil()
        {
            var parser = new OfxDocumentParser();
            var ofxDocument = parser.Import(new FileStream(@"bb.ofx", FileMode.Open), Encoding.GetEncoding("ISO-8859-1"));

            Assert.AreEqual(ofxDocument.Account.AccountId, "99999-9");
            Assert.AreEqual(ofxDocument.Account.BranchId, "9999-9");
            Assert.AreEqual(ofxDocument.Account.BankId, "1");

            Assert.AreEqual(3, ofxDocument.Transactions.Count());
            CollectionAssert.AreEqual(ofxDocument.Transactions.Select(x => x.Memo.Trim()).ToList(), new[] { "Transferência Agendada", "Compra com Cartão", "Saque" });
            
            Assert.IsNotNull(ofxDocument);
        }
    }
}
