using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Piggy2
{
    class ResultLine
    {
        [DisplayName("Fund Name")]
        public string FundName { get; set; }

        public string Performance;

        [DisplayName("3 Month %")]
        public double mon3 {get;set; }

        [DisplayName("6 Month %")]
        public double mon6 { get; set; }

        [DisplayName("12 Month %")]
        public double mon12 { get; set; }

        [DisplayName("18 Month %")]
        public double Month18 { get; set; }

        [DisplayName("5 Year %")]
        public double mon60 { get; set; }

        [DisplayName("pdf download link")]
        public string download_Link { get; set; }
    }
}
