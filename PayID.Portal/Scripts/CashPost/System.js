var System = {
    ServiceChange: function(ServiceCode)
    {
        $("#cbTypes option").hide();
        $("#cbTypes option").each(function (index) {
            var service = $(this).data("service");
            if (service == ServiceCode || service == "NONE") {
                $(this).show();
            }
        });
    },

    ListWorkFlow: function () {
        common.StartLoading();
        var UnitCode = $("#UnitProcess").val();

        $.ajax({
            url: "/Systems/Setting/ListWorkflow",
            dataType: "html",
            type: "Get",
            data: { unitCode: UnitCode },
            success: function (data) {
                common.EndLoading();
                $("#listWorkflow").html(data);
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

    SaveWork: function()
    {

        if (System.Value.length < 2)
        {
            $('.form-group-multiple-selects .input-group-multiple-select select').addClass("validate[required]");
        }
        else{
            $('.form-group-multiple-selects .input-group-multiple-select select').removeClass("validate[required]");
        }

        var valid = $('#frmWorkFlow').validationEngine('validate');

        if (valid == true) {
            common.StartLoading();
            var Service = "";
            $("#cbTypes option").each(function (index) {
                if ($(this).is(":selected")) {
                    Service = $(this).data("service");
                }
            });

            var Type = $('#cbTypes').val();
            var Token = $('input[name="__RequestVerificationToken"]').val();
            var headers = {};
            headers['__RequestVerificationToken'] = Token;
            $.ajax({
                url: "/Systems/Setting/UpdateWorkFlowService",
                dataType: "json",
                type: "Post",
                data: {
                    UnitCode: $("#UnitProcess").val(),
                    RequestService: Service,
                    RequestType: Type,
                    Steps: System.Value.toString(),
                    StepsName: System.Texts.slice(0, System.Value.length).toString(),
                },
                headers: headers,
                success: function (data) {
                    common.EndLoading();
                    if (data.Code == "00") {
                        common.Message("Thông báo", data.Mes, "success");
                        System.ListWorkFlow();
                    }
                    else {
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
            });
        }
    },

    RefreshWorkflowDiagram: function(){
        $('.stepwizard-row').html('');
        System.Texts = new Array();
        for (var i = 0, length = System.Value.length; i < length; i++) {
            textUnit = "";
            switch (System.Value[i]) {
                case "BDT":
                    textUnit = "Bưu điện Tỉnh";
                    break;
                case "BDTT":
                    textUnit = "BĐ Huyện/Trung Tâm";
                    break;
                case "BCU":
                    textUnit = "Bưu cục";
                    break;
                case "TCT":
                    textUnit = "Tổng công ty";
                    break;
                case "EMP":
                    textUnit = "Bưu tá";
                    break;
                case "DTH":
                    textUnit = "Đường thư";
                    break;
                default:
                    textUnit = "Đường thư";
                    break;
            }
            System.Texts.push(textUnit);
            $('.stepwizard-row').html($('.stepwizard-row').html() + "<div class='stepwizard-step'><button type='button' class='btn btn-primary btn-circle'>" + System.Value[i] + "</button><p>" + (i + 1) + "." + textUnit + "</p></div>");
        }
    },

    Value:[],
    Texts:[]
}