var Shipment = {
    OnSuccess: function (data) {
        common.EndLoading();
        if(data.Code > 0)
        {
            $("#btnSaveRequest").html("<i class='fa fa-save'></i> Cập nhật");
            $("#Id").val(data.Code);
            Shipment.DisableInfoCus();
            common.Message("Thông báo", data.Mes, "success");
        }
        else
        {
            common.Message("Thông báo", data.Mes, "error");
        }
    },

    SubmitShipment: function () {
        var valid = $('#fShipment').validationEngine('validate');

        if (valid == true) {
            common.StartLoading();
            $("#fShipment").submit();
        }
    },

    RefreshForm: function(){
        Shipment.EnableInfoCus();
        $("#Id").val('');
        $("#CustomerCode").val('');
        $("#CustomerCodeSearch").val('');
        $("#CustomerName").val('');
        $("#CustomerShortName").val('');
        $("#CustomerMobile").val('');
        $("#CustomerEmail").val('');
        $("#StoreId").html("<option>---Kho hàng---</option>");
        $("#StoreName").val('');
        $("#PostCodeLink").val('');
        $("#ContactName").val('');
        $("#ContactMobile").val('');
        common.SetDateTime("PickUpContactDate", "PickUpContactTime");
        $("#PickUpProvince").val('');
        $("#PickUpDistrict").html("<option>---Quận, Huyện---</option>");
        $("#PickUpWard").html("<option>---Xã, Phường---</option>");
        $("#PickUpStreet").val('');
        $("#PickUpAddress").val('');
        $("#ReceiverName").val('');
        $("#ReceiverMobile").val('');
        $("#ReceiverProvince").val('');
        $("#ReceiverDistrict").html("<option>---Quận, Huyện---</option>");
        $("#ReceiverWard").html("<option>---Xã, Phường---</option>");
        $("#ReceiverStreet").val('');
        $("#ReceiverAddress").val('');
        $("#ProductName").val('');
        $("#sWeight").val('0');
        $("#sQuantity").val('0');
        $("#sAmount").val('0');
        $("#ServiceType").val('1');
        $("#Description").val('');
        $("#btnSaveRequest").html("<i class='fa fa-save'></i> Lưu");
        $("#contentLastestRequest").html('');
    },

    DisableInfoCus: function(){
        $("#CustomerCodeSearch").attr("disabled", "disabled");
        $("#btnSearchCus").attr("disabled", "disabled");
        $("#btnSearchCus").css({ "display": "none" });
        $("#CustomerName").attr("disabled", "disabled");
        $("#CustomerShortName").attr("disabled", "disabled");
        $("#CustomerMobile").attr("disabled", "disabled");
        $("#CustomerEmail").attr("disabled", "disabled");
    },

    EnableInfoCus: function(){
        $("#CustomerCodeSearch").removeAttr("disabled");
        $("#btnSearchCus").removeAttr("disabled");
        $("#btnSearchCus").css({ "display": "" });
        $("#CustomerName").removeAttr("disabled");
        $("#CustomerShortName").removeAttr("disabled");
        $("#CustomerMobile").removeAttr("disabled");
        $("#CustomerEmail").removeAttr("disabled");
    },

    CustomerChange: function () {
        $("#CustomerCode").val('');
        $("#CustomerName").val('');
        $("#CustomerShortName").val('');
        $("#CustomerMobile").val('');
        $("#CustomerEmail").val('');
        $("#StoreId").html("<option>---Kho hàng---</option>");
        $("#PostCodeLink").val('');
        $("#ContactName").val('');
        $("#ContactMobile").val('');
        $("#PickUpProvince").val('');
        $("#PickUpDistrict").html("<option>---Quận, Huyện---</option>");
        $("#PickUpWard").html("<option>---Xã, Phường---</option>");
        $("#PickUpStreet").val('');
        $("#PickUpAddress").val('');
        $("#contentLastestRequest").html('');
        $("#contentImport").html("");
    },

    LastestRequestByCus: function (CustomerCode) {
        $.ajax({
            url: "/ServiceRequest/Request/LastestRequestByCus",
            type: "Get",
            dataType: "html",
            data: { CustomerCode: CustomerCode },
            success: function(data)
            {
                $("#contentLastestRequest").html(data);
            }
        });
    },

    GetShipment: function (Page) {
        var FromDate = $("#txtDateRange").val().split('-')[0];
        var ToDate = $("#txtDateRange").val().split('-')[1];
        var CustomerCode = $("#CustomerCode").val();
        var Status = $("#Status").val();
        var OrderCode = $("#OrderCode").val();
        var TrackingCode = $("#TrackingCode").val();
        common.StartLoading();
        Shipment.CurrentPage = Page;
        $.ajax({
            url: "/ServiceRequest/Request/GetOrder",
            type: "Get",
            dataType: "html",
            data: {
                FromDate: FromDate, 
                ToDate: ToDate, 
                CustomerCode: CustomerCode, 
                Status: Status, 
                OrderCode: OrderCode, 
                TrackingCode: TrackingCode, 
                PageIndex: Page
            }, success: function(data)
            {
                $("#listOrder").html(data);
                common.EndLoading();
            }, error: function (xhr, textStatus, errorThrown) {
                common.EndLoading();
                if (xhr.status == 200) {

                    common.Message("Lỗi", "Hãy kiểm tra lại đường truyền trước khi thực hiện thao tác.", "error");
                }
                else {
                    common.Message("Lỗi", "Có lỗi trong quá trình xử lý dữ liệu", "error");
                }
            }
        })
    },

    CheckStatusOther: function (Status) {
        var rows = $('#tblcontent .tblrow');

        var check = 0;
        $.each(rows, function (i, v) {
            var orderID = $(rows[i]).data('id');
            if ($("#ck-" + orderID).is(':checked')) {
                check++;
                return false;
            }
        });

        if (check == 0) {
            common.Message("Cảnh báo", "Hãy chọn những tin mà bạn cần thao tác.", "warning");
            return false;
        } else {
            check = 0;
            $.each(rows, function (i, v) {
                var orderID = $(rows[i]).data('id');
                if ($("#ck-" + orderID).is(':checked')) {
                    if ($("#status-" + orderID).data('status') != Status) {
                        if (Status == "C5") {
                            common.Message("Cảnh báo", "Bạn chỉ được phép thao tác với những tin đang ở trạng thái Chấp nhận.", "warning");
                        }
                        else if (Status == "C6") {
                            common.Message("Cảnh báo", "Bạn chỉ được phép thao tác với những tin đang ở trạng thái Đang lấy hàng.", "warning");
                        }
                        else {
                            common.Message("Cảnh báo", "Bạn chỉ được phép thao tác với những tin có cùng trạng thái.", "warning");
                        }

                        check++;
                        return false;
                    }
                }
            });

            if (check == 0) {
                return true;
            } else {
                return false;
            }
        }
    },

    AllotOrder: function (OrderId) {

        if (OrderId == "0") {
            if (Shipment.CheckStatusOther("C5")) {
                Shipment.IsMultiAssign = "Y";
                Shipment.ViewDistribute();
            }
        }
        else {
            Shipment.IsMultiAssign = OrderId;
            Shipment.ViewDistribute();
        }
    },

    ViewDistribute: function () {
        common.StartLoading();
        $.ajax({
            url: "/Systems/Proxy/Workflow_ListNextProcessUnit",
            type: "Get",
            dataType: "html",
            data: { Service: "SHIPPING", Type: "PICKUP" },
            success: function(data)
            {
                $("#tempContainerSmall").html(data);
                $("#tempModal_small").modal('show');
                common.EndLoading();
            }, error: function (xhr, textStatus, errorThrown) {
                common.EndLoading();
                if (xhr.status == 200) {
                    common.Message("Lỗi", "Hãy kiểm tra lại đường truyền trước khi thực hiện thao tác.", "error");
                }
                else {
                    common.Message("Lỗi", "Có lỗi trong quá trình xử lý dữ liệu", "error");
                }
            }
        })
    },

    ListPostMan: function(UnitCode)
    {
        $("#cbPostMan").val("<option value=''>---Bưu tá Thu gom---</option>");
        if (UnitCode != null && UnitCode != undefined && UnitCode != '')
        {
            $.ajax({
                url: "/ServiceRequest/Request/ListPostMan",
                type: "Get",
                dataType: "json",
                data: { UnitCode: UnitCode },
                success: function (data) {
                    var html = '';

                    $.each(data.data, function (i, v) {
                        html += "<option value='" + v.Value + "'>" + v.Text + "</option>";
                    });

                    $("#cbPostMan").html(html);

                }, error: function (xhr, textStatus, errorThrown) {
                    common.EndLoading();
                    if (xhr.status == 200) {
                        common.Message("Lỗi", "Hãy kiểm tra lại đường truyền trước khi thực hiện thao tác.", "error");
                    }
                    else {
                        common.Message("Lỗi", "Có lỗi trong quá trình xử lý dữ liệu", "error");
                    }
                }
            });
        }
    },

    ConfirmCancelAssign: function (ShipmentId) {
        if (ShipmentId == "0") {
            if (Shipment.CheckStatusOther("C6")) {
                Shipment.IsMultiCancelAssign = "Y";
                Shipment.ViewComfirmCancelAssign();
            }
        }
        else {
            Shipment.IsMultiCancelAssign = ShipmentId;
            Shipment.ViewComfirmCancelAssign();
        }
    },

    ViewComfirmCancelAssign: function()
    {
        common.StartLoading();
        $.ajax({
            url: "/ServiceRequest/Request/ConfirmAssignCancel",
            type: "Get",
            dataType: "html",
            data: { },
            success: function (data) {
                $("#tempContainerSmall").html(data);
                $("#tempModal_small").modal('show');
                common.EndLoading();

            }, error: function (xhr, textStatus, errorThrown) {
                common.EndLoading();
                if (xhr.status == 200) {
                    common.Message("Lỗi", "Hãy kiểm tra lại đường truyền trước khi thực hiện thao tác.", "error");
                }
                else {
                    common.Message("Lỗi", "Có lỗi trong quá trình xử lý dữ liệu", "error");
                }
            }
        });
    },

    CancelAssign: function(){
        var valid = $('#CancelAssignForm').validationEngine('validate');
        if (valid == true) {
            var Notes = $("#Remark").val();
            var Id = "";
            var TypeAssign = "";

            if (Shipment.IsMultiCancelAssign == "Y") {
                $(".checkMe").each(function (i, v) {
                    if ($(this).is(":checked")) {
                        Id = Id + $(this).data("id") + "|";
                    }
                });
                TypeAssign = "M";
            }
            else {
                Id = Shipment.IsMultiCancelAssign;
                TypeAssign = "";
            }

            if (Id != null && Id != "" && Id != undefined) {
                common.StartLoading();
                $.ajax({
                    url: "/ServiceRequest/Request/AssignCancel",
                    data: {
                        ShipmentId: Id,
                        Notes: Notes,
                        TypeAssign: TypeAssign

                    },
                    type: "Post",
                    dataType: "json",
                    success: function (data) {
                        common.EndLoading();
                        if (data.Code == "00" || data.Code == "01") {
                            $("#tempModal_small").modal('hide');
                            common.Message("Thông báo", data.Mes, "success");
                            Shipment.GetShipment(Shipment.CurrentPage);
                        }
                        else {
                            common.Message("Lỗi", data.Mes, "error");
                        }
                    }, error: function (xhr, textStatus, errorThrown) {
                        common.EndLoading();
                        if (xhr.status == 200) {
                            common.Message("Lỗi", "Hãy kiểm tra lại đường truyền trước khi thực hiện thao tác.", "error");
                        }
                        else {
                            common.Message("Lỗi", "Có lỗi trong quá trình xử lý dữ liệu", "error");
                        }
                    }
                });
            }
            else {
                common.Message("Cảnh báo", "Hãy chọn Tin mà bạn cần hủy điều Tin.", "warning");
            }
        }
    },

    Assign: function () {
        var valid = $('#AssignForm').validationEngine('validate');
        if (valid == true) {
            var UnitSelect = $("#cbUnitProcess option:selected");
            var UnitLink = UnitSelect.data("link");
            var UnitName = UnitSelect.html();
            var Id = "";
            var PostMan = $('#cbPostMan').val();
            var PostManName = $("#cbPostMan option:selected").text;
            var Notes = $("#noteUnitProcess").val();
            var TypeAssign = "";

            if (Shipment.IsMultiAssign == "Y") {
                $(".checkMe").each(function (i, v) {
                    if ($(this).is(":checked")) {
                        Id = Id + $(this).data("id") + "|";
                    }
                });
                TypeAssign = "M";
            }
            else {
                Id = Shipment.IsMultiAssign;
                TypeAssign = "";
            }


            if (Id != null && Id != "" && Id != undefined) {
                common.StartLoading();
                $.ajax({
                    url: "/ServiceRequest/Request/Assign",
                    data: {
                        ShipmentId: Id,
                        UnitCode: UnitLink,
                        UnitName: UnitName,
                        PostMan: PostMan,
                        PostManName: PostManName,
                        Notes: Notes,
                        TypeAssign: TypeAssign

                    },
                    type: "Post",
                    dataType: "json",
                    success: function (data) {
                        common.EndLoading();
                        if(data.Code == "00" || data.Code == "01")
                        {
                            $("#tempModal_small").modal('hide');
                            common.Message("Thông báo", data.Mes, "success");
                            Shipment.GetShipment(Shipment.CurrentPage);
                        }
                        else
                        {
                            common.Message("Lỗi", data.Mes, "error");
                        }
                    }, error: function (xhr, textStatus, errorThrown) {
                        common.EndLoading();
                        if (xhr.status == 200) {
                            common.Message("Lỗi", "Hãy kiểm tra lại đường truyền trước khi thực hiện thao tác.", "error");
                        }
                        else {
                            common.Message("Lỗi", "Có lỗi trong quá trình xử lý dữ liệu", "error");
                        }
                    }
                });
            }
            else {
                common.Message("Cảnh báo", "Hãy chọn Tin mà bạn cần điều.", "warning");
            }
        }
    },

    CurrentPage: 1,
    IsMultiAssign: "N",
    IsMultiCancelAssign: "N"
}