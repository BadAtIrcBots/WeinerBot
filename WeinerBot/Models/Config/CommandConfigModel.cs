using System.Collections.Generic;

namespace WeinerBot.Models.Config
{
    public class CommandConfigModel : BaseModel
    {
        public char Prefix { get; set; } = '!';
        public List<string> IgnoreList { get; set; }
        public string ResultSuffix { get; set; } = null;
        public bool IgnoreNonVoicedUsers { get; set; } = false;
    }
}
