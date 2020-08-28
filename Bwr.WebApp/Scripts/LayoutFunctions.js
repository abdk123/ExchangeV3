let typeOfPayOuterTransaction = $('#typeOfPayOuterTransaction');
let countryOuterTransaction = $('#countryOuterTransaction');
let companyOuterTransaction = $('#companyOuterTransaction');
let ClientSenderDiv = $('#ClientSender');
let AgentSenderDiv = $('#AgentSender');
let reciverClientSelectOuterTransaction = $('#reciverClientSelectOuterTransaction');
let senderClientSelectOuterTransaction = $('#senderClientSelectOuterTransaction');
let agentSenderOuterTransaction = $('#agentSenderOuterTransaction');
let coinOuterTransaction = $('#coinOuterTransaction');
let fromOuterTransaction = $('#fromOuterTransaction');
let toOuterTransaction = $('#toOuterTransaction');
$('#typeOfPayOuterTransaction').change(function () {
    
    if ($(this).val() == 2) {
        AgentSenderDiv.css('display', 'block');
        ClientSenderDiv.css("display", "none");
    } else {
        AgentSenderDiv.css('display', 'none');
        ClientSenderDiv.css("display", "block");
    }

});
$('#outerTrnasactionFilterButton').click(() => {
    var senderClientId;
    if (typeOfPayOuterTransaction.val() == 2) {
        senderClientId = agentSenderOuterTransaction.val();
    } else {
        senderClientId = senderClientSelectOuterTransaction.val();
    }
    if (senderClientId == "") {
        senderClientId = null;
    }

    window.location.replace("/BwTransactions/OuterIndex?coinId=" + coinOuterTransaction.val() + "&typeOfPay=" + typeOfPayOuterTransaction.val() + "&countryId=" + countryOuterTransaction.val() + "&recviecrClientId=" + reciverClientSelectOuterTransaction.val() + "&senderClientId=" + senderClientId + "&from=" + fromOuterTransaction.val() + "&to="+toOuterTransaction.val());
});
function modelSetting() {
    $('#reciverClientSelectOuterTransaction,#senderClientSelectOuterTransaction').select2({
        ajax: {
            url: '/BwClients/FilterClient',
            dataType: 'json',
            data: function (params) {
                var query = {
                    fullName: params.term,
                }
                return query;
            },
            processResults: function (data) {
                console.log(data);
                return {
                    results: data
                };
            }
        }
    });
}
function FillModalCoinSelect() {
    $.post(
        '/System/_JsonGetCoinInBalance', {
            id: 2
        }, function (data) {
            var mainOption = document.createElement('option');
            mainOption.innerText = "اختر العملة";
            mainOption.value = "";
            var modalCoinSelect = document.getElementById('LayoutmodalCoinSelect');
            modalCoinSelect.appendChild(mainOption);
            for (var i = 0; i < data.length; i++) {
                
                var option = document.createElement('option');
                option.innerText = data[i].CoinName;
                option.value = data[i].CoinId;
                modalCoinSelect.appendChild(option);
            }
        }
    );
}
$(document).ready(function () {
    var divThanHideNavBar = document.getElementById('divThanHideNavBar');
    divThanHideNavBar.click();
    FillModalCoinSelect();
});
$('#LayoutViewBlance').click(function () {
    var coinId = $('#LayoutmodalCoinSelect').val()
    var from = $('#LayoutmodalFromDate').val();
    var to = $('#LayoutmodaltoDate').val();
    if (coinId == "") {
        return;
    }
    window.location.href = "/BwBranchCashFlows/index?branchid="+2+"&coinId=" + coinId + "&from=" + from + "&to=" + to;
});


function numberWithCommas(number, bool) {

    if (number == null || number === "" || number == "لايوجد" || number == "null" || number == undefined) {
        if (bool == true)
            return "لايوجد";
        return "";

    }
    if (number == 0)
        return 0;
    var parts = number.toString().split(".");
    parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    return parts.join(".");
}