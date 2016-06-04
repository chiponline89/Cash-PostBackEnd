var Po = {
    LoadTree: function (UnitCode) {
        common.StartLoading();
        //Po.RefreshSearchForm();
        var View = $("#View").val();

        $("#UnitCode").val(UnitCode);
        var PoName = $("#po-" + UnitCode).attr("keyFullName");

        $("#po-Name").html(UnitCode + "-" + PoName);

        $("#createUser").css({ "display": "block" });
        $("#searchForm").css({ "display": "block" });
        $("#NameUnit").css({ "display": "block" });

        var UnitLink = $("#po-" + UnitCode).attr("keyFullCode");

        $.ajax({
            url: "/Metadata/Organization/LoadPosTree",
            dataType: "json",
            type: "Get",
            data: { UnitCode: UnitCode, UnitLink: UnitLink },
            success: function (foo) {
                $("#tree-" + UnitCode).html(foo.Data);
                $('.tree li:has(ul)').addClass('parent_li').find(' > span').attr('title', 'Collapse this branch');
                common.EndLoading(1000);
            }, error: function (xhr, textStatus, errorThrown) {
                if (xhr.status != 200) {
                    common.Message("Lỗi", "Hãy đăng nhập lại, trước khi thao tác tiếp.", "error");
                }
                common.EndLoading(1000);
            }
        });
    },

    ListTree: function (UnitCode, Margin, UnitLink, UnitParent, UnitName, PageUnit)
    {
        var check = $("#value_po_" + UnitCode).val();

        $("#NameUnit").css({ "display": "" });

        $("#CurrentUnitCode").val(UnitCode);
        $("#CurrentUnitLink").val(UnitLink);
        $("#CurrentParentUnitCode").val(UnitParent);
        $("#po-Name").html(UnitName);
        
        common.StartLoading();

        Po.ListUser(UnitCode);

        if (UnitCode.length >= 6)
        {
            $("#tabPo").css({ "display": "none" });
            $("#po").css({ "display": "none" });
            $("#tabPo").removeClass("active");
            $("#po").removeClass("active");

            $("#tabuser").addClass("active");
            $("#user").addClass("active");
            common.EndLoading();
        }
        else
        {
            $("#tabPo").css({ "display": "" });
            $("#po").css({ "display": "" });
            var _pageUnit = 1;

            if (PageUnit != undefined && PageUnit != null && PageUnit != '')
            {
                _pageUnit = PageUnit;
            }

            Po.ListUnit(_pageUnit);

            if (check == null || check == undefined || check == '' || check != 1) {
                $.ajax({
                    url: "/Metadata/Organization/ListTreeByUnit",
                    dataType: "html",
                    type: "Get",
                    data: {
                        UnitCode: UnitCode,
                        Margin: Margin
                    },
                    success: function (foo) {
                        $("#po_" + UnitCode).html(foo);
                        $("#value_po_" + UnitCode).val(1);

                        $("#on_po_" + UnitCode).addClass('hidden');
                        $("#off_po_" + UnitCode).removeClass('hidden');
      
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
            }
            else {
                var nameId = $("#on_po_" + UnitCode).hasClass("hidden");

                if (nameId == true) {
                    $("#on_po_" + UnitCode).removeClass('hidden');
                    $("#off_po_" + UnitCode).addClass('hidden');
                    $("#po_" + UnitCode).hide("slide", { direction: "top" }, 500);
                }
                else {
                    $("#on_po_" + UnitCode).addClass('hidden');
                    $("#off_po_" + UnitCode).removeClass('hidden');
                    $("#po_" + UnitCode).show("slide", { direction: "top" }, 500);
                }

                common.EndLoading();
            }
        }
    },

    ListUser: function(UnitCode)
    {
        $("#divUsers").isLoading({
            text: "Đang tải dữ liệu...",
            //position: "overlay"
            position: "inside"
        });
        $.ajax({
            url: "/Metadata/Organization/ListUsers",
            dataType: "html",
            type: "Get",
            data: { UnitCode: UnitCode },
            success: function(data)
            {
                $("#divUsers").html(data);
                $("#divUsers").isLoading("hide");
            }, error: function (xhr, textStatus, errorThrown) {
                $("#divUsers").isLoading("hide");
                if (xhr.status == 200) {
                    common.Message("Lỗi", "Hãy kiểm tra lại đường truyền trước khi thực hiện thao tác.", "error");
                }
                else {
                    common.Message("Lỗi", "Có lỗi trong quá trình xử lý dữ liệu", "error");
                }
            }
        })
    },

    CreateAccount: function(Id)
    {
        $.ajax({
            url: "/Metadata/Organization/CreateAccount",
            dataType: "html",
            type: "Get",
            data: { _id: Id },
            success: function(data)
            {
                $("#tempContainerSmall").html(data);
                if (Id != undefined && Id != null && Id != '')
                {
                    $("#modaltitle").html("Sửa thông tin " + Id + "(Đơn vị: " + $("#po-Name").html() + ")")
                }
                else
                {
                    $("#modaltitle").html("Thêm mới người dùng (Đơn vị: " + $("#po-Name").html() + ")")
                }
                $("#tempModal_small").modal('show');
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

    SubmitAccount: function () {
        var valid = $('#fCreateAccount').validationEngine('validate');
        if (valid == true) {
            $("#UnitLink").val($("#CurrentUnitLink").val());
            $("#UnitCode").val($("#CurrentUnitCode").val());

            common.StartLoading();
            $("#fCreateAccount").submit();

        }
    },

    AccountOnsuccess: function (data) {
        if(data.Code == "00")
        {
            $("#tempModal_small").modal('hide');
            common.Message("Thông báo", data.Mes, "success");
            var UnitCode = $("#CurrentUnitCode").val();
            common.EndLoading();
            Po.ListUser(UnitCode);
        }
        else {
            common.EndLoading();
            common.Message("Thông báo", data.Mes, "error");
        }
    },

    ChangeStatusAccount: function (_id) {
        $.ajax({
            url: "/Metadata/Organization/ChangeStatusAccount",
            dataType: "json",
            type: "Post",
            data: { _id: _id },
            success: function(data)
            {
                if (data.Code == "00") {
                    common.Message("Thông báo", data.Mes, "success");
                    var UnitCode = $("#CurrentUnitCode").val();
                    Po.ListUser(UnitCode);
                }
                else {
                    common.EndLoading();
                    common.Message("Thông báo", data.Mes, "error");
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
        })
    },

    ListUnit: function(PageIndex)
    {
        $("#divInfoPo").isLoading({
            text: "Đang tải dữ liệu...",
            //position: "overlay"
            position: "inside"
        });
        var UnitCode = $("#CurrentUnitCode").val();
        Po.CurrentPageUnit = PageIndex;

        $.ajax({
            url: "/Metadata/Organization/ListPo",
            dataType: "html",
            type: "Get",
            data: { UnitCode: UnitCode, PageIndex: PageIndex },
            success: function(data)
            {
                $("#divInfoPo").html(data);
                $("#divInfoPo").isLoading("hide");
            }, error: function (xhr, textStatus, errorThrown) {
                $("#divInfoPo").isLoading("hide");
                if (xhr.status == 200) {
                    common.Message("Lỗi", "Hãy kiểm tra lại đường truyền trước khi thực hiện thao tác.", "error");
                }
                else {
                    common.Message("Lỗi", "Có lỗi trong quá trình xử lý dữ liệu", "error");
                }
            }
        });
    },

    EditUnit: function(UnitCode)
    {
        common.StartLoading();
        $.ajax({
            url: "/Metadata/Organization/EditUnit",
            dataType: "html",
            type: "Get",
            data: { UnitCode: UnitCode },
            success: function (data) {
                $("#tempContainerSmall").html(data);
                $("#tempModal_small").modal('show');
                common.EndLoading();

            }, error: function (xhr, textStatus, errorThrown) {
                $("#divInfoPo").isLoading("hide");
                if (xhr.status == 200) {
                    common.Message("Lỗi", "Hãy kiểm tra lại đường truyền trước khi thực hiện thao tác.", "error");
                }
                else {
                    common.Message("Lỗi", "Có lỗi trong quá trình xử lý dữ liệu", "error");
                }
            }
        })
    },

    SubmitUnit: function () {
        var valid = $('#fEditUnit').validationEngine('validate');
        if (valid == true) {
            common.StartLoading();
            $("#fEditUnit").submit();

        }
    },

    UnitOnsuccess: function(data)
    {
        common.EndLoading();
        if(data.Code == "00")
        {
            $("#tempModal_small").modal('hide');
            common.Message("Thông báo", data.Mes, "success");

            var UnitCode = $("#CurrentUnitCode").val();
            $("#value_po_" + UnitCode).val('');
            var Margin = $("#Margin-" + UnitCode).val();
            var UnitLink = $("#hUnitLink-" + UnitCode).val();
            var UnitParent = $("#hUnitParent-" + UnitCode).val();
            var UnitName = $("#hUnitName-" + UnitCode).val();

            Po.ListTree(UnitCode, Margin, UnitLink, UnitParent, UnitName, Po.CurrentPageUnit);
        }
        else
        {
            common.Message("Thông báo", data.Mes, "error");
        }
    },

    EditPost: function(PostCode)
    {
        common.StartLoading();
        var UnitCode = $("#CurrentUnitCode").val();

        var ProvinceCode = $("#hProvinceCode-" + UnitCode).val();

        $.ajax({
            url: "/Metadata/Organization/EditPos",
            dataType: "html",
            type: "Get",
            data: { 
                PosCode: PostCode, 
                ProvinceCode: ProvinceCode
            },
            success: function (data) {
                $("#tempContainerSmall").html(data);
                if (PostCode == undefined || PostCode == null || PostCode == '')
                {
                    $("#modaltitle").html("Thêm mới Đơn vị thuộc đơn vị " + $("#po-Name").html())
                }

                $("#tempModal_small").modal('show');
                common.EndLoading();

            }, error: function (xhr, textStatus, errorThrown) {
                $("#divInfoPo").isLoading("hide");
                if (xhr.status == 200) {
                    common.Message("Lỗi", "Hãy kiểm tra lại đường truyền trước khi thực hiện thao tác.", "error");
                }
                else {
                    common.Message("Lỗi", "Có lỗi trong quá trình xử lý dữ liệu", "error");
                }
            }
        })
    },

    SubmitPost: function()
    {
        var valid = $('#fEditPost').validationEngine('validate');
        if (valid == true) {
            var UnitCode = $("#CurrentUnitCode").val();
            $("#UnitCode").val(UnitCode);
            common.StartLoading();
            $("#fEditPost").submit();
        }
    },

    PostOnsuccess: function(data)
    {
        common.EndLoading();
        if(data.Code == "00")
        {
            $("#tempModal_small").modal('hide');
            common.Message("Thông báo", data.Mes, "success");

            var UnitCode = $("#CurrentUnitCode").val();
            $("#value_po_" + UnitCode).val('');
            var Margin = $("#Margin-" + UnitCode).val();
            var UnitLink = $("#hUnitLink-" + UnitCode).val();
            var UnitParent = $("#hUnitParent-" + UnitCode).val();
            var UnitName = $("#hUnitName-" + UnitCode).val();

            Po.ListTree(UnitCode, Margin, UnitLink, UnitParent, UnitName, Po.CurrentPageUnit);
        }
        else
        {
            common.Message("Thông báo", data.Mes, "error");
        }
    },

    ChangeStatusPos: function(PostCode)
    {
        $.ajax({
            url: "/Metadata/Organization/ChangeStatusPos",
            dataType: "json",
            type: "Post",
            data: { PostCode: PostCode },
            success: function(data)
            {
                if(data.Code == "00")
                {
                    Po.ListUnit(Po.CurrentPageUnit);
                    common.Message("Thông báo", data.Mes, "success");
                }
                else
                {
                    common.Message("Thông báo", data.Mes, "error");
                }
            }, error: function (xhr, textStatus, errorThrown) {
                if (xhr.status == 200) {
                    common.Message("Lỗi", "Hãy kiểm tra lại đường truyền trước khi thực hiện thao tác.", "error");
                }
                else {
                    common.Message("Lỗi", "Có lỗi trong quá trình xử lý dữ liệu", "error");
                }
            }
        })
    },

    CurrentPageUnit: 1
}