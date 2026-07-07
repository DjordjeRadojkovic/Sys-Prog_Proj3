using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project_3.Messages
{
    public class SearchResault
    {
        public RequestMsg request;
        public List<Article>? articles;

        public SearchResault(RequestMsg req, List<Article> a)
        {
            request = req;
            articles = a;
        }
    }
}
