using System.ComponentModel;

namespace OfxSharpLib
{
    public enum OfxTransactionType
    {
        [Description("Unknown type")]
        Unknown,
        [Description("Basic Credit")]
        CREDIT,
        [Description("Basic Debit")]
        DEBIT,
        [Description("Interest")]
        INT,
        [Description("Dividend")]
        DIV,
        [Description("Fee")]
        FEE,
        [Description("Service Charge")]
        SRVCHG,
        [Description("Deposit")]
        DEP,
        [Description("ATM transfer")]
        ATM,
        [Description("Point of Sale transfer")]
        POS,
        [Description("Transfer")]
        XFER,
        [Description("Check")]
        CHECK,
        [Description("Payment")]
        PAYMENT,
        [Description("Cash Withdrawl")]
        CASH,
        [Description("Direct Deposit")]
        DIRECTDEP,
        [Description("Merchant Initiated Debit")]
        DIRECTDEBIT,
        [Description("Repeating Payment")]
        REPEATPMT,
        OTHER,
        IN,
        OUT,
        Invoice, //Iugu Credit
        WithdrawRequest //Iugu Debit
    }
}
