using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project_3.Messages
{
    public class SaveCache
    {
        public string KeyWord { get; }
        public List<Article> articles;

        public SaveCache(string kw, List<Article> a)
        {
            KeyWord = kw;
            articles = a;
        }
    }
}
