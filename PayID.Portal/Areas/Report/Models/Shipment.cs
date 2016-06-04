using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.Portal.Areas.Report.Models
{
    public class Shipment
    {
        string _CustomerCode = string.Empty;

        public string CustomerCode
        {
            get { return _CustomerCode; }
            set { _CustomerCode = value; }
        }
        string _CustomerName = string.Empty;

        public string CustomerName
        {
            get { return _CustomerName; }
            set { _CustomerName = value; }
        }
      
        string _TrackingCode = string.Empty;

        public string TrackingCode
        {
            get { return _TrackingCode; }
            set { _TrackingCode = value; }
        }
        string _ReceiverInfo = string.Empty;

        public string ReceiverInfo
        {
            get { return _ReceiverInfo; }
            set { _ReceiverInfo = value; }
        }
    
        decimal _ProductVal = 0;

        public decimal ProductVal
        {
            get { return _ProductVal; }
            set { _ProductVal = value; }
        }
        string _Productdes = string.Empty;

        public string Productdes
        {
            get { return _Productdes; }
            set { _Productdes = value; }
        }

        decimal _MainFee = 0;

        public decimal MainFee
        {
            get { return _MainFee; }
            set { _MainFee = value; }
        }

        decimal _ServiceFee = 0;

        public decimal ServiceFee
        {
            get { return _ServiceFee; }
            set { _ServiceFee = value; }
        }
        decimal _CodFee = 0;

        public decimal CodFee
        {
            get { return _CodFee; }
            set { _CodFee = value; }
        }
        decimal _TotalFee = 0;

        public decimal TotalFee
        {
            get { return _TotalFee; }
            set { _TotalFee = value; }
        }
        string _Status = string.Empty;

        decimal _Quantity = 0;

        public decimal Quantity
        {
            get { return _Quantity; }
            set { _Quantity = value; }
        }
        decimal _Weight = 0;

        public decimal Weight
        {
            get { return _Weight; }
            set { _Weight = value; }
        }
        decimal _collectvalue = 0;

        public decimal collectvalue
        {
            get { return _collectvalue; }
            set { _collectvalue = value; }
        }
        decimal _TongFee = 0;

        public decimal TongFee
        {
            get { return _TongFee; }
            set { _TongFee = value; }
        }
        decimal _TongTien = 0;

        public decimal TongTien
        {
            get { return _TongTien; }
            set { _TongTien = value; }
        }
        public string Status
        {
            get { return _Status; }
            set { _Status = value; }
        }
        string _ToProvince = string.Empty;
        public string ToProvince
        {
            get { return _ToProvince; }
            set { _ToProvince = value; }
        }
        string _OrderId = string.Empty;

        public string OrderId
        {
            get { return _OrderId; }
            set { _OrderId = value; }
        }
        string _Service = string.Empty;

        public string Service
        {
            get { return _Service; }
            set { _Service = value; }
        }
        decimal _STT = 0;

        public decimal STT
        {
            get { return _STT; }
            set { _STT = value; }
        }
        string _province = string.Empty;

        public string province
        {
            get { return _province; }
            set { _province = value; }
        }
        string _district = string.Empty;

        public string district
        {
            get { return _district; }
            set { _district = value; }
        }
        string _pos = string.Empty;

        public string pos
        {
            get { return _pos; }
            set { _pos = value; }
        }
    }
}