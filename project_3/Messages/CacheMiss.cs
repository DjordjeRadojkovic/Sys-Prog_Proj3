using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project_3.Messages
{
    public class CacheMiss
    {
        public RequestMsg request;
        public string keyword;

        public CacheMiss(string kw, RequestMsg request)
        {
            keyword = kw;
            this.request = request;
        }
    }
}
