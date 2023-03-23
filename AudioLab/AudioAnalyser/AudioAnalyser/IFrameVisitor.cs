using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioAnalyser
{
    internal interface IFrameVisitor
    {
        double Volume();
        double STE();
    }

}
