using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace TrumpBot.Models
{
    public class CommandConfigModel : BaseModel
    {
        public char Prefix { get; set; } = '!';
        public List<string> IgnoreList { get; set; }
        public string ResultSuffix { get; set; } = null;
        public bool IgnoreNonVoicedUsers { get; set; } = false;
    }
}
