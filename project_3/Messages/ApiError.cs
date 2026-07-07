using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project_3.Messages
{
    public class ApiError
    {
        public string keyword { get; set; }

        public ApiError(string kw)
        {
            keyword = kw;
        }
    }
}
