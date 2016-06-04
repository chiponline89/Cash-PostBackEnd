var common = {
    IsMobileNumber: function (mobileNumber) {
        try {
            //var _lngMobileNumber = parseFloat(mobileNumber);
            var _leading = mobileNumber.substr(0, 1);

            if (_leading == "0") {
                var _operator1 = mobileNumber.substr(1, 1);
                var _operator2 = mobileNumber.substr(1, 2);
                var _operator3 = mobileNumber.substr(1, 3);

                if (mobileNumber.length == 9) {
                    if (common.Contains(ListOperatorCodeTinh, _operator1) || common.Contains(ListOperatorCodeTinh, _operator2) || common.Contains(ListOperatorCodeTinh, _operator3)) {
                        return true;
                    }
                    else {
                        return false;
                    }
                }
                else if (mobileNumber.length == 10) {
                    if (common.Contains(ListOperatorCode9y, _operator2) || common.Contains(ListOperatorCode9yy, _operator3)) {
                        return true;
                    }
                    else if (common.Contains(ListOperatorCodeTinh, _operator1) || common.Contains(ListOperatorCodeTinh, _operator2) || common.Contains(ListOperatorCodeTinh, _operator3)) {
                        return true;
                    }
                    else {
                        return false;
                    }
                }
                else if (mobileNumber.length == 11) {
                    if (common.Contains(ListOperatorCode1yy, _operator3)) {
                        return true;
                    }
                    else if (common.Contains(ListOperatorCodeTinh, _operator1) || common.Contains(ListOperatorCodeTinh, _operator2) || common.Contains(ListOperatorCodeTinh, _operator3)) {
                        return true;
                    }
                    else {
                        return false;
                    }
                }
                    //else if(mobileNumber.length == 12)
                    //{
                    //    if (common.Contains(ListOperatorCodeTinh, _operator1) || common.Contains(ListOperatorCodeTinh, _operator2) || common.Contains(ListOperatorCodeTinh, _operator3)) {
                    //        return true;
                    //    }
                    //    else {
                    //        return false;
                    //    }
                    //}
                else {
                    return false;
                }
            }
            else {
                return false;
            }

        }
        catch (error) {
            return false;
        }
    },

    Contains: function (arr, obj) {
        for (var i = 0; i < arr.length; i++) {
            if (arr[i] === obj) {
                return true;
            }
        }
        return false;
    },

    CheckMobileNumberTk: function (field) {
        if (common.IsMobileNumber(field)) {
            return true;
        }
        else {
            return "* Số điện thoại không đúng"
        }
    },

    CheckMobileNumber: function (field) {
        if (common.IsMobileNumber(field.val())) {
            return true;
        }
        else {
            return "* Số điện thoại không đúng"
        }
    },

    ProvinceChange: function (ProvinceId, DistrictId, WardId, HamletId, DistrictValue, StreetId, AddressId) {
        $("#" + DistrictId).html("<option value=''>---Quận, Huyện---</option>");
        $("#" + WardId).html("<option value=''>---Xã, Phường---</option>");
        $("#" + HamletId).html("<option value=''>---Tổ, Thôn, Xóm---</option>");
        common.SetAddress(ProvinceId, DistrictId, WardId, HamletId, StreetId, AddressId);

        var ProvinceValue = $("#" + ProvinceId).val();

        if (ProvinceValue != null && ProvinceValue != undefined && ProvinceValue != '') {
            $.ajax({
                url: "/Merchant/Home/GetDistrictByProvince",
                data: { ProvinceCode: ProvinceValue },
                type: "Get",
                dataType: "json",
                success: function (data) {
                    var html = "<option value=''>---Quận, Huyện---</option>";
                    $.each(data.data, function (i, v) {
                        html += "<option value='" + v.Value + "'>" + v.Text + "</options>";
                    });

                    $("#" + DistrictId).html(html);

                    if (DistrictValue != null && DistrictValue != undefined && DistrictValue != '') {
                        $("#" + DistrictId).val(DistrictValue);
                    }

                    common.SetAddress(ProvinceId, DistrictId, WardId, HamletId, StreetId, AddressId);
                }
            });
        }
    },

    DistrictChange: function (DistrictId, WardId, HamletId, WardValue, ProvinceId, StreetId, AddressId, DisValue) {
        $("#" + WardId).html("<option value=''>---Xã, Phường---</option>");
        $("#" + HamletId).html("<option value=''>---Tổ, Thôn, Xóm---</option>");
        common.SetAddress(ProvinceId, DistrictId, WardId, HamletId, StreetId, AddressId);
        var DistrictValue = $("#" + DistrictId).val();

        if($.isNumeric(DistrictId) == true && DistrictId > 0)
        {
            DistrictValue = DistrictId;
        }

        if (DisValue != null && DisValue != undefined && DisValue != '' && $.isNumeric(DisValue) == true && DisValue > 0)
        {
            DistrictValue = DisValue;
        }

        if ((DistrictValue != null && DistrictValue != undefined && DistrictValue != '' && $.isNumeric(DistrictValue) == true && DistrictValue > 0)) {
            $.ajax({
                url: "/Merchant/Home/GetWardByDistrict",
                data: { DistrictCode: DistrictValue },
                type: "Get",
                dataType: "json",
                success: function (data) {
                    var html = "<option value=''>---Xã, Phường---</option>";
                    $.each(data.data, function (i, v) {
                        html += "<option value='" + v.Value + "'>" + v.Text + "</options>";
                    });

                    $("#" + WardId).html(html);

                    if (WardValue != null && WardValue != undefined && WardValue != '') {
                        $("#" + WardId).val(WardValue);
                    }

                    common.SetAddress(ProvinceId, DistrictId, WardId, HamletId, StreetId, AddressId);
                }
            })
        }
    },

    SetAddress: function (ProvinceId, DistrictId, WardId, HamletId, StreetId, AddressId) {
        var Province = "";
        var District = "";
        var Ward = "";
        var Hamlet = "";
        var Street = "";

        if ($("#" + ProvinceId).val() != undefined && $("#" + ProvinceId).val() != '' && $("#" + ProvinceId).val() != null && $("#" + ProvinceId).val() > 0) {
            Province = $("#" + ProvinceId + " option:selected").text();
        }

        if ($("#" + DistrictId).val() != undefined && $("#" + DistrictId).val() != '' && $("#" + DistrictId).val() != null && $("#" + DistrictId).val() > 0) {
            District = $("#" + DistrictId + " option:selected").text();
        }

        if ($("#" + WardId).val() != undefined && $("#" + WardId).val() != '' && $("#" + WardId).val() != null && $("#" + WardId).val() > 0) {
            Ward = $("#" + WardId + " option:selected").text();
        }

        if ($("#" + HamletId).val() != undefined && $("#" + HamletId).val() != '' && $("#" + HamletId).val() != null && $("#" + HamletId).val() > 0) {
            Hamlet = $("#" + HamletId + " option:selected").text();
        }

        Street = $("#" + StreetId).val();

        var Address = '';

        if (Street != null && Street != '' && Street != undefined) {
            Address += Street;
        }

        if (Hamlet != null && Hamlet != '' && Hamlet != undefined) {
            Address += ", " + Hamlet;
        }

        if (Ward != null && Ward != '' && Ward != undefined) {
            Address += ", " + Ward;
        }

        if (District != null && District != '' && District != undefined) {
            Address += ", " + District;
        }

        if (Province != null && Province != '' && Province != undefined) {
            Address += ", " + Province;
        }

        $("#" + AddressId).val(Address);
    },

    SetDateTime: function (controlDate, controlTime) {
        //2013/10/04 08:51:32
        var now = new Date();
        var year = now.getFullYear();
        var month = now.getMonth() + 1;
        var day = now.getDate();
        var hour = now.getHours();
        var minute = now.getMinutes();
        var second = now.getSeconds();
        if (month.toString().length == 1) {
            var month = '0' + month;
        }
        if (day.toString().length == 1) {
            var day = '0' + day;
        }
        if (hour.toString().length == 1) {
            var hour = '0' + hour;
        }
        if (minute.toString().length == 1) {
            var minute = '0' + minute;
        }
        if (second.toString().length == 1) {
            var second = '0' + second;
        }
        var datenow = year + '-' + month + '-' + day;
        var timenow = hour + ':' + minute;
        $('#' + controlDate).val(datenow);
        $('#' + controlTime).val(timenow);
    },

    Message: function (title, text, type) {
        $(".modal-backdrop").removeClass("fade").removeClass("modal-backdrop").removeClass("in");

        var colorHeader = "#3498DB";
        if (type == "warning") {
            colorHeader = "#f39c12";
        }
        else if (type == "error") {
            colorHeader = "#e74c3c";
        }

        var html = '<div id="CASHPOSTMessage" class="bootbox modal fade in"><div class="modal-dialog modal-sm" role="document"><div class="modal-content">';
        html += '<div class="modal-header" style="padding:3px !important; background-color:' + colorHeader + '">';
        html += '<button type="button" style="margin-right:5px;" class="close" data-dismiss="modal" aria-hidden="true">×</button>';
        html += '<h4 style="color:#fff; margin-left:10px;">' + title + '</h4>';
        html += '</div><div class="modal-body">' + text + '</div>';
        html += '<div class="modal-footer">';
        //html += '<button type="button" class="btn btn-small btn-primary confirm">Đồng ý</button>';
        html += '<button type="button" class="btn btn-small ';
        if (type == "success") {
            html += 'btn-success';
        }
        else if (type == "error") {
            html += 'btn-danger';
        }
        else if (type == "warning") {
            html += 'btn-warning';
        }

        html += '" data-dismiss="modal">Đóng</button>';
        html += '</div></div>';

        $("#confirmModal").html(html);
        $("#CASHPOSTMessage").modal('show');
    },

    RefreshDate: function (controlId) {
        $("#" + controlId).val('');
    },


    StartLoading: function (text) {
        var textContent = "";
        if (text != undefined && text != null && text != '') {
            textContent = text;
        }
        else {
            textContent = "Đang tiến hành xử lý dữ liệu...  ";
        }

        $("#tempModal_ms").modal('show');
        $("#tempContainer_ms").isLoading({
            text: textContent,
            position: "inside"
        });
    },

    EndLoading: function (timer) {
        var Time = 100;
        if (timer != null && timer != undefined && timer != '') {
            Time = timer;
        }
        setTimeout(function () {
            $("#tempContainer_ms").isLoading("hide");
            $("#tempModal_ms").modal('hide');
        }, Time);
    },


    HasCharSpecial: function (sInput) {
        var iChars = "~`!#$%^&*+=-[]\\\';,/{}|\":<>?";

        for (var i = 0; i < sInput.length; i++) {
            if (iChars.indexOf(sInput.charAt(i)) != -1) {
                return false;
            }
        }
        return true;
    },

    CheckCharSpecial: function (field) {
        if (common.HasCharSpecial(field.val())) {
            return true;
        }
        else {
            return "* Không được nhập ký tự đặc biệt"
        }
    },

    Message: function (title, text, type) {

        var colorHeader = "#3498DB";
        if (type == "warning") {
            colorHeader = "#f39c12";
        }
        else if (type == "error") {
            colorHeader = "#e74c3c";
        }

        var html = '<div id="CASHPOSTMessage" class="bootbox modal fade in"><div class="modal-dialog modal-sm" role="document"><div class="modal-content">';
        html += '<div class="modal-header" style="padding:3px !important; background-image:none; background-color:' + colorHeader + ' !important;">';
        html += '<button type="button" style="margin-right:5px;" class="close" data-dismiss="modal" aria-hidden="true">×</button>';
        html += '<h4 style="color:#fff; margin-left:10px;">' + title + '</h4>';
        html += '</div><div class="modal-body">' + text + '</div>';
        html += '<div class="modal-footer">';
        //html += '<button type="button" class="btn btn-small btn-primary confirm">Đồng ý</button>';
        html += '<button type="button" class="btn btn-small ';
        if (type == "success") {
            html += 'btn-success';
        }
        else if (type == "error") {
            html += 'btn-danger';
        }
        else if (type == "warning") {
            html += 'btn-warning';
        }

        html += '" data-dismiss="modal">Đóng</button>';
        html += '</div></div>';

        $("#confirmModal").html(html);
        $("#CASHPOSTMessage").modal('show');
    },

    isNumeric: function (n) {
        return !isNaN(parseFloat(n)) && isFinite(n);
    }
}

var ListOperatorCode9y = ["90", "91", "92", "93", "94", "95", "96", "97", "98"];
var ListOperatorCode9yy = ["992", "996", "997", "998", "999"];
var ListOperatorCode1yy = ["120", "121", "122", "123", "124", "125", "126", "127", "128", "129", "162", "163", "164", "165", "166", "167", "168", "169", "186", "188", "199"];
var ListOperatorCodeTinh =
    [
    "76",
    "75",
    "64",
    "281",
    "240",
    "781",
    "241",
    "56",
    "650",
    "651",
    "62",
    "780",
    "710",
    "26",
    "67",
    "511",
    "61",
    "500",
    "501",
    "230",
    "59",
    "219",
    "351",
    "39",
    "711",
    "218",
    "321",
    "8",
    "320",
    "31",
    "58",
    "77",
    "60",
    "231",
    "63",
    "25",
    "20",
    "72",
    "30",
    "68",
    "350",
    "38",
    "210",
    "57",
    "52",
    "510",
    "55",
    "33",
    "53",
    "79",
    "22",
    "66",
    "36",
    "280",
    "54",
    "37",
    "73",
    "74",
    "27",
    "70",
    "211",
    "29",
    "4"
    ];