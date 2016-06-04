using PayID.DataHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;
using MongoDB.Driver;
using System.Data;
namespace PayID.API.Controllers
{
    public class ProxyController : Controller
    {
        public DynamicObj processChargesInfo(int weight, double amount, string service_type, string po_sender_code, string po_receiver_code)
        {
            string json_string = "";
            dynamic myDynamicCharges = new DynamicObj();
            svcLading.PaycodeSoapClient ladingsvc = new svcLading.PaycodeSoapClient();
            DataSet ds = new DataSet();
            try
            {
                ds = ladingsvc.ProccessChargesInfo(weight, amount, service_type, po_sender_code, po_receiver_code);
            }
            catch { ds = null; }
            //ds = null;
            if (ds != null)
            {
                //System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                json_string = JsonConvert.SerializeObject(ds, Newtonsoft.Json.Formatting.Indented);
                myDynamicCharges = new DynamicObj(MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(json_string));
            }
            else
            {
                myDynamicCharges.CodFee = 0;
                myDynamicCharges.ServiceFee = 0;
                myDynamicCharges.MainFee = 0;
            }

            return myDynamicCharges;
        }

        
        public static void InsertShipmentOracle(dynamic shipment)
        {
            #region "save to oracle"
            dynamic _oraShipment = new DataHelper.DynamicObj();
            _oraShipment._id = shipment._id.ToString();
            _oraShipment.tracking_code = shipment.tracking_code.ToString();
            try
            {
                _oraShipment.order_id = shipment.order_id.ToString();
                _oraShipment.request_id = shipment.request_id.ToString();
            }
            catch { }
            try { 
            _oraShipment.from_address_id = shipment.from_address._id.ToString();
            _oraShipment.from_address_name = shipment.from_address.name.ToString();
            _oraShipment.from_address_address = shipment.from_address.address.ToString();
            _oraShipment.from_address_district = int.Parse("0" + shipment.from_address.district.ToString());
            _oraShipment.from_address_province = int.Parse("0" + shipment.from_address.province.ToString());
            _oraShipment.from_address_ward = int.Parse("0" + shipment.from_address.ward.ToString());
            _oraShipment.from_address_email = shipment.from_address.email.ToString();
            _oraShipment.from_address_phone = shipment.from_address.phone.ToString();
            }
            catch { }
                try { 
            _oraShipment.to_address_id = shipment.to_address._id.ToString();
            _oraShipment.to_address_name = shipment.to_address.name.ToString();
            _oraShipment.to_address_address = shipment.to_address.address.ToString();
            _oraShipment.to_address_district = int.Parse("0" + shipment.to_address.district.ToString());
            _oraShipment.to_address_province = int.Parse("0" + shipment.to_address.province.ToString());
            _oraShipment.to_address_ward = int.Parse("0" + shipment.to_address.ward.ToString());
            _oraShipment.to_address_email = shipment.to_address.email.ToString();
            _oraShipment.to_address_phone = shipment.to_address.phone.ToString();
                    }catch{}
                    try { 
            _oraShipment.product_name = shipment.product.name.ToString();
            _oraShipment.product_description = shipment.product.description.ToString();
            _oraShipment.product_value = decimal.Parse("0" + shipment.product.value.ToString());
            _oraShipment.product_id = shipment.product.id.ToString();
                        }catch{}
                        try { 
            _oraShipment.service_cashpost_service = int.Parse("0" + shipment.service.cashpost_service.ToString());
            _oraShipment.service_shipping_main_service = int.Parse("0" + shipment.service.shipping_main_service.ToString());
            _oraShipment.service_shipping_add_service = int.Parse("0" + shipment.service.shipping_add_service.ToString());
                            }catch{}
                            try { 
            _oraShipment.customer_code = shipment.customer.code.ToString();
            _oraShipment.customer_name = shipment.customer.full_name.ToString();
            _oraShipment.customer_email = shipment.customer.email.ToString();
            _oraShipment.customer_mobile = shipment.customer.mobile.ToString();
                                }catch{}
                                try { 
            _oraShipment.parcel_height = decimal.Parse("0" + shipment.parcel.height.ToString());
            _oraShipment.parcel_length = decimal.Parse("0" + shipment.parcel.length.ToString());
            _oraShipment.parcel_weight = decimal.Parse("0" + shipment.parcel.weight.ToString());
            _oraShipment.parcel_width = decimal.Parse("0" + shipment.parcel.width.ToString());
                                    }catch{}
                                    try { 
            _oraShipment.system_status = shipment.system_status.ToString();
            _oraShipment.current_assigned = shipment.current_assigned.ToString();
            _oraShipment.created_by = shipment.created_by.ToString();
            _oraShipment.created_at = shipment.created_at.ToString();
            _oraShipment.current_assigned_name = shipment.current_assigned_name.ToString();
                                    }catch{}
            
            //Common.OracleData.Insert("shipment", _oraShipment);

            //foreach (dynamic assign in shipment.assigned_to)
            //{
            //    dynamic _assigned = new DynamicObj();
            //    _assigned.shipment_id = shipment._id.ToString();
            //    _assigned.assigned_to_id = assign.assign_to_id.ToString();
            //    _assigned.assigned_to_full_name = assign.assign_to_full_name.ToString();
            //    _assigned.assigned_at = assign.assign_at.ToString();
            //    _assigned.assigned_by = assign.assign_by.ToString();
            //    _assigned._id = shipment._id + assign.assign_at.ToString();
            //    Common.OracleData.Insert("shipment_assign", _assigned);
            //}

            //foreach (dynamic comment in shipment.comments)
            //{
            //    dynamic _comment = new DynamicObj();
            //    _comment.shipment_id = shipment._id.ToString();
            //    _comment.comment_content = comment.comment.ToString();
            //    _comment.comment_at =comment.at.ToString();
            //    _comment.comment_by = comment.by.ToString();
            //    _comment._id = shipment._id + comment.at.ToString();

            //    Common.OracleData.Insert("shipment_comments", _comment);
            //}
            #endregion
        } 
    }
}
