

var Autorize;
$(document).ready(function () {
    Autorize = GetUser();
});
function GetUser() {
    return JSON.parse(localStorage.getItem('user'));
}

function HavePrivlege(privelegeName) {
    
    if (Autorize === undefined) {
        return false;       
    }
    return Autorize.Privileges.filter(c => c.Name === privelegeName).length > 0;
}
class priveleges {

    static showGroup = "ShowGroup";
    static addGroup = "AddGroup";
    static updateGroup = "UpdateGroup";
    static deleteGroup = "DeleteGroup";
    static reciveTransaction = "ReciveTransaction";
    static breakTheWall = "BreakTheWall";
    static showCountry = "ShowCountry";
    static updateCountry = "UpdateCountry";
    static deleteCountry = "DeleteCountry";
    static addCountry = "AddCountry";
}