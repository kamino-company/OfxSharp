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
            var ofxDocument = parser.Import(new FileStream(@"sulcredi-signon-request.ofx", FileMode.Open));

            Assert.IsNotNull(ofxDocument);
            Assert.IsNotNull(ofxDocument.SignOn);
            Assert.AreEqual("273", ofxDocument.Account.BankId);
            Assert.AreEqual("12345-6", ofxDocument.Account.AccountId);
            Assert.AreEqual(2, ofxDocument.Transactions.Count());
            Assert.AreEqual(1500m, ofxDocument.Transactions.First().Amount);
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
