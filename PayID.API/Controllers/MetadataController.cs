using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MongoDB.Driver.Builders;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Dynamic;
using System.Web.Mvc;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

using MongoDB.Driver.Linq;

namespace PayID.API.Controllers
{
    public class MetadataController : Controller
    {
        public void InsertAutoGenCode(int _id, string Code, string ProvinceCode, string CustomerCode)
        {
            try
            {
              
                dynamic myAutoGenCode = new PayID.DataHelper.DynamicObj();                
                myAutoGenCode._id = _id;
                myAutoGenCode.Code = Code;
                myAutoGenCode.ProvinceCode = ProvinceCode;
                myAutoGenCode.CustomerCode = CustomerCode;
                PayID.Portal.Areas.Lading.Configuration.Data.Insert("AutoGenCode", myAutoGenCode);              
             
            }
            catch (Exception ex)
            {
                //return false;
            }
        }
        public string CreateAutoGenCode(string CustomerCode, int type, string ProvinceCode)
        {
            //select SUBSTRING('123456789',0,2)  --> 1
            //select SUBSTRING('123456789',1,2)  --> 12
            //select SUBSTRING('123456789',2,3)  --> 234
            //string aa, bb, cc = "";
            //string str = "123456789";
            //aa = str.Substring(0, 2);
            //bb = str.Substring(1, 2);
            //cc = str.Substring(2, 3);

            int @Ma_E3_4 = 1;
            string string34 = @Ma_E3_4.ToString("00");

            string @ma_dv = "", @Ma_Code = "", @Ma_E1 = "", @sNumber = "";
            string @Ma_E1_1 = "", @Ma_E1_2 = "", @Ma_E1_3 = "", @Ma_E1_4 = "";
            string @Ma_E1_5 = "", @Ma_E1_6 = "", @Ma_E1_7 = "", @Ma_E1_8 = "", @Ma_E1_9 = "";
            string @Ma_E1_10 = "", @Ma_E1_11 = "", @Ma_E1_12 = "", @Ma_E1_13 = "";
            int @Tong = 0, @P = 0;

           
            //Lấy đầu EE trong EE123456789VN
           dynamic myObj = PayID.Portal.Areas.Merchant.Configuration.Data.Get("AutoGenPreCode", Query.EQ("Type", type));           
           @Ma_Code = myObj.PreCode;

          //  MongoDB.Driver.MongoCollection<dynamic> requestCollectionAutoGenPreCode = pay .GetCollection<dynamic>("AutoGenPreCode");
            //@Ma_Code = (from c in myObj
            //            where c.Type == type
            //            select c.PreCode).FirstOrDefault();


           dynamic[] lstAutogenCode = PayID.Portal.Areas.Lading.Configuration.Data.List("AutoGenCode", null);           

            // Kiểm tra tồn tại của mã
           if (lstAutogenCode.Length > 0)
            {
                //@Ma_E1 = (from c in lstAutogenCode select c.Code).ToArray().FirstOrDefault();
                @Ma_E1 =(from c in lstAutogenCode
                 orderby c._id descending
                 select c.Code).FirstOrDefault();

                @Ma_E1_12 = "V";
                @Ma_E1_13 = "N";

                @Ma_E1_1 = @Ma_Code.Substring(0, 1);
                @Ma_E1_2 = @Ma_Code.Substring(1, 1);
                @Ma_E1_11 = @Ma_E1.Substring(10, 1);
                //Set @sNumber = convert(varchar(8),convert(int,substring(@Ma_E1,3,8)) + 1)
                //Select  convert(varchar(8),convert(int,substring('EE441189063VN',3,8)) + 1)  --> 44118907
                //Select  convert(varchar(8),convert(int,substring('EE441189063VN',3,8)) )  --> 44118906


                @sNumber = (int.Parse(@Ma_E1.Substring(2, 8)) + 1).ToString();

                if (@sNumber.Length < 8)
                {
                    @sNumber = "0" + @sNumber;

                }
                @Ma_E1_3 = @sNumber.Substring(0, 1);
                @Ma_E1_4 = @sNumber.Substring(1, 1);
                if (@ma_dv.Length == 3)
                {
                    @Ma_E1_5 = @ma_dv.Substring(2, 1);
                }
                else
                {
                    @Ma_E1_5 = sNumber.Substring(2, 1);
                    @Ma_E1_6 = @sNumber.Substring(3, 1);
                    @Ma_E1_7 = @sNumber.Substring(4, 1);
                    @Ma_E1_8 = @sNumber.Substring(5, 1);
                    @Ma_E1_9 = @sNumber.Substring(6, 1);
                    @Ma_E1_10 = @sNumber.Substring(7, 1);
                }
                if ((@Ma_E1_3 == "9") && (@Ma_E1_4 == "9") && (@Ma_E1_5 == "9") && (@Ma_E1_6 == "9") && (@Ma_E1_7 == "9") && (@Ma_E1_8 == "9") && (@Ma_E1_9 == "9") && (@Ma_E1_10 == "9"))
                {
                    //@Ma_E1_3 = @ma_dv.Substring(0, 1);
                    //@Ma_E1_4 = @ma_dv.Substring(1, 1);
                    @Ma_E1_3 = "1";
                    @Ma_E1_4 = "0";
                    if (@ma_dv.Length == 3)
                    {
                        @Ma_E1_5 = @ma_dv.Substring(2, 1);
                    }

                    else
                    {
                        @Ma_E1_5 = "0";
                        @Ma_E1_6 = "0";
                        @Ma_E1_7 = "0";
                        @Ma_E1_8 = "0";
                        @Ma_E1_9 = "0";
                        @Ma_E1_10 = "0";
                    }
                }

                @Tong = int.Parse(@Ma_E1_3) * 8 + int.Parse(@Ma_E1_4) * 6 + int.Parse(@Ma_E1_5) * 4 + int.Parse(@Ma_E1_6) * 2;

                @Tong = @Tong + int.Parse(@Ma_E1_7) * 3 + int.Parse(@Ma_E1_8) * 5 + int.Parse(@Ma_E1_9) * 9 + int.Parse(@Ma_E1_10) * 7;
                @P = @Tong % 11;
                @P = 11 - @P;
                if (@P == 10)
                {
                    @P = 0;
                }
                if (@P == 11)
                {
                    @P = 5;
                }
                @Ma_E1 = @Ma_E1_1 + @Ma_E1_2 + @Ma_E1_3 + @Ma_E1_4 + @Ma_E1_5 + @Ma_E1_6 + @Ma_E1_7;
                @Ma_E1 = @Ma_E1 + @Ma_E1_8 + @Ma_E1_9 + @Ma_E1_10 + @P.ToString() + @Ma_E1_12 + @Ma_E1_13;

                InsertAutoGenCode(int.Parse(PayID.Portal.Areas.Lading.Configuration.Data.GetNextSquence("AutoGenCode").ToString()), @Ma_E1, ProvinceCode, CustomerCode);


            }
            else
            {

                @Ma_E1_1 = @Ma_Code.Substring(0, 1);
                @Ma_E1_2 = @Ma_Code.Substring(1, 1);
                //@Ma_E1_3 = @ma_dv.Substring(0, 1);
                //@Ma_E1_4 = @ma_dv.Substring(1, 1);

                @Ma_E1_3 = "1";
                @Ma_E1_4 = "0";

                if (@ma_dv.Length == 3)
                {
                    @Ma_E1_5 = @ma_dv.Substring(2, 1);
                }
                else
                {
                    @Ma_E1_5 = "0";
                    @Ma_E1_6 = "0";
                    @Ma_E1_7 = "0";
                    @Ma_E1_8 = "0";
                    @Ma_E1_9 = "0";
                    @Ma_E1_10 = "0";
                    @Ma_E1_11 = "0";

                    // set @Tong = convert(int,@Ma_E1_3)*8 + convert(int,@Ma_E1_4)*6 + convert(int,@Ma_E1_5)*4 + convert(int,@Ma_E1_6)*2
                    //set @Tong = @Tong + convert(int,@Ma_E1_7)*3 + convert(int,@Ma_E1_8)*5 + convert(int,@Ma_E1_9)*9 + convert(int,@Ma_E1_10)*7

                    @Tong = (int.Parse(@Ma_E1_3)) * 8 + (int.Parse(@Ma_E1_4)) * 6 + int.Parse(@Ma_E1_5) * 4 + int.Parse(@Ma_E1_6) * 2;
                    @Tong = @Tong + int.Parse(@Ma_E1_7) * 3 + int.Parse(@Ma_E1_8) * 5 + int.Parse(@Ma_E1_9) * 9 + int.Parse(@Ma_E1_10) * 7;


                    @P = @Tong % 11;
                    @P = 11 - @P;
                    if (@P == 10)
                    {
                        @P = 0;
                    }
                    if (@P == 11)
                    {
                        @P = 5;
                    }
                    @Ma_E1 = @Ma_E1_1 + @Ma_E1_2 + @Ma_E1_3 + @Ma_E1_4 + @Ma_E1_5 + @Ma_E1_6 + @Ma_E1_7;
                    @Ma_E1 = @Ma_E1 + @Ma_E1_8 + @Ma_E1_9 + @Ma_E1_10 + @P.ToString() + @Ma_E1_12 + @Ma_E1_13;
                    InsertAutoGenCode(int.Parse(PayID.Portal.Areas.Lading.Configuration.Data.GetNextSquence("AutoGenCode").ToString()), @Ma_E1, ProvinceCode, CustomerCode);

                }
            }
            return @Ma_E1;
        }

    }
}
