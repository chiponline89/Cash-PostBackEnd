using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.Portal.Models
{
    public class Shipment
    {
        public string Id { get; set; }

        public string TrackingCode { get; set; }

        public string UnitCreate { get; set; }

        public string UnitCreateName { get; set; }

        public string UserCreate { get; set; }

        public string UserCreateName { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string CustomerShortName { get; set; }

        public string CustomerMobile { get; set; }

        public string CustomerEmail { get; set; }

        public string PostCode { get; set; }

        public string StoreId { get; set; }

        public string StoreName { get; set; }

        public string PostCodeLink { get; set; }

        /// <summary>
        /// Quản lý kho
        /// </summary>
        public string ContactName { get; set; }

        /// <summary>
        /// Điện thoại quản lý Kho
        /// </summary>
        public string ContactMobile { get; set; }

        public string PickUpContactDate { get; set; }

        public string PickUpContactTime { get; set; }

        public int PickUpProvince { get; set; }

        public string PickUpProvinceName { get; set; }

        public int PickUpDistrict { get; set; }

        public string PickUpDistrictName { get; set; }

        public int PickUpWard { get; set; }

        public string PickUpWardName { get; set; }

        public string PickUpStreet { get; set; }

        public string PickUpAddress { get; set; }

        public string ReceiverName { get; set; }

        public string ReceiverMobile { get; set; }

        public int ReceiverProvince { get; set; }

        public string ReceiverProvinceName { get; set; }

        public int ReceiverDistrict { get; set; }

        public int ReceiverWard { get; set; }

        public string ReceiverStreet { get; set; }

        public string ReceiverAddress { get; set; }

        public string ProductName { get; set; }

        public long Weight { get; set; }

        public string sWeight { get; set; }

        public int Quantity { get; set; }

        public string sQuantity { get; set; }

        public long Amount { get; set; }

        public string sAmount { get; set; }

        public string Height { get; set; }

        public int ServiceType { get; set; }

        public string Description { get; set; }

        public string UnitLink { get; set; }

        public string UnitName { get; set; }
    }

    public class ShipmentAPI
    {
        public string Id { get; set; }

        public string TrackingCode { get; set; }

        public string RefCode { get; set; }

        public Address FromAddress { get; set; }

        public Address ToAddress { get; set; }

        public Product Product { get; set; }

        public Service Service { get; set; }

        public string Status { get; set; }

        public List<AssignedTo> AssignedTo { get; set; }

        public string CreatedBy { get; set; }

        public string CreateAt { get; set; }

        public string CurrentAssigned { get; set; }

        public string CurrentAssignedName { get; set; }

        public string CurrentPostman { get; set; }

        public string CurrentPostmanName { get; set; }

        public int IsAssigned { get; set; }

        public string OrderId { get; set; }

        public long CreateTime { get; set; }

        public long UpdateTime { get; set; }

        public List<Confirms> Confirms { get; set; }

        public List<Comments> Comments { get; set; }

        public string Parcel { get; set; }

        public long LastUpdateTime { get; set; }

        public string UserCreateId { get; set; }

        public string UserCreateName { get; set; }

        public string UnitCreateCode { get; set; }

        public string UnitCreateName { get; set; }
    }

    public class Confirms
    {
        public long Time { get; set; }

        public string By { get; set; }

        public string Comment { get; set; }

        public string Reason { get; set; }

        public string EmployeeId { get; set; }

        public string EmployeeName { get; set; }
    }

    public class AssignedTo
    {
        public string AssignBy { get; set; }

        public long AssignTime { get; set; }

        public string AssignToId { get; set; }

        public string AssignToFullName { get; set; }

        public string Notes { get; set; }
    }

    public class Product
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public long Value { get; set; }

        public long Weight { get; set; }

        public int Quantity { get; set; }
    }

    public class Comments
    {
        public long Time { get; set; }

        public string By { get; set; }

        public string Comment { get; set; }

        public string Notes { get; set; }
    }

    public class Service
    {
        public int CashPostService { get; set; }

        public int ShippingMainService { get; set; }

        public string ShippingAddService { get; set; }
    }

    public class Address
    {
        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string CustomerMobile { get; set; }

        public string CustomerEmail { get; set; }

        public string StoreCode { get; set; }

        public string StoreName { get; set; }

        public int ProvinceId { get; set; }

        public string ProvinceName { get; set; }

        public int DistrictId { get; set; }

        public string DistrictName { get; set; }

        public int WardId { get; set; }

        public string WardName { get; set; }

        public int HamletId { get; set; }

        public string HamletName { get; set; }

        public string Street { get; set; }

        public string FullAddress { get; set; }

        public string PostCodeLink { get; set; }

        public string PostCode { get; set; }

        public string ContactName { get; set; }

        public string ContactMobile { get; set; }

        public string ContactEmail { get; set; }
    }

    public class ShipmentCustomer
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Mobile { get; set; }
    }
}