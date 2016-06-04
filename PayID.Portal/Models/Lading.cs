using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.Portal.Models
{
    public class InsertLadingBills
    {
        public string Code { get; set; }
        public string Weight { get; set; }
        public string Quantity { get; set; }
        public string FromName { get; set; }
        public string FromAddress { get; set; }
        public string FromMobile { get; set; }
        public string ToName { get; set; }
        public string ToAddress { get; set; }
        public string ToMobile { get; set; }
        public string CustomerCode { get; set; }
        public string Value { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string Type { get; set; }
        public string ServiceCode { get; set; }
        public string TransportCode { get; set; }
        public string FromProvinceCode { get; set; }
        public string ToProvinceCode { get; set; }
        public string FromDistrictCode { get; set; }
        public string ToDistrictCode { get; set; }
        public string ToWardCode { get; set; }
        public string FileNo { get; set; }
        public string FileCode { get; set; }

    }
    public class UpdateLadingBills
    {
        public int idd { get; set; }
        public string ProductName { get; set; }
        public Int32 Value { get; set; }
        public Int32 Weight { get; set; }
        public Int32 Quantity { get; set; }
        public string DescriptProduct { get; set; }
        public string Name { get; set; }
        public string ToPostCode { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }

        /// <summary>
        /// 20140825-Append by Hoang Anh
        /// </summary>
        public string ProductName_Old { get; set; }
        public Int32 Value_Old { get; set; }
        public Int32 Weight_Old { get; set; }
        public Int32 Quantity_Old { get; set; }
        public string DescriptProduct_Old { get; set; }
        public string Name_Old { get; set; }
        public string ToPostCode_Old { get; set; }
        public string Mobile_Old { get; set; }
        public string Address_Old { get; set; }
    }

    public class Return
    {
        public string Code { get; set; }
        public double MainFee { get; set; }
        public double CodeFee { get; set; }
        public double ServiceFee { get; set; }
        public double TotalFee { get; set; }
        public string response_code { get; set; }
        public string response_message { get; set; }
    }

    public class Lading
    {
        public string _id { get; set; }
        public long Id { get; set; }
        public string Code { get; set; }
        public long Weight { get; set; }
        public int Quantity { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string SenderName { get; set; }
        public string SenderAddress { get; set; }
        public string SenderMobile { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverAddress { get; set; }
        public string ReceiverMobile { get; set; }
        public string CustomerCode { get; set; }
        public string StoreCode { get; set; }
        public long Value { get; set; }
        public long MainFee { get; set; }
        public long ServiceFee { get; set; }
        public long CodFee { get; set; }
        public long TotalFee { get; set; }
        public long CollectValue { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public int Delete { get; set; }
        public string FileCode { get; set; }
        public string FileNo { get; set; }
        public string OriginCountry { get; set; }
        public string FromProvinceCode { get; set; }
        public string FromProvinceName{get;set;}
        public string ToProvinceCode { get; set; }
        public string ToProvinceName { get; set; }
        public string FromDistrictCode { get; set; }
        public string FromDistrictName { get; set; }
        public string ToDistrictCode { get; set; }
        public string ToDistrictName { get; set; }
        public string ToWardCode { get; set; }
        public string ServiceCode { get; set; }
        public string TransportCode { get; set; }
        public DateTime DateCreated { get; set; }
        public string PackagingType { get; set; }
        public string PickupMethod { get; set; }
        public DateTime system_created_time { get; set; }
        public string PostCodeLink { get; set; }
        public string Height { get; set; }
        public string Channel { get; set; }
        public bool IsConsorShip { get; set; }
        public bool Check { get; set; }
        public string PartnerCode { get; set; }
        public string UserCreate { get; set; }
        public string UnitCreate { get; set; }

    }

    public class business_profile
    {
        public string general_code { get; set; }
        public string general_email { get; set; }
        public string general_system { get; set; }
        public string general_full_name { get; set; }
        public string general_account_type { get; set; }
        public bool system_is_active { get; set; }

        public string business_tax { get; set; }
        public string business_website { get; set; }
        public string contact_name { get; set; }
        public string contact_address_address { get; set; }
        public string contact_address_district { get; set; }

        public string contact_address_province { get; set; }
        public string contact_phone_mobile { get; set; }
        public string contact_phone_work { get; set; }
        public string contact_phone_fax { get; set; }
        public string legacy_no { get; set; }
        public string legacy_issued_by { get; set; }
        public DateTime? legacy_issued_date { get; set; }

        public string fee_method { get; set; }//s24
        public string cod_method { get; set; }//s24
        public string reference_code { get; set; }//s24

        public string settlement_channel { get; set; }
        public int settlement_scheme_process_window { get; set; }
        public int settlement_scheme_settle_window { get; set; }
        public int settlement_scheme_transaction_expire_window { get; set; }

        public DateTime system_last_updated_time { get; set; }
        public string system_last_updated_by { get; set; }
        public string system_historical_notes { get; set; }

    }
    public class Response
    {
        public string ErrorCode { get; set; }
        public string ErrorMsg { get; set; }
        public string Code { get; set; }
    }
    public class RegistrationResponse
    {
        public string ErrorCode { get; set; }
        public string ErrorMsg { get; set; }

        public string MerchantCode { get; set; }
        public int MerchantId { get; set; }
    }
    //public class Store
    //{
    //    public long _id { get; set; }
    //    public string StoreName { get; set; }
    //    public string ManagerName { get; set; }
    //    public string ManagerMobile { get; set; }
    //    public string ManagerEmail { get; set; }
    //    public int UserId { get; set; }
    //    public string ProvinceCode { get; set; }
    //    public string DistrictCode { get; set; }
    //    public string Address { get; set; }
    //    public string Description { get; set; }
    //    public int Default { get; set; }
    //    public DateTime system_last_updated_time { get; set; }
    //    public string system_historical_notes { get; set; }
    //}
    public class MailLadingBills
    {
        public string Code { get; set; }
        public decimal CuocCod { get; set; }
        public string Address { get; set; }
        public string Mobile { get; set; }
        public string AddressRe { get; set; }
        public decimal Value { get; set; }
        public string ProductDescription { get; set; }
        public decimal CuocTong { get; set; }

    }

    public class LogLadingBills
    {
        public string Code { get; set; }
        public string Weight { get; set; }
        public string Type { get; set; }
        public string Quantity { get; set; }
        public string Sender_name { get; set; }
        public string Sender_address { get; set; }
        public string Sender_mobile { get; set; }

        public string ServiceCode { get; set; }
        public string Receiver_name { get; set; }
        public string Receiver_address { get; set; }
        public string Receiver_mobile { get; set; }
        public string CustomerCode { get; set; }
        public string Value { get; set; }
        public string FromPostCode { get; set; }
        public string ToPostCode { get; set; }
        public DateTime DateCreated { get; set; }
        public string Productdescr { get; set; }
        public string NameProduct { get; set; }
        public string UserCreate { get; set; }
        public string FromDistrictCode { get; set; }
        public string ToDistrictCode { get; set; }
        public string FromWardCode { get; set; }
        public string ToWardCode { get; set; }
    }
}