using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetThins.Infrastructure
{
    public class BaseInfo
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string PathDirectory { get; set; }

        public string PathTempDirectory { get; set; }

        public string DirFileInput { get; set; }

        public string Resource { get; set; }
    }
}
