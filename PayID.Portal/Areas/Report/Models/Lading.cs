using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.Portal.Areas.Report.Models
{
    public class Lading
    {

        string _ladingcode = string.Empty;

        public string LadingCode
        {
            get { return _ladingcode; }
            set { _ladingcode = value; }
        }
        string _servicetype = string.Empty;

        public string ServiceType
        {
            get { return _servicetype; }
            set { _servicetype = value; }
        }
        string _CreatedDate = string.Empty;

        public string CreatedDate
        {
            get { return _CreatedDate; }
            set { _CreatedDate = value; }
        }

        decimal _main_fee = 0;
        
        public decimal MainFee
        {
            get { return _main_fee; }
            set { _main_fee = value; }
        }

        decimal _cod_fee = 0;

        public decimal CodFee
        {
            get { return _cod_fee; }
            set { _cod_fee = value; }
        }

        decimal _service_fee = 0;

        public decimal ServiceFee
        {
            get { return _service_fee; }
            set { _service_fee = value; }
        }

        decimal _quantity = 0;

        public decimal Quantity
        {
            get { return _quantity; }
            set { _quantity = value; }
        }

        decimal _amount = 0;

        public decimal Amount
        {
            get { return _amount; }
            set { _amount = value; }
        }

        decimal _totalfee = 0;

        public decimal TotalFee
        {
            get { return _totalfee; }
            set { _totalfee = value; }
        }

        decimal _totalAmount = 0;

        public decimal TotalAmount
        {
            get { return _totalAmount; }
            set { _totalAmount = value; }
        }
        string _billcode = string.Empty;

        public string BillCode
        {
            get { return _billcode; }
            set { _billcode = value; }
        }

        string _time = string.Empty;

        public string Time
        {
            get { return _time; }
            set { _time = value; }
        }

        string _paycode = string.Empty;

        public string PayCode
        {
            get { return _paycode; }
            set { _paycode = value; }
        }

        string _groupcode = string.Empty;

        public string GroupCode
        {
            get { return _groupcode; }
            set { _groupcode = value; }
        }

        string _servicecode = string.Empty;

        public string ServiceCode
        {
            get { return _servicecode; }
            set { _servicecode = value; }
        }
        string _status = string.Empty;

        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }
        string _customercode = string.Empty;

        public string CustomerCode
        {
            get { return _customercode; }
            set { _customercode = value; }
        }

    }

}