using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        class Filter
        {
            List<string> FilterWhiteList = new List<string>();
            List<string> FilterBlackList = new List<string>();

            public void SetFilterToAll()
            {
                FilterBlackList.Clear();
                FilterWhiteList.Clear();
                FilterWhiteList.Add("*");
            }

            public void InitFilter(string filterString)
            {
                FilterBlackList.Clear();
                FilterWhiteList.Clear();
                foreach (var s in filterString.Split(','))
                {
                    var filterStringTrimmed = s.Trim();
                    if (filterStringTrimmed.Length > 0 && filterStringTrimmed[0] == '-')
                    {
                        FilterBlackList.Add(filterStringTrimmed.Substring(1));
                    }
                    else
                    {
                        FilterWhiteList.Add(filterStringTrimmed);
                    }
                }
            }

            public bool IfFilter(string testName)
            {
                foreach (string s in FilterBlackList)
                {
                    if (testName.Contains(s))
                    {
                        return false;
                    }
                }

                foreach (string s in FilterWhiteList)
                {
                    if (s == "*" || testName.Contains(s))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
