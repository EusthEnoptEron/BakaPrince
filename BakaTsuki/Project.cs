using CsQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BakaPrince.BakaTsuki
{
    class Project : BakaPage
    {
        public Project(string title)
            : base(title)
        {
        }

        List<Volume> _volumes = null;
        public Volume[] Volumes
        {
            get
            {
                if (_volumes == null)
                {
                    _volumes = new List<Volume>();
                    CQNode.Find("a:contains(Illustrations)").Each((i,el) => {
                        _volumes.Add(new Volume(new CQ(el).Closest("ul")));
                        }
                    );
                }
                return _volumes.ToArray();
            }
        }


        /// <summary>
        /// 
        /// TODO: Get the real name
        /// </summary>
        public string Title
        {
            get
            {
                return Name;
            }
        }
    }
}
