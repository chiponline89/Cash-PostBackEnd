using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.Portal.Areas.ServiceRequest.Models
{
    public class Request
    {
        private string _CustomerCode = string.Empty;

        public string CustomerCode
        {
            get { return _CustomerCode; }
            set { _CustomerCode = value; }
        }
        private string _GroupCustomerCode = string.Empty;

        public string GroupCustomerCode
        {
            get { return _GroupCustomerCode; }
            set { _GroupCustomerCode = value; }
        }
        private string _SenderName = string.Empty;

        public string SenderName
        {
            get { return _SenderName; }
            set { _SenderName = value; }
        }
        private string _SenderAddr = string.Empty;

        public string SenderAddr
        {
            get { return _SenderAddr; }
            set { _SenderAddr = value; }
        }
        private string _SenderTel = string.Empty;

        public string SenderTel
        {
            get { return _SenderTel; }
            set { _SenderTel = value; }
        }
        private string _SenderEmail = string.Empty;

        public string SenderEmail
        {
            get { return _SenderEmail; }
            set { _SenderEmail = value; }
        }
        private string _SenderPostCode = string.Empty;

        public string SenderPostCode
        {
            get { return _SenderPostCode; }
            set { _SenderPostCode = value; }
        }
        private string _SenderTaxCode = string.Empty;

        public string SenderTaxCode
        {
            get { return _SenderTaxCode; }
            set { _SenderTaxCode = value; }
        }
        private string _SenderIdentification = string.Empty;

        public string SenderIdentification
        {
            get { return _SenderIdentification; }
            set { _SenderIdentification = value; }
        }
        private string _ReceiverName = string.Empty;

        public string ReceiverName
        {
            get { return _ReceiverName; }
            set { _ReceiverName = value; }
        }
        private string _ReceiverAddr = string.Empty;

        public string ReceiverAddr
        {
            get { return _ReceiverAddr; }
            set { _ReceiverAddr = value; }
        }
        private string _ReceiverCountry = string.Empty;

        public string ReceiverCountry
        {
            get { return _ReceiverCountry; }
            set { _ReceiverCountry = value; }
        }
        private string _ReceiverProvince = string.Empty;

        public string ReceiverProvince
        {
            get { return _ReceiverProvince; }
            set { _ReceiverProvince = value; }
        }
        private string _ReceiverDistrict = string.Empty;

        public string ReceiverDistrict
        {
            get { return _ReceiverDistrict; }
            set { _ReceiverDistrict = value; }
        }
        private string _ReceiverCounty = string.Empty;

        public string ReceiverCounty
        {
            get { return _ReceiverCounty; }
            set { _ReceiverCounty = value; }
        }
        private string _ReceiverMobile = string.Empty;

        public string ReceiverMobile
        {
            get { return _ReceiverMobile; }
            set { _ReceiverMobile = value; }
        }
        private string _ReceiverEmail = string.Empty;

        public string ReceiverEmail
        {
            get { return _ReceiverEmail; }
            set { _ReceiverEmail = value; }
        }
        private string _ContactName = string.Empty;

        public string ContactName
        {
            get { return _ContactName; }
            set { _ContactName = value; }
        }
        private string _ReceiverPostCode = string.Empty;

        public string ReceiverPostCode
        {
            get { return _ReceiverPostCode; }
            set { _ReceiverPostCode = value; }
        }
        private string _ReceiverTaxCode = string.Empty;

        public string ReceiverTaxCode
        {
            get { return _ReceiverTaxCode; }
            set { _ReceiverTaxCode = value; }
        }
        private string _ReceiverIdentify = string.Empty;

        public string ReceiverIdentify
        {
            get { return _ReceiverIdentify; }
            set { _ReceiverIdentify = value; }
        }
        private string _Code = string.Empty;

        public string Code
        {
            get { return _Code; }
            set { _Code = value; }
        }
        private string _DocumentCode = string.Empty;

        public string DocumentCode
        {
            get { return _DocumentCode; }
            set { _DocumentCode = value; }
        }
        private string _EventPackage = string.Empty;

        public string EventPackage
        {
            get { return _EventPackage; }
            set { _EventPackage = value; }
        }
        private string _EGroup = string.Empty;

        public string EGroup
        {
            get { return _EGroup; }
            set { _EGroup = value; }
        }
        private string _DHLCustomer = string.Empty;

        public string DHLCustomer
        {
            get { return _DHLCustomer; }
            set { _DHLCustomer = value; }
        }
        private string _BankAccount = string.Empty;

        public string BankAccount
        {
            get { return _BankAccount; }
            set { _BankAccount = value; }
        }
        private string _FarArea = string.Empty;

        public string FarArea
        {
            get { return _FarArea; }
            set { _FarArea = value; }
        }
        private string _AirTransport = string.Empty;

        public string AirTransport
        {
            get { return _AirTransport; }
            set { _AirTransport = value; }
        }
        private string _TaskNumber = string.Empty;

        public string TaskNumber
        {
            get { return _TaskNumber; }
            set { _TaskNumber = value; }
        }
        private string _PlusBill = string.Empty;

        public string PlusBill
        {
            get { return _PlusBill; }
            set { _PlusBill = value; }
        }
        private string _OtherDocAttach = string.Empty;

        public string OtherDocAttach
        {
            get { return _OtherDocAttach; }
            set { _OtherDocAttach = value; }
        }
        private string _OtherDoc = string.Empty;

        public string OtherDoc
        {
            get { return _OtherDoc; }
            set { _OtherDoc = value; }
        }
        private string _TypePackage = string.Empty;

        public string TypePackage
        {
            get { return _TypePackage; }
            set { _TypePackage = value; }
        }
        private string _Content = string.Empty;

        public string Content
        {
            get { return _Content; }
            set { _Content = value; }
        }
        private string _NotDeliveriedInstuct = string.Empty;

        public string NotDeliveriedInstuct
        {
            get { return _NotDeliveriedInstuct; }
            set { _NotDeliveriedInstuct = value; }
        }
        private string _ReturnedDay = string.Empty;

        public string ReturnedDay
        {
            get { return _ReturnedDay; }
            set { _ReturnedDay = value; }
        }
        private decimal _Weight = 0;

        public decimal Weight
        {
            get { return _Weight; }
            set { _Weight = value; }
        }
        private decimal _Long = 0;

        public decimal Long
        {
            get { return _Long; }
            set { _Long = value; }
        }
        private decimal _Height = 0;

        public decimal Height
        {
            get { return _Height; }
            set { _Height = value; }
        }
        private decimal _Width = 0;

        public decimal Width
        {
            get { return _Width; }
            set { _Width = value; }
        }
        private string _NoFee = string.Empty;

        public string NoFee
        {
            get { return _NoFee; }
            set { _NoFee = value; }
        }
        private string _Debit = string.Empty;

        public string Debit
        {
            get { return _Debit; }
            set { _Debit = value; }
        }
        private string _ExportBill = string.Empty;

        public string ExportBill
        {
            get { return _ExportBill; }
            set { _ExportBill = value; }
        }
        private string _AciontCombine = string.Empty;

        public string AciontCombine
        {
            get { return _AciontCombine; }
            set { _AciontCombine = value; }
        }
        private string _DeliveryAnnounce = string.Empty;

        public string DeliveryAnnounce
        {
            get { return _DeliveryAnnounce; }
            set { _DeliveryAnnounce = value; }
        }
        private string _EmailAnnounce = string.Empty;

        public string EmailAnnounce
        {
            get { return _EmailAnnounce; }
            set { _EmailAnnounce = value; }
        }
        private string _SmsAnnounce = string.Empty;

        public string SmsAnnounce
        {
            get { return _SmsAnnounce; }
            set { _SmsAnnounce = value; }
        }
        private string _HandDelivery = string.Empty;

        public string HandDelivery
        {
            get { return _HandDelivery; }
            set { _HandDelivery = value; }
        }
        private string _COD = string.Empty;

        public string COD
        {
            get { return _COD; }
            set { _COD = value; }
        }
        private decimal _AmountCOD = 0;

        public decimal AmountCOD
        {
            get { return _AmountCOD; }
            set { _AmountCOD = value; }
        }
        private string _DeliveriedPayer = string.Empty;

        public string DeliveriedPayer
        {
            get { return _DeliveriedPayer; }
            set { _DeliveriedPayer = value; }
        }
        private string _CollectPayer = string.Empty;

        public string CollectPayer
        {
            get { return _CollectPayer; }
            set { _CollectPayer = value; }
        }
        private string _MoneyTypePay = string.Empty;

        public string MoneyTypePay
        {
            get { return _MoneyTypePay; }
            set { _MoneyTypePay = value; }
        }
        private string _PayAtPos = string.Empty;

        public string PayAtPos
        {
            get { return _PayAtPos; }
            set { _PayAtPos = value; }
        }
        private string _AccNumber = string.Empty;

        public string AccNumber
        {
            get { return _AccNumber; }
            set { _AccNumber = value; }
        }
        private string _BankName = string.Empty;

        public string BankName
        {
            get { return _BankName; }
            set { _BankName = value; }
        }
        private string _BankBranch = string.Empty;

        public string BankBranch
        {
            get { return _BankBranch; }
            set { _BankBranch = value; }
        }
        private string _MoveFeeBank = string.Empty;

        public string MoveFeeBank
        {
            get { return _MoveFeeBank; }
            set { _MoveFeeBank = value; }
        }
        private string _CollectForGroup = string.Empty;

        public string CollectForGroup
        {
            get { return _CollectForGroup; }
            set { _CollectForGroup = value; }
        }
    }
    public class RequestNE
    {
        private string _CustomerCode = string.Empty;

        public string CustomerCode
        {
            get { return _CustomerCode; }
            set { _CustomerCode = value; }
        }
        private string _GroupCustomerCode = string.Empty;

        public string GroupCustomerCode
        {
            get { return _GroupCustomerCode; }
            set { _GroupCustomerCode = value; }
        }
        private string _SenderName = string.Empty;

        public string SenderName
        {
            get { return _SenderName; }
            set { _SenderName = value; }
        }
        private string _SenderAddr = string.Empty;

        public string SenderAddr
        {
            get { return _SenderAddr; }
            set { _SenderAddr = value; }
        }
        private string _SenderTel = string.Empty;

        public string SenderTel
        {
            get { return _SenderTel; }
            set { _SenderTel = value; }
        }
        private string _SenderEmail = string.Empty;

        public string SenderEmail
        {
            get { return _SenderEmail; }
            set { _SenderEmail = value; }
        }
        private string _SenderPostCode = string.Empty;

        public string SenderPostCode
        {
            get { return _SenderPostCode; }
            set { _SenderPostCode = value; }
        }
        private string _SenderTaxCode = string.Empty;

        public string SenderTaxCode
        {
            get { return _SenderTaxCode; }
            set { _SenderTaxCode = value; }
        }
        private string _SenderIdentification = string.Empty;

        public string SenderIdentification
        {
            get { return _SenderIdentification; }
            set { _SenderIdentification = value; }
        }
        private string _ReceiverName = string.Empty;

        public string ReceiverName
        {
            get { return _ReceiverName; }
            set { _ReceiverName = value; }
        }
        private string _ReceiverAddr = string.Empty;

        public string ReceiverAddr
        {
            get { return _ReceiverAddr; }
            set { _ReceiverAddr = value; }
        }
        private string _ReceiverCountry = string.Empty;

        public string ReceiverCountry
        {
            get { return _ReceiverCountry; }
            set { _ReceiverCountry = value; }
        }
        private string _ReceiverProvince = string.Empty;

        public string ReceiverProvince
        {
            get { return _ReceiverProvince; }
            set { _ReceiverProvince = value; }
        }
        private string _ReceiverDistrict = string.Empty;

        public string ReceiverDistrict
        {
            get { return _ReceiverDistrict; }
            set { _ReceiverDistrict = value; }
        }
        private string _ReceiverCounty = string.Empty;

        public string ReceiverCounty
        {
            get { return _ReceiverCounty; }
            set { _ReceiverCounty = value; }
        }
        private string _ReceiverMobile = string.Empty;

        public string ReceiverMobile
        {
            get { return _ReceiverMobile; }
            set { _ReceiverMobile = value; }
        }
        private string _ReceiverEmail = string.Empty;

        public string ReceiverEmail
        {
            get { return _ReceiverEmail; }
            set { _ReceiverEmail = value; }
        }
        private string _ContactName = string.Empty;

        public string ContactName
        {
            get { return _ContactName; }
            set { _ContactName = value; }
        }
        private string _ReceiverPostCode = string.Empty;

        public string ReceiverPostCode
        {
            get { return _ReceiverPostCode; }
            set { _ReceiverPostCode = value; }
        }
        private string _ReceiverTaxCode = string.Empty;

        public string ReceiverTaxCode
        {
            get { return _ReceiverTaxCode; }
            set { _ReceiverTaxCode = value; }
        }
        private string _ReceiverIdentify = string.Empty;

        public string ReceiverIdentify
        {
            get { return _ReceiverIdentify; }
            set { _ReceiverIdentify = value; }
        }
        private string _Code = string.Empty;

        public string Code
        {
            get { return _Code; }
            set { _Code = value; }
        }
        private string _DocumentCode = string.Empty;

        public string DocumentCode
        {
            get { return _DocumentCode; }
            set { _DocumentCode = value; }
        }
        private string _EventPackage = string.Empty;

        public string EventPackage
        {
            get { return _EventPackage; }
            set { _EventPackage = value; }
        }
        private string _EGroup = string.Empty;

        public string EGroup
        {
            get { return _EGroup; }
            set { _EGroup = value; }
        }
        private string _DHLCustomer = string.Empty;

        public string DHLCustomer
        {
            get { return _DHLCustomer; }
            set { _DHLCustomer = value; }
        }
        private string _BankAccount = string.Empty;

        public string BankAccount
        {
            get { return _BankAccount; }
            set { _BankAccount = value; }
        }
        private string _FarArea = string.Empty;

        public string FarArea
        {
            get { return _FarArea; }
            set { _FarArea = value; }
        }
        private string _AirTransport = string.Empty;

        public string AirTransport
        {
            get { return _AirTransport; }
            set { _AirTransport = value; }
        }
        private string _TaskNumber = string.Empty;

        public string TaskNumber
        {
            get { return _TaskNumber; }
            set { _TaskNumber = value; }
        }
        private string _PlusBill = string.Empty;

        public string PlusBill
        {
            get { return _PlusBill; }
            set { _PlusBill = value; }
        }
        private string _OtherDocAttach = string.Empty;

        public string OtherDocAttach
        {
            get { return _OtherDocAttach; }
            set { _OtherDocAttach = value; }
        }
        private string _OtherDoc = string.Empty;

        public string OtherDoc
        {
            get { return _OtherDoc; }
            set { _OtherDoc = value; }
        }
        private string _TypePackage = string.Empty;

        public string TypePackage
        {
            get { return _TypePackage; }
            set { _TypePackage = value; }
        }
        private string _Content = string.Empty;

        public string Content
        {
            get { return _Content; }
            set { _Content = value; }
        }
        private string _NotDeliveriedInstuct = string.Empty;

        public string NotDeliveriedInstuct
        {
            get { return _NotDeliveriedInstuct; }
            set { _NotDeliveriedInstuct = value; }
        }
        private string _ReturnedDay = string.Empty;

        public string ReturnedDay
        {
            get { return _ReturnedDay; }
            set { _ReturnedDay = value; }
        }
        private decimal _Weight = 0;

        public decimal Weight
        {
            get { return _Weight; }
            set { _Weight = value; }
        }
        private decimal _Long = 0;

        public decimal Long
        {
            get { return _Long; }
            set { _Long = value; }
        }
        private decimal _Height = 0;

        public decimal Height
        {
            get { return _Height; }
            set { _Height = value; }
        }
        private decimal _Width = 0;

        public decimal Width
        {
            get { return _Width; }
            set { _Width = value; }
        }
        private string _NoFee = string.Empty;

        public string NoFee
        {
            get { return _NoFee; }
            set { _NoFee = value; }
        }
        private string _Debit = string.Empty;

        public string Debit
        {
            get { return _Debit; }
            set { _Debit = value; }
        }
        private string _ExportBill = string.Empty;

        public string ExportBill
        {
            get { return _ExportBill; }
            set { _ExportBill = value; }
        }
        private string _AciontCombine = string.Empty;

        public string AciontCombine
        {
            get { return _AciontCombine; }
            set { _AciontCombine = value; }
        }
        private string _DeliveryAnnounce = string.Empty;

        public string DeliveryAnnounce
        {
            get { return _DeliveryAnnounce; }
            set { _DeliveryAnnounce = value; }
        }
        private string _EmailAnnounce = string.Empty;

        public string EmailAnnounce
        {
            get { return _EmailAnnounce; }
            set { _EmailAnnounce = value; }
        }
        private string _SmsAnnounce = string.Empty;

        public string SmsAnnounce
        {
            get { return _SmsAnnounce; }
            set { _SmsAnnounce = value; }
        }
        private string _HandDelivery = string.Empty;

        public string HandDelivery
        {
            get { return _HandDelivery; }
            set { _HandDelivery = value; }
        }
        private string _COD = string.Empty;

        public string COD
        {
            get { return _COD; }
            set { _COD = value; }
        }
        private decimal _AmountCOD = 0;

        public decimal AmountCOD
        {
            get { return _AmountCOD; }
            set { _AmountCOD = value; }
        }
        private string _DeliveriedPayer = string.Empty;

        public string DeliveriedPayer
        {
            get { return _DeliveriedPayer; }
            set { _DeliveriedPayer = value; }
        }
        private string _CollectPayer = string.Empty;

        public string CollectPayer
        {
            get { return _CollectPayer; }
            set { _CollectPayer = value; }
        }
        private string _MoneyTypePay = string.Empty;

        public string MoneyTypePay
        {
            get { return _MoneyTypePay; }
            set { _MoneyTypePay = value; }
        }
        private string _PayAtPos = string.Empty;

        public string PayAtPos
        {
            get { return _PayAtPos; }
            set { _PayAtPos = value; }
        }
        private string _AccNumber = string.Empty;

        public string AccNumber
        {
            get { return _AccNumber; }
            set { _AccNumber = value; }
        }
        private string _BankName = string.Empty;

        public string BankName
        {
            get { return _BankName; }
            set { _BankName = value; }
        }
        private string _BankBranch = string.Empty;

        public string BankBranch
        {
            get { return _BankBranch; }
            set { _BankBranch = value; }
        }
        private string _MoveFeeBank = string.Empty;

        public string MoveFeeBank
        {
            get { return _MoveFeeBank; }
            set { _MoveFeeBank = value; }
        }
        private string _CollectForGroup = string.Empty;

        public string CollectForGroup
        {
            get { return _CollectForGroup; }
            set { _CollectForGroup = value; }
        }
    }
    public class RequireInfo
    {
        private string _CustomerCode = string.Empty;

        public string CustomerCode
        {
            get { return _CustomerCode; }
            set { _CustomerCode = value; }
        }
        private string _BillCode = string.Empty;

        public string BillCode
        {
            get { return _BillCode; }
            set { _BillCode = value; }
        }
         
        private decimal _stt = 0;

        public decimal STT
        {
            get { return _stt; }
            set { _stt = value; }
        }
        private string _ReceiverAddr = string.Empty;

        public string ReceiverAddr
        {
            get { return _ReceiverAddr; }
            set { _ReceiverAddr = value; }
        }
        private decimal _Quantity = 0;

        public decimal Quantity
        {
            get { return _Quantity; }
            set { _Quantity = value; }
        }
        private string _ProductCode = string.Empty;

        public string ProductCode
        {
            get { return _ProductCode; }
            set { _ProductCode = value; }
        }
        private string _Description = string.Empty;

        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }
        private decimal _Amount = 0;

        public decimal Amount
        {
            get { return _Amount; }
            set { _Amount = value; }
        }
        private string _ReceiverMobile = string.Empty;

        public string ReceiverMobile
        {
            get { return _ReceiverMobile; }
            set { _ReceiverMobile = value; }
        }
         
        private string _Code = string.Empty;

        public string Code
        {
            get { return _Code; }
            set { _Code = value; }
        }
         
         
        private string _Content = string.Empty;

        public string Content
        {
            get { return _Content; }
            set { _Content = value; }
        }
        
        private decimal _Weight = 0;

        public decimal Weight
        {
            get { return _Weight; }
            set { _Weight = value; }
        }
         
        private decimal _AmountCOD = 0;

        public decimal AmountCOD
        {
            get { return _AmountCOD; }
            set { _AmountCOD = value; }
        }
         
    }
}