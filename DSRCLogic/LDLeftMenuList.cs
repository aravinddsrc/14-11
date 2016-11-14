using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.DSRCLogic
{
    public class LDLeftMenuList
    {
        #region Properties

        public string FunctionName { get; set; }

        public string ModuleName { get; set; }

        public string SubModuleName { get; set; }

        public string PageUrl { get; set; }

        public int AccessLevel { get; set; }

        public string ModuleIcon { get; set; }

        public string SubModuleIcon { get; set; }

        public string SubMenuIcon { get; set; }

        public Int64 PreceedanceOrder { get; set; }

        public Int32 ModulePreceedanceOrder { get; set; }

        #endregion
    }
}