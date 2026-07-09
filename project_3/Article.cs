using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project_3
{
    public class Article
    {
        public string title;
        public string content;
        public string sentiment = "Neodredjen!";
        public Article()
        {
            
        }
        public Article(string n, string con, string senti)
        {
            title = n;
            content = con;
            sentiment = senti;
        }
    }
}
