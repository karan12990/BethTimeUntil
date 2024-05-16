using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root.Models
{
    public class Setting
    {
        public static UserBasicDetail UserBasicDetail { get; set; }
        public const string BaseUrl = "http://pb-lsuat-wbc02.petbarn.com.au:3047/UATMIM/WS/Petbarn%20Pty%20Ltd/Codeunit/PDA";
    }
}
