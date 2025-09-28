using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ru.core.integrations.customer.core.Model.Configuration
{

    public class ODataConfigResponse
    {
        public string odatacontext { get; set; }
        public ODataConfigItem[] value { get; set; }
    }

    public class ODataConfigItem
    {
        public string odataetag { get; set; }
        public string rub_systemsettingid { get; set; }
        public string rub_value { get; set; }
    }
}
