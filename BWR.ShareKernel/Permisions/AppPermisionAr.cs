namespace BWR.ShareKernel.Permisions
{
    public class AppPermisionAr
    {
        /*=====Statement=====*/
        public const string Action_Statement_PaymentsAnalysisStatement = "كشف تحليل المدفوعات";
        public const string Action_Statement_OuterTransactions = "كشف الحوالات الصادرة";
        public const string Action_Statement_InnerTransactions = "كشف الحوالات المستلمة";

        /*=====OuterTransaction=====*/
        public const string Action_OuterTransaction_CreateOuterTransaction = "ارسال الحوالات الصادرة";

        /*=====InnerTransaction=====*/
        public const string Action_InnerTransaction_Index = "الحوالات الواردة";

        /*=====Transaction=====*/
        public const string Action_Transaction_TransactionDontDileverd = "كشف الحوالات الغير مسلمة";

        /*=====System=====*/
        public const string Page_System = "كل الاعدادات";

        /*=====Treasury=====*/
        public const string Page_Treasury = "صفحة الصناديق";
        public const string Action_Treasury_Create = "لإضافة صندوق";
        public const string Action_Treasury_Edit = "تعديل صندوق";
        public const string Action_Treasury_Delete = "حذف صندوق";
        public const string Action_Treasury_Viewe = "عرض الصندوق";
       
        /*=====Attachment=====*/
        public const string Page_Attachment = "صفحة المرفقات";
        public const string Action_Attachment_Create = "إضافة مرفق";
        public const string Action_Attachment_Edit = "تعديل مرفق";
        public const string Action_Attachment_Delete = "حذف مرفق";
        public const string Action_Attachment_Viewe = "عرض المرفق";

        /*=====Coin=====*/
        public const string Page_Coin = "صفحة العملات";
        public const string Action_Coin_Create = "إضافة عملة";
        public const string Action_Coin_Edit = "تعديل عملة";
        public const string Action_Coin_Delete = "حذف عملة";
        public const string Action_Coin_Viewe = "عرض العملة";

        /*=====Province=====*/
        public const string Page_Province = "صفحة المحافظة";
        public const string Action_Province_Create = "إضافة محافظة";
        public const string Action_Province_Edit = "تعديل محافظة";
        public const string Action_Province_Delete = "حذف محافظة";
        public const string Action_Province_Viewe = "عرض محافظة";

        /*=====Country=====*/
        public const string Page_Country = "صفحة البلدان";
        public const string Action_Country_Create = "إضافة بلد";
        public const string Action_Country_Edit = "تعديل بلد";
        public const string Action_Country_Delete = "حذف بلد";
        public const string Action_Country_Viewe = "عرض بلد";

        /*=====Role=====*/
        public const string Page_Role = "صفحة الادوار";
        public const string Action_Role_Create = "إضافة دور";
        public const string Action_Role_Edit = "تعديل دور";
        public const string Action_Role_Delete = "حذف دور";
        public const string Action_Role_Viewe = "عرض دور";
        public const string AddPermissions = "إضافة صلاحيات الدور";

        /*=====User=====*/
        public const string Page_User = "صفحة المستخدمين";
        public const string Action_User_Create = "إضافة مستخدم";
        public const string Action_User_Edit = "تعديل مستخدم";
        public const string Action_User_Delete = "حذف مستخدم";
        public const string Action_User_Viewe = "عرض مستخدم";
        public const string Action_User_AssignRolesToUser = "إسناد الادوار للمستخدم";

        /*=====Company=====*/
        public const string Page_Company = "صفحة الشركة";
        public const string Action_Company_Create = "إضافة شركة";
        public const string Action_Company_Edit = "تعديل شركة";
        public const string Action_Company_Delete = "حذف شركة";
        public const string Action_Company_Viewe = "عرض شركة";

        /*=====BoxAction=====*/
        public const string Page_BoxAction = "Pages.BoxAction";
        
        /*=====Exchange=====*/
        public const string Page_Exchange = "اعدادات الصرافة";
        public const string Action_Exchange_ExchangeTemplate = "الصيرفة";
        public const string Action_Exchange_Edit = "حفظ بيانات الصيرفة";
        //public const string Action_Exchange_Delete = "Action.Exchange.Delete";
        //public const string Action_Exchange_Viewe = "Action.Exchange.View";

        /*=====PublicExpense=====*/
        public const string Page_PublicExpense = "صفحة النفقات";
        public const string Action_PublicExpense_Create = "إضافة نفقات";
        public const string Action_PublicExpense_Edit = "تعديل نفقات";
        public const string Action_PublicExpense_Delete = "حذف نفقات";
        public const string Action_PublicExpense_Viewe = "عرض النفقات";
        /*=====PublicIncome=====*/
        public const string Page_PublicIncome = "صفحة الواردات";
        public const string Action_PublicIncome_Create = "إضافة واردات";
        public const string Action_PublicIncome_Edit = "تعديل واردات";
        public const string Action_PublicIncome_Delete = "حذف واردات";
        public const string Action_PublicIncome_Viewe = "عرض الواردات";
       
        /*=====Client=====*/
        public const string Page_Client = "صفحة العملاء";
        public const string Action_Client_Create = "إضافة العملاء";
        public const string Action_Client_Edit = "تعديل العملاء";
        public const string Action_Client_Delete = "حذف العملاء";
        public const string Action_Client_Viewe = "عرض العملاء";

        /*=====BranchCash OR InitialBalance=====*/
        public const string Page_BranchCash = "صفحة الرصيد الاولي";
        //public const string Action_BranchCash_Create = "Action.BranchCash.Create";
        public const string Action_BranchCash_Edit = "تعديل الرصيد الاولي";
        //public const string Action_BranchCash_Delete = "Action.BranchCash.Delete";
        //public const string Action_BranchCash_Viewe = "Action.BranchCash.View";

        /*=====BranchCashFlow=====*/
        public const string Page_BranchCashFlow = "كشف حساب الصندوق";

        /*=====BranchCommission=====*/
        public const string Page_BranchCommission = "العمولة";
    }
}
