using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace project_3.Messages
{
    public class RequestMsg
    {
        public HttpListenerContext _context;
        string _keyWord = string.Empty;

        public RequestMsg(HttpListenerContext c, string kw)
        {
            _context = c;
            _keyWord = HttpHelper.GetKeyWord(kw);
        }
        public string GetKeyword () { return _keyWord; }
    }
}
