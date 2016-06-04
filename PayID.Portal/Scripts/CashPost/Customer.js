var Customer = {
    GetCustomer: function () {
        var q = $("#CustomerCodeSearch").val();

        if (q != null && q != undefined && q != '') {
            $.ajax({
                url: "/Merchant/Home/SearchCustomer",
                dataType: "html",
                type: "Get",
                data: { sSearch: q },
                success: function (data) {
                    $("#tempContainer").html(data);
                    $("#tempModal").modal('show');

                    $("#wrapper").height("auto");
                }
            })
        }
        else {

        }
    },

    SelectCustomer: function (CustomerCode, CustomerName, CustomerShortName, CustomerMobile, CustomerEmail, ContactName, CustomerAddress, ProvinceId, DistrictId) {
        $("#CustomerCode").val(CustomerCode);
        $("#CustomerCodeSearch").val(CustomerCode);
        $("#CustomerName").val(CustomerName);
        $("#CustomerShortName").val(CustomerShortName);
        $("#CustomerMobile").val(CustomerMobile);
        $("#CustomerEmail").val(CustomerEmail);
        $("#ContactMobile").val(CustomerMobile);
        $("#ContactName").val(ContactName);

        $("#PickUpStreet").val(CustomerAddress);
        $("#PickUpProvince").val(ProvinceId);
        $("#PickUpDistrict").val(DistrictId);
        common.SetAddress("PickUpProvince", "PickUpDistrict", "PickUpWard", "PickUpHamlet", "PickUpStreet", "PickUpAddress");

        var view = $("#View").val();

        if (view == "ImportLading") {

        }
        else {
            if (ProvinceId != undefined && ProvinceId != null && ProvinceId != '') {
                common.ProvinceChange("PickUpProvince", "PickUpDistrict", "PickUpWard", "PickUpHamlet", DistrictId, "PickUpStreet", "PickUpAddress");

            }

            if (DistrictId != undefined && DistrictId != null && DistrictId != '') {
                common.DistrictChange("PickUpDistrict", "PickUpWard", "PickUpHamlet", "", "PickUpProvince", "PickUpStreet", "PickUpAddress", DistrictId);
            }

            Shipment.LastestRequestByCus(CustomerCode);
        }

        Customer.GetStoreByCustomer(CustomerCode, "StoreId");
        $("#tempModal").modal('hide');

    },

    GetStoreByCustomer: function (CustomerCode, StoreId) {
        $("#" + StoreId).html("<option data-Address='' data-CustomerCode='' data-Default='' data-DistrictCode='' data-Id='' data-ManagerEmail='' data-ManagerMobile='' data-ManagerName='' data-PostCode='' data-ProvinceCode=''  data-StoreCode='' data-StoreName=''  value=''>---Kho hàng---</option>");
        if (Customer != null && Customer != undefined && Customer != '') {
            $.ajax({
                url: "/Merchant/Home/GetStoreByCustomer",
                dataType: "json",
                type: "Get",
                data: { CustomerCode: CustomerCode, Id: 0 },
                success: function (data) {
                    console.log(data);
                    var html = "<option data-Address='' data-CustomerCode='' data-Default='' data-DistrictCode='' data-Id='' data-ManagerEmail='' data-ManagerMobile='' data-ManagerName='' data-PostCode='' data-ProvinceCode=''  data-StoreCode='' data-StoreName=''  value=''>---Kho hàng---</option>";
                    $.each(data.data, function (i, v) {
                        html += "<option data-Address='" + v.Address + "' data-CustomerCode='" + v.CustomerCode + "' data-Default='" + v.Default + "' data-DistrictCode='" + v.DistrictCode + "' data-Id='" + v.Id + "' data-ManagerEmail='" + v.ManagerEmail + "' data-ManagerMobile='" + v.ManagerMobile + "' data-ManagerName='" + v.ManagerName + "' data-PostCode='" + v.PostCode + "' data-ProvinceCode='" + v.ProvinceCode + "' data-StoreCode='" + v.StoreCode + "' data-StoreName='" + v.StoreName + "' value='" + v.Id + "'>" + v.StoreName + "</option>";
                    });

                    $("#" + StoreId).html(html);

                    //////////////////////////////////////////
                    var view = $("#View").val();

                    $("#" + StoreId).change(function () {
                        var selected = $(this).find('option:selected');

                        $("#PostCodeLink").val(selected.data("postcode"));
                        $("#ContactName").val(selected.data("managername"));
                        $("#ContactMobile").val(selected.data("managermobile"));
                        $("#PickUpProvince").val(selected.data("provincecode"));
                        $("#PickUpStreet").val(selected.data("address"));
                        $("#PickUpDistrict").val(selected.data("districtcode"));


                        if (selected.val() != null && selected.val() != undefined && selected.val() != '') {
                            $("#StoreName").val(selected.text());
                        }
                        else {
                            $("#StoreName").val('');
                        }

                        if (view == "ImportLading") {
                            common.SetAddress("PickUpProvince", "PickUpDistrict", "PickUpWard", "PickUpHamlet", "PickUpStreet", "PickUpAddress");
                        }
                        else {
                            if (selected.data("provincecode") != undefined && selected.data("provincecode") != null && selected.data("provincecode") != '') {
                                common.ProvinceChange("PickUpProvince", "PickUpDistrict", "PickUpWard", "PickUpHamlet", selected.data("districtcode"), "PickUpStreet", "PickUpAddress");
                            }
                            else {
                                common.ProvinceChange("PickUpProvince", "PickUpDistrict", "PickUpWard", "PickUpHamlet");
                                common.SetAddress("PickUpProvince", "PickUpDistrict", "PickUpWard", "PickUpHamlet", "PickUpStreet", "PickUpAddress");
                            }

                            if (selected.data("districtcode") != undefined && selected.data("districtcode") != null && selected.data("districtcode") != '') {
                                common.DistrictChange("PickUpDistrict", "PickUpWard", "PickUpHamlet", selected.data("wardcode"), "PickUpProvince", "PickUpStreet", "PickUpAddress", selected.data("districtcode"));
                            }
                            else {
                                common.DistrictChange("PickUpDistrict", "PickUpWard", "PickUpHamlet");
                                common.SetAddress("PickUpProvince", "PickUpDistrict", "PickUpWard", "PickUpHamlet", "PickUpStreet", "PickUpAddress");
                            }
                        }
                    });
                }
            });
        }
    }
}