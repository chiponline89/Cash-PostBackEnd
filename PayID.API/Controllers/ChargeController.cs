using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PayID.DataHelper;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using MongoDB.Bson;
using System.Dynamic;
namespace PayID.API.Controllers
{
    public class ChargeController : ApiController
    {
        public class Charges
        {
            public double CodFee { get; set; }
            public double MainFee { get; set; }
            public double TotalFee { get; set; }
            public double ServiceFee { get; set; }
        }
        public Charges[] GetCalculatorFee(string ServicesCode, double Value, string FromPostCode, string ToPostCode, int Weight, string CustomerCode)
        {
            ProxyController px = new ProxyController();
            dynamic dynamicObj = new DynamicObj();
            dynamicObj = px.processChargesInfo(Weight, Value, ServicesCode, FromPostCode, ToPostCode);
            dynamicObj = dynamicObj.Output[0];
            Charges charges = new Charges();
            charges.CodFee = double.Parse(dynamicObj.CodCharges.ToString());
            charges.MainFee = double.Parse(dynamicObj.MainCharges.ToString());
            charges.ServiceFee = double.Parse(dynamicObj.PlusServiceCharges.ToString());
            //ICaculatorCOD ca = new CaculatorCOD();
            //Charges[] mang = ca.CalutaltorFee(ServicesCode, Value, FromPostCode, "VN", ToPostCode, "VN", Weight, 0, CustomerCode);
            Charges[] myArray = { charges };
            return myArray;
        }

        [AcceptVerbs("POST")]
        //     public JObject GetQueryPostage(string ServicesCode, long Value, string FromProvinceCode, string ToProvinceCode, int Weight)
        public JObject QueryPostage(JObject request)
        {
            dynamic _request = new DynamicObj(
              MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(JsonConvert.SerializeObject(request)));
            string _json_response = String.Empty;
            dynamic myobject = new ExpandoObject();
            dynamic mydynamic = new DynamicObj();
            // dynamic myobject = new JObject();          
            ProxyController px = new ProxyController();
            try
            {
                int v_weight = Convert.ToInt32(_request.Weight);
                double v_value = (!String.IsNullOrEmpty(_request.Value)) ? Convert.ToDouble(_request.Value) : 0;
                // Tính cước theo WS của VNP
                mydynamic = px.processChargesInfo(v_weight, v_value, _request.ServiceCode.ToString(), _request.FromProvinceCode.ToString(), _request.ToProvinceCode.ToString());
                mydynamic = mydynamic.Output[0];
                myobject.MainFee = long.Parse(mydynamic.MainCharges.ToString());
                myobject.ServiceFee = long.Parse(mydynamic.PlusServiceCharges.ToString());
                myobject.CodFee = long.Parse(mydynamic.CodCharges.ToString());
            }
            catch
            {
                #region Tính cước theo bảng cước của hệ thống
                long v_value = (!String.IsNullOrEmpty(_request.Value)) ? Convert.ToInt64(_request.Value) : 0;
                long v_weight = Convert.ToInt64(_request.Weight);
                CalculateRate myCal = new CalculateRate();
                dynamic[] FeeArray = myCal.CalutaltorFee(_request.ServiceCode, v_value, _request.FromProvinceCode, "VN", _request.ToProvinceCode, "VN", v_weight, 0);
                myobject.MainFee = FeeArray[0].MainFee;
                myobject.CodFee = FeeArray[0].CodFee;
                myobject.ServiceFee = FeeArray[0].ServiceFee;
                #endregion
            }
            _json_response = JsonConvert.SerializeObject(myobject);
            return JObject.Parse(_json_response);


        }

    }
}
