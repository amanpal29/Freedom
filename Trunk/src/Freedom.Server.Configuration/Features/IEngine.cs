using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Freedom.Server.Tools.Features
{
    public interface IEngine
    {

        bool? Succeeded { get; }
        bool HasFinished { get; }
        void Start();
    }
}
