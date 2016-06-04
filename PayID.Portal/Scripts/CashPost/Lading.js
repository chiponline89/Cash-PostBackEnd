var Lading = {
    SearchLading: function (PageIndex) {
        common.StartLoading();
        var LadingCode = $("#LadingCode").val();
        var FromDate = $("#txtDateRange").val().split('-')[0];
        var ToDate = $("#txtDateRange").val().split('-')[1];
        var Status = $("#Status").val();
        var ProvinceCode = $("#ProvinceAccept").val();
        var CustomerCode = $("#Customer").val();
        var PageIndex = PageIndex;
        $.ajax({
            url: '/Lading/Home/ListLading',
            type: 'Get',
            dataType: 'html',
            data: {
                LadingCode: LadingCode,
                FromDate: FromDate,
                ToDate: ToDate,
                Status: Status,
                ProvinceCode: ProvinceCode,
                CustomerCode: CustomerCode,
                PageIndex: PageIndex
            },
            success: function (data) {
                $("#listLading").html(data);
                common.EndLoading();
            }
            , error: function (xhr, textStatus, errorThrown) {
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

    LadingCancel: function (LadingCode) {
        var html = '<div id="confirmCancelLading" class="bootbox modal fade in"><div class="modal-dialog modal-sm" role="document"><div class="modal-content">';
        html += '<div class="modal-header" style="padding:3px !important">';
        html += '<button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>';
        html += '<h4>Xác nhận</h4></div>';
        html += '<div class="modal-body">Bạn có thực sự muốn hủy vận đơn này không?</div>';
        html += '<div class="modal-footer">';
        html += '<button type="button" class="btn btn-small btn-primary confirm">Đồng ý</button>';
        html += '<button type="button" class="btn btn-small btn-danger" data-dismiss="modal">Hủy</button>';
        html += '</div></div></div>';

        $("#confirmModal").html(html);
        $("#confirmCancelLading").modal('show');

        $(".confirm").click(function () {
            $("#confirmCancelLading").modal('hide');
            common.StartLoading();
            $.ajax({
                url: "/Lading/Home/LadingCancel",
                type: "Post",
                dataType: "json",
                data: {
                    LadingCode: LadingCode
                },
                success: function (data) {
                    common.EndLoading();
                    if (data.Code == "00") {
                        common.Message("Thông báo", data.Mes, "success");
                        Lading.SearchLading(Lading.CurrentPage);
                    }
                    else {
                        common.Message("Thông báo", data.Mes, "error");
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    common.EndLoading();
                    if (xhr.status == 200) {
                        common.Message("Lỗi", "Hãy kiểm tra lại đường truyền trước khi thực hiện thao tác.", "error");
                    }
                    else {
                        common.Message("Lỗi", "Có lỗi trong quá trình xử lý dữ liệu", "error");
                    }
                }
            });
        });
    },

    LadingEdit: function (LadingCode) {
        common.StartLoading();
        $.ajax({
            url: "/Lading/Home/LadingEdit",
            type: "Get",
            dataType: "html",
            data: {
                Code: LadingCode
            },
            success: function (data) {
                common.EndLoading();
                $("#tempContainer").html(data);
                $("#tempModal").modal('show');
            },
            error: function (xhr, textStatus, errorThrown) {
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

    ChangeEvent: function () {
        common.StartLoading();
        var ServiceCode = $("#idServiceCode").val();
        var Value = $("#Value").val().split(',').join('').split('.').join('');
        var FromProvinceCode = $("#IdFromProvinceCode").val();
        var ToProvinceCode = $("#ToProvinceCode").val();
        var Weight = $("#Weight").val().split(',').join('').split('.').join('');
        $.ajax({
            type: 'POST',
            url: '/Lading/Home/GetFee',
            dataType: 'json',
            data:
                {
                    service_code: ServiceCode,
                    value: Value,
                    from_province_code: FromProvinceCode,
                    to_province_code: ToProvinceCode,
                    weight: Weight
                },
            success: function (data) {
                common.EndLoading();
                var TableContent = "<div class='table-responsive'><table class='table table-striped table-bordered datatables dataTable no-footer'>" +
                "<thead>" +
                    "<tr class='tr_head'>" +
                        "<td>Cước vận chuyển</td>" +
                        "<td>Phí thu hộ (COD)</td>" +
                        "<td>Phí dịch vụ</td>" +
                        "<td>Tổng tiền thu hộ<br>(Người nhận trả)</td>" +
                        "<td>Thời gian<br>vận chuyển</td>"
                "</tr>" +
            "</thead>"
                "<tbody class='tbody'>";
                TableContent += "<tr style='text-align: right; background-color:white;'>" +
                                        "<td><div id='idtthcuocchinh' align='right'>" + data.MainFee + "</div></td>" +
                                        "<td><div id='idtthcuoccod' align='right'>" + data.CodFee + "</div></td>" +
                                        "<td><div id='idtthcuocthem' align='right'>" + data.ServiceFee + "</div></td>" +
                                        "<td><div id='tthtongthuho' align='right'>" + data.TotalFee + "</div></td>" +
                                        " <td align='right'><span></span> <sup style='text-align: right;'> giờ</sup></td>" +
                                    "</tr>";

                TableContent += "</tbody></table></div>";

                $("#calculate_fee").html(TableContent);
            },
            error: function (xhr, textStatus, errorThrown) {
                common.EndLoading();
                if (xhr.status == 200) {
                    common.Message("Lỗi", "Hãy kiểm tra lại đường truyền trước khi thực hiện thao tác.", "error");
                }
                else {
                    common.Message("Lỗi", "Có lỗi trong quá trình xử lý dữ liệu", "error");
                }
            }
        });
        return false;
    },

    SaveLading: function () {
        var valid = $('#LadingEdit').validationEngine('validate');
        if (valid == true) {
            var html = '<div id="confirmUpdateLading" class="bootbox modal fade in"><div class="modal-dialog modal-sm" role="document"><div class="modal-content">';
            html += '<div class="modal-header" style="padding:3px !important">';
            html += '<button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>';
            html += '<h4>Xác nhận</h4></div>';
            html += '<div class="modal-body">Bạn có muốn cập nhật lại thông tin vận đơn này không?</div>';
            html += '<div class="modal-footer">';
            html += '<button type="button" class="btn btn-small btn-primary confirm">Đồng ý</button>';
            html += '<button type="button" class="btn btn-small btn-danger" data-dismiss="modal">Hủy</button>';
            html += '</div></div></div>';

            $("#confirmModal").html(html);
            $("#confirmUpdateLading").modal('show');

            $(".confirm").click(function () {
                $("#confirmUpdateLading").modal('hide');
                common.StartLoading();
                $.ajax({
                    type: 'POST',
                    url: '/Lading/Home/SaveLading',
                    dataType: 'json',
                    data: {
                        tenSP: $("#ProductName").val(),
                        tenSP_old: $("#IdProductName").val(),

                        giaTri: $("#Value").val().split(',').join('').split('.').join(''),
                        giaTri_old: $("#IdProductValue").val(),

                        khoiLuong: $("#Weight").val().split(',').join('').split('.').join(''),
                        khoiLuong_old: $("#IdProductWeight").val(),

                        soLuong: $("#Quantity").val().split(',').join('').split('.').join(''),
                        soLuong_old: $("#IdProductQuantity").val(),

                        moTaSP: $("#ProductDescription").val(),
                        moTaSP_old: $("#IdProductDescr").val(),

                        ten: $("#ReceiverName").val(),
                        ten_old: $("#IdReceiverName").val(),

                        soDienThoai: $("#ReceiverMobile").val(),
                        soDienThoai_old: $("#IdReceiverMobile").val(),

                        diaChi: $("#ReceiverAddress").val(),
                        diaChi_old: $("#IdReceiverAddress").val(),

                        buuCucNhan: $("#ToProvinceCode").val(),
                        buuCucNhan_old: $("#IdToProvinceCode").val(),

                        id: $("#txtId").val(),

                        MainFee: document.getElementById("idtthcuocchinh").innerHTML,
                        MainFee_old: $("#IdMainFee").val(),
                        CodFee: document.getElementById("idtthcuoccod").innerHTML,
                        CodFee_old: $("#IdCodFee").val(),
                        ServiceFee: document.getElementById("idtthcuocthem").innerHTML,
                        ServiceFee_old: $("#IdServiceFee").val(),
                        TotalFee: document.getElementById("tthtongthuho").innerHTML,
                        TotalFee_old: $("#IdTotalFee").val()
                    },
                    success: function (data) {
                        common.EndLoading();
                        common.Message("Thành công", "Cập nhật dữ liệu thành công", "success");
                        Lading.SearchLading(Lading.CurrentPage);
                        $("#tempModal").modal('hide');
                    },
                    error: function (xhr, textStatus, errorThrown) {
                        common.EndLoading();
                        if (xhr.status == 200) {
                            common.Message("Lỗi", "Hãy kiểm tra lại đường truyền trước khi thực hiện thao tác.", "error");
                        }
                        else {
                            common.Message("Lỗi", "Có lỗi trong quá trình xử lý dữ liệu", "error");
                        }
                    }
                });
                return true;
            });
        }
    },

    AddLading: function (arrShipment, filename) {
        Lading.ArrLading = [];
        if (arrShipment != null && arrShipment.length > 0) {

            var CustomerCode = $("#CustomerCode").val();
            var StoreCode = $("#StoreId").val();
            var CustomerName = $("#CustomerName").val();
            var CustomerMobile = $("#CustomerMobile").val();
            var CustomerStreet = $("#PickUpStreet").val();
            var CustomerAddress = $("#PickUpAddress").val();
            var CustomerProvince = $("#PickUpProvince").val();
            var CustomerDistrict = $("#PickUpDistrict").val();
            var CustomerWard = $("#PickUpWard").val();


            $.each(arrShipment, function (i, v) {
                Lading.ArrLading.push({
                    Id: v.Id,
                    Weight: v.Weight,
                    Quantity: v.Quantity,
                    CustomerCode: CustomerCode,
                    StoreCode: StoreCode,
                    SenderName: CustomerName,
                    SenderAddress: CustomerAddress,
                    SenderMobile: CustomerMobile,
                    ToProvinceCode: CustomerProvince,
                    ToDistrictCode: CustomerDistrict,
                    ToWardCode: CustomerWard,
                    Receiver_Name: v.Receiver_Name,
                    Receiver_Street: v.Receiver_Street,
                    Receiver_Address: v.Receiver_Address,
                    Receiver_Mobile: v.Receiver_Mobile,
                    ReceiverProvinceId: v.ReceiverProvinceId,
                    ReceiverDistrictId: v.ReceiverDistrictId,
                    ReceiverWardId: v.ReceiverWardId,
                    ProductName: v.ProductName,
                    ProductDescription: v.ProductDescription,
                    ServiceCode: v.ServiceCode,
                    Type: v.Type,
                    Value: v.Value,
                    CollectValue: v.CollectValue,
                    FileName: v.FileName,
                    Height: v.Height,
                    Channel: v.Channel,
                    Check: v.Check,
                    IsConsorShip: v.IsConsorShip
                });
            });

            if (Lading.ArrLading == null || Lading.ArrLading == undefined || Lading.ArrLading.length == 0) {
                common.Message("Cảnh báo", "Thao tác thất bại[Chưa có vận đơn nào cả].", "warning");
            }
            else {
                var post = { Lading: Lading.ArrLading };
                common.StartLoading("Đang tiến hành thêm mới Vận đơn...");
                $.ajax({
                    url: "/Lading/ImportLading/CreateLading",
                    dataType: "json",
                    type: "POST",
                    data: {
                        lading: JSON.stringify(post),
                        fileName: filename,
                        customerCode: CustomerCode
                    },
                    success: function (data) {
                        common.EndLoading();
                        if(data.Code == "00")
                        {
                            common.Message("Thành công", data.Mes, "success");
                        }
                        else
                        {
                            common.Message("Thông báo", data.Mes, "error");
                        }
                        //if (data.Code == "00" || data.Code == "01") {
                        //    common.Message("Thành công", data.Mes, "success");
                        //    //$("#contentImport").html('');
                        //    $("#btnImport").attr("disabled", "disabled");
                        //    Order.GetShipMentInAddOrder();

                        //    $.each(data.ListError, function (i, v) {
                        //        $("#row" + v).removeClass("trError");
                        //        $("#row" + v).addClass("errorUp");
                        //    });

                        //    common.EndLoading();
                        //    var view = $("#view").val();
                        //    if (view == "CreateShipment") {
                        //        new PNotify({
                        //            title: "Thông báo",
                        //            text: "Tạo mới đơn hàng thành công.",
                        //            type: 'success',
                        //            styling: 'fontawesome'
                        //        });

                        //        if (action == "E") {
                        //            Order.DisableFormShipment();
                        //        } else {
                        //            Order.RefreshFormShipment();
                        //        }
                        //    }


                        //}
                        //else {
                        //    common.Message("Lỗi", data.Mes, "error");
                        //}
                    },
                    error: function (xhr, textStatus, errorThrown) {
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



        }
    },

    ArrLading : [],

    CurrentPage: 1
}